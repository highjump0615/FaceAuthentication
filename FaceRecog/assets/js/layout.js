$(function() {
    'use strict';
    $(window).resize(function () {
        setTimeout(handleChangeablePosition, 0.01)
    });
    setTimeout(handleChangeablePosition, 0.01);
});

var handleChangeablePosition = function() {
    $('#changeable').css({
        'margin-top': 0,
        'margin-bottom': 0
    });
    var d = $('.wrap').height() - $('.wrap > .container').height();
	if (d > 0)
	{
		var v = d / 2;
		$('#changeable').css({
			'margin-top':v,
			'margin-bottom':v
		});
	}
}