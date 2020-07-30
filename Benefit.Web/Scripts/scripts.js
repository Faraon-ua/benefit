function CalculateProductPrice() {
    var checkOptions = $(".product_modal_form input[type=checkbox]:checked").map(function () {
        var checkboxPriceGrowth = parseFloat($(this).attr("data-price-growth"));
        var amount = $(this).siblings(".modal_amount_wrap").find(".product_modal_amount").val();
        return checkboxPriceGrowth * amount;
    }).get();
    var radioOptions = $(".product_modal_form input[type=radio]:checked").map(function () {
        return parseFloat($(this).attr("data-price-growth"));
    }).get();
    var checkPriceGrowth = checkOptions.reduce(function (pv, cv) { return pv + cv; }, 0);
    var radioPriceGrowth = radioOptions.reduce(function (pv, cv) { return pv + cv; }, 0);
    var originalPrice = parseFloat($(".product-price").attr("data-original-price"));
    $(".product-price").text((originalPrice + checkPriceGrowth + radioPriceGrowth).toFixed(2));
}

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

$(document).ready(function () {
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
   /* $('.top_slider').owlCarousel({
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

    /*organization slider#1#
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
     #1#
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
    });*/


    /*
     * Sub Header Fixed
     *
     */

    //var winHeight = $(window).scrollTop();
    //var subheader = $(".sub_header");
    //var subheaderTopOffset = subheader.offset().top;

    //$(window).resize(function () {
    //    if (subheader.hasClass('fixed')) {

    //    } else {
    //        subheaderTopOffset = subheader.offset().top;
    //    }
    //});

    //$(window).scroll(function () {
    //    winHeight = $(window).scrollTop();
    //    if (winHeight >= (subheaderTopOffset)) {
    //        subheader.addClass('fixed');
    //        $('header').addClass('fixed');
    //    } else if (winHeight < subheaderTopOffset) {
    //        subheader.removeClass('fixed');
    //        $('header').removeClass('fixed');
    //    }

    //});


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
        $(".region_modal").modal();
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

    /*
     * Login box
     *
     */
    $('.login_links > a').click(function () {
        return;
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
    $('.battery, .title-to-tooltip').tooltip();

    //structure table
    $('body').on('click', '.expand_close', function () {
        $(this).toggleClass('expand_close expand_open');
        $(this).parent().parent().children('ul').toggleClass('hidden');
    });
    $('body').on('click', '.expand_open', function () {
        $(this).toggleClass('expand_close expand_open');
        $(this).parent().parent().children('ul').toggleClass('hidden');
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
    if (typeof owlCarousel !== 'undefined') {
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
    }

    /*product_description_amount*/
    if ($('.product_description_amount').length > 0) {
        if ($('.product_description_amount').attr("data-is-weight-product") == "true") {
            $('.product_description_amount').mask("#9.99", { reverse: true });
        } else {
            $('.product_description_amount').mask("#00");
        }
    }

    /*product_modal_amount*/
    $('body').on('click', '.product_modal_plus, .product_modal_minus', function () {
        var tr = $(this).parents("tr");
        var sellerId = tr.attr("data-seller-id");
        var productAmount = $(this).parent().children('.product_modal_amount');
        var retailPrice = parseFloat(tr.attr('data-original-price'));
        var wholesalePrice = parseFloat(tr.attr('data-wholesale-price'));
        var wholesaleAmount = parseFloat(tr.attr('data-wholesale-from'));
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

        if (wholesaleAmount !== 0) {
            if (productCurrentValue >= wholesaleAmount) {
                tr.find(".wholesale-hint").hide();
                $(".basket_modal_saving").show();
                tr.find(".actual-product-price").text(wholesalePrice.toFixed(2));
                tr.find(".old-product-price").show();
                tr.find(".old-product-total").show();
            } else {
                $(".basket_modal_saving").hide();
                tr.find(".wholesale-hint").show();
                tr.find(".actual-product-price").text(retailPrice.toFixed(2));
                tr.find(".old-product-price").hide();
                tr.find(".old-product-total").hide();
            }
        }
        CalculateCartSum(sellerId);
    });

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


    $('body').on("blur", ".product_modal_amount", function () {
        CalculateCartSum();
    });

    $("body").on("click", ".product_modal_form input[type=checkbox], .product_modal_form input[type=radio]", function () {
        var id = $(this).attr("id");
        $(".product_modal_form input[data-binded-option-id=" + id + "]").prop('checked', true);
        CalculateProductPrice();
    });

    $("body").on("click", ".product_modal_form div.product_modal_minus, .product_modal_form div.product_modal_plus", function () {
        CalculateProductPrice();
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


    /*kabinet partnera finance mobile*/
    $('.finance .profile_menu').on('click', function () {
        $('.partner_menu, .profile_menu, .finance_bonuses_mobile').addClass('finance_mobile_hidden');
        $('.tab-content').removeClass('finance_mobile_hidden');
    });

    $('.finance_tab_title').on('click', function () {
        $('.partner_menu, .profile_menu, .finance_bonuses_mobile').removeClass('finance_mobile_hidden');
        $('.tab-content').addClass('finance_mobile_hidden');
    });


    /*bootstrap carousel swipe*/
    try {
        $(".carousel-inner").swipe({
            swipeLeft: function (event, direction, distance, duration, fingerCount) {
                $(this).parent().carousel('next');
            },
            swipeRight: function () {
                $(this).parent().carousel('prev');
            },
            //Default is 75px, set to 0 for demo so any distance triggers swipe
            threshold: 0
        });
    }
    catch (exception) { }
});
