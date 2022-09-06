
var ok = 'OK';
    
function ShowNotification(message, type) {
    $.notify(message, { position: "right middle", style: "bootstrap", className: type });
}

function modalSize(size) {
    $("#myModalDialog").removeClass("modal-lg");
    $("#myModalDialog").removeClass("modal-content");
    $("#myModalDialog").removeClass("modal-xl");

    if (size !== '') {
        $("#myModalDialog").addClass("modal-" + size);
    }
}

(function ($) {
    $("body").delegate(".numOnly", "keydown", function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13]) !== -1 ||
            // Allow: Ctrl+A, Command+A
            (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105)) {
            e.preventDefault();
        }
    });

    $("body").delegate(".currencyOnly", "keydown", function (e) {
        // Allow: backspace, delete, tab, escape, enter and .
        if ($.inArray(e.keyCode, [46, 8, 9, 27, 13, 110, 250]) !== -1 ||
            // Allow: Ctrl+A, Command+A
            (e.keyCode === 65 && (e.ctrlKey === true || e.metaKey === true)) ||
            // Allow: home, end, left, right, down, up
            (e.keyCode >= 35 && e.keyCode <= 40)) {
            // let it happen, don't do anything
            return;
        }
        // Ensure that it is a number and stop the keypress
        if ((e.shiftKey || (e.keyCode < 48 || e.keyCode > 57)) && (e.keyCode < 96 || e.keyCode > 105) || e.keyCode === 190) {
            e.preventDefault();
        }
    });

    $("body").delegate(".hasDatepicker", "keydown", function (e) {        
        e.preventDefault();
    });

})(jQuery);


(function ($) {
    $.extend({
        toDictionary: function (query) {
            var parms = {};
            var items = query.split("&"); // split
            for (var i = 0; i < items.length; i++) {
                var values = items[i].split("=");
                var key = decodeURIComponent(values.shift());
                var value = values.join("=");
                parms[key] = decodeURIComponent(value);
            }
            return (parms);
        }
    });
})(jQuery);

function onBegin() {
    $('#loader-wrapper').show();
}
function onComplete() {
    $('#loader-wrapper').hide();
}


function successPopup(Message) {
    bootbox.alert({
        message: Message,
        backdrop: true,
        buttons: {
            ok: {
                label: ok,
                className: 'btn-success',
                callback: function () {
                }
            }
        }
    }).find('.modal-content').css({
        //'background-color': '#f99',
        //'font-weight': 'bold',
        'color': '#3dd42f',
        //'font-size': '2em',
        'margin-top': function () {
            var w = $(window).height();
            var b = $(".modal-dialog").height();
            // should not be (w-h)/2
            var h = (w - (b + 120)) / 2;
            return h + "px";
        }
    });
}

function warningPopup(Message) {
    bootbox.alert({
        message: Message,
        backdrop: true,
        buttons: {
            ok: {
                label: ok,
                className: 'btn-danger',
                callback: function () {
                }
            }
        }
    }).find('.modal-content').css({
        //'background-color': '#f99',
        //'font-weight': 'bold',
        'color': '#F00',
        //'font-size': '2em',
        'margin-top': function () {
            var w = $(window).height();
            var b = $(".modal-dialog").height();
            // should not be (w-h)/2
            var h = (w - (b + 120)) / 2;
            return h + "px";
        }
    });
}