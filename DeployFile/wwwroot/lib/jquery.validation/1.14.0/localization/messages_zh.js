(function( factory ) {
	if ( typeof define === "function" && define.amd ) {
		define( ["jquery", "../jquery.validate"], factory );
	} else {
		factory( jQuery );
	}
}(function( $ ) {

/*
 * Translated default messages for the jQuery validation plugin.
 * Locale: ZH (Chinese, 中文 (Zhōngwén), 汉語, 漢語)
 */
$.extend($.validator.messages, {
	required: "这是必填字段",
	remote: "請修正此字段",
	email: "請输入有效的电子邮件地址",
	url: "請输入有效的网址",
	date: "請输入有效的日期",
	dateISO: "請输入有效的日期 (YYYY-MM-DD)",
	number: "請输入有效的數字",
	digits: "只能输入數字",
	creditcard: "請输入有效的信用卡号码",
	equalTo: "你的输入不相同",
	extension: "請输入有效的後缀",
	maxlength: $.validator.format("最多可以输入 {0} 个字符"),
	minlength: $.validator.format("最少要输入 {0} 个字符"),
	rangelength: $.validator.format("請输入长度在 {0} 到 {1} 之间的字符串"),
	range: $.validator.format("請输入范围在 {0} 到 {1} 之间的數值"),
	max: $.validator.format("請输入不大于 {0} 的數值"),
	min: $.validator.format("請输入不小于 {0} 的數值")
});

}));