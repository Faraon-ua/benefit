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

    if (document.getElementById('left-menu')) {
        document.getElementById('left-menu').addEventListener('swl', l, false);
    }
    if (document.getElementById('right-menu')) {
        document.getElementById('right-menu').addEventListener('swr', r, false);
    }
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

/*organization slider*/
$('.organization_slider').owlCarousel({
    loop: true,
    margin: 0,
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

// tooltip
$('.battery').tooltip();

//structure table
$('body').on('click', '.expand_close', function () {
    $(this).toggleClass('expand_close expand_open');
    $(this).parent().parent().children('ul').toggleClass('hidden');
});
$('body').on('click', '.expand_open', function () {
    $(this).toggleClass('expand_close expand_open');
    $(this).parent().parent().children('ul').toggleClass('hidden');
});

var balls = $(".structure_table_balls");
jQuery.each(balls, function () {
    if (parseInt($(this).html()) >= 500) {
        $(this).addClass('green_color');
    }
});

var bonusLevel = $('.bonusLevel');
jQuery.each(bonusLevel, function () {
    var tmp = $(this).html().split(' ');
    if (tmp[0] === '-') {
        $(this).parent().addClass('pink');
    }
});

var structureLine = $('.structureLine');
jQuery.each(structureLine, function () {
    if ($(this).html() === '1' || $(this).html() === 1) {
        $(this).parent().addClass('bg_pink');
    } else {
        $(this).parent().addClass('bg_grey');
    }
});

var today = new Date();
var currentYear = today.getFullYear();
for (var i = 1901; i <= currentYear; i++) {
    var tmp;
    if (i === currentYear) {
        var tmp = '<option selected>' + i + '</option>';
    } else {
        var tmp = '<option>' + i + '</option>';
    }
    $('.bonuses_invitation_year').append(tmp);
    $('.structre_year').append(tmp);
}

/*organization slider*/
$('.grr').owlCarousel({
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

/*product_description_amount*/
if ($('.product_description_amount').attr("data-is-weight-product") == "true") {
    $('.product_description_amount').mask("#0.0", { reverse: true });
} else {
    $('.product_description_amount').mask("#00");
}
if ($('.product_modal_amount').attr("data-is-weight-product").toLowerCase() == "true") {
    $('.product_modal_amount').mask("#0.0", { reverse: true });
} else {
    $('.product_modal_amount').mask("#00");
}

$('.product_description_amount_plus, .product_description_amount_minus').on('click', function () {
    var valueToAdd = 1;
    var isMinus = $(this).hasClass("product_description_amount_minus");
    var isWeightProduct = $(this).parent().attr("data-weight-product").toLowerCase() === "true";
    if (isWeightProduct) {
        valueToAdd = 0.1;
    }
    var productCurrentValue = parseFloat($('.product_description_amount').val());
    if (isMinus && productCurrentValue > valueToAdd) {
        productCurrentValue = (productCurrentValue - valueToAdd);
    }
    if (!isMinus) {
        productCurrentValue = (productCurrentValue + valueToAdd);
    }
    if (isWeightProduct) {
        productCurrentValue = productCurrentValue.toFixed(1);
    }
    $('.product_description_amount').val(productCurrentValue);
});

/*product_modal_amount*/
$('body').on('click', '.product_modal_plus, .product_modal_minus', function () {
    var productAmount = $(this).parent().children('.product_modal_amount');
    var valueToAdd = 1;
    var isMinus = $(this).hasClass("product_modal_minus");
    var isWeightProduct = false;
    if ($(this).parent().attr("data-weight-product")) {
        isWeightProduct = $(this).parent().attr("data-weight-product").toLowerCase() === "true";
    }
    if (isWeightProduct) {
        valueToAdd = 0.1;
    }
    var productCurrentValue = parseFloat(productAmount.val());
    if (isMinus && productCurrentValue > valueToAdd) {
        productCurrentValue = (productCurrentValue - valueToAdd);
    }
    if (!isMinus) {
        productCurrentValue = (productCurrentValue + valueToAdd);
    }
    if (isWeightProduct) {
        productCurrentValue = productCurrentValue.toFixed(1);
    }
    productAmount.val(productCurrentValue);
    CalculateCartSum();
});
$('body').on("blur", ".product_modal_amount", function () {
    CalculateCartSum();
});

/*basket_modal_table delete product*/
var deleteProductFromCartUrl = routePrefix + '/Cart/RemoveProduct?productId=';
$('body').on('click', '.delete_product', function () {
    var parentRow = $(this).parents('.basket_modal_table_row');
    if (parentRow.hasClass("product")) {
        var productOptions = parentRow.nextUntil(".product");
        productOptions.remove();
    }
    var productId = $(this).attr("data-product-id");
    $.post(deleteProductFromCartUrl + productId, function (data) {
        setCookie("cartNumber", data, { expires: 36000, path: "/" });
        $("#cart-items-number").text(data);
        if (data == 0) {
            $("#cart-items-number").hide();
        }
        parentRow.remove();
        CalculateCartSum();
    });

});

/*categories darker background*/

//$('#categories li, .header_categories_wrap li').on('mouseenter', function() {

$("body").on("mouseenter", "#categories li, .header_categories_wrap li", function () {
    //$('#categories li, .header_categories_wrap li').on('mouseenter', function() {
    $('.darker').removeClass('hidden');
});

//$('.darker, header, .sub_header').on('mouseenter', function() {
$("body").on("mouseenter", ".darker, header, .sub_header", function () {

    $('.darker').addClass('hidden');
});


/*header categorias link*/
$('.header_categories_wrap .header_categories').on('click', function (e) {
    e.preventDefault();
});