<div class="flex flex-col lg:flex-row gap-0">
    <!-- 左側：目錄樹 -->
    <aside class="w-full lg:w-64 bg-white border-r border-slate-200 flex flex-col lg:max-h-screen">
        <!-- 標題列 -->
        <div class="h-12 px-3 flex items-center justify-between border-b border-slate-200">
            <span class="text-xs lg:text-sm font-medium text-slate-900">檔案總管</span>
            <div class="flex items-center gap-0.5">
                <button onclick="event.stopPropagation(); showModal('filemanager-help')" class="w-7 h-7 flex items-center justify-center hover:bg-slate-100 rounded" title="操作說明">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="help-circle" class="lucide lucide-help-circle h-3 w-3 lg:h-4 lg:w-4 text-slate-600"><circle cx="12" cy="12" r="10"></circle><path d="M9.09 9a3 3 0 0 1 5.83 1c0 2-3 3-3 3"></path><path d="M12 17h.01"></path></svg>
                </button>
                <button onclick="showCreateFileModal()" class="w-7 h-7 flex items-center justify-center hover:bg-slate-100 rounded" title="新增檔案">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="file-plus" class="lucide lucide-file-plus h-3 w-3 lg:h-4 lg:w-4 text-slate-600"><path d="M15 2H6a2 2 0 0 0-2 2v16a2 2 0 0 0 2 2h12a2 2 0 0 0 2-2V7Z"></path><path d="M14 2v4a2 2 0 0 0 2 2h4"></path><path d="M9 15h6"></path><path d="M12 18v-6"></path></svg>
                </button>
                <button onclick="showCreateFolderModal()" class="w-7 h-7 flex items-center justify-center hover:bg-slate-100 rounded" title="新增資料夾">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder-plus" class="lucide lucide-folder-plus h-3 w-3 lg:h-4 lg:w-4 text-slate-600"><path d="M12 10v6"></path><path d="M9 13h6"></path><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                </button>
                <button onclick="location.reload()" class="w-7 h-7 flex items-center justify-center hover:bg-slate-100 rounded" title="重新整理">
                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="refresh-cw" class="lucide lucide-refresh-cw h-3 w-3 lg:h-4 lg:w-4 text-slate-600"><path d="M3 12a9 9 0 0 1 9-9 9.75 9.75 0 0 1 6.74 2.74L21 8"></path><path d="M21 3v5h-5"></path><path d="M21 12a9 9 0 0 1-9 9 9.75 9.75 0 0 1-6.74-2.74L3 16"></path><path d="M8 16H3v5"></path></svg>
                </button>
            </div>
        </div>
        
        <!-- 目錄樹 -->
        <div class="flex-1 overflow-y-auto py-2 max-h-60 lg:max-h-none">
            <!-- 根目錄 -->
            <a href="/Backoffice/FileManager/FileManager" class="flex items-center gap-1.5 px-3 py-1.5 hover:bg-slate-50 cursor-pointer bg-slate-100">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder-open" class="lucide lucide-folder-open h-4 w-4 text-amber-500"><path d="m6 14 1.5-2.9A2 2 0 0 1 9.24 10H20a2 2 0 0 1 1.94 2.5l-1.54 6a2 2 0 0 1-1.95 1.5H4a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h3.9a2 2 0 0 1 1.69.9l.81 1.2a2 2 0 0 0 1.67.9H18a2 2 0 0 1 2 2v2"></path></svg>
                <span class="text-sm font-medium text-slate-900">根目錄</span>
            </a>
            
                    <a href="/Backoffice/FileManager/FileManager?parentCode=folder_6c6aee7d67054ccc9bc783d1039d8a50" class="flex items-center gap-1.5 px-3 py-1.5 hover:bg-slate-50 cursor-pointer " style="padding-left: 1.75rem">
                            <span class="w-4"></span>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-4 w-4 text-amber-500"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                        <span class="text-sm text-slate-700">Horizon_Nova_Backoffice</span>
                    </a>
                    <a href="/Backoffice/FileManager/FileManager?parentCode=folder_4cb177268f714a689836742ba6945a9d" class="flex items-center gap-1.5 px-3 py-1.5 hover:bg-slate-50 cursor-pointer " style="padding-left: 1.75rem">
                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="chevron-right" onclick="toggleTreeNode(event, this)" class="lucide lucide-chevron-right lucide-chevron-down h-4 w-4 text-slate-600 cursor-pointer hover:bg-slate-200 rounded"><path d="m9 18 6-6-6-6"></path></svg>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-4 w-4 text-amber-500"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                        <span class="text-sm text-slate-700">第30屆大專校院資訊應用服務創新競賽</span>
                    </a>
                    <a href="/Backoffice/FileManager/FileManager?parentCode=folder_9ddd4d1e5aec4b12a8c5bf3801df83a9" class="flex items-center gap-1.5 px-3 py-1.5 hover:bg-slate-50 cursor-pointer " style="padding-left: 1.75rem">
                            <span class="w-4"></span>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-4 w-4 text-amber-500"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                        <span class="text-sm text-slate-700">第四屆智泰科技競賽(專案)</span>
                    </a>
                    <a href="/Backoffice/FileManager/FileManager?parentCode=folder_832c17e2a1774db5bfcd92dc827e7ffe" class="flex items-center gap-1.5 px-3 py-1.5 hover:bg-slate-50 cursor-pointer " style="padding-left: 1.75rem">
                            <span class="w-4"></span>
                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-4 w-4 text-amber-500"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                        <span class="text-sm text-slate-700">美商(ADVANTEK)</span>
                    </a>
        </div>
    </aside>

    <!-- 右側：內容區 -->
    <div class="flex-1 flex flex-col bg-white min-w-0">
        <!-- 頂部導航列 -->
        <div class="h-12 px-3 lg:px-6 flex items-center justify-between border-b border-slate-200">
            <!-- 麵包屑 -->
            <nav class="flex items-center gap-1 text-xs lg:text-sm overflow-x-auto">
                <a href="/Backoffice/FileManager/FileManager" class="px-2 py-1 hover:bg-slate-100 rounded font-medium text-slate-900">
                    根目錄
                </a>
            </nav>
            
            <!-- 統計資訊 -->
            <div class="text-xs text-slate-500 hidden lg:block whitespace-nowrap">
                10 個資料夾 · 17 個檔案 · 438.68 MB
            </div>
        </div>

        <!-- Dropzone 上傳區 -->
        <div class="mx-3 lg:mx-6 mt-4">
            <div id="dropZone" class="border-2 border-dashed border-slate-300 rounded-lg px-4 lg:px-6 py-6 lg:py-8 text-center hover:border-slate-400 hover:bg-slate-50/50 cursor-pointer group" onclick="document.getElementById('fileInput').click()">
                <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="upload-cloud" class="lucide lucide-upload-cloud h-8 w-8 lg:h-10 lg:w-10 mx-auto mb-2 lg:mb-3 text-slate-400 group-hover:text-slate-500"><path d="M12 13v8"></path><path d="M4 14.899A7 7 0 1 1 15.71 8h1.79a4.5 4.5 0 0 1 2.5 8.242"></path><path d="m8 17 4-4 4 4"></path></svg>
                <p class="text-xs lg:text-sm font-medium text-slate-700 mb-1">拖曳檔案或資料夾到這裡上傳</p>
                <p class="text-xs text-slate-500">或點擊選擇檔案</p>
            </div>
        </div>
        
        <!-- 隱藏的檔案輸入 -->
        <input type="file" id="fileInput" multiple="" class="hidden">

        <!-- 檔案列表 -->
        <div class="flex-1 overflow-y-auto px-3 lg:px-6 py-4">
            <!-- 表格標題 -->
            <div class="hidden lg:grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-2 text-xs font-medium text-slate-500 border-b border-slate-200">
                <div>名稱</div>
                <div>擁有者</div>
                <div>大小</div>
                <div>修改時間</div>
                <div class="text-right">操作</div>
            </div>

            <!-- 檔案項目 -->
            <div class="divide-y divide-slate-100">
                    <!-- 資料夾列表 -->
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_6c6aee7d67054ccc9bc783d1039d8a50'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">Horizon_Nova_Backoffice</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('Horizon_Nova_Backoffice', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('Horizon_Nova_Backoffice', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('Horizon_Nova_Backoffice', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('Horizon_Nova_Backoffice', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_c584885c98fa449d96224c074fa72222'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">mouse_trace</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('mouse_trace', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('mouse_trace', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('mouse_trace', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('mouse_trace', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_4c9523cf072b4bb79541b0110e30dce7'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">tbllock_multiobject_detect</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock_multiobject_detect', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock_multiobject_detect', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock_multiobject_detect', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock_multiobject_detect', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_ddf31b82653f45a7b65e9380412d63c0'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">tbllock_planetruck</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock_planetruck', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock_planetruck', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock_planetruck', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock_planetruck', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_d8f898626fdc47979029c5b896dd6f4a'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">tbllock-tugonly</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock-tugonly', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock-tugonly', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock-tugonly', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock-tugonly', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_a32c04edded14309b841a67c4d8d79bb'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">tbltrae</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbltrae', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbltrae', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbltrae', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbltrae', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_f2f6eb96ef7a4065b9fc00a5f72e0b7d'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">video</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('video', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('video', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('video', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('video', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_4cb177268f714a689836742ba6945a9d'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">第30屆大專校院資訊應用服務創新競賽</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('第30屆大專校院資訊應用服務創新競賽', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('第30屆大專校院資訊應用服務創新競賽', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('第30屆大專校院資訊應用服務創新競賽', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('第30屆大專校院資訊應用服務創新競賽', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_9ddd4d1e5aec4b12a8c5bf3801df83a9'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">第四屆智泰科技競賽(專案)</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('第四屆智泰科技競賽(專案)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('第四屆智泰科技競賽(專案)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('第四屆智泰科技競賽(專案)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('第四屆智泰科技競賽(專案)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="window.location.href='/Backoffice/FileManager/FileManager?parentCode=folder_832c17e2a1774db5bfcd92dc827e7ffe'" class="grid grid-cols-1 lg:grid-cols-[2fr_140px_140px_180px_120px] gap-2 lg:gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg">
                                <div class="flex items-center gap-3 min-w-0">
                                    <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="folder" class="lucide lucide-folder h-5 w-5 text-amber-500 shrink-0"><path d="M20 20a2 2 0 0 0 2-2V8a2 2 0 0 0-2-2h-7.9a2 2 0 0 1-1.69-.9L9.6 3.9A2 2 0 0 0 7.93 3H4a2 2 0 0 0-2 2v13a2 2 0 0 0 2 2Z"></path></svg>
                                    <span class="text-sm text-slate-900 truncate flex-1">美商(ADVANTEK)</span>
                                    <div class="flex items-center gap-1 lg:hidden">
                                        <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('美商(ADVANTEK)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                        </button>
                                        <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('美商(ADVANTEK)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                            <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                        </button>
                                    </div>
                                </div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block truncate">system</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">—</div>
                                <div class="text-xs lg:text-sm text-slate-600 hidden lg:block">2025/10/16 05:30</div>
                                <div class="hidden lg:flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('美商(ADVANTEK)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('美商(ADVANTEK)', 'folder')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                    <!-- 檔案列表 -->
                            <div onclick="openFile('file_0651a40dfe2344d685dc485f9f5d5d97', '2020新款Vertex裝機SOP-軟體安裝設定.pdf')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">2020新款Vertex裝機SOP-軟體安裝設定.pdf</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">987.39 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('2020新款Vertex裝機SOP-軟體安裝設定.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_0651a40dfe2344d685dc485f9f5d5d97" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('2020新款Vertex裝機SOP-軟體安裝設定.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_80f70a1a37074240975eafbea7df288b', 'HnbBackoffice_Postgre Specification.XLSX')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">HnbBackoffice_Postgre Specification.XLSX</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">51 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('HnbBackoffice_Postgre Specification.XLSX', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_80f70a1a37074240975eafbea7df288b" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('HnbBackoffice_Postgre Specification.XLSX', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_c588f9f6341b4808afefc4bcf0f0014c', 'INSPEC講義.pdf')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">INSPEC講義.pdf</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">755.98 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('INSPEC講義.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_c588f9f6341b4808afefc4bcf0f0014c" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('INSPEC講義.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_abc3b8466520485ca266b6dab5d1fa7a', 'mouse_trace.mp4')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">mouse_trace.mp4</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">45.64 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('mouse_trace.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_abc3b8466520485ca266b6dab5d1fa7a" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('mouse_trace.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_2612a0ecb6374713bfee2d9bac75b8b1', 'tbllock_multiobject(裁切).mp4')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">tbllock_multiobject(裁切).mp4</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">76.6 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock_multiobject(裁切).mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_2612a0ecb6374713bfee2d9bac75b8b1" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock_multiobject(裁切).mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_d759b5db524a42bdbba505d3ab474316', 'tbllock_planetruck.mp4')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">tbllock_planetruck.mp4</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">95.5 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock_planetruck.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_d759b5db524a42bdbba505d3ab474316" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock_planetruck.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_c35670a9f14e49aaa10b8954f6854ed7', 'tbllock-tugonly.mp4')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">tbllock-tugonly.mp4</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">80.55 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbllock-tugonly.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_c35670a9f14e49aaa10b8954f6854ed7" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbllock-tugonly.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_a99268073bb14c81be973c001702acdb', 'tbltrae.mp4')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">tbltrae.mp4</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">103.9 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('tbltrae.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_a99268073bb14c81be973c001702acdb" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('tbltrae.mp4', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_654ff94ef32247c48f3e54fb882188d2', '專案競賽VisLab操作簡報-第四屆.pdf')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">專案競賽VisLab操作簡報-第四屆.pdf</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">5.6 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('專案競賽VisLab操作簡報-第四屆.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_654ff94ef32247c48f3e54fb882188d2" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('專案競賽VisLab操作簡報-第四屆.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_4bf3c4a75c784b459b9a1a731cb151fa', '專案競賽簡報內容.pptx')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">專案競賽簡報內容.pptx</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">894.06 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('專案競賽簡報內容.pptx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_4bf3c4a75c784b459b9a1a731cb151fa" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('專案競賽簡報內容.pptx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_a0749d85624f4ffe957d7ed9eb47c2fa', '專案競賽簡報內容-初賽.pptx')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">專案競賽簡報內容-初賽.pptx</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">2.84 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('專案競賽簡報內容-初賽.pptx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_a0749d85624f4ffe957d7ed9eb47c2fa" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('專案競賽簡報內容-初賽.pptx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_33ce52ba58464c458aaea81fb93dff13', '智慧指標.pdf')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">智慧指標.pdf</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">1.39 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('智慧指標.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_33ce52ba58464c458aaea81fb93dff13" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('智慧指標.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_2219d3047cf44c6b8201983bf2a48ac7', '智慧指標.pptx')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">智慧指標.pptx</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">23.21 MB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('智慧指標.pptx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_2219d3047cf44c6b8201983bf2a48ac7" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('智慧指標.pptx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_3d0da068be5a4049b5d917f0d569e397', '智慧指標內容.txt')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">智慧指標內容.txt</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">3.74 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('智慧指標內容.txt', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_3d0da068be5a4049b5d917f0d569e397" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('智慧指標內容.txt', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_7034f1d72c9045588624ce8822d7af5d', '第四屆智泰科技-通用數字錶盤識別系統.md')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">第四屆智泰科技-通用數字錶盤識別系統.md</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">10.25 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('第四屆智泰科技-通用數字錶盤識別系統.md', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_7034f1d72c9045588624ce8822d7af5d" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('第四屆智泰科技-通用數字錶盤識別系統.md', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_91f76cbd1c5c4f51a61eb88e2ecd5d82', '評估時間表.docx')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">評估時間表.docx</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">15.08 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('評估時間表.docx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_91f76cbd1c5c4f51a61eb88e2ecd5d82" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('評估時間表.docx', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
                            <div onclick="openFile('file_4d40b926580b41488abd665ce2725b43', '量測程式智慧流程方案計畫-ADV_07152025_250722_132525.pdf')" class="grid grid-cols-[2fr_140px_140px_180px_120px] gap-4 px-3 py-3 hover:bg-slate-50 cursor-pointer group rounded-lg items-center">
                                <div class="flex items-center gap-3 min-w-0">
                                    <div class="w-5 h-5 shrink-0"></div>
                                    <span class="text-sm text-slate-900 truncate">量測程式智慧流程方案計畫-ADV_07152025_250722_132525.pdf</span>
                                </div>
                                <div class="text-sm text-slate-600 truncate">system</div>
                                <div class="text-sm text-slate-600">819.21 KB</div>
                                <div class="text-sm text-slate-600">2025/10/16 05:30</div>
                                <div class="flex items-center justify-end gap-1">
                                    <button onclick="event.preventDefault(); event.stopPropagation(); showRenameModal('量測程式智慧流程方案計畫-ADV_07152025_250722_132525.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="重新命名">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="edit-3" class="lucide lucide-edit-3 h-4 w-4 text-slate-600"><path d="M13 21h8"></path><path d="M21.174 6.812a1 1 0 0 0-3.986-3.987L3.842 16.174a2 2 0 0 0-.5.83l-1.321 4.352a.5.5 0 0 0 .623.622l4.353-1.32a2 2 0 0 0 .83-.497z"></path></svg>
                                    </button>
                                    <a href="/Backoffice/FileManager/Download?code=file_4d40b926580b41488abd665ce2725b43" onclick="event.stopPropagation()" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="下載">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="download" class="lucide lucide-download h-4 w-4 text-slate-600"><path d="M12 15V3"></path><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"></path><path d="m7 10 5 5 5-5"></path></svg>
                                    </a>
                                    <button onclick="event.preventDefault(); event.stopPropagation(); deleteItem('量測程式智慧流程方案計畫-ADV_07152025_250722_132525.pdf', 'file')" class="w-8 h-8 flex items-center justify-center hover:bg-slate-200 rounded" title="刪除">
                                        <svg xmlns="http://www.w3.org/2000/svg" width="24" height="24" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" data-lucide="trash-2" class="lucide lucide-trash-2 h-4 w-4 text-slate-600"><path d="M10 11v6"></path><path d="M14 11v6"></path><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6"></path><path d="M3 6h18"></path><path d="M8 6V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"></path></svg>
                                    </button>
                                </div>
                            </div>
            </div>
        </div>
    </div>
</div>