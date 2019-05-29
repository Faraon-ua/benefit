var options = "";
var showFiltersReset = false;
var parts = location.href.split('/');
var lastSegment = (parts.pop() || parts.pop()).replace('#', ''); // handle potential trailing slash
lastSegment = lastSegment.replace(location.search, ""); //handle query params
var sortControl = $(".sort_btn[data-sort-value=Rating]");

$(function () {
    if ($('#price-slider').length > 0) {
        $('#price-slider').bootstrapSlider().on('change',
            function (data) {
                $("#price-lower-bound").val(data.value.newValue[0]);
                $("#price-upper-bound").val(data.value.newValue[1]);
                $("#price-filter").attr("data-filter-value",
                    data.value.newValue[0] + "-" + data.value.newValue[1]);
            }).data('slider');
    }

    $("#remove-all").click(function () {
        $(".remove-from-favorites").click();
    });

    $("#remove-selected").click(function () {
        $(".select-product:checked").siblings(".remove-from-favorites ").click();
    });
    $("#buy-selected").click(function () {
        $(".select-product:checked").closest(".product-item").find(".product_buy.lg-hidden").click();
    });

    $("#buy-all").click(function () {
        $(this).attr("disabled", "disabled");
        $(".product_buy").click();
    });

    $(".ajax_load_btn, .paging").click(function (e) {
        e.preventDefault();
        if ($(this).parent().hasClass("active"))
            return;
        parts = location.href.split('/');
        lastSegment = (parts.pop() || parts.pop()).replace("#", "");
        lastSegment = lastSegment.replace(location.search, ""); //handle query params
        $(".products-wrapper").css("opacity", "0.3");
        var moreBtn = $(this);
        var page = parseInt(moreBtn.attr("data-page"));
        var pageUrl;
        if (lastSegment === categoryUrlName) {
            options = "page=" + (page + 1) + ";";
            pageUrl = lastSegment + "/" + options;
        } else {
            options = pageUrl = lastSegment.replace(/page=\d+/g, 'page=' + (page + 1));
        }
        if (categoryUrlName === "search") {
            pageUrl += location.search;
        }
        history.pushState("history", "page " + page, pageUrl);
        
        $(".loader").show();
        if (!moreBtn.hasClass("ajax_load_btn")) {
            $("html, body").animate({ scrollTop: 0 }, 1000);
        }
        var getProductsWithDataUrl = getProductsUrl +
            "?categoryId=" +
            categoryId +
            "&sellerId=" +
            sellerId +
            "&page=" +
            (page + 1) +
            "&options=" +
            options +
            "&term=" +
            term;
        if (typeof productTemplateLayout !== 'undefined') {
            getProductsWithDataUrl = getProductsWithDataUrl + "&layout=" + productTemplateLayout;
        }
        $.get(getProductsWithDataUrl,
            function (data) {
                var maxNumber = parseInt($(".paging:not(.next)").last().attr("data-page"));
                $(".paging").parent().show();
                $(".split").remove();
                if (moreBtn.hasClass("ajax_load_btn")) {
                    if (data.number <= takePerPage) {
                        moreBtn.hide();
                        $(".paging.next").parent().hide();
                        $(".ajax_load_btn").hide();
                    } else {
                        $(".paging.next").parent().show();
                        $(".ajax_load_btn").show();
                    }
                    $(".product-item:last").after(data.products);
                    moreBtn.attr("data-page", page + 1);
                    $(".paging[data-page=" + page + "]:not(.prev):not(.next)").parent()
                        .addClass("active");
                } else {
                    if (data.number <= takePerPage) {
                        $(".ajax_load_btn").hide();
                        $(".paging.next").parent().hide();
                    } else {
                        $(".ajax_load_btn").show();
                        $(".paging.next").parent().show();
                    }
                    $(".products-wrapper").html(data.products);
                    $(".paging").parent().removeClass("active");
                    $(".paging[data-page=" + page + "]:not(.prev):not(.next)").parent().addClass("active");
                    if (page > 0) {
                        $(".paging.prev").parent().show();
                    } else {
                        $(".paging.prev").parent().hide();
                    }
                    $(".paging.prev").attr("data-page", (page - 1));
                    $(".paging.next").attr("data-page", (page + 1));
                    $(".ajax_load_btn").attr("data-page", (page + 1));
                }
                var visibleLinks = $("a[data-page=0],a[data-page=1],a[data-page=" +
                    maxNumber +
                    "],a[data-page=" +
                    (maxNumber - 1) +
                    "],a[data-page=" +
                    page +
                    "],a[data-page=" +
                    (page + 1) +
                    "],a[data-page=" +
                    (page - 1) +
                    "]").not(".prev,.next");
                $(".paging:not(.prev):not(.next)").not(visibleLinks).parent().hide();
                if (page > 3) {
                    $(".paging[data-page=" + (page - 1) + "]:not(.prev)").parent()
                        .before("<li class='page_item split'>...</li>");
                }
                if (page >= pagesMaxNumber) {
                    $(".paging.next").parent().hide();
                }
                if (page < (maxNumber - 1)) {
                    $(".paging[data-page=" + (page + 1) + "]:not(.next)").parent()
                        .after("<li class='page_item split'>...</li>");
                }
                $(".products-wrapper").css("opacity", "1");
                $(".loader").hide();
                if (page >= 1) {
                    $(".group_description_block").css("display", "none");
                    $(".seo-fix").css("display", "none");
                } else {
                    $(".group_description_block").css("display", "block");
                    $(".seo-fix").css("display", "block");
                }
                $("img[data-defer-src]").each(function () {
                    $(this).attr("src", $(this).attr("data-defer-src"));
                });
                if (page != 0) {
                    var title = $("title").text();
                    var title = title.substr(0, (title.indexOf("сторінка") == -1 ? title.length : title.indexOf("сторінка")));
                    $("title").text(title + " сторінка " + (page + 1));
                }
            });
    });

    $("#reset-filters").click(function () {
        location.href = location.href.substring(0, location.href.indexOf(lastSegment)) + location.search;
    });

    $('body').on("click",
        ".remove-filter",
        function () {
            var optionName = $(this).parent().attr("data-option-name");
            if (optionName === "price") {
                location.href = location.href.replace(/price=\d+-\d+;/g, '');
                return;
            }
            var optValue = decodeURI($(this).parent().attr("data-option-value"));
            var checkbox = $(".filter-section[data-filter-name=" + optionName + "] input#" + optValue);
            if (checkbox.length === 0) {
                checkbox = $(".filter-section[data-filter-name=" +
                    optionName +
                    "] input[name=" +
                    optValue.replace(" ", "") +
                    "]");
            }
            checkbox.click();
        });

    $(".bx_filter_parameters_box_title").click(function () {
        $(this).parent().toggleClass("active");
        $(this).next().slideToggle();
    });

    $(".filter_opener").click(function () {
        $(this).toggleClass("opened");
        $(".bx_filter_vertical, .bx_filter").slideToggle(333);
    });

    var pagesMaxNumber = pagesCount - 1;
    var currentPage = 0;
    if (lastSegment !== categoryUrlName) {
        var result = lastSegment.match(/page=(\d+)/i);
        if (result && result[1]) {
            currentPage = parseInt(result[1]) - 1;
            $(".paging").parent().removeClass("active");
            $("a[data-page=" + currentPage + "]").parent().addClass("active");
            $(".ajax_load_btn").attr("data-page", currentPage + 1);
        }
    }
    //hide redundant pages
    var visibleLinks = $("a[data-page=0],a[data-page=1],a[data-page=2],a[data-page=" +
        pagesMaxNumber +
        "],a[data-page=" +
        (pagesMaxNumber - 1) +
        "],a[data-page=" +
        currentPage +
        "],a[data-page=" +
        (currentPage + 1) +
        "],a[data-page=" +
        (currentPage - 1) +
        "]").not(".prev,.next");
    $(".paging:not(.prev):not(.next)").not(visibleLinks).parent().hide();
    if (currentPage >= pagesMaxNumber) {
        $(".paging.next").parent().hide();
        $(".ajax_load_btn").hide();
    }
    if (currentPage > 0) {
        $(".paging.prev").parent().show();
    }
    $(".paging[data-page=2]").parent().after("<li class='page_item split'>...</li>");

    //select all checkboxes from url

    if (hasCategory) {
        if (lastSegment.toLowerCase() != categoryUrlName) {
            options = lastSegment;
            $.each(options.split(";"),
                function (i, urlSegment) {
                    if (urlSegment === "") return;
                    var optKeyValue = urlSegment.split("=");
                    var optionName = optKeyValue[0];
                    var optionValues = optKeyValue[1].split(",");
                    if (optionName) {
                        if (optionName === "sort") {
                            var optionValue = optionValues[0];
                            sortControl = $(".sort_btn[data-sort-value=" + optionValue + "]");
                            $(".sort_btn span").removeClass("badge");
                            return true;
                        }
                    }
                    if (optionName === "price") {
                        var optionValue = optionValues[0];
                        var prices = optionValue.split('-');
                        var lowerPrice = parseInt(prices[0]);
                        var upperPrice = parseInt(prices[1]);
                        $("#price-lower-bound").val(lowerPrice);
                        $("#price-upper-bound").val(upperPrice);
                        $("#price-filter").prop('checked', true);
                        $("#price-slider").data('bootstrapSlider').setValue([lowerPrice, upperPrice]);
                        $(".selected-filters").append(
                            "<span class='badge padding_5 margin-right-10 margin-top-5 ' data-option-name='price' data-option-value='price-filter'>" +
                            optionValue +
                            " грн<i class='fa fa-times-circle pointer remove-filter' style='font-size:1.5em'></i></span>"
                        );
                        return true;
                    }
                    $.each(optionValues,
                        function (j, optValue) {
                            if (optionName == "page") {
                                return;
                            }
                            showFiltersReset = true;
                            var checkbox = $(".filter-section[data-filter-name=" +
                                optionName +
                                "] input#" +
                                decodeURI(optValue));
                            if (checkbox.length === 0) {
                                checkbox = $(".filter-section[data-filter-name=" +
                                    optionName +
                                    "] input[name=" +
                                    decodeURI(optValue).replace(" ", "") +
                                    "]");
                            }
                            checkbox.prop('checked', true);
                            var optionvalueText = checkbox.attr("text");
                            $(".selected-filters").append(
                                "<span class='badge padding_5 margin-right-10 margin-top-5 ' data-option-name='" +
                                optionName +
                                "' data-option-value='" +
                                optValue +
                                "'>" +
                                optionvalueText +
                                " <i class='fa fa-times-circle pointer remove-filter' style='font-size:1.5em'></i></span>"
                            );
                        });
                    if (showFiltersReset) {
                        $("#reset-filters").show();
                    }
                });
        }

    }
    sortControl.find("span").addClass("badge");

    if (hasCategory) {
        $('body').on('change click',
            "#productFilters input[type=checkbox], #productFilters button, .sort_btn",
            function (e) {
                e.preventDefault();
                $(".loader").show();
                var parts = location.href.split('/');
                lastSegment =
                    (parts.pop() || parts.pop()).replace("#", ""); // handle potential trailing slash
                lastSegment = lastSegment.replace(location.search, ""); //handle query params

                if (lastSegment != categoryUrlName) {
                    options = lastSegment;
                }

                var parent = $(this).parents(".filter-section");
                var optionName = parent.attr("data-filter-name");
                var optionNameIndex = options.indexOf(optionName);

                var currentOption = '';
                var selectedValues = parent.find("input[type=checkbox]:checked, button").map(function () {
                    if ($(this).attr("data-filter-value")) {
                        return $(this).attr("data-filter-value");
                    }
                    return $(this).attr("id");
                }).get();

                if ($(this).attr("class").indexOf("sort_btn") >= 0) {
                    var val = $(this).attr("data-sort-value");
                    selectedValues.push(val);
                }
                if (selectedValues.length > 0) {
                    currentOption = optionName;
                    currentOption += "=";
                    currentOption += selectedValues.join();
                    currentOption += ";";
                }
                if (optionNameIndex >= 0) {
                    var ending = options.indexOf(";", optionNameIndex);
                    var oldOption = options.substring(optionNameIndex, ending + 1);
                    options = options.replace(oldOption, currentOption);
                } else {
                    options += currentOption;
                    options = options.replace(/page=\d+/g, 'page=1');
                }
                //check for page
                if (options.indexOf('page=') === -1) {
                    options += "page=1;";
                }
                var locBuilder;
                if (lastSegment !== categoryUrlName) {
                    locBuilder = location.protocol + '//' + location.host + location.pathname.replace(lastSegment, "") + options + location.search;
                    location.href = locBuilder;
                } else {
                    locBuilder = location.protocol + '//' + location.host + location.pathname.replace(location.search, "");
                    if (location.href[location.href.length - 1] !== "/") {
                        locBuilder += "/";
                    }
                    if (lastSegment === "search") {
                        window.location.href = locBuilder + options + location.search;
                    } else {
                        window.location.href = locBuilder + options;
                    }
                }
            });
    }
});