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

// удаляет cookie с именем name
function deleteCookie(name) {
    setCookie(name, "", {
        expires: -1
    });
}

$(function () {
    $("#predefinedRegions a").click(function () {
        var regionName = $(this).text();
        var regionId = $(this).attr("data-region-id");
        $("#select_place .inside").text(regionName);
        $(".region-search-txt").val(regionName);
        $(".region_modal").modal("hide");
        setCookie("regionName", regionName, { expires: 31536000, path: "/" });//year
        setCookie("regionId", regionId, { expires: 31536000, path: "/" });//year
    });

    if (!getCookie("regionName")) {
        $(".region_modal").modal();
    }
    $("#select_place").click(function () {
        $(".region_modal").modal();
    });

    $(".region-search-txt, .region-modal-search-txt").devbridgeAutocomplete({
        minChars: 3,
        serviceUrl: routePrefix + '/Home/SearchRegion',
        onSelect: function (suggestion) {
            var result = suggestion.value.substring(0, suggestion.value.indexOf(" ("));
            $("#select_place .inside").text(result);
            $(".region-search-txt").val(result);
            $(".region_modal").modal("hide");
            setCookie("regionName", result, { expires: 31536000, path: "/" });//year
            setCookie("regionId", suggestion.data, { expires: 31536000, path: "/" });//year
        }
    });

    if (getCookie("regionName")) {
        $("#select_place .inside").text(getCookie("regionName"));
    } else {
        $("#select_place .inside").text("Оберіть місто");
    }

    $("body").on("click", ".structure_table_register_email input", function() {
        $(this).select();
    });
});