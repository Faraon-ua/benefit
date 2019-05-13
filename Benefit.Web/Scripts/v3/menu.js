
; /* Start:"a:4:{s:4:"full";s:101:"/bitrix/templates/zelenoe/components/bitrix/catalog.section.list/sectionmenu/script.js?15060443584197";s:6:"source";s:86:"/bitrix/templates/zelenoe/components/bitrix/catalog.section.list/sectionmenu/script.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
$(document).ready(function(){

    if( $(window).outerWidth() > 768 ) {

        $('#section-menu').on('mouseover', function () {
            var backLink = $(this).find('#back_link');
            if (backLink.length) {
                if (backLink.is(':hover')) {
                    $(this).find('.section-menu').hide();
                } else {
                    $('#body-overlay').addClass('active');
                    $(this).addClass('active');
                    $(this).find('.section-menu').show();
                }
            } else {
                $('#body-overlay').addClass('active');
                $(this).addClass('active');
            }
        });

    } else{
        $('#section-menu .title').on('click', function() {
            if( $(this.parentNode).hasClass('active') ){
                $('#body-overlay').removeClass('active');
                $(this.parentNode).removeClass('active');
            } else{
                $('#body-overlay').addClass('active');
                $(this.parentNode).addClass('active');
            }
        });
    }

    $('#section-menu').on('mouseleave', function() { $('#body-overlay').removeClass('active'); $(this).removeClass('active'); });

    //768
    var WinWidth = window.innerWidth;


    if( WinWidth > 767 ){
        $('#section-menu .li-depth-1').on('mouseover', function() {
            var liWidth = $(this).outerWidth(),
                childMenu = $(this).find('.child-menu-2');
            $(this).addClass('active');
            childMenu.removeClass('hidden'); childMenu.css({'left':liWidth + 1, 'width':liWidth});
            $(this).on('mouseleave', function(){ childMenu.addClass('hidden'); $(this).removeClass('active'); });
        });

        $('#section-menu .li-depth-2').on('mouseover', function() {
            var liWidth = $(this).outerWidth(),
                childMenu = $(this).find('.child-menu-3');
            $(this).addClass('active');
            childMenu.removeClass('hidden'); childMenu.css({ 'left': liWidth + 1 });
            childMenu.find("img").each(function () {
                var dataSrc = $(this).attr("data-menu-src");
                $(this).attr("src", dataSrc)
            })
            $(this).on('mouseleave', function(){ $(this).removeClass('active'); childMenu.addClass('hidden'); });
        });

        $('#section-menu .li-depth-3').on('mouseover', function() {
            var liWidth = $(this).outerWidth(),
                childMenu = $(this).find('.child-menu-4');
            $(this).addClass('active');
            childMenu.removeClass('hidden'); childMenu.css({'left':liWidth + 1, 'width':liWidth});
            $(this).on('mouseleave', function(){ $(this).removeClass('active'); childMenu.addClass('hidden'); });
        });
    } else{

        $('.next-level').on('click',function() {

            var childMenu = this.parentNode.querySelector('.child-menu');
            var childLi = this.parentNode;
            childMenu.style.zIndex = '200';
            childMenu.style.width = '100%';
            $(childMenu).removeClass('hidden');

            $(childLi).css({
                'position':'absolute',
                'left':'0px',
                'width':'100%',
                'height':'100%',
                'top':'0px'
            });

            $(childMenu).css({'left':'100%'});
            $(childMenu).animate({'left':'0%'},250);


        });

        $('.prev-level').on('click',function() {
            var childMenu = this.parentNode.parentNode;
            var childLi = this.parentNode.parentNode.parentNode;


            $(childMenu).animate({'left':'100%'},250);
            setTimeout(function(){
                childLi.setAttribute('style','');
                childMenu.setAttribute('style','');
                $(childMenu).addClass('hidden');
            },270);

           /* $(childLi).css({
                'position':'inherit',
                'left':'0px',
                'width':'100%',
                'height':'auto',
                'top':'0px'
            }); */

            console.log( childMenu );

        });

    }


    /*$('#back_link').on('mouseover', function(){
        var catalogUL = this.parentNode.parentNode.querySelector('.section-menu');
     $('#body-overlay').removeClass('active');
     $(catalogUL).removeClass('active');
    });*/

});


/* End */
;; /* /bitrix/templates/zelenoe/components/bitrix/catalog.section.list/sectionmenu/script.js?15060443584197*/
