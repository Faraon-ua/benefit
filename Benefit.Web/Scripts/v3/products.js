$(function () {
    $("body").on('click',
        ".product-item .plus, .product-item .minus, .product-page .plus, .product-page .minus",
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

    $("body").on('click', "#buy-product-with-options", function () {
        var productId = $(this).attr("data-product-id");
        var sellerId = $(this).attr("data-seller-id");
        AddOrderProduct(productId, sellerId, true, false);
    });

    $("body").on("click",
        ".product_buy",
        function (e) {
            e.preventDefault();
            var productId = $(this).attr("data-product-id");
            var isWeightProduct = $(this).attr("data-is-weight-product").toLowerCase() === 'true';
            var sellerId = $(this).attr("data-seller-id");
            var amount = $(this).prev(".counter").find(".quantity").val();
            $.get(productOptionsUrl + "?productId=" + productId,
                function (data) {
                    if (data) {
                        $("#product-options-wrap").html(data);
                        $("#product_modal").modal('show');
                    } else {
                        AddOrderProduct(amount, productId, sellerId, false, isWeightProduct);
                    }
                });
        });
});

function AddOrderProduct(amount, productId, sellerId, hasOptions, isWeightProduct) {
    $("#" + productId).css("opacity", 0.3);
    var productAmount = amount;
    var productOptions;
    if (hasOptions) {
        productOptions = $(".product_modal_form input[type=checkbox]:checked, .product_modal_form input[type=radio]:checked").map(function () {
            var id = $(this).attr("id");
            var amount = $(this).siblings(".modal_amount_wrap").find(".product_modal_amount").val();
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
        OrderProductOptions: productOptions
    };

    var order = {
        product: product,
        amount: productAmount
    };
    $.post(addToCartUrl + "?sellerId=" + sellerId,
        order,
        function (data) {
            $('#product_modal').modal('hide');
            setTimeout(function () {
                $("#buy-product, #buy-product-with-options").removeAttr('disabled');
                $("#" + productId).css("opacity", 1);
                $(".product_buy").removeClass("loadings");
                setCartSummary(data);
            }, 1000);
        });
}