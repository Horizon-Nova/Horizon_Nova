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
        filePlural: '个文件',
        browseLabel: '選擇 &hellip;',
        removeLabel: '移除',
        removeTitle: '清除選中文件',
        cancelLabel: '取消',
        cancelTitle: '取消进行中的上传',
        pauseLabel: 'Pause',
        pauseTitle: 'Pause ongoing upload',
        uploadLabel: '上传',
        uploadTitle: '上传選中文件',
        msgNo: '沒有',
        msgNoFilesSelected: '未選擇文件',
        msgPaused: 'Paused',
        msgCancelled: '取消',
        msgPlaceholder: '選擇 {files}...',
        msgZoomModalHeading: '详细预览',
        msgFileRequired: '必须選擇一个文件上传.',
        msgSizeTooSmall: '文件 "{name}" (<b>{size} KB</b>) 必须大于限定大小 <b>{minSize} KB</b>.',
        msgSizeTooLarge: '文件 "{name}" (<b>{size} KB</b>) 超过了允許大小 <b>{maxSize} KB</b>.',
        msgFilesTooLess: '你必须選擇最少 <b>{n}</b> {files} 来上传. ',
        msgFilesTooMany: '選擇的上传文件个數 <b>({n})</b> 超出最大文件的限制个數 <b>{m}</b>.',
        msgFileNotFound: '文件 "{name}" 未找到!',
        msgFileSecured: '安全限制，為了防止读取文件 "{name}".',
        msgFileNotReadable: '文件 "{name}" 不可读.',
        msgFilePreviewAborted: '取消 "{name}" 的预览.',
        msgFilePreviewError: '读取 "{name}" 时出现了一个錯誤.',
        msgInvalidFileName: '文件名 "{name}" 包含非法字符.',
        msgInvalidFileType: '不正确的类型 "{name}". 只支持 "{types}" 类型的文件.',
        msgInvalidFileExtension: '不正确的文件扩展名 "{name}". 只支持 "{extensions}" 的文件扩展名.',
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
        msgUploadAborted: '该文件上传被中止',
        msgUploadThreshold: '處理中...',
        msgUploadBegin: '正在初始化...',
        msgUploadEnd: '完成',
        msgUploadResume: 'Resuming upload...',
        msgUploadEmpty: '无效的文件上传.',
        msgUploadError: 'Upload Error',
        msgDeleteError: 'Delete Error',
        msgProgressError: '上传出錯',
        msgValidationError: '验证錯誤',
        msgLoading: '加載第 {index} 文件 共 {files} &hellip;',
        msgProgress: '加載第 {index} 文件 共 {files} - {name} - {percent}% 完成.',
        msgSelected: '{n} {files} 選中',
        msgFoldersNotAllowed: '只支持拖拽文件! 跳过 {n} 拖拽的文件夹.',
        msgImageWidthSmall: '圖像文件的"{name}"的寬度必须是至少{size}像素.',
        msgImageHeightSmall: '圖像文件的"{name}"的高度必须至少為{size}像素.',
        msgImageWidthLarge: '圖像文件"{name}"的寬度不能超过{size}像素.',
        msgImageHeightLarge: '圖像文件"{name}"的高度不能超过{size}像素.',
        msgImageResizeError: '无法獲取的圖像尺寸调整。',
        msgImageResizeException: '调整圖像大小时发生錯誤。<pre>{errors}</pre>',
        msgAjaxError: '{operation} 发生錯誤. 請重试!',
        msgAjaxProgressError: '{operation} 失败',
        msgDuplicateFile: 'File "{name}" of same size "{size} KB" has already been selected earlier. Skipping duplicate selection.',
        msgResumableUploadRetriesExceeded:  'Upload aborted beyond <b>{max}</b> retries for file <b>{file}</b>! Error Details: <pre>{error}</pre>',
        msgPendingTime: '{time} remaining',
        msgCalculatingTime: 'calculating time remaining',
        ajaxOperations: {
            deleteThumb: '删除文件',
            uploadThumb: '上传文件',
            uploadBatch: '批量上传',
            uploadExtra: '表單資料上传'
        },
        dropZoneTitle: '拖拽文件到这里 &hellip;<br>支持多文件同时上传',
        dropZoneClickTitle: '<br>(或點擊{files}按鈕選擇文件)',
        fileActionSettings: {
            removeTitle: '删除文件',
            uploadTitle: '上传文件',
            downloadTitle: '下载文件',
            uploadRetryTitle: '重试',
            zoomTitle: '查看详情',
            dragTitle: '移动 / 重置',
            indicatorNewTitle: '沒有上传',
            indicatorSuccessTitle: '上传',
            indicatorErrorTitle: '上传錯誤',
            indicatorPausedTitle: 'Upload Paused',
            indicatorLoadingTitle:  '上传 ...'
        },
        previewZoomButtonTitles: {
            prev: '预览上一个文件',
            next: '预览下一个文件',
            toggleheader: '缩放',
            fullscreen: '全螢幕',
            borderless: '无边界模式',
            close: '關閉當前预览'
        }
    };
})(window.jQuery);
