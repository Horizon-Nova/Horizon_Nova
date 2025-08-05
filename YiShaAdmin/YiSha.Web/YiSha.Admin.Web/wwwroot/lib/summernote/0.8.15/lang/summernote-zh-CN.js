/*!
 * 
 * Super simple wysiwyg editor v0.8.15
 * https://summernote.org
 * 
 * 
 * Copyright 2013- Alan Hong. and other contributors
 * summernote may be freely distributed under the MIT license.
 * 
 * Date: 2020-01-04T11:44Z
 * 
 */
(function webpackUniversalModuleDefinition(root, factory) {
	if(typeof exports === 'object' && typeof module === 'object')
		module.exports = factory();
	else if(typeof define === 'function' && define.amd)
		define([], factory);
	else {
		var a = factory();
		for(var i in a) (typeof exports === 'object' ? exports : root)[i] = a[i];
	}
})(window, function() {
return /******/ (function(modules) { // webpackBootstrap
/******/ 	// The module cache
/******/ 	var installedModules = {};
/******/
/******/ 	// The require function
/******/ 	function __webpack_require__(moduleId) {
/******/
/******/ 		// Check if module is in cache
/******/ 		if(installedModules[moduleId]) {
/******/ 			return installedModules[moduleId].exports;
/******/ 		}
/******/ 		// Create a new module (and put it into the cache)
/******/ 		var module = installedModules[moduleId] = {
/******/ 			i: moduleId,
/******/ 			l: false,
/******/ 			exports: {}
/******/ 		};
/******/
/******/ 		// Execute the module function
/******/ 		modules[moduleId].call(module.exports, module, module.exports, __webpack_require__);
/******/
/******/ 		// Flag the module as loaded
/******/ 		module.l = true;
/******/
/******/ 		// Return the exports of the module
/******/ 		return module.exports;
/******/ 	}
/******/
/******/
/******/ 	// expose the modules object (__webpack_modules__)
/******/ 	__webpack_require__.m = modules;
/******/
/******/ 	// expose the module cache
/******/ 	__webpack_require__.c = installedModules;
/******/
/******/ 	// define getter function for harmony exports
/******/ 	__webpack_require__.d = function(exports, name, getter) {
/******/ 		if(!__webpack_require__.o(exports, name)) {
/******/ 			Object.defineProperty(exports, name, { enumerable: true, get: getter });
/******/ 		}
/******/ 	};
/******/
/******/ 	// define __esModule on exports
/******/ 	__webpack_require__.r = function(exports) {
/******/ 		if(typeof Symbol !== 'undefined' && Symbol.toStringTag) {
/******/ 			Object.defineProperty(exports, Symbol.toStringTag, { value: 'Module' });
/******/ 		}
/******/ 		Object.defineProperty(exports, '__esModule', { value: true });
/******/ 	};
/******/
/******/ 	// create a fake namespace object
/******/ 	// mode & 1: value is a module id, require it
/******/ 	// mode & 2: merge all properties of value into the ns
/******/ 	// mode & 4: return value when already ns object
/******/ 	// mode & 8|1: behave like require
/******/ 	__webpack_require__.t = function(value, mode) {
/******/ 		if(mode & 1) value = __webpack_require__(value);
/******/ 		if(mode & 8) return value;
/******/ 		if((mode & 4) && typeof value === 'object' && value && value.__esModule) return value;
/******/ 		var ns = Object.create(null);
/******/ 		__webpack_require__.r(ns);
/******/ 		Object.defineProperty(ns, 'default', { enumerable: true, value: value });
/******/ 		if(mode & 2 && typeof value != 'string') for(var key in value) __webpack_require__.d(ns, key, function(key) { return value[key]; }.bind(null, key));
/******/ 		return ns;
/******/ 	};
/******/
/******/ 	// getDefaultExport function for compatibility with non-harmony modules
/******/ 	__webpack_require__.n = function(module) {
/******/ 		var getter = module && module.__esModule ?
/******/ 			function getDefault() { return module['default']; } :
/******/ 			function getModuleExports() { return module; };
/******/ 		__webpack_require__.d(getter, 'a', getter);
/******/ 		return getter;
/******/ 	};
/******/
/******/ 	// Object.prototype.hasOwnProperty.call
/******/ 	__webpack_require__.o = function(object, property) { return Object.prototype.hasOwnProperty.call(object, property); };
/******/
/******/ 	// __webpack_public_path__
/******/ 	__webpack_require__.p = "";
/******/
/******/
/******/ 	// Load entry module and return exports
/******/ 	return __webpack_require__(__webpack_require__.s = 48);
/******/ })
/************************************************************************/
/******/ ({

/***/ 48:
/***/ (function(module, exports) {

(function ($) {
  $.extend($.summernote.lang, {
    'zh-CN': {
      font: {
        bold: '粗體',
        italic: '斜體',
        underline: '底線',
        clear: '清除格式',
        height: '行高',
        name: '字體',
        strikethrough: '删除線',
        subscript: '下標',
        superscript: '上標',
        size: '字號'
      },
      image: {
        image: '圖片',
        insert: '插入圖片',
        resizeFull: '縮放至 100%',
        resizeHalf: '縮放至 50%',
        resizeQuarter: '縮放至 25%',
        floatLeft: '靠左浮動',
        floatRight: '靠右浮動',
        floatNone: '取消浮動',
        shapeRounded: '形狀: 圓角',
        shapeCircle: '形狀: 圓',
        shapeThumbnail: '形狀: 縮略圖',
        shapeNone: '形狀: 无',
        dragImageHere: '將圖片拖拽至此處',
        dropImage: '拖拽圖片或文本',
        selectFromFiles: '從本地上傳',
        maximumFileSize: '文件大小最大值',
        maximumFileSizeError: '文件大小超出最大值。',
        url: '圖片地址',
        remove: '移除圖片',
        original: '原始圖片'
      },
      video: {
        video: '視频',
        videoLink: '視频連結',
        insert: '插入視频',
        url: '視频地址',
        providers: '(優酷, 腾訊, Instagram, DailyMotion, Youtube等)'
      },
      link: {
        link: '連結',
        insert: '插入連結',
        unlink: '去除連結',
        edit: '編輯連結',
        textToDisplay: '顯示文本',
        url: '連結地址',
        openInNewWindow: '在新窗口打開'
      },
      table: {
        table: '表格',
        addRowAbove: '在上方插入行',
        addRowBelow: '在下方插入行',
        addColLeft: '在左側插入列',
        addColRight: '在右側插入列',
        delRow: '删除行',
        delCol: '删除列',
        delTable: '删除表格'
      },
      hr: {
        insert: '水平線'
      },
      style: {
        style: '樣式',
        p: '普通',
        blockquote: '引用',
        pre: '代碼',
        h1: '標題 1',
        h2: '標題 2',
        h3: '標題 3',
        h4: '標題 4',
        h5: '標題 5',
        h6: '標題 6'
      },
      lists: {
        unordered: '无序列表',
        ordered: '有序列表'
      },
      options: {
        help: '帮助',
        fullscreen: '全螢幕',
        codeview: '源代碼'
      },
      paragraph: {
        paragraph: '段落',
        outdent: '减少縮進',
        indent: '增加縮進',
        left: '左對齐',
        center: '居中對齐',
        right: '右對齐',
        justify: '兩端對齐'
      },
      color: {
        recent: '最近使用',
        more: '更多',
        background: '背景',
        foreground: '前景',
        transparent: '透明',
        setTransparent: '透明',
        reset: '重置',
        resetToDefault: '默認'
      },
      shortcut: {
        shortcuts: '快捷鍵',
        close: '關閉',
        textFormatting: '文本格式',
        action: '動作',
        paragraphFormatting: '段落格式',
        documentStyle: '檔案樣式',
        extraKeys: '額外按鍵'
      },
      help: {
        insertParagraph: '插入段落',
        undo: '撤銷',
        redo: '重做',
        tab: '增加縮進',
        untab: '减少縮進',
        bold: '粗體',
        italic: '斜體',
        underline: '底線',
        strikethrough: '删除線',
        removeFormat: '清除格式',
        justifyLeft: '左對齐',
        justifyCenter: '居中對齐',
        justifyRight: '右對齐',
        justifyFull: '兩端對齐',
        insertUnorderedList: '无序列表',
        insertOrderedList: '有序列表',
        outdent: '减少縮進',
        indent: '增加縮進',
        formatPara: '設置選中內容樣式為 普通',
        formatH1: '設置選中內容樣式為 標題1',
        formatH2: '設置選中內容樣式為 標題2',
        formatH3: '設置選中內容樣式為 標題3',
        formatH4: '設置選中內容樣式為 標題4',
        formatH5: '設置選中內容樣式為 標題5',
        formatH6: '設置選中內容樣式為 標題6',
        insertHorizontalRule: '插入水平線',
        'linkDialog.show': '顯示連結對話框'
      },
      history: {
        undo: '撤銷',
        redo: '重做'
      },
      specialChar: {
        specialChar: '特殊字符',
        select: '選取特殊字符'
      }
    }
  });
})(jQuery);

/***/ })

/******/ });
});