function SetAutocomplete(target) {
    var regionSearch = target.val();
    //get regions
    $.getJSON(routePrefix + "/Home/SearchRegion", {
        search: regionSearch,
        minLevel: 0
}, function (data) {
        //map regions to lable and value
        var array = data.error ? [] : $.map(data, function (m) {
            return {
                label: m.ExpandedName,
                value: m.Id
            };
        });
        //set autocomplete
        $(".regionSearch").autocomplete({
            minLength: 3,
            source: array,
            select: function (event, ui) {
                target.val(ui.item.label);
                target.next().val(ui.item.value);
                return false;
            },
            focus: function (event, ui) {
                target.val(ui.item.label);
                return false;
            }
        });
    });
}

function UpdateAddressesNumber() {
    var maxNumber = GetMaxAttributeValue('.address-id', 'data-number');
    var href = $("#addNewAddress").attr("href");
    href = href.substring(0, href.length - 1) + (maxNumber + 1);
    $("#addNewAddress").attr("href", href);
}

$(function () {
    $("#addresses").on("click", ".removeAddress", function() {
        $(this).parent().parent().remove();

    });

    /*$('#addresses').autocomplete({
        source: function (request, response) {
            $.ajax({
                url: routePrefix + "/Home/SearchRegion",
                dataType: "json",
                data: {
                    search: $('#addresses').val(),
                },
                success: response //response is a callable accepting data parameter. no reason to wrap in anonymous function.
            });
        },
        minLength: 3,
        cacheLength: 0,
        select: function (event, ui) {
            target.val(ui.item.label);
            target.next().val(ui.item.value);
            return false;
        },
        focus: function (event, ui) {
            target.val(ui.item.label);
            return false;
        }
    });*/

    $("body").on("keyup", ".regionSearch", function () {
        var target = $(this);
        if (target.val().length < 3) {
            return false;
        } else {
            setTimeout(function() {
                SetAutocomplete(target);
            }, 2000);
        }
    });
});