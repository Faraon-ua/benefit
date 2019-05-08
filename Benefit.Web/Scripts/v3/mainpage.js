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

    $(".basket-link:not(.no-action), #order-cart, .order-edit").click(function (e) {
        e.preventDefault();
        $("#cart-container").load(cartUrl,
            function () {
                $("#basket_modal").modal('show');
            });
    });

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

var jsControl = new JCTitleSearch2({
    'AJAX_PAGE': '/',
    'CONTAINER_ID': 'title-search',
    'INPUT_ID': 'title-search-input',
    'MIN_QUERY_LEN': 2
});