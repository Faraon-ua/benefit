var routePrefix = "/Benefit.Web";
//var routePrefix = "";

function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
          .toString(16)
          .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
      s4() + '-' + s4() + s4();
}

var QueryString = function () {
    // This function is anonymous, is executed immediately and 
    // the return value is assigned to QueryString!
    var query_string = {};
    var query = window.location.search.substring(1);
    var vars = query.split("&");
    for (var i = 0; i < vars.length; i++) {
        var pair = vars[i].split("=");
        // If first entry with this name
        if (typeof query_string[pair[0]] === "undefined") {
            query_string[pair[0]] = decodeURIComponent(pair[1]);
            // If second entry with this name
        } else if (typeof query_string[pair[0]] === "string") {
            var arr = [query_string[pair[0]], decodeURIComponent(pair[1])];
            query_string[pair[0]] = arr;
            // If third or later entry with this name
        } else {
            query_string[pair[0]].push(decodeURIComponent(pair[1]));
        }
    }
    return query_string;
}();

function insertUrlParam(key, value) {
    key = encodeURI(key); value = encodeURI(value);

    var kvp = document.location.search.substr(1).split('&');

    var i = kvp.length; var x; while (i--) {
        x = kvp[i].split('=');

        if (x[0] == key) {
            x[1] = value;
            kvp[i] = x.join('=');
            break;
        }
    }

    if (i < 0) { kvp[kvp.length] = [key, value].join('='); }
    //this will reload the page, it's likely better to store this until finished
    return document.location.pathname + "?" + kvp.join('&');
}

function onBegin() {
    $('#searchError').hide(0);
    $('#searchResults').hide(0);
    $('#loadingDisplay').show(0);
    $('#loadProgressBar').css('width', '50%').attr("aria-valuenow", 50);
}

function onComplete() {
    $('#searchResults').show(0);
    $('#loadProgressBar').css('width', '100%').attr("aria-valuenow", 100);
    $("#loadingDisplay").delay(500).fadeOut(20).queue(function (next) {
        $('#loadProgressBar').delay(1200).css('width', '0%').attr("aria-valuenow", 0);
        next();
    });
}
function onSuccess() {
    $('#searchResults').show();
}

function onFailure() {
    $('#searchError').show(0);
}

function ReAssignIndexesToChildren(id, itemSelector) {
    var container = $("#" + id);
    var rows = container.find("." + itemSelector);
    rows.each(function (rowIndex) {
        $(this).find("input, select").each(function () {
            var name = $(this).attr("name");
            var id = $(this).attr("id");
            if (name) {
                name = name.replace(/\[.*\]/g, '['+ rowIndex+']');
                if (id) {
                    id = id.replace(/\[.*\]/g, '['+ rowIndex+']');
                }
                $(this).attr("name", name);
                if (id) {
                    $(this).attr("id", id);
                }
                if ($(this).attr("data-number")) {
                    $(this).attr("data-number", rowIndex);
                }
            }
        });
    });

    UpdateCategoriesNumber();
    UpdateCurrenciesNumber();
    UpdateShippingMethodsNumber();
    UpdateAddressesNumber();
}

$(function () {
    try {
        $('.number-input').mask("#");
    } catch (err) {

    }

    setTimeout(function () {
        $("#flashMessage").html("");
    }, 10000);

    /*  $(".urlName").focus(function () {
          if ($(this).val() == "") {
              var originalName = $(".name").val();
              $(this).val(urlRusLat(originalName));
          }
      });*/

    $("body").on('focus', '.urlName', function () {
        if ($(this).val() == "") {
            var originalName = $(".name").val();
            $(this).val(urlRusLat(originalName));
        }
    });

    //show specific tab on page load
    var url = document.location.toString();
    if (url.match('#')) {
        $('.nav-tabs a[href="#' + url.split('#')[1] + '"]').tab('show');
    }
    $('.nav-tabs a').on('shown.bs.tab', function (e) {
        window.location.hash = e.target.hash;
    });
});

function GetMaxAttributeValue(selector, attributeName) {
    var maximum = 0;
    $(selector).each(function () {
        var value = parseFloat($(this).attr(attributeName));
        maximum = (value > maximum) ? value : maximum;
    });
    return maximum;
}

function CheckSearchLength() {
    var searchText = $("#searchText").val();
    if (searchText.length >= 1 && searchText.length < 3) {
        alert("Поле пошуку має містити мінімум 3 символи");
        return false;
    } else {
        onBegin();
    }
}

function flashMessage(text, error, alwaysStayVisible) {
    var status = "alert-success";
    if (error) {
        status = "alert-danger";
    }
    $("#flashMessage").html(
        "<p class='alert " + status + "'>" + text + "</p>"
    );
    if (!alwaysStayVisible) {
        setTimeout(function() {
            $("#flashMessage").html("");
        }, 7000);
    }
}

function SaveLocalization(id, resourceType, resourceId, resourceField, resourceValue, lang) {
    var localization = {
        Id: id,
        ResourceType: resourceType,
        ResourceId: resourceId,
        ResourceField: resourceField,
        ResourceValue: resourceValue,
        LanguageCode: lang
    };

    $.post(routePrefix + "/Admin/Categories/Index", localization);
}

//Транслитерация кириллицы в URL
function urlRusLat(str) {
    str = str.toLowerCase(); // все в нижний регистр
    var cyr2latChars = new Array(
            ['а', 'a'], ['б', 'b'], ['в', 'v'], ['г', 'g'],
            ['д', 'd'], ['е', 'e'], ['ё', 'yo'], ['ж', 'zh'], ['з', 'z'],
            ['и', 'i'], ['й', 'y'], ['к', 'k'], ['л', 'l'],
            ['м', 'm'], ['н', 'n'], ['о', 'o'], ['п', 'p'], ['р', 'r'],
            ['с', 's'], ['т', 't'], ['у', 'u'], ['ф', 'f'],
            ['х', 'h'], ['ц', 'c'], ['ч', 'ch'], ['ш', 'sh'], ['щ', 'shch'],
            ['ъ', ''], ['ы', 'y'], ['ь', ''], ['э', 'e'], ['ю', 'yu'], ['я', 'ya'],
            ['і', 'i'], ['ї', 'ji'], ['є', 'e'], ['ґ', 'g'],

            ['А', 'A'], ['Б', 'B'], ['В', 'V'], ['Г', 'G'],
            ['Д', 'D'], ['Е', 'E'], ['Ё', 'YO'], ['Ж', 'ZH'], ['З', 'Z'],
            ['И', 'I'], ['Й', 'Y'], ['К', 'K'], ['Л', 'L'],
            ['М', 'M'], ['Н', 'N'], ['О', 'O'], ['П', 'P'], ['Р', 'R'],
            ['С', 'S'], ['Т', 'T'], ['У', 'U'], ['Ф', 'F'],
            ['Х', 'H'], ['Ц', 'C'], ['Ч', 'CH'], ['Ш', 'SH'], ['Щ', 'SHCH'],
            ['Ъ', ''], ['Ы', 'Y'],
            ['Ь', ''], ['Э', 'E'], ['Ю', 'YU'], ['Я', 'YA'],
            ['І', 'i'], ['Ї', 'ji'], ['Є', 'e'], ['Ґ', 'g'],

            ['a', 'a'], ['b', 'b'], ['c', 'c'], ['d', 'd'], ['e', 'e'],
            ['f', 'f'], ['g', 'g'], ['h', 'h'], ['i', 'i'], ['j', 'j'],
            ['k', 'k'], ['l', 'l'], ['m', 'm'], ['n', 'n'], ['o', 'o'],
            ['p', 'p'], ['q', 'q'], ['r', 'r'], ['s', 's'], ['t', 't'],
            ['u', 'u'], ['v', 'v'], ['w', 'w'], ['x', 'x'], ['y', 'y'],
            ['z', 'z'],

            ['A', 'A'], ['B', 'B'], ['C', 'C'], ['D', 'D'], ['E', 'E'],
            ['F', 'F'], ['G', 'G'], ['H', 'H'], ['I', 'I'], ['J', 'J'], ['K', 'K'],
            ['L', 'L'], ['M', 'M'], ['N', 'N'], ['O', 'O'], ['P', 'P'],
            ['Q', 'Q'], ['R', 'R'], ['S', 'S'], ['T', 'T'], ['U', 'U'], ['V', 'V'],
            ['W', 'W'], ['X', 'X'], ['Y', 'Y'], ['Z', 'Z'],

            [' ', '_'], ['0', '0'], ['1', '1'], ['2', '2'], ['3', '3'],
            ['4', '4'], ['5', '5'], ['6', '6'], ['7', '7'], ['8', '8'], ['9', '9'],
            ['-', '-']

);

    var newStr = new String();

    for (var i = 0; i < str.length; i++) {

        ch = str.charAt(i);
        var newCh = '';

        for (var j = 0; j < cyr2latChars.length; j++) {
            if (ch == cyr2latChars[j][0]) {
                newCh = cyr2latChars[j][1];

            }
        }
        // Если найдено совпадение, то добавляется соответствие, если нет - пустая строка
        newStr += newCh;

    }
    // Удаляем повторяющие знаки - Именно на них заменяются пробелы.
    // Так же удаляем символы перевода строки, но это наверное уже лишнее
    return newStr.replace(/[_]{2,}/gim, '_').replace(/\n/gim, '');
}
