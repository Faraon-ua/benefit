/* Начальное кол-во для смены в карточке и корзине */
okay.amount = 1;

/* Аяксовая корзина */
$(document).on('submit', '.fn_variants', function(e) {
    e.preventDefault();
    var variant,
        amount;
    /* Вариант */
    if($(this).find('input[name=variant]:checked').size() > 0 ) {
        variant = $(this).find('input[name=variant]:checked').val();
    } else if($(this ).find('input[name=variant]').size() > 0 ) {
        variant = $(this).find('input[name=variant]').val();
    } else if($(this).find('select[name=variant]').size() > 0 ) {
        variant = $(this).find('select[name=variant]').val();
    }
    /* Кол-во */
    if($(this).find('input[name=amount]').size()>0) {
        amount = $(this).find('input[name=amount]').val();
    } else {
        amount = 1;
    }
    /* ajax запрос */
    $.ajax( {
        url: "ajax/cart.php",
        data: {
            variant: variant,
            amount: amount
        },
        dataType: 'json',
        success: function(data) {
            $( '#cart_informer' ).html( data );
        }
    } );
    /* Улеталка */
    transfer( $('#cart_informer'), $(this) );
});

/* Смена варианта в превью товара и в карточке */
$(document).on('change', '.fn_variant', function() {
    var selected = $( this ).children( ':selected' ),
        parent = selected.closest( '.fn_product' ),
        price = parent.find( '.fn_price' ),
        cprice = parent.find( '.fn_old_price' ),
        sku = parent.find( '.fn_sku' ),
        stock = parseInt( selected.data( 'stock' ) ),
        amount = parent.find( 'input[name="amount"]' ),
        camoun = parseInt( amount.val()),
        units = selected.data('units');
    price.html( selected.data( 'price' ) );
    amount.data('max', stock);
    /* Количество товаров */
    if ( stock < camoun ) {
        amount.val( stock );
    } else if ( okay.amount > camoun ) {
        amount.val( okay.amount );
    }
    else if(isNaN(camoun)){
        amount.val( okay.amount );
    }
    /* Цены */
    if( selected.data( 'cprice' ) ) {
        cprice.html( selected.data( 'cprice' ) );
        cprice.parent().removeClass( 'hidden' );
    } else {
        cprice.parent().addClass( 'hidden' );
    }
    /* Наличие на складе */
    if (stock == 0) {
        parent.find('.fn_not_stock').removeClass('hidden');
        parent.find('.fn_in_stock').addClass('hidden');
    } else {
        parent.find('.fn_in_stock').removeClass('hidden');
        parent.find('.fn_not_stock').addClass('hidden');
    }
    /* Предзаказ */
    if (stock == 0 && okay.is_preorder) {
        parent.find('.fn_is_preorder').removeClass('hidden');
        parent.find('.fn_is_stock, .fn_not_preorder').addClass('hidden');
    } else if (stock == 0 && !okay.is_preorder) {
        parent.find('.fn_not_preorder').removeClass('hidden');
        parent.find('.fn_is_stock, .fn_is_preorder').addClass('hidden');
    } else {
        parent.find('.fn_is_stock').removeClass('hidden');
        parent.find('.fn_is_preorder, .fn_not_preorder').addClass('hidden');
    }

    if( typeof(units) != 'undefined' ) {
        parent.find('.fn_units').text(', ' + units);
    } else {
        parent.find('.fn_units').text('');
    }
});

/* Количество товара в карточке и корзине */
$( document ).on( 'click', '.fn_product_amount:not(.partial) span', function() {
    var input = $( this ).parent().find( 'input' ),
        action;
    if ( $( this ).hasClass( 'plus' ) ) {
        action = 'plus';
    } else if ( $( this ).hasClass( 'minus' ) ) {
        action = 'minus';
    }
    var step = 1;
    var isWeightProduct = $(this).parent().attr("data-weight-product").toLowerCase() === "true";
    if (isWeightProduct) {
        step = 0.1;
    }
    amount_change( input, action, step );
} );

/* Функция добавления / удаления в папку сравнения */
$(document).on('click', '.fn_comparison', function(e){
    e.preventDefault();
    var button = $( this ),
        action = $( this ).hasClass( 'selected' ) ? 'delete' : 'add',
        product = parseInt( $( this ).data( 'id' ) );
    /* ajax запрос */
    $.ajax( {
        url: "ajax/comparison.php",
        data: { product: product, action: action },
        dataType: 'json',
        success: function(data) {
            $( '#comparison' ).html( data );
            /* Смена класса кнопки */
            if( action == 'add' ) {
                button.addClass( 'selected' );
            } else if( action == 'delete' ) {
                button.removeClass( 'selected' );
            }
            /* Смена тайтла */
            if( button.attr( 'title' ) ) {
                var text = button.data( 'result-text' ),
                    title = button.attr( 'title' );
                button.data( 'result-text', title );
                button.attr( 'title', text );
            }
            /* Если находимся на странице сравнения - перезагрузить */
            if( $( '.fn_comparison_products' ).size() ) {
                window.location = window.location;
            }
        }
    } );
    /* Улеталка */
    if( !button.hasClass( 'selected' ) ) {
        transfer( $( '#comparison' ), $( this ) );
    }
});

/* Функция добавления / удаления в папку избранного */
$(document).on('click', '.fn_wishlist', function (e) {
    e.preventDefault();
    var button = $( this ),
        action = $( this ).hasClass( 'selected' ) ? 'delete' : '';
    /* ajax запрос */
    //$.ajax( {
    //    url: "ajax/wishlist.php",
    //    data: { id: $( this ).data( 'id' ), action: action },
    //    dataType: 'json',
    //    success: function(data) {
    //        $( '#wishlist' ).html( data.info );
    //        /* Смена класса кнопки */
    //        if (action == '') {
    //            button.addClass( 'selected' );
    //        } else {
    //            button.removeClass( 'selected' );
    //        }
    //        /* Смена тайтла */
    //        if( button.attr( 'title' ) ) {
    //            var text = button.data( 'result-text' ),
    //                title = button.attr( 'title' );
    //            button.data( 'result-text', title );
    //            button.attr( 'title', text );
    //        }
    //        /* Если находимся на странице сравнения - перезагрузить */
    //        if( $( '.fn_wishlist_page' ).size() ) {
    //            window.location = window.location;
    //        }
    //    }
    //} );
    /* Улеталка */
    if( !button.hasClass( 'selected' ) ) {
        transfer( $( '#wishlist' ), $( this ) );
    }
});

/* Отправка купона по нажатию на enter */
$( document ).on( 'keypress', '.fn_coupon', function(e) {
    if( e.keyCode == 13 ) {
        e.preventDefault();
        ajax_coupon();
    }
} );

/* Отправка купона по нажатию на кнопку */
$( document ).on( 'click', '.fn_sub_coupon', function(e) {
    ajax_coupon();
} );

/* Document ready */
$(function(){

    $(document).on("click", ".fn_filter_link", function() {
        location.href = location.protocol + "//" + location.hostname + $(this).attr("href");
        return false;
    });

    /* Обратный звонок */
    $('.fn_callback').fancybox();

    // Выпадающие блоки
    $('.fn_switch').click(function(e){
        e.preventDefault();

        $(this).next().slideToggle(300);

        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
        }
        else {
            $(this).addClass('active');
        }
    });

    //Главное меню для мобильных
    $('.fn_menu_switch').on("click", function(){
        $('.menu').slideToggle(300);
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
        }
        else {
            $(this).addClass('active');
        }
    });

    //Меню каталога для мобильных и планшетов
    $('.fn_menu_catalog').on("click", function(){
        $('.categories_nav').slideToggle(300);
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
        }
        else {
            $(this).addClass('active');
        }
    });

    //Кабинет
    $('.fn_account').on("click", function(){
        $('.account').slideToggle(300);
        if ($(this).hasClass('active')) {
            $(this).removeClass('active');
        }
        else {
            $(this).addClass('active');
        }
    })

    //Фильтры мобильные
    $('.subswitch').click(function(){
        $(this).parent().next().slideToggle(500);

        if ($(this).hasClass('down')) {
            $(this).removeClass('down');
        }
        else {
            $(this).addClass('down');
        }
    });

    //Табы в карточке товара
    var nav = $('.tabs').find('.tab_navigation');
    var tabs = $('.tabs').find('.tab_container');

    if(nav.children('.selected').size() > 0) {
        $(nav.children('.selected').attr("href")).show();
    } else {
        nav.children().first().addClass('selected');
        tabs.children().first().show();
    }

    $('.tab_navigation a').click(function(e){
        e.preventDefault();
        if($(this).hasClass('selected')){
            return true;
        }
        tabs.children().hide();
        nav.children().removeClass('selected');
        $(this).addClass('selected');
        $($(this).attr("href")).fadeIn(200);
    });

    //Кнопка вверх
    $(window).scroll(function () {
    var scroll_height = $(window).height();

     if ($(this).scrollTop() >= scroll_height) {
            $('.to_top').fadeIn();
        } else {
            $('.to_top').fadeOut();
        }
    });

    $('.to_top').click(function(){
        $("html, body").animate({scrollTop: 0}, 500);
    });


    /* Инициализация баннера */
    $('.fn_banner_group1').slick({
        infinite: true,
        speed: 1000,
        slidesToShow: 1,
        slidesToScroll: 1,
        swipeToSlide : true,
        dots: true,
        arrows: false,
        adaptiveHeight: true,
        autoplaySpeed: 5000,
        autoplay: true,
        fade: true
    });


    /* Инициализация бокового баннера */
    $('.fn-slick-banner_2.okaycms').slick({
        infinite: true,
        speed: 500,
        slidesToShow: 1,
        slidesToScroll: 1,
        swipeToSlide : true,
        dots: true,
        arrows: false,
        adaptiveHeight: true,
        autoplaySpeed: 8000,
        autoplay: true
    });

    /* Товары слайдер*/
    $(".products_slider").slick({
        infinite: true,
        speed: 500,
        slidesToShow: 4,
        slidesToScroll: 1,
        arrows: true,
        responsive: [
            {
                breakpoint: 1200,
                settings: {
                    slidesToShow: 3
                }
            },
            {
                breakpoint: 992,
                settings: {
                    slidesToShow: 2
                }
            },
            {
                breakpoint: 768,
                settings: {
                    slidesToShow: 3
                }
            },
            {
                breakpoint: 668,
                settings: {
                    slidesToShow: 2
                }
            },
            {
                breakpoint: 420,
                settings: {
                    slidesToShow: 1
                }
            }
        ]
    });

    /* Бренды слайдер*/
	$(".fn_all_brands").slick({
		infinite: true,
		speed: 500,
		slidesToShow: 4,
		slidesToScroll: 1,
		arrows: true,
		responsive: [
            {
                breakpoint: 768,
                settings: {
                    slidesToShow: 3
                }
            },
            {
                breakpoint: 544,
                settings: {
                    slidesToShow: 2
                }
            }
		]
	});

    /* Инициализация доп. фото в карточке */
	$(".fn_images").slick({
		infinite: false,
		speed: 500,
		slidesToShow: 4,
		slidesToScroll: 1,
        swipeToSlide : true,
		arrows: true,
		responsive: [
			{
				breakpoint: 481,
				settings: {
					slidesToShow: 3
				}
			}
		]
	});


    //$('.featured_products .product_name').matchHeight({
    //    byRow: false
    //});
    //$('.featured_products .preview_details').matchHeight({
    //    byRow: false
    //});
    //$('.new_products .product_name').matchHeight({
    //    byRow: false
    //});
    //$('.new_products .preview_details').matchHeight({
    //    byRow: false
    //});
    //$('.discounted_products .product_name').matchHeight({
    //    byRow: false
    //});
    //$('.discounted_products .preview_details').matchHeight({
    //    byRow: false
    //});
    //$('.related .product_name').matchHeight({
    //    byRow: false
    //});
    //$('.related .preview_details').matchHeight({
    //    byRow: false
    //});
    //$('.fn_categories .product_name').matchHeight();
    //$('.fn_categories .preview_details').matchHeight();
    //$('.fn_blog_item').matchHeight();

    /* Зум картинок в карточке */
    $('[data-fancybox]').fancybox({
        image : {
            protect: true
        }
    });

    /* Аяксовый фильтр по цене */
    if( $( '#fn_slider_price' ).size() ) {
        var slider_all = $( '#fn_slider_min, #fn_slider_max' ),
            slider_min = $( '#fn_slider_min' ),
            slider_max = $( '#fn_slider_max' ),
            current_min = slider_min.val(),
            current_max = slider_max.val(),
            range_min = slider_min.data( 'price' ),
            range_max = slider_max.data( 'price' ),
            link = window.location.href.replace( /\/page-(\d{1,5})/, '' ),
            ajax_slider = function() {
                $.ajax( {
                    url: link,
                    data: {
                        ajax: 1,
                        'p[min]': slider_min.val(),
                        'p[max]': slider_max.val()
                    },
                    dataType: 'json',
                    success: function(data) {
                        $( '#fn_products_content' ).html( data.products_content );
                        $( '.fn_pagination' ).html( data.products_pagination );
                        $('.fn_products_sort').html(data.products_sort);
                        $('.fn_categories .product_name').matchHeight();
                        $('.fn_categories .preview_details').matchHeight();
                        $('.fn_ajax_wait').remove();
                    }
                } );
            };
        link = link.replace(/\/sort-([a-zA-Z_]+)/, '');

        $( '#fn_slider_price' ).slider( {
            range: true,
            min: range_min,
            max: range_max,
            values: [current_min, current_max],
            slide: function(event, ui) {
                slider_min.val( ui.values[0] );
                slider_max.val( ui.values[1] );
            },
            stop: function(event, ui) {
                slider_min.val( ui.values[0] );
                slider_max.val( ui.values[1] );
                $('.fn_categories').append('<div class="fn_ajax_wait"></div>');
                ajax_slider();
            }
        } );

        slider_all.on( 'change', function() {
            $( "#fn_slider_price" ).slider( 'option', 'values', [slider_min.val(), slider_max.val()] );
            ajax_slider();
        } );

        // Если после фильтрации у нас осталось товаров на несколько страниц, то постраничную навигацию мы тоже проведем с помощью ajax чтоб не сбить фильтр по цене
        $( document ).on( 'click', '.fn_is_ajax a', function(e) {
            e.preventDefault();
            $('.fn_categories').append('<div class="fn_ajax_wait"></div>');
            var link = $(this).attr( 'href' ),
                send_min = $("#fn_slider_min").val();
                send_max = $("#fn_slider_max").val();
            $.ajax( {
                url: link,
                data: { ajax: 1, 'p[min]': send_min, 'p[max]': send_max },
                dataType: 'json',
                success: function(data) {
                    $( '#fn_products_content' ).html( data.products_content );
                    $( '.fn_pagination' ).html( data.products_pagination );
                    $('#fn_products_sort').html(data.products_sort);
                    $('.fn_categories .product_name').matchHeight();
                    $('.fn_categories .preview_details').matchHeight();
                    $('.fn_ajax_wait').remove();
                }
            } );
        } );
    }

    ///* Автозаполнитель поиска */
    //$( ".fn_search" ).autocomplete( {
    //    serviceUrl: 'ajax/search_products.php',
    //    minChars: 1,
    //    noCache: true,
    //    onSelect: function(suggestion) {
    //        $( "#fn_search" ).submit();
    //    },
    //    transformResult: function(result, query) {
    //        var data = JSON.parse(result);
    //        $(".fn_search").autocomplete('setOptions', {triggerSelectOnValidInput: data.suggestions.length == 1});
    //        return data;
    //    },
    //    formatResult: function(suggestion, currentValue) {
    //        var reEscape = new RegExp( '(\\' + ['/', '.', '*', '+', '?', '|', '(', ')', '[', ']', '{', '}', '\\'].join( '|\\' ) + ')', 'g' );
    //        var pattern = '(' + currentValue.replace( reEscape, '\\$1' ) + ')';
    //        return "<div>" + (suggestion.data.image ? "<img align=absmiddle src='" + suggestion.data.image + "'> " : '') + "</div>" + "<a href=" + suggestion.lang + "products/" + suggestion.data.url + '>' + suggestion.value.replace( new RegExp( pattern, 'gi' ), '<strong>$1<\/strong>' ) + '<\/a>' + "<span>" + suggestion.price + " " + suggestion.currency + "</span>";
    //    }
    //} );

    /* Слайдер в сравнении */
    if( $( '.fn_comparison_products' ).size() ) {
            $( '.fn_comparison_products' ).slick( {
                infinite: true,
                slidesToShow: 3,
                slidesToScroll: 1,
                arrows: true,
                responsive: [
                    {
                      breakpoint: 1200,
                      settings: {
                        slidesToShow: 2,
                      }
                    },
                    {
                      breakpoint: 992,
                      settings: {
                        slidesToShow: 1
                      }
                    }
                ]
            } );

        resize_comparison();

        /* Показать / скрыть одинаковые характеристики в сравнении */
        $( document ).on( 'click', '.fn_show a', function(e) {
            e.preventDefault();
            $( '.fn_show a.active' ).removeClass( 'active' );
            $( this ).addClass( 'active' );
            if( $( this ).hasClass( 'unique' ) ) {
                $( '.cell.not_unique' ).hide();
            } else {
                $( '.cell.not_unique' ).show();
            }
        } );
    };
    /* Рейтинг товара */
    $('.product_rating').rater({ postHref: 'ajax/rating.php' });

    /* Переключатель способа оплаты */
    $( document ).on( 'click', '[name="payment_method_id"]', function() {
        $( '[name="payment_method_id"]' ).parent().removeClass( 'active' );
        $( this ).parent().addClass( 'active' );
    } );
});


/* Обновление блоков: cart_informer, cart_purchases, cart_deliveries */
function ajax_set_result(data) {
    $( '#cart_informer' ).html( data.cart_informer );
    $( '#fn_purchases' ).html( data.cart_purchases );
    $( '#fn_ajax_deliveries' ).html( data.cart_deliveries );
}

/* Аяксовое изменение кол-ва товаров в корзине */
function ajax_change_amount(object, variant_id) {
    var amount = $( object ).val(),
        coupon_code = $( 'input[name="coupon_code"]' ).val(),
        delivery_id = $( 'input[name="delivery_id"]:checked' ).val(),
        payment_id = $( 'input[name="payment_method_id"]:checked' ).val();
}

/* Функция изменения количества товаров */
function amount_change(input, action, step) {
    var max_val,
        curr_val = parseFloat( input.val() ),
        id = input.data('id');
        if(isNaN(curr_val)){
            curr_val = okay.amount;
        }

    /* Если включен предзаказ макс. кол-во товаров ставим 50 */
    if ( input.parent().hasClass('fn_is_preorder')) {
        max_val = 50;
    } else {
        max_val = parseFloat( input.data( 'max' ) );
    }
    /* Изменение кол-ва товара */
    if( action == 'plus' ) {
        input.val((curr_val + step).toFixed(1) );
        input.trigger('change');
    } else if( action == 'minus' ) {
        input.val((curr_val - step).toFixed(1) );
        input.trigger('change');
    } else if( action == 'keyup' ) {
        input.val((curr_val).toFixed(1) );
        input.trigger('change');
    }
    okay.amount = parseFloat( input.val() );
}

/* Функция анимации добавления товара в корзину */
function transfer(informer, thisEl) {
    var o1 = thisEl.offset(),
        o2 = informer.offset(),
        dx = o1.left - o2.left,
        dy = o1.top - o2.top,
        distance = Math.sqrt(dx * dx + dy * dy);
    thisEl.closest( '.fn_transfer' ).find( '.fn_img' ).effect( "transfer", {
        to: informer,
        className: "transfer_class"
    }, distance );

    var container = $( '.transfer_class' );
    container.html( thisEl.closest( '.fn_transfer' ).find( '.fn_img' ).parent().html() );
    container.find( '*' ).css( 'display', 'none' );
    container.find( '.fn_img' ).css( {
        'display': 'block',
        'height': '100%',
        'z-index': '2',
        'position': 'relative'
    } );
}

/* Изменение способа доставки */
function change_payment_method($id) {
    $( "#fn_delivery_payment_" + $id + " [name='payment_method_id']" ).first().trigger('click');
    $( ".fn_delivery_payment" ).hide();
    $( "#fn_delivery_payment_" + $id ).show();
    $( 'input[name="delivery_id"]' ).parent().removeClass( 'active' );
    $( '#deliveries_' + $id ).parent().addClass( 'active' );
}

/* Формирование ровных строчек для характеристик */
function resize_comparison() {
    var minHeightHead = 0;
    $('.fn_resize' ).each(function(){
        if( $(this ).height() > minHeightHead ) {
            minHeightHead = $(this ).height();
        }
    });
    $('.fn_resize' ).height(minHeightHead);
    if ($('[data-use]').size()) {
        $('[data-use]').each(function () {
            var use = '.' + $(this).data('use');
            var minHeight = $(this).height();
            if ($(use).size()) {
                $(use).each(function () {
                    if ($(this).height() >= minHeight) {
                        minHeight = $(this).height();
                    }
                });
                $(use).height(minHeight);
            }
        });
    }
}

/* В сравнении выравниваем строки */
$( window ).load( resize_comparison );

/* Звёздный рейтинг товаров */
$.fn.rater = function (options) {
    var opts = $.extend({}, $.fn.rater.defaults, options);
    return this.each(function () {
        var $this = $(this);
        var $on = $this.find('.rating_starOn');
        var $off = $this.find('.rating_starOff');
        opts.size = $on.height();
        if (opts.rating == undefined) opts.rating = $on.width() / opts.size;

        $off.mousemove(function (e) {
            var left = e.clientX - $off.offset().left;
            var width = $off.width() - ($off.width() - left);
            width = Math.ceil(width / (opts.size / opts.step)) * opts.size / opts.step;
            $on.width(width);
        }).hover(function (e) { $on.addClass('rating_starHover'); }, function (e) {
            $on.removeClass('rating_starHover'); $on.width(opts.rating * opts.size);
        }).click(function (e) {
            var r = Math.round($on.width() / $off.width() * (opts.units * opts.step)) / opts.step;
            $off.unbind('click').unbind('mousemove').unbind('mouseenter').unbind('mouseleave');
            $off.css('cursor', 'default'); $on.css('cursor', 'default');
            opts.id = $this.attr('id');
            $.fn.rater.rate($this, opts, r);
        }).css('cursor', 'pointer'); $on.css('cursor', 'pointer');
    });
};

$.fn.rater.defaults = {
    postHref: location.href,
    units: 5,
    step: 1
};

$.fn.rater.rate = function ($this, opts, rating) {
    var $on = $this.find('.rating_starOn');
    var $off = $this.find('.rating_starOff');
    $off.fadeTo(600, 0.4, function () {
        $.ajax({
            url: opts.postHref,
            type: "POST",
            data: 'id=' + opts.id + '&rating=' + rating,
            complete: function (req) {
                if (req.status == 200) { /* success */
                    opts.rating = parseFloat(req.responseText);

                    if (opts.rating > 0) {
                        opts.rating = parseFloat(req.responseText);
                        $off.fadeTo(200, 0.1, function () {
                            $on.removeClass('rating_starHover').width(opts.rating * opts.size);
                            var $count = $this.find('.rating_count');
                            $count.text(parseInt($count.text()) + 1);
                            $this.find('.rating_value').text(opts.rating.toFixed(1));
                            $off.fadeTo(200, 1);
                        });
                    }
                    else
                    if (opts.rating == -1) {
                        $off.fadeTo(200, 0.6, function () {
                            $this.find('.rating_text').text('Ошибка');
                        });
                    }
                    else {
                        $off.fadeTo(200, 0.6, function () {
                            $this.find('.rating_text').text('Вы уже голосовали!');
                        });
                    }
                } else { /* failure */
                    alert(req.responseText);
                    $on.removeClass('rating_starHover').width(opts.rating * opts.size);
                    $this.rater(opts);
                    $off.fadeTo(2200, 1);
                }
            }
        });
    });
};
