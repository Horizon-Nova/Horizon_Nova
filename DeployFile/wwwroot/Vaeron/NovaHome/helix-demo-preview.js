// From helix-demo-interactive_1.html
window.lucide = window.lucide || { createIcons: () => {} };
// ── 資料 ──────────────────────────────────────
const WORKSPACES = [
  { name:'HN-Production',  desc:'主要生產工作區', path:'D:\\Helix\\Workspaces\\HN-Production\\Modules',  modules:['production-monitor','deploy-manager'] },
  { name:'HN-Testing',     desc:'測試與驗證環境', path:'D:\\Helix\\Workspaces\\HN-Testing\\Modules',     modules:['testrunner-core','mockserver-lite'] },
  { name:'HN-Maintenance', desc:'維運與排查環境', path:'D:\\Helix\\Workspaces\\HN-Maintenance\\Modules', modules:['error-tracker'] },
];

const MODS = {
  'production-monitor': {
    displayName:'ProductionMonitor', version:'1.3.0', status:'啟用', icon:'activity',
    author:'Vaeron Studio', desc:'產線機台即時監控模組，顯示各機台運行狀態、今日產量與即時異常告警。',
    minHost:'2.1.0', bindingId:'com.vaeron.production.monitor',
  },
  'deploy-manager': {
    displayName:'DeployManager', version:'2.1.4', status:'啟用', icon:'send',
    author:'Vaeron Studio', desc:'多環境部署管理模組，追蹤各環境版本狀態、部署紀錄與操作者資訊。',
    minHost:'2.0.0', bindingId:'com.vaeron.deploy.manager',
  },
  'testrunner-core': {
    displayName:'TestRunner.Core', version:'1.2.0', status:'啟用', icon:'check-circle',
    author:'Horizon Nova Team', desc:'自動化測試執行引擎，支援批次測試與結果彙整。',
    minHost:'2.0.0', bindingId:'com.hn.testrunner.core',
  },
  'mockserver-lite': {
    displayName:'MockServer.Lite', version:'0.9.4', status:'啟用', icon:'server',
    author:'Horizon Nova Labs', desc:'輕量級 Mock Server，用於隔離外部依賴進行整合測試。',
    minHost:'2.0.0', bindingId:'com.hn.mockserver.lite',
  },
  'error-tracker': {
    displayName:'ErrorTracker', version:'1.0.1', status:'啟用', icon:'bug',
    author:'Vaeron Studio', desc:'錯誤追蹤與告警模組，整合事件日誌提供根因分析。',
    minHost:'2.0.0', bindingId:'com.vaeron.error.tracker',
  },
};

const LOGS = [
  {ts:'2026-05-10 14:00:02', lv:'info',    src:'Shell',           msg:'Workspace loaded: HN-Production'},
  {ts:'2026-05-10 14:00:03', lv:'info',    src:'ModuleMgr',       msg:'Module initialized: ProductionMonitor v1.3.0'},
  {ts:'2026-05-10 14:00:04', lv:'info',    src:'ModuleMgr',       msg:'Module initialized: DeployManager v2.1.4'},
  {ts:'2026-05-10 14:00:09', lv:'warning', src:'Router',          msg:'偵測到重複封包，已略過'},
  {ts:'2026-05-10 14:00:21', lv:'error',   src:'Storage',         msg:'連線逾時，啟動重試  ·  COM3'},
  {ts:'2026-05-10 14:00:36', lv:'info',    src:'Storage',         msg:'重試成功，已恢復同步'},
  {ts:'2026-05-10 14:01:02', lv:'debug',   src:'Telemetry',       msg:'已送出工作區心跳'},
  {ts:'2026-05-10 14:01:15', lv:'info',    src:'ProductionMonitor',msg:'Machine M02 status changed: running → error'},
  {ts:'2026-05-10 14:01:16', lv:'warning', src:'ProductionMonitor',msg:'Alert triggered: M02 sensor signal lost'},
  {ts:'2026-05-10 14:02:00', lv:'info',    src:'DeployManager',   msg:'Deployment v2.4.1 to Production: started'},
  {ts:'2026-05-10 14:02:08', lv:'info',    src:'DeployManager',   msg:'Deployment v2.4.1 to Production: success (8s)'},
];

// 產線機台資料
const MACHINES = [
  {id:'M01', name:'組裝線 A', status:'running', out:842,  tgt:900,  last:'14:04:58 正常'},
  {id:'M02', name:'組裝線 B', status:'error',   out:310,  tgt:900,  last:'14:03:41 感測器異常'},
  {id:'M03', name:'品檢站 1', status:'running', out:1204, tgt:1200, last:'14:05:02 正常'},
  {id:'M04', name:'包裝線 C', status:'idle',    out:0,    tgt:600,  last:'13:58:10 排班待機'},
  {id:'M05', name:'組裝線 C', status:'running', out:778,  tgt:900,  last:'14:04:58 正常'},
];

const DEPLOY_RECORDS = [
  {ver:'v2.4.1', env:'Production', st:'success', op:'chen.wei', ts:'14:00:12', dur:'2m 14s'},
  {ver:'v2.4.1', env:'Staging',    st:'success', op:'chen.wei', ts:'13:52:04', dur:'1m 58s'},
  {ver:'v2.4.0', env:'Production', st:'failed',  op:'lin.jia',  ts:'11:30:44', dur:'0m 42s'},
  {ver:'v2.4.0', env:'Staging',    st:'success', op:'lin.jia',  ts:'11:21:09', dur:'2m 01s'},
  {ver:'v2.3.9', env:'Production', st:'success', op:'chen.wei', ts:'09:15:33', dur:'2m 08s'},
];

// ── 狀態 ──────────────────────────────────────
let currentWs = 0;
let currentPage = 'home';
let currentModule = null;
let selectedModId = null;
let selectedDlgFolder = null;
let wsPopupOpen = false;
let logFilter = 'all';
let selectedMachine = 'M01';

// ── 初始化 ────────────────────────────────────
function init() {
  renderModuleNav();
  renderModList();
  renderHomeModules();
  renderLogs();
  updateStatusBar();
  lucide.createIcons();
}

// ── 導覽 ──────────────────────────────────────
function navigate(page) {
  closeWsPopup();
  if (page.startsWith('mod:')) {
    const modId = page.slice(4);
    currentPage = 'module-ui';
    currentModule = modId;
    updateNavActive(null, modId);
    const m = MODS[modId];
    setTopbar(m.displayName, m.displayName);
    showPage('module-ui');
    renderModuleUI(modId);
    return;
  }
  currentPage = page;
  currentModule = null;
  updateNavActive(page, null);
  const titles = {home:'Home',modules:'Modules',eventlog:'Event Log',settings:'Settings'};
  setTopbar(titles[page]||page, titles[page]||page);
  showPage(page);
}

function showPage(page) {
  document.querySelectorAll('.page').forEach(p=>p.classList.remove('active'));
  const el = document.getElementById('page-'+page);
  if (el) el.classList.add('active');
}

function updateNavActive(page, modId) {
  document.querySelectorAll('.nav-item').forEach(el=>el.classList.remove('active'));
  if (page) {
    const el = document.querySelector(`.nav-item[data-page="${page}"]`);
    if (el) el.classList.add('active');
  }
  if (modId) {
    const el = document.querySelector(`.nav-item[data-modid="${modId}"]`);
    if (el) el.classList.add('active');
  }
}

function setTopbar(bc, title) {
  const ws = WORKSPACES[currentWs].name;
  document.getElementById('breadcrumb').innerHTML = `${ws} &nbsp;/&nbsp; <span>${bc}</span>`;
  document.getElementById('pageTitle').textContent = title;
}

// ── Workspace ─────────────────────────────────
function toggleWsPopup() {
  wsPopupOpen = !wsPopupOpen;
  document.getElementById('wsPopup').classList.toggle('open', wsPopupOpen);
  document.getElementById('wsBtnEl').classList.toggle('open', wsPopupOpen);
  if (wsPopupOpen) {
    const btn = document.getElementById('wsBtnEl');
    document.getElementById('wsPopup').style.top = (btn.offsetTop + btn.offsetHeight + 4) + 'px';
  }
}
function closeWsPopup() {
  wsPopupOpen = false;
  document.getElementById('wsPopup').classList.remove('open');
  document.getElementById('wsBtnEl').classList.remove('open');
}
function switchWorkspace(idx) {
  if (idx === currentWs) { closeWsPopup(); return; }
  closeWsPopup();
  const sk = document.getElementById('sbSkeleton');
  sk.style.display = 'flex';
  for (let i=0;i<3;i++) {
    document.getElementById('wsRow'+i).classList.toggle('active', i===idx);
    document.getElementById('wsDot'+i).style.display = i===idx ? '' : 'none';
  }
  setTimeout(() => {
    currentWs = idx;
    selectedModId = null;
    const ws = WORKSPACES[idx];
    document.getElementById('wsNameEl').textContent = ws.name;
    document.getElementById('hWsMachine').textContent = 'HELIX-WS-01';
    document.getElementById('hCardWs').textContent = ws.name;
    document.getElementById('modPath').textContent = ws.path;
    sk.style.display = 'none';
    renderModuleNav();
    renderModList();
    renderHomeModules();
    updateStatusBar();
    navigate('home');
    showToast(`已切換至 ${ws.name}`);
    lucide.createIcons();
  }, 600);
}

// ── 模組導覽 ──────────────────────────────────
function renderModuleNav() {
  const ws = WORKSPACES[currentWs];
  const el = document.getElementById('moduleNavItems');
  el.innerHTML = '';
  ws.modules.forEach(id => {
    const m = MODS[id]; if (!m) return;
    const div = document.createElement('div');
    div.className = 'nav-item';
    div.dataset.modid = id;
    div.innerHTML = `<div class="nav-icon"><i data-lucide="${m.icon}" style="width:14px;height:14px"></i></div>${m.displayName}`;
    div.onclick = () => navigate('mod:'+id);
    el.appendChild(div);
  });
}

// ── Home 模組列表 ─────────────────────────────
function renderHomeModules() {
  const ws = WORKSPACES[currentWs];
  const el = document.getElementById('homeModuleList');
  const cnt = ws.modules.length;
  document.getElementById('hCardModCount').textContent = cnt;
  document.getElementById('hCardModStatus').textContent = cnt > 0 ? 'All modules ready' : 'No modules loaded';
  if (cnt === 0) {
    el.innerHTML = `<div style="text-align:center;padding:32px;color:var(--TextTertiary)">
      <div style="font-size:13px;font-weight:600;color:var(--TextSecondary);margin-bottom:4px">No modules loaded</div>
      <div style="font-size:11px">Modules will appear here once loaded by the shell.</div>
    </div>`; return;
  }
  el.innerHTML = ws.modules.map(id => {
    const m = MODS[id]; if (!m) return '';
    return `<div class="loaded-module-row">
      <div class="lm-icon"><i data-lucide="${m.icon}" style="width:18px;height:18px"></i></div>
      <div class="lm-info">
        <div class="lm-name-row">
          <div class="lm-name">${m.displayName}</div>
          <div class="lm-ver">${m.version}</div>
        </div>
        <div class="lm-desc">${m.desc}</div>
      </div>
      <div class="lm-open" onclick="navigate('mod:${id}')">
        <i data-lucide="external-link" style="width:12px;height:12px;color:var(--AccentPrimary)"></i>開啟
      </div>
    </div>`;
  }).join('');
}

// ── Modules 頁列表 ────────────────────────────
function renderModList() {
  const ws = WORKSPACES[currentWs];
  const el = document.getElementById('modList');
  const cnt = document.getElementById('modCount');
  cnt.textContent = ws.modules.length;
  if (ws.modules.length === 0) {
    el.innerHTML = `<div class="mod-empty">
      <div class="mod-empty-icon"><i data-lucide="package" style="width:32px;height:32px"></i></div>
      <div class="mod-empty-title">尚未安裝模組</div>
      <div class="mod-empty-sub">點選「導入模組」以從本機資料夾安裝</div>
    </div>`;
    document.getElementById('modDetailArea').innerHTML = renderNoSel();
    return;
  }
  el.innerHTML = ws.modules.map(id => {
    const m = MODS[id]; if (!m) return '';
    const sel = selectedModId === id;
    return `<div class="mod-row${sel?' selected':''}" data-modid="${id}" onclick="selectMod('${id}')">
      <div class="mod-icon"><i data-lucide="${m.icon}" style="width:16px;height:16px"></i></div>
      <div class="mod-info">
        <div class="mod-name">${m.displayName}</div>
        <div class="mod-meta">
          <span class="mod-ver">${m.version}</span>
          <span class="sbadge s-ok">${m.status}</span>
        </div>
      </div>
    </div>`;
  }).join('');
  if (selectedModId && ws.modules.includes(selectedModId)) renderDetail(selectedModId);
  else { selectedModId = null; document.getElementById('modDetailArea').innerHTML = renderNoSel(); }
}

function selectMod(id) {
  selectedModId = id;
  document.querySelectorAll('.mod-row').forEach(r => r.classList.toggle('selected', r.dataset.modid===id));
  renderDetail(id);
}

function renderDetail(id) {
  const m = MODS[id];
  document.getElementById('modDetailArea').innerHTML = `
    <div class="mod-detail">
      <div class="detail-hdr">
        <div class="detail-icon"><i data-lucide="${m.icon}" style="width:24px;height:24px"></i></div>
        <div>
          <div class="detail-name">${m.displayName}</div>
          <div class="detail-badges">
            <span class="vbadge">${m.version}</span>
            <span class="sbadge s-ok">${m.status}</span>
          </div>
        </div>
      </div>
      <p class="detail-desc">${m.desc}</p>
      <div class="meta-card">
        <div class="meta-row"><div class="meta-lbl">開發者</div><div class="meta-val">${m.author}</div></div>
        <div class="meta-row"><div class="meta-lbl">模組識別碼</div><div class="meta-val mono">${m.bindingId}</div></div>
        <div class="meta-row"><div class="meta-lbl">最低殼層版本</div><div class="meta-val">${m.minHost}</div></div>
      </div>
      <div class="detail-actions">
        <div class="primary-btn" onclick="navigate('mod:${id}')">
          <i data-lucide="external-link" style="width:13px;height:13px"></i>開啟模組
        </div>
        <div class="ghost-btn">停用模組</div>
        <div class="danger-btn" onclick="removeMod('${id}')">
          <i data-lucide="trash-2" style="width:13px;height:13px"></i>移除模組
        </div>
      </div>
    </div>`;
  lucide.createIcons();
}

function renderNoSel() {
  return `<div class="no-sel">
    <i data-lucide="package" style="width:40px;height:40px;opacity:.25"></i>
    <div style="font-size:13px;color:var(--TextSecondary)">選取左側模組以查看詳情</div>
  </div>`;
}

function removeMod(id) {
  const ws = WORKSPACES[currentWs];
  const i = ws.modules.indexOf(id);
  if (i !== -1) {
    ws.modules.splice(i, 1);
    if (selectedModId === id) selectedModId = null;
    renderModuleNav();
    renderModList();
    renderHomeModules();
    updateStatusBar();
    showToast(`${MODS[id].displayName} 已移除`);
    lucide.createIcons();
  }
}

function rescan() { showToast('重新掃描完成，未發現新模組'); }

// ── Import dialog ─────────────────────────────
function openImportDialog() {
  selectedDlgFolder = null;
  document.getElementById('dlgPath').textContent = 'D:\\Helix\\Plugins\\';
  document.querySelectorAll('.dialog-file').forEach(f=>f.classList.remove('selected'));
  const btn = document.getElementById('dlgConfirmBtn');
  btn.style.opacity='.4'; btn.style.pointerEvents='none';
  document.getElementById('importDialog').classList.add('open');
}
function closeImportDialog() {
  document.getElementById('importDialog').classList.remove('open');
  selectedDlgFolder = null;
}
function selectDlgFolder(el, name) {
  document.querySelectorAll('.dialog-file').forEach(f=>f.classList.remove('selected'));
  el.classList.add('selected');
  selectedDlgFolder = name;
  document.getElementById('dlgPath').textContent = `D:\\Helix\\Plugins\\${name}`;
  const btn = document.getElementById('dlgConfirmBtn');
  btn.style.opacity='1'; btn.style.pointerEvents='all';
}
function confirmImport() {
  if (!selectedDlgFolder) return;
  closeImportDialog();
  const map = {'ProductionMonitor':'production-monitor','DeployManager':'deploy-manager'};
  const modId = map[selectedDlgFolder];
  const ws = WORKSPACES[currentWs];
  if (modId && !ws.modules.includes(modId)) {
    ws.modules.push(modId);
    renderModuleNav();
    renderModList();
    renderHomeModules();
    updateStatusBar();
    selectMod(modId);
    showToast(`${MODS[modId].displayName} 匯入成功`);
    lucide.createIcons();
  } else if (modId) {
    showToast(`${MODS[modId].displayName} 已存在`);
  } else {
    showToast(`${selectedDlgFolder} 不包含有效模組`);
  }
}

// ── Event Log ─────────────────────────────────
function renderLogs() {
  const lvMap = {info:'lv-i',warning:'lv-w',error:'lv-e',debug:'lv-d'};
  const lvLabel = {info:'Info',warning:'Warning',error:'Error',debug:'Debug'};
  document.getElementById('logBody').innerHTML = LOGS.map(e => `
    <div class="log-row${e.lv==='error'?' lv-error':''}" data-lv="${e.lv}">
      <div class="log-td">${e.ts}</div>
      <div class="log-td"><span class="lvbadge ${lvMap[e.lv]}">${lvLabel[e.lv]}</span></div>
      <div class="log-td">${e.src}</div>
      <div class="log-td">${e.msg}</div>
    </div>`).join('');
}
function setLogFilter(f, btn) {
  logFilter = f;
  document.querySelectorAll('.filter-btn').forEach(b=>b.classList.remove('active'));
  btn.classList.add('active');
  document.querySelectorAll('.log-row').forEach(r => {
    r.classList.toggle('hidden', f!=='all' && r.dataset.lv!==f);
  });
}

// ── Module UIs ────────────────────────────────
function renderModuleUI(modId) {
  const wrap = document.getElementById('moduleUiContent');
  if (modId === 'production-monitor') {
    wrap.innerHTML = buildProductionMonitor();
    selectMachine('M01');
  } else if (modId === 'deploy-manager') {
    wrap.innerHTML = buildDeployManager();
  } else if (modId === 'testrunner-core') {
    wrap.innerHTML = buildTestRunner();
  } else if (modId === 'mockserver-lite') {
    wrap.innerHTML = buildMockServer();
  } else {
    wrap.innerHTML = buildGenericModule(modId);
  }
  lucide.createIcons();
}

function buildProductionMonitor() {
  const machineRows = MACHINES.map(m => {
    const dotColor = {running:'var(--StatusSuccess)',error:'var(--StatusDanger)',idle:'var(--TextTertiary)'}[m.status];
    const pct = m.out === 0 ? 0 : Math.min(100, Math.round(m.out/m.tgt*100));
    return `<div class="pm-machine-row" onclick="selectMachine('${m.id}')" data-mid="${m.id}">
      <div class="pm-status-dot" style="background:${dotColor}"></div>
      <div class="pm-machine-name">${m.name}</div>
      <div class="pm-machine-out">${m.out===0?'—':m.out.toLocaleString()}</div>
    </div>`;
  }).join('');

  return `
    <div class="mui-header">
      <div>
        <div class="mui-hdr-title">ProductionMonitor</div>
        <div class="mui-hdr-sub">產線機台即時監控  ·  v1.3.0  ·  Vaeron Studio</div>
      </div>
      <div class="mui-hdr-actions">
        <div class="ghost-btn"><i data-lucide="refresh-cw" style="width:13px;height:13px"></i>重新整理</div>
        <div class="ghost-btn"><i data-lucide="download" style="width:13px;height:13px"></i>匯出報表</div>
      </div>
    </div>
    <div class="mui-body">
      <div class="pm-left">
        <div class="pm-section">
          <div class="pm-section-lbl">今日總覽</div>
          <div class="pm-stat"><div class="pm-stat-val" style="color:var(--StatusSuccess)">3</div><div class="pm-stat-unit">/ 5 運行中</div></div>
          <div class="pm-stat-sub" style="color:var(--StatusDanger);margin-bottom:4px">⚠ 1 台異常</div>
          <div class="pm-stat-sub">今日產量：3,134 件</div>
        </div>
        <div class="pm-section-lbl" style="padding:10px 14px 4px">機台列表</div>
        <div class="pm-machine-list">${machineRows}</div>
      </div>
      <div class="pm-right">
        <div class="pm-detail" id="pmDetail"></div>
      </div>
    </div>`;
}

function selectMachine(mid) {
  selectedMachine = mid;
  document.querySelectorAll('.pm-machine-row').forEach(r => r.classList.toggle('active', r.dataset.mid===mid));
  const m = MACHINES.find(x=>x.id===mid);
  if (!m) return;
  const stColor = {running:'var(--StatusSuccess)',error:'var(--StatusDanger)',idle:'var(--TextTertiary)'}[m.status];
  const stLabel = {running:'運行中',error:'異常',idle:'待機'}[m.status];
  const pct = m.out===0?0:Math.min(100,Math.round(m.out/m.tgt*100));
  const barColor = m.status==='error'?'var(--StatusDanger)':m.out>=m.tgt?'var(--StatusSuccess)':'var(--AccentPrimary)';

  const events = {
    M01:[['14:05:02','正常運行'],['14:02:18','完成批次 #A-0041'],['13:58:00','班次開始']],
    M02:[['14:03:41','感測器訊號中斷'],['14:01:20','警告：訊號衰減'],['13:55:00','正常運行']],
    M03:[['14:05:02','正常運行'],['14:03:00','產量超標預警'],['14:00:00','班次開始']],
    M04:[['13:58:10','排班待機'],['13:30:00','完成批次'],['12:00:00','班次結束']],
    M05:[['14:04:58','正常運行'],['14:01:00','完成批次 #C-0022'],['13:55:00','班次開始']],
  }[mid] || [];

  document.getElementById('pmDetail').innerHTML = `
    <div class="pm-detail-hdr">
      <div class="pm-status-dot" style="background:${stColor};width:10px;height:10px"></div>
      <div class="pm-detail-name">${m.name}  ·  ${m.id}</div>
      <span style="font-family:var(--mono);font-size:10px;padding:2px 8px;border-radius:3px;background:rgba(${m.status==='running'?'66,179,107':m.status==='error'?'255,107,98':'107,122,153'},0.1);color:${stColor}">${stLabel}</span>
    </div>
    <div class="pm-stats-row">
      <div class="pm-stat-card">
        <div class="pm-sc-lbl">今日產量</div>
        <div class="pm-sc-val" style="color:var(--TextPrimary)">${m.out===0?'—':m.out.toLocaleString()}</div>
        <div class="pm-sc-sub">目標 ${m.tgt.toLocaleString()} 件</div>
      </div>
      <div class="pm-stat-card">
        <div class="pm-sc-lbl">達成率</div>
        <div class="pm-sc-val" style="color:${pct>=100?'var(--StatusSuccess)':pct>60?'var(--AccentPrimary)':'var(--StatusDanger)'}">${m.out===0?'—':pct+'%'}</div>
        <div class="pm-sc-sub">當班目標</div>
      </div>
      <div class="pm-stat-card">
        <div class="pm-sc-lbl">狀態</div>
        <div class="pm-sc-val" style="font-size:14px;color:${stColor}">${stLabel}</div>
        <div class="pm-sc-sub">${m.last.split(' ')[0]}</div>
      </div>
    </div>
    <div class="pm-progress-wrap">
      <div class="pm-progress-lbl"><span>產量進度</span><span>${m.out===0?'0':pct}%</span></div>
      <div class="pm-progress-bar"><div class="pm-progress-fill" style="width:${pct}%;background:${barColor}"></div></div>
    </div>
    <div class="pm-events">
      <div class="pm-events-hdr">
        <div class="pm-events-th">時間</div>
        <div class="pm-events-th">事件</div>
      </div>
      ${events.map(([t,e])=>`<div class="pm-event-row">
        <div class="pm-event-td">${t}</div>
        <div class="pm-event-td" style="color:${e.includes('異常')||e.includes('中斷')||e.includes('衰減')?'var(--StatusDanger)':e.includes('警告')||e.includes('預警')?'var(--StatusWarning)':'var(--TextPrimary)'};font-family:var(--sans);font-size:11px">${e}</div>
      </div>`).join('')}
    </div>`;
}

function buildDeployManager() {
  const stC = {success:'var(--StatusSuccess)',failed:'var(--StatusDanger)',running:'var(--AccentPrimary)'};
  const stL = {success:'成功',failed:'失敗',running:'進行中'};
  const stBg = {success:'var(--StatusSuccessSoft)',failed:'var(--StatusDangerSoft)',running:'var(--AccentSoft)'};

  const envCards = [
    {name:'Development', ver:'v2.4.2-dev', st:'running', ago:'3 分鐘前'},
    {name:'Staging',     ver:'v2.4.1',     st:'success', ago:'12 分鐘前'},
    {name:'Production',  ver:'v2.4.1',     st:'success', ago:'14 分鐘前'},
  ].map(e=>`
    <div class="dm-env-card">
      <div class="dm-env-hdr">
        <div class="dm-env-name">${e.name}</div>
        <span style="font-family:var(--mono);font-size:9px;font-weight:700;padding:2px 7px;border-radius:2px;background:${stBg[e.st]};color:${stC[e.st]}">${stL[e.st]}</span>
      </div>
      <div class="dm-env-ver">${e.ver}</div>
      <div class="dm-env-time">${e.ago}更新</div>
    </div>`).join('');

  const rows = DEPLOY_RECORDS.map((r,i)=>`
    <div class="dm-row${r.st==='failed'?' failed':''}">
      <div class="dm-td ver">${r.ver}</div>
      <div class="dm-td sans">${r.env}</div>
      <div class="dm-td"><span style="font-family:var(--mono);font-size:9px;font-weight:700;padding:2px 6px;border-radius:2px;background:${stBg[r.st]};color:${stC[r.st]}">${stL[r.st]}</span></div>
      <div class="dm-td">${r.op}</div>
      <div class="dm-td">${r.ts}</div>
      <div class="dm-td">${r.dur}</div>
    </div>`).join('');

  return `
    <div class="mui-header">
      <div>
        <div class="mui-hdr-title">DeployManager</div>
        <div class="mui-hdr-sub">多環境部署管理  ·  v2.1.4  ·  Vaeron Studio</div>
      </div>
      <div class="mui-hdr-actions">
        <div class="primary-btn" onclick="showToast('部署流程已啟動')">
          <i data-lucide="send" style="width:13px;height:13px"></i>新增部署
        </div>
        <div class="ghost-btn"><i data-lucide="refresh-cw" style="width:13px;height:13px"></i>重新整理</div>
      </div>
    </div>
    <div class="dm-body">
      <div class="dm-env-grid">${envCards}</div>
      <div style="font-family:var(--mono);font-size:10px;font-weight:500;color:var(--TextTertiary);letter-spacing:.4px;margin-bottom:10px">DEPLOYMENT HISTORY</div>
      <div class="dm-tbl-hdr">
        <div class="dm-th">版本</div>
        <div class="dm-th">目標環境</div>
        <div class="dm-th">狀態</div>
        <div class="dm-th">操作者</div>
        <div class="dm-th">時間</div>
        <div class="dm-th">耗時</div>
      </div>
      ${rows}
    </div>`;
}

function buildTestRunner() {
  return `
    <div class="mui-header">
      <div><div class="mui-hdr-title">TestRunner.Core</div><div class="mui-hdr-sub">自動化測試執行引擎  ·  v1.2.0</div></div>
      <div class="mui-hdr-actions">
        <div class="primary-btn" onclick="showToast('測試執行中…')"><i data-lucide="play" style="width:13px;height:13px"></i>執行測試</div>
      </div>
    </div>
    <div style="padding:24px;overflow-y:auto;flex:1">
      <div style="display:grid;grid-template-columns:repeat(4,1fr);gap:10px;margin-bottom:20px">
        ${[['總測試數','48',''],['通過','45','var(--StatusSuccess)'],['失敗','2','var(--StatusDanger)'],['略過','1','var(--TextTertiary)']].map(([l,v,c])=>`
        <div class="card"><div class="card-col-header">${l}</div><div class="card-value big" ${c?`style="color:${c}"`:''}>${v}</div></div>`).join('')}
      </div>
      ${[
        ['TestSuite.Production.SmokeTest','通過','1.2s'],
        ['TestSuite.Integration.ApiGateway','通過','3.4s'],
        ['TestSuite.Unit.DataParser','通過','0.8s'],
        ['TestSuite.Integration.Storage','失敗','2.1s'],
        ['TestSuite.E2E.Workflow','失敗','8.7s'],
        ['TestSuite.Unit.Formatter','略過','—'],
      ].map(([name,st,dur])=>{
        const c={通過:'var(--StatusSuccess)',失敗:'var(--StatusDanger)',略過:'var(--TextTertiary)'}[st];
        return `<div style="display:grid;grid-template-columns:1fr 70px 60px;height:34px;border-bottom:1px solid var(--BorderLight);${st==='失敗'?'border-left:2px solid var(--StatusDanger);':''}">
          <div style="display:flex;align-items:center;padding:0 12px;font-size:12px;color:var(--TextPrimary)">${name}</div>
          <div style="display:flex;align-items:center;padding:0 8px"><span style="font-family:var(--mono);font-size:9px;font-weight:700;padding:2px 6px;border-radius:2px;background:rgba(${st==='通過'?'66,179,107':st==='失敗'?'255,107,98':'107,122,153'},0.1);color:${c}">${st}</span></div>
          <div style="display:flex;align-items:center;padding:0 8px;font-family:var(--mono);font-size:10px;color:var(--TextTertiary)">${dur}</div>
        </div>`;
      }).join('')}
    </div>`;
}

function buildMockServer() {
  return `
    <div class="mui-header">
      <div><div class="mui-hdr-title">MockServer.Lite</div><div class="mui-hdr-sub">輕量級 Mock Server  ·  v0.9.4</div></div>
      <div class="mui-hdr-actions">
        <div class="primary-btn" style="background:var(--StatusSuccess)" onclick="showToast('Mock Server 已啟動  ·  port 8080')"><i data-lucide="play" style="width:13px;height:13px"></i>啟動伺服器</div>
      </div>
    </div>
    <div style="padding:24px;overflow-y:auto;flex:1">
      <div style="display:grid;grid-template-columns:repeat(3,1fr);gap:10px;margin-bottom:20px">
        ${[['狀態','已停止','var(--TextTertiary)'],['已定義路由','6 條',''],['請求攔截','—','']].map(([l,v,c])=>`
        <div class="card"><div class="card-col-header">${l}</div><div class="card-value" ${c?`style="color:${c}"`:''}>${v}</div></div>`).join('')}
      </div>
      <div style="font-family:var(--mono);font-size:10px;font-weight:500;color:var(--TextTertiary);letter-spacing:.4px;margin-bottom:10px">MOCK ROUTES</div>
      ${[
        ['GET','/api/v1/products','200','products.json'],
        ['GET','/api/v1/orders','200','orders.json'],
        ['POST','/api/v1/orders','201','order_created.json'],
        ['GET','/api/v1/inventory','200','inventory.json'],
        ['PUT','/api/v1/inventory/:id','200','update_ok.json'],
        ['GET','/api/v1/users/me','401','auth_error.json'],
      ].map(([method,path,code,file])=>{
        const mc={GET:'var(--StatusSuccess)',POST:'var(--AccentPrimary)',PUT:'var(--StatusWarning)'}[method]||'var(--TextTertiary)';
        const cc={'200':'var(--StatusSuccess)','201':'var(--StatusSuccess)','401':'var(--StatusDanger)'}[code];
        return `<div style="display:grid;grid-template-columns:60px 1fr 50px 140px;height:34px;border-bottom:1px solid var(--BorderLight)">
          <div style="display:flex;align-items:center;padding:0 10px"><span style="font-family:var(--mono);font-size:9px;font-weight:700;color:${mc}">${method}</span></div>
          <div style="display:flex;align-items:center;padding:0 10px;font-family:var(--mono);font-size:11px;color:var(--TextPrimary)">${path}</div>
          <div style="display:flex;align-items:center;padding:0 10px;font-family:var(--mono);font-size:11px;color:${cc}">${code}</div>
          <div style="display:flex;align-items:center;padding:0 10px;font-family:var(--mono);font-size:10px;color:var(--TextTertiary)">${file}</div>
        </div>`;
      }).join('')}
    </div>`;
}

function buildGenericModule(id) {
  const m = MODS[id];
  return `
    <div class="mui-header">
      <div><div class="mui-hdr-title">${m.displayName}</div><div class="mui-hdr-sub">v${m.version}  ·  ${m.author}</div></div>
    </div>
    <div style="flex:1;display:flex;align-items:center;justify-content:center;flex-direction:column;gap:12px;color:var(--TextTertiary)">
      <i data-lucide="${m.icon}" style="width:40px;height:40px;opacity:.3"></i>
      <div style="font-size:14px;color:var(--TextSecondary)">${m.displayName}</div>
      <div style="font-size:12px;max-width:320px;text-align:center;line-height:1.6">${m.desc}</div>
    </div>`;
}

// ── Status bar ────────────────────────────────
function updateStatusBar() {
  const ws = WORKSPACES[currentWs];
  document.getElementById('sbWs').textContent = ws.name;
  document.getElementById('sbMods').textContent = `模組：${ws.modules.length} loaded`;
}

// ── Toast ─────────────────────────────────────
let toastTimer = null;
function showToast(msg) {
  const t = document.getElementById('toast');
  document.getElementById('toastMsg').textContent = msg;
  t.classList.add('show');
  if (toastTimer) clearTimeout(toastTimer);
  toastTimer = setTimeout(()=>t.classList.remove('show'), 2800);
}

// ── Close popup on outside click ──────────────
document.addEventListener('click', e => {
  if (wsPopupOpen &&
      !document.getElementById('wsPopup').contains(e.target) &&
      !document.getElementById('wsBtnEl').contains(e.target)) closeWsPopup();
});

if (document.getElementById('helix-demo-root')) init();
