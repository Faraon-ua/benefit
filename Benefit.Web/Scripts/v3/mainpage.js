$(function () {
    $('#loginPartial').load(loginPartialUrl);

    $(".search-input").devbridgeAutocomplete({
        width: 300,
        minChars: 3,
        serviceUrl: searchWordsUrl,
        onSelect: function (suggestion) {
            window.location.href = searchUrl + "?term=" + suggestion.data;
        }
    });

    //$(".basket-link, #order-cart, .order-edit").click(function (e) {
    //    e.preventDefault();
    //    $("#cart-container").load(cartUrl,
    //        function () {
    //            $("#basket_modal").modal('show');
    //        });
    //});

    $(".region_wrapper, .burger").popover({
        content: $(".region-popover").html(),
        html: true,
        placement: "bottom",
        trigger: "manual",
        container: 'body'
    });

    if (!getCookie("regionName")) {
        $(".region_wrapper").popover('show');
    }
});

var arNextOptions = ({
    "SITE_DIR": "/",
    "SITE_ID": "s1",
    "SITE_ADDRESS": "/",
    "FORM": ({
        "ASK_FORM_ID": "ASK",
        "SERVICES_FORM_ID": "SERVICES",
        "FEEDBACK_FORM_ID": "FEEDBACK",
        "CALLBACK_FORM_ID": "CALLBACK",
        "RESUME_FORM_ID": "RESUME",
        "TOORDER_FORM_ID": "TOORDER"
    }),
    "PAGES": ({
        "FRONT_PAGE": "1",
        "BASKET_PAGE": "",
        "ORDER_PAGE": "",
        "PERSONAL_PAGE": "",
        "CATALOG_PAGE": "",
        "CATALOG_PAGE_URL": "/catalog/",
        "BASKET_PAGE_URL": "/basket/",
    }),
    "PRICES": ({
        "MIN_PRICE": "1000",
    }),
    "THEME": ({
        'THEME_SWITCHER': 'Y',
        'BASE_COLOR': '5',
        'BASE_COLOR_CUSTOM': 'b41818',
        'TOP_MENU': '',
        'TOP_MENU_FIXED': 'Y',
        'COLORED_LOGO': 'Y',
        'SIDE_MENU': 'LEFT',
        'SCROLLTOTOP_TYPE': 'ROUND_COLOR',
        'SCROLLTOTOP_POSITION': 'PADDING',
        'CAPTCHA_FORM_TYPE': '',
        'PHONE_MASK': '+7 (999) 999-99-99',
        'VALIDATE_PHONE_MASK': '^[+][7] [(][0-9]{3}[)] [0-9]{3}[-][0-9]{2}[-][0-9]{2}$',
        'DATE_MASK': 'd.m.y',
        'DATE_PLACEHOLDER': 'дд.мм.гггг',
        'VALIDATE_DATE_MASK': '^[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{4}$',
        'DATETIME_MASK': 'd.m.y h:s',
        'DATETIME_PLACEHOLDER': 'дд.мм.гггг чч:мм',
        'VALIDATE_DATETIME_MASK': '^[0-9]{1,2}\.[0-9]{1,2}\.[0-9]{4} [0-9]{1,2}\:[0-9]{1,2}$',
        'VALIDATE_FILE_EXT': 'png|jpg|jpeg|gif|doc|docx|xls|xlsx|txt|pdf|odt|rtf',
        'BANNER_WIDTH': '',
        'BIGBANNER_ANIMATIONTYPE': 'SLIDE_HORIZONTAL',
        'BIGBANNER_SLIDESSHOWSPEED': '5000',
        'BIGBANNER_ANIMATIONSPEED': '600',
        'PARTNERSBANNER_SLIDESSHOWSPEED': '5000',
        'PARTNERSBANNER_ANIMATIONSPEED': '600',
        'ORDER_BASKET_VIEW': 'NORMAL',
        'SHOW_BASKET_ONADDTOCART': 'Y',
        'SHOW_BASKET_PRINT': 'N',
        "SHOW_ONECLICKBUY_ON_BASKET_PAGE": 'Y',
        'SHOW_LICENCE': 'Y',
        'LICENCE_CHECKED': 'N',
        'SHOW_TOTAL_SUMM': 'Y',
        'CHANGE_TITLE_ITEM': 'N',
        'DISCOUNT_PRICE': '',
        'STORES': '',
        'STORES_SOURCE': 'IBLOCK',
        'TYPE_SKU': 'TYPE_1',
        'MENU_POSITION': 'LINE',
        'DETAIL_PICTURE_MODE': 'POPUP',
        'PAGE_WIDTH': '3',
        'PAGE_CONTACTS': '2',
        'HEADER_TYPE': '3',
        'HEADER_TOP_LINE': '',
        'HEADER_FIXED': '2',
        'HEADER_MOBILE': '1',
        'HEADER_MOBILE_MENU': '2',
        'HEADER_MOBILE_MENU_SHOW_TYPE': '',
        'TYPE_SEARCH': 'fixed',
        'PAGE_TITLE': '3',
        'INDEX_TYPE': 'index3',
        'FOOTER_TYPE': '2',
        'PRINT_BUTTON': 'N',
        'EXPRESSION_FOR_PRINT_PAGE': 'Версия для печати',
        'FILTER_VIEW': 'VERTICAL',
        'YA_GOALS': 'N',
        'YA_COUNTER_ID': '111',
        'USE_FORMS_GOALS': 'COMMON',
        'USE_SALE_GOALS': '',
        'USE_DEBUG_GOALS': 'N',

    }),
    "REGIONALITY": ({
        'USE_REGIONALITY': 'Y',
        'REGIONALITY_VIEW': 'POPUP_REGIONS_SMALL',
    }),
    "COUNTERS": ({
        "YANDEX_COUNTER": 1,
        "GOOGLE_COUNTER": 1,
        "YANDEX_ECOMERCE": "N",
        "GOOGLE_ECOMERCE": "N",
        "TYPE": {
            "ONE_CLICK": "Купить в 1 клик",
            "QUICK_ORDER": "Быстрый заказ",
        },
        "GOOGLE_EVENTS": {
            "ADD2BASKET": "addToCart",
            "REMOVE_BASKET": "removeFromCart",
            "CHECKOUT_ORDER": "checkout",
        }
    }),
    "JS_ITEM_CLICK": ({
        "precision": 6,
        "precisionFactor": Math.pow(10, 6)
    })
});

var jsControl = new JCTitleSearch2({
    'AJAX_PAGE': '/',
    'CONTAINER_ID': 'title-search',
    'INPUT_ID': 'title-search-input',
    'MIN_QUERY_LEN': 2
});