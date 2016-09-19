/*$(function() {
    $(".sorting i").removeClass();
    $(".sorting i").addClass("icon-circle");
    $(".sorting").each(function() {
        var icon = $(this).find("i");
        var href = window.location.href;
        if (href.indexOf("sort") != -1) {
            icon.removeClass();
        }
        if (href.indexOf("NameAsc") != -1 || href.indexOf("SKUAsc") != -1) {
            if(href.contains(this.attr("class")) {
                icon.addClass("icon-chevron-down");
            }
        }
        if (href.indexOf("NameDesc") != -1 || href.indexOf("SKUDesc") != -1) {
            icon.addClass("icon-chevron-up");
        }
    });

    $("body").on("click", ".sorting", function () {
        var icon = $(this).find("i");
        if (icon.hasClass("icon-circle") || icon.hasClass("icon-chevron-up")) {
            icon.removeClass();
            icon.addClass("icon-chevron-down");
        } else {
            if (icon.hasClass("icon-chevron-down")) {
                icon.removeClass();
                icon.addClass("icon-chevron-up");
            }
        }
    });
});*/