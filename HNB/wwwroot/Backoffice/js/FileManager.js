// 檔案總管 - 使用官方 Dropzone.js

let currentPath = '/';

// 初始化
$(function() {
    // 取得當前路徑（從頁面元素或 URL）
    const $pathEl = $('[data-current-path]');
    if ($pathEl.length) {
        currentPath = $pathEl.attr('data-current-path') || '/';
    } else {
        const urlParams = new URLSearchParams(window.location.search);
        currentPath = urlParams.get('path') || '/';
    }
    
    // 初始化 Dropzone
    initializeDropzone();
    
    console.log('檔案總管已載入，當前路徑：', currentPath);
});

// 初始化 Dropzone
function initializeDropzone() {
    // 禁用 Dropzone 自動發現
    Dropzone.autoDiscover = false;
    
    // 初始化 Dropzone
    $("#fileUploadDropZone").dropzone({
        url: "/Backoffice/FileManager/Upload",
        paramName: "files",
        maxFilesize: 100, // MB
        acceptedFiles: null, // 接受所有檔案類型
        addRemoveLinks: true,
        dictDefaultMessage: "拖曳檔案到這裡上傳",
        dictRemoveFile: "移除",
        dictCancelUpload: "取消上傳",
        dictUploadCanceled: "上傳已取消",
        dictInvalidFileType: "不支援的檔案類型",
        dictFileTooBig: "檔案太大 ({{filesize}}MB). 最大限制: {{maxFilesize}}MB.",
        dictResponseError: "伺服器回應錯誤 ({{statusCode}})",
        dictCancelUploadConfirmation: "確定要取消上傳嗎？",
        dictRemoveFileConfirmation: "確定要移除這個檔案嗎？",
        dictMaxFilesExceeded: "無法上傳更多檔案",
        
        // 額外參數
        params: {
            path: currentPath,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        
        // 上傳成功
        success: function(file, response) {
            console.log('上傳成功:', file.name);
        },
        
        // 上傳錯誤
        error: function(file, errorMessage) {
            console.error('上傳失敗:', file.name, errorMessage);
            alert('上傳失敗: ' + errorMessage);
        },
        
        // 所有檔案上傳完成
        queuecomplete: function() {
            console.log('所有檔案上傳完成');
            // 延遲重新載入頁面
            setTimeout(() => {
                location.reload();
            }, 1000);
        },
        
        // 檔案添加時
        addedfile: function(file) {
            console.log('添加檔案:', file.name);
        },
        
        // 檔案移除時
        removedfile: function(file) {
            console.log('移除檔案:', file.name);
        }
    });
}