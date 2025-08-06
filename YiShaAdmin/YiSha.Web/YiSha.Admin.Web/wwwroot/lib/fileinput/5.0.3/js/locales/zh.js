/*!
 * FileInput Chinese Translations
 *
 * This file must be loaded after 'fileinput.js'. Patterns in braces '{}', or
 * any HTML markup tags in the messages must not be converted or translated.
 *
 * @see http://github.com/kartik-v/bootstrap-fileinput
 * @author kangqf <kangqingfei@gmail.com>
 *
 * NOTE: this file must be saved in UTF-8 encoding.
 */
(function ($) {
    "use strict";

    $.fn.fileinputLocales['zh'] = {
        fileSingle: '文件',
        filePlural: '個文件',
        browseLabel: '選擇 &hellip;',
        removeLabel: '移除',
        removeTitle: '清除選中文件',
        cancelLabel: '取消',
        cancelTitle: '取消進行中的上傳',
        pauseLabel: 'Pause',
        pauseTitle: 'Pause ongoing upload',
        uploadLabel: '上傳',
        uploadTitle: '上傳選中文件',
        msgNo: '沒有',
        msgNoFilesSelected: '未選擇文件',
        msgPaused: 'Paused',
        msgCancelled: '取消',
        msgPlaceholder: '選擇 {files}...',
        msgZoomModalHeading: '詳细預覽',
        msgFileRequired: '必須選擇一個文件上傳.',
        msgSizeTooSmall: '文件 "{name}" (<b>{size} KB</b>) 必須大於限定大小 <b>{minSize} KB</b>.',
        msgSizeTooLarge: '文件 "{name}" (<b>{size} KB</b>) 超過了允許大小 <b>{maxSize} KB</b>.',
        msgFilesTooLess: '你必須選擇最少 <b>{n}</b> {files} 來上傳. ',
        msgFilesTooMany: '選擇的上傳文件個數 <b>({n})</b> 超出最大文件的限制個數 <b>{m}</b>.',
        msgFileNotFound: '文件 "{name}" 未找到!',
        msgFileSecured: '安全限制，為了防止讀取文件 "{name}".',
        msgFileNotReadable: '文件 "{name}" 不可讀.',
        msgFilePreviewAborted: '取消 "{name}" 的預覽.',
        msgFilePreviewError: '讀取 "{name}" 時出現了一個錯誤.',
        msgInvalidFileName: '文件名 "{name}" 包含非法字符.',
        msgInvalidFileType: '不正確的類型 "{name}". 只支持 "{types}" 類型的文件.',
        msgInvalidFileExtension: '不正確的文件扩展名 "{name}". 只支持 "{extensions}" 的文件扩展名.',
        msgFileTypes: {
            'image': 'image',
            'html': 'HTML',
            'text': 'text',
            'video': 'video',
            'audio': 'audio',
            'flash': 'flash',
            'pdf': 'PDF',
            'object': 'object'
        },
        msgUploadAborted: '該文件上傳被中止',
        msgUploadThreshold: '處理中...',
        msgUploadBegin: '正在初始化...',
        msgUploadEnd: '完成',
        msgUploadResume: 'Resuming upload...',
        msgUploadEmpty: '无效的文件上傳.',
        msgUploadError: 'Upload Error',
        msgDeleteError: 'Delete Error',
        msgProgressError: '上傳出錯',
        msgValidationError: '驗證錯誤',
        msgLoading: '加載第 {index} 文件 共 {files} &hellip;',
        msgProgress: '加載第 {index} 文件 共 {files} - {name} - {percent}% 完成.',
        msgSelected: '{n} {files} 選中',
        msgFoldersNotAllowed: '只支持拖拽文件! 跳過 {n} 拖拽的文件夹.',
        msgImageWidthSmall: '圖像文件的"{name}"的寬度必須是至少{size}像素.',
        msgImageHeightSmall: '圖像文件的"{name}"的高度必須至少為{size}像素.',
        msgImageWidthLarge: '圖像文件"{name}"的寬度不能超過{size}像素.',
        msgImageHeightLarge: '圖像文件"{name}"的高度不能超過{size}像素.',
        msgImageResizeError: '无法獲取的圖像尺寸調整。',
        msgImageResizeException: '調整圖像大小時發生錯誤。<pre>{errors}</pre>',
        msgAjaxError: '{operation} 發生錯誤. 請重试!',
        msgAjaxProgressError: '{operation} 失敗',
        msgDuplicateFile: 'File "{name}" of same size "{size} KB" has already been selected earlier. Skipping duplicate selection.',
        msgResumableUploadRetriesExceeded:  'Upload aborted beyond <b>{max}</b> retries for file <b>{file}</b>! Error Details: <pre>{error}</pre>',
        msgPendingTime: '{time} remaining',
        msgCalculatingTime: 'calculating time remaining',
        ajaxOperations: {
            deleteThumb: '删除文件',
            uploadThumb: '上傳文件',
            uploadBatch: '批量上傳',
            uploadExtra: '表單資料上傳'
        },
        dropZoneTitle: '拖拽文件到這里 &hellip;<br>支持多文件同時上傳',
        dropZoneClickTitle: '<br>(或點擊{files}按鈕選擇文件)',
        fileActionSettings: {
            removeTitle: '删除文件',
            uploadTitle: '上傳文件',
            downloadTitle: '下載文件',
            uploadRetryTitle: '重试',
            zoomTitle: '查看詳情',
            dragTitle: '移動 / 重置',
            indicatorNewTitle: '沒有上傳',
            indicatorSuccessTitle: '上傳',
            indicatorErrorTitle: '上傳錯誤',
            indicatorPausedTitle: 'Upload Paused',
            indicatorLoadingTitle:  '上傳 ...'
        },
        previewZoomButtonTitles: {
            prev: '預覽上一個文件',
            next: '預覽下一個文件',
            toggleheader: '縮放',
            fullscreen: '全螢幕',
            borderless: '无邊界模式',
            close: '關閉當前預覽'
        }
    };
})(window.jQuery);
