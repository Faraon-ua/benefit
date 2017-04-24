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

function setCartSummary(data) {
    setCookie("cartNumber", data.ProductsNumber, { expires: 36000, path: "/" });
    setCookie("cartPrice", data.Price.toFixed(2), { expires: 36000, path: "/" });
    $("#cart-items-number").text(data.ProductsNumber + " шт.");
    $("#cart-items-price").text(data.Price.toFixed(2) + " грн");
    /*if (data.IsFreeShipping) {
        $("#cart-items-price").removeClass();
        $("#cart-items-price").addClass("text-success");
    } else {
        $("#cart-items-price").removeClass();
        $("#cart-items-price").addClass("text-danger");
    }*/
    $(".cart-summary").show();

    if (data.ProductsNumber == 0) {
        $(".cart-summary").hide();
    }
}

// удаляет cookie с именем name
function deleteCookie(name) {
    setCookie(name, "", {
        expires: -1
    });
}

function CalculateCartSum() {
    var products = $(".basket_modal tr.basket_modal_table_row.product").map(function () {
        //", tr.basket_modal_table_row.option product-total-price"
        var price = parseFloat($(this).attr("data-original-price"));
        var amount = parseFloat($(this).find(".product_modal_amount").val());
        var productOptionSum = $(this).nextUntil(".product").map(function () {
            var optionPrice = parseFloat($(this).attr("data-original-price"));
            var optionAmount = parseFloat($(this).find(".product_modal_amount").val());
            return optionPrice * optionAmount;
        }).get();
        var productSum = price * amount + productOptionSum.reduce(function (pv, cv) { return pv + cv; }, 0);
        $(this).find(".product-total-price").text(productSum.toFixed(2));
        return productSum;
    }).get();
    var sum = products.reduce(function (pv, cv) { return pv + cv; }, 0);
    $(".basket_modal_price span").text(sum.toFixed(2));
}

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

function processRating(ratingStars, e, isClick) {
    var parentOffset = ratingStars.offset();
    var relX = e.pageX - parentOffset.left;
    var percent = relX / ratingStars.width();
    ratingStars.removeClass();
    ratingStars.addClass("pointer");
    ratingStars.addClass("rating_star");
    var intRating = 0;
    if (percent >= 0 && percent <= 0.2) {
        ratingStars.addClass("very_bed");
        intRating = 1;
    }
    if (percent > 0.2 && percent <= 0.4) {
        ratingStars.addClass("bed");
        intRating = 2;
    }
    if (percent > 0.4 && percent <= 0.6) {
        ratingStars.addClass("middle");
        intRating = 3;
    }
    if (percent > 0.6 && percent <= 0.8) {
        ratingStars.addClass("good");
        intRating = 4;
    }
    if (percent > 0.8 && percent <= 1) {
        ratingStars.addClass("top");
        intRating = 5;
    }
    if (isClick) {
        $("#Rating").val(intRating);
    }
}

$(function () {
    $("#review-rating").mousemove(function (e) {
        processRating($(this), e);
    });

    $("#review-rating").mouseout(function () {
        $(this).removeClass();
        $(this).addClass("pointer");
        $(this).addClass("rating_star");
        $(this).addClass("none");
    });

    $("#review-rating").click(function (e) {
        processRating($(this), e, true);
        $("#review-rating").off("mousemove");
        $("#review-rating").off("mouseout");
    });

    $('input:checkbox').prop('checked', false);

    $(document).keyup(function(e) {
        if (e.keyCode == 27) {
            $(".modal").modal("hide");
        }
    });

    $("#search-form").submit(function (e) {
        if ($("#search").val().length < 3) {
            alert("Для пошуку необхідно мінімум 3 символи");
            e.preventDefault();
        }
    });

    $("body").on("click", ".parent-category", function () {
        $(this).next("ul").slideToggle();
    });

    $("body").on("click", ".product_modal_form input[type=checkbox], .product_modal_form input[type=radio], .product_modal_form div.product_modal_minus, .product_modal_form div.product_modal_plus", function () {
        CalculateProductPrice();
    });

    var url = document.location.toString();
    if (url.match('#')) {
        $('.nav-tabs a[href="#' + url.split('#')[1] + '"]').tab('show');
    }
    $('.nav-tabs a').on('shown.bs.tab', function (e) {
        window.location.hash = e.target.hash;
    });

    $("#predefinedRegions a").click(function () {
        var regionName = $(this).text();
        var regionId = $(this).attr("data-region-id");
        $(".select_place_container .inside").text(regionName);
        $(".region-search-txt").val(regionName);
        $(".region_modal").modal("hide");
        setCookie("regionName", regionName, { expires: 31536000, path: "/" });//year
        setCookie("regionId", regionId, { expires: 31536000, path: "/" });//year
        location.reload();
    });

    $("body").on("click", ".goto_back", function () {
        $("#basket_modal").modal('hide');
    });

    $("body").on("click", "#complete-order", function () {
        $(this).attr("disabled", "disabled");
        var products = $("#cart-container tr.basket_modal_table_row.product").map(function () {
            var id = $(this).attr("data-product-id");
            var amount = $(this).find(".product_modal_amount").val();
            var productOptions = $(this).nextUntil(".product").map(function () {
                var optionId = $(this).attr("data-option-id");
                var optionAmount = $(this).find(".product_modal_amount").val();
                return {
                    ProductOptionId: optionId,
                    Amount: optionAmount
                };
            }).get();
            return {
                ProductId: id,
                Amount: amount,
                OrderProductOptions: productOptions
            };
        }).get();

        var sellerId = $("#basket_modal").attr("data-seller-id");
        $.post(routePrefix + "/Cart/CompleteOrder?sellerId=" + sellerId,
            { 'orderProducts': products },
            function (data) {
                setCartSummary(data);
                window.location.href = data.redirectUrl;
            });
    });

    $("body").on("click", "#continue-purchase", function () {
        var products = $("#cart-container tr.basket_modal_table_row.product").map(function () {
            var id = $(this).attr("data-product-id");
            var amount = $(this).find(".product_modal_amount").val();
            var productOptions = $(this).nextUntil(".product").map(function () {
                var optionId = $(this).attr("data-option-id");
                var optionAmount = $(this).find(".product_modal_amount").val();
                return {
                    ProductOptionId: optionId,
                    Amount: optionAmount
                };
            }).get();
            return {
                ProductId: id,
                Amount: amount,
                OrderProductOptions: productOptions
            };
        }).get();

        var sellerId = $("#basket_modal").attr("data-seller-id");
        $.post(routePrefix + "/Cart/CompleteOrder?sellerId=" + sellerId,
            { 'orderProducts': products },
            function (data) {
                setCartSummary(data);
            }
        );
    });

    if (!getCookie("regionName")) {
        $(".region_modal").modal();
    }
    $(".select_place_container").click(function () {
        $(".region_modal").modal();
    });

    $(".region-search-txt, .region-modal-search-txt").devbridgeAutocomplete({
        width: '500',
        minChars: 3,
        serviceUrl: routePrefix + '/Home/SearchRegion',
        onSelect: function (suggestion) {
            var result = suggestion.value.substring(0, suggestion.value.indexOf(" ("));
            $(".select_place_container .inside").text(result);
            $(".region-search-txt").val(result);
            $(".region_modal").modal("hide");
            setCookie("regionName", result, { expires: 31536000, path: "/" });//year
            setCookie("regionId", suggestion.data, { expires: 31536000, path: "/" });//year
            location.reload();
        }
    });

    if (getCookie("cartNumber")) {
        $("#cart-items-number").text(getCookie("cartNumber") + " шт.");
        $("#cart-items-price").text(getCookie("cartPrice") + " грн");
        $(".cart-summary").show();
    }

    if (getCookie("regionName")) {
        $(".select_place_container .inside").text(getCookie("regionName"));
    } else {
        $(".select_place_container .inside").text("Оберіть місто");
    }

    $("body").on("click", ".structure_table_register_email input", function () {
        $(this).select();
    });

    $(".structure_table_balls").click(function () {
        $(".structure_table_wrap ul").each(function () {
            var currentUl = $(this);
            currentUl.children("li").not(".structure_table_head").sort(function (a, b) {
                var aFloat = parseFloat($(a).find(".structure_table_balls").text());
                var bFloat = parseFloat($(b).find(".structure_table_balls").text());
                return (aFloat > bFloat) ? -1 : (aFloat < bFloat) ? 1 : 0;
            }).appendTo(currentUl);
        });
    });

    $(".structure_table_bonuses").click(function () {
        $(".structure_table_wrap ul").each(function () {
            var currentUl = $(this);
            currentUl.children("li").not(".structure_table_head").sort(function (a, b) {
                var aFloat = parseFloat($(a).find(".structure_table_bonuses").text());
                var bFloat = parseFloat($(b).find(".structure_table_bonuses").text());
                return (aFloat > bFloat) ? -1 : (aFloat < bFloat) ? 1 : 0;
            }).appendTo(currentUl);
        });
    });

    $("#showAllStructure").change(function () {
        var levels = $(".structure_table_wrap li").map(function () {
            return parseInt($(this).attr("data-structure-level"));
        }).get();
        if (this.checked) {
            $(".expand_close").click();
        } else {
            $(".expand_open").click();
        }
    });

    $("#hasNoCard").change(function () {
        if (this.checked) {
            $(".structure_table_wrap li").not(".structure_table_head").filter(function () {
                return $(this).find(".structure_table_register_card_number").text() != "";
            }).hide();
        } else {
            $(".structure_table_wrap li").not(".structure_table_head").filter(function () {
                return $(this).find(".structure_table_register_card_number").text() != "";
            }).show();
        }
    });
});