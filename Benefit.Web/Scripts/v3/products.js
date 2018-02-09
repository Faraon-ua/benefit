$(function() {
    $("body").on('click', ".counter .plus, .counter .minus", function () {
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

    $("body").on("focus", ".quantity", function () {
        if ($(this).attr("data-weight-product").toLowerCase() === "true") {
            $(this).mask("#9.99", { reverse: true });
        } else {
            $(this).mask("#00");
        }
    });
})