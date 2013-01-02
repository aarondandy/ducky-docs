// initial namespace selection
$(document).ready(function () {
	var namespaceElm = $("#current-namespace");
	if (!namespaceElm) {return;}
	var nsId = "#typelist-" + namespaceElm.text().replace(/[.]/g,"-");
	var nsGroup = $(nsId);
	if (!nsGroup) { return; }
	nsGroup.addClass('in');
});
// enable tips on flair icons
$(document).ready(function () {
	$(".flair-icon").tooltip({placement: 'left'});
});