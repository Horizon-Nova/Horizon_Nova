// RegisterScreen 專用 JavaScript
// 日期時間選擇器變數（全局）
let selectedDate = null; // yyyy-mm-dd
let selectedTime = '00:00'; // HH:mm

$(function () {
	// 初始化建立時間
	const now = new Date();
	const pad = (n) => (n < 10 ? '0' + n : n);
	const fmt = (d) => `${d.getFullYear()}-${pad(d.getMonth()+1)}-${pad(d.getDate())} ${pad(d.getHours())}:${pad(d.getMinutes())}`;
	$('#createdAt').val(fmt(now));

	// Kendo UI 初始化（時間/日期）
	if ($.fn.kendoTimePicker) {
		try {
			$('#timeInput').kendoTimePicker({
				format: 'HH:mm',
				dateInput: true,
				change: function () { updateNextCheckFromRule(); }
			});
			$('#onceDateInput').kendoDatePicker({
				format: 'yyyy-MM-dd',
				dateInput: true,
				change: function () { updateNextCheckFromRule(); }
			});
			if (!$('#timeInput').val()) {
				$('#timeInput').data('kendoTimePicker')?.value('09:00');
			}
		} catch (e) {}
	}

	function getTodayAtHM(hhStr, mmStr) {
		if (!hhStr || !mmStr) return null;
		const hh = parseInt(hhStr, 10), mm = parseInt(mmStr, 10);
		const d = new Date();
		d.setSeconds(0, 0);
		d.setHours(isNaN(hh) ? 0 : hh, isNaN(mm) ? 0 : mm, 0, 0);
		return d;
	}

	function nextDaily(hhStr, mmStr) {
		const target = getTodayAtHM(hhStr, mmStr);
		if (!target) return null;
		const nowLocal = new Date();
		if (target <= nowLocal) {
			target.setDate(target.getDate() + 1);
		}
		return target;
	}

	function nextWeekly(weekdays, hhStr, mmStr) {
		if (!weekdays || weekdays.length === 0) return null;
		const nowLocal = new Date();
		const targetTime = getTodayAtHM(hhStr, mmStr) || nowLocal;
		let best = null;
		for (let i = 0; i < 7; i++) {
			const d = new Date(targetTime.getTime());
			d.setDate(nowLocal.getDate() + i);
			const wd = d.getDay(); // 0=日
			if (weekdays.includes(wd)) {
				if (i === 0 && d <= nowLocal) {
					continue;
				}
				if (!best || d < best) best = d;
			}
		}
		// 若本週都過了，取下週最早
		if (!best) {
			const nextWeek = new Date(targetTime.getTime());
			for (let i = 1; i <= 7; i++) {
				const d = new Date(nextWeek.getTime());
				d.setDate(nowLocal.getDate() + i);
				const wd = d.getDay();
				if (weekdays.includes(wd)) return d;
			}
		}
		return best;
	}

	function nextMonthly(day, hhStr, mmStr) {
		const d = getTodayAtHM(hhStr, mmStr) || new Date();
		const nowLocal = new Date();
		let targetDay = Math.min(Math.max(parseInt(day || '1', 10), 1), 31);
		d.setDate(targetDay);
		if (d <= nowLocal) {
			d.setMonth(d.getMonth() + 1);
			d.setDate(targetDay);
		}
		return d;
	}

	function lastDayOfMonth(date) {
		const d = new Date(date.getFullYear(), date.getMonth() + 1, 0);
		return d.getDate();
	}

	// === 單一欄位解析 ===
	const mapHanWeek = { '日':0,'天':0,'一':1,'二':2,'三':3,'四':4,'五':5,'六':6 };
	const timeRegex = /(\b[01]?\d|2[0-3]):([0-5]\d)\b/g;
	const dateYmdRegex = /\b(\d{4})-(\d{2})-(\d{2})\b/;
	const monthDayRegex = /\b(\d{2})-(\d{2})\b/; // MM-DD
	function getDefaultHM(){ return {hh:'09', mm:'00'}; }
	function parseSchedule(inputRaw) {
		const input = (inputRaw || '').replace(/\s+/g,' ').trim();
		if (!input) return [];
		const parts = input.split(/[;；]/).map(s=>s.trim()).filter(Boolean);
		const items = [];
		for (const part of parts) {
			const times = [];
			let m;
			while ((m = timeRegex.exec(part)) !== null) {
				times.push(`${String(m[1]).padStart(2,'0')}:${m[2]}`);
			}
			if (times.length === 0) {
				const {hh,mm} = getDefaultHM();
				times.push(`${hh}:${mm}`);
			}
			const dOnce = part.match(dateYmdRegex);
			if (dOnce) { items.push({kind:'once', y:dOnce[1], m:dOnce[2], d:dOnce[3], times}); continue; }
			const dYearly = part.match(monthDayRegex);
			if (dYearly && /每年/.test(part)) { items.push({kind:'yearly', m:dYearly[1], d:dYearly[2], times}); continue; }
			if (/每月/.test(part)) {
				if (/最後一天|末日/.test(part)) items.push({kind:'monthly_last', times});
				else {
					const dm = part.match(/每月\s*(\d{1,2})/);
					if (dm) items.push({kind:'monthly_day', day:parseInt(dm[1],10), times});
				}
				continue;
			}
			if (/每週/.test(part)) {
				const wdHan = part.replace('每週','').replace(/[^\u4e00-\u9fa5]/g,'');
				const wds = [];
				for (const ch of wdHan) if (mapHanWeek[ch]!=null) wds.push(mapHanWeek[ch]);
				if (wds.length) { items.push({kind:'weekly', weekdays:Array.from(new Set(wds)), times}); continue; }
			}
			if (/工作日/.test(part)) { items.push({kind:'workdays', times}); continue; }
			if (/週末|假日/.test(part)) { items.push({kind:'holidays', times}); continue; }
			// 預設每天
			items.push({kind:'daily', times});
		}
		return items;
	}

	function nextWorkdays(hh, mm) { return nextWeekly([1,2,3,4,5], hh, mm); }
	function nextHolidays(hh, mm) { return nextWeekly([0,6], hh, mm); }

	function nextYearly(monthStr, dayStr) {
		const { hh, mm } = getHM();
		const month = parseInt(monthStr || '1', 10) - 1;
		const day = parseInt(dayStr || '1', 10);
		const nowLocal = new Date();
		let d = new Date(nowLocal.getFullYear(), month, day, parseInt(hh,10), parseInt(mm,10), 0, 0);
		if (d <= nowLocal) {
			d = new Date(nowLocal.getFullYear()+1, month, day, parseInt(hh,10), parseInt(mm,10), 0, 0);
		}
		return d;
	}

	function updatePreviewFromSchedule() {
		const list = parseSchedule($('#scheduleInput').val());
		const nowLocal = new Date();
		let candidates = [];
		for (const item of list) {
			for (const t of item.times) {
				const [hh, mm] = t.split(':');
				if (item.kind === 'once') {
					const d = new Date(`${item.y}-${item.m}-${item.d}T${hh}:${mm}:00`);
					if (d > nowLocal) candidates.push(d);
				} else if (item.kind === 'daily') {
					candidates.push(nextDaily(hh, mm));
				} else if (item.kind === 'workdays') {
					candidates.push(nextWorkdays(hh, mm));
				} else if (item.kind === 'holidays') {
					candidates.push(nextHolidays(hh, mm));
				} else if (item.kind === 'weekly') {
					candidates.push(nextWeekly(item.weekdays, hh, mm));
				} else if (item.kind === 'monthly_last') {
					const d = lastDayOfMonth(nowLocal);
					candidates.push(nextMonthly(d, hh, mm));
				} else if (item.kind === 'monthly_day') {
					candidates.push(nextMonthly(item.day, hh, mm));
				} else if (item.kind === 'yearly') {
					candidates.push(nextYearly(item.m, item.d));
				}
			}
		}
		candidates = candidates.filter(Boolean).sort((a,b)=>a-b);
		const next = candidates[0] || null;
		$('#nextCheck').val(next ? fmt(next) + `（共 ${candidates.length} 組）` : '');
	}

});

// 日期格式化函數
function ymd(d) {
	return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`;
}

// 載入日曆月份（通過 AJAX 從 Controller 獲取 HTML）
function loadCalendarMonth(year, month) {
	$.ajax({
		url: '/NovaAPP/DialVision/LoadCalendar',
		method: 'GET',
		data: { year: year, month: month, selectedDate: selectedDate },
		success: function(html) {
			$('#schedCalendar').html(html);
		},
		error: function() {
			alert('載入日曆失敗');
		}
	});
}

// 選擇日期（只更新 class，不組裝 HTML）
function selectDate(dateStr) {
	selectedDate = dateStr;
	$('.cal-cell[data-date]').removeClass('selected');
	$(`.cal-cell[data-date="${dateStr}"]`).addClass('selected');
}

// 選擇時間（只更新 class，不組裝 HTML）
function selectTime(timeStr) {
	selectedTime = timeStr;
	$('#wheelTimes .wheel-item').removeClass('selected');
	$(`#wheelTimes .wheel-item[data-time="${timeStr}"]`).addClass('selected');
}

// 確認選擇
function confirmPicker() {
	if (selectedDate && selectedTime) {
		$('#nextCheck').val(`${selectedDate} ${selectedTime}`);
		$('#schedulerPicker').addClass('d-none');
	}
}

// 取消選擇
function cancelPicker() {
	$('#schedulerPicker').addClass('d-none');
}

$(function () {
	// 點擊 nextCheck 顯示選擇器
	$(document).on('click', '#nextCheck', function() {
		$('#schedulerPicker').removeClass('d-none');
		// 如果有已選值，解析並設定
		const currentVal = $(this).val();
		if (currentVal && /^\d{4}-\d{2}-\d{2} \d{2}:\d{2}$/.test(currentVal)) {
			const [datePart, timePart] = currentVal.split(' ');
			selectedDate = datePart;
			selectedTime = timePart;
			// 重新載入日曆和時間轉盤
			const date = new Date(datePart);
			loadCalendarMonth(date.getFullYear(), date.getMonth() + 1);
			$.ajax({
				url: '/NovaAPP/DialVision/LoadTimeWheel',
				method: 'GET',
				data: { selectedTime: timePart },
				success: function(html) {
					$('#wheelTimes').html(html);
				}
			});
		} else {
			// 預設為今天
			const now = new Date();
			selectedDate = ymd(now);
			selectedTime = '00:00';
		}
	});

	// 載入編輯數據（如果有的話）
	function loadEditData() {
		if (window.currentEditMeter) {
			const data = window.currentEditMeter;
			
			// 填充表單欄位
			$('#meterLocation').val(data.location || '');
			$('#meterArea').val(data.area || '');
			
			// 映射錶盤類型
			const typeMap = {
				'牆壁電表': 'wall',
				'智慧電表': 'smart',
				'水表': 'water',
				'氣表': 'gas'
			};
			const typeValue = typeMap[data.type] || '';
			if (typeValue) {
				$('#meterType').val(typeValue);
			}
			
			// 映射檢測週期
			const cycleMap = {
				'每周': 'weekly',
				'每月': 'monthly',
				'每年': 'yearly',
				'特定日期': 'specific'
			};
			const cycleValue = cycleMap[data.cycle] || '';
			if (cycleValue) {
				$('#checkCycle').val(cycleValue);
			}
			
			// 填充下次檢測時間
			if (data.nextCheck) {
				$('#nextCheck').val(data.nextCheck);
			}
			
			// 修改標題為「修改錶盤」
			$('.register-heading').text('修改錶盤');
			$('.btn-register span').text('更新錶盤');
			
			// 清除編輯標記（避免重複載入）
			window.currentEditMeter = null;
		} else {
			// 新增模式：重置表單
			$('.register-heading').text('註冊新的錶盤');
			$('.btn-register span').text('建立錶盤');
		}
	}

	// 初始化
	buildTimes();
	buildCalendar(new Date());
	
	// 延遲載入編輯數據，確保 DOM 已完全載入
	setTimeout(function() {
		loadEditData();
	}, 100);

	// 調整時間：以當前選項為單位覆寫時間
	$(document).on('click', '#btnTimeEdit', function() {
		const key = $('#smartPreset').val();
		window.__presetTimes = window.__presetTimes || {};
		const current = (window.__presetTimes[key] || []).join(',') || '';
		const text = prompt('輸入時間（24小時，逗號分隔）\n例：09:00,18:30', current);
		if (text == null) return;
		const parts = text.split(/[,，]/).map(s => s.trim()).filter(Boolean);
		const ok = parts.every(t => /^([01]?\d|2[0-3]):[0-5]\d$/.test(t));
		if (!ok) {
			alert('時間格式錯誤，請用 HH:mm，並用逗號分隔多筆');
			return;
		}
		window.__presetTimes[key] = parts;
		previewFromPreset();
	});

	// 建立錶盤（示意：驗證必填後切回 dashboard）
	$(document).on('click', '#btnCreateMeter', function (e) {
		e.preventDefault();
		const location = $('#meterLocation').val()?.trim();
		const area = $('#meterArea').val()?.trim();
		const type = $('#meterType').val();
		const cycle = $('#checkCycle').val();
		const hasNext = $('#nextCheck').val()?.trim();
		if (!location || !area || !type || !cycle || !hasNext) {
			alert('請完整填寫 地點、場域、錶盤類型、檢測週期 與 距離下次檢測時間');
			return;
		}
		if (typeof switchScreen === 'function') {
			switchScreen('dashboard');
		}
	});

	// 預設時間
	if (!$('#timeInput').val()) {
		$('#timeInput').val('09:00');
	}
});


