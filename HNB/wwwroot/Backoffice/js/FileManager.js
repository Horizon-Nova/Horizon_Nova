// 檔案總管 JavaScript

let currentPath = '/';
let renameTarget = { name: '', type: '', path: '' };

// 初始化
document.addEventListener('DOMContentLoaded', function() {
    // 初始化 Lucide Icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    // 取得當前路徑
    const urlParams = new URLSearchParams(window.location.search);
    currentPath = urlParams.get('path') || '/';
    
    // 初始化拖曳上傳
    initializeDragAndDrop();
    
    // 初始化檔案輸入
    initializeFileInput();
    
    console.log('檔案總管已載入，當前路徑：', currentPath);
});

// ========== 拖曳上傳初始化 ==========

function initializeDragAndDrop() {
    const dropZone = document.getElementById('dropZone');
    if (!dropZone) return;
    
    ['dragenter', 'dragover', 'dragleave', 'drop'].forEach(eventName => {
        dropZone.addEventListener(eventName, preventDefaults, false);
    });
    
    function preventDefaults(e) {
        e.preventDefault();
        e.stopPropagation();
    }
    
    dropZone.addEventListener('dragover', function() {
        dropZone.classList.add('border-blue-500', 'bg-blue-50');
    });
    
    dropZone.addEventListener('dragleave', function() {
        dropZone.classList.remove('border-blue-500', 'bg-blue-50');
    });
    
    dropZone.addEventListener('drop', async function(e) {
        dropZone.classList.remove('border-blue-500', 'bg-blue-50');
        
        // 使用 DataTransferItems API 來支援資料夾拖曳
        const items = e.dataTransfer.items;
        if (items && items.length > 0) {
            const filesWithPaths = await getAllFilesFromItems(items);
            handleFileUploadWithPaths(filesWithPaths);
        } else {
            // 降級處理：只支援檔案拖曳
            const files = Array.from(e.dataTransfer.files).map(f => ({ file: f, path: f.name }));
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
    const fileInput = document.getElementById('fileInput');
    if (!fileInput) return;
    
    fileInput.addEventListener('change', function(e) {
        handleFileUpload(Array.from(e.target.files));
        // 清空 input，允許重複上傳同一檔案
        e.target.value = '';
    });
}

// ========== 目錄樹折疊功能 ==========

function toggleTreeNode(event, element) {
    event.preventDefault();
    event.stopPropagation();
    
    const chevron = element;
    const isExpanded = chevron.getAttribute('data-lucide') === 'chevron-down';
    const treeNode = element.closest('a, div');
    const currentIndent = parseFloat(treeNode.style.paddingLeft || '0.75rem');
    
    // 切換 chevron 圖標
    chevron.setAttribute('data-lucide', isExpanded ? 'chevron-right' : 'chevron-down');
    lucide.createIcons();
    
    // 找到所有子節點並顯示/隱藏
    let nextNode = treeNode.nextElementSibling;
    while (nextNode) {
        const nextIndent = parseFloat(nextNode.style.paddingLeft || '0.75rem');
        
        // 如果縮排較小或相等，表示不是子節點
        if (nextIndent <= currentIndent) break;
        
        // 顯示/隱藏節點
        if (isExpanded) {
            nextNode.classList.add('hidden');
        } else {
            // 只顯示直接子節點，不顯示更深層的節點
            const isDirectChild = Math.abs(nextIndent - currentIndent - 1.0) < 0.1;
            if (isDirectChild) {
                nextNode.classList.remove('hidden');
            }
        }
        
        nextNode = nextNode.nextElementSibling;
    }
}

// ========== Modal 操作 ==========

function showModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.remove('hidden');
        modal.classList.add('flex');
        
        // 聚焦到輸入框
        const input = modal.querySelector('input[type="text"]');
        if (input) {
            setTimeout(() => input.focus(), 100);
        }
    }
}

function hideModal(modalId) {
    const modal = document.getElementById(modalId);
    if (modal) {
        modal.classList.add('hidden');
        modal.classList.remove('flex');
    }
}

// ========== 新增資料夾 ==========

function showCreateFolderModal() {
    const input = document.getElementById('folderNameInput');
    if (input) input.value = '';
    showModal('createFolderModal');
}

async function createFolder() {
    const name = document.getElementById('folderNameInput').value.trim();
    if (!name) {
        alert('請輸入資料夾名稱');
        return;
    }
    
    try {
        const response = await fetch('/Backoffice/FileManager/CreateFolder', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ path: currentPath, name })
        });
        
        const result = await response.json();
        
        if (result.success) {
            hideModal('createFolderModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        alert('操作失敗：' + error.message);
    }
}

// ========== 新增檔案 ==========

function showCreateFileModal() {
    const input = document.getElementById('fileNameInput');
    if (input) input.value = '';
    showModal('createFileModal');
}

async function createFile() {
    const name = document.getElementById('fileNameInput').value.trim();
    if (!name) {
        alert('請輸入檔案名稱');
        return;
    }
    
    try {
        const response = await fetch('/Backoffice/FileManager/CreateFile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ path: currentPath, name })
        });
        
        const result = await response.json();
        
        if (result.success) {
            hideModal('createFileModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        alert('操作失敗：' + error.message);
    }
}

// ========== 重新命名 ==========

function showRenameModal(name, type) {
    renameTarget = { name, type, path: currentPath };
    
    const title = document.getElementById('renameModalTitle');
    const input = document.getElementById('renameInput');
    
    if (title) title.textContent = type === 'folder' ? '重新命名資料夾' : '重新命名檔案';
    if (input) {
        input.value = name;
        setTimeout(() => {
            input.focus();
            input.select();
        }, 100);
    }
    
    showModal('renameModal');
}

async function confirmRename() {
    const newName = document.getElementById('renameInput').value.trim();
    if (!newName) {
        alert('請輸入新名稱');
        return;
    }
    
    if (newName === renameTarget.name) {
        hideModal('renameModal');
        return;
    }
    
    try {
        const endpoint = renameTarget.type === 'folder' ? 'RenameFolder' : 'RenameFile';
        const response = await fetch(`/Backoffice/FileManager/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                path: renameTarget.path, 
                oldName: renameTarget.name, 
                newName 
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            hideModal('renameModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        alert('操作失敗：' + error.message);
    }
}

// ========== 刪除 ==========

let deleteTarget = { name: '', type: '', path: '' };

function deleteItem(name, type) {
    deleteTarget = { name, type, path: currentPath };
    
    const itemType = type === 'folder' ? '資料夾' : '檔案';
    const title = document.getElementById('deleteModalTitle');
    const targetName = document.getElementById('deleteTargetName');
    const input = document.getElementById('deleteConfirmInput');
    const btn = document.getElementById('deleteConfirmBtn');
    
    if (title) title.textContent = `刪除${itemType}`;
    if (targetName) targetName.textContent = name;
    if (input) {
        input.value = '';
        input.placeholder = `輸入「${name}」以確認刪除`;
    }
    if (btn) btn.disabled = true;
    
    showModal('deleteModal');
    
    // 重新初始化 Lucide Icons（因為有新的 icon）
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function validateDeleteInput() {
    const input = document.getElementById('deleteConfirmInput');
    const btn = document.getElementById('deleteConfirmBtn');
    
    if (!input || !btn) return;
    
    // 檢查輸入是否與目標名稱完全匹配
    const isMatch = input.value === deleteTarget.name;
    btn.disabled = !isMatch;
}

async function confirmDelete() {
    if (!deleteTarget.name) return;
    
    // 記錄刪除操作（用於除錯）
    console.log('[刪除操作]', {
        type: deleteTarget.type,
        path: deleteTarget.path,
        name: deleteTarget.name
    });
    
    try {
        const endpoint = deleteTarget.type === 'folder' ? 'DeleteFolder' : 'DeleteFile';
        const payload = { 
            path: deleteTarget.path, 
            name: deleteTarget.name 
        };
        
        console.log('[發送請求]', endpoint, payload);
        
        const response = await fetch(`/Backoffice/FileManager/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify(payload)
        });
        
        const result = await response.json();
        console.log('[刪除結果]', result);
        
        if (result.success) {
            hideModal('deleteModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        console.error('[刪除錯誤]', error);
        alert('操作失敗：' + error.message);
    }
}

// ========== 檔案上傳 ==========

// 處理帶路徑信息的檔案上傳（支援資料夾結構）
async function handleFileUploadWithPaths(filesWithPaths) {
    if (!filesWithPaths || filesWithPaths.length === 0) return;
    
    const progress = document.getElementById('uploadProgress');
    const items = document.getElementById('uploadItems');
    const speedEl = document.getElementById('uploadSpeed');
    const etaEl = document.getElementById('uploadEta');
    
    // 顯示上傳進度視窗
    progress.classList.remove('hidden');
    items.innerHTML = '';
    
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
        
        speedEl.textContent = formatSize(Math.round(speed)) + '/s';
        etaEl.textContent = '剩餘 ' + formatTime(eta);
    }
    
    // 建立所有上傳項目的 UI
    filesWithPaths.forEach((item, i) => {
        const itemId = 'upload-item-' + i;
        items.innerHTML += `
            <div id="${itemId}">
                <div class="flex items-center justify-between mb-1.5">
                    <span class="text-sm text-slate-900 truncate" title="${item.path}">${item.path}</span>
                    <span class="text-xs font-medium text-slate-600">0%</span>
                </div>
                <div class="h-1.5 bg-slate-100 rounded-full overflow-hidden">
                    <div class="h-full bg-blue-600 rounded-full" style="width: 0%"></div>
                </div>
            </div>
        `;
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
            const percentEl = document.querySelector(`#${itemId} .text-xs`);
            if (percentEl) percentEl.textContent = '完成';
            const barEl = document.querySelector(`#${itemId} .bg-blue-600`);
            if (barEl) barEl.classList.replace('bg-blue-600', 'bg-green-600');
            
            return { success: true, path };
        } catch (error) {
            console.error(`上傳失敗 [${path}]:`, error);
            const percentEl = document.querySelector(`#${itemId} .text-xs`);
            if (percentEl) {
                percentEl.textContent = '失敗';
                percentEl.classList.add('text-red-600');
            }
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
        progress.classList.add('hidden');
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
        
        xhr.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable) {
                const percent = Math.round((e.loaded / e.total) * 100);
                
                // 更新此檔案的進度
                const percentEl = document.querySelector(`#${itemId} .text-xs`);
                const barEl = document.querySelector(`#${itemId} .bg-blue-600, #${itemId} .bg-green-600`);
                
                if (percentEl) percentEl.textContent = percent + '%';
                if (barEl) barEl.style.width = percent + '%';
                
                // 回調整體進度
                if (onProgress) onProgress(e.loaded);
            }
        });
        
        xhr.addEventListener('load', () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                resolve(xhr.response);
            } else {
                reject(new Error('上傳失敗'));
            }
        });
        
        xhr.addEventListener('error', () => reject(new Error('網路錯誤')));
        
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
    const modal = document.getElementById('fileEditorModal');
    const editorContent = document.getElementById('fileEditorContent');
    const previewFrame = document.getElementById('filePreviewFrame');
    const loading = document.getElementById('editorLoading');
    const cannotPreview = document.getElementById('cannotPreview');
    const fileNameEl = document.getElementById('editorFileName');
    const readOnlyBadge = document.getElementById('editorReadOnlyBadge');
    const saveBtn = document.getElementById('saveFileBtn');
    const infoEl = document.getElementById('editorInfo');
    const encodingEl = document.getElementById('editorEncoding');
    
    // 重置狀態
    modal.classList.remove('hidden');
    modal.classList.add('flex');
    editorContent.classList.add('hidden');
    previewFrame.classList.add('hidden');
    cannotPreview.classList.add('hidden');
    loading.classList.remove('hidden');
    fileNameEl.textContent = fileName;
    
    // 初始化 Lucide Icons
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
    
    if (fileType === 'editable') {
        // 可編輯文字檔案
        try {
            const response = await fetch(`/Backoffice/FileManager/ReadTextFile?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`);
            const result = await response.json();
            
            if (result.success) {
                currentEditingFile.originalContent = result.content;
                currentEditingFile.encoding = result.encoding;
                
                editorContent.value = result.content;
                editorContent.classList.remove('hidden');
                editorContent.readOnly = false;
                loading.classList.add('hidden');
                
                readOnlyBadge.classList.add('hidden');
                saveBtn.classList.remove('hidden');
                
                infoEl.textContent = `${result.content.split('\n').length} 行`;
                encodingEl.textContent = `編碼: ${result.encoding}`;
                
                // 重新初始化 Lucide Icons
                if (typeof lucide !== 'undefined') {
                    lucide.createIcons();
                }
            } else {
                alert('無法讀取檔案：' + result.message);
                closeFileEditor();
            }
        } catch (error) {
            alert('讀取檔案失敗：' + error.message);
            closeFileEditor();
        }
    } else if (fileType === 'pdf') {
        // PDF 文件 - 直接在瀏覽器預覽
        const previewUrl = `/Backoffice/FileManager/Preview?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        
        previewFrame.src = previewUrl;
        previewFrame.classList.remove('hidden');
        loading.classList.add('hidden');
        
        readOnlyBadge.classList.remove('hidden');
        saveBtn.classList.add('hidden');
        
        infoEl.textContent = `PDF 預覽`;
        encodingEl.textContent = '';
        
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
        
        previewFrame.src = officeViewerUrl;
        previewFrame.classList.remove('hidden');
        loading.classList.add('hidden');
        
        readOnlyBadge.classList.remove('hidden');
        saveBtn.classList.add('hidden');
        
        infoEl.textContent = `Office 線上預覽 (需要網際網路連線)`;
        encodingEl.textContent = '';
        
        // 設定超時檢測（如果 10 秒後還在載入，可能是失敗了）
        const timeout = setTimeout(() => {
            // 顯示無法預覽提示
            previewFrame.classList.add('hidden');
            cannotPreview.classList.remove('hidden');
            
            const cannotPreviewMsg = document.getElementById('cannotPreviewMessage');
            const downloadLink = document.getElementById('downloadFileLink');
            
            cannotPreviewMsg.textContent = 'Office 線上預覽服務無法載入，可能是網路問題或檔案過大。請下載後使用 Office 應用程式查看。';
            downloadLink.href = downloadUrl;
            
            infoEl.textContent = `預覽失敗`;
            
            // 重新初始化 Lucide Icons
            if (typeof lucide !== 'undefined') {
                lucide.createIcons();
            }
        }, 10000);
        
        // 如果成功載入，清除超時
        previewFrame.onload = function() {
            clearTimeout(timeout);
        };
        
        // 重新初始化 Lucide Icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    } else if (fileType === 'image' || fileType === 'video' || fileType === 'audio') {
        // 圖片、影片、音訊 - 使用預覽
        const previewUrl = `/Backoffice/FileManager/Preview?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        
        previewFrame.src = previewUrl;
        previewFrame.classList.remove('hidden');
        loading.classList.add('hidden');
        
        readOnlyBadge.classList.remove('hidden');
        saveBtn.classList.add('hidden');
        
        infoEl.textContent = `預覽模式 (${fileType})`;
        encodingEl.textContent = '';
        
        // 重新初始化 Lucide Icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    } else {
        // 其他檔案類型 - 顯示無法預覽提示
        loading.classList.add('hidden');
        cannotPreview.classList.remove('hidden');
        
        const cannotPreviewMsg = document.getElementById('cannotPreviewMessage');
        const downloadLink = document.getElementById('downloadFileLink');
        
        cannotPreviewMsg.textContent = '此檔案類型不支援線上預覽，請下載後查看';
        downloadLink.href = `/Backoffice/FileManager/Download?path=${encodeURIComponent(currentPath)}&name=${encodeURIComponent(fileName)}`;
        
        readOnlyBadge.classList.remove('hidden');
        saveBtn.classList.add('hidden');
        
        infoEl.textContent = `不支援預覽`;
        encodingEl.textContent = '';
        
        // 重新初始化 Lucide Icons
        if (typeof lucide !== 'undefined') {
            lucide.createIcons();
        }
    }
}

// 儲存檔案
async function saveFile() {
    const editorContent = document.getElementById('fileEditorContent');
    const newContent = editorContent.value;
    
    // 檢查是否有變更
    if (newContent === currentEditingFile.originalContent) {
        alert('檔案內容無變更');
        return;
    }
    
    try {
        const response = await fetch('/Backoffice/FileManager/SaveTextFile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                path: currentEditingFile.path,
                name: currentEditingFile.name,
                content: newContent,
                encoding: currentEditingFile.encoding
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            currentEditingFile.originalContent = newContent;
            alert('檔案已儲存');
        } else {
            alert('儲存失敗：' + result.message);
        }
    } catch (error) {
        alert('儲存失敗：' + error.message);
    }
}

// 關閉檔案編輯器
function closeFileEditor() {
    const modal = document.getElementById('fileEditorModal');
    const editorContent = document.getElementById('fileEditorContent');
    const previewFrame = document.getElementById('filePreviewFrame');
    
    // 檢查是否有未儲存的變更
    if (editorContent.value !== currentEditingFile.originalContent) {
        if (!confirm('有未儲存的變更，確定要關閉嗎？')) {
            return;
        }
    }
    
    modal.classList.add('hidden');
    modal.classList.remove('flex');
    editorContent.value = '';
    previewFrame.src = '';
    
    currentEditingFile = { name: '', path: '', encoding: 'utf-8', originalContent: '' };
}

// ========== 鍵盤快捷鍵 ==========

document.addEventListener('keydown', function(e) {
    // ESC 鍵關閉所有 Modal
    if (e.key === 'Escape') {
        hideModal('createFolderModal');
        hideModal('createFileModal');
        hideModal('renameModal');
        hideModal('deleteModal');
        
        // 關閉檔案編輯器
        const editorModal = document.getElementById('fileEditorModal');
        if (editorModal && !editorModal.classList.contains('hidden')) {
            closeFileEditor();
        }
    }
    
    // Ctrl+S 或 Cmd+S 儲存檔案
    if ((e.ctrlKey || e.metaKey) && e.key === 's') {
        const editorModal = document.getElementById('fileEditorModal');
        const saveBtn = document.getElementById('saveFileBtn');
        if (editorModal && !editorModal.classList.contains('hidden') && 
            saveBtn && !saveBtn.classList.contains('hidden')) {
            e.preventDefault();
            saveFile();
        }
    }
    
    // Enter 鍵確認操作
    if (e.key === 'Enter') {
        // 如果 Modal 開啟，執行對應操作
        if (!document.getElementById('createFolderModal').classList.contains('hidden')) {
            createFolder();
        } else if (!document.getElementById('createFileModal').classList.contains('hidden')) {
            createFile();
        } else if (!document.getElementById('renameModal').classList.contains('hidden')) {
            confirmRename();
        } else if (!document.getElementById('deleteModal').classList.contains('hidden')) {
            const btn = document.getElementById('deleteConfirmBtn');
            if (btn && !btn.disabled) {
                confirmDelete();
            }
        }
    }
});
