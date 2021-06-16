var routePrefix = "/Benefit.Web";
//var routePrefix = "";

function setCartSummary(data) {
    setCookie("cartNumber", data.ProductsNumber, { expires: 21600, path: "/" });
    setCookie("cartPrice", data.Price.toFixed(2), { expires: 21600, path: "/" });
    $(".cart-items-number").parent().removeClass("disabled");
    $(".cart-items-number").text(data.ProductsNumber);
    $("#cart-items-price").text(data.Price.toFixed(2) + " грн");
    $(".cart-summary").show();
    $(".basket-link .svg").css("opacity", "1");
    $(".cart-items-number").css("background-color", "#e52929");
    if (data.ProductsNumber == 0) {
        $(".cart-summary").hide();
    }
}

function setFavorites(number) {
    $(".favorites-number").text(number);
    if (number == 0) {
        $(".favorites-number").css("background-color", "#b0b0b0");
    }
    else {
        $(".favorites-number:not(.no-red)").css("background-color", "#e52929");
        $(".favorites-number").siblings(".svg").css("opacity", "1");
        $(".favorites-number").parents("a").removeClass("disabled");
        $(".favorites-number").parents("a").unbind("click");
    }
}

// удаляет cookie с именем name
function deleteCookie(name) {
    setCookie(name, "", {
        expires: -1
    });
}

function CalculateCartSum(sellerId) {
    var wrap = $(".order-wrap[data-seller-id=" + sellerId + "]");
    var userDiscount = parseFloat(wrap.attr("data-seller-userdiscount"));
    var products = wrap.find(".basket_modal_table_row.product").map(function () {
        var actualPrice = parseFloat($(this).find(".actual-product-price").text());
        var oldPrice = parseFloat($(this).attr("data-original-price"));
        var amount = parseFloat($(this).find(".quantity").val());
        var productOptionSum = $(this).nextUntil(".product").map(function () {
            var optionPrice = parseFloat($(this).attr("data-original-price"));
            var optionAmount = parseFloat($(this).find(".quantity").val());
            return optionPrice * optionAmount;
        }).get();
        var actualProductSum = (actualPrice + productOptionSum.reduce(function (pv, cv) { return pv + cv; }, 0)) * amount;
        var oldProductSum = (oldPrice + productOptionSum.reduce(function (pv, cv) { return pv + cv; }, 0)) * amount;
        $(this).find(".actual-product-total").text(actualProductSum.toFixed(2));
        $(this).find(".old-product-total").text(oldProductSum.toFixed(2));
        return { actual: actualProductSum, old: oldProductSum };
    }).get();
    var actualSum = products.reduce(function (pv, cv) {
        return pv + cv.actual;
    }, 0);
    wrap.find(".basket_modal_bonus span").text((actualSum * userDiscount / 100).toFixed(2));
    wrap.find(".basket_modal_price span").text(actualSum.toFixed(2));
    var oldSum = products.reduce(function (pv, cv) { return pv + cv.old; }, 0);
    if (oldSum > actualSum) {
        wrap.find(".basket_modal_saving span").text((oldSum - actualSum).toFixed(2));
    } else {
        wrap.find(".basket_modal_saving").hide();
    }
}

function processRating(ratingStars, e, isClick) {
    var parentOffset = ratingStars.offset();
    var relX = e.pageX - parentOffset.left;
    var percent = relX / ratingStars.width();
    var intRating = 0;
    var ratingBar = ratingStars.find(".x-rating__bar");
    if (percent >= 0 && percent <= 0.2) {
        ratingBar.attr("style", "width: 20%");
        intRating = 1;
    }
    if (percent > 0.2 && percent <= 0.4) {
        ratingBar.attr("style", "width: 40%");
        intRating = 2;
    }
    if (percent > 0.4 && percent <= 0.6) {
        ratingBar.attr("style", "width: 60%");
        intRating = 3;
    }
    if (percent > 0.6 && percent <= 0.8) {
        ratingBar.attr("style", "width: 80%");
        intRating = 4;
    }
    if (percent > 0.8 && percent <= 1) {
        ratingBar.attr("style", "width: 100%");
        intRating = 5;
    }
    if (isClick) {
        $(".rating-value").val(intRating);
    }
}

$(function () {
    $("img[data-src]").each(function () {
        $(this).attr("src", $(this).attr("data-src"));
    });

    $('body').on('click', '.add-to-favorites', function (e) {
        e.preventDefault();
        var btn = $(this);
        var productId = btn.attr("data-product-id");
        $.post(addToFavoritesUrl + "&productId=" + productId,
            null,
            function (data) {
                if (data.count !== undefined) {
                    if (typeof sellerUrlName === 'undefined') {
                        alert(data.message);
                    }
                    setCookie("favoritesNumber", data.count, { expires: 31536000, path: "/", domain: "." + location.host.replace(data.sellerurl + ".", "") });
                    if (typeof sellerUrlName !== 'undefined') {
                        setCookie(sellerUrlName + "-favoritesNumber",
                            data.sellercount,
                            { expires: 31536000, path: "/", domain: "." + location.host.replace(sellerUrlName + ".", "") });
                    }
                    $(".remove-from-favorites").show();
                    var favLink = $(".add-to-favorites[data-product-id=" + productId + "]");
                    if (favLink.hasClass("fav-switch")) {
                        favLink.removeClass("add-to-favorites");
                        favLink.addClass("remove-from-favorites");
                        favLink.addClass("selected");
                    } else {
                        favLink.hide();
                    }
                    $(".seller-favorites-number").text(data.sellercount);
                    setFavorites(data.count);
                } else {
                    $.post(showMessagePopupUrl,
                        { message: data.message, showButtons: false },
                        function (popupHtml) {
                            $(".modal-container").html(popupHtml);
                            $(".message-modal").modal();
                        }
                    );
                }
            });
    });

    $(".remove-from-favorites").click(function (e) {
        e.preventDefault();
        var link = $(this);
        var id = link.attr("data-product-id");
        $.post(removeFromFavoritesUrl + "?productId=" + id,
            null,
            function (data) {
                //alert(data.message);
                if (data.count !== undefined) {
                    setCookie("favoritesNumber", data.count, { expires: 31536000, path: "/", domain: "." + location.host.replace(data.sellerurl + ".", "") });
                    setCookie(data.sellerurl + "-favoritesNumber",
                        data.sellercount,
                        { expires: 31536000, path: "/", domain: "." + location.host.replace(data.sellerurl + ".", "") });
                    var favLink = $(".remove-from-favorites[data-product-id=" + id + "]");
                    if (favLink.hasClass("fav-switch") && location.href.indexOf("favorites") < 0) {
                        favLink.addClass("add-to-favorites");
                        favLink.removeClass("remove-from-favorites");
                        favLink.removeClass("selected");
                    } else {
                        favLink.hide();
                        link.parents(".product-item").remove();
                    }
                    $(".add-to-favorites").show();
                    setFavorites(data.count);
                    $(".seller-favorites-number").text(data.sellercount);
                }
            });
    });

    $("body").on("click", ".show-more-options", function (e) {
        e.preventDefault();
        $(this).parent().prev().css("max-height", "");
        $(this).parent().find(".show-less-options").show();
        $(this).hide();
    });
    $("body").on("click", ".show-less-options", function (e) {
        e.preventDefault();
        $(this).parent().prev().css("max-height", "215px");
        $(this).parent().find(".show-more-options").show();
        $(this).hide();
    });

    $(".submit-review").click(function () {
        var parent = $(this).parents(".review-form");
        var productId = parent.find("input[name=ProductId]").val();
        var sellerId = parent.find("input[name=SellerId]").val();
        var rating = parent.find("#Rating").val();
        var message = parent.find("#Message").val();
        $.post(submitReviewUrl,
            {
                Message: message,
                Rating: rating,
                ProductId: productId,
                SellerId: sellerId
            },
            function (data) {
                if (data.error) {
                    flashMessage(data.error, true, true);
                } else {
                    location.href = location.href;
                }
            });
    });

    $(".section-menu").hover(function () {
        $("#bg").css("background", "rgba(0, 0, 0, 0.3)");
        $("#bg").css("z-index", "100");
    }, function () {
        $("#bg").css("background", "rgba(0, 0, 0, 0)");
        $("#bg").css("z-index", "-1");
    });

    $('body').on('click focusout', '.basket_modal .plus, .basket_modal .minus, .basket_modal .quantity', function () {
        var completeOrderBtn = $(this).parents(".order-wrap").find(".save-order");
        completeOrderBtn.attr("disabled", "disabled");
        var tr = $(this).parents("tr, li");
        var productId = tr.attr("data-product-id");
        var sellerId = tr.attr("data-seller-id");
        var productAmount = $(this).parent().children('.quantity');
        var productCurrentValue = parseFloat(productAmount.val());
        var retailPrice = parseFloat(tr.attr('data-original-price'));
        var wholesalePrice = parseFloat(tr.attr('data-wholesale-price'));
        var wholesaleAmount = parseFloat(tr.attr('data-wholesale-from'));
        if (!$(this).hasClass("quantity")) {
            var valueToAdd = 1;
            var isMinus = $(this).hasClass("minus");
            var isWeightProduct = false;
            if ($(this).parent().attr("data-weight-product")) {
                isWeightProduct = $(this).parent().attr("data-weight-product").toLowerCase() === "true";
            }
            if (isWeightProduct) {
                valueToAdd = 0.1;
            }
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
        }
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

        var updateProducQuantityUrl = routePrefix + '/Cart/UpdateQuantity';
        $.post(updateProducQuantityUrl,
            {
                productId: productId,
                sellerId: sellerId,
                amount: productCurrentValue
            },
            function (data) {
                completeOrderBtn.removeAttr("disabled");
                setCartSummary(data);
            });
        CalculateCartSum(sellerId);
    });

    $('body').on("blur", ".basket_modal .quantity", function () {
        var sellerId = $(this).parents("tr").attr("data-seller-id");
        CalculateCartSum(sellerId);
    });

    $("body").on("focus",
        ".quantity",
        function () {
            if ($(this).attr("data-weight-product").toLowerCase() === "true") {
                $(this).mask("#9.99", { reverse: true });
            } else {
                $(this).mask("#00");
            }
        });

    var deleteProductFromCartUrl = routePrefix + '/Cart/RemoveProduct';
    $('body').on('click', '.delete_product', function (e) {
        e.preventDefault();
        var sellerId = $(this).parents("tr, li").attr("data-seller-id");
        var parentRow = $(this).parents('.basket_modal_table_row');
        if (parentRow.hasClass("product")) {
            var productOptions = parentRow.nextUntil(".product");
            productOptions.remove();
        }
        var productId = $(this).attr("data-product-id");
        $.post(deleteProductFromCartUrl,
            {
                productId: productId,
                sellerId: sellerId
            }, function (data) {
                setCartSummary(data);
                var orderWrap = parentRow.parents(".order-wrap");
                parentRow.remove();
                if (orderWrap.find("tbody tr").length === 0) {
                    orderWrap.remove();
                }
                CalculateCartSum(sellerId);
            });
    });

    if ($.fn.tooltip) {
        $('.battery, .title-to-tooltip').tooltip();
    }

    $(".review-rating").mousemove(function (e) {
        if (!$(".rating-value").val()) {
            processRating($(this), e);
        }
    });

    $(".review-rating").mouseout(function () {
        if (!$(".rating-value").val()) {
            $(this).find(".x-rating__bar").attr("style", "width: 0;");
        }
    });

    $(".review-rating").click(function (e) {
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

    //$('input:checkbox').prop('checked', false);

    $(document).keyup(function (e) {
        if (e.keyCode == 27 && $.fn.modal) {
            $(".modal").modal("hide");
        }
    });

    $(".search-form").submit(function (e) {
        if ($(this).find(".search-input").val().length < 3) {
            alert("Для пошуку необхідно мінімум 3 символи");
            e.preventDefault();
        }
    });

    $(".search-form button").click(function () {
        var sellerId = $("#seller-id").val();
        if ($(this).attr("data-search-seller") && sellerId !== undefined) {
            $("#search-seller-id").val(sellerId);
        } else {
            $("#search-seller-id").val();
        }
    });

    $("body").on("click", ".parent-cat", function (e) {
        e.preventDefault();
        $(this).next("ul").slideToggle();
    });

    var url = document.location.toString();
    if (url.match('#') && $.fn.tab) {
        $('.nav-tabs a[href="#' + url.split('#')[1] + '"]').tab('show');
    }

    $("body").on("click", ".predefinedRegions a", function (e) {
        e.preventDefault();
        var regionName;
        if ($(this).attr("data-region-name")) {
            regionName = $(this).attr("data-region-name");
        }
        else {
            regionName = $(this).text();
        }
        var regionId = $(this).attr("data-region-id");
        $(".select_place_container .inside").text(regionName);
        $(".region-search-txt").val(regionName);
        $(".region_modal").modal("hide");
        setCookie("regionName", regionName, { expires: 31536000, path: "/" });//year
        setCookie("regionId", regionId, { expires: 31536000, path: "/" });//year
        var selectRegionsWrap = $("#selectRegionWrap");
        selectRegionsWrap.load(selectRegionsWrap.attr("data-url"));
        $(".region_modal").modal("hide");
        if (selectedProductId !== null) {
            $(".product_buy").each(function () {
                if ($(this).attr("data-purchase-region") && $(this).attr("data-purchase-region").indexOf(regionName) > -1) {
                    $(this).attr("data-purchase-region", "");
                    $(this).attr("data-purchase-region-id", "");
                }
            })
            $(".product_buy[data-product-id=" + selectedProductId + "]").click();
            if (typeof otherRegion !== 'undefined') {
                otherRegion = '';
                $("#buy-product").click();
            }
            selectedProductId = null;
            reloadOnProductAdd = true;
        }
    });

    $("body").on("click", ".select_place_container", function () {
        $(".region_modal").modal();
    });

    $("body").on("click", ".region-popover-content button",
        function () {
            if ($(this).attr("data-region-id")) {
                var regionId = $(this).attr("data-region-id");
                var regionName = $(this).attr("data-region-name");
                setCookie("regionName", regionName, { expires: 31536000, path: "/" }); //year
                setCookie("regionId", regionId, { expires: 31536000, path: "/" }); //year
                location.reload();
            } else {
                $(".select_place_container").click();
            }
            $('.region_wrapper').popover('hide');
        });

    if ($.fn.modal) {
        $(".message-modal").modal({ backdrop: 'static', keyboard: false });
    }
    $("body").on("click", ".goto_back", function () {
        $("#basket_modal").modal('hide');
    });

    $("body").on("click", ".save-order", function () {
        var btn = $(this);
        btn.attr("disabled", "disabled");
        var wrap = $(this).parents(".order-wrap");
        var products = wrap.find("tr.basket_modal_table_row.product").map(function () {
            var id = $(this).attr("data-product-id");
            var available = parseFloat($(this).attr("data-available-amount"));
            var nameSuffix = $(this).attr("data-name-suffix");
            var priceGrowth = parseFloat($(this).attr("data-price-growth"));
            var amount = parseFloat($(this).find(".product_modal_amount").val());
            var productOptions = $(this).nextUntil(".product").map(function () {
                var optionId = $(this).attr("data-option-id");
                var optionAmount = $(this).find(".quantity").val();
                return {
                    ProductOptionId: optionId,
                    Amount: optionAmount
                };
            }).get();
            return {
                ProductId: id,
                NameSuffix: nameSuffix,
                PriceGrowth: priceGrowth,
                Amount: amount,
                AvailableAmount: available,
                OrderProductOptions: productOptions
            };
        }).get();

        var hasExceededAmount = false;
        for (var i = 0; i < products.length; i++) {
            var prod = products[i];
            if (!isNaN(prod.AvailableAmount) && prod.Amount > prod.AvailableAmount) {
                wrap.find("tr.basket_modal_table_row.product[data-product-id=" + prod.ProductId + "]").addClass("bg-danger");
                hasExceededAmount = true;
            } else {
                wrap.find("tr.basket_modal_table_row.product[data-product-id=" + prod.ProductId + "]").removeClass("bg-danger");
            }
        }
        if (hasExceededAmount) {
            alert("Замовлена кількість товарів перевищує доступну кількість");
            btn.removeAttr("disabled");
            return;
        }

        var sellerId = wrap.attr("data-seller-id");
        $.post(routePrefix + "/Cart/CompleteOrder?sellerId=" + sellerId,
            { 'orderProducts': products },
            function (data) {
                setCartSummary(data);
                window.location.href = data.redirectUrl;
            });
    });

    if ($.fn.devbridgeAutocomplete) {
        $(".region-search-txt, .region-modal-search-txt").devbridgeAutocomplete({
            width: '500',
            minChars: 3,
            serviceUrl: routePrefix + '/ProductRegions/SearchRegion',
            onSelect: function (suggestion) {
                var result = suggestion.value.substring(0, suggestion.value.indexOf(" ("));
                $(".select_place_container .inside").text(result);
                $(".region-search-txt").val(result);
                $(".region_modal").modal("hide");
                setCookie("regionName", result, { expires: 31536000, path: "/" }); //year
                setCookie("regionId", suggestion.data, { expires: 31536000, path: "/" }); //year
                location.reload();
            }
        });
    }

    if (getCookie("cartNumber")) {
        $(".cart-items-number").text(getCookie("cartNumber"));
        $(".cart-items-number").parent().removeClass("disabled");
        $("#cart-items-price").text(getCookie("cartPrice") + " грн");
        $(".cart-items-number").siblings(".svg").css("opacity", "1");
        if (getCookie("cartNumber") !== "0") {
            $(".cart-items-number").css("background-color", "#e52929");
        }
        $(".cart-summary").show();
    }
    if (typeof sellerUrlName !== 'undefined' && getCookie(sellerUrlName + "-favoritesNumber") !== undefined) {
        $(".seller-favorites-number").text(getCookie(sellerUrlName + "-favoritesNumber"));
    }
    if (getCookie("favoritesNumber")) {
        setFavorites(getCookie("favoritesNumber"));
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