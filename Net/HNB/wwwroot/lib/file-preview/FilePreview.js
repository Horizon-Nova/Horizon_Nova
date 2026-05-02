/**
 * File Preview Reader
 * 支援多種檔案類型的預覽和編輯功能
 * 使用方式：在元素上添加 class="file-preview" 和 data-file-url 屬性
 */
class FilePreview {
    constructor() {
        this.container = null;
        this.currentFile = null;
        this.originalContent = null;
        this.isChanged = false;
        this.encoding = 'UTF-8';
        this.timeoutId = null;
        
        this.init();
    }

    init() {
        this.createContainer();
        this.bindEvents();
        this.bindKeyboardShortcuts();
    }

    createContainer() {
        // 創建主容器
        this.container = document.createElement('div');
        this.container.className = 'file-preview-container';
        this.container.innerHTML = `
            <div class="file-preview-modal">
                <div class="file-preview-header">
                    <div class="file-preview-title">
                        <span class="file-icon"></span>
                        <span class="file-name"></span>
                        <span class="status-indicator saved"></span>
                    </div>
                    <div class="file-preview-actions">
                        <button class="file-preview-btn save-btn" disabled>
                            <span>💾</span> 儲存
                        </button>
                        <button class="file-preview-btn download-btn">
                            <span>⬇️</span> 下載
                        </button>
                        <button class="close-btn">&times;</button>
                    </div>
                </div>
                <div class="file-preview-content">
                    <div class="file-preview-body"></div>
                </div>
            </div>
        `;

        document.body.appendChild(this.container);
    }

    bindEvents() {
        // 綁定點擊事件到所有 file-preview 元素
        document.addEventListener('click', (e) => {
            const element = e.target.closest('.file-preview');
            if (element) {
                e.preventDefault();
                const fileUrl = element.dataset.fileUrl;
                const fileName = element.dataset.fileName || this.getFileNameFromUrl(fileUrl);
                this.openFile(fileUrl, fileName);
            }
        });

        // 關閉按鈕
        this.container.addEventListener('click', (e) => {
            if (e.target.classList.contains('close-btn') || 
                e.target.classList.contains('file-preview-container')) {
                this.close();
            }
        });

        // 儲存按鈕
        this.container.addEventListener('click', (e) => {
            if (e.target.closest('.save-btn')) {
                this.saveFile();
            }
        });

        // 下載按鈕
        this.container.addEventListener('click', (e) => {
            if (e.target.closest('.download-btn')) {
                this.downloadFile();
            }
        });
    }

    bindKeyboardShortcuts() {
        document.addEventListener('keydown', (e) => {
            if (!this.container.classList.contains('show')) return;

            // ESC 關閉
            if (e.key === 'Escape') {
                this.close();
                return;
            }

            // Ctrl+S / Cmd+S 儲存
            if ((e.ctrlKey || e.metaKey) && e.key === 's') {
                e.preventDefault();
                this.saveFile();
                return;
            }

            // Enter 確認操作
            if (e.key === 'Enter' && e.target.classList.contains('file-preview-btn')) {
                e.target.click();
            }
        });
    }

    async openFile(url, fileName) {
        this.currentFile = { url, fileName };
        this.show();
        this.showLoading();

        try {
            const fileType = this.getFileType(fileName);
            const fileExtension = this.getFileExtension(fileName);

            // 更新標題和圖標
            this.updateHeader(fileName, fileType);

            // 根據檔案類型選擇預覽方式
            if (this.isTextFile(fileExtension)) {
                await this.loadTextFile(url);
            } else if (this.isOfficeFile(fileExtension)) {
                await this.loadOfficeFile(url);
            } else if (this.isPdfFile(fileExtension)) {
                await this.loadPdfFile(url);
            } else if (this.isVideoFile(fileExtension)) {
                await this.loadVideoFile(url);
            } else if (this.isAudioFile(fileExtension)) {
                await this.loadAudioFile(url);
            } else if (this.isImageFile(fileExtension)) {
                await this.loadImageFile(url);
            } else {
                this.showUnsupportedFile();
            }
        } catch (error) {
            console.error('Error opening file:', error);
            this.showError('無法開啟檔案: ' + error.message);
        }
    }

    async loadTextFile(url) {
        try {
            const response = await fetch(url);
            if (!response.ok) throw new Error(`HTTP ${response.status}`);
            
            const arrayBuffer = await response.arrayBuffer();
            const { text, encoding } = this.detectEncoding(arrayBuffer);
            
            this.originalContent = text;
            this.encoding = encoding;
            
            this.showTextEditor(text, encoding);
        } catch (error) {
            throw new Error('無法載入文字檔案: ' + error.message);
        }
    }

    async loadOfficeFile(url) {
        const officeViewerUrl = `https://view.officeapps.live.com/op/embed.aspx?src=${encodeURIComponent(url)}`;
        
        this.showOfficeViewer(officeViewerUrl);
        
        // 10秒超時檢測
        this.timeoutId = setTimeout(() => {
            this.showOfficeTimeout();
        }, 10000);
    }

    async loadPdfFile(url) {
        this.showPdfViewer(url);
    }

    async loadVideoFile(url) {
        this.showVideoPlayer(url);
    }

    async loadAudioFile(url) {
        this.showAudioPlayer(url);
    }

    async loadImageFile(url) {
        this.showImageViewer(url);
    }

    showTextEditor(content, encoding) {
        const body = this.container.querySelector('.file-preview-body');
        const lines = content.split('\n').length;
        
        body.innerHTML = `
            <div class="text-editor-container">
                <div class="text-editor-toolbar">
                    <div class="text-editor-info">
                        <div class="text-editor-encoding">
                            <span>編碼:</span>
                            <span class="encoding-value">${encoding}</span>
                        </div>
                        <div class="text-editor-lines">
                            <span>行數:</span>
                            <span class="lines-count">${lines}</span>
                        </div>
                        <div class="text-editor-changed" style="display: none;">
                            檔案已修改
                        </div>
                    </div>
                </div>
                <div class="text-editor-wrapper">
                    <textarea class="text-editor" placeholder="檔案內容將在此顯示...">${this.escapeHtml(content)}</textarea>
                </div>
            </div>
        `;

        // 綁定文字編輯器事件
        const textEditor = body.querySelector('.text-editor');
        textEditor.addEventListener('input', () => {
            this.onTextChange();
        });

        // 隱藏下載按鈕，顯示儲存按鈕
        this.updateActionButtons(true);
    }

    showOfficeViewer(url) {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="office-viewer-container">
                <div class="office-viewer-loading">
                    <div class="loading-spinner"></div>
                    <div>正在載入 Office 檔案...</div>
                </div>
                <iframe class="office-viewer-iframe" src="${url}" style="display: none;"></iframe>
            </div>
        `;

        const iframe = body.querySelector('.office-viewer-iframe');
        iframe.onload = () => {
            clearTimeout(this.timeoutId);
            body.querySelector('.office-viewer-loading').style.display = 'none';
            iframe.style.display = 'block';
        };

        iframe.onerror = () => {
            clearTimeout(this.timeoutId);
            this.showOfficeError();
        };

        this.updateActionButtons(false);
    }

    showPdfViewer(url) {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="pdf-preview-container">
                <iframe class="pdf-preview-iframe" src="${url}"></iframe>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showVideoPlayer(url) {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="media-preview-container">
                <video class="media-preview-video" controls>
                    <source src="${url}" type="video/mp4">
                    您的瀏覽器不支援影片播放。
                </video>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showAudioPlayer(url) {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="media-preview-container">
                <audio class="media-preview-audio" controls>
                    <source src="${url}" type="audio/mpeg">
                    您的瀏覽器不支援音訊播放。
                </audio>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showImageViewer(url) {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="media-preview-container">
                <img src="${url}" style="max-width: 100%; max-height: 100%; object-fit: contain;" alt="圖片預覽">
            </div>
        `;

        this.updateActionButtons(false);
    }

    showOfficeTimeout() {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="office-viewer-error">
                <div class="office-viewer-timeout">
                    <h4>預覽載入超時</h4>
                    <p>Office 檔案預覽載入時間過長，請選擇下載檔案進行查看。</p>
                    <button class="file-preview-btn primary download-btn">
                        <span>⬇️</span> 下載檔案
                    </button>
                </div>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showOfficeError() {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="office-viewer-error">
                <h4>無法預覽 Office 檔案</h4>
                <p>此檔案無法在瀏覽器中預覽，請下載後使用相應的應用程式開啟。</p>
                <button class="file-preview-btn primary download-btn">
                    <span>⬇️</span> 下載檔案
                </button>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showUnsupportedFile() {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="office-viewer-error">
                <h4>不支援的檔案類型</h4>
                <p>此檔案類型無法在瀏覽器中預覽，請下載後使用相應的應用程式開啟。</p>
                <button class="file-preview-btn primary download-btn">
                    <span>⬇️</span> 下載檔案
                </button>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showError(message) {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="office-viewer-error">
                <h4>錯誤</h4>
                <p>${message}</p>
                <button class="file-preview-btn primary download-btn">
                    <span>⬇️</span> 下載檔案
                </button>
            </div>
        `;

        this.updateActionButtons(false);
    }

    showLoading() {
        const body = this.container.querySelector('.file-preview-body');
        body.innerHTML = `
            <div class="office-viewer-loading">
                <div class="loading-spinner"></div>
                <div>正在載入檔案...</div>
            </div>
        `;
    }

    onTextChange() {
        const textEditor = this.container.querySelector('.text-editor');
        const changedIndicator = this.container.querySelector('.text-editor-changed');
        const saveBtn = this.container.querySelector('.save-btn');
        const statusIndicator = this.container.querySelector('.status-indicator');

        if (textEditor.value !== this.originalContent) {
            this.isChanged = true;
            changedIndicator.style.display = 'block';
            saveBtn.disabled = false;
            statusIndicator.className = 'status-indicator changed';
        } else {
            this.isChanged = false;
            changedIndicator.style.display = 'none';
            saveBtn.disabled = true;
            statusIndicator.className = 'status-indicator saved';
        }
    }

    async saveFile() {
        if (!this.isChanged) return;

        const textEditor = this.container.querySelector('.text-editor');
        const content = textEditor.value;

        try {
            // 從 URL 中提取檔案名稱
            const fileName = this.getFileNameFromUrl(this.currentFile.url);
            
            // 調用後端 API 儲存檔案
            const response = await fetch('/Backoffice/Test/SaveFileContent', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded',
                },
                body: `fileName=${encodeURIComponent(fileName)}&content=${encodeURIComponent(content)}`
            });

            if (!response.ok) {
                throw new Error(`HTTP ${response.status}: ${response.statusText}`);
            }

            const result = await response.json();
            
            if (result.success) {
                this.originalContent = content;
                this.isChanged = false;
                
                const changedIndicator = this.container.querySelector('.text-editor-changed');
                const saveBtn = this.container.querySelector('.save-btn');
                const statusIndicator = this.container.querySelector('.status-indicator');
                
                changedIndicator.style.display = 'none';
                saveBtn.disabled = true;
                statusIndicator.className = 'status-indicator saved';
                
                // 顯示儲存成功提示
                this.showSaveSuccess();
            } else {
                throw new Error(result.message || '儲存失敗');
            }
            
        } catch (error) {
            console.error('Save error:', error);
            this.showSaveError(error.message);
        }
    }

    downloadFile() {
        if (this.currentFile) {
            const link = document.createElement('a');
            link.href = this.currentFile.url;
            link.download = this.currentFile.fileName;
            link.click();
        }
    }

    showSaveSuccess() {
        const saveBtn = this.container.querySelector('.save-btn');
        const originalText = saveBtn.innerHTML;
        saveBtn.innerHTML = '<span>✅</span> 已儲存';
        saveBtn.disabled = true;
        
        setTimeout(() => {
            saveBtn.innerHTML = originalText;
            saveBtn.disabled = !this.isChanged;
        }, 2000);
    }

    showSaveError(message) {
        const saveBtn = this.container.querySelector('.save-btn');
        const originalText = saveBtn.innerHTML;
        saveBtn.innerHTML = '<span>❌</span> 儲存失敗';
        saveBtn.disabled = true;
        
        setTimeout(() => {
            saveBtn.innerHTML = originalText;
            saveBtn.disabled = !this.isChanged;
        }, 2000);
    }

    updateHeader(fileName, fileType) {
        const title = this.container.querySelector('.file-preview-title');
        const fileIcon = title.querySelector('.file-icon');
        const fileNameSpan = title.querySelector('.file-name');
        
        fileNameSpan.textContent = fileName;
        fileIcon.className = `file-icon file-icon-${fileType}`;
    }

    updateActionButtons(showSave) {
        const saveBtn = this.container.querySelector('.save-btn');
        const downloadBtn = this.container.querySelector('.download-btn');
        
        saveBtn.style.display = showSave ? 'flex' : 'none';
        downloadBtn.style.display = 'flex';
    }

    show() {
        this.container.classList.add('show');
        document.body.style.overflow = 'hidden';
    }

    close() {
        if (this.isChanged) {
            if (!confirm('檔案已修改但未儲存，確定要關閉嗎？')) {
                return;
            }
        }
        
        this.container.classList.remove('show');
        document.body.style.overflow = '';
        
        // 清理資源
        if (this.timeoutId) {
            clearTimeout(this.timeoutId);
            this.timeoutId = null;
        }
        
        this.currentFile = null;
        this.originalContent = null;
        this.isChanged = false;
    }

    // 工具方法
    getFileNameFromUrl(url) {
        return url.split('/').pop() || '未知檔案';
    }

    getFileType(fileName) {
        const extension = this.getFileExtension(fileName).toLowerCase();
        
        if (['txt', 'log', 'json', 'xml', 'html', 'css', 'js', 'cs', 'py', 'java', 'cpp', 'c', 'h'].includes(extension)) {
            return 'txt';
        } else if (extension === 'pdf') {
            return 'pdf';
        } else if (['doc', 'docx'].includes(extension)) {
            return 'doc';
        } else if (['xls', 'xlsx'].includes(extension)) {
            return 'xls';
        } else if (['ppt', 'pptx'].includes(extension)) {
            return 'ppt';
        } else if (['mp4', 'webm', 'ogg', 'mov', 'avi'].includes(extension)) {
            return 'video';
        } else if (['mp3', 'wav', 'ogg', 'aac', 'flac'].includes(extension)) {
            return 'audio';
        } else if (['jpg', 'jpeg', 'png', 'gif', 'bmp', 'svg'].includes(extension)) {
            return 'image';
        }
        
        return 'txt';
    }

    getFileExtension(fileName) {
        return fileName.split('.').pop() || '';
    }

    isTextFile(extension) {
        const textExtensions = ['txt', 'log', 'json', 'xml', 'html', 'css', 'js', 'cs', 'py', 'java', 'cpp', 'c', 'h', 'md', 'yml', 'yaml', 'sql', 'sh', 'bat', 'ps1'];
        return textExtensions.includes(extension.toLowerCase());
    }

    isOfficeFile(extension) {
        const officeExtensions = ['doc', 'docx', 'xls', 'xlsx', 'ppt', 'pptx'];
        return officeExtensions.includes(extension.toLowerCase());
    }

    isPdfFile(extension) {
        return extension.toLowerCase() === 'pdf';
    }

    isVideoFile(extension) {
        const videoExtensions = ['mp4', 'webm', 'ogg', 'mov', 'avi'];
        return videoExtensions.includes(extension.toLowerCase());
    }

    isAudioFile(extension) {
        const audioExtensions = ['mp3', 'wav', 'ogg', 'aac', 'flac'];
        return audioExtensions.includes(extension.toLowerCase());
    }

    isImageFile(extension) {
        const imageExtensions = ['jpg', 'jpeg', 'png', 'gif', 'bmp', 'svg'];
        return imageExtensions.includes(extension.toLowerCase());
    }

    detectEncoding(arrayBuffer) {
        // 簡單的編碼檢測
        const bytes = new Uint8Array(arrayBuffer);
        
        // 檢查 BOM
        if (bytes.length >= 3 && bytes[0] === 0xEF && bytes[1] === 0xBB && bytes[2] === 0xBF) {
            return { text: new TextDecoder('utf-8').decode(arrayBuffer.slice(3)), encoding: 'UTF-8' };
        }
        
        if (bytes.length >= 2 && bytes[0] === 0xFF && bytes[1] === 0xFE) {
            return { text: new TextDecoder('utf-16le').decode(arrayBuffer.slice(2)), encoding: 'UTF-16 LE' };
        }
        
        if (bytes.length >= 2 && bytes[0] === 0xFE && bytes[1] === 0xFF) {
            return { text: new TextDecoder('utf-16be').decode(arrayBuffer.slice(2)), encoding: 'UTF-16 BE' };
        }
        
        // 嘗試 UTF-8
        try {
            const text = new TextDecoder('utf-8', { fatal: true }).decode(arrayBuffer);
            return { text, encoding: 'UTF-8' };
        } catch (e) {
            // 回退到 Latin-1
            const text = new TextDecoder('latin1').decode(arrayBuffer);
            return { text, encoding: 'Latin-1' };
        }
    }

    escapeHtml(text) {
        const div = document.createElement('div');
        div.textContent = text;
        return div.innerHTML;
    }
}

// 自動初始化
document.addEventListener('DOMContentLoaded', () => {
    new FilePreview();
});

// 導出類別供外部使用
if (typeof module !== 'undefined' && module.exports) {
    module.exports = FilePreview;
}
