var routePrefix = "/Benefit.Web";
//var routePrefix = "";

// возвращает cookie с именем name, если есть, если нет, то undefined
function getCookie(name) {
    var matches = document.cookie.match(new RegExp(
      "(?:^|; )" + name.replace(/([\.$?*|{}\(\)\[\]\\\/\+^])/g, '\\$1') + "=([^;]*)"
    ));
    return matches ? decodeURIComponent(matches[1]) : undefined;
}

// устанавливает cookie с именем name и значением value
// options - объект с свойствами cookie (expires, path, domain, secure)
function setCookie(name, value, options) {
    options = options || {};

    var expires = options.expires;

    if (typeof expires == "number" && expires) {
        var d = new Date();
        d.setTime(d.getTime() + expires * 1000);
        expires = options.expires = d;
    }
    if (expires && expires.toUTCString) {
        options.expires = expires.toUTCString();
    }

    value = encodeURIComponent(value);

    var updatedCookie = name + "=" + value;

    for (var propName in options) {
        updatedCookie += "; " + propName;
        var propValue = options[propName];
        if (propValue !== true) {
            updatedCookie += "=" + propValue;
        }
    }

    document.cookie = updatedCookie;
}

// удаляет cookie с именем name
function deleteCookie(name) {
    setCookie(name, "", {
        expires: -1
    });
}

$(function () {
    if (!getCookie("regionName")) {
        $(".region_modal").modal();
    }

    $(".region-search-txt, .region-modal-search-txt").devbridgeAutocomplete({
        minChars: 3,
        serviceUrl: routePrefix + '/Home/SearchRegion',
        onSelect: function (suggestion) {
            var result = suggestion.value.substring(0, suggestion.value.indexOf(" ("));
            $("#select_place .inside").text(result);
            $(".region-search-txt").val(result);
            $(".region_modal").modal("hide");
            setCookie("regionName", result, { expires: 31536000, path: "/" });//year
            setCookie("regionId", suggestion.data, { expires: 31536000, path: "/" });//year
        }
    });

    if (getCookie("regionName")) {
        $("#select_place .inside").text(getCookie("regionName"));
    } else {
        $("#select_place .inside").text("Оберіть місто");
    }
});

window.onload = function () {
    (function (d) {
        var
        ce = function (e, n) { var a = document.createEvent("CustomEvent"); a.initCustomEvent(n, true, true, e.target); e.target.dispatchEvent(a); a = null; return false },
        nm = true, sp = { x: 0, y: 0 }, ep = { x: 0, y: 0 },
        touch = {
            touchstart: function (e) { sp = { x: e.touches[0].pageX, y: e.touches[0].pageY } },
            touchmove: function (e) { nm = false; ep = { x: e.touches[0].pageX, y: e.touches[0].pageY } },
            touchend: function (e) { if (nm) { ce(e, 'fc') } else { var x = ep.x - sp.x, xr = Math.abs(x), y = ep.y - sp.y, yr = Math.abs(y); if (Math.max(xr, yr) > 20) { ce(e, (xr > yr ? (x < 0 ? 'swl' : 'swr') : (y < 0 ? 'swu' : 'swd'))) } }; nm = true },
            touchcancel: function (e) { nm = false }
        };
        for (var a in touch) { d.addEventListener(a, touch[a], false); }
    })(document);

    var l = function (e) {
        console.log(e.type, e);
        if ($('.lmenu_block').hasClass('visible')) {
            $('.lmenu_block').removeClass('visible');
        }
    };
    var r = function (e) {
        console.log(e.type, e);
        if ($('.vertical_menu').hasClass('visible')) {
            $('.vertical_menu').removeClass('visible');
        }
    };

    document.getElementById('left-menu').addEventListener('swl', l, false);
    document.getElementById('right-menu').addEventListener('swr', r, false);
}

$.fn.isolatedScroll = function () {
    this.bind('touchmove touchstart mousewheel DOMMouseScroll', function (e) {
        var delta = e.wheelDelta || (e.originalEvent && e.originalEvent.wheelDelta) || -e.detail,
            bottomOverflow = this.scrollTop + $(this).outerHeight() - this.scrollHeight >= 0,
            topOverflow = this.scrollTop <= 0;

        if ((delta < 0 && bottomOverflow && $(window).width() < 1000) || (delta > 0 && topOverflow && $(window).width() < 1000)) {
            e.preventDefault();
        }
    });
    return this;
};


/*
 * Owl Carousel slider
 *
 */
$('.top_slider').owlCarousel({
    loop: true,
    margin: 0,
    dots: true,
    responsiveClass: true,
    nav: true,
    navText: [
      "<i class='fa fa-angle-left' aria-hidden='true'></i>",
      "<i class='fa fa-angle-right' aria-hidden='true'></i>"
    ],
    items: 1,
    autoplay: true,
    autoplayTimeout: 7000,
    startPosition: '0',
    fluidSpeed: true,
    smartSpeed: 1000
});

/*
 * Owl Carousel products
 *
 */
$('.product_carousel').owlCarousel({
    loop: true,
    items: 2,
    margin: 5,
    dots: false,
    autoWidth: false,
    responsiveClass: true,
    navText: [
      "<i class='fa fa-angle-left' aria-hidden='true'></i>",
      "<i class='fa fa-angle-right' aria-hidden='true'></i>"
    ],
    responsive: {
        0: {
            items: 2,
            nav: false,
            margin: 0
        },
        480: {
            items: 3,
            nav: false,
            margin: 0
        },
        720: {
            items: 3,
            nav: false,
            margin: 0
        },
        800: {
            items: 4,
            nav: false,
            margin: 0
        },
        1000: {
            items: 4,
            nav: false
        },
        1001: {
            items: 2,
            nav: false
        },
        1130: {
            items: 3,
            nav: true
        },
        1366: {
            items: 4,
            nav: true
        },
        1560: {
            items: 5,
            nav: true
        }
    }
});


/*
 * Sub Header Fixed
 *
 */

var winHeight = $(window).scrollTop();
var subheader = $(".sub_header");
var subheaderTopOffset = subheader.offset().top;

$(window).resize(function () {
    if (subheader.hasClass('fixed')) {

    } else {
        subheaderTopOffset = subheader.offset().top;
    }
});

$(window).scroll(function () {
    winHeight = $(window).scrollTop();
    if (winHeight >= (subheaderTopOffset)) {
        subheader.addClass('fixed');
        $('header').addClass('fixed');
    } else if (winHeight < subheaderTopOffset) {
        subheader.removeClass('fixed');
        $('header').removeClass('fixed');
    }

});


/*
* Search block toggle
*
*/
$('.togglesearch').click(function () {
    $('.position_block').removeClass('visible');
    $('.lmenu_block').removeClass('visible');
    $('.vertical_menu').removeClass('visible');
    $('.search_block').toggleClass('visible');

    over4menu();
});

/*
* Position block toggle
*
*/
$('.toggleposition').click(function () {
    $('.search_block').removeClass('visible');
    $('.lmenu_block').removeClass('visible');
    $('.vertical_menu').removeClass('visible');
    $('.position_block').toggleClass('visible');

    over4menu();
});


// var for subheader
var shh = 74;
if ($(window).width() <= 767) {
    shh = 40;
} else if ($(window).width() > 767) {
    shh = 74;
}
$(window).resize(function () {
    if ($(window).width() <= 767) {
        shh = 40;
    } else if ($(window).width() > 767) {
        shh = 74;
    }
});

/*
* Left menu toggle
*
*/
$('.togglerightmenu').click(function () {
    $('.search_block').removeClass('visible');
    $('.position_block').removeClass('visible');
    $('.lmenu_block').removeClass('visible');
    $('.vertical_menu').toggleClass('visible');

    over4menu();
});

$('.empty_over, header, .sub_header').on('touchmove mousewheel DOMMouseScroll', function (e) {
    if ($('.vertical_menu').hasClass('visible')) {
        e.preventDefault();
    }
    if ($('.lmenu_block').hasClass('visible')) {
        e.preventDefault();
    }
});

function over4menu() {
    if ($('.vertical_menu').hasClass('visible')) {
        $('.empty_over').removeClass('hidden');
    } else if ($('.lmenu_block').hasClass('visible')) {
        $('.empty_over').removeClass('hidden');
    } else {
        $('.empty_over').addClass('hidden');
    }
}


$('.vertical_menu, .lmenu_block').isolatedScroll();

if ($(window).width() <= 1000) {

    if (subheader.hasClass('fixed')) {
        $('.vertical_menu').css({
            'top': shh + 'px',
            'height': $(window).height() - shh + 'px'
        });
    } else {
        $('.vertical_menu').css({
            'top': subheader.offset().top + shh,
            'height': $(window).height() - subheader.offset().top - shh + 'px'
        });
        $('.vertical_menu').removeClass('fixed');
    }
}

$(window).scroll(function () {
    if ($(window).width() <= 1000) {
        if (subheader.hasClass('fixed')) {
            $('.vertical_menu').addClass('fixed');
            $('.vertical_menu').css({
                'top': shh + 'px',
                'height': $(window).height() - shh + 'px'
            });
        } else {
            $('.vertical_menu').removeClass('fixed');
            $('.vertical_menu').css({
                'top': subheader.offset().top + shh,
                'height': $(window).height() - subheader.offset().top - shh + 'px'
            });
        }
    }
});


/*
* Left menu toggle
*
*/
$('.toggleleftmenu').click(function () {
    $('.search_block').removeClass('visible');
    $('.position_block').removeClass('visible');
    $('.vertical_menu').removeClass('visible');
    $('.lmenu_block').toggleClass('visible');

    over4menu();
});

if ($(window).width() <= 1000) {
    if (subheader.hasClass('fixed')) {
        $('.lmenu_block').css({
            'top': shh + 'px',
            'height': $(window).height() - shh + 'px'
        });
    } else {
        $('.lmenu_block').css({
            'top': subheader.offset().top + shh,
            'height': $(window).height() - subheader.offset().top - shh + 'px'
        });
        $('.lmenu_block').removeClass('fixed');
    }
}

$(window).scroll(function () {
    if ($(window).width() <= 1000) {
        if (subheader.hasClass('fixed')) {
            $('.lmenu_block').addClass('fixed');
            $('.lmenu_block').css({
                'top': shh + 'px',
                'height': $(window).height() - shh + 'px'
            });
        } else {
            $('.lmenu_block').removeClass('fixed');
            $('.lmenu_block').css({
                'top': subheader.offset().top + shh,
                'height': $(window).height() - subheader.offset().top - shh + 'px'
            });
        }
    }
});

/*
 * Login box
 *
 */
$('.login_links > a').click(function () {
    $('.login_box').addClass('visible');
    $('.lmenu_block').removeClass('visible');
    $('.login_box_over').removeClass('hidden');
});
$('.login_box_over, .login_box > .login_box_header > .close').click(function () {
    $('.login_box').removeClass('visible');
    $('.login_box_over').addClass('hidden');
});


/* text input desctop */
$('.mega-dropdown').on('click', function (event) {
    $(this).parent().toggleClass('open');
});
$('body').on('click', function (e) {
    if (!$('.mega-dropdown').is(e.target)
        && $('.mega-dropdown').has(e.target).length === 0
        && $('.open').has(e.target).length === 0
    ) {
        $('.mega-dropdown').parent().removeClass('open');
    }
});
$(function () {
    $("#select_place").click(function () {
        $(".region-search-txt").show();
        $(".region-search-txt").focus();
        $(this).hide();
    });

    $(".region-search-txt").focusout(function () {
        $("#select_place").show();
        $(this).hide();
    });

    var availableTags = [
      "Глазго",
      "Киев",
      "Братислава",
      "Берлин"
    ];
    $(".select_place_input").autocomplete({
        source: availableTags
    });
});
$('.big_sugestions > span').click(function () {
    $('.select_place_input').val($(this).text());
    $('.big_sugestions').addClass('hidden');
});




// reset
$(window).resize(function () {
    if ($(window).width() > 1000) {
        $('.lmenu_block').removeClass('fixed');
        $('.lmenu_block').attr('style', '');
        $('.vertical_menu').removeClass('fixed');
        $('.vertical_menu').attr('style', '');
        $('.vertical_menu ul li ul').attr('style', '');
    }
});
