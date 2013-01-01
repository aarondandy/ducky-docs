$(document).ready(function () {
	var namespaceElm = $("#current-namespace");
	if (!namespaceElm) {return;}
	var nsId = "#typelist-" + namespaceElm.text().replace(/[.]/g,"-");
	var nsGroup = $(nsId);
	if (!nsGroup) { return; }
	nsGroup.addClass('in');
});