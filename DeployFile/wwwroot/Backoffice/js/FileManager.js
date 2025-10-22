// 檔案總管 JavaScript

let currentPath = '/';
let renameTarget = { name: '', type: '', path: '' };

// 初始化
$(function() {
    // 初始化 Lucide Icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    // 取得當前路徑（從頁面元素或 URL）
    const $pathEl = $('[data-current-path]');
    if ($pathEl.length) {
        currentPath = $pathEl.attr('data-current-path') || '/';
    } else {
        const urlParams = new URLSearchParams(window.location.search);
        currentPath = urlParams.get('path') || '/';
    }
    
    // 初始化拖曳上傳
    initializeDragAndDrop();
    
    // 初始化檔案輸入
    initializeFileInput();
    
    console.log('檔案總管已載入，當前路徑：', currentPath);
});

// ===== 圖片預覽（自訂 Viewer） =====
let __imgPv = {
    scale: 1,
    minScale: 0.1,
    maxScale: 5,
    fitScale: 1,
    originX: 0,
    originY: 0,
    isPanning: false,
    startX: 0,
    startY: 0
};

// 初始化圖片預覽（置中、以容器中心縮放、支援拖移）
function setupImagePreview(src) {
    const $wrap = $('#imagePreviewWrapper');
    const $img = $('#imagePreview');
    if ($wrap.length === 0 || $img.length === 0) return;
    const wrap = $wrap[0];
    const img = $img[0];
    __imgPv.scale = 1; __imgPv.originX = 0; __imgPv.originY = 0; __imgPv.fitScale = 1;
    img.style.transformOrigin = '0 0';
    img.style.transform = 'translate(0px, 0px) scale(1)';
    img.style.cursor = 'grab';
    img.style.willChange = 'transform';
    $img.on('load', function() {
        const wrapRect = wrap.getBoundingClientRect();
        const scaleX = wrapRect.width / img.naturalWidth;
        const scaleY = wrapRect.height / img.naturalHeight;
        __imgPv.fitScale = Math.min(scaleX, scaleY, 1);
        __imgPv.scale = __imgPv.fitScale;
        __imgPv.originX = Math.round((wrapRect.width - img.naturalWidth * __imgPv.scale) / 2);
        __imgPv.originY = Math.round((wrapRect.height - img.naturalHeight * __imgPv.scale) / 2);
        applyImageTransform();
    });
    img.src = src;
    $wrap.on('wheel', function(e) {
        e.preventDefault();
        const delta = e.originalEvent.deltaY < 0 ? 1.1 : 0.9;
        const newScale = clamp(__imgPv.scale * delta, __imgPv.minScale, __imgPv.maxScale);
        const wrapRect = wrap.getBoundingClientRect();
        const mx = wrapRect.width / 2;
        const my = wrapRect.height / 2;
        const preX = (mx - __imgPv.originX) / __imgPv.scale;
        const preY = (my - __imgPv.originY) / __imgPv.scale;
        __imgPv.scale = newScale;
        __imgPv.originX = Math.round(mx - preX * __imgPv.scale);
        __imgPv.originY = Math.round(my - preY * __imgPv.scale);
        applyImageTransform();
    });
    $wrap.on('mousedown', function(e) {
        if (e.button !== 0) return;
        __imgPv.isPanning = true;
        __imgPv.startX = e.clientX - __imgPv.originX;
        __imgPv.startY = e.clientY - __imgPv.originY;
        img.style.cursor = 'grabbing';
    });
    $(window).on('mouseup', function() {
        __imgPv.isPanning = false;
        img.style.cursor = 'grab';
    });
    $(window).on('mousemove', function(e) {
        if (!__imgPv.isPanning) return;
        __imgPv.originX = Math.round(e.clientX - __imgPv.startX);
        __imgPv.originY = Math.round(e.clientY - __imgPv.startY);
        applyImageTransform();
    });
}

function applyImageTransform() {
    const $img = $('#imagePreview');
    if ($img.length === 0) return;
    $img[0].style.transform = `translate(${__imgPv.originX}px, ${__imgPv.originY}px) scale(${__imgPv.scale})`;
}

function clamp(v, a, b) { return Math.max(a, Math.min(b, v)); }

// 圖片預覽：適合視窗
function imagePreviewFit() {
    __imgPv.scale = __imgPv.fitScale || 1;
    __imgPv.originX = 0;
    __imgPv.originY = 0;
    applyImageTransform();
}
// 圖片預覽：實際大小
function imagePreviewActual() {
    __imgPv.scale = 1;
    __imgPv.originX = 0;
    __imgPv.originY = 0;
    applyImageTransform();
}
// 圖片預覽：放大
function imagePreviewZoomIn() {
    __imgPv.scale = clamp(__imgPv.scale * 1.2, __imgPv.minScale, __imgPv.maxScale);
    applyImageTransform();
}
// 圖片預覽：縮小
function imagePreviewZoomOut() {
    __imgPv.scale = clamp(__imgPv.scale / 1.2, __imgPv.minScale, __imgPv.maxScale);
    applyImageTransform();
}

// ========== 拖曳上傳初始化 ==========

function initializeDragAndDrop() {
    const $dropZone = $('#dropZone');
    if ($dropZone.length === 0) return;
    
    $dropZone.on('dragenter dragover dragleave drop', function(e) {
        e.preventDefault();
        e.stopPropagation();
    });
    
    $dropZone.on('dragover', function() {
        $dropZone.addClass('border-primary bg-light');
    });
    
    $dropZone.on('dragleave', function() {
        $dropZone.removeClass('border-primary bg-light');
    });
    
    $dropZone.on('drop', async function(e) {
        $dropZone.removeClass('border-primary bg-light');
        
        // 使用 DataTransferItems API 來支援資料夾拖曳
        const items = e.originalEvent.dataTransfer.items;
        if (items && items.length > 0) {
            const filesWithPaths = await getAllFilesFromItems(items);
            handleFileUploadWithPaths(filesWithPaths);
        } else {
            // 降級處理：只支援檔案拖曳
            const files = Array.from(e.originalEvent.dataTransfer.files).map(f => ({ file: f, path: f.name }));
            handleFileUploadWithPaths(files);
        }
    });
}

// 從 DataTransferItems 中遞迴獲取所有檔案（支援資料夾）
async function getAllFilesFromItems(items) {
    const filesWithPaths = [];
    
    // 遍歷所有拖曳項目
    for (let i = 0; i < items.length; i++) {
        const item = items[i].webkitGetAsEntry();
        if (item) {
            await traverseFileTree(item, '', filesWithPaths);
        }
    }
    
    return filesWithPaths;
}

// 遞迴遍歷檔案樹
async function traverseFileTree(item, path, filesWithPaths) {
    if (item.isFile) {
        // 如果是檔案，獲取 File 物件
        const file = await new Promise((resolve, reject) => {
            item.file(resolve, reject);
        });
        filesWithPaths.push({
            file: file,
            path: path + file.name
        });
    } else if (item.isDirectory) {
        // 如果是資料夾，遞迴處理
        const dirReader = item.createReader();
        const entries = await new Promise((resolve, reject) => {
            dirReader.readEntries(resolve, reject);
        });
        
        for (const entry of entries) {
            await traverseFileTree(entry, path + item.name + '/', filesWithPaths);
        }
    }
}

function initializeFileInput() {
    const $fileInput = $('#fileInput');
    if ($fileInput.length === 0) return;
    
    $fileInput.on('change', function(e) {
        handleFileUpload(Array.from(e.target.files));
        // 清空 input，允許重複上傳同一檔案
        $(this).val('');
    });
}


// ========== 新增資料夾 ==========

async function createFolder() {
    if (isCreatingFolder) return;
    
    const $form = $('#createFolderForm');
    const dataStr = $form.serialize();
    const name = ($form.find('[name="Name"]').val() || '').toString().trim();
    if (!name) { alert('請輸入資料夾名稱'); return; }
    
    // 設定防呆狀態
    isCreatingFolder = true;
    const $btn = $("#createFolderModal button:contains('建立')");
    const originalText = $btn.text();
    $btn.prop('disabled', true).text('處理中...');
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/FileManager/CreateFolder',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({ Path: currentPath, Name: name, SharedUsers: parseShared($form.find('[name="SharedUsers"]').val()) }),
        success: function (res) {
            if (res && res.success) {
                closeModal('createFolderModal');
                location.reload();
            } else {
                alert(res && res.message ? res.message : '建立失敗');
                // 恢復按鈕狀態
                $btn.prop('disabled', false).text(originalText);
                isCreatingFolder = false;
            }
        },
        error: function () { 
            alert('建立失敗，請稍後再試。'); 
            // 恢復按鈕狀態
            $btn.prop('disabled', false).text(originalText);
            isCreatingFolder = false;
        }
    });
}

// ========== 新增檔案 ==========

async function createFile() {
    if (isCreatingFile) return;
    
    const $form = $('#createFileForm');
    const name = ($form.find('[name="Name"]').val() || '').toString().trim();
    if (!name) { alert('請輸入檔案名稱'); return; }
    
    // 設定防呆狀態
    isCreatingFile = true;
    const $btn = $("#createFileModal button:contains('建立')");
    const originalText = $btn.text();
    $btn.prop('disabled', true).text('處理中...');
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/FileManager/CreateFile',
        contentType: 'application/json; charset=utf-8',
        data: JSON.stringify({ Path: currentPath, Name: name, SharedUsers: parseShared($form.find('[name="SharedUsers"]').val()) }),
        success: function (res) {
            if (res && res.success) {
                closeModal('createFileModal');
                location.reload();
            } else {
                alert(res && res.message ? res.message : '建立失敗');
                // 恢復按鈕狀態
                $btn.prop('disabled', false).text(originalText);
                isCreatingFile = false;
            }
        },
        error: function () { 
            alert('建立失敗，請稍後再試。'); 
            // 恢復按鈕狀態
            $btn.prop('disabled', false).text(originalText);
            isCreatingFile = false;
        }
    });
}

function parseShared(val) {
    const s = (val || '').toString().trim();
    if (!s) return [];
    return s.split(',').map(x => x.trim()).filter(x => x.length > 0);
}

// ========== 重新命名 ==========

function showRenameModal(name, type) {
    renameTarget = { name, type, path: currentPath };
    
    const $title = $('#renameModalLabel');
    const $input = $('#renameInput');
    
    if ($title.length) $title.text(type === 'folder' ? '重新命名資料夾' : '重新命名檔案');
    if ($input.length) $input.val(name);
    
    showModal('renameModal');
    
    if ($input.length) {
        setTimeout(() => {
            $input.focus().select();
        }, 100);
    }
}

async function confirmRename() {
    const newName = $('#renameInput').val().trim();
    if (!newName) {
        alert('請輸入新名稱');
        return;
    }
    
    if (newName === renameTarget.name) {
        const modal = bootstrap.Modal.getInstance($('#renameModal')[0]);
        if (modal) modal.hide();
        return;
    }
    
    const endpoint = renameTarget.type === 'folder' ? 'RenameFolder' : 'RenameFile';
    $.ajax({
        type: 'POST',
        url: `/Backoffice/FileManager/${endpoint}`,
        contentType: 'application/json',
        data: JSON.stringify({ 
            path: renameTarget.path, 
            oldName: renameTarget.name, 
            newName,
            sharedUsers: [] // 暫時傳送空陣列，需要從資料庫獲取現有共享者
        }),
        success: function(result) {
            if (result.success) {
                const modal = bootstrap.Modal.getInstance($('#renameModal')[0]);
                if (modal) modal.hide();
                location.reload();
            } else {
                alert(result.message);
            }
        },
        error: function() {
            alert('操作失敗，請稍後再試');
        }
    });
}

// ========== 刪除 ==========

let deleteTarget = { name: '', type: '', path: '' };

function deleteItem(name, type) {
    deleteTarget = { name, type, path: currentPath };
    
    const itemType = type === 'folder' ? '資料夾' : '檔案';
    const $title = $('#deleteModalLabel');
    const $targetName = $('#deleteTargetName');
    const $input = $('#deleteConfirmInput');
    const $btn = $('#deleteConfirmBtn');
    
    if ($title.length) $title.text(`刪除${itemType}`);
    if ($targetName.length) $targetName.text(name);
    if ($input.length) {
        $input.val('');
        $input.attr('placeholder', `輸入「${name}」以確認刪除`);
    }
    if ($btn.length) $btn.prop('disabled', true);
    
    showModal('deleteModal');
    
    // 重新初始化 Lucide Icons（因為有新的 icon）
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function validateDeleteInput() {
    const $input = $('#deleteConfirmInput');
    const $btn = $('#deleteConfirmBtn');
    
    if ($input.length === 0 || $btn.length === 0) return;
    
    // 檢查輸入是否與目標名稱完全匹配
    const isMatch = $input.val() === deleteTarget.name;
    $btn.prop('disabled', !isMatch);
}

// 防呆變數
let isDeleting = false;
let isCreatingFolder = false;
let isCreatingFile = false;

async function confirmDelete() {
    if (!deleteTarget.name || isDeleting) return;
    
    // 設定防呆狀態
    isDeleting = true;
    const $btn = $('#deleteConfirmBtn');
    const originalText = $btn.text();
    
    // 更新按鈕狀態
    $btn.prop('disabled', true).text('刪除中...');
    
    console.log('[刪除操作]', {
        type: deleteTarget.type,
        path: deleteTarget.path,
        name: deleteTarget.name
    });
    
    const endpoint = deleteTarget.type === 'folder' ? 'DeleteFolder' : 'DeleteFile';
    const payload = { 
        path: deleteTarget.path, 
        name: deleteTarget.name 
    };
    
    $.ajax({
        type: 'POST',
        url: `/Backoffice/FileManager/${endpoint}`,
        contentType: 'application/json',
        data: JSON.stringify(payload),
        success: function(result) {
            console.log('[刪除結果]', result);
            if (result.success) {
                const modal = bootstrap.Modal.getInstance($('#deleteModal')[0]);
                if (modal) modal.hide();
                location.reload();
            } else {
                alert(result.message);
                // 恢復按鈕狀態
                $btn.prop('disabled', false).text(originalText);
                isDeleting = false;
            }
        },
        error: function(xhr, status, error) {
            console.error('[刪除錯誤]', error);
            alert('操作失敗，請稍後再試');
            // 恢復按鈕狀態
            $btn.prop('disabled', false).text(originalText);
            isDeleting = false;
        }
    });
}

// ========== 檔案上傳 ==========

// 處理帶路徑信息的檔案上傳（支援資料夾結構）
async function handleFileUploadWithPaths(filesWithPaths) {
    if (!filesWithPaths || filesWithPaths.length === 0) return;
    
    const $progress = $('#uploadProgress');
    const $items = $('#uploadItems');
    const $speedEl = $('#uploadSpeed');
    const $etaEl = $('#uploadEta');
    
    // 顯示上傳進度視窗
    const modal = new bootstrap.Modal($progress[0]);
    modal.show();
    $items.html('');
    
    const startTime = Date.now();
    const totalBytes = filesWithPaths.reduce((sum, f) => sum + f.file.size, 0);
    let completedBytes = 0;
    const currentLoaded = new Array(filesWithPaths.length).fill(0);
    
    console.log(`[並行上傳] 共 ${filesWithPaths.length} 個檔案，總大小 ${formatSize(totalBytes)}`);
    
    // 更新整體進度的函數
    function updateOverallProgress(index, loaded) {
        currentLoaded[index] = loaded;
        const totalLoaded = currentLoaded.reduce((sum, val) => sum + val, 0);
        
        const elapsed = (Date.now() - startTime) / 1000;
        const speed = totalLoaded / elapsed;
        const remaining = totalBytes - totalLoaded;
        const eta = remaining / speed;
        
        $speedEl.text(formatSize(Math.round(speed)) + '/s');
        $etaEl.text('剩餘 ' + formatTime(eta));
    }
    
    // 建立所有上傳項目的 UI
    filesWithPaths.forEach((item, i) => {
        const itemId = 'upload-item-' + i;
        $items.append(`
            <div id="${itemId}" class="mb-2">
                <div class="d-flex align-items-center justify-content-between mb-1">
                    <span class="small text-truncate" title="${item.path}">${item.path}</span>
                    <span class="badge bg-light text-muted upload-percent">0%</span>
                </div>
                <div class="progress" style="height: 4px;">
                    <div class="progress-bar bg-primary upload-bar" role="progressbar" style="width: 0%"></div>
                </div>
            </div>
        `);
    });
    
    // 並行上傳所有檔案
    const uploadPromises = filesWithPaths.map(async ({ file, path }, i) => {
        const itemId = 'upload-item-' + i;
        
        // 建立 FormData
        const formData = new FormData();
        formData.append('path', currentPath);
        formData.append('files', file);
        formData.append('keys', path);
        
        try {
            await uploadFileWithProgress(file, formData, itemId, (loaded) => {
                updateOverallProgress(i, loaded);
            });
            
            completedBytes += file.size;
            
            // 標記完成
            $(`#${itemId} .upload-percent`).text('完成').removeClass('bg-light text-muted').addClass('bg-success text-white');
            $(`#${itemId} .upload-bar`).removeClass('bg-primary').addClass('bg-success');
            
            return { success: true, path };
        } catch (error) {
            console.error(`上傳失敗 [${path}]:`, error);
            $(`#${itemId} .upload-percent`).text('失敗').removeClass('bg-light text-muted').addClass('bg-danger text-white');
            return { success: false, path, error };
        }
    });
    
    // 等待所有上傳完成
    const results = await Promise.all(uploadPromises);
    
    const successCount = results.filter(r => r.success).length;
    const failCount = results.filter(r => !r.success).length;
    
    console.log(`[上傳完成] 成功: ${successCount}, 失敗: ${failCount}`);
    
    // 全部完成後延遲關閉並重新載入
    setTimeout(() => {
        const modal = bootstrap.Modal.getInstance($progress[0]);
        if (modal) modal.hide();
        location.reload();
    }, 2000);
}

// 處理普通檔案上傳（向後兼容）
async function handleFileUpload(files) {
    const filesWithPaths = Array.from(files).map(f => ({ file: f, path: f.name }));
    await handleFileUploadWithPaths(filesWithPaths);
}

function uploadFileWithProgress(file, formData, itemId, onProgress) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        
        xhr.upload.onprogress = (e) => {
            if (e.lengthComputable) {
                const percent = Math.round((e.loaded / e.total) * 100);
                
                // 更新此檔案的進度
                const $percentEl = $(`#${itemId} .upload-percent`);
                const $barEl = $(`#${itemId} .upload-bar`);
                
                if ($percentEl.length) $percentEl.text(percent + '%');
                if ($barEl.length) $barEl.css('width', percent + '%');
                
                // 回調整體進度
                if (onProgress) onProgress(e.loaded);
            }
        };
        
        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.response);
            } else {
                reject(new Error('上傳失敗'));
            }
        };
        
        xhr.onerror = () => reject(new Error('網路錯誤'));
        
        xhr.open('POST', '/Backoffice/FileManager/Upload');
        xhr.send(formData);
    });
}

function formatSize(bytes) {
    const sizes = ['B', 'KB', 'MB', 'GB'];
    if (bytes === 0) return '0 B';
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return (bytes / Math.pow(1024, i)).toFixed(2) + ' ' + sizes[i];
}

function formatTime(seconds) {
    if (!isFinite(seconds) || seconds < 0) return '計算中...';
    if (seconds < 60) return Math.round(seconds) + ' 秒';
    if (seconds < 3600) return Math.round(seconds / 60) + ' 分鐘';
    return Math.round(seconds / 3600) + ' 小時';
}

// ========== 檔案預覽與編輯 ==========

let currentEditingFile = { name: '', path: '', encoding: 'utf-8', originalContent: '' };

// 判斷檔案類型
function getFileType(fileName) {
    const ext = fileName.toLowerCase().split('.').pop();
    
    // PDF 文件
    if (ext === 'pdf') {
        return 'pdf';
    }
    
    // Office 文件
    const officeExts = ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'];
    if (officeExts.includes(ext)) {
        return 'office';
    }
    
    // 可編輯的文字檔案
    const editableExts = ['txt', 'js', 'css', 'html', 'json', 'xml', 'md', 'yml', 'yaml', 
                          'cs', 'java', 'py', 'php', 'rb', 'go', 'rs', 'ts', 'tsx', 'jsx',
                          'sql', 'sh', 'bat', 'ps1', 'ini', 'conf', 'log', 'csv'];
    if (editableExts.includes(ext)) {
        return 'editable';
    }
    
    // 圖片
    const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'svg', 'webp'];
    if (imageExts.includes(ext)) {
        return 'image';
    }
    
    // 影片
    const videoExts = ['mp4', 'webm', 'ogg', 'mov', 'avi'];
    if (videoExts.includes(ext)) {
        return 'video';
    }
    
    // 音訊
    const audioExts = ['mp3', 'wav', 'ogg', 'aac', 'flac'];
    if (audioExts.includes(ext)) {
        return 'audio';
    }
    
    return 'other';
}

// 打開檔案
async function openFile(fileName) {
    const fileType = getFileType(fileName);
    
    currentEditingFile = {
        name: fileName,
        path: currentPath,
        encoding: 'utf-8',
        originalContent: ''
    };
    
    // 顯示編輯器 Modal
    const $modal = $('#fileEditorModal');
    const $editorContent = $('#fileEditorContent');
    const $previewFrame = $('#filePreviewFrame');
    const $imgWrap = $('#imagePreviewWrapper');
    const $loading = $('#editorLoading');
    const $cannotPreview = $('#cannotPreview');
    const $fileNameEl = $('#editorFileName');
    const $readOnlyBadge = $('#editorReadOnlyBadge');
    const $saveBtn = $('#saveFileBtn');
    const $infoEl = $('#editorInfo');
    const $encodingEl = $('#editorEncoding');
    
    // 重置狀態
    const modal = new bootstrap.Modal($modal[0]);
    modal.show();
    $editorContent.addClass('d-none');
    $previewFrame.addClass('d-none');
    $imgWrap.addClass('d-none');
    $cannotPreview.addClass('d-none');
    $loading.removeClass('d-none');
    $fileNameEl.text(fileName);
    
    // 初始化 Lucide Icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    if (fileType === 'editable') {
        // 可編輯文字檔案
        $.ajax({
            type: 'GET',
            url: '/Backoffice/FileManager/ReadTextFile',
            data: { path: currentPath, name: fileName },
            success: function(result) {
                if (result.success) {
                    currentEditingFile.originalContent = result.content;
                    currentEditingFile.encoding = result.encoding;
                    
                    $editorContent.val(result.content);
                    $editorContent.removeClass('d-none').prop('readOnly', false);
                    $loading.addClass('d-none');
                    
                    $readOnlyBadge.addClass('d-none');
                    $saveBtn.removeClass('d-none');
                    
                    $infoEl.text(`${result.content.split('\n').length} 行`);
                    $encodingEl.text(`編碼: ${result.encoding}`);
                    
                    // 重新初始化 Lucide Icons
                    if (typeof lucide !== 'undefined') {
                        lucide.createIcons();
                    }
                } else {
                    alert('無法讀取檔案：' + result.message);
                    closeFileEditor();
                }
            },
            error: function() {
                alert('讀取檔案失敗，請稍後再試');
                closeFileEditor();
            }
        });
    } else if (fileType === 'pdf') {
        // PDF 文件 - 直接在瀏覽器預覽
        const previewUrl = `/Backoffice/FileManager/Preview?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        
        $previewFrame.attr('src', previewUrl);
        $previewFrame.removeClass('d-none');
        $loading.addClass('d-none');
        
        $readOnlyBadge.removeClass('d-none');
        $saveBtn.addClass('d-none');
        
        $infoEl.text('PDF 預覽');
        $encodingEl.text('');
        
        // 重新初始化 Lucide Icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    } else if (fileType === 'office') {
        // Office 文件 - 使用 Microsoft Office Online Viewer
        const downloadUrl = `/Backoffice/FileManager/Download?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        const fullUrl = window.location.origin + downloadUrl;
        
        // 使用 Microsoft Office Online Viewer
        const officeViewerUrl = `https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(fullUrl)}`;
        
        $previewFrame.attr('src', officeViewerUrl);
        $previewFrame.removeClass('d-none');
        $loading.addClass('d-none');
        
        $readOnlyBadge.removeClass('d-none');
        $saveBtn.addClass('d-none');
        
        $infoEl.text('Office 線上預覽 (需要網際網路連線)');
        $encodingEl.text('');
        
        // 設定超時檢測（如果 10 秒後還在載入，可能是失敗了）
        const timeout = setTimeout(() => {
            // 顯示無法預覽提示
            $previewFrame.addClass('d-none');
            $cannotPreview.removeClass('d-none');
            
            $('#cannotPreviewMessage').text('Office 線上預覽服務無法載入，可能是網路問題或檔案過大。請下載後使用 Office 應用程式查看。');
            $('#downloadFileLink').attr('href', downloadUrl);
            
            $infoEl.text('預覽失敗');
            
            // 重新初始化 Lucide Icons
            if (typeof lucide !== 'undefined') {
                lucide.createIcons();
            }
        }, 10000);
        
        // 如果成功載入，清除超時
        $previewFrame[0].onload = function() {
            clearTimeout(timeout);
        };
        
        // 重新初始化 Lucide Icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    } else if (fileType === 'image') {
        // 圖片：用自訂 viewer（適合視窗＋縮放/拖移）
        const previewUrl = `/Backoffice/FileManager/Download?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        if ($imgWrap.length) {
            setupImagePreview(previewUrl);
            $imgWrap.removeClass('d-none');
            $loading.addClass('d-none');
            $readOnlyBadge.removeClass('d-none');
            $saveBtn.addClass('d-none');
            $infoEl.text('圖片預覽');
            $encodingEl.text('');
            if (typeof lucide !== 'undefined') lucide.createIcons();
        } else {
            // 後備：用 iframe
            $previewFrame.attr('src', `/Backoffice/FileManager/Preview?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`);
            $previewFrame.removeClass('d-none');
            $loading.addClass('d-none');
        }
    } else if (fileType === 'video' || fileType === 'audio') {
        // 影片 / 音訊：仍使用內建預覽頁
        const previewUrl = `/Backoffice/FileManager/Preview?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        $previewFrame.attr('src', previewUrl);
        $previewFrame.removeClass('d-none');
        $loading.addClass('d-none');
        $readOnlyBadge.removeClass('d-none');
        $saveBtn.addClass('d-none');
        $infoEl.text(`預覽模式 (${fileType})`);
        $encodingEl.text('');
        if (typeof lucide !== 'undefined') lucide.createIcons();
    } else {
        // 其他檔案類型 - 顯示無法預覽提示
        $loading.addClass('d-none');
        $cannotPreview.removeClass('d-none');
        
        $('#cannotPreviewMessage').text('此檔案類型不支援線上預覽，請下載後查看');
        $('#downloadFileLink').attr('href', `/Backoffice/FileManager/Download?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`);
        
        $readOnlyBadge.removeClass('d-none');
        $saveBtn.addClass('d-none');
        
        $infoEl.text('不支援預覽');
        $encodingEl.text('');
        
        // 重新初始化 Lucide Icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
}

// 儲存檔案
async function saveFile() {
    const $editorContent = $('#fileEditorContent');
    const newContent = $editorContent.val();
    
    // 檢查是否有變更
    if (newContent === currentEditingFile.originalContent) {
        alert('檔案內容無變更');
        return;
    }
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/FileManager/SaveTextFile',
        contentType: 'application/json',
        data: JSON.stringify({
            path: currentEditingFile.path,
            name: currentEditingFile.name,
            content: newContent,
            encoding: currentEditingFile.encoding
        }),
        success: function(result) {
            if (result.success) {
                currentEditingFile.originalContent = newContent;
                alert('檔案已儲存');
            } else {
                alert('儲存失敗：' + result.message);
            }
        },
        error: function() {
            alert('儲存失敗，請稍後再試');
        }
    });
}

// 關閉檔案編輯器
function closeFileEditor() {
    const $modal = $('#fileEditorModal');
    const $editorContent = $('#fileEditorContent');
    const $previewFrame = $('#filePreviewFrame');
    const $imgWrap = $('#imagePreviewWrapper');
    const $imgEl = $('#imagePreview');
    
    // 檢查是否有未儲存的變更
    if ($editorContent.val() !== currentEditingFile.originalContent) {
        if (!confirm('有未儲存的變更，確定要關閉嗎？')) {
            return;
        }
    }
    
    const modal = bootstrap.Modal.getInstance($modal[0]);
    if (modal) modal.hide();
    $editorContent.val('');
    $previewFrame.attr('src', '');
    $imgWrap.addClass('d-none');
    $imgEl.attr('src', '');
    
    currentEditingFile = { name: '', path: '', encoding: 'utf-8', originalContent: '' };
}

// ========== 鍵盤快捷鍵 ==========

$(document).on('keydown', function(e) {
    // ESC 鍵關閉所有 Modal
    if (e.key === 'Escape') {
        $('.modal.show').each(function() {
            const modal = bootstrap.Modal.getInstance(this);
            if (modal) modal.hide();
        });
        
        // 關閉檔案編輯器
        const $editorModal = $('#fileEditorModal');
        if ($editorModal.hasClass('show')) {
            closeFileEditor();
        }
    }
    
    // Ctrl+S 或 Cmd+S 儲存檔案
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        const $editorModal = $('#fileEditorModal');
        const $saveBtn = $('#saveFileBtn');
        if ($editorModal.hasClass('show') && !$saveBtn.hasClass('d-none')) {
            e.preventDefault();
            saveFile();
        }
    }
    
    // Enter 鍵確認操作
    if (e.key === 'Enter') {
        // 如果 Modal 開啟，執行對應操作
        if ($('#createFolderModal').hasClass('show')) {
            createFolder();
        } else if ($('#createFileModal').hasClass('show')) {
            createFile();
        } else if ($('#renameModal').hasClass('show')) {
            confirmRename();
        } else if ($('#deleteModal').hasClass('show')) {
            const $btn = $('#deleteConfirmBtn');
            if ($btn.length && !$btn.prop('disabled')) {
                confirmDelete();
            }
        }
    }
});

// ========== 目錄樹折疊功能（簡化版） ==========
function toggleTreeNode(event, element) {
    event.preventDefault();
    event.stopPropagation();
    
    const $button = $(element);
    const $treeItem = $button.closest('.tree-item');
    if ($treeItem.length === 0) return;
    
    const $chevron = $button.find('.tree-chevron');
    const isOpen = $chevron.attr('data-lucide') === 'chevron-down';
    
    // 切換圖示
    $chevron.attr('data-lucide', isOpen ? 'chevron-right' : 'chevron-down');
    if (typeof lucide !== 'undefined' && lucide.createIcons) lucide.createIcons();
    
    // 簡單的折疊邏輯：找到下一個同級或上級的項目
    let $next = $treeItem.next('.tree-item');
    const currentLevel = parseInt($treeItem.data('level')) || 0;
    
    while ($next.length) {
        const nextLevel = parseInt($next.data('level')) || 0;
        
        // 如果遇到同級或上級項目，停止
        if (nextLevel <= currentLevel) break;
        
        if (isOpen) {
            // 收合：隱藏所有子項目
            $next.addClass('hidden');
        } else {
            // 展開：只顯示直接子項目（level = currentLevel + 1）
            if (nextLevel === currentLevel + 1) {
                $next.removeClass('hidden');
            }
        }
        
        $next = $next.next('.tree-item');
    }
}

// ========== 項目操作選單（更多 ...） ==========
let itemMenuEl = null;
let itemMenuAnchor = null;
let itemMenuOnDocClick = null;
let itemMenuOnScroll = null;
let itemMenuOnResize = null;
function ensureItemMenu() {
    if (itemMenuEl) return itemMenuEl;
    const $menu = $('<div id="itemActionMenu" class="d-none position-fixed border rounded shadow-sm bg-white p-2" style="z-index:1050; width:11rem;"></div>');
    $menu.html(`
        <button type="button" data-action="open" class="btn btn-light btn-sm w-100 text-start mb-1">
            <i data-lucide="corner-down-right" style="width:1rem;height:1rem;"></i> 開啟
        </button>
        <button type="button" data-action="details" class="btn btn-light btn-sm w-100 text-start mb-1">
            <i data-lucide="eye" style="width:1rem;height:1rem;"></i> 詳細資料
        </button>
        <button type="button" data-action="rename" class="btn btn-light btn-sm w-100 text-start mb-1">
            <i data-lucide="edit-3" style="width:1rem;height:1rem;"></i> 重新命名
        </button>
        <button type="button" data-action="download" class="btn btn-light btn-sm w-100 text-start mb-1">
            <i data-lucide="download" style="width:1rem;height:1rem;"></i> 下載
        </button>
        <hr class="my-2">
        <button type="button" data-action="delete" class="btn btn-light btn-sm w-100 text-start text-danger">
            <i data-lucide="trash-2" style="width:1rem;height:1rem;"></i> 刪除
        </button>
    `);
    $('body').append($menu);
    itemMenuEl = $menu[0];
    if (typeof lucide !== 'undefined' && lucide.createIcons) lucide.createIcons();
    return itemMenuEl;
}

function hideItemMenu() {
    if (!itemMenuEl) return;
    $(itemMenuEl).addClass('d-none');
    itemMenuAnchor = null;
    if (itemMenuOnDocClick) $(document).off('click', itemMenuOnDocClick);
    if (itemMenuOnScroll) $(window).off('scroll', itemMenuOnScroll);
    if (itemMenuOnResize) $(window).off('resize', itemMenuOnResize);
    itemMenuOnDocClick = null;
    itemMenuOnScroll = null;
    itemMenuOnResize = null;
}

function openItemMenu(e, kind, name, path) {
    e.preventDefault();
    e.stopPropagation();
    const menu = ensureItemMenu();
    const $menu = $(menu);

    // 若點同一顆按鈕 -> 切換顯示/隱藏
    if (!$menu.hasClass('d-none') && itemMenuAnchor === e.currentTarget) {
        hideItemMenu();
        return;
    }

    // 定位在按鈕下方（fixed 以 viewport 為參考）
    const rect = e.currentTarget.getBoundingClientRect();
    $menu.css('visibility', 'hidden');
    $menu.removeClass('d-none');
    
    // 計算選單尺寸
    const menuWidth = $menu.outerWidth() || 176;
    const menuHeight = $menu.outerHeight() || 200; // 預估高度
    const padding = 8;
    
    // 水平定位
    const viewportRight = window.innerWidth - padding;
    let left = rect.right - menuWidth; // 右對齊按鈕
    if (left + menuWidth > viewportRight) left = viewportRight - menuWidth;
    if (left < padding) left = padding;
    $menu.css('left', `${left}px`);
    
    // 垂直定位 - 檢查是否會被底部截掉
    const viewportBottom = window.innerHeight - padding;
    let top = rect.bottom + 4;
    
    // 如果選單會被底部截掉，則顯示在按鈕上方
    if (top + menuHeight > viewportBottom) {
        top = rect.top - menuHeight - 4;
        // 如果上方也沒有空間，則調整到可見範圍內
        if (top < padding) {
            top = padding;
        }
    }
    
    $menu.css('top', `${top}px`);
    $menu.css('visibility', 'visible');

    // 記住來源，綁定關閉事件（點外部 / 捲動 / 變更尺寸）
    itemMenuAnchor = e.currentTarget;
    itemMenuOnDocClick = (ev) => {
        if (!$menu[0].contains(ev.target) && ev.target !== itemMenuAnchor) hideItemMenu();
    };
    itemMenuOnScroll = () => hideItemMenu();
    itemMenuOnResize = () => hideItemMenu();
    setTimeout(() => {
        $(document).on('click', itemMenuOnDocClick);
        $(window).on('scroll', itemMenuOnScroll);
        $(window).on('resize', itemMenuOnResize);
    }, 0);

    // 設定動作
    $menu.find('[data-action="open"]').off('click').on('click', () => {
        if (kind === 'folder') {
            window.location.href = `/Backoffice/FileManager/FileManager?path=${encodeURIComponent(path === '/' ? '/' + name : path + '/' + name)}`;
        } else {
            openFile(name);
        }
        hideItemMenu();
    });
    $menu.find('[data-action="details"]').off('click').on('click', () => {
        loadAndShowFileDetail(path, name);
        hideItemMenu();
    });
    $menu.find('[data-action="rename"]').off('click').on('click', () => {
        showRenameModal(name, kind);
        hideItemMenu();
    });
    $menu.find('[data-action="download"]').off('click').on('click', () => {
        if (kind === 'folder') {
            window.location.href = `/Backoffice/FileManager/DownloadFolder?path=${encodeURIComponent(path)}&name=${encodeURIComponent(name)}`;
        } else {
            window.location.href = `/Backoffice/FileManager/Download?path=${encodeURIComponent(path)}&name=${encodeURIComponent(name)}`;
        }
        hideItemMenu();
    });
    $menu.find('[data-action="delete"]').off('click').on('click', () => {
        deleteItem(name, kind);
        hideItemMenu();
    });
}

// 載入並顯示檔案/資料夾詳細資料（部分視圖）
function loadAndShowFileDetail(path, name) {
    $.ajax({
        type: 'GET',
        url: '/Backoffice/FileManager/LoadDetail',
        data: { path, name },
        success: function (html) {
            $('#fileManagerModalHost').html(html);
            if (typeof lucide !== 'undefined') lucide.createIcons();
            showModal('itemDetailModal');
        },
        error: function () {
            alert('載入詳細資料失敗');
        }
    });
}


