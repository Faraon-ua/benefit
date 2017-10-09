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
        var actualPrice = parseFloat($(this).find(".actual-product-price").text());
        var oldPrice = parseFloat($(this).attr("data-original-price"));
        var amount = parseFloat($(this).find(".product_modal_amount").val());
        var productOptionSum = $(this).nextUntil(".product").map(function () {
            var optionPrice = parseFloat($(this).attr("data-original-price"));
            var optionAmount = parseFloat($(this).find(".product_modal_amount").val());
            return optionPrice * optionAmount;
        }).get();
        var actualProductSum = actualPrice * amount + productOptionSum.reduce(function (pv, cv) { return pv + cv; }, 0);
        var oldProductSum = oldPrice * amount + productOptionSum.reduce(function (pv, cv) { return pv + cv; }, 0);
        $(this).find(".actual-product-total").text(actualProductSum.toFixed(2));
        $(this).find(".old-product-total").text(oldProductSum.toFixed(2));
        return { actual: actualProductSum, old: oldProductSum };
    }).get();
    var actualSum = products.reduce(function (pv, cv) {
        return pv + cv.actual;
    }, 0);
    $(".basket_modal_price span").text(actualSum.toFixed(2));
    var oldSum = products.reduce(function (pv, cv) { return pv + cv.old; }, 0);
    if (oldSum > actualSum) {
        $(".basket_modal_saving span").text((oldSum - actualSum).toFixed(2));
    } else {
        $(".basket_modal_saving").hide();
    }
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

    $(".review-comment").click(function (e) {
        e.preventDefault();
        var answerLink = $(this);
        var id = answerLink.attr("data-review-id");
        answerLink.next().load(routePrefix + "/Tovar/GetReviewCommentForm/" + id, function () {
            answerLink.next().removeClass("hidden");
            answerLink.hide();
        });
    });

    $('input:checkbox').prop('checked', false);

    $(document).keyup(function (e) {
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
        var btn = $(this);
        btn.attr("disabled", "disabled");
        var products = $("#cart-container tr.basket_modal_table_row.product").map(function () {
            var id = $(this).attr("data-product-id");
            var available = parseFloat($(this).attr("data-available-amount"));
            var amount = parseFloat($(this).find(".product_modal_amount").val());
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
                AvailableAmount: available,
                OrderProductOptions: productOptions
            };
        }).get();

        var hasExceededAmount = false;
        for (var i = 0; i < products.length; i++) {
            var prod = products[i];
            if (!isNaN(prod.AvailableAmount) && prod.Amount > prod.AvailableAmount) {
                $("#cart-container tr.basket_modal_table_row.product[data-product-id=" + prod.ProductId + "]").addClass("bg-danger");
                hasExceededAmount = true;
            } else {
                $("#cart-container tr.basket_modal_table_row.product[data-product-id=" + prod.ProductId + "]").removeClass("bg-danger");
            }
        }
        if (hasExceededAmount) {
            alert("Замовлена кількість товарів перевищує доступну кількість");
            btn.removeAttr("disabled");
            return;
        }

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