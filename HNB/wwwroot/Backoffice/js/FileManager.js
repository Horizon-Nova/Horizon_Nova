// 檔案總管

Dropzone.autoDiscover = false;

let currentPath = '/';
let myDropzone = null;

$(function() {
    const urlParams = new URLSearchParams(window.location.search);
    currentPath = urlParams.get('path') || '/';
    
    initializeUpload();
});

function initializeUpload() {
    const $dropzone = $('#fileUploadDropZone');
    
    $dropzone.on('dragenter dragover', function(e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).addClass('border-primary bg-light');
    });
    
    $dropzone.on('dragleave', function(e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('border-primary bg-light');
    });
    
    $dropzone.on('drop', async function(e) {
        e.preventDefault();
        e.stopPropagation();
        $(this).removeClass('border-primary bg-light');
        
        const items = e.originalEvent.dataTransfer.items;
        const files = [];
        
        for (let i = 0; i < items.length; i++) {
            const item = items[i].webkitGetAsEntry();
            if (item) {
                await traverseFileTree(item, '', files);
            }
        }
        
        if (files.length > 0) {
            uploadFiles(files);
        }
    });
    
    $dropzone.on('click', function() {
        const input = document.createElement('input');
        input.type = 'file';
        input.multiple = true;
        
        input.onchange = function(e) {
            const fileList = Array.from(e.target.files);
            const files = fileList.map(f => ({
                file: f,
                path: f.name
            }));
            if (files.length > 0) {
                uploadFiles(files);
            }
        };
        
        input.click();
    });
}

async function traverseFileTree(item, path, files) {
    if (item.isFile) {
        return new Promise((resolve) => {
            item.file((file) => {
                files.push({ 
                    file: file, 
                    path: path + file.name 
                });
                resolve();
            });
        });
    } else if (item.isDirectory) {
        const dirReader = item.createReader();
        return new Promise((resolve) => {
            dirReader.readEntries(async (entries) => {
                for (const entry of entries) {
                    await traverseFileTree(entry, path + item.name + '/', files);
                }
                resolve();
            });
        });
    }
}

function uploadFiles(filesWithPaths) {
    if (!filesWithPaths || filesWithPaths.length === 0) return;
    
    const $items = $('#uploadItems');
    $items.empty();
    
    showModal('uploadProgress');
    
    filesWithPaths.forEach(({ file, path }) => {
        const fileItem = `
            <div class="upload-item">
                <div class="d-flex justify-content-between align-items-center mb-1">
                    <span class="small text-truncate" style="max-width: 350px;" title="${path}">${path}</span>
                    <span class="badge bg-secondary">上傳中</span>
                </div>
                <div class="progress" style="height: 4px;">
                    <div class="progress-bar progress-bar-striped progress-bar-animated" role="progressbar" style="width: 100%"></div>
                </div>
            </div>
        `;
        $items.append(fileItem);
    });
    
    const formData = new FormData();
    formData.append('virtualPath', currentPath);
    formData.append('__RequestVerificationToken', $('input[name="__RequestVerificationToken"]').val());
    
    filesWithPaths.forEach(({ file, path }) => {
        formData.append('files', file);
        formData.append('relativePaths', path);
    });
    
    $.ajax({
        type: 'POST',
        url: '/Backoffice/FileManager/SubmitUpload',
        data: formData,
        processData: false,
        contentType: false,
        success: (response) => {
            if (response.success) {
                $('.upload-item').each(function() {
                    $(this).find('.badge').removeClass('bg-secondary').addClass('bg-success').text('完成');
                    $(this).find('.progress-bar').removeClass('progress-bar-animated').addClass('bg-success');
                });
                
                setTimeout(() => {
                    closeModal('uploadProgress');
                    location.reload();
                }, 1500);
            } else {
                $('.upload-item').each(function() {
                    $(this).find('.badge').removeClass('bg-secondary').addClass('bg-danger').text('失敗');
                    $(this).find('.progress-bar').removeClass('progress-bar-animated').addClass('bg-danger');
                });
                alert(response.message || '上傳失敗');
            }
        },
        error: () => {
            $('.upload-item').each(function() {
                $(this).find('.badge').removeClass('bg-secondary').addClass('bg-danger').text('錯誤');
                $(this).find('.progress-bar').removeClass('progress-bar-animated').addClass('bg-danger');
            });
            alert('上傳發生錯誤');
            
            setTimeout(() => {
                closeModal('uploadProgress');
            }, 3000);
        }
    });
}

// 分享功能
$(document).on('click', '.remove-share', function(){
    const $badge = $(this).closest('.share-badge');
    const isPrimary = $badge.data('is-primary') === 'true' || $badge.data('is-primary') === true;
    
    if(isPrimary){
        alert('無法移除原擁有者');
        return;
    }
    
    const uid = $(this).data('user');
    $('#shareSelected .share-badge[data-user="'+uid+'"]').remove();
});

$(document).on('click', '#btnSaveShare', function(){
    const $btn = $(this);
    const isOwner = ($('#sidePanelRoot').data('is-owner')+"") === 'true';
    if(!isOwner){ return; }
    const primary = $btn.data('primary-owner');
    const typed = ($('#shareEditor').val()||'').split(',').map(s=>s.trim()).filter(Boolean);
    const badges = $('#shareSelected .share-badge').map(function(){ return $(this).data('user'); }).get();
    const others = Array.from(new Set([...badges, ...typed])).filter(u => u && u !== primary);
    const owners = [primary, ...others.filter(u => u !== primary)];
    const token = $('input[name="__RequestVerificationToken"]').val();
    const payload = {
        Path: $btn.data('current-path'),
        Name: $btn.data('item-name'),
        Owners: owners,
        __RequestVerificationToken: token
    };
    $.ajax({
        type: 'POST',
        url: '/Backoffice/FileManager/SubmitShare',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        headers: { 'RequestVerificationToken': token },
        success: (res)=>{ 
            if(res.success){ 
                alert('分享設定已更新'); 
                location.reload(); 
            } else { 
                alert(res.message||'更新失敗'); 
            } 
        },
        error: ()=> alert('系統發生錯誤')
    });
});

// 名稱儲存（重新命名）
$(document).on('click', '#btnSaveName', function(){
    const $btn = $(this);
    const isOwner = ($('#sidePanelRoot').data('is-owner')+"") === 'true';
    if(!isOwner){ return; }
    const type = $btn.data('type');
    const path = $btn.data('current-path');
    const oldName = $btn.data('old-name');
    const newName = $('#spNameInput').val();
    if(!newName || newName === oldName){ return; }
    const token = $('input[name="__RequestVerificationToken"]').val();
    const payload = { Path: path, OldName: oldName, NewName: newName, __RequestVerificationToken: token };
    $.ajax({
        type: 'POST',
        url: type === 'file' ? '/Backoffice/FileManager/SubmitRenameFile' : '/Backoffice/FileManager/SubmitRename',
        contentType: 'application/json',
        data: JSON.stringify(payload),
        headers: { 'RequestVerificationToken': token },
        success: (res)=>{ if(res.success){ location.href = location.pathname + '?path=' + encodeURIComponent(path); } else { alert(res.message||'重新命名失敗'); } },
        error: ()=> alert('系統發生錯誤')
    });
});

// 右側欄開啟
window.openItemPanel = function(name, type, path){
    const el = document.getElementById('itemSidePanel');
    if(!el){ return; }
    const panel = new bootstrap.Offcanvas(el);

    // 先顯示 loading
    const $body = $('#itemSidePanelBody');
    $body.html('<div class="text-center py-3"><div class="spinner-border text-primary" role="status"><span class="visually-hidden">載入中...</span></div></div>');

    // 載入內容
    $.ajax({
        url: '/Backoffice/FileManager/LoadDetail',
        method: 'GET',
        data: { name: name, currentPath: path, type: type },
        success: function(html){
            $body.html(html);
            window.lucide?.createIcons?.();
        },
        error: function(){
            $body.html('<div class="alert alert-danger m-3">載入失敗，請稍後再試。</div>');
        }
    });

    panel.show();
}