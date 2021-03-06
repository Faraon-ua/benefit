﻿function SetRegionsAutocomplete() {
    var regionSearchUrl = '/ProductRegions/SearchRegion';
    var regionSearch = $(".regionSearch");
    if (regionSearch.hasClass("includeRegions")) {
        regionSearchUrl = regionSearchUrl + "?minLevel=1";
    }
    $(".regionSearch").devbridgeAutocomplete({
        minChars: 3,
        serviceUrl: routePrefix + regionSearchUrl,
        onSelect: function (suggestion) {
            $(this).val(suggestion.value);
            $(this).next().val(suggestion.data);
        }
    });
}

function UpdateAddressesNumber() {
    var maxNumber = parseInt(GetMaxAttributeValue('.address-id', 'data-number'));
    var href = $("#addNewAddress").attr("href");
    href = href.replace(/[0-9]/g, "");
    href = href + (maxNumber + 1);
    $("#addNewAddress").attr("href", href);
    $("#addNewAddress").removeAttr('disabled');
    SetRegionsAutocomplete();
}

$(function () {
    $("#addresses").on("click", ".removeAddress", function () {
        $(this).parent().parent().remove();
        ReAssignIndexesToChildren('addresses', 'belong-address');
    });
});