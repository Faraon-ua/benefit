function SetRegionsAutocomplete() {
    $(".regionSearch").devbridgeAutocomplete({
        minChars: 3,
        serviceUrl: routePrefix + '/Home/SearchRegion',
        onSelect: function (suggestion) {
            $(this).val(suggestion.value);
            $(this).next().val(suggestion.data);
        }
    });
}

function UpdateAddressesNumber() {
    var maxNumber = GetMaxAttributeValue('.address-id', 'data-number');
    var href = $("#addNewAddress").attr("href");
    href = href.substring(0, href.length - 1) + (maxNumber + 1);
    $("#addNewAddress").attr("href", href);
    SetRegionsAutocomplete();
}

$(function () {
    $("#addresses").on("click", ".removeAddress", function () {
        $(this).parent().parent().remove();
    });
});