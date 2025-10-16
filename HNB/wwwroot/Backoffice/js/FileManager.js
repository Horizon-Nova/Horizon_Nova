// 檔案總管核心功能
// 依賴: Modal.js (showModal/closeModal)

let currentParentCode = null;
let renameTarget = { code: '', name: '', type: '' };
let deleteTarget = { code: '', name: '', type: '' };
let currentEditingFile = { code: '', name: '', encoding: 'utf-8', originalContent: '' };

document.addEventListener('DOMContentLoaded', function() {
    const urlParams = new URLSearchParams(window.location.search);
    currentParentCode = urlParams.get('parentCode') || null;
    
    initializeDragAndDrop();
    initializeFileInput();
    
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
});

// ========== 目錄樹折疊 ==========

function toggleTreeFolder(event, code) {
    event.preventDefault();
    event.stopPropagation();
    
    const treeFolder = document.querySelector(`.tree-folder[data-code="${code}"]`);
    if (!treeFolder) return;
    
    const children = treeFolder.querySelector('.tree-children');
    const chevron = treeFolder.querySelector('.tree-chevron');
    
    if (children && chevron) {
        children.classList.toggle('hidden');
        chevron.classList.toggle('rotate-90');
    }
}

// ========== 拖曳上傳 ==========

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
        
        const items = e.dataTransfer.items;
        if (items && items.length > 0) {
            const filesWithPaths = await getAllFilesFromItems(items);
            handleFileUploadWithPaths(filesWithPaths);
        } else {
            const files = Array.from(e.dataTransfer.files).map(f => ({ file: f, path: f.name }));
            handleFileUploadWithPaths(files);
        }
    });
}

async function getAllFilesFromItems(items) {
    const filesWithPaths = [];
    
    for (let i = 0; i < items.length; i++) {
        const item = items[i].webkitGetAsEntry();
        if (item) {
            await traverseFileTree(item, '', filesWithPaths);
        }
    }
    
    return filesWithPaths;
}

async function traverseFileTree(item, path, filesWithPaths) {
    if (item.isFile) {
        const file = await new Promise((resolve, reject) => {
            item.file(resolve, reject);
        });
        filesWithPaths.push({
            file: file,
            path: path + file.name
        });
    } else if (item.isDirectory) {
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
        const filesWithPaths = Array.from(e.target.files).map(f => ({ file: f, path: f.name }));
        handleFileUploadWithPaths(filesWithPaths);
        e.target.value = '';
    });
}

async function handleFileUploadWithPaths(filesWithPaths) {
    if (!filesWithPaths || filesWithPaths.length === 0) return;
    
    const progress = document.getElementById('uploadProgress');
    const items = document.getElementById('uploadItems');
    const speedEl = document.getElementById('uploadSpeed');
    const etaEl = document.getElementById('uploadEta');
    
    progress.classList.remove('hidden');
    items.innerHTML = '';
    
    const startTime = Date.now();
    const totalBytes = filesWithPaths.reduce((sum, f) => sum + f.file.size, 0);
    const currentLoaded = new Array(filesWithPaths.length).fill(0);
    
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
    
    const uploadPromises = filesWithPaths.map(async ({ file, path }, i) => {
        const itemId = 'upload-item-' + i;
        const formData = new FormData();
        formData.append('path', currentParentCode || '');
        formData.append('files', file);
        formData.append('keys', path);
        
        try {
            await uploadFileWithProgress(file, formData, itemId, (loaded) => {
                updateOverallProgress(i, loaded);
            });
            
            const percentEl = document.querySelector(`#${itemId} .text-xs`);
            if (percentEl) percentEl.textContent = '完成';
            const barEl = document.querySelector(`#${itemId} .bg-blue-600`);
            if (barEl) barEl.classList.replace('bg-blue-600', 'bg-green-600');
            
            return { success: true, path };
        } catch (error) {
            const percentEl = document.querySelector(`#${itemId} .text-xs`);
            if (percentEl) {
                percentEl.textContent = '失敗';
                percentEl.classList.add('text-red-600');
            }
            return { success: false, path, error };
        }
    });
    
    await Promise.all(uploadPromises);
    
    setTimeout(() => {
        progress.classList.add('hidden');
        location.reload();
    }, 2000);
}

function uploadFileWithProgress(file, formData, itemId, onProgress) {
    return new Promise((resolve, reject) => {
        const xhr = new XMLHttpRequest();
        
        xhr.upload.addEventListener('progress', (e) => {
            if (e.lengthComputable) {
                const percent = Math.round((e.loaded / e.total) * 100);
                
                const percentEl = document.querySelector(`#${itemId} .text-xs`);
                const barEl = document.querySelector(`#${itemId} .bg-blue-600, #${itemId} .bg-green-600`);
                
                if (percentEl) percentEl.textContent = percent + '%';
                if (barEl) barEl.style.width = percent + '%';
                
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

// ========== 建立資料夾 ==========

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
            body: JSON.stringify({ path: currentParentCode || '', name })
        });
        
        const result = await response.json();
        
        if (result.success) {
            closeModal('createFolderModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        alert('操作失敗：' + error.message);
    }
}

// ========== 建立檔案 ==========

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
            body: JSON.stringify({ path: currentParentCode || '', name })
        });
        
        const result = await response.json();
        
        if (result.success) {
            closeModal('createFileModal');
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
    renameTarget = { name, type };
    
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
        closeModal('renameModal');
        return;
    }
    
    try {
        const endpoint = renameTarget.type === 'folder' ? 'RenameFolder' : 'RenameFile';
        const response = await fetch(`/Backoffice/FileManager/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                path: currentParentCode || '', 
                oldName: renameTarget.name, 
                newName 
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            closeModal('renameModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        alert('操作失敗：' + error.message);
    }
}

// ========== 刪除 ==========

function deleteItem(name, type) {
    deleteTarget = { name, type };
    
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
}

function validateDeleteInput() {
    const input = document.getElementById('deleteConfirmInput');
    const btn = document.getElementById('deleteConfirmBtn');
    
    if (!input || !btn) return;
    
    btn.disabled = input.value !== deleteTarget.name;
}

async function confirmDelete() {
    if (!deleteTarget.name) return;
    
    try {
        const endpoint = deleteTarget.type === 'folder' ? 'DeleteFolder' : 'DeleteFile';
        const response = await fetch(`/Backoffice/FileManager/${endpoint}`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ 
                path: currentParentCode || '', 
                name: deleteTarget.name 
            })
        });
        
        const result = await response.json();
        
        if (result.success) {
            closeModal('deleteModal');
            location.reload();
        } else {
            alert(result.message);
        }
    } catch (error) {
        alert('操作失敗：' + error.message);
    }
}

// ========== 打開檔案 ==========

async function openFile(code, fileName) {
    currentEditingFile = { code, name: fileName, encoding: 'utf-8', originalContent: '' };
    
    const fileType = getFileType(fileName);
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
    
    showModal('fileEditorModal');
    
    editorContent.classList.add('hidden');
    previewFrame.classList.add('hidden');
    cannotPreview.classList.add('hidden');
    loading.classList.remove('hidden');
    fileNameEl.textContent = fileName;
    
    if (fileType === 'editable') {
        // 可編輯文字檔案
        try {
            const response = await fetch(`/Backoffice/FileManager/ReadTextFile?path=${encodeURIComponent(currentParentCode || '')}&name=${encodeURIComponent(fileName)}`);
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
            } else {
                alert('無法讀取檔案：' + result.message);
                closeModal('fileEditorModal');
            }
        } catch (error) {
            alert('讀取檔案失敗：' + error.message);
            closeModal('fileEditorModal');
        }
    } else {
        // 其他類型 - 使用 URL 直接預覽或下載
        const downloadUrl = `/Backoffice/FileManager/Download?code=${encodeURIComponent(code)}`;
        
        if (fileType === 'pdf' || fileType === 'image' || fileType === 'video' || fileType === 'audio') {
            previewFrame.src = downloadUrl;
            previewFrame.classList.remove('hidden');
            loading.classList.add('hidden');
            
            readOnlyBadge.classList.remove('hidden');
            saveBtn.classList.add('hidden');
            
            infoEl.textContent = `預覽模式 (${fileType})`;
            encodingEl.textContent = '';
        } else {
            // 不支援預覽
            loading.classList.add('hidden');
            cannotPreview.classList.remove('hidden');
            
            const cannotPreviewMsg = document.getElementById('cannotPreviewMessage');
            const downloadLink = document.getElementById('downloadFileLink');
            
            cannotPreviewMsg.textContent = '此檔案類型不支援線上預覽，請下載後查看';
            downloadLink.href = downloadUrl;
            
            readOnlyBadge.classList.remove('hidden');
            saveBtn.classList.add('hidden');
            
            infoEl.textContent = `不支援預覽`;
            encodingEl.textContent = '';
        }
    }
    
    if (typeof lucide !== 'undefined') {
        lucide.createIcons();
    }
}

function getFileType(fileName) {
    const ext = fileName.toLowerCase().split('.').pop();
    
    if (ext === 'pdf') return 'pdf';
    
    const officeExts = ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'];
    if (officeExts.includes(ext)) return 'office';
    
    const editableExts = ['txt', 'js', 'css', 'html', 'json', 'xml', 'md', 'yml', 'yaml', 
                          'cs', 'java', 'py', 'php', 'rb', 'go', 'rs', 'ts', 'tsx', 'jsx',
                          'sql', 'sh', 'bat', 'ps1', 'ini', 'conf', 'log', 'csv'];
    if (editableExts.includes(ext)) return 'editable';
    
    const imageExts = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'svg', 'webp'];
    if (imageExts.includes(ext)) return 'image';
    
    const videoExts = ['mp4', 'webm', 'ogg', 'mov', 'avi'];
    if (videoExts.includes(ext)) return 'video';
    
    const audioExts = ['mp3', 'wav', 'ogg', 'aac', 'flac'];
    if (audioExts.includes(ext)) return 'audio';
    
    return 'other';
}

async function saveFile() {
    const editorContent = document.getElementById('fileEditorContent');
    const newContent = editorContent.value;
    
    if (newContent === currentEditingFile.originalContent) {
        alert('檔案內容無變更');
        return;
    }
    
    try {
        const response = await fetch('/Backoffice/FileManager/SaveTextFile', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({
                path: currentParentCode || '',
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

function closeFileEditor() {
    const modal = document.getElementById('fileEditorModal');
    const editorContent = document.getElementById('fileEditorContent');
    const previewFrame = document.getElementById('filePreviewFrame');
    
    if (editorContent.value !== currentEditingFile.originalContent) {
        if (!confirm('有未儲存的變更，確定要關閉嗎？')) {
            return;
        }
    }
    
    closeModal('fileEditorModal');
    editorContent.value = '';
    previewFrame.src = '';
    
    currentEditingFile = { code: '', name: '', encoding: 'utf-8', originalContent: '' };
}

// ========== 鍵盤快捷鍵 ==========

document.addEventListener('keydown', function(e) {
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
        if (!document.getElementById('createFolderModal')?.classList.contains('hidden')) {
            createFolder();
        } else if (!document.getElementById('createFileModal')?.classList.contains('hidden')) {
            createFile();
        } else if (!document.getElementById('renameModal')?.classList.contains('hidden')) {
            confirmRename();
        } else if (!document.getElementById('deleteModal')?.classList.contains('hidden')) {
            const btn = document.getElementById('deleteConfirmBtn');
            if (btn && !btn.disabled) {
                confirmDelete();
            }
        }
    }
});
