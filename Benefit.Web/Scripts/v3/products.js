$(function () {
    function CalculateProductPrice() {
        var checkOptions = $(".product_modal_form input[type=checkbox]:checked").map(function () {
            var checkboxPriceGrowth = parseFloat($(this).attr("data-price-growth"));
            var amount = $(this).siblings(".counter").find(".quantity").val();
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

    $('body').on("blur",
        ".product_modal_form .counter",
        function () {
            CalculateCartSum();
        });

    $("body").on("click",
        ".product_modal_form input[type=checkbox], .product_modal_form input[type=radio]",
        function () {
            var id = $(this).attr("id");
            $(".product_modal_form input[data-binded-option-id=" + id + "]").prop('checked', true);
            CalculateProductPrice();
        });

    $("body").on("click",
        ".product_modal_form div.minus, .product_modal_form div.plus",
        function () {
            CalculateProductPrice();
        });

    $("body").on('click',
        ".product_modal_form .plus, .product_modal_form .minus, .product-item .plus, .product-item .minus, .product-page .plus, .product-page .minus",
        function () {
            var quantity = $(this).parent().find('input[name=quantity]');
            var valueToAdd = 1;
            var isMinus = $(this).hasClass("minus");
            var isWeightProduct = quantity.attr("data-weight-product").toLowerCase() === "true";
            if (isWeightProduct) {
                valueToAdd = 0.1;
            }
            var productCurrentValue = parseFloat(quantity.val());
            if (isMinus && productCurrentValue > valueToAdd) {
                productCurrentValue = (productCurrentValue - valueToAdd);
            }
            if (!isMinus) {
                productCurrentValue = (productCurrentValue + valueToAdd);
            }
            if (isWeightProduct) {
                productCurrentValue = productCurrentValue.toFixed(1);
            }
            quantity.val(productCurrentValue);
        });

    $("body").on('click',
        "#buy-product-with-options",
        function () {
            var productId = $(this).attr("data-product-id");
            var sellerId = $(this).attr("data-seller-id");
            AddOrderProduct(1, productId, sellerId, true, false);
        });

    //variants
    $("body").on("click",
        ".variants .variant-item",
        function () {
            $(this).siblings().removeClass("active");
            $(this).addClass("active");
            if ($(this).parents(".variants").find(".variants-group").length === 1) {
                $("#buy-product-with-variants").click();
            }
        });
    //variants

    $("body").on('click',
        "#buy-product-with-variants",
        function() {
            var productNameSufix = "";
            var productPriceGrowth = 0;
            var productId = $(this).attr("data-product-id");
            var sellerId = $(this).attr("data-seller-id");
            var isValid = true;
            var variantsWrap = $(this).parentsUntil(".variants");
            variantsWrap.find(".variants-group").each(function() {
                var selectedVariant = $(this).find(".variant-item.active");
                if (selectedVariant.length === 0) {
                    $(this).find(".validation").show();
                    isValid = false;
                } else {
                    productNameSufix += " " +
                        selectedVariant.attr("data-option-title") +
                        " " +
                        selectedVariant.attr("data-option-name");
                    productPriceGrowth += parseFloat(selectedVariant.attr("data-price-growth"));
                }
            });
            if (isValid) {
                AddOrderProduct(1,
                    productId,
                    sellerId,
                    true,
                    false,
                    productNameSufix,
                    productPriceGrowth);
                $(".modal.variants").modal('hide');
            }
        });

    $("body").on("click",
        ".product_buy",
        function (e) {
            e.preventDefault();
            if ($(this).attr("disabled")) {
                return;
            }
            var productId = $(this).attr("data-product-id");
            var isWeightProduct = $(this).attr("data-is-weight-product").toLowerCase() === 'true';
            var sellerId = $(this).attr("data-seller-id");
            var amount = $(this).parent().find(".counter").find(".quantity").val();
            if (!amount) {
                amount = 1;
            }
            $.get(productOptionsUrl + "?productId=" + productId,
                function (data) {
                    if (data) {
                        $("#product-options-wrap").html(data);
                        $("#product_modal").modal('show');
                    } else {
                        $.get(productVariantsUrl + "?productId=" + productId,
                            function (data) {
                                if (data) {
                                    $("#product-variants-wrap").html(data);
                                    $(".variants.modal").modal('show');
                                } else {
                                    AddOrderProduct(amount, productId, sellerId, false, isWeightProduct);
                                }
                            });
                    }
                });

        });
});

function AddOrderProduct(amount, productId, sellerId, hasOptions, isWeightProduct, nameSuffix, priceGrowth) {
    $("#" + productId).css("opacity", 0.3);
    var productAmount = amount;
    var productOptions;
    if (hasOptions) {
        productOptions = $(".product_modal_form input[type=checkbox]:checked, .product_modal_form input[type=radio]:checked").map(function () {
            var id = $(this).attr("id");
            var amount = $(this).siblings(".counter").find(".quantity").val();
            if (!amount) {
                amount = 1;
            }
            return {
                ProductOptionId: id,
                Amount: amount
            };
        }).get();
    } else {
        productOptions = [];
    }

    var product = {
        ProductId: productId,
        Amount: productAmount,
        IsWeightProduct: isWeightProduct,
        OrderProductOptions: productOptions,
        NameSuffix: nameSuffix,
        PriceGrowth: priceGrowth,
    };

    var order = {
        product: product,
        amount: productAmount
    };
    $.post(addToCartUrl + "?sellerId=" + sellerId,
        order,
        function (data) {
            if ($.fn.modal) {
                $('#product_modal').modal('hide');
            }
            setTimeout(function () {
                $("#buy-product, #buy-product-with-options").removeAttr('disabled');
                $("#" + productId).css("opacity", 1);
                $(".product_buy").removeClass("loadings");
                setCartSummary(data);
                if (typeof showCartOnOrder !== 'undefined' && showCartOnOrder) {
                    $(".basket-link:not(.no-action)").eq(0).click();
                }
            }, 1000);
        });
}