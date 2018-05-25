
; /* Start:"a:4:{s:4:"full";s:66:"/bitrix/templates/elasto_start/js/bootstrap.min.js?145950725436868";s:6:"source";s:50:"/bitrix/templates/elasto_start/js/bootstrap.min.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
/*!
 * Bootstrap v3.3.6 (http://getbootstrap.com)
 * Copyright 2011-2015 Twitter, Inc.
 * Licensed under the MIT license
 */
if("undefined"==typeof jQuery)throw new Error("Bootstrap's JavaScript requires jQuery");+function(a){"use strict";var b=a.fn.jquery.split(" ")[0].split(".");if(b[0]<2&&b[1]<9||1==b[0]&&9==b[1]&&b[2]<1||b[0]>2)throw new Error("Bootstrap's JavaScript requires jQuery version 1.9.1 or higher, but lower than version 3")}(jQuery),+function(a){"use strict";function b(){var a=document.createElement("bootstrap"),b={WebkitTransition:"webkitTransitionEnd",MozTransition:"transitionend",OTransition:"oTransitionEnd otransitionend",transition:"transitionend"};for(var c in b)if(void 0!==a.style[c])return{end:b[c]};return!1}a.fn.emulateTransitionEnd=function(b){var c=!1,d=this;a(this).one("bsTransitionEnd",function(){c=!0});var e=function(){c||a(d).trigger(a.support.transition.end)};return setTimeout(e,b),this},a(function(){a.support.transition=b(),a.support.transition&&(a.event.special.bsTransitionEnd={bindType:a.support.transition.end,delegateType:a.support.transition.end,handle:function(b){return a(b.target).is(this)?b.handleObj.handler.apply(this,arguments):void 0}})})}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var c=a(this),e=c.data("bs.alert");e||c.data("bs.alert",e=new d(this)),"string"==typeof b&&e[b].call(c)})}var c='[data-dismiss="alert"]',d=function(b){a(b).on("click",c,this.close)};d.VERSION="3.3.6",d.TRANSITION_DURATION=150,d.prototype.close=function(b){function c(){g.detach().trigger("closed.bs.alert").remove()}var e=a(this),f=e.attr("data-target");f||(f=e.attr("href"),f=f&&f.replace(/.*(?=#[^\s]*$)/,""));var g=a(f);b&&b.preventDefault(),g.length||(g=e.closest(".alert")),g.trigger(b=a.Event("close.bs.alert")),b.isDefaultPrevented()||(g.removeClass("in"),a.support.transition&&g.hasClass("fade")?g.one("bsTransitionEnd",c).emulateTransitionEnd(d.TRANSITION_DURATION):c())};var e=a.fn.alert;a.fn.alert=b,a.fn.alert.Constructor=d,a.fn.alert.noConflict=function(){return a.fn.alert=e,this},a(document).on("click.bs.alert.data-api",c,d.prototype.close)}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var d=a(this),e=d.data("bs.button"),f="object"==typeof b&&b;e||d.data("bs.button",e=new c(this,f)),"toggle"==b?e.toggle():b&&e.setState(b)})}var c=function(b,d){this.$element=a(b),this.options=a.extend({},c.DEFAULTS,d),this.isLoading=!1};c.VERSION="3.3.6",c.DEFAULTS={loadingText:"loading..."},c.prototype.setState=function(b){var c="disabled",d=this.$element,e=d.is("input")?"val":"html",f=d.data();b+="Text",null==f.resetText&&d.data("resetText",d[e]()),setTimeout(a.proxy(function(){d[e](null==f[b]?this.options[b]:f[b]),"loadingText"==b?(this.isLoading=!0,d.addClass(c).attr(c,c)):this.isLoading&&(this.isLoading=!1,d.removeClass(c).removeAttr(c))},this),0)},c.prototype.toggle=function(){var a=!0,b=this.$element.closest('[data-toggle="buttons"]');if(b.length){var c=this.$element.find("input");"radio"==c.prop("type")?(c.prop("checked")&&(a=!1),b.find(".active").removeClass("active"),this.$element.addClass("active")):"checkbox"==c.prop("type")&&(c.prop("checked")!==this.$element.hasClass("active")&&(a=!1),this.$element.toggleClass("active")),c.prop("checked",this.$element.hasClass("active")),a&&c.trigger("change")}else this.$element.attr("aria-pressed",!this.$element.hasClass("active")),this.$element.toggleClass("active")};var d=a.fn.button;a.fn.button=b,a.fn.button.Constructor=c,a.fn.button.noConflict=function(){return a.fn.button=d,this},a(document).on("click.bs.button.data-api",'[data-toggle^="button"]',function(c){var d=a(c.target);d.hasClass("btn")||(d=d.closest(".btn")),b.call(d,"toggle"),a(c.target).is('input[type="radio"]')||a(c.target).is('input[type="checkbox"]')||c.preventDefault()}).on("focus.bs.button.data-api blur.bs.button.data-api",'[data-toggle^="button"]',function(b){a(b.target).closest(".btn").toggleClass("focus",/^focus(in)?$/.test(b.type))})}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var d=a(this),e=d.data("bs.carousel"),f=a.extend({},c.DEFAULTS,d.data(),"object"==typeof b&&b),g="string"==typeof b?b:f.slide;e||d.data("bs.carousel",e=new c(this,f)),"number"==typeof b?e.to(b):g?e[g]():f.interval&&e.pause().cycle()})}var c=function(b,c){this.$element=a(b),this.$indicators=this.$element.find(".carousel-indicators"),this.options=c,this.paused=null,this.sliding=null,this.interval=null,this.$active=null,this.$items=null,this.options.keyboard&&this.$element.on("keydown.bs.carousel",a.proxy(this.keydown,this)),"hover"==this.options.pause&&!("ontouchstart"in document.documentElement)&&this.$element.on("mouseenter.bs.carousel",a.proxy(this.pause,this)).on("mouseleave.bs.carousel",a.proxy(this.cycle,this))};c.VERSION="3.3.6",c.TRANSITION_DURATION=600,c.DEFAULTS={interval:5e3,pause:"hover",wrap:!0,keyboard:!0},c.prototype.keydown=function(a){if(!/input|textarea/i.test(a.target.tagName)){switch(a.which){case 37:this.prev();break;case 39:this.next();break;default:return}a.preventDefault()}},c.prototype.cycle=function(b){return b||(this.paused=!1),this.interval&&clearInterval(this.interval),this.options.interval&&!this.paused&&(this.interval=setInterval(a.proxy(this.next,this),this.options.interval)),this},c.prototype.getItemIndex=function(a){return this.$items=a.parent().children(".item"),this.$items.index(a||this.$active)},c.prototype.getItemForDirection=function(a,b){var c=this.getItemIndex(b),d="prev"==a&&0===c||"next"==a&&c==this.$items.length-1;if(d&&!this.options.wrap)return b;var e="prev"==a?-1:1,f=(c+e)%this.$items.length;return this.$items.eq(f)},c.prototype.to=function(a){var b=this,c=this.getItemIndex(this.$active=this.$element.find(".item.active"));return a>this.$items.length-1||0>a?void 0:this.sliding?this.$element.one("slid.bs.carousel",function(){b.to(a)}):c==a?this.pause().cycle():this.slide(a>c?"next":"prev",this.$items.eq(a))},c.prototype.pause=function(b){return b||(this.paused=!0),this.$element.find(".next, .prev").length&&a.support.transition&&(this.$element.trigger(a.support.transition.end),this.cycle(!0)),this.interval=clearInterval(this.interval),this},c.prototype.next=function(){return this.sliding?void 0:this.slide("next")},c.prototype.prev=function(){return this.sliding?void 0:this.slide("prev")},c.prototype.slide=function(b,d){var e=this.$element.find(".item.active"),f=d||this.getItemForDirection(b,e),g=this.interval,h="next"==b?"left":"right",i=this;if(f.hasClass("active"))return this.sliding=!1;var j=f[0],k=a.Event("slide.bs.carousel",{relatedTarget:j,direction:h});if(this.$element.trigger(k),!k.isDefaultPrevented()){if(this.sliding=!0,g&&this.pause(),this.$indicators.length){this.$indicators.find(".active").removeClass("active");var l=a(this.$indicators.children()[this.getItemIndex(f)]);l&&l.addClass("active")}var m=a.Event("slid.bs.carousel",{relatedTarget:j,direction:h});return a.support.transition&&this.$element.hasClass("slide")?(f.addClass(b),f[0].offsetWidth,e.addClass(h),f.addClass(h),e.one("bsTransitionEnd",function(){f.removeClass([b,h].join(" ")).addClass("active"),e.removeClass(["active",h].join(" ")),i.sliding=!1,setTimeout(function(){i.$element.trigger(m)},0)}).emulateTransitionEnd(c.TRANSITION_DURATION)):(e.removeClass("active"),f.addClass("active"),this.sliding=!1,this.$element.trigger(m)),g&&this.cycle(),this}};var d=a.fn.carousel;a.fn.carousel=b,a.fn.carousel.Constructor=c,a.fn.carousel.noConflict=function(){return a.fn.carousel=d,this};var e=function(c){var d,e=a(this),f=a(e.attr("data-target")||(d=e.attr("href"))&&d.replace(/.*(?=#[^\s]+$)/,""));if(f.hasClass("carousel")){var g=a.extend({},f.data(),e.data()),h=e.attr("data-slide-to");h&&(g.interval=!1),b.call(f,g),h&&f.data("bs.carousel").to(h),c.preventDefault()}};a(document).on("click.bs.carousel.data-api","[data-slide]",e).on("click.bs.carousel.data-api","[data-slide-to]",e),a(window).on("load",function(){a('[data-ride="carousel"]').each(function(){var c=a(this);b.call(c,c.data())})})}(jQuery),+function(a){"use strict";function b(b){var c,d=b.attr("data-target")||(c=b.attr("href"))&&c.replace(/.*(?=#[^\s]+$)/,"");return a(d)}function c(b){return this.each(function(){var c=a(this),e=c.data("bs.collapse"),f=a.extend({},d.DEFAULTS,c.data(),"object"==typeof b&&b);!e&&f.toggle&&/show|hide/.test(b)&&(f.toggle=!1),e||c.data("bs.collapse",e=new d(this,f)),"string"==typeof b&&e[b]()})}var d=function(b,c){this.$element=a(b),this.options=a.extend({},d.DEFAULTS,c),this.$trigger=a('[data-toggle="collapse"][href="#'+b.id+'"],[data-toggle="collapse"][data-target="#'+b.id+'"]'),this.transitioning=null,this.options.parent?this.$parent=this.getParent():this.addAriaAndCollapsedClass(this.$element,this.$trigger),this.options.toggle&&this.toggle()};d.VERSION="3.3.6",d.TRANSITION_DURATION=350,d.DEFAULTS={toggle:!0},d.prototype.dimension=function(){var a=this.$element.hasClass("width");return a?"width":"height"},d.prototype.show=function(){if(!this.transitioning&&!this.$element.hasClass("in")){var b,e=this.$parent&&this.$parent.children(".panel").children(".in, .collapsing");if(!(e&&e.length&&(b=e.data("bs.collapse"),b&&b.transitioning))){var f=a.Event("show.bs.collapse");if(this.$element.trigger(f),!f.isDefaultPrevented()){e&&e.length&&(c.call(e,"hide"),b||e.data("bs.collapse",null));var g=this.dimension();this.$element.removeClass("collapse").addClass("collapsing")[g](0).attr("aria-expanded",!0),this.$trigger.removeClass("collapsed").attr("aria-expanded",!0),this.transitioning=1;var h=function(){this.$element.removeClass("collapsing").addClass("collapse in")[g](""),this.transitioning=0,this.$element.trigger("shown.bs.collapse")};if(!a.support.transition)return h.call(this);var i=a.camelCase(["scroll",g].join("-"));this.$element.one("bsTransitionEnd",a.proxy(h,this)).emulateTransitionEnd(d.TRANSITION_DURATION)[g](this.$element[0][i])}}}},d.prototype.hide=function(){if(!this.transitioning&&this.$element.hasClass("in")){var b=a.Event("hide.bs.collapse");if(this.$element.trigger(b),!b.isDefaultPrevented()){var c=this.dimension();this.$element[c](this.$element[c]())[0].offsetHeight,this.$element.addClass("collapsing").removeClass("collapse in").attr("aria-expanded",!1),this.$trigger.addClass("collapsed").attr("aria-expanded",!1),this.transitioning=1;var e=function(){this.transitioning=0,this.$element.removeClass("collapsing").addClass("collapse").trigger("hidden.bs.collapse")};return a.support.transition?void this.$element[c](0).one("bsTransitionEnd",a.proxy(e,this)).emulateTransitionEnd(d.TRANSITION_DURATION):e.call(this)}}},d.prototype.toggle=function(){this[this.$element.hasClass("in")?"hide":"show"]()},d.prototype.getParent=function(){return a(this.options.parent).find('[data-toggle="collapse"][data-parent="'+this.options.parent+'"]').each(a.proxy(function(c,d){var e=a(d);this.addAriaAndCollapsedClass(b(e),e)},this)).end()},d.prototype.addAriaAndCollapsedClass=function(a,b){var c=a.hasClass("in");a.attr("aria-expanded",c),b.toggleClass("collapsed",!c).attr("aria-expanded",c)};var e=a.fn.collapse;a.fn.collapse=c,a.fn.collapse.Constructor=d,a.fn.collapse.noConflict=function(){return a.fn.collapse=e,this},a(document).on("click.bs.collapse.data-api",'[data-toggle="collapse"]',function(d){var e=a(this);e.attr("data-target")||d.preventDefault();var f=b(e),g=f.data("bs.collapse"),h=g?"toggle":e.data();c.call(f,h)})}(jQuery),+function(a){"use strict";function b(b){var c=b.attr("data-target");c||(c=b.attr("href"),c=c&&/#[A-Za-z]/.test(c)&&c.replace(/.*(?=#[^\s]*$)/,""));var d=c&&a(c);return d&&d.length?d:b.parent()}function c(c){c&&3===c.which||(a(e).remove(),a(f).each(function(){var d=a(this),e=b(d),f={relatedTarget:this};e.hasClass("open")&&(c&&"click"==c.type&&/input|textarea/i.test(c.target.tagName)&&a.contains(e[0],c.target)||(e.trigger(c=a.Event("hide.bs.dropdown",f)),c.isDefaultPrevented()||(d.attr("aria-expanded","false"),e.removeClass("open").trigger(a.Event("hidden.bs.dropdown",f)))))}))}function d(b){return this.each(function(){var c=a(this),d=c.data("bs.dropdown");d||c.data("bs.dropdown",d=new g(this)),"string"==typeof b&&d[b].call(c)})}var e=".dropdown-backdrop",f='[data-toggle="dropdown"]',g=function(b){a(b).on("click.bs.dropdown",this.toggle)};g.VERSION="3.3.6",g.prototype.toggle=function(d){var e=a(this);if(!e.is(".disabled, :disabled")){var f=b(e),g=f.hasClass("open");if(c(),!g){"ontouchstart"in document.documentElement&&!f.closest(".navbar-nav").length&&a(document.createElement("div")).addClass("dropdown-backdrop").insertAfter(a(this)).on("click",c);var h={relatedTarget:this};if(f.trigger(d=a.Event("show.bs.dropdown",h)),d.isDefaultPrevented())return;e.trigger("focus").attr("aria-expanded","true"),f.toggleClass("open").trigger(a.Event("shown.bs.dropdown",h))}return!1}},g.prototype.keydown=function(c){if(/(38|40|27|32)/.test(c.which)&&!/input|textarea/i.test(c.target.tagName)){var d=a(this);if(c.preventDefault(),c.stopPropagation(),!d.is(".disabled, :disabled")){var e=b(d),g=e.hasClass("open");if(!g&&27!=c.which||g&&27==c.which)return 27==c.which&&e.find(f).trigger("focus"),d.trigger("click");var h=" li:not(.disabled):visible a",i=e.find(".dropdown-menu"+h);if(i.length){var j=i.index(c.target);38==c.which&&j>0&&j--,40==c.which&&j<i.length-1&&j++,~j||(j=0),i.eq(j).trigger("focus")}}}};var h=a.fn.dropdown;a.fn.dropdown=d,a.fn.dropdown.Constructor=g,a.fn.dropdown.noConflict=function(){return a.fn.dropdown=h,this},a(document).on("click.bs.dropdown.data-api",c).on("click.bs.dropdown.data-api",".dropdown form",function(a){a.stopPropagation()}).on("click.bs.dropdown.data-api",f,g.prototype.toggle).on("keydown.bs.dropdown.data-api",f,g.prototype.keydown).on("keydown.bs.dropdown.data-api",".dropdown-menu",g.prototype.keydown)}(jQuery),+function(a){"use strict";function b(b,d){return this.each(function(){var e=a(this),f=e.data("bs.modal"),g=a.extend({},c.DEFAULTS,e.data(),"object"==typeof b&&b);f||e.data("bs.modal",f=new c(this,g)),"string"==typeof b?f[b](d):g.show&&f.show(d)})}var c=function(b,c){this.options=c,this.$body=a(document.body),this.$element=a(b),this.$dialog=this.$element.find(".modal-dialog"),this.$backdrop=null,this.isShown=null,this.originalBodyPad=null,this.scrollbarWidth=0,this.ignoreBackdropClick=!1,this.options.remote&&this.$element.find(".modal-content").load(this.options.remote,a.proxy(function(){this.$element.trigger("loaded.bs.modal")},this))};c.VERSION="3.3.6",c.TRANSITION_DURATION=300,c.BACKDROP_TRANSITION_DURATION=150,c.DEFAULTS={backdrop:!0,keyboard:!0,show:!0},c.prototype.toggle=function(a){return this.isShown?this.hide():this.show(a)},c.prototype.show=function(b){var d=this,e=a.Event("show.bs.modal",{relatedTarget:b});this.$element.trigger(e),this.isShown||e.isDefaultPrevented()||(this.isShown=!0,this.checkScrollbar(),this.setScrollbar(),this.$body.addClass("modal-open"),this.escape(),this.resize(),this.$element.on("click.dismiss.bs.modal",'[data-dismiss="modal"]',a.proxy(this.hide,this)),this.$dialog.on("mousedown.dismiss.bs.modal",function(){d.$element.one("mouseup.dismiss.bs.modal",function(b){a(b.target).is(d.$element)&&(d.ignoreBackdropClick=!0)})}),this.backdrop(function(){var e=a.support.transition&&d.$element.hasClass("fade");d.$element.parent().length||d.$element.appendTo(d.$body),d.$element.show().scrollTop(0),d.adjustDialog(),e&&d.$element[0].offsetWidth,d.$element.addClass("in"),d.enforceFocus();var f=a.Event("shown.bs.modal",{relatedTarget:b});e?d.$dialog.one("bsTransitionEnd",function(){d.$element.trigger("focus").trigger(f)}).emulateTransitionEnd(c.TRANSITION_DURATION):d.$element.trigger("focus").trigger(f)}))},c.prototype.hide=function(b){b&&b.preventDefault(),b=a.Event("hide.bs.modal"),this.$element.trigger(b),this.isShown&&!b.isDefaultPrevented()&&(this.isShown=!1,this.escape(),this.resize(),a(document).off("focusin.bs.modal"),this.$element.removeClass("in").off("click.dismiss.bs.modal").off("mouseup.dismiss.bs.modal"),this.$dialog.off("mousedown.dismiss.bs.modal"),a.support.transition&&this.$element.hasClass("fade")?this.$element.one("bsTransitionEnd",a.proxy(this.hideModal,this)).emulateTransitionEnd(c.TRANSITION_DURATION):this.hideModal())},c.prototype.enforceFocus=function(){a(document).off("focusin.bs.modal").on("focusin.bs.modal",a.proxy(function(a){this.$element[0]===a.target||this.$element.has(a.target).length||this.$element.trigger("focus")},this))},c.prototype.escape=function(){this.isShown&&this.options.keyboard?this.$element.on("keydown.dismiss.bs.modal",a.proxy(function(a){27==a.which&&this.hide()},this)):this.isShown||this.$element.off("keydown.dismiss.bs.modal")},c.prototype.resize=function(){this.isShown?a(window).on("resize.bs.modal",a.proxy(this.handleUpdate,this)):a(window).off("resize.bs.modal")},c.prototype.hideModal=function(){var a=this;this.$element.hide(),this.backdrop(function(){a.$body.removeClass("modal-open"),a.resetAdjustments(),a.resetScrollbar(),a.$element.trigger("hidden.bs.modal")})},c.prototype.removeBackdrop=function(){this.$backdrop&&this.$backdrop.remove(),this.$backdrop=null},c.prototype.backdrop=function(b){var d=this,e=this.$element.hasClass("fade")?"fade":"";if(this.isShown&&this.options.backdrop){var f=a.support.transition&&e;if(this.$backdrop=a(document.createElement("div")).addClass("modal-backdrop "+e).appendTo(this.$body),this.$element.on("click.dismiss.bs.modal",a.proxy(function(a){return this.ignoreBackdropClick?void(this.ignoreBackdropClick=!1):void(a.target===a.currentTarget&&("static"==this.options.backdrop?this.$element[0].focus():this.hide()))},this)),f&&this.$backdrop[0].offsetWidth,this.$backdrop.addClass("in"),!b)return;f?this.$backdrop.one("bsTransitionEnd",b).emulateTransitionEnd(c.BACKDROP_TRANSITION_DURATION):b()}else if(!this.isShown&&this.$backdrop){this.$backdrop.removeClass("in");var g=function(){d.removeBackdrop(),b&&b()};a.support.transition&&this.$element.hasClass("fade")?this.$backdrop.one("bsTransitionEnd",g).emulateTransitionEnd(c.BACKDROP_TRANSITION_DURATION):g()}else b&&b()},c.prototype.handleUpdate=function(){this.adjustDialog()},c.prototype.adjustDialog=function(){var a=this.$element[0].scrollHeight>document.documentElement.clientHeight;this.$element.css({paddingLeft:!this.bodyIsOverflowing&&a?this.scrollbarWidth:"",paddingRight:this.bodyIsOverflowing&&!a?this.scrollbarWidth:""})},c.prototype.resetAdjustments=function(){this.$element.css({paddingLeft:"",paddingRight:""})},c.prototype.checkScrollbar=function(){var a=window.innerWidth;if(!a){var b=document.documentElement.getBoundingClientRect();a=b.right-Math.abs(b.left)}this.bodyIsOverflowing=document.body.clientWidth<a,this.scrollbarWidth=this.measureScrollbar()},c.prototype.setScrollbar=function(){var a=parseInt(this.$body.css("padding-right")||0,10);this.originalBodyPad=document.body.style.paddingRight||"",this.bodyIsOverflowing&&this.$body.css("padding-right",a+this.scrollbarWidth)},c.prototype.resetScrollbar=function(){this.$body.css("padding-right",this.originalBodyPad)},c.prototype.measureScrollbar=function(){var a=document.createElement("div");a.className="modal-scrollbar-measure",this.$body.append(a);var b=a.offsetWidth-a.clientWidth;return this.$body[0].removeChild(a),b};var d=a.fn.modal;a.fn.modal=b,a.fn.modal.Constructor=c,a.fn.modal.noConflict=function(){return a.fn.modal=d,this},a(document).on("click.bs.modal.data-api",'[data-toggle="modal"]',function(c){var d=a(this),e=d.attr("href"),f=a(d.attr("data-target")||e&&e.replace(/.*(?=#[^\s]+$)/,"")),g=f.data("bs.modal")?"toggle":a.extend({remote:!/#/.test(e)&&e},f.data(),d.data());d.is("a")&&c.preventDefault(),f.one("show.bs.modal",function(a){a.isDefaultPrevented()||f.one("hidden.bs.modal",function(){d.is(":visible")&&d.trigger("focus")})}),b.call(f,g,this)})}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var d=a(this),e=d.data("bs.tooltip"),f="object"==typeof b&&b;(e||!/destroy|hide/.test(b))&&(e||d.data("bs.tooltip",e=new c(this,f)),"string"==typeof b&&e[b]())})}var c=function(a,b){this.type=null,this.options=null,this.enabled=null,this.timeout=null,this.hoverState=null,this.$element=null,this.inState=null,this.init("tooltip",a,b)};c.VERSION="3.3.6",c.TRANSITION_DURATION=150,c.DEFAULTS={animation:!0,placement:"top",selector:!1,template:'<div class="tooltip" role="tooltip"><div class="tooltip-arrow"></div><div class="tooltip-inner"></div></div>',trigger:"hover focus",title:"",delay:0,html:!1,container:!1,viewport:{selector:"body",padding:0}},c.prototype.init=function(b,c,d){if(this.enabled=!0,this.type=b,this.$element=a(c),this.options=this.getOptions(d),this.$viewport=this.options.viewport&&a(a.isFunction(this.options.viewport)?this.options.viewport.call(this,this.$element):this.options.viewport.selector||this.options.viewport),this.inState={click:!1,hover:!1,focus:!1},this.$element[0]instanceof document.constructor&&!this.options.selector)throw new Error("`selector` option must be specified when initializing "+this.type+" on the window.document object!");for(var e=this.options.trigger.split(" "),f=e.length;f--;){var g=e[f];if("click"==g)this.$element.on("click."+this.type,this.options.selector,a.proxy(this.toggle,this));else if("manual"!=g){var h="hover"==g?"mouseenter":"focusin",i="hover"==g?"mouseleave":"focusout";this.$element.on(h+"."+this.type,this.options.selector,a.proxy(this.enter,this)),this.$element.on(i+"."+this.type,this.options.selector,a.proxy(this.leave,this))}}this.options.selector?this._options=a.extend({},this.options,{trigger:"manual",selector:""}):this.fixTitle()},c.prototype.getDefaults=function(){return c.DEFAULTS},c.prototype.getOptions=function(b){return b=a.extend({},this.getDefaults(),this.$element.data(),b),b.delay&&"number"==typeof b.delay&&(b.delay={show:b.delay,hide:b.delay}),b},c.prototype.getDelegateOptions=function(){var b={},c=this.getDefaults();return this._options&&a.each(this._options,function(a,d){c[a]!=d&&(b[a]=d)}),b},c.prototype.enter=function(b){var c=b instanceof this.constructor?b:a(b.currentTarget).data("bs."+this.type);return c||(c=new this.constructor(b.currentTarget,this.getDelegateOptions()),a(b.currentTarget).data("bs."+this.type,c)),b instanceof a.Event&&(c.inState["focusin"==b.type?"focus":"hover"]=!0),c.tip().hasClass("in")||"in"==c.hoverState?void(c.hoverState="in"):(clearTimeout(c.timeout),c.hoverState="in",c.options.delay&&c.options.delay.show?void(c.timeout=setTimeout(function(){"in"==c.hoverState&&c.show()},c.options.delay.show)):c.show())},c.prototype.isInStateTrue=function(){for(var a in this.inState)if(this.inState[a])return!0;return!1},c.prototype.leave=function(b){var c=b instanceof this.constructor?b:a(b.currentTarget).data("bs."+this.type);return c||(c=new this.constructor(b.currentTarget,this.getDelegateOptions()),a(b.currentTarget).data("bs."+this.type,c)),b instanceof a.Event&&(c.inState["focusout"==b.type?"focus":"hover"]=!1),c.isInStateTrue()?void 0:(clearTimeout(c.timeout),c.hoverState="out",c.options.delay&&c.options.delay.hide?void(c.timeout=setTimeout(function(){"out"==c.hoverState&&c.hide()},c.options.delay.hide)):c.hide())},c.prototype.show=function(){var b=a.Event("show.bs."+this.type);if(this.hasContent()&&this.enabled){this.$element.trigger(b);var d=a.contains(this.$element[0].ownerDocument.documentElement,this.$element[0]);if(b.isDefaultPrevented()||!d)return;var e=this,f=this.tip(),g=this.getUID(this.type);this.setContent(),f.attr("id",g),this.$element.attr("aria-describedby",g),this.options.animation&&f.addClass("fade");var h="function"==typeof this.options.placement?this.options.placement.call(this,f[0],this.$element[0]):this.options.placement,i=/\s?auto?\s?/i,j=i.test(h);j&&(h=h.replace(i,"")||"top"),f.detach().css({top:0,left:0,display:"block"}).addClass(h).data("bs."+this.type,this),this.options.container?f.appendTo(this.options.container):f.insertAfter(this.$element),this.$element.trigger("inserted.bs."+this.type);var k=this.getPosition(),l=f[0].offsetWidth,m=f[0].offsetHeight;if(j){var n=h,o=this.getPosition(this.$viewport);h="bottom"==h&&k.bottom+m>o.bottom?"top":"top"==h&&k.top-m<o.top?"bottom":"right"==h&&k.right+l>o.width?"left":"left"==h&&k.left-l<o.left?"right":h,f.removeClass(n).addClass(h)}var p=this.getCalculatedOffset(h,k,l,m);this.applyPlacement(p,h);var q=function(){var a=e.hoverState;e.$element.trigger("shown.bs."+e.type),e.hoverState=null,"out"==a&&e.leave(e)};a.support.transition&&this.$tip.hasClass("fade")?f.one("bsTransitionEnd",q).emulateTransitionEnd(c.TRANSITION_DURATION):q()}},c.prototype.applyPlacement=function(b,c){var d=this.tip(),e=d[0].offsetWidth,f=d[0].offsetHeight,g=parseInt(d.css("margin-top"),10),h=parseInt(d.css("margin-left"),10);isNaN(g)&&(g=0),isNaN(h)&&(h=0),b.top+=g,b.left+=h,a.offset.setOffset(d[0],a.extend({using:function(a){d.css({top:Math.round(a.top),left:Math.round(a.left)})}},b),0),d.addClass("in");var i=d[0].offsetWidth,j=d[0].offsetHeight;"top"==c&&j!=f&&(b.top=b.top+f-j);var k=this.getViewportAdjustedDelta(c,b,i,j);k.left?b.left+=k.left:b.top+=k.top;var l=/top|bottom/.test(c),m=l?2*k.left-e+i:2*k.top-f+j,n=l?"offsetWidth":"offsetHeight";d.offset(b),this.replaceArrow(m,d[0][n],l)},c.prototype.replaceArrow=function(a,b,c){this.arrow().css(c?"left":"top",50*(1-a/b)+"%").css(c?"top":"left","")},c.prototype.setContent=function(){var a=this.tip(),b=this.getTitle();a.find(".tooltip-inner")[this.options.html?"html":"text"](b),a.removeClass("fade in top bottom left right")},c.prototype.hide=function(b){function d(){"in"!=e.hoverState&&f.detach(),e.$element.removeAttr("aria-describedby").trigger("hidden.bs."+e.type),b&&b()}var e=this,f=a(this.$tip),g=a.Event("hide.bs."+this.type);return this.$element.trigger(g),g.isDefaultPrevented()?void 0:(f.removeClass("in"),a.support.transition&&f.hasClass("fade")?f.one("bsTransitionEnd",d).emulateTransitionEnd(c.TRANSITION_DURATION):d(),this.hoverState=null,this)},c.prototype.fixTitle=function(){var a=this.$element;(a.attr("title")||"string"!=typeof a.attr("data-original-title"))&&a.attr("data-original-title",a.attr("title")||"").attr("title","")},c.prototype.hasContent=function(){return this.getTitle()},c.prototype.getPosition=function(b){b=b||this.$element;var c=b[0],d="BODY"==c.tagName,e=c.getBoundingClientRect();null==e.width&&(e=a.extend({},e,{width:e.right-e.left,height:e.bottom-e.top}));var f=d?{top:0,left:0}:b.offset(),g={scroll:d?document.documentElement.scrollTop||document.body.scrollTop:b.scrollTop()},h=d?{width:a(window).width(),height:a(window).height()}:null;return a.extend({},e,g,h,f)},c.prototype.getCalculatedOffset=function(a,b,c,d){return"bottom"==a?{top:b.top+b.height,left:b.left+b.width/2-c/2}:"top"==a?{top:b.top-d,left:b.left+b.width/2-c/2}:"left"==a?{top:b.top+b.height/2-d/2,left:b.left-c}:{top:b.top+b.height/2-d/2,left:b.left+b.width}},c.prototype.getViewportAdjustedDelta=function(a,b,c,d){var e={top:0,left:0};if(!this.$viewport)return e;var f=this.options.viewport&&this.options.viewport.padding||0,g=this.getPosition(this.$viewport);if(/right|left/.test(a)){var h=b.top-f-g.scroll,i=b.top+f-g.scroll+d;h<g.top?e.top=g.top-h:i>g.top+g.height&&(e.top=g.top+g.height-i)}else{var j=b.left-f,k=b.left+f+c;j<g.left?e.left=g.left-j:k>g.right&&(e.left=g.left+g.width-k)}return e},c.prototype.getTitle=function(){var a,b=this.$element,c=this.options;return a=b.attr("data-original-title")||("function"==typeof c.title?c.title.call(b[0]):c.title)},c.prototype.getUID=function(a){do a+=~~(1e6*Math.random());while(document.getElementById(a));return a},c.prototype.tip=function(){if(!this.$tip&&(this.$tip=a(this.options.template),1!=this.$tip.length))throw new Error(this.type+" `template` option must consist of exactly 1 top-level element!");return this.$tip},c.prototype.arrow=function(){return this.$arrow=this.$arrow||this.tip().find(".tooltip-arrow")},c.prototype.enable=function(){this.enabled=!0},c.prototype.disable=function(){this.enabled=!1},c.prototype.toggleEnabled=function(){this.enabled=!this.enabled},c.prototype.toggle=function(b){var c=this;b&&(c=a(b.currentTarget).data("bs."+this.type),c||(c=new this.constructor(b.currentTarget,this.getDelegateOptions()),a(b.currentTarget).data("bs."+this.type,c))),b?(c.inState.click=!c.inState.click,c.isInStateTrue()?c.enter(c):c.leave(c)):c.tip().hasClass("in")?c.leave(c):c.enter(c)},c.prototype.destroy=function(){var a=this;clearTimeout(this.timeout),this.hide(function(){a.$element.off("."+a.type).removeData("bs."+a.type),a.$tip&&a.$tip.detach(),a.$tip=null,a.$arrow=null,a.$viewport=null})};var d=a.fn.tooltip;a.fn.tooltip=b,a.fn.tooltip.Constructor=c,a.fn.tooltip.noConflict=function(){return a.fn.tooltip=d,this}}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var d=a(this),e=d.data("bs.popover"),f="object"==typeof b&&b;(e||!/destroy|hide/.test(b))&&(e||d.data("bs.popover",e=new c(this,f)),"string"==typeof b&&e[b]())})}var c=function(a,b){this.init("popover",a,b)};if(!a.fn.tooltip)throw new Error("Popover requires tooltip.js");c.VERSION="3.3.6",c.DEFAULTS=a.extend({},a.fn.tooltip.Constructor.DEFAULTS,{placement:"right",trigger:"click",content:"",template:'<div class="popover" role="tooltip"><div class="arrow"></div><h3 class="popover-title"></h3><div class="popover-content"></div></div>'}),c.prototype=a.extend({},a.fn.tooltip.Constructor.prototype),c.prototype.constructor=c,c.prototype.getDefaults=function(){return c.DEFAULTS},c.prototype.setContent=function(){var a=this.tip(),b=this.getTitle(),c=this.getContent();a.find(".popover-title")[this.options.html?"html":"text"](b),a.find(".popover-content").children().detach().end()[this.options.html?"string"==typeof c?"html":"append":"text"](c),a.removeClass("fade top bottom left right in"),a.find(".popover-title").html()||a.find(".popover-title").hide()},c.prototype.hasContent=function(){return this.getTitle()||this.getContent()},c.prototype.getContent=function(){var a=this.$element,b=this.options;return a.attr("data-content")||("function"==typeof b.content?b.content.call(a[0]):b.content)},c.prototype.arrow=function(){return this.$arrow=this.$arrow||this.tip().find(".arrow")};var d=a.fn.popover;a.fn.popover=b,a.fn.popover.Constructor=c,a.fn.popover.noConflict=function(){return a.fn.popover=d,this}}(jQuery),+function(a){"use strict";function b(c,d){this.$body=a(document.body),this.$scrollElement=a(a(c).is(document.body)?window:c),this.options=a.extend({},b.DEFAULTS,d),this.selector=(this.options.target||"")+" .nav li > a",this.offsets=[],this.targets=[],this.activeTarget=null,this.scrollHeight=0,this.$scrollElement.on("scroll.bs.scrollspy",a.proxy(this.process,this)),this.refresh(),this.process()}function c(c){return this.each(function(){var d=a(this),e=d.data("bs.scrollspy"),f="object"==typeof c&&c;e||d.data("bs.scrollspy",e=new b(this,f)),"string"==typeof c&&e[c]()})}b.VERSION="3.3.6",b.DEFAULTS={offset:10},b.prototype.getScrollHeight=function(){return this.$scrollElement[0].scrollHeight||Math.max(this.$body[0].scrollHeight,document.documentElement.scrollHeight)},b.prototype.refresh=function(){var b=this,c="offset",d=0;this.offsets=[],this.targets=[],this.scrollHeight=this.getScrollHeight(),a.isWindow(this.$scrollElement[0])||(c="position",d=this.$scrollElement.scrollTop()),this.$body.find(this.selector).map(function(){var b=a(this),e=b.data("target")||b.attr("href"),f=/^#./.test(e)&&a(e);return f&&f.length&&f.is(":visible")&&[[f[c]().top+d,e]]||null}).sort(function(a,b){return a[0]-b[0]}).each(function(){b.offsets.push(this[0]),b.targets.push(this[1])})},b.prototype.process=function(){var a,b=this.$scrollElement.scrollTop()+this.options.offset,c=this.getScrollHeight(),d=this.options.offset+c-this.$scrollElement.height(),e=this.offsets,f=this.targets,g=this.activeTarget;if(this.scrollHeight!=c&&this.refresh(),b>=d)return g!=(a=f[f.length-1])&&this.activate(a);if(g&&b<e[0])return this.activeTarget=null,this.clear();for(a=e.length;a--;)g!=f[a]&&b>=e[a]&&(void 0===e[a+1]||b<e[a+1])&&this.activate(f[a])},b.prototype.activate=function(b){this.activeTarget=b,this.clear();var c=this.selector+'[data-target="'+b+'"],'+this.selector+'[href="'+b+'"]',d=a(c).parents("li").addClass("active");
d.parent(".dropdown-menu").length&&(d=d.closest("li.dropdown").addClass("active")),d.trigger("activate.bs.scrollspy")},b.prototype.clear=function(){a(this.selector).parentsUntil(this.options.target,".active").removeClass("active")};var d=a.fn.scrollspy;a.fn.scrollspy=c,a.fn.scrollspy.Constructor=b,a.fn.scrollspy.noConflict=function(){return a.fn.scrollspy=d,this},a(window).on("load.bs.scrollspy.data-api",function(){a('[data-spy="scroll"]').each(function(){var b=a(this);c.call(b,b.data())})})}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var d=a(this),e=d.data("bs.tab");e||d.data("bs.tab",e=new c(this)),"string"==typeof b&&e[b]()})}var c=function(b){this.element=a(b)};c.VERSION="3.3.6",c.TRANSITION_DURATION=150,c.prototype.show=function(){var b=this.element,c=b.closest("ul:not(.dropdown-menu)"),d=b.data("target");if(d||(d=b.attr("href"),d=d&&d.replace(/.*(?=#[^\s]*$)/,"")),!b.parent("li").hasClass("active")){var e=c.find(".active:last a"),f=a.Event("hide.bs.tab",{relatedTarget:b[0]}),g=a.Event("show.bs.tab",{relatedTarget:e[0]});if(e.trigger(f),b.trigger(g),!g.isDefaultPrevented()&&!f.isDefaultPrevented()){var h=a(d);this.activate(b.closest("li"),c),this.activate(h,h.parent(),function(){e.trigger({type:"hidden.bs.tab",relatedTarget:b[0]}),b.trigger({type:"shown.bs.tab",relatedTarget:e[0]})})}}},c.prototype.activate=function(b,d,e){function f(){g.removeClass("active").find("> .dropdown-menu > .active").removeClass("active").end().find('[data-toggle="tab"]').attr("aria-expanded",!1),b.addClass("active").find('[data-toggle="tab"]').attr("aria-expanded",!0),h?(b[0].offsetWidth,b.addClass("in")):b.removeClass("fade"),b.parent(".dropdown-menu").length&&b.closest("li.dropdown").addClass("active").end().find('[data-toggle="tab"]').attr("aria-expanded",!0),e&&e()}var g=d.find("> .active"),h=e&&a.support.transition&&(g.length&&g.hasClass("fade")||!!d.find("> .fade").length);g.length&&h?g.one("bsTransitionEnd",f).emulateTransitionEnd(c.TRANSITION_DURATION):f(),g.removeClass("in")};var d=a.fn.tab;a.fn.tab=b,a.fn.tab.Constructor=c,a.fn.tab.noConflict=function(){return a.fn.tab=d,this};var e=function(c){c.preventDefault(),b.call(a(this),"show")};a(document).on("click.bs.tab.data-api",'[data-toggle="tab"]',e).on("click.bs.tab.data-api",'[data-toggle="pill"]',e)}(jQuery),+function(a){"use strict";function b(b){return this.each(function(){var d=a(this),e=d.data("bs.affix"),f="object"==typeof b&&b;e||d.data("bs.affix",e=new c(this,f)),"string"==typeof b&&e[b]()})}var c=function(b,d){this.options=a.extend({},c.DEFAULTS,d),this.$target=a(this.options.target).on("scroll.bs.affix.data-api",a.proxy(this.checkPosition,this)).on("click.bs.affix.data-api",a.proxy(this.checkPositionWithEventLoop,this)),this.$element=a(b),this.affixed=null,this.unpin=null,this.pinnedOffset=null,this.checkPosition()};c.VERSION="3.3.6",c.RESET="affix affix-top affix-bottom",c.DEFAULTS={offset:0,target:window},c.prototype.getState=function(a,b,c,d){var e=this.$target.scrollTop(),f=this.$element.offset(),g=this.$target.height();if(null!=c&&"top"==this.affixed)return c>e?"top":!1;if("bottom"==this.affixed)return null!=c?e+this.unpin<=f.top?!1:"bottom":a-d>=e+g?!1:"bottom";var h=null==this.affixed,i=h?e:f.top,j=h?g:b;return null!=c&&c>=e?"top":null!=d&&i+j>=a-d?"bottom":!1},c.prototype.getPinnedOffset=function(){if(this.pinnedOffset)return this.pinnedOffset;this.$element.removeClass(c.RESET).addClass("affix");var a=this.$target.scrollTop(),b=this.$element.offset();return this.pinnedOffset=b.top-a},c.prototype.checkPositionWithEventLoop=function(){setTimeout(a.proxy(this.checkPosition,this),1)},c.prototype.checkPosition=function(){if(this.$element.is(":visible")){var b=this.$element.height(),d=this.options.offset,e=d.top,f=d.bottom,g=Math.max(a(document).height(),a(document.body).height());"object"!=typeof d&&(f=e=d),"function"==typeof e&&(e=d.top(this.$element)),"function"==typeof f&&(f=d.bottom(this.$element));var h=this.getState(g,b,e,f);if(this.affixed!=h){null!=this.unpin&&this.$element.css("top","");var i="affix"+(h?"-"+h:""),j=a.Event(i+".bs.affix");if(this.$element.trigger(j),j.isDefaultPrevented())return;this.affixed=h,this.unpin="bottom"==h?this.getPinnedOffset():null,this.$element.removeClass(c.RESET).addClass(i).trigger(i.replace("affix","affixed")+".bs.affix")}"bottom"==h&&this.$element.offset({top:g-b-f})}};var d=a.fn.affix;a.fn.affix=b,a.fn.affix.Constructor=c,a.fn.affix.noConflict=function(){return a.fn.affix=d,this},a(window).on("load",function(){a('[data-spy="affix"]').each(function(){var c=a(this),d=c.data();d.offset=d.offset||{},null!=d.offsetBottom&&(d.offset.bottom=d.offsetBottom),null!=d.offsetTop&&(d.offset.top=d.offsetTop),b.call(c,d)})})}(jQuery);
/* End */
;
; /* Start:"a:4:{s:4:"full";s:72:"/bitrix/templates/elasto_start/js/formValidation.min.js?1474014364129735";s:6:"source";s:55:"/bitrix/templates/elasto_start/js/formValidation.min.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
if(window.FormValidation={AddOn:{},Framework:{},I18n:{},Validator:{}},"undefined"==typeof jQuery)throw new Error("FormValidation requires jQuery");!function(a){var b=a.fn.jquery.split(" ")[0].split(".");if(+b[0]<2&&+b[1]<9||1===+b[0]&&9===+b[1]&&+b[2]<1)throw new Error("FormValidation requires jQuery version 1.9.1 or higher")}(jQuery),function(a){FormValidation.Base=function(b,c,d){this.$form=a(b),this.options=a.extend({},a.fn.formValidation.DEFAULT_OPTIONS,c),this._namespace=d||"fv",this.$invalidFields=a([]),this.$submitButton=null,this.$hiddenButton=null,this.STATUS_NOT_VALIDATED="NOT_VALIDATED",this.STATUS_VALIDATING="VALIDATING",this.STATUS_INVALID="INVALID",this.STATUS_VALID="VALID",this.STATUS_IGNORED="IGNORED",this.DEFAULT_MESSAGE=a.fn.formValidation.DEFAULT_MESSAGE,this._ieVersion=function(){for(var a=3,b=document.createElement("div"),c=b.all||[];b.innerHTML="<!--[if gt IE "+ ++a+"]><br><![endif]-->",c[0];);return a>4?a:document.documentMode}();var e=document.createElement("div");this._changeEvent=9!==this._ieVersion&&"oninput"in e?"input":"keyup",this._submitIfValid=null,this._cacheFields={},this._init()},FormValidation.Base.prototype={constructor:FormValidation.Base,_exceedThreshold:function(b){var c=this._namespace,d=b.attr("data-"+c+"-field"),e=this.options.fields[d].threshold||this.options.threshold;if(!e)return!0;var f=-1!==a.inArray(b.attr("type"),["button","checkbox","file","hidden","image","radio","reset","submit"]);return f||b.val().length>=e},_init:function(){var b=this,c=this._namespace,d={addOns:{},autoFocus:this.$form.attr("data-"+c+"-autofocus"),button:{selector:this.$form.attr("data-"+c+"-button-selector")||this.$form.attr("data-"+c+"-submitbuttons"),disabled:this.$form.attr("data-"+c+"-button-disabled")},control:{valid:this.$form.attr("data-"+c+"-control-valid"),invalid:this.$form.attr("data-"+c+"-control-invalid")},err:{clazz:this.$form.attr("data-"+c+"-err-clazz"),container:this.$form.attr("data-"+c+"-err-container")||this.$form.attr("data-"+c+"-container"),parent:this.$form.attr("data-"+c+"-err-parent")},events:{formInit:this.$form.attr("data-"+c+"-events-form-init"),formPreValidate:this.$form.attr("data-"+c+"-events-form-prevalidate"),formError:this.$form.attr("data-"+c+"-events-form-error"),formReset:this.$form.attr("data-"+c+"-events-form-reset"),formSuccess:this.$form.attr("data-"+c+"-events-form-success"),fieldAdded:this.$form.attr("data-"+c+"-events-field-added"),fieldRemoved:this.$form.attr("data-"+c+"-events-field-removed"),fieldInit:this.$form.attr("data-"+c+"-events-field-init"),fieldError:this.$form.attr("data-"+c+"-events-field-error"),fieldReset:this.$form.attr("data-"+c+"-events-field-reset"),fieldSuccess:this.$form.attr("data-"+c+"-events-field-success"),fieldStatus:this.$form.attr("data-"+c+"-events-field-status"),localeChanged:this.$form.attr("data-"+c+"-events-locale-changed"),validatorError:this.$form.attr("data-"+c+"-events-validator-error"),validatorSuccess:this.$form.attr("data-"+c+"-events-validator-success"),validatorIgnored:this.$form.attr("data-"+c+"-events-validator-ignored")},excluded:this.$form.attr("data-"+c+"-excluded"),icon:{valid:this.$form.attr("data-"+c+"-icon-valid")||this.$form.attr("data-"+c+"-feedbackicons-valid"),invalid:this.$form.attr("data-"+c+"-icon-invalid")||this.$form.attr("data-"+c+"-feedbackicons-invalid"),validating:this.$form.attr("data-"+c+"-icon-validating")||this.$form.attr("data-"+c+"-feedbackicons-validating"),feedback:this.$form.attr("data-"+c+"-icon-feedback")},live:this.$form.attr("data-"+c+"-live"),locale:this.$form.attr("data-"+c+"-locale"),message:this.$form.attr("data-"+c+"-message"),onPreValidate:this.$form.attr("data-"+c+"-onprevalidate"),onError:this.$form.attr("data-"+c+"-onerror"),onReset:this.$form.attr("data-"+c+"-onreset"),onSuccess:this.$form.attr("data-"+c+"-onsuccess"),row:{selector:this.$form.attr("data-"+c+"-row-selector")||this.$form.attr("data-"+c+"-group"),valid:this.$form.attr("data-"+c+"-row-valid"),invalid:this.$form.attr("data-"+c+"-row-invalid"),feedback:this.$form.attr("data-"+c+"-row-feedback")},threshold:this.$form.attr("data-"+c+"-threshold"),trigger:this.$form.attr("data-"+c+"-trigger"),verbose:this.$form.attr("data-"+c+"-verbose"),fields:{}};this.$form.attr("novalidate","novalidate").addClass(this.options.elementClass).on("submit."+c,function(a){a.preventDefault(),b.validate()}).on("click."+c,this.options.button.selector,function(){b.$submitButton=a(this),b._submitIfValid=!0}),(this.options.declarative===!0||"true"===this.options.declarative)&&this.$form.find("[name], [data-"+c+"-field]").each(function(){var e=a(this),f=e.attr("name")||e.attr("data-"+c+"-field"),g=b._parseOptions(e);g&&(e.attr("data-"+c+"-field",f),d.fields[f]=a.extend({},g,d.fields[f]))}),this.options=a.extend(!0,this.options,d),"string"==typeof this.options.err.parent&&(this.options.err.parent=new RegExp(this.options.err.parent)),this.options.container&&(this.options.err.container=this.options.container,delete this.options.container),this.options.feedbackIcons&&(this.options.icon=a.extend(!0,this.options.icon,this.options.feedbackIcons),delete this.options.feedbackIcons),this.options.group&&(this.options.row.selector=this.options.group,delete this.options.group),this.options.submitButtons&&(this.options.button.selector=this.options.submitButtons,delete this.options.submitButtons),FormValidation.I18n[this.options.locale]||(this.options.locale=a.fn.formValidation.DEFAULT_OPTIONS.locale),(this.options.declarative===!0||"true"===this.options.declarative)&&(this.options=a.extend(!0,this.options,{addOns:this._parseAddOnOptions()})),this.$hiddenButton=a("<button/>").attr("type","submit").prependTo(this.$form).addClass("fv-hidden-submit").css({display:"none",width:0,height:0}),this.$form.on("click."+this._namespace,'[type="submit"]',function(c){if(!c.isDefaultPrevented()){var d=a(c.target),e=d.is('[type="submit"]')?d.eq(0):d.parent('[type="submit"]').eq(0);if(b.options.button.selector&&!e.is(b.options.button.selector)&&!e.is(b.$hiddenButton))return b.$form.off("submit."+b._namespace).submit(),!1}});for(var e in this.options.fields)this._initField(e);for(var f in this.options.addOns)"function"==typeof FormValidation.AddOn[f].init&&FormValidation.AddOn[f].init(this,this.options.addOns[f]);this.$form.trigger(a.Event(this.options.events.formInit),{bv:this,fv:this,options:this.options}),this.options.onPreValidate&&this.$form.on(this.options.events.formPreValidate,function(a){FormValidation.Helper.call(b.options.onPreValidate,[a])}),this.options.onSuccess&&this.$form.on(this.options.events.formSuccess,function(a){FormValidation.Helper.call(b.options.onSuccess,[a])}),this.options.onError&&this.$form.on(this.options.events.formError,function(a){FormValidation.Helper.call(b.options.onError,[a])}),this.options.onReset&&this.$form.on(this.options.events.formReset,function(a){FormValidation.Helper.call(b.options.onReset,[a])})},_initField:function(b){var c=this._namespace,d=a([]);switch(typeof b){case"object":d=b,b=b.attr("data-"+c+"-field");break;case"string":d=this.getFieldElements(b),d.attr("data-"+c+"-field",b)}if(0!==d.length&&null!==this.options.fields[b]&&null!==this.options.fields[b].validators){var e,f,g=this.options.fields[b].validators;for(e in g)f=g[e].alias||e,FormValidation.Validator[f]||delete this.options.fields[b].validators[e];null===this.options.fields[b].enabled&&(this.options.fields[b].enabled=!0);for(var h=this,i=d.length,j=d.attr("type"),k=1===i||"radio"===j||"checkbox"===j,l=this._getFieldTrigger(d.eq(0)),m=this.options.err.clazz.split(" ").join("."),n=a.map(l,function(a){return a+".update."+c}).join(" "),o=0;i>o;o++){var p=d.eq(o),q=this.options.fields[b].row||this.options.row.selector,r=p.closest(q),s="function"==typeof(this.options.fields[b].container||this.options.fields[b].err||this.options.err.container)?(this.options.fields[b].container||this.options.fields[b].err||this.options.err.container).call(this,p,this):this.options.fields[b].container||this.options.fields[b].err||this.options.err.container,t=s&&"tooltip"!==s&&"popover"!==s?a(s):this._getMessageContainer(p,q);s&&"tooltip"!==s&&"popover"!==s&&t.addClass(this.options.err.clazz),t.find("."+m+"[data-"+c+"-validator][data-"+c+'-for="'+b+'"]').remove(),r.find("i[data-"+c+'-icon-for="'+b+'"]').remove(),p.off(n).on(n,function(){h.updateStatus(a(this),h.STATUS_NOT_VALIDATED)}),p.data(c+".messages",t);for(e in g)p.data(c+".result."+e,this.STATUS_NOT_VALIDATED),k&&o!==i-1||a("<small/>").css("display","none").addClass(this.options.err.clazz).attr("data-"+c+"-validator",e).attr("data-"+c+"-for",b).attr("data-"+c+"-result",this.STATUS_NOT_VALIDATED).html(this._getMessage(b,e)).appendTo(t),f=g[e].alias||e,"function"==typeof FormValidation.Validator[f].init&&FormValidation.Validator[f].init(this,p,this.options.fields[b].validators[e],e);if(this.options.fields[b].icon!==!1&&"false"!==this.options.fields[b].icon&&this.options.icon&&this.options.icon.valid&&this.options.icon.invalid&&this.options.icon.validating&&(!k||o===i-1)){r.addClass(this.options.row.feedback);var u=a("<i/>").css("display","none").addClass(this.options.icon.feedback).attr("data-"+c+"-icon-for",b).insertAfter(p);(k?d:p).data(c+".icon",u),("tooltip"===s||"popover"===s)&&((k?d:p).on(this.options.events.fieldError,function(){r.addClass("fv-has-tooltip")}).on(this.options.events.fieldSuccess,function(){r.removeClass("fv-has-tooltip")}),p.off("focus.container."+c).on("focus.container."+c,function(){h._showTooltip(a(this),s)}).off("blur.container."+c).on("blur.container."+c,function(){h._hideTooltip(a(this),s)})),"string"==typeof this.options.fields[b].icon&&"true"!==this.options.fields[b].icon?u.appendTo(a(this.options.fields[b].icon)):this._fixIcon(p,u)}}var v=[];for(e in g)f=g[e].alias||e,g[e].priority=parseInt(g[e].priority||FormValidation.Validator[f].priority||1,10),v.push({validator:e,priority:g[e].priority});v=v.sort(function(a,b){return a.priority-b.priority}),d.data(c+".validators",v).on(this.options.events.fieldSuccess,function(a,b){var c=h.getOptions(b.field,null,"onSuccess");c&&FormValidation.Helper.call(c,[a,b])}).on(this.options.events.fieldError,function(a,b){var c=h.getOptions(b.field,null,"onError");c&&FormValidation.Helper.call(c,[a,b])}).on(this.options.events.fieldReset,function(a,b){var c=h.getOptions(b.field,null,"onReset");c&&FormValidation.Helper.call(c,[a,b])}).on(this.options.events.fieldStatus,function(a,b){var c=h.getOptions(b.field,null,"onStatus");c&&FormValidation.Helper.call(c,[a,b])}).on(this.options.events.validatorError,function(a,b){var c=h.getOptions(b.field,b.validator,"onError");c&&FormValidation.Helper.call(c,[a,b])}).on(this.options.events.validatorIgnored,function(a,b){var c=h.getOptions(b.field,b.validator,"onIgnored");c&&FormValidation.Helper.call(c,[a,b])}).on(this.options.events.validatorSuccess,function(a,b){var c=h.getOptions(b.field,b.validator,"onSuccess");c&&FormValidation.Helper.call(c,[a,b])}),this.onLiveChange(d,"live",function(){h._exceedThreshold(a(this))&&h.validateField(a(this))}),d.trigger(a.Event(this.options.events.fieldInit),{bv:this,fv:this,field:b,element:d})}},_isExcluded:function(b){var c=this._namespace,d=b.attr("data-"+c+"-excluded"),e=b.attr("data-"+c+"-field")||b.attr("name");switch(!0){case!!e&&this.options.fields&&this.options.fields[e]&&("true"===this.options.fields[e].excluded||this.options.fields[e].excluded===!0):case"true"===d:case""===d:return!0;case!!e&&this.options.fields&&this.options.fields[e]&&("false"===this.options.fields[e].excluded||this.options.fields[e].excluded===!1):case"false"===d:return!1;case!!e&&this.options.fields&&this.options.fields[e]&&"function"==typeof this.options.fields[e].excluded:return this.options.fields[e].excluded.call(this,b,this);case!!e&&this.options.fields&&this.options.fields[e]&&"string"==typeof this.options.fields[e].excluded:case d:return FormValidation.Helper.call(this.options.fields[e].excluded,[b,this]);default:if(this.options.excluded){"string"==typeof this.options.excluded&&(this.options.excluded=a.map(this.options.excluded.split(","),function(b){return a.trim(b)}));for(var f=this.options.excluded.length,g=0;f>g;g++)if("string"==typeof this.options.excluded[g]&&b.is(this.options.excluded[g])||"function"==typeof this.options.excluded[g]&&this.options.excluded[g].call(this,b,this)===!0)return!0}return!1}},_getFieldTrigger:function(a){var b=this._namespace,c=a.data(b+".trigger");if(c)return c;var d=a.attr("type"),e=a.attr("data-"+b+"-field"),f="radio"===d||"checkbox"===d||"file"===d||"SELECT"===a.get(0).tagName?"change":this._ieVersion>=10&&a.attr("placeholder")?"keyup":this._changeEvent;return c=((this.options.fields[e]?this.options.fields[e].trigger:null)||this.options.trigger||f).split(" "),a.data(b+".trigger",c),c},_getMessage:function(a,b){if(!this.options.fields[a]||!this.options.fields[a].validators)return"";var c=this.options.fields[a].validators,d=c[b]&&c[b].alias?c[b].alias:b;if(!FormValidation.Validator[d])return"";switch(!0){case!!c[b].message:return c[b].message;case!!this.options.fields[a].message:return this.options.fields[a].message;case!!this.options.message:return this.options.message;case!!FormValidation.I18n[this.options.locale]&&!!FormValidation.I18n[this.options.locale][d]&&!!FormValidation.I18n[this.options.locale][d]["default"]:return FormValidation.I18n[this.options.locale][d]["default"];default:return this.DEFAULT_MESSAGE}},_getMessageContainer:function(a,b){if(!this.options.err.parent)throw new Error("The err.parent option is not defined");var c=a.parent();if(c.is(b))return c;var d=c.attr("class");return d&&this.options.err.parent.test(d)?c:this._getMessageContainer(c,b)},_parseAddOnOptions:function(){var a=this._namespace,b=this.$form.attr("data-"+a+"-addons"),c=this.options.addOns||{};if(b){b=b.replace(/\s/g,"").split(",");for(var d=0;d<b.length;d++)c[b[d]]||(c[b[d]]={})}var e,f,g,h;for(e in c)if(FormValidation.AddOn[e]){if(f=FormValidation.AddOn[e].html5Attributes)for(g in f)h=this.$form.attr("data-"+a+"-addons-"+e.toLowerCase()+"-"+g.toLowerCase()),h&&(c[e][f[g]]=h)}else delete c[e];return c},_parseOptions:function(b){var c,d,e,f,g,h,i,j,k,l=this._namespace,m=b.attr("name")||b.attr("data-"+l+"-field"),n={},o=new RegExp("^data-"+l+"-([a-z]+)-alias$"),p=a.extend({},FormValidation.Validator);a.each(b.get(0).attributes,function(a,b){b.value&&o.test(b.name)&&(d=b.name.split("-")[2],p[b.value]&&(p[d]=p[b.value],p[d].alias=b.value))});for(d in p)if(c=p[d],e="data-"+l+"-"+d.toLowerCase(),f=b.attr(e)+"",k="function"==typeof c.enableByHtml5?c.enableByHtml5(b):null,k&&"false"!==f||k!==!0&&(""===f||"true"===f||e===f.toLowerCase())){c.html5Attributes=a.extend({},{message:"message",onerror:"onError",onreset:"onReset",onsuccess:"onSuccess",priority:"priority",transformer:"transformer"},c.html5Attributes),n[d]=a.extend({},k===!0?{}:k,n[d]),c.alias&&(n[d].alias=c.alias);for(j in c.html5Attributes)g=c.html5Attributes[j],h="data-"+l+"-"+d.toLowerCase()+"-"+j,i=b.attr(h),i&&("true"===i||h===i.toLowerCase()?i=!0:"false"===i&&(i=!1),n[d][g]=i)}var q={autoFocus:b.attr("data-"+l+"-autofocus"),err:b.attr("data-"+l+"-err-container")||b.attr("data-"+l+"-container"),enabled:b.attr("data-"+l+"-enabled"),excluded:b.attr("data-"+l+"-excluded"),icon:b.attr("data-"+l+"-icon")||b.attr("data-"+l+"-feedbackicons")||(this.options.fields&&this.options.fields[m]?this.options.fields[m].feedbackIcons:null),message:b.attr("data-"+l+"-message"),onError:b.attr("data-"+l+"-onerror"),onReset:b.attr("data-"+l+"-onreset"),onStatus:b.attr("data-"+l+"-onstatus"),onSuccess:b.attr("data-"+l+"-onsuccess"),row:b.attr("data-"+l+"-row")||b.attr("data-"+l+"-group")||(this.options.fields&&this.options.fields[m]?this.options.fields[m].group:null),selector:b.attr("data-"+l+"-selector"),threshold:b.attr("data-"+l+"-threshold"),transformer:b.attr("data-"+l+"-transformer"),trigger:b.attr("data-"+l+"-trigger"),verbose:b.attr("data-"+l+"-verbose"),validators:n},r=a.isEmptyObject(q),s=a.isEmptyObject(n);return!s||!r&&this.options.fields&&this.options.fields[m]?q:null},_submit:function(){var b=this.isValid();if(null!==b){var c=b?this.options.events.formSuccess:this.options.events.formError,d=a.Event(c);this.$form.trigger(d),this.$submitButton&&(b?this._onSuccess(d):this._onError(d))}},_onError:function(b){if(!b.isDefaultPrevented()){if("submitted"===this.options.live){this.options.live="enabled";var c=this;for(var d in this.options.fields)!function(b){var d=c.getFieldElements(b);d.length&&c.onLiveChange(d,"live",function(){c._exceedThreshold(a(this))&&c.validateField(a(this))})}(d)}for(var e=this._namespace,f=0;f<this.$invalidFields.length;f++){var g=this.$invalidFields.eq(f),h=this.isOptionEnabled(g.attr("data-"+e+"-field"),"autoFocus");if(h){g.focus();break}}}},_onFieldValidated:function(b,c){var d=this._namespace,e=b.attr("data-"+d+"-field"),f=this.options.fields[e].validators,g={},h=0,i={bv:this,fv:this,field:e,element:b,validator:c,result:b.data(d+".response."+c)};if(c)switch(b.data(d+".result."+c)){case this.STATUS_INVALID:b.trigger(a.Event(this.options.events.validatorError),i);break;case this.STATUS_VALID:b.trigger(a.Event(this.options.events.validatorSuccess),i);break;case this.STATUS_IGNORED:b.trigger(a.Event(this.options.events.validatorIgnored),i)}g[this.STATUS_NOT_VALIDATED]=0,g[this.STATUS_VALIDATING]=0,g[this.STATUS_INVALID]=0,g[this.STATUS_VALID]=0,g[this.STATUS_IGNORED]=0;for(var j in f)if(f[j].enabled!==!1){h++;var k=b.data(d+".result."+j);k&&g[k]++}g[this.STATUS_VALID]+g[this.STATUS_IGNORED]===h?(this.$invalidFields=this.$invalidFields.not(b),b.trigger(a.Event(this.options.events.fieldSuccess),i)):(0===g[this.STATUS_NOT_VALIDATED]||!this.isOptionEnabled(e,"verbose"))&&0===g[this.STATUS_VALIDATING]&&g[this.STATUS_INVALID]>0&&(this.$invalidFields=this.$invalidFields.add(b),b.trigger(a.Event(this.options.events.fieldError),i))},_onSuccess:function(a){a.isDefaultPrevented()||this.disableSubmitButtons(!0).defaultSubmit()},_fixIcon:function(a,b){},_createTooltip:function(a,b,c){},_destroyTooltip:function(a,b){},_hideTooltip:function(a,b){},_showTooltip:function(a,b){},defaultSubmit:function(){var b=this._namespace;this.$submitButton&&a("<input/>").attr({type:"hidden",name:this.$submitButton.attr("name")}).attr("data-"+b+"-submit-hidden","").val(this.$submitButton.val()).appendTo(this.$form),this.$form.off("submit."+b).submit()},disableSubmitButtons:function(a){return a?"disabled"!==this.options.live&&this.$form.find(this.options.button.selector).attr("disabled","disabled").addClass(this.options.button.disabled):this.$form.find(this.options.button.selector).removeAttr("disabled").removeClass(this.options.button.disabled),this},getFieldElements:function(b){if(!this._cacheFields[b])if(this.options.fields[b]&&this.options.fields[b].selector){var c=this.$form.find(this.options.fields[b].selector);this._cacheFields[b]=c.length?c:a(this.options.fields[b].selector)}else this._cacheFields[b]=this.$form.find('[name="'+b+'"]');return this._cacheFields[b]},getFieldValue:function(a,b){var c,d=this._namespace;if("string"==typeof a){if(c=this.getFieldElements(a),0===c.length)return null}else c=a,a=c.attr("data-"+d+"-field");if(!a||!this.options.fields[a])return c.val();var e=(this.options.fields[a].validators&&this.options.fields[a].validators[b]?this.options.fields[a].validators[b].transformer:null)||this.options.fields[a].transformer;return e?FormValidation.Helper.call(e,[c,b,this]):c.val()},getNamespace:function(){return this._namespace},getOptions:function(a,b,c){var d=this._namespace;if(!a)return c?this.options[c]:this.options;if("object"==typeof a&&(a=a.attr("data-"+d+"-field")),!this.options.fields[a])return null;var e=this.options.fields[a];return b?e.validators&&e.validators[b]?c?e.validators[b][c]:e.validators[b]:null:c?e[c]:e},getStatus:function(a,b){var c=this._namespace;switch(typeof a){case"object":return a.data(c+".result."+b);case"string":default:return this.getFieldElements(a).eq(0).data(c+".result."+b)}},isOptionEnabled:function(a,b){return!this.options.fields[a]||"true"!==this.options.fields[a][b]&&this.options.fields[a][b]!==!0?!this.options.fields[a]||"false"!==this.options.fields[a][b]&&this.options.fields[a][b]!==!1?"true"===this.options[b]||this.options[b]===!0:!1:!0},isValid:function(){for(var a in this.options.fields){var b=this.isValidField(a);if(null===b)return null;if(b===!1)return!1}return!0},isValidContainer:function(b){var c=this,d=this._namespace,e=[],f="string"==typeof b?a(b):b;if(0===f.length)return!0;f.find("[data-"+d+"-field]").each(function(){var b=a(this);c._isExcluded(b)||e.push(b)});for(var g=e.length,h=this.options.err.clazz.split(" ").join("."),i=0;g>i;i++){var j=e[i],k=j.attr("data-"+d+"-field"),l=j.data(d+".messages").find("."+h+"[data-"+d+"-validator][data-"+d+'-for="'+k+'"]');if(!this.options.fields||!this.options.fields[k]||"false"!==this.options.fields[k].enabled&&this.options.fields[k].enabled!==!1){if(l.filter("[data-"+d+'-result="'+this.STATUS_INVALID+'"]').length>0)return!1;if(l.filter("[data-"+d+'-result="'+this.STATUS_NOT_VALIDATED+'"]').length>0||l.filter("[data-"+d+'-result="'+this.STATUS_VALIDATING+'"]').length>0)return null}}return!0},isValidField:function(b){var c=this._namespace,d=a([]);switch(typeof b){case"object":d=b,b=b.attr("data-"+c+"-field");break;case"string":d=this.getFieldElements(b)}if(0===d.length||!this.options.fields[b]||"false"===this.options.fields[b].enabled||this.options.fields[b].enabled===!1)return!0;for(var e,f,g,h=d.attr("type"),i="radio"===h||"checkbox"===h?1:d.length,j=0;i>j;j++)if(e=d.eq(j),!this._isExcluded(e))for(f in this.options.fields[b].validators)if(this.options.fields[b].validators[f].enabled!==!1){if(g=e.data(c+".result."+f),g===this.STATUS_VALIDATING||g===this.STATUS_NOT_VALIDATED)return null;if(g===this.STATUS_INVALID)return!1}return!0},offLiveChange:function(b,c){if(null===b||0===b.length)return this;var d=this._namespace,e=this._getFieldTrigger(b.eq(0)),f=a.map(e,function(a){return a+"."+c+"."+d}).join(" ");return b.off(f),this},onLiveChange:function(b,c,d){if(null===b||0===b.length)return this;var e=this._namespace,f=this._getFieldTrigger(b.eq(0)),g=a.map(f,function(a){return a+"."+c+"."+e}).join(" ");switch(this.options.live){case"submitted":break;case"disabled":b.off(g);break;case"enabled":default:b.off(g).on(g,function(a){d.apply(this,arguments)})}return this},updateMessage:function(b,c,d){var e=this._namespace,f=a([]);switch(typeof b){case"object":f=b,b=b.attr("data-"+e+"-field");break;case"string":f=this.getFieldElements(b)}var g=this.options.err.clazz.split(" ").join(".");return f.each(function(){a(this).data(e+".messages").find("."+g+"[data-"+e+'-validator="'+c+'"][data-'+e+'-for="'+b+'"]').html(d)}),this},updateStatus:function(b,c,d){var e=this._namespace,f=a([]);switch(typeof b){case"object":f=b,b=b.attr("data-"+e+"-field");break;case"string":f=this.getFieldElements(b)}if(!b||!this.options.fields[b])return this;c===this.STATUS_NOT_VALIDATED&&(this._submitIfValid=!1);for(var g=this,h=f.attr("type"),i=this.options.fields[b].row||this.options.row.selector,j="radio"===h||"checkbox"===h?1:f.length,k=this.options.err.clazz.split(" ").join("."),l=0;j>l;l++){var m=f.eq(l);if(!this._isExcluded(m)){var n,o,p=m.closest(i),q=m.data(e+".messages"),r=q.find("."+k+"[data-"+e+"-validator][data-"+e+'-for="'+b+'"]'),s=d?r.filter("[data-"+e+'-validator="'+d+'"]'):r,t=m.data(e+".icon"),u="function"==typeof(this.options.fields[b].container||this.options.fields[b].err||this.options.err.container)?(this.options.fields[b].container||this.options.fields[b].err||this.options.err.container).call(this,m,this):this.options.fields[b].container||this.options.fields[b].err||this.options.err.container,v=null;if(d)m.data(e+".result."+d,c);else for(var w in this.options.fields[b].validators)m.data(e+".result."+w,c);switch(s.attr("data-"+e+"-result",c),c){case this.STATUS_VALIDATING:v=null,this.disableSubmitButtons(!0),m.removeClass(this.options.control.valid).removeClass(this.options.control.invalid),p.removeClass(this.options.row.valid).removeClass(this.options.row.invalid),t&&t.removeClass(this.options.icon.valid).removeClass(this.options.icon.invalid).addClass(this.options.icon.validating).show();break;case this.STATUS_INVALID:v=!1,this.disableSubmitButtons(!0),m.removeClass(this.options.control.valid).addClass(this.options.control.invalid),p.removeClass(this.options.row.valid).addClass(this.options.row.invalid),t&&t.removeClass(this.options.icon.valid).removeClass(this.options.icon.validating).addClass(this.options.icon.invalid).show();break;case this.STATUS_IGNORED:case this.STATUS_VALID:n=r.filter("[data-"+e+'-result="'+this.STATUS_VALIDATING+'"]').length>0,o=r.filter("[data-"+e+'-result="'+this.STATUS_NOT_VALIDATED+'"]').length>0;var x=r.filter("[data-"+e+'-result="'+this.STATUS_IGNORED+'"]').length;v=n||o?null:r.filter("[data-"+e+'-result="'+this.STATUS_VALID+'"]').length+x===r.length,m.removeClass(this.options.control.valid).removeClass(this.options.control.invalid),v===!0?(this.disableSubmitButtons(this.isValid()===!1),c===this.STATUS_VALID&&m.addClass(this.options.control.valid)):v===!1&&(this.disableSubmitButtons(!0),c===this.STATUS_VALID&&m.addClass(this.options.control.invalid)),t&&(t.removeClass(this.options.icon.invalid).removeClass(this.options.icon.validating).removeClass(this.options.icon.valid),(c===this.STATUS_VALID||x!==r.length)&&t.addClass(n?this.options.icon.validating:null===v?"":v?this.options.icon.valid:this.options.icon.invalid).show());var y=this.isValidContainer(p);null!==y&&(p.removeClass(this.options.row.valid).removeClass(this.options.row.invalid),(c===this.STATUS_VALID||x!==r.length)&&p.addClass(y?this.options.row.valid:this.options.row.invalid));break;case this.STATUS_NOT_VALIDATED:default:v=null,this.disableSubmitButtons(!1),m.removeClass(this.options.control.valid).removeClass(this.options.control.invalid),p.removeClass(this.options.row.valid).removeClass(this.options.row.invalid),t&&t.removeClass(this.options.icon.valid).removeClass(this.options.icon.invalid).removeClass(this.options.icon.validating).hide()}!t||"tooltip"!==u&&"popover"!==u?c===this.STATUS_INVALID?s.show():s.hide():v===!1?this._createTooltip(m,r.filter("[data-"+e+'-result="'+g.STATUS_INVALID+'"]').eq(0).html(),u):this._destroyTooltip(m,u),m.trigger(a.Event(this.options.events.fieldStatus),{bv:this,fv:this,field:b,element:m,status:c,validator:d}),this._onFieldValidated(m,d)}}return this},validate:function(){if(a.isEmptyObject(this.options.fields))return this._submit(),this;this.$form.trigger(a.Event(this.options.events.formPreValidate)),this.disableSubmitButtons(!0),this._submitIfValid=!1;for(var b in this.options.fields)this.validateField(b);return this._submit(),this._submitIfValid=!0,this},validateField:function(b){var c=this._namespace,d=a([]);switch(typeof b){case"object":d=b,b=b.attr("data-"+c+"-field");break;case"string":d=this.getFieldElements(b)}if(0===d.length||!this.options.fields[b]||"false"===this.options.fields[b].enabled||this.options.fields[b].enabled===!1)return this;for(var e,f,g,h=this,i=d.attr("type"),j="radio"!==i&&"checkbox"!==i||"disabled"===this.options.live?d.length:1,k="radio"===i||"checkbox"===i,l=this.options.fields[b].validators,m=this.isOptionEnabled(b,"verbose"),n=0;j>n;n++){var o=d.eq(n);if(!this._isExcluded(o))for(var p=!1,q=o.data(c+".validators"),r=q.length,s=0;r>s&&(e=q[s].validator,o.data(c+".dfs."+e)&&o.data(c+".dfs."+e).reject(),!p);s++){var t=o.data(c+".result."+e);if(t!==this.STATUS_VALID&&t!==this.STATUS_INVALID)if(l[e].enabled!==!1)if(o.data(c+".result."+e,this.STATUS_VALIDATING),f=l[e].alias||e,g=FormValidation.Validator[f].validate(this,o,l[e],e),"object"==typeof g&&g.resolve)this.updateStatus(k?b:o,this.STATUS_VALIDATING,e),o.data(c+".dfs."+e,g),g.done(function(a,b,d){a.removeData(c+".dfs."+b).data(c+".response."+b,d),d.message&&h.updateMessage(a,b,d.message),h.updateStatus(k?a.attr("data-"+c+"-field"):a,d.valid===!0?h.STATUS_VALID:d.valid===!1?h.STATUS_INVALID:h.STATUS_IGNORED,b),d.valid&&h._submitIfValid===!0?h._submit():d.valid!==!1||m||(p=!0)});else if("object"==typeof g&&void 0!==g.valid){if(o.data(c+".response."+e,g),g.message&&this.updateMessage(k?b:o,e,g.message),this.updateStatus(k?b:o,g.valid===!0?this.STATUS_VALID:g.valid===!1?this.STATUS_INVALID:this.STATUS_IGNORED,e),g.valid===!1&&!m)break}else if("boolean"==typeof g){if(o.data(c+".response."+e,g),this.updateStatus(k?b:o,g?this.STATUS_VALID:this.STATUS_INVALID,e),!g&&!m)break}else null===g&&(o.data(c+".response."+e,g),this.updateStatus(k?b:o,this.STATUS_IGNORED,e));else this.updateStatus(k?b:o,this.STATUS_IGNORED,e);else this._onFieldValidated(o,e)}}return this},addField:function(b,c){var d=this._namespace,e=a([]);switch(typeof b){case"object":e=b,b=b.attr("data-"+d+"-field")||b.attr("name");break;case"string":delete this._cacheFields[b],e=this.getFieldElements(b)}e.attr("data-"+d+"-field",b);for(var f=e.attr("type"),g="radio"===f||"checkbox"===f?1:e.length,h=0;g>h;h++){var i=e.eq(h),j=this._parseOptions(i);j=null===j?c:a.extend(!0,j,c),this.options.fields[b]=a.extend(!0,this.options.fields[b],j),this._cacheFields[b]=this._cacheFields[b]?this._cacheFields[b].add(i):i,this._initField("checkbox"===f||"radio"===f?b:i)}return this.disableSubmitButtons(!1),this.$form.trigger(a.Event(this.options.events.fieldAdded),{field:b,element:e,options:this.options.fields[b]}),this},destroy:function(){var a,b,c,d,e,f,g,h,i=this._namespace;for(b in this.options.fields)for(c=this.getFieldElements(b),a=0;a<c.length;a++){d=c.eq(a);for(e in this.options.fields[b].validators)d.data(i+".dfs."+e)&&d.data(i+".dfs."+e).reject(),d.removeData(i+".result."+e).removeData(i+".response."+e).removeData(i+".dfs."+e),h=this.options.fields[b].validators[e].alias||e,"function"==typeof FormValidation.Validator[h].destroy&&FormValidation.Validator[h].destroy(this,d,this.options.fields[b].validators[e],e)}var j=this.options.err.clazz.split(" ").join(".");for(b in this.options.fields)for(c=this.getFieldElements(b),g=this.options.fields[b].row||this.options.row.selector,a=0;a<c.length;a++){d=c.eq(a);var k=d.data(i+".messages");k&&k.find("."+j+"[data-"+i+"-validator][data-"+i+'-for="'+b+'"]').remove(),d.removeData(i+".messages").removeData(i+".validators").closest(g).removeClass(this.options.row.valid).removeClass(this.options.row.invalid).removeClass(this.options.row.feedback).end().off("."+i).removeAttr("data-"+i+"-field");var l="function"==typeof(this.options.fields[b].container||this.options.fields[b].err||this.options.err.container)?(this.options.fields[b].container||this.options.fields[b].err||this.options.err.container).call(this,d,this):this.options.fields[b].container||this.options.fields[b].err||this.options.err.container;("tooltip"===l||"popover"===l)&&this._destroyTooltip(d,l),f=d.data(i+".icon"),f&&f.remove(),d.removeData(i+".icon").removeData(i+".trigger")}for(var m in this.options.addOns)"function"==typeof FormValidation.AddOn[m].destroy&&FormValidation.AddOn[m].destroy(this,this.options.addOns[m]);this.disableSubmitButtons(!1),this.$hiddenButton.remove(),this.$form.removeClass(this.options.elementClass).off("."+i).removeData("bootstrapValidator").removeData("formValidation").find("[data-"+i+"-submit-hidden]").remove().end().find('[type="submit"]').off("click."+i)},enableFieldValidators:function(a,b,c){var d=this.options.fields[a].validators;if(c&&d&&d[c]&&d[c].enabled!==b)this.options.fields[a].validators[c].enabled=b,this.updateStatus(a,this.STATUS_NOT_VALIDATED,c);else if(!c&&this.options.fields[a].enabled!==b){this.options.fields[a].enabled=b;for(var e in d)this.enableFieldValidators(a,b,e)}return this},getDynamicOption:function(a,b){var c="string"==typeof a?this.getFieldElements(a):a,d=c.val();if("function"==typeof b)return FormValidation.Helper.call(b,[d,this,c]);if("string"==typeof b){var e=this.getFieldElements(b);return e.length?e.val():FormValidation.Helper.call(b,[d,this,c])||b}return null},getForm:function(){return this.$form},getInvalidFields:function(){
return this.$invalidFields},getLocale:function(){return this.options.locale},getMessages:function(b,c){var d=this,e=this._namespace,f=[],g=a([]);switch(!0){case b&&"object"==typeof b:g=b;break;case b&&"string"==typeof b:var h=this.getFieldElements(b);if(h.length>0){var i=h.attr("type");g="radio"===i||"checkbox"===i?h.eq(0):h}break;default:g=this.$invalidFields}var j=c?"[data-"+e+'-validator="'+c+'"]':"",k=this.options.err.clazz.split(" ").join(".");return g.each(function(){f=f.concat(a(this).data(e+".messages").find("."+k+"[data-"+e+'-for="'+a(this).attr("data-"+e+"-field")+'"][data-'+e+'-result="'+d.STATUS_INVALID+'"]'+j).map(function(){var b=a(this).attr("data-"+e+"-validator"),c=a(this).attr("data-"+e+"-for");return d.options.fields[c].validators[b].enabled===!1?"":a(this).html()}).get())}),f},getSubmitButton:function(){return this.$submitButton},removeField:function(b){var c=this._namespace,d=a([]);switch(typeof b){case"object":d=b,b=b.attr("data-"+c+"-field")||b.attr("name"),d.attr("data-"+c+"-field",b);break;case"string":d=this.getFieldElements(b)}if(0===d.length)return this;for(var e=d.attr("type"),f="radio"===e||"checkbox"===e?1:d.length,g=0;f>g;g++){var h=d.eq(g);this.$invalidFields=this.$invalidFields.not(h),this._cacheFields[b]=this._cacheFields[b].not(h)}return this._cacheFields[b]&&0!==this._cacheFields[b].length||delete this.options.fields[b],("checkbox"===e||"radio"===e)&&this._initField(b),this.disableSubmitButtons(!1),this.$form.trigger(a.Event(this.options.events.fieldRemoved),{field:b,element:d}),this},resetField:function(b,c){var d=this._namespace,e=a([]);switch(typeof b){case"object":e=b,b=b.attr("data-"+d+"-field");break;case"string":e=this.getFieldElements(b)}var f=0,g=e.length;if(this.options.fields[b])for(f=0;g>f;f++)for(var h in this.options.fields[b].validators)e.eq(f).removeData(d+".dfs."+h);if(c){var i=e.attr("type");"radio"===i||"checkbox"===i?e.prop("checked",!1).removeAttr("selected"):e.val("")}for(this.updateStatus(b,this.STATUS_NOT_VALIDATED),f=0;g>f;f++)e.eq(f).trigger(a.Event(this.options.events.fieldReset),{fv:this,field:b,element:e.eq(f),resetValue:c});return this},resetForm:function(b){for(var c in this.options.fields)this.resetField(c,b);return this.$invalidFields=a([]),this.$submitButton=null,this.disableSubmitButtons(!1),this.$form.trigger(a.Event(this.options.events.formReset),{fv:this,resetValue:b}),this},revalidateField:function(a){return this.updateStatus(a,this.STATUS_NOT_VALIDATED).validateField(a),this},setLocale:function(b){return this.options.locale=b,this.$form.trigger(a.Event(this.options.events.localeChanged),{locale:b,bv:this,fv:this}),this},updateOption:function(a,b,c,d){var e=this._namespace;return"object"==typeof a&&(a=a.attr("data-"+e+"-field")),this.options.fields[a]&&this.options.fields[a].validators[b]&&(this.options.fields[a].validators[b][c]=d,this.updateStatus(a,this.STATUS_NOT_VALIDATED,b)),this},validateContainer:function(b){var c=this,d=this._namespace,e=[],f="string"==typeof b?a(b):b;if(0===f.length)return this;f.find("[data-"+d+"-field]").each(function(){var b=a(this);c._isExcluded(b)||e.push(b)});for(var g=e.length,h=0;g>h;h++)this.validateField(e[h]);return this}},a.fn.formValidation=function(b){var c=arguments;return this.each(function(){var d=a(this),e=d.data("formValidation"),f="object"==typeof b&&b;if(!e){var g=(f.framework||d.attr("data-fv-framework")||"bootstrap").toLowerCase(),h=g.substr(0,1).toUpperCase()+g.substr(1);if("undefined"==typeof FormValidation.Framework[h])throw new Error("The class FormValidation.Framework."+h+" is not implemented");e=new FormValidation.Framework[h](this,f),d.addClass("fv-form-"+g).data("formValidation",e)}"string"==typeof b&&e[b].apply(e,Array.prototype.slice.call(c,1))})},a.fn.formValidation.Constructor=FormValidation.Base,a.fn.formValidation.DEFAULT_MESSAGE="This value is not valid",a.fn.formValidation.DEFAULT_OPTIONS={autoFocus:!0,declarative:!0,elementClass:"fv-form",events:{formInit:"init.form.fv",formPreValidate:"prevalidate.form.fv",formError:"err.form.fv",formReset:"rst.form.fv",formSuccess:"success.form.fv",fieldAdded:"added.field.fv",fieldRemoved:"removed.field.fv",fieldInit:"init.field.fv",fieldError:"err.field.fv",fieldReset:"rst.field.fv",fieldSuccess:"success.field.fv",fieldStatus:"status.field.fv",localeChanged:"changed.locale.fv",validatorError:"err.validator.fv",validatorSuccess:"success.validator.fv",validatorIgnored:"ignored.validator.fv"},excluded:[":disabled",":hidden",":not(:visible)"],fields:null,live:"enabled",locale:"en_US",message:null,threshold:null,verbose:!0,button:{selector:'[type="submit"]:not([formnovalidate])',disabled:""},control:{valid:"",invalid:""},err:{clazz:"",container:null,parent:null},icon:{valid:null,invalid:null,validating:null,feedback:""},row:{selector:null,valid:"",invalid:"",feedback:""}}}(jQuery),function(a){FormValidation.Helper={call:function(a,b){if("function"==typeof a)return a.apply(this,b);if("string"==typeof a){"()"===a.substring(a.length-2)&&(a=a.substring(0,a.length-2));for(var c=a.split("."),d=c.pop(),e=window,f=0;f<c.length;f++)e=e[c[f]];return"undefined"==typeof e[d]?null:e[d].apply(this,b)}},date:function(a,b,c,d){if(isNaN(a)||isNaN(b)||isNaN(c))return!1;if(c.length>2||b.length>2||a.length>4)return!1;if(c=parseInt(c,10),b=parseInt(b,10),a=parseInt(a,10),1e3>a||a>9999||0>=b||b>12)return!1;var e=[31,28,31,30,31,30,31,31,30,31,30,31];if((a%400===0||a%100!==0&&a%4===0)&&(e[1]=29),0>=c||c>e[b-1])return!1;if(d===!0){var f=new Date,g=f.getFullYear(),h=f.getMonth(),i=f.getDate();return g>a||a===g&&h>b-1||a===g&&b-1===h&&i>c}return!0},format:function(b,c){a.isArray(c)||(c=[c]);for(var d in c)b=b.replace("%s",c[d]);return b},luhn:function(a){for(var b=a.length,c=0,d=[[0,1,2,3,4,5,6,7,8,9],[0,2,4,6,8,1,3,5,7,9]],e=0;b--;)e+=d[c][parseInt(a.charAt(b),10)],c^=1;return e%10===0&&e>0},mod11And10:function(a){for(var b=5,c=a.length,d=0;c>d;d++)b=(2*(b||10)%11+parseInt(a.charAt(d),10))%10;return 1===b},mod37And36:function(a,b){b=b||"0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";for(var c=b.length,d=a.length,e=Math.floor(c/2),f=0;d>f;f++)e=(2*(e||c)%(c+1)+b.indexOf(a.charAt(f)))%c;return 1===e}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{base64:{"default":"Please enter a valid base 64 encoded"}}}),FormValidation.Validator.base64={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:/^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=|[A-Za-z0-9+/]{4})$/.test(e)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{between:{"default":"Please enter a value between %s and %s",notInclusive:"Please enter a value between %s and %s strictly"}}}),FormValidation.Validator.between={html5Attributes:{message:"message",min:"min",max:"max",inclusive:"inclusive"},enableByHtml5:function(a){return"range"===a.attr("type")?{min:a.attr("min"),max:a.attr("max")}:!1},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;f=this._format(f);var g=b.getLocale(),h=a.isNumeric(d.min)?d.min:b.getDynamicOption(c,d.min),i=a.isNumeric(d.max)?d.max:b.getDynamicOption(c,d.max),j=this._format(h),k=this._format(i);return d.inclusive===!0||void 0===d.inclusive?{valid:a.isNumeric(f)&&parseFloat(f)>=j&&parseFloat(f)<=k,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].between["default"],[h,i])}:{valid:a.isNumeric(f)&&parseFloat(f)>j&&parseFloat(f)<k,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].between.notInclusive,[h,i])}},_format:function(a){return(a+"").replace(",",".")}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{bic:{"default":"Please enter a valid BIC number"}}}),FormValidation.Validator.bic={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:/^[a-zA-Z]{6}[a-zA-Z0-9]{2}([a-zA-Z0-9]{3})?$/.test(e)}}}(jQuery),function(a){FormValidation.Validator.blank={validate:function(a,b,c,d){return!0}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{callback:{"default":"Please enter a valid value"}}}),FormValidation.Validator.callback={priority:999,html5Attributes:{message:"message",callback:"callback"},validate:function(b,c,d,e){var f=b.getFieldValue(c,e),g=new a.Deferred,h={valid:!0};if(d.callback){var i=FormValidation.Helper.call(d.callback,[f,b,c]);h="boolean"==typeof i||null===i?{valid:i}:i}return g.resolve(c,e,h),g}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{choice:{"default":"Please enter a valid value",less:"Please choose %s options at minimum",more:"Please choose %s options at maximum",between:"Please choose %s - %s options"}}}),FormValidation.Validator.choice={html5Attributes:{message:"message",min:"min",max:"max"},validate:function(b,c,d,e){var f=b.getLocale(),g=b.getNamespace(),h=c.is("select")?b.getFieldElements(c.attr("data-"+g+"-field")).find("option").filter(":selected").length:b.getFieldElements(c.attr("data-"+g+"-field")).filter(":checked").length,i=d.min?a.isNumeric(d.min)?d.min:b.getDynamicOption(c,d.min):null,j=d.max?a.isNumeric(d.max)?d.max:b.getDynamicOption(c,d.max):null,k=!0,l=d.message||FormValidation.I18n[f].choice["default"];switch((i&&h<parseInt(i,10)||j&&h>parseInt(j,10))&&(k=!1),!0){case!!i&&!!j:l=FormValidation.Helper.format(d.message||FormValidation.I18n[f].choice.between,[parseInt(i,10),parseInt(j,10)]);break;case!!i:l=FormValidation.Helper.format(d.message||FormValidation.I18n[f].choice.less,parseInt(i,10));break;case!!j:l=FormValidation.Helper.format(d.message||FormValidation.I18n[f].choice.more,parseInt(j,10))}return{valid:k,message:l}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{color:{"default":"Please enter a valid color"}}}),FormValidation.Validator.color={html5Attributes:{message:"message",type:"type"},enableByHtml5:function(a){return"color"===a.attr("type")},SUPPORTED_TYPES:["hex","rgb","rgba","hsl","hsla","keyword"],KEYWORD_COLORS:["aliceblue","antiquewhite","aqua","aquamarine","azure","beige","bisque","black","blanchedalmond","blue","blueviolet","brown","burlywood","cadetblue","chartreuse","chocolate","coral","cornflowerblue","cornsilk","crimson","cyan","darkblue","darkcyan","darkgoldenrod","darkgray","darkgreen","darkgrey","darkkhaki","darkmagenta","darkolivegreen","darkorange","darkorchid","darkred","darksalmon","darkseagreen","darkslateblue","darkslategray","darkslategrey","darkturquoise","darkviolet","deeppink","deepskyblue","dimgray","dimgrey","dodgerblue","firebrick","floralwhite","forestgreen","fuchsia","gainsboro","ghostwhite","gold","goldenrod","gray","green","greenyellow","grey","honeydew","hotpink","indianred","indigo","ivory","khaki","lavender","lavenderblush","lawngreen","lemonchiffon","lightblue","lightcoral","lightcyan","lightgoldenrodyellow","lightgray","lightgreen","lightgrey","lightpink","lightsalmon","lightseagreen","lightskyblue","lightslategray","lightslategrey","lightsteelblue","lightyellow","lime","limegreen","linen","magenta","maroon","mediumaquamarine","mediumblue","mediumorchid","mediumpurple","mediumseagreen","mediumslateblue","mediumspringgreen","mediumturquoise","mediumvioletred","midnightblue","mintcream","mistyrose","moccasin","navajowhite","navy","oldlace","olive","olivedrab","orange","orangered","orchid","palegoldenrod","palegreen","paleturquoise","palevioletred","papayawhip","peachpuff","peru","pink","plum","powderblue","purple","red","rosybrown","royalblue","saddlebrown","salmon","sandybrown","seagreen","seashell","sienna","silver","skyblue","slateblue","slategray","slategrey","snow","springgreen","steelblue","tan","teal","thistle","tomato","transparent","turquoise","violet","wheat","white","whitesmoke","yellow","yellowgreen"],validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;if(this.enableByHtml5(c))return/^#[0-9A-F]{6}$/i.test(f);var g=d.type||this.SUPPORTED_TYPES;a.isArray(g)||(g=g.replace(/s/g,"").split(","));for(var h,i,j=!1,k=0;k<g.length;k++)if(i=g[k],h="_"+i.toLowerCase(),j=j||this[h](f))return!0;return!1},_hex:function(a){return/(^#[0-9A-F]{6}$)|(^#[0-9A-F]{3}$)/i.test(a)},_hsl:function(a){return/^hsl\((\s*(-?\d+)\s*,)(\s*(\b(0?\d{1,2}|100)\b%)\s*,)(\s*(\b(0?\d{1,2}|100)\b%)\s*)\)$/.test(a)},_hsla:function(a){return/^hsla\((\s*(-?\d+)\s*,)(\s*(\b(0?\d{1,2}|100)\b%)\s*,){2}(\s*(0?(\.\d+)?|1(\.0+)?)\s*)\)$/.test(a)},_keyword:function(b){return a.inArray(b,this.KEYWORD_COLORS)>=0},_rgb:function(a){var b=/^rgb\((\s*(\b([01]?\d{1,2}|2[0-4]\d|25[0-5])\b)\s*,){2}(\s*(\b([01]?\d{1,2}|2[0-4]\d|25[0-5])\b)\s*)\)$/,c=/^rgb\((\s*(\b(0?\d{1,2}|100)\b%)\s*,){2}(\s*(\b(0?\d{1,2}|100)\b%)\s*)\)$/;return b.test(a)||c.test(a)},_rgba:function(a){var b=/^rgba\((\s*(\b([01]?\d{1,2}|2[0-4]\d|25[0-5])\b)\s*,){3}(\s*(0?(\.\d+)?|1(\.0+)?)\s*)\)$/,c=/^rgba\((\s*(\b(0?\d{1,2}|100)\b%)\s*,){3}(\s*(0?(\.\d+)?|1(\.0+)?)\s*)\)$/;return b.test(a)||c.test(a)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{creditCard:{"default":"Please enter a valid credit card number"}}}),FormValidation.Validator.creditCard={validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;if(/[^0-9-\s]+/.test(f))return!1;if(f=f.replace(/\D/g,""),!FormValidation.Helper.luhn(f))return!1;var g,h,i={AMERICAN_EXPRESS:{length:[15],prefix:["34","37"]},DANKORT:{length:[16],prefix:["5019"]},DINERS_CLUB:{length:[14],prefix:["300","301","302","303","304","305","36"]},DINERS_CLUB_US:{length:[16],prefix:["54","55"]},DISCOVER:{length:[16],prefix:["6011","622126","622127","622128","622129","62213","62214","62215","62216","62217","62218","62219","6222","6223","6224","6225","6226","6227","6228","62290","62291","622920","622921","622922","622923","622924","622925","644","645","646","647","648","649","65"]},ELO:{length:[16],prefix:["4011","4312","4389","4514","4573","4576","5041","5066","5067","509","6277","6362","6363","650","6516","6550"]},FORBRUGSFORENINGEN:{length:[16],prefix:["600722"]},JCB:{length:[16],prefix:["3528","3529","353","354","355","356","357","358"]},LASER:{length:[16,17,18,19],prefix:["6304","6706","6771","6709"]},MAESTRO:{length:[12,13,14,15,16,17,18,19],prefix:["5018","5020","5038","5868","6304","6759","6761","6762","6763","6764","6765","6766"]},MASTERCARD:{length:[16],prefix:["51","52","53","54","55"]},SOLO:{length:[16,18,19],prefix:["6334","6767"]},UNIONPAY:{length:[16,17,18,19],prefix:["622126","622127","622128","622129","62213","62214","62215","62216","62217","62218","62219","6222","6223","6224","6225","6226","6227","6228","62290","62291","622920","622921","622922","622923","622924","622925"]},VISA_ELECTRON:{length:[16],prefix:["4026","417500","4405","4508","4844","4913","4917"]},VISA:{length:[16],prefix:["4"]}};for(g in i)for(h in i[g].prefix)if(f.substr(0,i[g].prefix[h].length)===i[g].prefix[h]&&-1!==a.inArray(f.length,i[g].length))return{valid:!0,type:g};return!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{cusip:{"default":"Please enter a valid CUSIP number"}}}),FormValidation.Validator.cusip={validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;if(f=f.toUpperCase(),!/^[0-9A-Z]{9}$/.test(f))return!1;for(var g=a.map(f.split(""),function(a){var b=a.charCodeAt(0);return b>="A".charCodeAt(0)&&b<="Z".charCodeAt(0)?b-"A".charCodeAt(0)+10:a}),h=g.length,i=0,j=0;h-1>j;j++){var k=parseInt(g[j],10);j%2!==0&&(k*=2),k>9&&(k-=9),i+=k}return i=(10-i%10)%10,i===parseInt(g[h-1],10)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{cvv:{"default":"Please enter a valid CVV number"}}}),FormValidation.Validator.cvv={html5Attributes:{message:"message",ccfield:"creditCardField"},init:function(a,b,c,d){if(c.creditCardField){var e=a.getFieldElements(c.creditCardField);a.onLiveChange(e,"live_"+d,function(){var c=a.getStatus(b,d);c!==a.STATUS_NOT_VALIDATED&&a.revalidateField(b)})}},destroy:function(a,b,c,d){if(c.creditCardField){var e=a.getFieldElements(c.creditCardField);a.offLiveChange(e,"live_"+d)}},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;if(!/^[0-9]{3,4}$/.test(f))return!1;if(!d.creditCardField)return!0;var g=b.getFieldValue(d.creditCardField,"creditCard");if(null===g||""===g)return!0;g=g.replace(/\D/g,"");var h,i,j={AMERICAN_EXPRESS:{length:[15],prefix:["34","37"]},DANKORT:{length:[16],prefix:["5019"]},DINERS_CLUB:{length:[14],prefix:["300","301","302","303","304","305","36"]},DINERS_CLUB_US:{length:[16],prefix:["54","55"]},DISCOVER:{length:[16],prefix:["6011","622126","622127","622128","622129","62213","62214","62215","62216","62217","62218","62219","6222","6223","6224","6225","6226","6227","6228","62290","62291","622920","622921","622922","622923","622924","622925","644","645","646","647","648","649","65"]},ELO:{length:[16],prefix:["4011","4312","4389","4514","4573","4576","5041","5066","5067","509","6277","6362","6363","650","6516","6550"]},FORBRUGSFORENINGEN:{length:[16],prefix:["600722"]},JCB:{length:[16],prefix:["3528","3529","353","354","355","356","357","358"]},LASER:{length:[16,17,18,19],prefix:["6304","6706","6771","6709"]},MAESTRO:{length:[12,13,14,15,16,17,18,19],prefix:["5018","5020","5038","5868","6304","6759","6761","6762","6763","6764","6765","6766"]},MASTERCARD:{length:[16],prefix:["51","52","53","54","55"]},SOLO:{length:[16,18,19],prefix:["6334","6767"]},UNIONPAY:{length:[16,17,18,19],prefix:["622126","622127","622128","622129","62213","62214","62215","62216","62217","62218","62219","6222","6223","6224","6225","6226","6227","6228","62290","62291","622920","622921","622922","622923","622924","622925"]},VISA_ELECTRON:{length:[16],prefix:["4026","417500","4405","4508","4844","4913","4917"]},VISA:{length:[16],prefix:["4"]}},k=null;for(h in j)for(i in j[h].prefix)if(g.substr(0,j[h].prefix[i].length)===j[h].prefix[i]&&-1!==a.inArray(g.length,j[h].length)){k=h;break}return null===k?!1:"AMERICAN_EXPRESS"===k?4===f.length:3===f.length}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{date:{"default":"Please enter a valid date",min:"Please enter a date after %s",max:"Please enter a date before %s",range:"Please enter a date in the range %s - %s"}}}),FormValidation.Validator.date={html5Attributes:{message:"message",format:"format",min:"min",max:"max",separator:"separator"},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;d.format=d.format||"MM/DD/YYYY","date"===c.attr("type")&&(d.format="YYYY-MM-DD");var g=b.getLocale(),h=d.message||FormValidation.I18n[g].date["default"],i=d.format.split(" "),j=i[0],k=i.length>1?i[1]:null,l=i.length>2?i[2]:null,m=f.split(" "),n=m[0],o=m.length>1?m[1]:null;if(i.length!==m.length)return{valid:!1,message:h};var p=d.separator;if(p||(p=-1!==n.indexOf("/")?"/":-1!==n.indexOf("-")?"-":-1!==n.indexOf(".")?".":null),null===p||-1===n.indexOf(p))return{valid:!1,message:h};if(n=n.split(p),j=j.split(p),n.length!==j.length)return{valid:!1,message:h};var q=n[a.inArray("YYYY",j)],r=n[a.inArray("MM",j)],s=n[a.inArray("DD",j)];if(!q||!r||!s||4!==q.length)return{valid:!1,message:h};var t=null,u=null,v=null;if(k){if(k=k.split(":"),o=o.split(":"),k.length!==o.length)return{valid:!1,message:h};if(u=o.length>0?o[0]:null,t=o.length>1?o[1]:null,v=o.length>2?o[2]:null,""===u||""===t||""===v)return{valid:!1,message:h};if(v){if(isNaN(v)||v.length>2)return{valid:!1,message:h};if(v=parseInt(v,10),0>v||v>60)return{valid:!1,message:h}}if(u){if(isNaN(u)||u.length>2)return{valid:!1,message:h};if(u=parseInt(u,10),0>u||u>=24||l&&u>12)return{valid:!1,message:h}}if(t){if(isNaN(t)||t.length>2)return{valid:!1,message:h};if(t=parseInt(t,10),0>t||t>59)return{valid:!1,message:h}}}var w=FormValidation.Helper.date(q,r,s),x=null,y=null,z=d.min,A=d.max;switch(z&&(x=z instanceof Date?z:this._parseDate(z,j,p)||this._parseDate(b.getDynamicOption(c,z),j,p),z=this._formatDate(x,d.format)),A&&(y=A instanceof Date?A:this._parseDate(A,j,p)||this._parseDate(b.getDynamicOption(c,A),j,p),A=this._formatDate(y,d.format)),n=new Date(q,r-1,s,u,t,v),!0){case z&&!A&&w:w=n.getTime()>=x.getTime(),h=d.message||FormValidation.Helper.format(FormValidation.I18n[g].date.min,z);break;case A&&!z&&w:w=n.getTime()<=y.getTime(),h=d.message||FormValidation.Helper.format(FormValidation.I18n[g].date.max,A);break;case A&&z&&w:w=n.getTime()<=y.getTime()&&n.getTime()>=x.getTime(),h=d.message||FormValidation.Helper.format(FormValidation.I18n[g].date.range,[z,A])}return{valid:w,date:n,message:h}},_parseDate:function(b,c,d){if(b instanceof Date)return b;if("string"!=typeof b)return null;var e=a.inArray("YYYY",c),f=a.inArray("MM",c),g=a.inArray("DD",c);if(-1===e||-1===f||-1===g)return null;var h=0,i=0,j=0,k=b.split(" "),l=k[0].split(d);if(l.length<3)return null;if(k.length>1){var m=k[1].split(":");i=m.length>0?m[0]:null,h=m.length>1?m[1]:null,j=m.length>2?m[2]:null}return new Date(l[e],l[f]-1,l[g],i,h,j)},_formatDate:function(a,b){b=b.replace(/Y/g,"y").replace(/M/g,"m").replace(/D/g,"d").replace(/:m/g,":M").replace(/:mm/g,":MM").replace(/:S/,":s").replace(/:SS/,":ss");var c={d:function(a){return a.getDate()},dd:function(a){var b=a.getDate();return 10>b?"0"+b:b},m:function(a){return a.getMonth()+1},mm:function(a){var b=a.getMonth()+1;return 10>b?"0"+b:b},yy:function(a){return(""+a.getFullYear()).substr(2)},yyyy:function(a){return a.getFullYear()},h:function(a){return a.getHours()%12||12},hh:function(a){var b=a.getHours()%12||12;return 10>b?"0"+b:b},H:function(a){return a.getHours()},HH:function(a){var b=a.getHours();return 10>b?"0"+b:b},M:function(a){return a.getMinutes()},MM:function(a){var b=a.getMinutes();return 10>b?"0"+b:b},s:function(a){return a.getSeconds()},ss:function(a){var b=a.getSeconds();return 10>b?"0"+b:b}};return b.replace(/d{1,4}|m{1,4}|yy(?:yy)?|([HhMs])\1?|"[^"]*"|'[^']*'/g,function(b){return c[b]?c[b](a):b.slice(1,b.length-1)})}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{different:{"default":"Please enter a different value"}}}),FormValidation.Validator.different={html5Attributes:{message:"message",field:"field"},init:function(b,c,d,e){for(var f=d.field.split(","),g=0;g<f.length;g++){var h=b.getFieldElements(a.trim(f[g]));b.onLiveChange(h,"live_"+e,function(){var a=b.getStatus(c,e);a!==b.STATUS_NOT_VALIDATED&&b.revalidateField(c)})}},destroy:function(b,c,d,e){for(var f=d.field.split(","),g=0;g<f.length;g++){var h=b.getFieldElements(a.trim(f[g]));b.offLiveChange(h,"live_"+e)}},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;for(var g=d.field.split(","),h=!0,i=0;i<g.length;i++){var j=b.getFieldElements(a.trim(g[i]));if(null!=j&&0!==j.length){var k=b.getFieldValue(j,e);f===k?h=!1:""!==k&&b.updateStatus(j,b.STATUS_VALID,e)}}return h}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{digits:{"default":"Please enter only digits"}}}),FormValidation.Validator.digits={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:/^\d+$/.test(e)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{ean:{"default":"Please enter a valid EAN number"}}}),FormValidation.Validator.ean={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;if(!/^(\d{8}|\d{12}|\d{13})$/.test(e))return!1;for(var f=e.length,g=0,h=8===f?[3,1]:[1,3],i=0;f-1>i;i++)g+=parseInt(e.charAt(i),10)*h[i%2];return g=(10-g%10)%10,g+""===e.charAt(f-1)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{ein:{"default":"Please enter a valid EIN number"}}}),FormValidation.Validator.ein={CAMPUS:{ANDOVER:["10","12"],ATLANTA:["60","67"],AUSTIN:["50","53"],BROOKHAVEN:["01","02","03","04","05","06","11","13","14","16","21","22","23","25","34","51","52","54","55","56","57","58","59","65"],CINCINNATI:["30","32","35","36","37","38","61"],FRESNO:["15","24"],KANSAS_CITY:["40","44"],MEMPHIS:["94","95"],OGDEN:["80","90"],PHILADELPHIA:["33","39","41","42","43","48","62","63","64","66","68","71","72","73","74","75","76","77","81","82","83","84","85","86","87","88","91","92","93","98","99"],INTERNET:["20","26","27","45","46","47"],SMALL_BUSINESS_ADMINISTRATION:["31"]},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;if(!/^[0-9]{2}-?[0-9]{7}$/.test(f))return!1;var g=f.substr(0,2)+"";for(var h in this.CAMPUS)if(-1!==a.inArray(g,this.CAMPUS[h]))return{valid:!0,campus:h};return!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{emailAddress:{"default":"Please enter a valid email address"}}}),FormValidation.Validator.emailAddress={html5Attributes:{message:"message",multiple:"multiple",separator:"separator"},enableByHtml5:function(a){return"email"===a.attr("type")},validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f=/^(([^<>()[\]\\.,;:\s@\"]+(\.[^<>()[\]\\.,;:\s@\"]+)*)|(\".+\"))@[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?(?:\.[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?)*$/,g=c.multiple===!0||"true"===c.multiple;if(g){for(var h=c.separator||/[,;]/,i=this._splitEmailAddresses(e,h),j=0;j<i.length;j++)if(!f.test(i[j]))return!1;return!0}return f.test(e)},_splitEmailAddresses:function(a,b){for(var c=a.split(/"/),d=c.length,e=[],f="",g=0;d>g;g++)if(g%2===0){var h=c[g].split(b),i=h.length;if(1===i)f+=h[0];else{e.push(f+h[0]);for(var j=1;i-1>j;j++)e.push(h[j]);f=h[i-1]}}else f+='"'+c[g],d-1>g&&(f+='"');return e.push(f),e}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{file:{"default":"Please choose a valid file"}}}),FormValidation.Validator.file={Error:{EXTENSION:"EXTENSION",MAX_FILES:"MAX_FILES",MAX_SIZE:"MAX_SIZE",MAX_TOTAL_SIZE:"MAX_TOTAL_SIZE",MIN_FILES:"MIN_FILES",MIN_SIZE:"MIN_SIZE",MIN_TOTAL_SIZE:"MIN_TOTAL_SIZE",TYPE:"TYPE"},html5Attributes:{extension:"extension",maxfiles:"maxFiles",minfiles:"minFiles",maxsize:"maxSize",minsize:"minSize",maxtotalsize:"maxTotalSize",mintotalsize:"minTotalSize",message:"message",type:"type"},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;var g,h=d.extension?d.extension.toLowerCase().split(","):null,i=d.type?d.type.toLowerCase().split(","):null,j=window.File&&window.FileList&&window.FileReader;if(j){var k=c.get(0).files,l=k.length,m=0;if(d.maxFiles&&l>parseInt(d.maxFiles,10))return{valid:!1,error:this.Error.MAX_FILES};if(d.minFiles&&l<parseInt(d.minFiles,10))return{valid:!1,error:this.Error.MIN_FILES};for(var n={},o=0;l>o;o++){if(m+=k[o].size,g=k[o].name.substr(k[o].name.lastIndexOf(".")+1),n={file:k[o],size:k[o].size,ext:g,type:k[o].type},d.minSize&&k[o].size<parseInt(d.minSize,10))return{valid:!1,error:this.Error.MIN_SIZE,metaData:n};if(d.maxSize&&k[o].size>parseInt(d.maxSize,10))return{valid:!1,error:this.Error.MAX_SIZE,metaData:n};if(h&&-1===a.inArray(g.toLowerCase(),h))return{valid:!1,error:this.Error.EXTENSION,metaData:n};if(k[o].type&&i&&-1===a.inArray(k[o].type.toLowerCase(),i))return{valid:!1,error:this.Error.TYPE,metaData:n}}if(d.maxTotalSize&&m>parseInt(d.maxTotalSize,10))return{valid:!1,error:this.Error.MAX_TOTAL_SIZE,metaData:{totalSize:m}};if(d.minTotalSize&&m<parseInt(d.minTotalSize,10))return{valid:!1,error:this.Error.MIN_TOTAL_SIZE,metaData:{totalSize:m}}}else if(g=f.substr(f.lastIndexOf(".")+1),h&&-1===a.inArray(g.toLowerCase(),h))return{valid:!1,error:this.Error.EXTENSION,metaData:{ext:g}};return!0}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{greaterThan:{"default":"Please enter a value greater than or equal to %s",notInclusive:"Please enter a value greater than %s"}}}),FormValidation.Validator.greaterThan={html5Attributes:{message:"message",value:"value",inclusive:"inclusive"},enableByHtml5:function(a){var b=a.attr("type"),c=a.attr("min");return c&&"date"!==b?{value:c}:!1},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;f=this._format(f);var g=b.getLocale(),h=a.isNumeric(d.value)?d.value:b.getDynamicOption(c,d.value),i=this._format(h);return d.inclusive===!0||void 0===d.inclusive?{valid:a.isNumeric(f)&&parseFloat(f)>=i,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].greaterThan["default"],h)}:{valid:a.isNumeric(f)&&parseFloat(f)>i,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].greaterThan.notInclusive,h)}},_format:function(a){return(a+"").replace(",",".")}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{grid:{"default":"Please enter a valid GRId number"}}}),FormValidation.Validator.grid={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:(e=e.toUpperCase(),/^[GRID:]*([0-9A-Z]{2})[-\s]*([0-9A-Z]{5})[-\s]*([0-9A-Z]{10})[-\s]*([0-9A-Z]{1})$/g.test(e)?(e=e.replace(/\s/g,"").replace(/-/g,""),"GRID:"===e.substr(0,5)&&(e=e.substr(5)),FormValidation.Helper.mod37And36(e)):!1)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{hex:{"default":"Please enter a valid hexadecimal number"}}}),FormValidation.Validator.hex={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:/^[0-9a-fA-F]+$/.test(e)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{iban:{"default":"Please enter a valid IBAN number",country:"Please enter a valid IBAN number in %s",countries:{AD:"Andorra",AE:"United Arab Emirates",AL:"Albania",AO:"Angola",AT:"Austria",AZ:"Azerbaijan",BA:"Bosnia and Herzegovina",BE:"Belgium",BF:"Burkina Faso",BG:"Bulgaria",BH:"Bahrain",BI:"Burundi",BJ:"Benin",BR:"Brazil",CH:"Switzerland",CI:"Ivory Coast",CM:"Cameroon",CR:"Costa Rica",CV:"Cape Verde",CY:"Cyprus",CZ:"Czech Republic",DE:"Germany",DK:"Denmark",DO:"Dominican Republic",DZ:"Algeria",EE:"Estonia",ES:"Spain",FI:"Finland",FO:"Faroe Islands",FR:"France",GB:"United Kingdom",GE:"Georgia",GI:"Gibraltar",GL:"Greenland",GR:"Greece",GT:"Guatemala",HR:"Croatia",HU:"Hungary",IE:"Ireland",IL:"Israel",IR:"Iran",IS:"Iceland",IT:"Italy",JO:"Jordan",KW:"Kuwait",KZ:"Kazakhstan",LB:"Lebanon",LI:"Liechtenstein",LT:"Lithuania",LU:"Luxembourg",LV:"Latvia",MC:"Monaco",MD:"Moldova",ME:"Montenegro",MG:"Madagascar",MK:"Macedonia",ML:"Mali",MR:"Mauritania",MT:"Malta",MU:"Mauritius",MZ:"Mozambique",NL:"Netherlands",NO:"Norway",PK:"Pakistan",PL:"Poland",PS:"Palestine",PT:"Portugal",QA:"Qatar",RO:"Romania",RS:"Serbia",SA:"Saudi Arabia",SE:"Sweden",SI:"Slovenia",SK:"Slovakia",SM:"San Marino",SN:"Senegal",TL:"East Timor",TN:"Tunisia",TR:"Turkey",VG:"Virgin Islands, British",XK:"Republic of Kosovo"}}}}),FormValidation.Validator.iban={html5Attributes:{message:"message",country:"country",sepa:"sepa"},REGEX:{AD:"AD[0-9]{2}[0-9]{4}[0-9]{4}[A-Z0-9]{12}",AE:"AE[0-9]{2}[0-9]{3}[0-9]{16}",AL:"AL[0-9]{2}[0-9]{8}[A-Z0-9]{16}",AO:"AO[0-9]{2}[0-9]{21}",AT:"AT[0-9]{2}[0-9]{5}[0-9]{11}",AZ:"AZ[0-9]{2}[A-Z]{4}[A-Z0-9]{20}",BA:"BA[0-9]{2}[0-9]{3}[0-9]{3}[0-9]{8}[0-9]{2}",BE:"BE[0-9]{2}[0-9]{3}[0-9]{7}[0-9]{2}",BF:"BF[0-9]{2}[0-9]{23}",BG:"BG[0-9]{2}[A-Z]{4}[0-9]{4}[0-9]{2}[A-Z0-9]{8}",BH:"BH[0-9]{2}[A-Z]{4}[A-Z0-9]{14}",BI:"BI[0-9]{2}[0-9]{12}",BJ:"BJ[0-9]{2}[A-Z]{1}[0-9]{23}",BR:"BR[0-9]{2}[0-9]{8}[0-9]{5}[0-9]{10}[A-Z][A-Z0-9]",CH:"CH[0-9]{2}[0-9]{5}[A-Z0-9]{12}",CI:"CI[0-9]{2}[A-Z]{1}[0-9]{23}",CM:"CM[0-9]{2}[0-9]{23}",CR:"CR[0-9]{2}[0-9]{3}[0-9]{14}",CV:"CV[0-9]{2}[0-9]{21}",CY:"CY[0-9]{2}[0-9]{3}[0-9]{5}[A-Z0-9]{16}",CZ:"CZ[0-9]{2}[0-9]{20}",DE:"DE[0-9]{2}[0-9]{8}[0-9]{10}",DK:"DK[0-9]{2}[0-9]{14}",DO:"DO[0-9]{2}[A-Z0-9]{4}[0-9]{20}",DZ:"DZ[0-9]{2}[0-9]{20}",EE:"EE[0-9]{2}[0-9]{2}[0-9]{2}[0-9]{11}[0-9]{1}",ES:"ES[0-9]{2}[0-9]{4}[0-9]{4}[0-9]{1}[0-9]{1}[0-9]{10}",FI:"FI[0-9]{2}[0-9]{6}[0-9]{7}[0-9]{1}",FO:"FO[0-9]{2}[0-9]{4}[0-9]{9}[0-9]{1}",FR:"FR[0-9]{2}[0-9]{5}[0-9]{5}[A-Z0-9]{11}[0-9]{2}",GB:"GB[0-9]{2}[A-Z]{4}[0-9]{6}[0-9]{8}",GE:"GE[0-9]{2}[A-Z]{2}[0-9]{16}",GI:"GI[0-9]{2}[A-Z]{4}[A-Z0-9]{15}",GL:"GL[0-9]{2}[0-9]{4}[0-9]{9}[0-9]{1}",GR:"GR[0-9]{2}[0-9]{3}[0-9]{4}[A-Z0-9]{16}",GT:"GT[0-9]{2}[A-Z0-9]{4}[A-Z0-9]{20}",HR:"HR[0-9]{2}[0-9]{7}[0-9]{10}",HU:"HU[0-9]{2}[0-9]{3}[0-9]{4}[0-9]{1}[0-9]{15}[0-9]{1}",
IE:"IE[0-9]{2}[A-Z]{4}[0-9]{6}[0-9]{8}",IL:"IL[0-9]{2}[0-9]{3}[0-9]{3}[0-9]{13}",IR:"IR[0-9]{2}[0-9]{22}",IS:"IS[0-9]{2}[0-9]{4}[0-9]{2}[0-9]{6}[0-9]{10}",IT:"IT[0-9]{2}[A-Z]{1}[0-9]{5}[0-9]{5}[A-Z0-9]{12}",JO:"JO[0-9]{2}[A-Z]{4}[0-9]{4}[0]{8}[A-Z0-9]{10}",KW:"KW[0-9]{2}[A-Z]{4}[0-9]{22}",KZ:"KZ[0-9]{2}[0-9]{3}[A-Z0-9]{13}",LB:"LB[0-9]{2}[0-9]{4}[A-Z0-9]{20}",LI:"LI[0-9]{2}[0-9]{5}[A-Z0-9]{12}",LT:"LT[0-9]{2}[0-9]{5}[0-9]{11}",LU:"LU[0-9]{2}[0-9]{3}[A-Z0-9]{13}",LV:"LV[0-9]{2}[A-Z]{4}[A-Z0-9]{13}",MC:"MC[0-9]{2}[0-9]{5}[0-9]{5}[A-Z0-9]{11}[0-9]{2}",MD:"MD[0-9]{2}[A-Z0-9]{20}",ME:"ME[0-9]{2}[0-9]{3}[0-9]{13}[0-9]{2}",MG:"MG[0-9]{2}[0-9]{23}",MK:"MK[0-9]{2}[0-9]{3}[A-Z0-9]{10}[0-9]{2}",ML:"ML[0-9]{2}[A-Z]{1}[0-9]{23}",MR:"MR13[0-9]{5}[0-9]{5}[0-9]{11}[0-9]{2}",MT:"MT[0-9]{2}[A-Z]{4}[0-9]{5}[A-Z0-9]{18}",MU:"MU[0-9]{2}[A-Z]{4}[0-9]{2}[0-9]{2}[0-9]{12}[0-9]{3}[A-Z]{3}",MZ:"MZ[0-9]{2}[0-9]{21}",NL:"NL[0-9]{2}[A-Z]{4}[0-9]{10}",NO:"NO[0-9]{2}[0-9]{4}[0-9]{6}[0-9]{1}",PK:"PK[0-9]{2}[A-Z]{4}[A-Z0-9]{16}",PL:"PL[0-9]{2}[0-9]{8}[0-9]{16}",PS:"PS[0-9]{2}[A-Z]{4}[A-Z0-9]{21}",PT:"PT[0-9]{2}[0-9]{4}[0-9]{4}[0-9]{11}[0-9]{2}",QA:"QA[0-9]{2}[A-Z]{4}[A-Z0-9]{21}",RO:"RO[0-9]{2}[A-Z]{4}[A-Z0-9]{16}",RS:"RS[0-9]{2}[0-9]{3}[0-9]{13}[0-9]{2}",SA:"SA[0-9]{2}[0-9]{2}[A-Z0-9]{18}",SE:"SE[0-9]{2}[0-9]{3}[0-9]{16}[0-9]{1}",SI:"SI[0-9]{2}[0-9]{5}[0-9]{8}[0-9]{2}",SK:"SK[0-9]{2}[0-9]{4}[0-9]{6}[0-9]{10}",SM:"SM[0-9]{2}[A-Z]{1}[0-9]{5}[0-9]{5}[A-Z0-9]{12}",SN:"SN[0-9]{2}[A-Z]{1}[0-9]{23}",TL:"TL38[0-9]{3}[0-9]{14}[0-9]{2}",TN:"TN59[0-9]{2}[0-9]{3}[0-9]{13}[0-9]{2}",TR:"TR[0-9]{2}[0-9]{5}[A-Z0-9]{1}[A-Z0-9]{16}",VG:"VG[0-9]{2}[A-Z]{4}[0-9]{16}",XK:"XK[0-9]{2}[0-9]{4}[0-9]{10}[0-9]{2}"},SEPA_COUNTRIES:["AT","BE","BG","CH","CY","CZ","DE","DK","EE","ES","FI","FR","GB","GI","GR","HR","HU","IE","IS","IT","LI","LT","LU","LV","MC","MT","NL","NO","PL","PT","RO","SE","SI","SK","SM"],validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;f=f.replace(/[^a-zA-Z0-9]/g,"").toUpperCase();var g=d.country;g?"string"==typeof g&&this.REGEX[g]||(g=b.getDynamicOption(c,g)):g=f.substr(0,2);var h=b.getLocale();if(!this.REGEX[g])return!1;if(void 0!==typeof d.sepa){var i=-1!==a.inArray(g,this.SEPA_COUNTRIES);if(("true"===d.sepa||d.sepa===!0)&&!i||("false"===d.sepa||d.sepa===!1)&&i)return!1}if(!new RegExp("^"+this.REGEX[g]+"$").test(f))return{valid:!1,message:FormValidation.Helper.format(d.message||FormValidation.I18n[h].iban.country,FormValidation.I18n[h].iban.countries[g])};f=f.substr(4)+f.substr(0,4),f=a.map(f.split(""),function(a){var b=a.charCodeAt(0);return b>="A".charCodeAt(0)&&b<="Z".charCodeAt(0)?b-"A".charCodeAt(0)+10:a}),f=f.join("");for(var j=parseInt(f.substr(0,1),10),k=f.length,l=1;k>l;++l)j=(10*j+parseInt(f.substr(l,1),10))%97;return{valid:1===j,message:FormValidation.Helper.format(d.message||FormValidation.I18n[h].iban.country,FormValidation.I18n[h].iban.countries[g])}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{id:{"default":"Please enter a valid identification number",country:"Please enter a valid identification number in %s",countries:{BA:"Bosnia and Herzegovina",BG:"Bulgaria",BR:"Brazil",CH:"Switzerland",CL:"Chile",CN:"China",CZ:"Czech Republic",DK:"Denmark",EE:"Estonia",ES:"Spain",FI:"Finland",HR:"Croatia",IE:"Ireland",IS:"Iceland",LT:"Lithuania",LV:"Latvia",ME:"Montenegro",MK:"Macedonia",NL:"Netherlands",PL:"Poland",RO:"Romania",RS:"Serbia",SE:"Sweden",SI:"Slovenia",SK:"Slovakia",SM:"San Marino",TH:"Thailand",TR:"Turkey",ZA:"South Africa"}}}}),FormValidation.Validator.id={html5Attributes:{message:"message",country:"country"},COUNTRY_CODES:["BA","BG","BR","CH","CL","CN","CZ","DK","EE","ES","FI","HR","IE","IS","LT","LV","ME","MK","NL","PL","RO","RS","SE","SI","SK","SM","TH","TR","ZA"],validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;var g=b.getLocale(),h=d.country;if(h?("string"!=typeof h||-1===a.inArray(h.toUpperCase(),this.COUNTRY_CODES))&&(h=b.getDynamicOption(c,h)):h=f.substr(0,2),-1===a.inArray(h,this.COUNTRY_CODES))return!0;var i=["_",h.toLowerCase()].join(""),j=this[i](f);return j=j===!0||j===!1?{valid:j}:j,j.message=FormValidation.Helper.format(d.message||FormValidation.I18n[g].id.country,FormValidation.I18n[g].id.countries[h.toUpperCase()]),j},_validateJMBG:function(a,b){if(!/^\d{13}$/.test(a))return!1;var c=parseInt(a.substr(0,2),10),d=parseInt(a.substr(2,2),10),e=(parseInt(a.substr(4,3),10),parseInt(a.substr(7,2),10)),f=parseInt(a.substr(12,1),10);if(c>31||d>12)return!1;for(var g=0,h=0;6>h;h++)g+=(7-h)*(parseInt(a.charAt(h),10)+parseInt(a.charAt(h+6),10));if(g=11-g%11,(10===g||11===g)&&(g=0),g!==f)return!1;switch(b.toUpperCase()){case"BA":return e>=10&&19>=e;case"MK":return e>=41&&49>=e;case"ME":return e>=20&&29>=e;case"RS":return e>=70&&99>=e;case"SI":return e>=50&&59>=e;default:return!0}},_ba:function(a){return this._validateJMBG(a,"BA")},_mk:function(a){return this._validateJMBG(a,"MK")},_me:function(a){return this._validateJMBG(a,"ME")},_rs:function(a){return this._validateJMBG(a,"RS")},_si:function(a){return this._validateJMBG(a,"SI")},_bg:function(a){if(!/^\d{10}$/.test(a)&&!/^\d{6}\s\d{3}\s\d{1}$/.test(a))return!1;a=a.replace(/\s/g,"");var b=parseInt(a.substr(0,2),10)+1900,c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10);if(c>40?(b+=100,c-=40):c>20&&(b-=100,c-=20),!FormValidation.Helper.date(b,c,d))return!1;for(var e=0,f=[2,4,8,5,10,9,7,3,6],g=0;9>g;g++)e+=parseInt(a.charAt(g),10)*f[g];return e=e%11%10,e+""===a.substr(9,1)},_br:function(a){if(a=a.replace(/\D/g,""),!/^\d{11}$/.test(a)||/^1{11}|2{11}|3{11}|4{11}|5{11}|6{11}|7{11}|8{11}|9{11}|0{11}$/.test(a))return!1;for(var b=0,c=0;9>c;c++)b+=(10-c)*parseInt(a.charAt(c),10);if(b=11-b%11,(10===b||11===b)&&(b=0),b+""!==a.charAt(9))return!1;var d=0;for(c=0;10>c;c++)d+=(11-c)*parseInt(a.charAt(c),10);return d=11-d%11,(10===d||11===d)&&(d=0),d+""===a.charAt(10)},_ch:function(a){if(!/^756[\.]{0,1}[0-9]{4}[\.]{0,1}[0-9]{4}[\.]{0,1}[0-9]{2}$/.test(a))return!1;a=a.replace(/\D/g,"").substr(3);for(var b=a.length,c=0,d=8===b?[3,1]:[1,3],e=0;b-1>e;e++)c+=parseInt(a.charAt(e),10)*d[e%2];return c=10-c%10,c+""===a.charAt(b-1)},_cl:function(a){if(!/^\d{7,8}[-]{0,1}[0-9K]$/i.test(a))return!1;for(a=a.replace(/\-/g,"");a.length<9;)a="0"+a;for(var b=0,c=[3,2,7,6,5,4,3,2],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b=11-b%11,11===b?b=0:10===b&&(b="K"),b+""===a.charAt(8).toUpperCase()},_cn:function(b){if(b=b.trim(),!/^\d{15}$/.test(b)&&!/^\d{17}[\dXx]{1}$/.test(b))return!1;var c={11:{0:[0],1:[[0,9],[11,17]],2:[0,28,29]},12:{0:[0],1:[[0,16]],2:[0,21,23,25]},13:{0:[0],1:[[0,5],7,8,21,[23,33],[81,85]],2:[[0,5],[7,9],[23,25],27,29,30,81,83],3:[[0,4],[21,24]],4:[[0,4],6,21,[23,35],81],5:[[0,3],[21,35],81,82],6:[[0,4],[21,38],[81,84]],7:[[0,3],5,6,[21,33]],8:[[0,4],[21,28]],9:[[0,3],[21,30],[81,84]],10:[[0,3],[22,26],28,81,82],11:[[0,2],[21,28],81,82]},14:{0:[0],1:[0,1,[5,10],[21,23],81],2:[[0,3],11,12,[21,27]],3:[[0,3],11,21,22],4:[[0,2],11,21,[23,31],81],5:[[0,2],21,22,24,25,81],6:[[0,3],[21,24]],7:[[0,2],[21,29],81],8:[[0,2],[21,30],81,82],9:[[0,2],[21,32],81],10:[[0,2],[21,34],81,82],11:[[0,2],[21,30],81,82],23:[[0,3],22,23,[25,30],32,33]},15:{0:[0],1:[[0,5],[21,25]],2:[[0,7],[21,23]],3:[[0,4]],4:[[0,4],[21,26],[28,30]],5:[[0,2],[21,26],81],6:[[0,2],[21,27]],7:[[0,3],[21,27],[81,85]],8:[[0,2],[21,26]],9:[[0,2],[21,29],81],22:[[0,2],[21,24]],25:[[0,2],[22,31]],26:[[0,2],[24,27],[29,32],34],28:[0,1,[22,27]],29:[0,[21,23]]},21:{0:[0],1:[[0,6],[11,14],[22,24],81],2:[[0,4],[11,13],24,[81,83]],3:[[0,4],11,21,23,81],4:[[0,4],11,[21,23]],5:[[0,5],21,22],6:[[0,4],24,81,82],7:[[0,3],11,26,27,81,82],8:[[0,4],11,81,82],9:[[0,5],11,21,22],10:[[0,5],11,21,81],11:[[0,3],21,22],12:[[0,2],4,21,23,24,81,82],13:[[0,3],21,22,24,81,82],14:[[0,4],21,22,81]},22:{0:[0],1:[[0,6],12,22,[81,83]],2:[[0,4],11,21,[81,84]],3:[[0,3],22,23,81,82],4:[[0,3],21,22],5:[[0,3],21,23,24,81,82],6:[[0,2],4,5,[21,23],25,81],7:[[0,2],[21,24],81],8:[[0,2],21,22,81,82],24:[[0,6],24,26]},23:{0:[0],1:[[0,12],21,[23,29],[81,84]],2:[[0,8],21,[23,25],27,[29,31],81],3:[[0,7],21,81,82],4:[[0,7],21,22],5:[[0,3],5,6,[21,24]],6:[[0,6],[21,24]],7:[[0,16],22,81],8:[[0,5],11,22,26,28,33,81,82],9:[[0,4],21],10:[[0,5],24,25,81,[83,85]],11:[[0,2],21,23,24,81,82],12:[[0,2],[21,26],[81,83]],27:[[0,4],[21,23]]},31:{0:[0],1:[0,1,[3,10],[12,20]],2:[0,30]},32:{0:[0],1:[[0,7],11,[13,18],24,25],2:[[0,6],11,81,82],3:[[0,5],11,12,[21,24],81,82],4:[[0,2],4,5,11,12,81,82],5:[[0,9],[81,85]],6:[[0,2],11,12,21,23,[81,84]],7:[0,1,3,5,6,[21,24]],8:[[0,4],11,26,[29,31]],9:[[0,3],[21,25],28,81,82],10:[[0,3],11,12,23,81,84,88],11:[[0,2],11,12,[81,83]],12:[[0,4],[81,84]],13:[[0,2],11,[21,24]]},33:{0:[0],1:[[0,6],[8,10],22,27,82,83,85],2:[0,1,[3,6],11,12,25,26,[81,83]],3:[[0,4],22,24,[26,29],81,82],4:[[0,2],11,21,24,[81,83]],5:[[0,3],[21,23]],6:[[0,2],21,24,[81,83]],7:[[0,3],23,26,27,[81,84]],8:[[0,3],22,24,25,81],9:[[0,3],21,22],10:[[0,4],[21,24],81,82],11:[[0,2],[21,27],81]},34:{0:[0],1:[[0,4],11,[21,24],81],2:[[0,4],7,8,[21,23],25],3:[[0,4],11,[21,23]],4:[[0,6],21],5:[[0,4],6,[21,23]],6:[[0,4],21],7:[[0,3],11,21],8:[[0,3],11,[22,28],81],10:[[0,4],[21,24]],11:[[0,3],22,[24,26],81,82],12:[[0,4],21,22,25,26,82],13:[[0,2],[21,24]],14:[[0,2],[21,24]],15:[[0,3],[21,25]],16:[[0,2],[21,23]],17:[[0,2],[21,23]],18:[[0,2],[21,25],81]},35:{0:[0],1:[[0,5],11,[21,25],28,81,82],2:[[0,6],[11,13]],3:[[0,5],22],4:[[0,3],21,[23,30],81],5:[[0,5],21,[24,27],[81,83]],6:[[0,3],[22,29],81],7:[[0,2],[21,25],[81,84]],8:[[0,2],[21,25],81],9:[[0,2],[21,26],81,82]},36:{0:[0],1:[[0,5],11,[21,24]],2:[[0,3],22,81],3:[[0,2],13,[21,23]],4:[[0,3],21,[23,30],81,82],5:[[0,2],21],6:[[0,2],22,81],7:[[0,2],[21,35],81,82],8:[[0,3],[21,30],81],9:[[0,2],[21,26],[81,83]],10:[[0,2],[21,30]],11:[[0,2],[21,30],81]},37:{0:[0],1:[[0,5],12,13,[24,26],81],2:[[0,3],5,[11,14],[81,85]],3:[[0,6],[21,23]],4:[[0,6],81],5:[[0,3],[21,23]],6:[[0,2],[11,13],34,[81,87]],7:[[0,5],24,25,[81,86]],8:[[0,2],11,[26,32],[81,83]],9:[[0,3],11,21,23,82,83],10:[[0,2],[81,83]],11:[[0,3],21,22],12:[[0,3]],13:[[0,2],11,12,[21,29]],14:[[0,2],[21,28],81,82],15:[[0,2],[21,26],81],16:[[0,2],[21,26]],17:[[0,2],[21,28]]},41:{0:[0],1:[[0,6],8,22,[81,85]],2:[[0,5],11,[21,25]],3:[[0,7],11,[22,29],81],4:[[0,4],11,[21,23],25,81,82],5:[[0,3],5,6,22,23,26,27,81],6:[[0,3],11,21,22],7:[[0,4],11,21,[24,28],81,82],8:[[0,4],11,[21,23],25,[81,83]],9:[[0,2],22,23,[26,28]],10:[[0,2],[23,25],81,82],11:[[0,4],[21,23]],12:[[0,2],21,22,24,81,82],13:[[0,3],[21,30],81],14:[[0,3],[21,26],81],15:[[0,3],[21,28]],16:[[0,2],[21,28],81],17:[[0,2],[21,29]],90:[0,1]},42:{0:[0],1:[[0,7],[11,17]],2:[[0,5],22,81],3:[[0,3],[21,25],81],5:[[0,6],[25,29],[81,83]],6:[[0,2],6,7,[24,26],[82,84]],7:[[0,4]],8:[[0,2],4,21,22,81],9:[[0,2],[21,23],81,82,84],10:[[0,3],[22,24],81,83,87],11:[[0,2],[21,27],81,82],12:[[0,2],[21,24],81],13:[[0,3],21,81],28:[[0,2],22,23,[25,28]],90:[0,[4,6],21]},43:{0:[0],1:[[0,5],11,12,21,22,24,81],2:[[0,4],11,21,[23,25],81],3:[[0,2],4,21,81,82],4:[0,1,[5,8],12,[21,24],26,81,82],5:[[0,3],11,[21,25],[27,29],81],6:[[0,3],11,21,23,24,26,81,82],7:[[0,3],[21,26],81],8:[[0,2],11,21,22],9:[[0,3],[21,23],81],10:[[0,3],[21,28],81],11:[[0,3],[21,29]],12:[[0,2],[21,30],81],13:[[0,2],21,22,81,82],31:[0,1,[22,27],30]},44:{0:[0],1:[[0,7],[11,16],83,84],2:[[0,5],21,22,24,29,32,33,81,82],3:[0,1,[3,8]],4:[[0,4]],5:[0,1,[6,15],23,82,83],6:[0,1,[4,8]],7:[0,1,[3,5],81,[83,85]],8:[[0,4],11,23,25,[81,83]],9:[[0,3],23,[81,83]],12:[[0,3],[23,26],83,84],13:[[0,3],[22,24],81],14:[[0,2],[21,24],26,27,81],15:[[0,2],21,23,81],16:[[0,2],[21,25]],17:[[0,2],21,23,81],18:[[0,3],21,23,[25,27],81,82],19:[0],20:[0],51:[[0,3],21,22],52:[[0,3],21,22,24,81],53:[[0,2],[21,23],81]},45:{0:[0],1:[[0,9],[21,27]],2:[[0,5],[21,26]],3:[[0,5],11,12,[21,32]],4:[0,1,[3,6],11,[21,23],81],5:[[0,3],12,21],6:[[0,3],21,81],7:[[0,3],21,22],8:[[0,4],21,81],9:[[0,3],[21,24],81],10:[[0,2],[21,31]],11:[[0,2],[21,23]],12:[[0,2],[21,29],81],13:[[0,2],[21,24],81],14:[[0,2],[21,25],81]},46:{0:[0],1:[0,1,[5,8]],2:[0,1],3:[0,[21,23]],90:[[0,3],[5,7],[21,39]]},50:{0:[0],1:[[0,19]],2:[0,[22,38],[40,43]],3:[0,[81,84]]},51:{0:[0],1:[0,1,[4,8],[12,15],[21,24],29,31,32,[81,84]],3:[[0,4],11,21,22],4:[[0,3],11,21,22],5:[[0,4],21,22,24,25],6:[0,1,3,23,26,[81,83]],7:[0,1,3,4,[22,27],81],8:[[0,2],11,12,[21,24]],9:[[0,4],[21,23]],10:[[0,2],11,24,25,28],11:[[0,2],[11,13],23,24,26,29,32,33,81],13:[[0,4],[21,25],81],14:[[0,2],[21,25]],15:[[0,3],[21,29]],16:[[0,3],[21,23],81],17:[[0,3],[21,25],81],18:[[0,3],[21,27]],19:[[0,3],[21,23]],20:[[0,2],21,22,81],32:[0,[21,33]],33:[0,[21,38]],34:[0,1,[22,37]]},52:{0:[0],1:[[0,3],[11,15],[21,23],81],2:[0,1,3,21,22],3:[[0,3],[21,30],81,82],4:[[0,2],[21,25]],5:[[0,2],[21,27]],6:[[0,3],[21,28]],22:[0,1,[22,30]],23:[0,1,[22,28]],24:[0,1,[22,28]],26:[0,1,[22,36]],27:[[0,2],22,23,[25,32]]},53:{0:[0],1:[[0,3],[11,14],21,22,[24,29],81],3:[[0,2],[21,26],28,81],4:[[0,2],[21,28]],5:[[0,2],[21,24]],6:[[0,2],[21,30]],7:[[0,2],[21,24]],8:[[0,2],[21,29]],9:[[0,2],[21,27]],23:[0,1,[22,29],31],25:[[0,4],[22,32]],26:[0,1,[21,28]],27:[0,1,[22,30]],28:[0,1,22,23],29:[0,1,[22,32]],31:[0,2,3,[22,24]],34:[0,[21,23]],33:[0,21,[23,25]],35:[0,[21,28]]},54:{0:[0],1:[[0,2],[21,27]],21:[0,[21,29],32,33],22:[0,[21,29],[31,33]],23:[0,1,[22,38]],24:[0,[21,31]],25:[0,[21,27]],26:[0,[21,27]]},61:{0:[0],1:[[0,4],[11,16],22,[24,26]],2:[[0,4],22],3:[[0,4],[21,24],[26,31]],4:[[0,4],[22,31],81],5:[[0,2],[21,28],81,82],6:[[0,2],[21,32]],7:[[0,2],[21,30]],8:[[0,2],[21,31]],9:[[0,2],[21,29]],10:[[0,2],[21,26]]},62:{0:[0],1:[[0,5],11,[21,23]],2:[0,1],3:[[0,2],21],4:[[0,3],[21,23]],5:[[0,3],[21,25]],6:[[0,2],[21,23]],7:[[0,2],[21,25]],8:[[0,2],[21,26]],9:[[0,2],[21,24],81,82],10:[[0,2],[21,27]],11:[[0,2],[21,26]],12:[[0,2],[21,28]],24:[0,21,[24,29]],26:[0,21,[23,30]],29:[0,1,[21,27]],30:[0,1,[21,27]]},63:{0:[0],1:[[0,5],[21,23]],2:[0,2,[21,25]],21:[0,[21,23],[26,28]],22:[0,[21,24]],23:[0,[21,24]],25:[0,[21,25]],26:[0,[21,26]],27:[0,1,[21,26]],28:[[0,2],[21,23]]},64:{0:[0],1:[0,1,[4,6],21,22,81],2:[[0,3],5,[21,23]],3:[[0,3],[21,24],81],4:[[0,2],[21,25]],5:[[0,2],21,22]},65:{0:[0],1:[[0,9],21],2:[[0,5]],21:[0,1,22,23],22:[0,1,22,23],23:[[0,3],[23,25],27,28],28:[0,1,[22,29]],29:[0,1,[22,29]],30:[0,1,[22,24]],31:[0,1,[21,31]],32:[0,1,[21,27]],40:[0,2,3,[21,28]],42:[[0,2],21,[23,26]],43:[0,1,[21,26]],90:[[0,4]],27:[[0,2],22,23]},71:{0:[0]},81:{0:[0]},82:{0:[0]}},d=parseInt(b.substr(0,2),10),e=parseInt(b.substr(2,2),10),f=parseInt(b.substr(4,2),10);if(!c[d]||!c[d][e])return!1;for(var g=!1,h=c[d][e],i=0;i<h.length;i++)if(a.isArray(h[i])&&h[i][0]<=f&&f<=h[i][1]||!a.isArray(h[i])&&f===h[i]){g=!0;break}if(!g)return!1;var j;j=18===b.length?b.substr(6,8):"19"+b.substr(6,6);var k=parseInt(j.substr(0,4),10),l=parseInt(j.substr(4,2),10),m=parseInt(j.substr(6,2),10);if(!FormValidation.Helper.date(k,l,m))return!1;if(18===b.length){var n=0,o=[7,9,10,5,8,4,2,1,6,3,7,9,10,5,8,4,2];for(i=0;17>i;i++)n+=parseInt(b.charAt(i),10)*o[i];n=(12-n%11)%11;var p="X"!==b.charAt(17).toUpperCase()?parseInt(b.charAt(17),10):10;return p===n}return!0},_cz:function(a){if(!/^\d{9,10}$/.test(a))return!1;var b=1900+parseInt(a.substr(0,2),10),c=parseInt(a.substr(2,2),10)%50%20,d=parseInt(a.substr(4,2),10);if(9===a.length){if(b>=1980&&(b-=100),b>1953)return!1}else 1954>b&&(b+=100);if(!FormValidation.Helper.date(b,c,d))return!1;if(10===a.length){var e=parseInt(a.substr(0,9),10)%11;return 1985>b&&(e%=10),e+""===a.substr(9,1)}return!0},_dk:function(a){if(!/^[0-9]{6}[-]{0,1}[0-9]{4}$/.test(a))return!1;a=a.replace(/-/g,"");var b=parseInt(a.substr(0,2),10),c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10);switch(!0){case-1!=="5678".indexOf(a.charAt(6))&&d>=58:d+=1800;break;case-1!=="0123".indexOf(a.charAt(6)):case-1!=="49".indexOf(a.charAt(6))&&d>=37:d+=1900;break;default:d+=2e3}return FormValidation.Helper.date(d,c,b)},_ee:function(a){return this._lt(a)},_es:function(a){var b=/^[0-9]{8}[-]{0,1}[A-HJ-NP-TV-Z]$/.test(a),c=/^[XYZ][-]{0,1}[0-9]{7}[-]{0,1}[A-HJ-NP-TV-Z]$/.test(a),d=/^[A-HNPQS][-]{0,1}[0-9]{7}[-]{0,1}[0-9A-J]$/.test(a);if(!b&&!c&&!d)return!1;a=a.replace(/-/g,"");var e,f,g=!0;if(b||c){f="DNI";var h="XYZ".indexOf(a.charAt(0));return-1!==h&&(a=h+a.substr(1)+"",f="NIE"),e=parseInt(a.substr(0,8),10),e="TRWAGMYFPDXBNJZSQVHLCKE"[e%23],{valid:e===a.substr(8,1),type:f}}e=a.substr(1,7),f="CIF";for(var i=a[0],j=a.substr(-1),k=0,l=0;l<e.length;l++)if(l%2!==0)k+=parseInt(e[l],10);else{var m=""+2*parseInt(e[l],10);k+=parseInt(m[0],10),2===m.length&&(k+=parseInt(m[1],10))}var n=k-10*Math.floor(k/10);return 0!==n&&(n=10-n),g=-1!=="KQS".indexOf(i)?j==="JABCDEFGHI"[n]:-1!=="ABEH".indexOf(i)?j===""+n:j===""+n||j==="JABCDEFGHI"[n],{valid:g,type:f}},_fi:function(a){if(!/^[0-9]{6}[-+A][0-9]{3}[0-9ABCDEFHJKLMNPRSTUVWXY]$/.test(a))return!1;var b=parseInt(a.substr(0,2),10),c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10),e={"+":1800,"-":1900,A:2e3};if(d=e[a.charAt(6)]+d,!FormValidation.Helper.date(d,c,b))return!1;var f=parseInt(a.substr(7,3),10);if(2>f)return!1;var g=a.substr(0,6)+a.substr(7,3)+"";return g=parseInt(g,10),"0123456789ABCDEFHJKLMNPRSTUVWXY".charAt(g%31)===a.charAt(10)},_hr:function(a){return/^[0-9]{11}$/.test(a)?FormValidation.Helper.mod11And10(a):!1},_ie:function(a){if(!/^\d{7}[A-W][AHWTX]?$/.test(a))return!1;var b=function(a){for(;a.length<7;)a="0"+a;for(var b="WABCDEFGHIJKLMNOPQRSTUV",c=0,d=0;7>d;d++)c+=parseInt(a.charAt(d),10)*(8-d);return c+=9*b.indexOf(a.substr(7)),b[c%23]};return 9!==a.length||"A"!==a.charAt(8)&&"H"!==a.charAt(8)?a.charAt(7)===b(a.substr(0,7)):a.charAt(7)===b(a.substr(0,7)+a.substr(8)+"")},_is:function(a){if(!/^[0-9]{6}[-]{0,1}[0-9]{4}$/.test(a))return!1;a=a.replace(/-/g,"");var b=parseInt(a.substr(0,2),10),c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10),e=parseInt(a.charAt(9),10);if(d=9===e?1900+d:100*(20+e)+d,!FormValidation.Helper.date(d,c,b,!0))return!1;for(var f=0,g=[3,2,7,6,5,4,3,2],h=0;8>h;h++)f+=parseInt(a.charAt(h),10)*g[h];return f=11-f%11,f+""===a.charAt(8)},_lt:function(a){if(!/^[0-9]{11}$/.test(a))return!1;var b=parseInt(a.charAt(0),10),c=parseInt(a.substr(1,2),10),d=parseInt(a.substr(3,2),10),e=parseInt(a.substr(5,2),10),f=b%2===0?17+b/2:17+(b+1)/2;if(c=100*f+c,!FormValidation.Helper.date(c,d,e,!0))return!1;for(var g=0,h=[1,2,3,4,5,6,7,8,9,1],i=0;10>i;i++)g+=parseInt(a.charAt(i),10)*h[i];if(g%=11,10!==g)return g+""===a.charAt(10);for(g=0,h=[3,4,5,6,7,8,9,1,2,3],i=0;10>i;i++)g+=parseInt(a.charAt(i),10)*h[i];return g%=11,10===g&&(g=0),g+""===a.charAt(10)},_lv:function(a){if(!/^[0-9]{6}[-]{0,1}[0-9]{5}$/.test(a))return!1;a=a.replace(/\D/g,"");var b=parseInt(a.substr(0,2),10),c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10);if(d=d+1800+100*parseInt(a.charAt(6),10),!FormValidation.Helper.date(d,c,b,!0))return!1;for(var e=0,f=[10,5,8,4,2,1,6,3,7,9],g=0;10>g;g++)e+=parseInt(a.charAt(g),10)*f[g];return e=(e+1)%11%10,e+""===a.charAt(10)},_nl:function(a){if(a.length<8)return!1;if(8===a.length&&(a="0"+a),!/^[0-9]{4}[.]{0,1}[0-9]{2}[.]{0,1}[0-9]{3}$/.test(a))return!1;if(a=a.replace(/\./g,""),0===parseInt(a,10))return!1;for(var b=0,c=a.length,d=0;c-1>d;d++)b+=(9-d)*parseInt(a.charAt(d),10);return b%=11,10===b&&(b=0),b+""===a.charAt(c-1)},_pl:function(a){if(!/^[0-9]{11}$/.test(a))return!1;for(var b=0,c=a.length,d=[1,3,7,9,1,3,7,9,1,3,7],e=0;c-1>e;e++)b+=d[e]*parseInt(a.charAt(e),10);return b%=10,0===b&&(b=10),b=10-b,b+""===a.charAt(c-1)},_ro:function(a){if(!/^[0-9]{13}$/.test(a))return!1;var b=parseInt(a.charAt(0),10);if(0===b||7===b||8===b)return!1;var c=parseInt(a.substr(1,2),10),d=parseInt(a.substr(3,2),10),e=parseInt(a.substr(5,2),10),f={1:1900,2:1900,3:1800,4:1800,5:2e3,6:2e3};if(e>31&&d>12)return!1;if(9!==b&&(c=f[b+""]+c,!FormValidation.Helper.date(c,d,e)))return!1;for(var g=0,h=[2,7,9,1,4,6,3,5,8,2,7,9],i=a.length,j=0;i-1>j;j++)g+=parseInt(a.charAt(j),10)*h[j];return g%=11,10===g&&(g=1),g+""===a.charAt(i-1)},_se:function(a){if(!/^[0-9]{10}$/.test(a)&&!/^[0-9]{6}[-|+][0-9]{4}$/.test(a))return!1;a=a.replace(/[^0-9]/g,"");var b=parseInt(a.substr(0,2),10)+1900,c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10);return FormValidation.Helper.date(b,c,d)?FormValidation.Helper.luhn(a):!1},_sk:function(a){return this._cz(a)},_sm:function(a){return/^\d{5}$/.test(a)},_th:function(a){if(13!==a.length)return!1;for(var b=0,c=0;12>c;c++)b+=parseInt(a.charAt(c),10)*(13-c);return(11-b%11)%10===parseInt(a.charAt(12),10)},_tr:function(a){if(11!==a.length)return!1;for(var b=0,c=0;10>c;c++)b+=parseInt(a.charAt(c),10);return b%10===parseInt(a.charAt(10),10)},_za:function(a){if(!/^[0-9]{10}[0|1][8|9][0-9]$/.test(a))return!1;var b=parseInt(a.substr(0,2),10),c=(new Date).getFullYear()%100,d=parseInt(a.substr(2,2),10),e=parseInt(a.substr(4,2),10);return b=b>=c?b+1900:b+2e3,FormValidation.Helper.date(b,d,e)?FormValidation.Helper.luhn(a):!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{identical:{"default":"Please enter the same value"}}}),FormValidation.Validator.identical={html5Attributes:{message:"message",field:"field"},init:function(a,b,c,d){var e=a.getFieldElements(c.field);a.onLiveChange(e,"live_"+d,function(){var c=a.getStatus(b,d);c!==a.STATUS_NOT_VALIDATED&&a.revalidateField(b)})},destroy:function(a,b,c,d){var e=a.getFieldElements(c.field);a.offLiveChange(e,"live_"+d)},validate:function(a,b,c,d){var e=a.getFieldValue(b,d),f=a.getFieldElements(c.field);if(null===f||0===f.length)return!0;var g=a.getFieldValue(f,d);return e===g?(a.updateStatus(f,a.STATUS_VALID,d),!0):!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{imei:{"default":"Please enter a valid IMEI number"}}}),FormValidation.Validator.imei={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;switch(!0){case/^\d{15}$/.test(e):case/^\d{2}-\d{6}-\d{6}-\d{1}$/.test(e):case/^\d{2}\s\d{6}\s\d{6}\s\d{1}$/.test(e):return e=e.replace(/[^0-9]/g,""),FormValidation.Helper.luhn(e);case/^\d{14}$/.test(e):case/^\d{16}$/.test(e):case/^\d{2}-\d{6}-\d{6}(|-\d{2})$/.test(e):case/^\d{2}\s\d{6}\s\d{6}(|\s\d{2})$/.test(e):return!0;default:return!1}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{imo:{"default":"Please enter a valid IMO number"}}}),FormValidation.Validator.imo={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;if(!/^IMO \d{7}$/i.test(e))return!1;for(var f=0,g=e.replace(/^.*(\d{7})$/,"$1"),h=6;h>=1;h--)f+=g.slice(6-h,-h)*(h+1);return f%10===parseInt(g.charAt(6),10)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{integer:{"default":"Please enter a valid number"}}}),FormValidation.Validator.integer={html5Attributes:{message:"message",thousandsseparator:"thousandsSeparator",decimalseparator:"decimalSeparator"},enableByHtml5:function(a){return"number"===a.attr("type")&&(void 0===a.attr("step")||a.attr("step")%1===0)},validate:function(a,b,c,d){if(this.enableByHtml5(b)&&b.get(0).validity&&b.get(0).validity.badInput===!0)return!1;var e=a.getFieldValue(b,d);if(""===e)return!0;var f=c.decimalSeparator||".",g=c.thousandsSeparator||"";f="."===f?"\\.":f,g="."===g?"\\.":g;var h=new RegExp("^-?[0-9]{1,3}("+g+"[0-9]{3})*("+f+"[0-9]+)?$"),i=new RegExp(g,"g");return h.test(e)?(g&&(e=e.replace(i,"")),f&&(e=e.replace(f,".")),isNaN(e)||!isFinite(e)?!1:(e=parseFloat(e),Math.floor(e)===e)):!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{ip:{"default":"Please enter a valid IP address",ipv4:"Please enter a valid IPv4 address",ipv6:"Please enter a valid IPv6 address"}}}),FormValidation.Validator.ip={html5Attributes:{message:"message",ipv4:"ipv4",ipv6:"ipv6"},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;d=a.extend({},{ipv4:!0,ipv6:!0},d);var g,h=b.getLocale(),i=/^(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)(\/([0-9]|[1-2][0-9]|3[0-2]))?$/,j=/^\s*((([0-9A-Fa-f]{1,4}:){7}([0-9A-Fa-f]{1,4}|:))|(([0-9A-Fa-f]{1,4}:){6}(:[0-9A-Fa-f]{1,4}|((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){5}(((:[0-9A-Fa-f]{1,4}){1,2})|:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3})|:))|(([0-9A-Fa-f]{1,4}:){4}(((:[0-9A-Fa-f]{1,4}){1,3})|((:[0-9A-Fa-f]{1,4})?:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){3}(((:[0-9A-Fa-f]{1,4}){1,4})|((:[0-9A-Fa-f]{1,4}){0,2}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){2}(((:[0-9A-Fa-f]{1,4}){1,5})|((:[0-9A-Fa-f]{1,4}){0,3}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(([0-9A-Fa-f]{1,4}:){1}(((:[0-9A-Fa-f]{1,4}){1,6})|((:[0-9A-Fa-f]{1,4}){0,4}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:))|(:(((:[0-9A-Fa-f]{1,4}){1,7})|((:[0-9A-Fa-f]{1,4}){0,5}:((25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)(\.(25[0-5]|2[0-4]\d|1\d\d|[1-9]?\d)){3}))|:)))(%.+)?\s*(\/(\d|\d\d|1[0-1]\d|12[0-8]))?$/,k=!1;switch(!0){case d.ipv4&&!d.ipv6:k=i.test(f),g=d.message||FormValidation.I18n[h].ip.ipv4;break;case!d.ipv4&&d.ipv6:k=j.test(f),g=d.message||FormValidation.I18n[h].ip.ipv6;break;case d.ipv4&&d.ipv6:default:k=i.test(f)||j.test(f),g=d.message||FormValidation.I18n[h].ip["default"]}return{valid:k,message:g}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{isbn:{"default":"Please enter a valid ISBN number"}}}),FormValidation.Validator.isbn={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f;switch(!0){case/^\d{9}[\dX]$/.test(e):case 13===e.length&&/^(\d+)-(\d+)-(\d+)-([\dX])$/.test(e):case 13===e.length&&/^(\d+)\s(\d+)\s(\d+)\s([\dX])$/.test(e):f="ISBN10";break;case/^(978|979)\d{9}[\dX]$/.test(e):case 17===e.length&&/^(978|979)-(\d+)-(\d+)-(\d+)-([\dX])$/.test(e):case 17===e.length&&/^(978|979)\s(\d+)\s(\d+)\s(\d+)\s([\dX])$/.test(e):f="ISBN13";break;default:return!1}e=e.replace(/[^0-9X]/gi,"");var g,h,i=e.split(""),j=i.length,k=0;switch(f){case"ISBN10":for(k=0,g=0;j-1>g;g++)k+=parseInt(i[g],10)*(10-g);return h=11-k%11,11===h?h=0:10===h&&(h="X"),{type:f,valid:h+""===i[j-1]};case"ISBN13":for(k=0,g=0;j-1>g;g++)k+=g%2===0?parseInt(i[g],10):3*parseInt(i[g],10);return h=10-k%10,10===h&&(h="0"),{type:f,valid:h+""===i[j-1]};default:return!1}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{isin:{"default":"Please enter a valid ISIN number"}}}),FormValidation.Validator.isin={COUNTRY_CODES:"AF|AX|AL|DZ|AS|AD|AO|AI|AQ|AG|AR|AM|AW|AU|AT|AZ|BS|BH|BD|BB|BY|BE|BZ|BJ|BM|BT|BO|BQ|BA|BW|BV|BR|IO|BN|BG|BF|BI|KH|CM|CA|CV|KY|CF|TD|CL|CN|CX|CC|CO|KM|CG|CD|CK|CR|CI|HR|CU|CW|CY|CZ|DK|DJ|DM|DO|EC|EG|SV|GQ|ER|EE|ET|FK|FO|FJ|FI|FR|GF|PF|TF|GA|GM|GE|DE|GH|GI|GR|GL|GD|GP|GU|GT|GG|GN|GW|GY|HT|HM|VA|HN|HK|HU|IS|IN|ID|IR|IQ|IE|IM|IL|IT|JM|JP|JE|JO|KZ|KE|KI|KP|KR|KW|KG|LA|LV|LB|LS|LR|LY|LI|LT|LU|MO|MK|MG|MW|MY|MV|ML|MT|MH|MQ|MR|MU|YT|MX|FM|MD|MC|MN|ME|MS|MA|MZ|MM|NA|NR|NP|NL|NC|NZ|NI|NE|NG|NU|NF|MP|NO|OM|PK|PW|PS|PA|PG|PY|PE|PH|PN|PL|PT|PR|QA|RE|RO|RU|RW|BL|SH|KN|LC|MF|PM|VC|WS|SM|ST|SA|SN|RS|SC|SL|SG|SX|SK|SI|SB|SO|ZA|GS|SS|ES|LK|SD|SR|SJ|SZ|SE|CH|SY|TW|TJ|TZ|TH|TL|TG|TK|TO|TT|TN|TR|TM|TC|TV|UG|UA|AE|GB|US|UM|UY|UZ|VU|VE|VN|VG|VI|WF|EH|YE|ZM|ZW",validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;e=e.toUpperCase();var f=new RegExp("^("+this.COUNTRY_CODES+")[0-9A-Z]{10}$");if(!f.test(e))return!1;for(var g="",h=e.length,i=0;h-1>i;i++){var j=e.charCodeAt(i);g+=j>57?(j-55).toString():e.charAt(i)}var k="",l=g.length,m=l%2!==0?0:1;for(i=0;l>i;i++)k+=parseInt(g[i],10)*(i%2===m?2:1)+"";var n=0;for(i=0;i<k.length;i++)n+=parseInt(k.charAt(i),10);return n=(10-n%10)%10,n+""===e.charAt(h-1)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{ismn:{"default":"Please enter a valid ISMN number"}}}),FormValidation.Validator.ismn={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f;switch(!0){case/^M\d{9}$/.test(e):case/^M-\d{4}-\d{4}-\d{1}$/.test(e):case/^M\s\d{4}\s\d{4}\s\d{1}$/.test(e):f="ISMN10";break;case/^9790\d{9}$/.test(e):case/^979-0-\d{4}-\d{4}-\d{1}$/.test(e):case/^979\s0\s\d{4}\s\d{4}\s\d{1}$/.test(e):f="ISMN13";break;default:return!1}"ISMN10"===f&&(e="9790"+e.substr(1)),e=e.replace(/[^0-9]/gi,"");for(var g=e.length,h=0,i=[1,3],j=0;g-1>j;j++)h+=parseInt(e.charAt(j),10)*i[j%2];return h=10-h%10,{type:f,valid:h+""===e.charAt(g-1)}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{issn:{"default":"Please enter a valid ISSN number"}}}),FormValidation.Validator.issn={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;if(!/^\d{4}\-\d{3}[\dX]$/.test(e))return!1;e=e.replace(/[^0-9X]/gi,"");var f=e.split(""),g=f.length,h=0;"X"===f[7]&&(f[7]=10);for(var i=0;g>i;i++)h+=parseInt(f[i],10)*(8-i);return h%11===0}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{lessThan:{"default":"Please enter a value less than or equal to %s",notInclusive:"Please enter a value less than %s"}}}),FormValidation.Validator.lessThan={html5Attributes:{message:"message",value:"value",inclusive:"inclusive"},enableByHtml5:function(a){var b=a.attr("type"),c=a.attr("max");return c&&"date"!==b?{value:c}:!1},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;f=this._format(f);var g=b.getLocale(),h=a.isNumeric(d.value)?d.value:b.getDynamicOption(c,d.value),i=this._format(h);return d.inclusive===!0||void 0===d.inclusive?{valid:a.isNumeric(f)&&parseFloat(f)<=i,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].lessThan["default"],h)}:{valid:a.isNumeric(f)&&parseFloat(f)<i,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].lessThan.notInclusive,h)}},_format:function(a){return(a+"").replace(",",".")}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{mac:{"default":"Please enter a valid MAC address"}}}),FormValidation.Validator.mac={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:/^([0-9A-Fa-f]{2}[:-]){5}([0-9A-Fa-f]{2})$/.test(e)||/^([0-9A-Fa-f]{4}\.){2}([0-9A-Fa-f]{4})$/.test(e)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{meid:{"default":"Please enter a valid MEID number"}}}),FormValidation.Validator.meid={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;switch(!0){case/^[0-9A-F]{15}$/i.test(e):case/^[0-9A-F]{2}[- ][0-9A-F]{6}[- ][0-9A-F]{6}[- ][0-9A-F]$/i.test(e):case/^\d{19}$/.test(e):case/^\d{5}[- ]\d{5}[- ]\d{4}[- ]\d{4}[- ]\d$/.test(e):var f=e.charAt(e.length-1);if(e=e.replace(/[- ]/g,""),e.match(/^\d*$/i))return FormValidation.Helper.luhn(e);e=e.slice(0,-1);for(var g="",h=1;13>=h;h+=2)g+=(2*parseInt(e.charAt(h),16)).toString(16);var i=0;for(h=0;h<g.length;h++)i+=parseInt(g.charAt(h),16);return i%10===0?"0"===f:f===(2*(10*Math.floor((i+10)/10)-i)).toString(16);case/^[0-9A-F]{14}$/i.test(e):case/^[0-9A-F]{2}[- ][0-9A-F]{6}[- ][0-9A-F]{6}$/i.test(e):case/^\d{18}$/.test(e):case/^\d{5}[- ]\d{5}[- ]\d{4}[- ]\d{4}$/.test(e):return!0;default:return!1}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{notEmpty:{"default":"Please enter a value"}}}),FormValidation.Validator.notEmpty={enableByHtml5:function(a){var b=a.attr("required")+"";return"required"===b||"true"===b},validate:function(b,c,d,e){var f=c.attr("type");if("radio"===f||"checkbox"===f){var g=b.getNamespace();return b.getFieldElements(c.attr("data-"+g+"-field")).filter(":checked").length>0}if("number"===f&&c.get(0).validity&&c.get(0).validity.badInput===!0)return!0;var h=b.getFieldValue(c,e);return""!==a.trim(h)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{numeric:{"default":"Please enter a valid float number"}}}),FormValidation.Validator.numeric={html5Attributes:{message:"message",separator:"separator",thousandsseparator:"thousandsSeparator",decimalseparator:"decimalSeparator"
},enableByHtml5:function(a){return"number"===a.attr("type")&&void 0!==a.attr("step")&&a.attr("step")%1!==0},validate:function(a,b,c,d){if(this.enableByHtml5(b)&&b.get(0).validity&&b.get(0).validity.badInput===!0)return!1;var e=a.getFieldValue(b,d);if(""===e)return!0;var f=c.separator||c.decimalSeparator||".",g=c.thousandsSeparator||"";e.substr(0,1)===f?e="0"+f+e.substr(1):e.substr(0,2)==="-"+f&&(e="-0"+f+e.substr(2)),f="."===f?"\\.":f,g="."===g?"\\.":g;var h=new RegExp("^-?[0-9]{1,3}("+g+"[0-9]{3})*("+f+"[0-9]+)?$"),i=new RegExp(g,"g");return h.test(e)?(g&&(e=e.replace(i,"")),f&&(e=e.replace(f,".")),!isNaN(parseFloat(e))&&isFinite(e)):!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{phone:{"default":"Please enter a valid phone number",country:"Please enter a valid phone number in %s",countries:{AE:"United Arab Emirates",BG:"Bulgaria",BR:"Brazil",CN:"China",CZ:"Czech Republic",DE:"Germany",DK:"Denmark",ES:"Spain",FR:"France",GB:"United Kingdom",IN:"India",MA:"Morocco",NL:"Netherlands",PK:"Pakistan",RO:"Romania",RU:"Russia",SK:"Slovakia",TH:"Thailand",US:"USA",VE:"Venezuela"}}}}),FormValidation.Validator.phone={html5Attributes:{message:"message",country:"country"},COUNTRY_CODES:["AE","BG","BR","CN","CZ","DE","DK","ES","FR","GB","IN","MA","NL","PK","RO","RU","SK","TH","US","VE"],validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;var g=b.getLocale(),h=d.country;if(("string"!=typeof h||-1===a.inArray(h,this.COUNTRY_CODES))&&(h=b.getDynamicOption(c,h)),!h||-1===a.inArray(h.toUpperCase(),this.COUNTRY_CODES))return!0;var i=!0;switch(h.toUpperCase()){case"AE":f=a.trim(f),i=/^(((\+|00)?971[\s\.-]?(\(0\)[\s\.-]?)?|0)(\(5(0|2|5|6)\)|5(0|2|5|6)|2|3|4|6|7|9)|60)([\s\.-]?[0-9]){7}$/.test(f);break;case"BG":f=f.replace(/\+|\s|-|\/|\(|\)/gi,""),i=/^(0|359|00)(((700|900)[0-9]{5}|((800)[0-9]{5}|(800)[0-9]{4}))|(87|88|89)([0-9]{7})|((2[0-9]{7})|(([3-9][0-9])(([0-9]{6})|([0-9]{5})))))$/.test(f);break;case"BR":f=a.trim(f),i=/^(([\d]{4}[-.\s]{1}[\d]{2,3}[-.\s]{1}[\d]{2}[-.\s]{1}[\d]{2})|([\d]{4}[-.\s]{1}[\d]{3}[-.\s]{1}[\d]{4})|((\(?\+?[0-9]{2}\)?\s?)?(\(?\d{2}\)?\s?)?\d{4,5}[-.\s]?\d{4}))$/.test(f);break;case"CN":f=a.trim(f),i=/^((00|\+)?(86(?:-| )))?((\d{11})|(\d{3}[- ]{1}\d{4}[- ]{1}\d{4})|((\d{2,4}[- ]){1}(\d{7,8}|(\d{3,4}[- ]{1}\d{4}))([- ]{1}\d{1,4})?))$/.test(f);break;case"CZ":i=/^(((00)([- ]?)|\+)(420)([- ]?))?((\d{3})([- ]?)){2}(\d{3})$/.test(f);break;case"DE":f=a.trim(f),i=/^(((((((00|\+)49[ \-/]?)|0)[1-9][0-9]{1,4})[ \-/]?)|((((00|\+)49\()|\(0)[1-9][0-9]{1,4}\)[ \-/]?))[0-9]{1,7}([ \-/]?[0-9]{1,5})?)$/.test(f);break;case"DK":f=a.trim(f),i=/^(\+45|0045|\(45\))?\s?[2-9](\s?\d){7}$/.test(f);break;case"ES":f=a.trim(f),i=/^(?:(?:(?:\+|00)34\D?))?(?:5|6|7|8|9)(?:\d\D?){8}$/.test(f);break;case"FR":f=a.trim(f),i=/^(?:(?:(?:\+|00)33[ ]?(?:\(0\)[ ]?)?)|0){1}[1-9]{1}([ .-]?)(?:\d{2}\1?){3}\d{2}$/.test(f);break;case"GB":f=a.trim(f),i=/^\(?(?:(?:0(?:0|11)\)?[\s-]?\(?|\+)44\)?[\s-]?\(?(?:0\)?[\s-]?\(?)?|0)(?:\d{2}\)?[\s-]?\d{4}[\s-]?\d{4}|\d{3}\)?[\s-]?\d{3}[\s-]?\d{3,4}|\d{4}\)?[\s-]?(?:\d{5}|\d{3}[\s-]?\d{3})|\d{5}\)?[\s-]?\d{4,5}|8(?:00[\s-]?11[\s-]?11|45[\s-]?46[\s-]?4\d))(?:(?:[\s-]?(?:x|ext\.?\s?|\#)\d+)?)$/.test(f);break;case"IN":f=a.trim(f),i=/((\+?)((0[ -]+)*|(91 )*)(\d{12}|\d{10}))|\d{5}([- ]*)\d{6}/.test(f);break;case"MA":f=a.trim(f),i=/^(?:(?:(?:\+|00)212[\s]?(?:[\s]?\(0\)[\s]?)?)|0){1}(?:5[\s.-]?[2-3]|6[\s.-]?[13-9]){1}[0-9]{1}(?:[\s.-]?\d{2}){3}$/.test(f);break;case"NL":f=a.trim(f),i=/^((\+|00(\s|\s?\-\s?)?)31(\s|\s?\-\s?)?(\(0\)[\-\s]?)?|0)[1-9]((\s|\s?\-\s?)?[0-9])((\s|\s?-\s?)?[0-9])((\s|\s?-\s?)?[0-9])\s?[0-9]\s?[0-9]\s?[0-9]\s?[0-9]\s?[0-9]$/gm.test(f);break;case"PK":f=a.trim(f),i=/^0?3[0-9]{2}[0-9]{7}$/.test(f);break;case"RO":i=/^(\+4|)?(07[0-8]{1}[0-9]{1}|02[0-9]{2}|03[0-9]{2}){1}?(\s|\.|\-)?([0-9]{3}(\s|\.|\-|)){2}$/g.test(f);break;case"RU":i=/^((8|\+7|007)[\-\.\/ ]?)?([\(\/\.]?\d{3}[\)\/\.]?[\-\.\/ ]?)?[\d\-\.\/ ]{7,10}$/g.test(f);break;case"SK":i=/^(((00)([- ]?)|\+)(421)([- ]?))?((\d{3})([- ]?)){2}(\d{3})$/.test(f);break;case"TH":i=/^0\(?([6|8-9]{2})*-([0-9]{3})*-([0-9]{4})$/.test(f);break;case"VE":f=a.trim(f),i=/^0(?:2(?:12|4[0-9]|5[1-9]|6[0-9]|7[0-8]|8[1-35-8]|9[1-5]|3[45789])|4(?:1[246]|2[46]))\d{7}$/.test(f);break;case"US":default:i=/^(?:(1\-?)|(\+1 ?))?\(?\d{3}\)?[\-\.\s]?\d{3}[\-\.\s]?\d{4}$/.test(f)}return{valid:i,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].phone.country,FormValidation.I18n[g].phone.countries[h])}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{promise:{"default":"Please enter a valid value"}}}),FormValidation.Validator.promise={priority:999,html5Attributes:{message:"message",promise:"promise"},validate:function(b,c,d,e){var f=b.getFieldValue(c,e),g=new a.Deferred,h=FormValidation.Helper.call(d.promise,[f,b,c]);return h.done(function(a){g.resolve(c,e,a)}).fail(function(a){a=a||{},a.valid=!1,g.resolve(c,e,a)}),g}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{regexp:{"default":"Please enter a value matching the pattern"}}}),FormValidation.Validator.regexp={html5Attributes:{message:"message",flags:"flags",regexp:"regexp"},enableByHtml5:function(a){var b=a.attr("pattern");return b?{regexp:b}:!1},validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f="string"==typeof c.regexp?c.flags?new RegExp(c.regexp,c.flags):new RegExp(c.regexp):c.regexp;return f.test(e)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{remote:{"default":"Please enter a valid value"}}}),FormValidation.Validator.remote={priority:1e3,html5Attributes:{async:"async",crossdomain:"crossDomain",data:"data",datatype:"dataType",delay:"delay",message:"message",name:"name",type:"type",url:"url",validkey:"validKey"},destroy:function(a,b,c,d){var e=a.getNamespace(),f=b.data(e+"."+d+".timer");f&&(clearTimeout(f),b.removeData(e+"."+d+".timer"))},validate:function(b,c,d,e){function f(){var b=a.ajax(n);return b.success(function(a){a.valid=a[m]===!0||"true"===a[m]?!0:a[m]===!1||"false"===a[m]?!1:null,i.resolve(c,e,a)}).error(function(a){i.resolve(c,e,{valid:!1})}),i.fail(function(){b.abort()}),i}var g=b.getNamespace(),h=b.getFieldValue(c,e),i=new a.Deferred;if(""===h)return i.resolve(c,e,{valid:!0}),i;var j=c.attr("data-"+g+"-field"),k=d.data||{},l=d.url,m=d.validKey||"valid";"function"==typeof k&&(k=k.call(this,b,c,h)),"string"==typeof k&&(k=JSON.parse(k)),"function"==typeof l&&(l=l.call(this,b,c,h)),k[d.name||j]=h;var n={async:null===d.async||d.async===!0||"true"===d.async,data:k,dataType:d.dataType||"json",headers:d.headers||{},type:d.type||"GET",url:l};return null!==d.crossDomain&&(n.crossDomain=d.crossDomain===!0||"true"===d.crossDomain),d.delay?(c.data(g+"."+e+".timer")&&clearTimeout(c.data(g+"."+e+".timer")),c.data(g+"."+e+".timer",setTimeout(f,d.delay)),i):f()}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{rtn:{"default":"Please enter a valid RTN number"}}}),FormValidation.Validator.rtn={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;if(!/^\d{9}$/.test(e))return!1;for(var f=0,g=0;g<e.length;g+=3)f+=3*parseInt(e.charAt(g),10)+7*parseInt(e.charAt(g+1),10)+parseInt(e.charAt(g+2),10);return 0!==f&&f%10===0}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{sedol:{"default":"Please enter a valid SEDOL number"}}}),FormValidation.Validator.sedol={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;if(e=e.toUpperCase(),!/^[0-9A-Z]{7}$/.test(e))return!1;for(var f=0,g=[1,3,1,7,3,9,1],h=e.length,i=0;h-1>i;i++)f+=g[i]*parseInt(e.charAt(i),36);return f=(10-f%10)%10,f+""===e.charAt(h-1)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{siren:{"default":"Please enter a valid SIREN number"}}}),FormValidation.Validator.siren={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);return""===e?!0:/^\d{9}$/.test(e)?FormValidation.Helper.luhn(e):!1}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{siret:{"default":"Please enter a valid SIRET number"}}}),FormValidation.Validator.siret={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;for(var f,g=0,h=e.length,i=0;h>i;i++)f=parseInt(e.charAt(i),10),i%2===0&&(f=2*f,f>9&&(f-=9)),g+=f;return g%10===0}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{step:{"default":"Please enter a valid step of %s"}}}),FormValidation.Validator.step={html5Attributes:{message:"message",base:"baseValue",step:"step"},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;if(d=a.extend({},{baseValue:0,step:1},d),f=parseFloat(f),!a.isNumeric(f))return!1;var g=function(a,b){var c=Math.pow(10,b);a*=c;var d=a>0|-(0>a),e=a%1===.5*d;return e?(Math.floor(a)+(d>0))/c:Math.round(a)/c},h=function(a,b){if(0===b)return 1;var c=(a+"").split("."),d=(b+"").split("."),e=(1===c.length?0:c[1].length)+(1===d.length?0:d[1].length);return g(a-b*Math.floor(a/b),e)},i=b.getLocale(),j=h(f-d.baseValue,d.step);return{valid:0===j||j===d.step,message:FormValidation.Helper.format(d.message||FormValidation.I18n[i].step["default"],[d.step])}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{stringCase:{"default":"Please enter only lowercase characters",upper:"Please enter only uppercase characters"}}}),FormValidation.Validator.stringCase={html5Attributes:{message:"message","case":"case"},validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f=a.getLocale(),g=(c["case"]||"lower").toLowerCase();return{valid:"upper"===g?e===e.toUpperCase():e===e.toLowerCase(),message:c.message||("upper"===g?FormValidation.I18n[f].stringCase.upper:FormValidation.I18n[f].stringCase["default"])}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{stringLength:{"default":"Please enter a value with valid length",less:"Please enter less than %s characters",more:"Please enter more than %s characters",between:"Please enter value between %s and %s characters long"}}}),FormValidation.Validator.stringLength={html5Attributes:{message:"message",min:"min",max:"max",trim:"trim",utf8bytes:"utf8Bytes"},enableByHtml5:function(b){var c={},d=b.attr("maxlength"),e=b.attr("minlength");return d&&(c.max=parseInt(d,10)),e&&(c.min=parseInt(e,10)),a.isEmptyObject(c)?!1:c},validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if((d.trim===!0||"true"===d.trim)&&(f=a.trim(f)),""===f)return!0;var g=b.getLocale(),h=a.isNumeric(d.min)?d.min:b.getDynamicOption(c,d.min),i=a.isNumeric(d.max)?d.max:b.getDynamicOption(c,d.max),j=function(a){for(var b=a.length,c=a.length-1;c>=0;c--){var d=a.charCodeAt(c);d>127&&2047>=d?b++:d>2047&&65535>=d&&(b+=2),d>=56320&&57343>=d&&c--}return b},k=d.utf8Bytes?j(f):f.length,l=!0,m=d.message||FormValidation.I18n[g].stringLength["default"];switch((h&&k<parseInt(h,10)||i&&k>parseInt(i,10))&&(l=!1),!0){case!!h&&!!i:m=FormValidation.Helper.format(d.message||FormValidation.I18n[g].stringLength.between,[parseInt(h,10),parseInt(i,10)]);break;case!!h:m=FormValidation.Helper.format(d.message||FormValidation.I18n[g].stringLength.more,parseInt(h,10)-1);break;case!!i:m=FormValidation.Helper.format(d.message||FormValidation.I18n[g].stringLength.less,parseInt(i,10)+1)}return{valid:l,message:m}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{uri:{"default":"Please enter a valid URI"}}}),FormValidation.Validator.uri={html5Attributes:{message:"message",allowlocal:"allowLocal",allowemptyprotocol:"allowEmptyProtocol",protocol:"protocol"},enableByHtml5:function(a){return"url"===a.attr("type")},validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f=c.allowLocal===!0||"true"===c.allowLocal,g=c.allowEmptyProtocol===!0||"true"===c.allowEmptyProtocol,h=(c.protocol||"http, https, ftp").split(",").join("|").replace(/\s/g,""),i=new RegExp("^(?:(?:"+h+")://)"+(g?"?":"")+"(?:\\S+(?::\\S*)?@)?(?:"+(f?"":"(?!(?:10|127)(?:\\.\\d{1,3}){3})(?!(?:169\\.254|192\\.168)(?:\\.\\d{1,3}){2})(?!172\\.(?:1[6-9]|2\\d|3[0-1])(?:\\.\\d{1,3}){2})")+"(?:[1-9]\\d?|1\\d\\d|2[01]\\d|22[0-3])(?:\\.(?:1?\\d{1,2}|2[0-4]\\d|25[0-5])){2}(?:\\.(?:[1-9]\\d?|1\\d\\d|2[0-4]\\d|25[0-4]))|(?:(?:[a-z\\u00a1-\\uffff0-9]-?)*[a-z\\u00a1-\\uffff0-9]+)(?:\\.(?:[a-z\\u00a1-\\uffff0-9]-?)*[a-z\\u00a1-\\uffff0-9])*(?:\\.(?:[a-z\\u00a1-\\uffff]{2,}))"+(f?"?":"")+")(?::\\d{2,5})?(?:/[^\\s]*)?$","i");return i.test(e)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{uuid:{"default":"Please enter a valid UUID number",version:"Please enter a valid UUID version %s number"}}}),FormValidation.Validator.uuid={html5Attributes:{message:"message",version:"version"},validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;var f=a.getLocale(),g={3:/^[0-9A-F]{8}-[0-9A-F]{4}-3[0-9A-F]{3}-[0-9A-F]{4}-[0-9A-F]{12}$/i,4:/^[0-9A-F]{8}-[0-9A-F]{4}-4[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$/i,5:/^[0-9A-F]{8}-[0-9A-F]{4}-5[0-9A-F]{3}-[89AB][0-9A-F]{3}-[0-9A-F]{12}$/i,all:/^[0-9A-F]{8}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{4}-[0-9A-F]{12}$/i},h=c.version?c.version+"":"all";return{valid:null===g[h]?!0:g[h].test(e),message:c.version?FormValidation.Helper.format(c.message||FormValidation.I18n[f].uuid.version,c.version):c.message||FormValidation.I18n[f].uuid["default"]}}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{vat:{"default":"Please enter a valid VAT number",country:"Please enter a valid VAT number in %s",countries:{AT:"Austria",BE:"Belgium",BG:"Bulgaria",BR:"Brazil",CH:"Switzerland",CY:"Cyprus",CZ:"Czech Republic",DE:"Germany",DK:"Denmark",EE:"Estonia",ES:"Spain",FI:"Finland",FR:"France",GB:"United Kingdom",GR:"Greek",EL:"Greek",HU:"Hungary",HR:"Croatia",IE:"Ireland",IS:"Iceland",IT:"Italy",LT:"Lithuania",LU:"Luxembourg",LV:"Latvia",MT:"Malta",NL:"Netherlands",NO:"Norway",PL:"Poland",PT:"Portugal",RO:"Romania",RU:"Russia",RS:"Serbia",SE:"Sweden",SI:"Slovenia",SK:"Slovakia",VE:"Venezuela",ZA:"South Africa"}}}}),FormValidation.Validator.vat={html5Attributes:{message:"message",country:"country"},COUNTRY_CODES:["AT","BE","BG","BR","CH","CY","CZ","DE","DK","EE","EL","ES","FI","FR","GB","GR","HR","HU","IE","IS","IT","LT","LU","LV","MT","NL","NO","PL","PT","RO","RU","RS","SE","SK","SI","VE","ZA"],validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f)return!0;var g=b.getLocale(),h=d.country;if(h?("string"!=typeof h||-1===a.inArray(h.toUpperCase(),this.COUNTRY_CODES))&&(h=b.getDynamicOption(c,h)):h=f.substr(0,2),-1===a.inArray(h,this.COUNTRY_CODES))return!0;var i=["_",h.toLowerCase()].join(""),j=this[i](f);return j=j===!0||j===!1?{valid:j}:j,j.message=FormValidation.Helper.format(d.message||FormValidation.I18n[g].vat.country,FormValidation.I18n[g].vat.countries[h.toUpperCase()]),j},_at:function(a){if(/^ATU[0-9]{8}$/.test(a)&&(a=a.substr(2)),!/^U[0-9]{8}$/.test(a))return!1;a=a.substr(1);for(var b=0,c=[1,2,1,2,1,2,1],d=0,e=0;7>e;e++)d=parseInt(a.charAt(e),10)*c[e],d>9&&(d=Math.floor(d/10)+d%10),b+=d;return b=10-(b+4)%10,10===b&&(b=0),b+""===a.substr(7,1)},_be:function(a){if(/^BE[0]{0,1}[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0]{0,1}[0-9]{9}$/.test(a))return!1;if(9===a.length&&(a="0"+a),"0"===a.substr(1,1))return!1;var b=parseInt(a.substr(0,8),10)+parseInt(a.substr(8,2),10);return b%97===0},_bg:function(a){if(/^BG[0-9]{9,10}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9,10}$/.test(a))return!1;var b=0,c=0;if(9===a.length){for(c=0;8>c;c++)b+=parseInt(a.charAt(c),10)*(c+1);if(b%=11,10===b)for(b=0,c=0;8>c;c++)b+=parseInt(a.charAt(c),10)*(c+3);return b%=10,b+""===a.substr(8)}if(10===a.length){var d=function(a){var b=parseInt(a.substr(0,2),10)+1900,c=parseInt(a.substr(2,2),10),d=parseInt(a.substr(4,2),10);if(c>40?(b+=100,c-=40):c>20&&(b-=100,c-=20),!FormValidation.Helper.date(b,c,d))return!1;for(var e=0,f=[2,4,8,5,10,9,7,3,6],g=0;9>g;g++)e+=parseInt(a.charAt(g),10)*f[g];return e=e%11%10,e+""===a.substr(9,1)},e=function(a){for(var b=0,c=[21,19,17,13,11,9,7,3,1],d=0;9>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%=10,b+""===a.substr(9,1)},f=function(a){for(var b=0,c=[4,3,2,7,6,5,4,3,2],d=0;9>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b=11-b%11,10===b?!1:(11===b&&(b=0),b+""===a.substr(9,1))};return d(a)||e(a)||f(a)}return!1},_br:function(a){if(""===a)return!0;var b=a.replace(/[^\d]+/g,"");if(""===b||14!==b.length)return!1;if("00000000000000"===b||"11111111111111"===b||"22222222222222"===b||"33333333333333"===b||"44444444444444"===b||"55555555555555"===b||"66666666666666"===b||"77777777777777"===b||"88888888888888"===b||"99999999999999"===b)return!1;for(var c=b.length-2,d=b.substring(0,c),e=b.substring(c),f=0,g=c-7,h=c;h>=1;h--)f+=parseInt(d.charAt(c-h),10)*g--,2>g&&(g=9);var i=2>f%11?0:11-f%11;if(i!==parseInt(e.charAt(0),10))return!1;for(c+=1,d=b.substring(0,c),f=0,g=c-7,h=c;h>=1;h--)f+=parseInt(d.charAt(c-h),10)*g--,2>g&&(g=9);return i=2>f%11?0:11-f%11,i===parseInt(e.charAt(1),10)},_ch:function(a){if(/^CHE[0-9]{9}(MWST)?$/.test(a)&&(a=a.substr(2)),!/^E[0-9]{9}(MWST)?$/.test(a))return!1;a=a.substr(1);for(var b=0,c=[5,4,3,2,7,6,5,4],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b=11-b%11,10===b?!1:(11===b&&(b=0),b+""===a.substr(8,1))},_cy:function(a){if(/^CY[0-5|9]{1}[0-9]{7}[A-Z]{1}$/.test(a)&&(a=a.substr(2)),!/^[0-5|9]{1}[0-9]{7}[A-Z]{1}$/.test(a))return!1;if("12"===a.substr(0,2))return!1;for(var b=0,c={0:1,1:0,2:5,3:7,4:9,5:13,6:15,7:17,8:19,9:21},d=0;8>d;d++){var e=parseInt(a.charAt(d),10);d%2===0&&(e=c[e+""]),b+=e}return b="ABCDEFGHIJKLMNOPQRSTUVWXYZ"[b%26],b+""===a.substr(8,1)},_cz:function(a){if(/^CZ[0-9]{8,10}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{8,10}$/.test(a))return!1;var b=0,c=0;if(8===a.length){if(a.charAt(0)+""=="9")return!1;for(b=0,c=0;7>c;c++)b+=parseInt(a.charAt(c),10)*(8-c);return b=11-b%11,10===b&&(b=0),11===b&&(b=1),b+""===a.substr(7,1)}if(9===a.length&&a.charAt(0)+""=="6"){for(b=0,c=0;7>c;c++)b+=parseInt(a.charAt(c+1),10)*(8-c);return b=11-b%11,10===b&&(b=0),11===b&&(b=1),b=[8,7,6,5,4,3,2,1,0,9,10][b-1],b+""===a.substr(8,1)}if(9===a.length||10===a.length){var d=1900+parseInt(a.substr(0,2),10),e=parseInt(a.substr(2,2),10)%50%20,f=parseInt(a.substr(4,2),10);if(9===a.length){if(d>=1980&&(d-=100),d>1953)return!1}else 1954>d&&(d+=100);if(!FormValidation.Helper.date(d,e,f))return!1;if(10===a.length){var g=parseInt(a.substr(0,9),10)%11;return 1985>d&&(g%=10),g+""===a.substr(9,1)}return!0}return!1},_de:function(a){return/^DE[0-9]{9}$/.test(a)&&(a=a.substr(2)),/^[0-9]{9}$/.test(a)?FormValidation.Helper.mod11And10(a):!1},_dk:function(a){if(/^DK[0-9]{8}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{8}$/.test(a))return!1;for(var b=0,c=[2,7,6,5,4,3,2,1],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%11===0},_ee:function(a){if(/^EE[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9}$/.test(a))return!1;for(var b=0,c=[3,7,1,3,7,1,3,7,1],d=0;9>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%10===0},_es:function(a){if(/^ES[0-9A-Z][0-9]{7}[0-9A-Z]$/.test(a)&&(a=a.substr(2)),!/^[0-9A-Z][0-9]{7}[0-9A-Z]$/.test(a))return!1;var b=function(a){var b=parseInt(a.substr(0,8),10);return b="TRWAGMYFPDXBNJZSQVHLCKE"[b%23],b+""===a.substr(8,1)},c=function(a){var b=["XYZ".indexOf(a.charAt(0)),a.substr(1)].join("");return b=parseInt(b,10),b="TRWAGMYFPDXBNJZSQVHLCKE"[b%23],b+""===a.substr(8,1)},d=function(a){var b,c=a.charAt(0);if(-1!=="KLM".indexOf(c))return b=parseInt(a.substr(1,8),10),b="TRWAGMYFPDXBNJZSQVHLCKE"[b%23],b+""===a.substr(8,1);if(-1!=="ABCDEFGHJNPQRSUVW".indexOf(c)){for(var d=0,e=[2,1,2,1,2,1,2],f=0,g=0;7>g;g++)f=parseInt(a.charAt(g+1),10)*e[g],f>9&&(f=Math.floor(f/10)+f%10),d+=f;return d=10-d%10,10===d&&(d=0),d+""===a.substr(8,1)||"JABCDEFGHI"[d]===a.substr(8,1)}return!1},e=a.charAt(0);return/^[0-9]$/.test(e)?{valid:b(a),type:"DNI"}:/^[XYZ]$/.test(e)?{valid:c(a),type:"NIE"}:{valid:d(a),type:"CIF"}},_fi:function(a){if(/^FI[0-9]{8}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{8}$/.test(a))return!1;for(var b=0,c=[7,9,10,5,8,4,2,1],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%11===0},_fr:function(a){if(/^FR[0-9A-Z]{2}[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0-9A-Z]{2}[0-9]{9}$/.test(a))return!1;if(!FormValidation.Helper.luhn(a.substr(2)))return!1;if(/^[0-9]{2}$/.test(a.substr(0,2)))return a.substr(0,2)===parseInt(a.substr(2)+"12",10)%97+"";var b,c="0123456789ABCDEFGHJKLMNPQRSTUVWXYZ";return b=/^[0-9]{1}$/.test(a.charAt(0))?24*c.indexOf(a.charAt(0))+c.indexOf(a.charAt(1))-10:34*c.indexOf(a.charAt(0))+c.indexOf(a.charAt(1))-100,(parseInt(a.substr(2),10)+1+Math.floor(b/11))%11===b%11},_gb:function(a){if((/^GB[0-9]{9}$/.test(a)||/^GB[0-9]{12}$/.test(a)||/^GBGD[0-9]{3}$/.test(a)||/^GBHA[0-9]{3}$/.test(a)||/^GB(GD|HA)8888[0-9]{5}$/.test(a))&&(a=a.substr(2)),!(/^[0-9]{9}$/.test(a)||/^[0-9]{12}$/.test(a)||/^GD[0-9]{3}$/.test(a)||/^HA[0-9]{3}$/.test(a)||/^(GD|HA)8888[0-9]{5}$/.test(a)))return!1;var b=a.length;if(5===b){var c=a.substr(0,2),d=parseInt(a.substr(2),10);return"GD"===c&&500>d||"HA"===c&&d>=500}if(11===b&&("GD8888"===a.substr(0,6)||"HA8888"===a.substr(0,6)))return"GD"===a.substr(0,2)&&parseInt(a.substr(6,3),10)>=500||"HA"===a.substr(0,2)&&parseInt(a.substr(6,3),10)<500?!1:parseInt(a.substr(6,3),10)%97===parseInt(a.substr(9,2),10);if(9===b||12===b){for(var e=0,f=[8,7,6,5,4,3,2,10,1],g=0;9>g;g++)e+=parseInt(a.charAt(g),10)*f[g];return e%=97,parseInt(a.substr(0,3),10)>=100?0===e||42===e||55===e:0===e}return!0},_gr:function(a){if(/^(GR|EL)[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9}$/.test(a))return!1;8===a.length&&(a="0"+a);for(var b=0,c=[256,128,64,32,16,8,4,2],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b=b%11%10,b+""===a.substr(8,1)},_el:function(a){return this._gr(a)},_hu:function(a){if(/^HU[0-9]{8}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{8}$/.test(a))return!1;for(var b=0,c=[9,7,3,1,9,7,3,1],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%10===0},_hr:function(a){return/^HR[0-9]{11}$/.test(a)&&(a=a.substr(2)),/^[0-9]{11}$/.test(a)?FormValidation.Helper.mod11And10(a):!1},_ie:function(a){if(/^IE[0-9]{1}[0-9A-Z\*\+]{1}[0-9]{5}[A-Z]{1,2}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{1}[0-9A-Z\*\+]{1}[0-9]{5}[A-Z]{1,2}$/.test(a))return!1;var b=function(a){for(;a.length<7;)a="0"+a;for(var b="WABCDEFGHIJKLMNOPQRSTUV",c=0,d=0;7>d;d++)c+=parseInt(a.charAt(d),10)*(8-d);return c+=9*b.indexOf(a.substr(7)),b[c%23]};return/^[0-9]+$/.test(a.substr(0,7))?a.charAt(7)===b(a.substr(0,7)+a.substr(8)+""):-1!=="ABCDEFGHIJKLMNOPQRSTUVWXYZ+*".indexOf(a.charAt(1))?a.charAt(7)===b(a.substr(2,5)+a.substr(0,1)+""):!0},_is:function(a){return/^IS[0-9]{5,6}$/.test(a)&&(a=a.substr(2)),/^[0-9]{5,6}$/.test(a)},_it:function(a){if(/^IT[0-9]{11}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{11}$/.test(a))return!1;if(0===parseInt(a.substr(0,7),10))return!1;var b=parseInt(a.substr(7,3),10);return 1>b||b>201&&999!==b&&888!==b?!1:FormValidation.Helper.luhn(a)},_lt:function(a){if(/^LT([0-9]{7}1[0-9]{1}|[0-9]{10}1[0-9]{1})$/.test(a)&&(a=a.substr(2)),!/^([0-9]{7}1[0-9]{1}|[0-9]{10}1[0-9]{1})$/.test(a))return!1;var b,c=a.length,d=0;for(b=0;c-1>b;b++)d+=parseInt(a.charAt(b),10)*(1+b%9);var e=d%11;if(10===e)for(d=0,b=0;c-1>b;b++)d+=parseInt(a.charAt(b),10)*(1+(b+2)%9);return e=e%11%10,e+""===a.charAt(c-1)},_lu:function(a){return/^LU[0-9]{8}$/.test(a)&&(a=a.substr(2)),/^[0-9]{8}$/.test(a)?parseInt(a.substr(0,6),10)%89+""===a.substr(6,2):!1},_lv:function(a){if(/^LV[0-9]{11}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{11}$/.test(a))return!1;var b,c=parseInt(a.charAt(0),10),d=0,e=[],f=a.length;if(c>3){for(d=0,e=[9,1,4,8,3,10,2,5,7,6,1],b=0;f>b;b++)d+=parseInt(a.charAt(b),10)*e[b];return d%=11,3===d}var g=parseInt(a.substr(0,2),10),h=parseInt(a.substr(2,2),10),i=parseInt(a.substr(4,2),10);if(i=i+1800+100*parseInt(a.charAt(6),10),!FormValidation.Helper.date(i,h,g))return!1;for(d=0,e=[10,5,8,4,2,1,6,3,7,9],b=0;f-1>b;b++)d+=parseInt(a.charAt(b),10)*e[b];return d=(d+1)%11%10,d+""===a.charAt(f-1)},_mt:function(a){if(/^MT[0-9]{8}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{8}$/.test(a))return!1;for(var b=0,c=[3,4,6,7,8,9,10,1],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%37===0},_nl:function(a){if(/^NL[0-9]{9}B[0-9]{2}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9}B[0-9]{2}$/.test(a))return!1;for(var b=0,c=[9,8,7,6,5,4,3,2],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%=11,b>9&&(b=0),b+""===a.substr(8,1)},_no:function(a){if(/^NO[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9}$/.test(a))return!1;for(var b=0,c=[3,2,7,6,5,4,3,2],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b=11-b%11,11===b&&(b=0),b+""===a.substr(8,1)},_pl:function(a){if(/^PL[0-9]{10}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{10}$/.test(a))return!1;for(var b=0,c=[6,5,7,2,3,4,5,6,7,-1],d=0;10>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b%11===0},_pt:function(a){if(/^PT[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9}$/.test(a))return!1;for(var b=0,c=[9,8,7,6,5,4,3,2],d=0;8>d;d++)b+=parseInt(a.charAt(d),10)*c[d];return b=11-b%11,b>9&&(b=0),b+""===a.substr(8,1)},_ro:function(a){if(/^RO[1-9][0-9]{1,9}$/.test(a)&&(a=a.substr(2)),!/^[1-9][0-9]{1,9}$/.test(a))return!1;for(var b=a.length,c=[7,5,3,2,1,7,5,3,2].slice(10-b),d=0,e=0;b-1>e;e++)d+=parseInt(a.charAt(e),10)*c[e];return d=10*d%11%10,d+""===a.substr(b-1,1)},_ru:function(a){if(/^RU([0-9]{10}|[0-9]{12})$/.test(a)&&(a=a.substr(2)),!/^([0-9]{10}|[0-9]{12})$/.test(a))return!1;var b=0;if(10===a.length){var c=0,d=[2,4,10,3,5,9,4,6,8,0];for(b=0;10>b;b++)c+=parseInt(a.charAt(b),10)*d[b];return c%=11,c>9&&(c%=10),c+""===a.substr(9,1)}if(12===a.length){var e=0,f=[7,2,4,10,3,5,9,4,6,8,0],g=0,h=[3,7,2,4,10,3,5,9,4,6,8,0];for(b=0;11>b;b++)e+=parseInt(a.charAt(b),10)*f[b],g+=parseInt(a.charAt(b),10)*h[b];return e%=11,e>9&&(e%=10),g%=11,g>9&&(g%=10),e+""===a.substr(10,1)&&g+""===a.substr(11,1)}return!1},_rs:function(a){if(/^RS[0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[0-9]{9}$/.test(a))return!1;for(var b=10,c=0,d=0;8>d;d++)c=(parseInt(a.charAt(d),10)+b)%10,0===c&&(c=10),b=2*c%11;return(b+parseInt(a.substr(8,1),10))%10===1},_se:function(a){return/^SE[0-9]{10}01$/.test(a)&&(a=a.substr(2)),/^[0-9]{10}01$/.test(a)?(a=a.substr(0,10),FormValidation.Helper.luhn(a)):!1},_si:function(a){var b=a.match(/^(SI)?([1-9][0-9]{7})$/);if(!b)return!1;b[1]&&(a=a.substr(2));for(var c=0,d=[8,7,6,5,4,3,2],e=0;7>e;e++)c+=parseInt(a.charAt(e),10)*d[e];return c=11-c%11,10===c&&(c=0),c+""===a.substr(7,1)},_sk:function(a){return/^SK[1-9][0-9][(2-4)|(6-9)][0-9]{7}$/.test(a)&&(a=a.substr(2)),/^[1-9][0-9][(2-4)|(6-9)][0-9]{7}$/.test(a)?parseInt(a,10)%11===0:!1},_ve:function(a){if(/^VE[VEJPG][0-9]{9}$/.test(a)&&(a=a.substr(2)),!/^[VEJPG][0-9]{9}$/.test(a))return!1;for(var b={V:4,E:8,J:12,P:16,G:20},c=b[a.charAt(0)],d=[3,2,7,6,5,4,3,2],e=0;8>e;e++)c+=parseInt(a.charAt(e+1),10)*d[e];return c=11-c%11,(11===c||10===c)&&(c=0),c+""===a.substr(9,1)},_za:function(a){return/^ZA4[0-9]{9}$/.test(a)&&(a=a.substr(2)),/^4[0-9]{9}$/.test(a)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{vin:{"default":"Please enter a valid VIN number"}}}),FormValidation.Validator.vin={validate:function(a,b,c,d){var e=a.getFieldValue(b,d);if(""===e)return!0;if(!/^[a-hj-npr-z0-9]{8}[0-9xX][a-hj-npr-z0-9]{8}$/i.test(e))return!1;e=e.toUpperCase();for(var f={A:1,B:2,C:3,D:4,E:5,F:6,G:7,H:8,J:1,K:2,L:3,M:4,N:5,P:7,R:9,S:2,T:3,U:4,V:5,W:6,X:7,Y:8,Z:9,1:1,2:2,3:3,4:4,5:5,6:6,7:7,8:8,9:9,0:0},g=[8,7,6,5,4,3,2,10,0,9,8,7,6,5,4,3,2],h=0,i=e.length,j=0;i>j;j++)h+=f[e.charAt(j)+""]*g[j];var k=h%11;return 10===k&&(k="X"),k+""===e.charAt(8)}}}(jQuery),function(a){FormValidation.I18n=a.extend(!0,FormValidation.I18n||{},{en_US:{zipCode:{"default":"Please enter a valid postal code",country:"Please enter a valid postal code in %s",countries:{AT:"Austria",BG:"Bulgaria",BR:"Brazil",CA:"Canada",CH:"Switzerland",CZ:"Czech Republic",DE:"Germany",DK:"Denmark",ES:"Spain",FR:"France",GB:"United Kingdom",IE:"Ireland",IN:"India",IT:"Italy",MA:"Morocco",NL:"Netherlands",PL:"Poland",PT:"Portugal",RO:"Romania",RU:"Russia",SE:"Sweden",SG:"Singapore",SK:"Slovakia",US:"USA"}}}}),FormValidation.Validator.zipCode={html5Attributes:{message:"message",country:"country"},COUNTRY_CODES:["AT","BG","BR","CA","CH","CZ","DE","DK","ES","FR","GB","IE","IN","IT","MA","NL","PL","PT","RO","RU","SE","SG","SK","US"],validate:function(b,c,d,e){var f=b.getFieldValue(c,e);if(""===f||!d.country)return!0;var g=b.getLocale(),h=d.country;if(("string"!=typeof h||-1===a.inArray(h,this.COUNTRY_CODES))&&(h=b.getDynamicOption(c,h)),!h||-1===a.inArray(h.toUpperCase(),this.COUNTRY_CODES))return!0;var i=!1;switch(h=h.toUpperCase()){case"AT":i=/^([1-9]{1})(\d{3})$/.test(f);break;case"BG":i=/^([1-9]{1}[0-9]{3})$/.test(a.trim(f));break;case"BR":i=/^(\d{2})([\.]?)(\d{3})([\-]?)(\d{3})$/.test(f);break;case"CA":i=/^(?:A|B|C|E|G|H|J|K|L|M|N|P|R|S|T|V|X|Y){1}[0-9]{1}(?:A|B|C|E|G|H|J|K|L|M|N|P|R|S|T|V|W|X|Y|Z){1}\s?[0-9]{1}(?:A|B|C|E|G|H|J|K|L|M|N|P|R|S|T|V|W|X|Y|Z){1}[0-9]{1}$/i.test(f);break;case"CH":i=/^([1-9]{1})(\d{3})$/.test(f);break;case"CZ":i=/^(\d{3})([ ]?)(\d{2})$/.test(f);break;case"DE":i=/^(?!01000|99999)(0[1-9]\d{3}|[1-9]\d{4})$/.test(f);break;case"DK":i=/^(DK(-|\s)?)?\d{4}$/i.test(f);break;case"ES":i=/^(?:0[1-9]|[1-4][0-9]|5[0-2])\d{3}$/.test(f);break;case"FR":i=/^[0-9]{5}$/i.test(f);break;case"GB":i=this._gb(f);break;case"IN":i=/^\d{3}\s?\d{3}$/.test(f);break;case"IE":i=/^(D6W|[ACDEFHKNPRTVWXY]\d{2})\s[0-9ACDEFHKNPRTVWXY]{4}$/.test(f);break;case"IT":i=/^(I-|IT-)?\d{5}$/i.test(f);break;case"MA":i=/^[1-9][0-9]{4}$/i.test(f);break;case"NL":i=/^[1-9][0-9]{3} ?(?!sa|sd|ss)[a-z]{2}$/i.test(f);break;case"PL":i=/^[0-9]{2}\-[0-9]{3}$/.test(f);break;case"PT":i=/^[1-9]\d{3}-\d{3}$/.test(f);break;case"RO":i=/^(0[1-8]{1}|[1-9]{1}[0-5]{1})?[0-9]{4}$/i.test(f);break;case"RU":i=/^[0-9]{6}$/i.test(f);break;case"SE":i=/^(S-)?\d{3}\s?\d{2}$/i.test(f);break;case"SG":i=/^([0][1-9]|[1-6][0-9]|[7]([0-3]|[5-9])|[8][0-2])(\d{4})$/i.test(f);break;case"SK":i=/^(\d{3})([ ]?)(\d{2})$/.test(f);break;case"US":default:i=/^\d{4,5}([\-]?\d{4})?$/.test(f)}return{valid:i,message:FormValidation.Helper.format(d.message||FormValidation.I18n[g].zipCode.country,FormValidation.I18n[g].zipCode.countries[h])}},_gb:function(a){for(var b="[ABCDEFGHIJKLMNOPRSTUWYZ]",c="[ABCDEFGHKLMNOPQRSTUVWXY]",d="[ABCDEFGHJKPMNRSTUVWXY]",e="[ABEHMNPRVWXY]",f="[ABDEFGHJLNPQRSTUWXYZ]",g=[new RegExp("^("+b+"{1}"+c+"?[0-9]{1,2})(\\s*)([0-9]{1}"+f+"{2})$","i"),new RegExp("^("+b+"{1}[0-9]{1}"+d+"{1})(\\s*)([0-9]{1}"+f+"{2})$","i"),new RegExp("^("+b+"{1}"+c+"{1}?[0-9]{1}"+e+"{1})(\\s*)([0-9]{1}"+f+"{2})$","i"),new RegExp("^(BF1)(\\s*)([0-6]{1}[ABDEFGHJLNPQRST]{1}[ABDEFGHJLNPQRSTUWZYZ]{1})$","i"),/^(GIR)(\s*)(0AA)$/i,/^(BFPO)(\s*)([0-9]{1,4})$/i,/^(BFPO)(\s*)(c\/o\s*[0-9]{1,3})$/i,/^([A-Z]{4})(\s*)(1ZZ)$/i,/^(AI-2640)$/i],h=0;h<g.length;h++)if(g[h].test(a))return!0;return!1}}}(jQuery);
!function(a){FormValidation.Framework.Bootstrap=function(b,c,d){c=a.extend(!0,{button:{selector:'[type="submit"]:not([formnovalidate])',disabled:"disabled"},err:{clazz:"help-block",parent:"^(.*)col-(xs|sm|md|lg)-(offset-){0,1}[0-9]+(.*)$"},icon:{valid:null,invalid:null,validating:null,feedback:"form-control-feedback"},row:{selector:".form-group",valid:"has-success",invalid:"has-error",feedback:"has-feedback"}},c),FormValidation.Base.apply(this,[b,c,d])},FormValidation.Framework.Bootstrap.prototype=a.extend({},FormValidation.Base.prototype,{_fixIcon:function(a,b){var c=this._namespace,d=a.attr("type"),e=a.attr("data-"+c+"-field"),f=this.options.fields[e].row||this.options.row.selector,g=a.closest(f);if("checkbox"===d||"radio"===d){var h=a.parent();h.hasClass(d)?b.insertAfter(h):h.parent().hasClass(d)&&b.insertAfter(h.parent())}0!==g.find(".input-group").length&&b.addClass("fv-bootstrap-icon-input-group").insertAfter(g.find(".input-group").eq(0))},_createTooltip:function(a,b,c){var d=this._namespace,e=a.data(d+".icon");if(e)switch(c){case"popover":e.css({cursor:"pointer","pointer-events":"auto"}).popover("destroy").popover({container:"body",content:b,html:!0,placement:"auto top",trigger:"hover click"});break;case"tooltip":default:e.css({cursor:"pointer","pointer-events":"auto"}).tooltip("destroy").tooltip({container:"body",html:!0,placement:"auto top",title:b})}},_destroyTooltip:function(a,b){var c=this._namespace,d=a.data(c+".icon");if(d)switch(b){case"popover":d.css({cursor:"","pointer-events":"none"}).popover("destroy");break;case"tooltip":default:d.css({cursor:"","pointer-events":"none"}).tooltip("destroy")}},_hideTooltip:function(a,b){var c=this._namespace,d=a.data(c+".icon");if(d)switch(b){case"popover":d.popover("hide");break;case"tooltip":default:d.tooltip("hide")}},_showTooltip:function(a,b){var c=this._namespace,d=a.data(c+".icon");if(d)switch(b){case"popover":d.popover("show");break;case"tooltip":default:d.tooltip("show")}}}),a.fn.bootstrapValidator=function(b){var c=arguments;return this.each(function(){var d=a(this),e=d.data("formValidation")||d.data("bootstrapValidator"),f="object"==typeof b&&b;e||(e=new FormValidation.Framework.Bootstrap(this,a.extend({},{events:{formInit:"init.form.bv",formPreValidate:"prevalidate.form.bv",formError:"error.form.bv",formSuccess:"success.form.bv",fieldAdded:"added.field.bv",fieldRemoved:"removed.field.bv",fieldInit:"init.field.bv",fieldError:"error.field.bv",fieldSuccess:"success.field.bv",fieldStatus:"status.field.bv",localeChanged:"changed.locale.bv",validatorError:"error.validator.bv",validatorSuccess:"success.validator.bv"}},f),"bv"),d.addClass("fv-form-bootstrap").data("formValidation",e).data("bootstrapValidator",e)),"string"==typeof b&&e[b].apply(e,Array.prototype.slice.call(c,1))})},a.fn.bootstrapValidator.Constructor=FormValidation.Framework.Bootstrap}(jQuery);

/* End */
;
; /* Start:"a:4:{s:4:"full";s:80:"/bitrix/templates/elasto_start/js/jquery.inputmask.bundle.min.js?146131939870473";s:6:"source";s:64:"/bitrix/templates/elasto_start/js/jquery.inputmask.bundle.min.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
/*!
* jquery.inputmask.bundle.js
* https://github.com/RobinHerbots/jquery.inputmask
* Copyright (c) 2010 - 2016 Robin Herbots
* Licensed under the MIT license (http://www.opensource.org/licenses/mit-license.php)
* Version: 3.3.1-9
*/
!function(a){function b(c,d){return this instanceof b?(a.isPlainObject(c)?d=c:(d=d||{},d.alias=c),this.el=void 0,this.opts=a.extend(!0,{},this.defaults,d),this.noMasksCache=d&&void 0!==d.definitions,this.userOptions=d||{},this.events={},void e(this.opts.alias,d,this.opts)):new b(c,d)}function c(a){var b=document.createElement("input"),c="on"+a,d=c in b;return d||(b.setAttribute(c,"return;"),d="function"==typeof b[c]),b=null,d}function d(b,c){var d=b.getAttribute("type"),e="INPUT"===b.tagName&&-1!==a.inArray(d,c.supportsInputType)||b.isContentEditable||"TEXTAREA"===b.tagName;if(!e&&"INPUT"===b.tagName){var f=document.createElement("input");f.setAttribute("type",d),e="text"===f.type,f=null}return e}function e(b,c,d){var f=d.aliases[b];return f?(f.alias&&e(f.alias,void 0,d),a.extend(!0,d,f),a.extend(!0,d,c),!0):(null===d.mask&&(d.mask=b),!1)}function f(b,c,d){function f(a,c){c=void 0!==c?c:b.getAttribute("data-inputmask-"+a),null!==c&&("string"==typeof c&&(0===a.indexOf("on")?c=window[c]:"false"===c?c=!1:"true"===c&&(c=!0)),d[a]=c)}var g,h,i,j,k=b.getAttribute("data-inputmask");if(k&&""!==k&&(k=k.replace(new RegExp("'","g"),'"'),h=JSON.parse("{"+k+"}")),h){i=void 0;for(j in h)if("alias"===j.toLowerCase()){i=h[j];break}}f("alias",i),d.alias&&e(d.alias,d,c);for(g in c){if(h){i=void 0;for(j in h)if(j.toLowerCase()===g.toLowerCase()){i=h[j];break}}f(g,i)}return a.extend(!0,c,d),c}function g(c,d){function e(b){function d(a,b,c,d){this.matches=[],this.isGroup=a||!1,this.isOptional=b||!1,this.isQuantifier=c||!1,this.isAlternator=d||!1,this.quantifier={min:1,max:1}}function e(b,d,e){var f=c.definitions[d];e=void 0!==e?e:b.matches.length;var g=b.matches[e-1];if(f&&!r){f.placeholder=a.isFunction(f.placeholder)?f.placeholder(c):f.placeholder;for(var h=f.prevalidator,i=h?h.length:0,j=1;j<f.cardinality;j++){var k=i>=j?h[j-1]:[],l=k.validator,m=k.cardinality;b.matches.splice(e++,0,{fn:l?"string"==typeof l?new RegExp(l):new function(){this.test=l}:new RegExp("."),cardinality:m?m:1,optionality:b.isOptional,newBlockMarker:void 0===g||g.def!==(f.definitionSymbol||d),casing:f.casing,def:f.definitionSymbol||d,placeholder:f.placeholder,mask:d}),g=b.matches[e-1]}b.matches.splice(e++,0,{fn:f.validator?"string"==typeof f.validator?new RegExp(f.validator):new function(){this.test=f.validator}:new RegExp("."),cardinality:f.cardinality,optionality:b.isOptional,newBlockMarker:void 0===g||g.def!==(f.definitionSymbol||d),casing:f.casing,def:f.definitionSymbol||d,placeholder:f.placeholder,mask:d})}else b.matches.splice(e++,0,{fn:null,cardinality:0,optionality:b.isOptional,newBlockMarker:void 0===g||g.def!==d,casing:null,def:c.staticDefinitionSymbol||d,placeholder:void 0!==c.staticDefinitionSymbol?d:void 0,mask:d}),r=!1}function f(a,b){a.isGroup&&(a.isGroup=!1,e(a,c.groupmarker.start,0),b!==!0&&e(a,c.groupmarker.end))}function g(a,b,c,d){b.matches.length>0&&(void 0===d||d)&&(c=b.matches[b.matches.length-1],f(c)),e(b,a)}function h(){if(t.length>0){if(m=t[t.length-1],g(k,m,o,!m.isAlternator),m.isAlternator){n=t.pop();for(var a=0;a<n.matches.length;a++)n.matches[a].isGroup=!1;t.length>0?(m=t[t.length-1],m.matches.push(n)):s.matches.push(n)}}else g(k,s,o)}function i(a){function b(a){return a===c.optionalmarker.start?a=c.optionalmarker.end:a===c.optionalmarker.end?a=c.optionalmarker.start:a===c.groupmarker.start?a=c.groupmarker.end:a===c.groupmarker.end&&(a=c.groupmarker.start),a}a.matches=a.matches.reverse();for(var d in a.matches){var e=parseInt(d);if(a.matches[d].isQuantifier&&a.matches[e+1]&&a.matches[e+1].isGroup){var f=a.matches[d];a.matches.splice(d,1),a.matches.splice(e+1,0,f)}void 0!==a.matches[d].matches?a.matches[d]=i(a.matches[d]):a.matches[d]=b(a.matches[d])}return a}for(var j,k,l,m,n,o,p,q=/(?:[?*+]|\{[0-9\+\*]+(?:,[0-9\+\*]*)?\})|[^.?*+^${[]()|\\]+|./g,r=!1,s=new d,t=[],u=[];j=q.exec(b);)if(k=j[0],r)h();else switch(k.charAt(0)){case c.escapeChar:r=!0;break;case c.optionalmarker.end:case c.groupmarker.end:if(l=t.pop(),void 0!==l)if(t.length>0){if(m=t[t.length-1],m.matches.push(l),m.isAlternator){n=t.pop();for(var v=0;v<n.matches.length;v++)n.matches[v].isGroup=!1;t.length>0?(m=t[t.length-1],m.matches.push(n)):s.matches.push(n)}}else s.matches.push(l);else h();break;case c.optionalmarker.start:t.push(new d(!1,!0));break;case c.groupmarker.start:t.push(new d(!0));break;case c.quantifiermarker.start:var w=new d(!1,!1,!0);k=k.replace(/[{}]/g,"");var x=k.split(","),y=isNaN(x[0])?x[0]:parseInt(x[0]),z=1===x.length?y:isNaN(x[1])?x[1]:parseInt(x[1]);if(("*"===z||"+"===z)&&(y="*"===z?0:1),w.quantifier={min:y,max:z},t.length>0){var A=t[t.length-1].matches;j=A.pop(),j.isGroup||(p=new d(!0),p.matches.push(j),j=p),A.push(j),A.push(w)}else j=s.matches.pop(),j.isGroup||(p=new d(!0),p.matches.push(j),j=p),s.matches.push(j),s.matches.push(w);break;case c.alternatormarker:t.length>0?(m=t[t.length-1],o=m.matches.pop()):o=s.matches.pop(),o.isAlternator?t.push(o):(n=new d(!1,!1,!1,!0),n.matches.push(o),t.push(n));break;default:h()}for(;t.length>0;)l=t.pop(),f(l,!0),s.matches.push(l);return s.matches.length>0&&(o=s.matches[s.matches.length-1],f(o),u.push(s)),c.numericInput&&i(u[0]),u}function f(f,g){if(null===f||""===f)return void 0;if(1===f.length&&c.greedy===!1&&0!==c.repeat&&(c.placeholder=""),c.repeat>0||"*"===c.repeat||"+"===c.repeat){var h="*"===c.repeat?0:"+"===c.repeat?1:c.repeat;f=c.groupmarker.start+f+c.groupmarker.end+c.quantifiermarker.start+h+","+c.repeat+c.quantifiermarker.end}var i;return void 0===b.prototype.masksCache[f]||d===!0?(i={mask:f,maskToken:e(f),validPositions:{},_buffer:void 0,buffer:void 0,tests:{},metadata:g},d!==!0&&(b.prototype.masksCache[c.numericInput?f.split("").reverse().join(""):f]=i,i=a.extend(!0,{},b.prototype.masksCache[c.numericInput?f.split("").reverse().join(""):f]))):i=a.extend(!0,{},b.prototype.masksCache[c.numericInput?f.split("").reverse().join(""):f]),i}function g(a){return a=a.toString()}var h;if(a.isFunction(c.mask)&&(c.mask=c.mask(c)),a.isArray(c.mask)){if(c.mask.length>1){c.keepStatic=null===c.keepStatic?!0:c.keepStatic;var i="(";return a.each(c.numericInput?c.mask.reverse():c.mask,function(b,c){i.length>1&&(i+=")|("),i+=g(void 0===c.mask||a.isFunction(c.mask)?c:c.mask)}),i+=")",f(i,c.mask)}c.mask=c.mask.pop()}return c.mask&&(h=void 0===c.mask.mask||a.isFunction(c.mask.mask)?f(g(c.mask),c.mask):f(g(c.mask.mask),c.mask)),h}function h(e,f,g){function i(a,b,c){b=b||0;var d,e,f,h=[],i=0,j=o();do{if(a===!0&&m().validPositions[i]){var k=m().validPositions[i];e=k.match,d=k.locator.slice(),h.push(c===!0?k.input:I(i,e))}else f=r(i,d,i-1),e=f.match,d=f.locator.slice(),(g.jitMasking===!1||j>i||isFinite(g.jitMasking)&&g.jitMasking>i)&&h.push(I(i,e));i++}while((void 0===ha||ha>i-1)&&null!==e.fn||null===e.fn&&""!==e.def||b>=i);return""===h[h.length-1]&&h.pop(),h}function m(){return f}function n(a){var b=m();b.buffer=void 0,a!==!0&&(b.tests={},b._buffer=void 0,b.validPositions={},b.p=0)}function o(a,b){var c=-1,d=-1,e=m().validPositions;void 0===a&&(a=-1);for(var f in e){var g=parseInt(f);e[g]&&(b||null!==e[g].match.fn)&&(a>=g&&(c=g),g>=a&&(d=g))}return-1!==c&&a-c>1||a>d?c:d}function p(b,c,d,e){if(e||g.insertMode&&void 0!==m().validPositions[b]&&void 0===d){var f,h=a.extend(!0,{},m().validPositions),i=o();for(f=b;i>=f;f++)delete m().validPositions[f];m().validPositions[b]=c;var j,k=!0,l=m().validPositions,p=!1;for(f=j=b;i>=f;f++){var q=h[f];if(void 0!==q)for(var r=j,s=-1;r<D()&&(null==q.match.fn&&l[f]&&(l[f].match.optionalQuantifier===!0||l[f].match.optionality===!0)||null!=q.match.fn);){if(null===q.match.fn||!g.keepStatic&&l[f]&&(void 0!==l[f+1]&&v(f+1,l[f].locator.slice(),f).length>1||void 0!==l[f].alternation)?r++:r=E(j),p===!1&&h[r]&&h[r].match.def===q.match.def){m().validPositions[r]=a.extend(!0,{},h[r]),m().validPositions[r].input=q.input,j=r,k=!0;break}if(t(r,q.match.def)){var u=B(r,q.input,!0,!0);if(k=u!==!1,j=u.caret||u.insert?o():r,p=!0,k)break}else{if(k=null==q.match.fn,s===r)break;s=r}}if(!k)break}if(!k)return m().validPositions=a.extend(!0,{},h),n(!0),!1}else m().validPositions[b]=c;return n(!0),!0}function q(b,c,d,e){function f(a){var b=m().validPositions[a];if(void 0!==b&&null===b.match.fn){var c=m().validPositions[a-1],d=m().validPositions[a+1];return void 0!==c&&void 0!==d}return!1}var h,i=b,j=a.extend(!0,{},m().validPositions),k=!1;for(m().p=b,h=c-1;h>=i;h--)void 0!==m().validPositions[h]&&(d===!0||!f(h)&&g.canClearPosition(m(),h,o(),e,g)!==!1)&&delete m().validPositions[h];for(n(!0),h=i+1;h<=o();){for(;void 0!==m().validPositions[i];)i++;var l=m().validPositions[i];if(i>h&&(h=i+1),void 0===m().validPositions[h]&&C(h)||void 0!==l)h++;else{var p=r(h);k===!1&&j[i]&&j[i].match.def===p.match.def?(m().validPositions[i]=a.extend(!0,{},j[i]),m().validPositions[i].input=p.input,delete m().validPositions[h],h++):t(i,p.match.def)?B(i,p.input||I(h),!0)!==!1&&(delete m().validPositions[h],h++,k=!0):C(h)||(h++,i--),i++}}n(!0)}function r(a,b,c){var d=m().validPositions[a];if(void 0===d)for(var e=v(a,b,c),f=o(),h=m().validPositions[f]||v(0)[0],i=void 0!==h.alternation?h.locator[h.alternation].toString().split(","):[],j=0;j<e.length&&(d=e[j],!(d.match&&(g.greedy&&d.match.optionalQuantifier!==!0||(d.match.optionality===!1||d.match.newBlockMarker===!1)&&d.match.optionalQuantifier!==!0)&&(void 0===h.alternation||h.alternation!==d.alternation||void 0!==d.locator[h.alternation]&&A(d.locator[h.alternation].toString().split(","),i))));j++);return d}function s(a){return m().validPositions[a]?m().validPositions[a].match:v(a)[0].match}function t(a,b){for(var c=!1,d=v(a),e=0;e<d.length;e++)if(d[e].match&&d[e].match.def===b){c=!0;break}return c}function u(b,c){var d,e;return(m().tests[b]||m().validPositions[b])&&a.each(m().tests[b]||[m().validPositions[b]],function(a,b){var f=b.alternation?b.locator[b.alternation].toString().indexOf(c):-1;(void 0===e||e>f)&&-1!==f&&(d=b,e=f)}),d}function v(b,c,d){function e(c,d,f,h){function j(f,h,o){function p(b,c){var d=0===a.inArray(b,c.matches);return d||a.each(c.matches,function(a,e){return e.isQuantifier===!0&&(d=p(b,c.matches[a-1]))?!1:void 0}),d}function q(a,b){var c=u(a,b);return c?c.locator.slice(c.alternation+1):[]}if(i>1e4)throw"Inputmask: There is probably an error in your mask definition or in the code. Create an issue on github with an example of the mask you are using. "+m().mask;if(i===b&&void 0===f.matches)return k.push({match:f,locator:h.reverse(),cd:n}),!0;if(void 0!==f.matches){if(f.isGroup&&o!==f){if(f=j(c.matches[a.inArray(f,c.matches)+1],h))return!0}else if(f.isOptional){var r=f;if(f=e(f,d,h,o)){if(g=k[k.length-1].match,!p(g,r))return!0;l=!0,i=b}}else if(f.isAlternator){var s,t=f,v=[],w=k.slice(),x=h.length,y=d.length>0?d.shift():-1;if(-1===y||"string"==typeof y){var z,A=i,B=d.slice(),C=[];if("string"==typeof y)C=y.split(",");else for(z=0;z<t.matches.length;z++)C.push(z);for(var D=0;D<C.length;D++){if(z=parseInt(C[D]),k=[],d=q(i,z),f=j(t.matches[z]||c.matches[z],[z].concat(h),o)||f,f!==!0&&void 0!==f&&C[C.length-1]<t.matches.length){var E=a.inArray(f,c.matches)+1;c.matches.length>E&&(f=j(c.matches[E],[E].concat(h.slice(1,h.length)),o),f&&(C.push(E.toString()),a.each(k,function(a,b){b.alternation=h.length-1})))}s=k.slice(),i=A,k=[];for(var F=0;F<B.length;F++)d[F]=B[F];for(var G=0;G<s.length;G++){var H=s[G];H.alternation=H.alternation||x;for(var I=0;I<v.length;I++){var J=v[I];if(H.match.def===J.match.def&&("string"!=typeof y||-1!==a.inArray(H.locator[H.alternation].toString(),C))){H.match.mask===J.match.mask&&(s.splice(G,1),G--),-1===J.locator[H.alternation].toString().indexOf(H.locator[H.alternation])&&(J.locator[H.alternation]=J.locator[H.alternation]+","+H.locator[H.alternation],J.alternation=H.alternation);break}}}v=v.concat(s)}"string"==typeof y&&(v=a.map(v,function(b,c){if(isFinite(c)){var d,e=b.alternation,f=b.locator[e].toString().split(",");b.locator[e]=void 0,b.alternation=void 0;for(var g=0;g<f.length;g++)d=-1!==a.inArray(f[g],C),d&&(void 0!==b.locator[e]?(b.locator[e]+=",",b.locator[e]+=f[g]):b.locator[e]=parseInt(f[g]),b.alternation=e);if(void 0!==b.locator[e])return b}})),k=w.concat(v),i=b,l=k.length>0}else f=j(t.matches[y]||c.matches[y],[y].concat(h),o);if(f)return!0}else if(f.isQuantifier&&o!==c.matches[a.inArray(f,c.matches)-1])for(var K=f,L=d.length>0?d.shift():0;L<(isNaN(K.quantifier.max)?L+1:K.quantifier.max)&&b>=i;L++){var M=c.matches[a.inArray(K,c.matches)-1];if(f=j(M,[L].concat(h),M)){if(g=k[k.length-1].match,g.optionalQuantifier=L>K.quantifier.min-1,p(g,M)){if(L>K.quantifier.min-1){l=!0,i=b;break}return!0}return!0}}else if(f=e(f,d,h,o))return!0}else i++}for(var o=d.length>0?d.shift():0;o<c.matches.length;o++)if(c.matches[o].isQuantifier!==!0){var p=j(c.matches[o],[o].concat(f),h);if(p&&i===b)return p;if(i>b)break}}function f(a){var b=a[0]||a;return b.locator.slice()}var g,h=m().maskToken,i=c?d:0,j=c||[0],k=[],l=!1,n=c?c.join(""):"";if(b>-1){if(void 0===c){for(var o,p=b-1;void 0===(o=m().validPositions[p]||m().tests[p])&&p>-1;)p--;void 0!==o&&p>-1&&(j=f(o),n=j.join(""),o=o[0]||o,i=p)}if(m().tests[b]&&m().tests[b][0].cd===n)return m().tests[b];for(var q=j.shift();q<h.length;q++){var r=e(h[q],j,[q]);if(r&&i===b||i>b)break}}return(0===k.length||l)&&k.push({match:{fn:null,cardinality:0,optionality:!0,casing:null,def:""},locator:[]}),m().tests[b]=a.extend(!0,[],k),m().tests[b]}function w(){return void 0===m()._buffer&&(m()._buffer=i(!1,1)),m()._buffer}function x(a){if(void 0===m().buffer||a===!0){if(a===!0)for(var b in m().tests)void 0===m().validPositions[b]&&delete m().tests[b];m().buffer=i(!0,o(),!0)}return m().buffer}function y(a,b,c){var d;if(c=c,a===!0)n(),a=0,b=c.length;else for(d=a;b>d;d++)delete m().validPositions[d],delete m().tests[d];for(d=a;b>d;d++)n(!0),c[d]!==g.skipOptionalPartCharacter&&B(d,c[d],!0,!0)}function z(a,b){switch(b.casing){case"upper":a=a.toUpperCase();break;case"lower":a=a.toLowerCase()}return a}function A(b,c){for(var d=g.greedy?c:c.slice(0,1),e=!1,f=0;f<b.length;f++)if(-1!==a.inArray(b[f],d)){e=!0;break}return e}function B(c,d,e,f){function h(a){return ja?a.begin-a.end>1||a.begin-a.end===1&&g.insertMode:a.end-a.begin>1||a.end-a.begin===1&&g.insertMode}function i(b,d,e,f){var i=!1;return a.each(v(b),function(j,k){for(var l=k.match,r=d?1:0,s="",t=l.cardinality;t>r;t--)s+=G(b-(t-1));if(d&&(s+=d),x(!0),i=null!=l.fn?l.fn.test(s,m(),b,e,g,h(c)):d!==l.def&&d!==g.skipOptionalPartCharacter||""===l.def?!1:{c:l.placeholder||l.def,pos:b},i!==!1){var u=void 0!==i.c?i.c:d;u=u===g.skipOptionalPartCharacter&&null===l.fn?l.placeholder||l.def:u;var v=b,w=x();if(void 0!==i.remove&&(a.isArray(i.remove)||(i.remove=[i.remove]),a.each(i.remove.sort(function(a,b){return b-a}),function(a,b){q(b,b+1,!0)})),void 0!==i.insert&&(a.isArray(i.insert)||(i.insert=[i.insert]),a.each(i.insert.sort(function(a,b){return a-b}),function(a,b){B(b.pos,b.c,!1,f)})),i.refreshFromBuffer){var A=i.refreshFromBuffer;if(e=!0,y(A===!0?A:A.start,A.end,w),void 0===i.pos&&void 0===i.c)return i.pos=o(),!1;if(v=void 0!==i.pos?i.pos:b,v!==b)return i=a.extend(i,B(v,u,!0,f)),!1}else if(i!==!0&&void 0!==i.pos&&i.pos!==b&&(v=i.pos,y(b,v,x().slice()),v!==b))return i=a.extend(i,B(v,u,!0)),!1;return i!==!0&&void 0===i.pos&&void 0===i.c?!1:(j>0&&n(!0),p(v,a.extend({},k,{input:z(u,l)}),f,h(c))||(i=!1),!1)}}),i}function j(b,c,d,e){for(var f,h,i,j,k,l,p=a.extend(!0,{},m().validPositions),q=a.extend(!0,{},m().tests),s=o();s>=0&&(j=m().validPositions[s],!j||void 0===j.alternation||(f=s,h=m().validPositions[f].alternation,r(f).locator[j.alternation]===j.locator[j.alternation]));s--);if(void 0!==h){f=parseInt(f);for(var t in m().validPositions)if(t=parseInt(t),j=m().validPositions[t],t>=f&&void 0!==j.alternation){var v;0===f?(v=[],a.each(m().tests[f],function(a,b){void 0!==b.locator[h]&&(v=v.concat(b.locator[h].toString().split(",")))})):v=m().validPositions[f].locator[h].toString().split(",");var w=void 0!==j.locator[h]?j.locator[h]:v[0];w.length>0&&(w=w.split(",")[0]);for(var x=0;x<v.length;x++){var y=[],z=0,A=0;if(w<v[x]){for(var C,D,E=t;E>=0;E--)if(C=m().validPositions[E],void 0!==C){var F=u(E,v[x]);m().validPositions[E].match.def!==F.match.def&&(y.push(m().validPositions[E].input),m().validPositions[E]=F,m().validPositions[E].input=I(E),null===m().validPositions[E].match.fn&&A++,C=F),D=C.locator[h],C.locator[h]=parseInt(v[x]);break}if(w!==C.locator[h]){for(k=t+1;k<o(void 0,!0)+1;k++)l=m().validPositions[k],l&&null!=l.match.fn?y.push(l.input):b>k&&z++,delete m().validPositions[k],delete m().tests[k];for(n(!0),g.keepStatic=!g.keepStatic,i=!0;y.length>0;){var G=y.shift();if(G!==g.skipOptionalPartCharacter&&!(i=B(o(void 0,!0)+1,G,!1,e)))break}if(C.alternation=h,C.locator[h]=D,i){var H=o(b)+1;for(k=t+1;k<o()+1;k++)l=m().validPositions[k],(void 0===l||null==l.match.fn)&&b>k&&A++;b+=A-z,i=B(b>H?H:b,c,d,e)}if(g.keepStatic=!g.keepStatic,i)return i;n(),m().validPositions=a.extend(!0,{},p),m().tests=a.extend(!0,{},q)}}}break}}return!1}function k(b,c){for(var d=m().validPositions[c],e=d.locator,f=e.length,g=b;c>g;g++)if(void 0===m().validPositions[g]&&!C(g,!0)){var h=v(g),i=h[0],j=-1;a.each(h,function(a,b){for(var c=0;f>c&&(void 0!==b.locator[c]&&A(b.locator[c].toString().split(","),e[c].toString().split(",")));c++)c>j&&(j=c,i=b)}),p(g,a.extend({},i,{input:i.match.placeholder||i.match.def}),!0)}}e=e===!0;var l=c;void 0!==c.begin&&(l=ja&&!h(c)?c.end:c.begin);for(var s=!1,t=a.extend(!0,{},m().validPositions),w=l-1;w>-1&&!m().validPositions[w];w--);var F;for(w++;l>w;w++)void 0===m().validPositions[w]&&(g.jitMasking===!1||g.jitMasking>w)&&((F=r(w)).match.def===g.radixPointDefinitionSymbol||!C(w,!0)||a.inArray(g.radixPoint,x())<w&&F.match.fn&&F.match.fn.test(I(w),m(),w,!1,g))&&i(o(w,!0)+1,F.match.placeholder||(null==F.match.fn?F.match.def:""!==I(w)?I(w):x()[w]),!0,f);if(h(c)&&(Q(void 0,b.keyCode.DELETE,c),l=m().p),l<D()&&(s=i(l,d,e,f),(!e||f===!0)&&s===!1)){var H=m().validPositions[l];if(!H||null!==H.match.fn||H.match.def!==d&&d!==g.skipOptionalPartCharacter){if((g.insertMode||void 0===m().validPositions[E(l)])&&!C(l,!0)){var J=r(l).match;J=J.placeholder||J.def,i(l,J,e,f);for(var K=l+1,L=E(l);L>=K;K++)if(s=i(K,d,e,f),s!==!1){k(l,K),l=K;break}}}else s={caret:E(l)}}return s===!1&&g.keepStatic&&(s=j(l,d,e,f)),s===!0&&(s={pos:l}),a.isFunction(g.postValidation)&&s!==!1&&!e&&f!==!0&&(s=g.postValidation(x(!0),s,g)?s:!1),void 0===s.pos&&(s.pos=l),s===!1&&(n(!0),m().validPositions=a.extend(!0,{},t)),s}function C(a,b){var c;if(b?(c=r(a).match,""===c.def&&(c=s(a))):c=s(a),null!=c.fn)return c.fn;if(b!==!0&&a>-1&&!g.keepStatic&&void 0===m().validPositions[a]){var d=v(a);return d.length>2}return!1}function D(){var a;ha=void 0!==fa?fa.maxLength:void 0,-1===ha&&(ha=void 0);var b,c=o(),d=m().validPositions[c],e=void 0!==d?d.locator.slice():void 0;for(b=c+1;void 0===d||null!==d.match.fn||null===d.match.fn&&""!==d.match.def;b++)d=r(b,e,b-1),e=d.locator.slice();var f=s(b-1);return a=""!==f.def?b:b-1,void 0===ha||ha>a?a:ha}function E(a,b){var c=D();if(a>=c)return c;for(var d=a;++d<c&&(b===!0&&(s(d).newBlockMarker!==!0||!C(d))||b!==!0&&!C(d)&&(g.nojumps!==!0||g.nojumpsThreshold>d)););return d}function F(a,b){var c=a;if(0>=c)return 0;for(;--c>0&&(b===!0&&s(c).newBlockMarker!==!0||b!==!0&&!C(c)););return c}function G(a){return void 0===m().validPositions[a]?I(a):m().validPositions[a].input}function H(b,c,d,e,f){if(e&&a.isFunction(g.onBeforeWrite)){var h=g.onBeforeWrite(e,c,d,g);if(h){if(h.refreshFromBuffer){var i=h.refreshFromBuffer;y(i===!0?i:i.start,i.end,h.buffer||c),c=x(!0)}void 0!==d&&(d=void 0!==h.caret?h.caret:d)}}b.inputmask._valueSet(c.join("")),void 0===d||void 0!==e&&"blur"===e.type||L(b,d),f===!0&&(la=!0,a(b).trigger("input"))}function I(a,b){if(b=b||s(a),void 0!==b.placeholder)return b.placeholder;if(null===b.fn){if(a>-1&&!g.keepStatic&&void 0===m().validPositions[a]){var c,d=v(a),e=0;if(d.length>2)for(var f=0;f<d.length;f++)if(d[f].match.optionality!==!0&&d[f].match.optionalQuantifier!==!0&&(null===d[f].match.fn||void 0===c||d[f].match.fn.test(c.match.def,m(),a,!0,g)!==!1)&&(e++,null===d[f].match.fn&&(c=d[f]),e>1))return g.placeholder.charAt(a%g.placeholder.length)}return b.def}return g.placeholder.charAt(a%g.placeholder.length)}function J(c,d,e,f){function h(){var a=!1,b=w().slice(l,E(l)).join("").indexOf(k);if(-1!==b&&!C(l)){a=!0;for(var c=w().slice(l,l+b),d=0;d<c.length;d++)if(" "!==c[d]){a=!1;break}}return a}var i,j=f.slice(),k="",l=0;if(n(),m().p=E(-1),!e)if(g.autoUnmask!==!0){var p=w().slice(0,E(-1)).join(""),q=j.join("").match(new RegExp("^"+b.escapeRegex(p),"g"));q&&q.length>0&&(j.splice(0,q.length*p.length),l=E(l))}else l=E(l);a.each(j,function(b,d){if(void 0!==d){var f=new a.Event("keypress");f.which=d.charCodeAt(0),k+=d;var j=o(void 0,!0),p=m().validPositions[j],q=r(j+1,p?p.locator.slice():void 0,j);if(!h()||e||g.autoUnmask){var s=e?b:null==q.match.fn&&q.match.optionality&&j+1<m().p?j+1:m().p;i=S.call(c,f,!0,!1,e,s),l=s+1,k=""}else i=S.call(c,f,!0,!1,!0,j+1);if(!e&&a.isFunction(g.onBeforeWrite)&&(i=g.onBeforeWrite(f,x(),i.forwardPosition,g),i&&i.refreshFromBuffer)){var t=i.refreshFromBuffer;y(t===!0?t:t.start,t.end,i.buffer),n(!0),i.caret&&(m().p=i.caret)}}}),d&&H(c,x(),document.activeElement===c?E(o(0)):void 0,new a.Event("checkval"))}function K(b){if(b&&void 0===b.inputmask)return b.value;var c=[],d=m().validPositions;for(var e in d)d[e].match&&null!=d[e].match.fn&&c.push(d[e].input);var f=0===c.length?null:(ja?c.reverse():c).join("");if(null!==f){var h=(ja?x().slice().reverse():x()).join("");a.isFunction(g.onUnMask)&&(f=g.onUnMask(h,f,g)||f)}return f}function L(a,b,c,d){function e(a){if(d!==!0&&ja&&"number"==typeof a&&(!g.greedy||""!==g.placeholder)){var b=x().join("").length;a=b-a}return a}var f;if("number"!=typeof b)return a.setSelectionRange?(b=a.selectionStart,c=a.selectionEnd):window.getSelection?(f=window.getSelection().getRangeAt(0),(f.commonAncestorContainer.parentNode===a||f.commonAncestorContainer===a)&&(b=f.startOffset,c=f.endOffset)):document.selection&&document.selection.createRange&&(f=document.selection.createRange(),b=0-f.duplicate().moveStart("character",-a.inputmask._valueGet().length),c=b+f.text.length),{begin:e(b),end:e(c)};b=e(b),c=e(c),c="number"==typeof c?c:b;var h=parseInt(((a.ownerDocument.defaultView||window).getComputedStyle?(a.ownerDocument.defaultView||window).getComputedStyle(a,null):a.currentStyle).fontSize)*c;if(a.scrollLeft=h>a.scrollWidth?h:0,j||g.insertMode!==!1||b!==c||c++,a.setSelectionRange)a.selectionStart=b,a.selectionEnd=c;else if(window.getSelection){if(f=document.createRange(),void 0===a.firstChild||null===a.firstChild){var i=document.createTextNode("");a.appendChild(i)}f.setStart(a.firstChild,b<a.inputmask._valueGet().length?b:a.inputmask._valueGet().length),f.setEnd(a.firstChild,c<a.inputmask._valueGet().length?c:a.inputmask._valueGet().length),f.collapse(!0);var k=window.getSelection();k.removeAllRanges(),k.addRange(f)}else a.createTextRange&&(f=a.createTextRange(),f.collapse(!0),f.moveEnd("character",c),f.moveStart("character",b),f.select())}function M(b){var c,d,e=x(),f=e.length,g=o(),h={},i=m().validPositions[g],j=void 0!==i?i.locator.slice():void 0;for(c=g+1;c<e.length;c++)d=r(c,j,c-1),j=d.locator.slice(),h[c]=a.extend(!0,{},d);var k=i&&void 0!==i.alternation?i.locator[i.alternation]:void 0;for(c=f-1;c>g&&(d=h[c],(d.match.optionality||d.match.optionalQuantifier||k&&(k!==h[c].locator[i.alternation]&&null!=d.match.fn||null===d.match.fn&&d.locator[i.alternation]&&A(d.locator[i.alternation].toString().split(","),k.toString().split(","))&&""!==v(c)[0].def))&&e[c]===I(c,d.match));c--)f--;return b?{l:f,def:h[f]?h[f].match:void 0}:f}function N(a){for(var b=M(),c=a.length-1;c>b&&!C(c);c--);return a.splice(b,c+1-b),a}function O(b){if(a.isFunction(g.isComplete))return g.isComplete(b,g);if("*"===g.repeat)return void 0;var c=!1,d=M(!0),e=F(d.l);if(void 0===d.def||d.def.newBlockMarker||d.def.optionality||d.def.optionalQuantifier){c=!0;for(var f=0;e>=f;f++){var h=r(f).match;if(null!==h.fn&&void 0===m().validPositions[f]&&h.optionality!==!0&&h.optionalQuantifier!==!0||null===h.fn&&b[f]!==I(f,h)){c=!1;break}}}return c}function P(b){function c(b){if(a.valHooks&&(void 0===a.valHooks[b]||a.valHooks[b].inputmaskpatch!==!0)){var c=a.valHooks[b]&&a.valHooks[b].get?a.valHooks[b].get:function(a){return a.value},d=a.valHooks[b]&&a.valHooks[b].set?a.valHooks[b].set:function(a,b){return a.value=b,a};a.valHooks[b]={get:function(a){if(a.inputmask){if(a.inputmask.opts.autoUnmask)return a.inputmask.unmaskedvalue();var b=c(a);return-1!==o()||g.nullable!==!0?b:""}return c(a)},set:function(b,c){var e,f=a(b);return e=d(b,c),b.inputmask&&f.trigger("setvalue"),e},inputmaskpatch:!0}}}function d(){return this.inputmask?this.inputmask.opts.autoUnmask?this.inputmask.unmaskedvalue():-1!==o()||g.nullable!==!0?document.activeElement===this&&g.clearMaskOnLostFocus?(ja?N(x().slice()).reverse():N(x().slice())).join(""):h.call(this):"":h.call(this)}function e(b){i.call(this,b),this.inputmask&&a(this).trigger("setvalue")}function f(b){oa.on(b,"mouseenter",function(b){var c=a(this),d=this,e=d.inputmask._valueGet();e!==x().join("")&&c.trigger("setvalue")})}var h,i;if(!b.inputmask.__valueGet){if(Object.getOwnPropertyDescriptor){var j=Object.getPrototypeOf?Object.getOwnPropertyDescriptor(Object.getPrototypeOf(b),"value"):void 0;j&&j.get&&j.set?(h=j.get,i=j.set,Object.defineProperty(b,"value",{get:d,set:e,configurable:!0})):"INPUT"!==b.tagName&&(h=function(){return this.textContent},i=function(a){this.textContent=a},Object.defineProperty(b,"value",{get:d,set:e,configurable:!0}))}else document.__lookupGetter__&&b.__lookupGetter__("value")&&(h=b.__lookupGetter__("value"),i=b.__lookupSetter__("value"),b.__defineGetter__("value",d),b.__defineSetter__("value",e));b.inputmask.__valueGet=h,b.inputmask._valueGet=function(a){return ja&&a!==!0?h.call(this.el).split("").reverse().join(""):h.call(this.el)},b.inputmask.__valueSet=i,b.inputmask._valueSet=function(a,b){i.call(this.el,null===a||void 0===a?"":b!==!0&&ja?a.split("").reverse().join(""):a)},void 0===h&&(h=function(){return this.value},i=function(a){this.value=a},c(b.type),f(b))}}function Q(c,d,e,f){function h(){if(g.keepStatic){n(!0);var b,d=[],e=a.extend(!0,{},m().validPositions);for(b=o();b>=0;b--){var f=m().validPositions[b];if(f&&(null!=f.match.fn&&d.push(f.input),delete m().validPositions[b],void 0!==f.alternation&&f.locator[f.alternation]===r(b).locator[f.alternation]))break}if(b>-1)for(;d.length>0;){m().p=E(o());var h=new a.Event("keypress");h.which=d.pop().charCodeAt(0),S.call(c,h,!0,!1,!1,m().p)}else m().validPositions=a.extend(!0,{},e)}}if((g.numericInput||ja)&&(d===b.keyCode.BACKSPACE?d=b.keyCode.DELETE:d===b.keyCode.DELETE&&(d=b.keyCode.BACKSPACE),ja)){var i=e.end;e.end=e.begin,e.begin=i}d===b.keyCode.BACKSPACE&&(e.end-e.begin<1||g.insertMode===!1)?(e.begin=F(e.begin),void 0===m().validPositions[e.begin]||m().validPositions[e.begin].input!==g.groupSeparator&&m().validPositions[e.begin].input!==g.radixPoint||e.begin--):d===b.keyCode.DELETE&&e.begin===e.end&&(e.end=C(e.end)?e.end+1:E(e.end)+1,void 0===m().validPositions[e.begin]||m().validPositions[e.begin].input!==g.groupSeparator&&m().validPositions[e.begin].input!==g.radixPoint||e.end++),q(e.begin,e.end,!1,f),f!==!0&&h();var j=o(e.begin);j<e.begin?(-1===j&&n(),m().p=E(j)):f!==!0&&(m().p=e.begin)}function R(d){var e=this,f=a(e),h=d.keyCode,i=L(e);if(h===b.keyCode.BACKSPACE||h===b.keyCode.DELETE||l&&h===b.keyCode.BACKSPACE_SAFARI||d.ctrlKey&&h===b.keyCode.X&&!c("cut"))d.preventDefault(),Q(e,h,i),H(e,x(),m().p,d,ea!==x().join("")),e.inputmask._valueGet()===w().join("")?f.trigger("cleared"):O(x())===!0&&f.trigger("complete"),g.showTooltip&&(e.title=g.tooltip||m().mask);else if(h===b.keyCode.END||h===b.keyCode.PAGE_DOWN){d.preventDefault();var j=E(o());g.insertMode||j!==D()||d.shiftKey||j--,L(e,d.shiftKey?i.begin:j,j,!0)}else h===b.keyCode.HOME&&!d.shiftKey||h===b.keyCode.PAGE_UP?(d.preventDefault(),L(e,0,d.shiftKey?i.begin:0,!0)):(g.undoOnEscape&&h===b.keyCode.ESCAPE||90===h&&d.ctrlKey)&&d.altKey!==!0?(J(e,!0,!1,ea.split("")),f.trigger("click")):h!==b.keyCode.INSERT||d.shiftKey||d.ctrlKey?g.tabThrough===!0&&h===b.keyCode.TAB?(d.shiftKey===!0?(null===s(i.begin).fn&&(i.begin=E(i.begin)),i.end=F(i.begin,!0),i.begin=F(i.end,!0)):(i.begin=E(i.begin,!0),i.end=E(i.begin,!0),i.end<D()&&i.end--),i.begin<D()&&(d.preventDefault(),L(e,i.begin,i.end))):g.insertMode!==!1||d.shiftKey||(h===b.keyCode.RIGHT?setTimeout(function(){var a=L(e);L(e,a.begin)},0):h===b.keyCode.LEFT&&setTimeout(function(){var a=L(e);L(e,ja?a.begin+1:a.begin-1)},0)):(g.insertMode=!g.insertMode,L(e,g.insertMode||i.begin!==D()?i.begin:i.begin-1));g.onKeyDown.call(this,d,x(),L(e).begin,g),ma=-1!==a.inArray(h,g.ignorables)}function S(c,d,e,f,h){var i=this,j=a(i),k=c.which||c.charCode||c.keyCode;if(!(d===!0||c.ctrlKey&&c.altKey)&&(c.ctrlKey||c.metaKey||ma))return k===b.keyCode.ENTER&&ea!==x().join("")&&(ea=x().join(""),setTimeout(function(){j.trigger("change")},0)),!0;if(k){46===k&&c.shiftKey===!1&&","===g.radixPoint&&(k=44);var l,o=d?{begin:h,end:h}:L(i),p=String.fromCharCode(k);m().writeOutBuffer=!0;var q=B(o,p,f);if(q!==!1){var r=q.pos;if(n(!0),void 0!==q.caret)l=q.caret;else{var s=m().validPositions;l=!g.keepStatic&&(void 0!==s[r+1]&&v(r+1,s[r].locator.slice(),r).length>1||void 0!==s[r].alternation)?r+1:E(r)}m().p=l}if(e!==!1){var t=this;if(setTimeout(function(){g.onKeyValidation.call(t,k,q,g)},0),m().writeOutBuffer&&q!==!1){var u=x();H(i,u,g.numericInput&&void 0===q.caret?F(l):l,c,d!==!0),d!==!0&&setTimeout(function(){O(u)===!0&&j.trigger("complete")},0)}}if(g.showTooltip&&(i.title=g.tooltip||m().mask),c.preventDefault(),d)return q.forwardPosition=l,q}}function T(b){var c,d=this,e=b.originalEvent||b,f=a(d),h=d.inputmask._valueGet(!0),i=L(d);ja&&(c=i.end,i.end=i.begin,i.begin=c);var j=h.substr(0,i.begin),k=h.substr(i.end,h.length);j===(ja?w().reverse():w()).slice(0,i.begin).join("")&&(j=""),k===(ja?w().reverse():w()).slice(i.end).join("")&&(k=""),ja&&(c=j,j=k,k=c),window.clipboardData&&window.clipboardData.getData?h=j+window.clipboardData.getData("Text")+k:e.clipboardData&&e.clipboardData.getData&&(h=j+e.clipboardData.getData("text/plain")+k);var l=h;if(a.isFunction(g.onBeforePaste)){if(l=g.onBeforePaste(h,g),l===!1)return b.preventDefault();l||(l=h)}return J(d,!1,!1,ja?l.split("").reverse():l.toString().split("")),H(d,x(),E(o()),b,!0),O(x())===!0&&f.trigger("complete"),b.preventDefault()}function U(c){var d=this,e=d.inputmask._valueGet();if(x().join("")!==e){var f=L(d);if(e=e.replace(new RegExp("("+b.escapeRegex(w().join(""))+")*"),""),k){var g=e.replace(x().join(""),"");if(1===g.length){var h=new a.Event("keypress");return h.which=g.charCodeAt(0),S.call(d,h,!0,!0,!1,m().validPositions[f.begin-1]?f.begin:f.begin-1),!1}}if(f.begin>e.length&&(L(d,e.length),f=L(d)),x().length-e.length!==1||e.charAt(f.begin)===x()[f.begin]||e.charAt(f.begin+1)===x()[f.begin]||C(f.begin)){for(var i=o()+1,j=x().slice(i).join("");null===e.match(b.escapeRegex(j)+"$");)j=j.slice(1);e=e.replace(j,""),e=e.split(""),J(d,!0,!1,e),O(x())===!0&&a(d).trigger("complete")}else c.keyCode=b.keyCode.BACKSPACE,R.call(d,c);c.preventDefault()}}function V(b){var c=this,d=c.inputmask._valueGet();J(c,!0,!1,(a.isFunction(g.onBeforeMask)?g.onBeforeMask(d,g)||d:d).split("")),ea=x().join(""),(g.clearMaskOnLostFocus||g.clearIncomplete)&&c.inputmask._valueGet()===w().join("")&&c.inputmask._valueSet("")}function W(a){var b=this,c=b.inputmask._valueGet();g.showMaskOnFocus&&(!g.showMaskOnHover||g.showMaskOnHover&&""===c)?b.inputmask._valueGet()!==x().join("")&&H(b,x(),E(o())):na===!1&&L(b,E(o())),g.positionCaretOnTab===!0&&setTimeout(function(){L(b,E(o()))},0),ea=x().join("")}function X(a){var b=this;if(na=!1,g.clearMaskOnLostFocus&&document.activeElement!==b){var c=x().slice(),d=b.inputmask._valueGet();d!==b.getAttribute("placeholder")&&""!==d&&(-1===o()&&d===w().join("")?c=[]:N(c),H(b,c))}}function Y(b){function c(b){if(g.radixFocus&&""!==g.radixPoint){var c=m().validPositions;if(void 0===c[b]||c[b].input===I(b)){if(b<E(-1))return!0;var d=a.inArray(g.radixPoint,x());
if(-1!==d){for(var e in c)if(e>d&&c[e].input!==I(e))return!1;return!0}}}return!1}var d=this;setTimeout(function(){if(document.activeElement===d){var b=L(d);if(b.begin===b.end)if(c(b.begin))L(d,g.numericInput?E(a.inArray(g.radixPoint,x())):a.inArray(g.radixPoint,x()));else{var e=b.begin,f=o(e,!0),h=E(f);if(h>e)L(d,C(e)||C(e-1)?e:E(e));else{var i=I(h);(""!==i&&x()[h]!==i||!C(h,!0)&&s(h).def===i)&&(h=E(h)),L(d,h)}}}},0)}function Z(a){var b=this;setTimeout(function(){L(b,0,E(o()))},0)}function $(c){var d=this,e=a(d),f=L(d),h=c.originalEvent||c,i=window.clipboardData||h.clipboardData,j=ja?x().slice(f.end,f.begin):x().slice(f.begin,f.end);i.setData("text",ja?j.reverse().join(""):j.join("")),document.execCommand&&document.execCommand("copy"),Q(d,b.keyCode.DELETE,f),H(d,x(),m().p,c,ea!==x().join("")),d.inputmask._valueGet()===w().join("")&&e.trigger("cleared"),g.showTooltip&&(d.title=g.tooltip||m().mask)}function _(b){var c=a(this),d=this;if(d.inputmask){var e=d.inputmask._valueGet(),f=x().slice();ea!==f.join("")&&setTimeout(function(){c.trigger("change"),ea=f.join("")},0),""!==e&&(g.clearMaskOnLostFocus&&(-1===o()&&e===w().join("")?f=[]:N(f)),O(f)===!1&&(setTimeout(function(){c.trigger("incomplete")},0),g.clearIncomplete&&(n(),f=g.clearMaskOnLostFocus?[]:w().slice())),H(d,f,void 0,b))}}function aa(a){var b=this;na=!0,document.activeElement!==b&&g.showMaskOnHover&&b.inputmask._valueGet()!==x().join("")&&H(b,x())}function ba(a){ea!==x().join("")&&ga.trigger("change"),g.clearMaskOnLostFocus&&-1===o()&&fa.inputmask._valueGet&&fa.inputmask._valueGet()===w().join("")&&fa.inputmask._valueSet(""),g.removeMaskOnSubmit&&(fa.inputmask._valueSet(fa.inputmask.unmaskedvalue(),!0),setTimeout(function(){H(fa,x())},0))}function ca(a){setTimeout(function(){ga.trigger("setvalue")},0)}function da(b){if(fa=b,ga=a(fa),g.showTooltip&&(fa.title=g.tooltip||m().mask),("rtl"===fa.dir||g.rightAlign)&&(fa.style.textAlign="right"),("rtl"===fa.dir||g.numericInput)&&(fa.dir="ltr",fa.removeAttribute("dir"),fa.inputmask.isRTL=!0,ja=!0),oa.off(fa),P(fa),d(fa,g)&&(oa.on(fa,"submit",ba),oa.on(fa,"reset",ca),oa.on(fa,"mouseenter",aa),oa.on(fa,"blur",_),oa.on(fa,"focus",W),oa.on(fa,"mouseleave",X),oa.on(fa,"click",Y),oa.on(fa,"dblclick",Z),oa.on(fa,"paste",T),oa.on(fa,"dragdrop",T),oa.on(fa,"drop",T),oa.on(fa,"cut",$),oa.on(fa,"complete",g.oncomplete),oa.on(fa,"incomplete",g.onincomplete),oa.on(fa,"cleared",g.oncleared),oa.on(fa,"keydown",R),oa.on(fa,"keypress",S),oa.on(fa,"input",U)),oa.on(fa,"setvalue",V),""!==fa.inputmask._valueGet()||g.clearMaskOnLostFocus===!1||document.activeElement===fa){var c=a.isFunction(g.onBeforeMask)?g.onBeforeMask(fa.inputmask._valueGet(),g)||fa.inputmask._valueGet():fa.inputmask._valueGet();J(fa,!0,!1,c.split(""));var e=x().slice();ea=e.join(""),O(e)===!1&&g.clearIncomplete&&n(),g.clearMaskOnLostFocus&&document.activeElement!==fa&&(-1===o()?e=[]:N(e)),H(fa,e),document.activeElement===fa&&L(fa,E(o()))}}var ea,fa,ga,ha,ia,ja=!1,ka=!1,la=!1,ma=!1,na=!0,oa={on:function(c,d,e){var f=function(c){if(void 0===this.inputmask&&"FORM"!==this.nodeName){var d=a.data(this,"_inputmask_opts");d?new b(d).mask(this):oa.off(this)}else{if("setvalue"===c.type||!(this.disabled||this.readOnly&&!("keydown"===c.type&&c.ctrlKey&&67===c.keyCode||g.tabThrough===!1&&c.keyCode===b.keyCode.TAB))){switch(c.type){case"input":if(la===!0)return la=!1,c.preventDefault();break;case"keydown":ka=!1,la=!1;break;case"keypress":if(ka===!0)return c.preventDefault();ka=!0;break;case"click":if(k){var f=this;return setTimeout(function(){e.apply(f,arguments)},0),!1}}var h=e.apply(this,arguments);return h===!1&&(c.preventDefault(),c.stopPropagation()),h}c.preventDefault()}};c.inputmask.events[d]=c.inputmask.events[d]||[],c.inputmask.events[d].push(f),-1!==a.inArray(d,["submit","reset"])?null!=c.form&&a(c.form).on(d,f):a(c).on(d,f)},off:function(b,c){if(b.inputmask&&b.inputmask.events){var d;c?(d=[],d[c]=b.inputmask.events[c]):d=b.inputmask.events,a.each(d,function(c,d){for(;d.length>0;){var e=d.pop();-1!==a.inArray(c,["submit","reset"])?null!=b.form&&a(b.form).off(c,e):a(b).off(c,e)}delete b.inputmask.events[c]})}}};if(void 0!==e)switch(e.action){case"isComplete":return fa=e.el,O(x());case"unmaskedvalue":return fa=e.el,void 0!==fa&&void 0!==fa.inputmask?(f=fa.inputmask.maskset,g=fa.inputmask.opts,ja=fa.inputmask.isRTL):(ia=e.value,g.numericInput&&(ja=!0),ia=(a.isFunction(g.onBeforeMask)?g.onBeforeMask(ia,g)||ia:ia).split(""),J(void 0,!1,!1,ja?ia.reverse():ia),a.isFunction(g.onBeforeWrite)&&g.onBeforeWrite(void 0,x(),0,g)),K(fa);case"mask":fa=e.el,f=fa.inputmask.maskset,g=fa.inputmask.opts,ja=fa.inputmask.isRTL,ea=x().join(""),da(fa);break;case"format":return g.numericInput&&(ja=!0),ia=(a.isFunction(g.onBeforeMask)?g.onBeforeMask(e.value,g)||e.value:e.value).split(""),J(void 0,!1,!1,ja?ia.reverse():ia),a.isFunction(g.onBeforeWrite)&&g.onBeforeWrite(void 0,x(),0,g),e.metadata?{value:ja?x().slice().reverse().join(""):x().join(""),metadata:h({action:"getmetadata"},f,g)}:ja?x().slice().reverse().join(""):x().join("");case"isValid":g.numericInput&&(ja=!0),e.value?(ia=e.value.split(""),J(void 0,!1,!0,ja?ia.reverse():ia)):e.value=x().join("");for(var pa=x(),qa=M(),ra=pa.length-1;ra>qa&&!C(ra);ra--);return pa.splice(qa,ra+1-qa),O(pa)&&e.value===x().join("");case"getemptymask":return w().join("");case"remove":fa=e.el,ga=a(fa),f=fa.inputmask.maskset,g=fa.inputmask.opts,fa.inputmask._valueSet(K(fa)),oa.off(fa);var sa;Object.getOwnPropertyDescriptor&&Object.getPrototypeOf?(sa=Object.getOwnPropertyDescriptor(Object.getPrototypeOf(fa),"value"),sa&&fa.inputmask.__valueGet&&Object.defineProperty(fa,"value",{get:fa.inputmask.__valueGet,set:fa.inputmask.__valueSet,configurable:!0})):document.__lookupGetter__&&fa.__lookupGetter__("value")&&fa.inputmask.__valueGet&&(fa.__defineGetter__("value",fa.inputmask.__valueGet),fa.__defineSetter__("value",fa.inputmask.__valueSet)),fa.inputmask=void 0;break;case"getmetadata":if(a.isArray(f.metadata)){for(var ta,ua=o(void 0,!0),va=ua;va>=0;va--)if(m().validPositions[va]&&void 0!==m().validPositions[va].alternation){ta=m().validPositions[va].alternation;break}return void 0!==ta?f.metadata[m().validPositions[va].locator[ta]]:[]}return f.metadata}}b.prototype={defaults:{placeholder:"_",optionalmarker:{start:"[",end:"]"},quantifiermarker:{start:"{",end:"}"},groupmarker:{start:"(",end:")"},alternatormarker:"|",escapeChar:"\\",mask:null,oncomplete:a.noop,onincomplete:a.noop,oncleared:a.noop,repeat:0,greedy:!0,autoUnmask:!1,removeMaskOnSubmit:!1,clearMaskOnLostFocus:!0,insertMode:!0,clearIncomplete:!1,aliases:{},alias:null,onKeyDown:a.noop,onBeforeMask:null,onBeforePaste:function(b,c){return a.isFunction(c.onBeforeMask)?c.onBeforeMask(b,c):b},onBeforeWrite:null,onUnMask:null,showMaskOnFocus:!0,showMaskOnHover:!0,onKeyValidation:a.noop,skipOptionalPartCharacter:" ",showTooltip:!1,tooltip:void 0,numericInput:!1,rightAlign:!1,undoOnEscape:!0,radixPoint:"",radixPointDefinitionSymbol:void 0,groupSeparator:"",radixFocus:!1,nojumps:!1,nojumpsThreshold:0,keepStatic:null,positionCaretOnTab:!1,tabThrough:!1,supportsInputType:["text","tel","password"],definitions:{9:{validator:"[0-9]",cardinality:1,definitionSymbol:"*"},a:{validator:"[A-Za-z\u0410-\u044f\u0401\u0451\xc0-\xff\xb5]",cardinality:1,definitionSymbol:"*"},"*":{validator:"[0-9A-Za-z\u0410-\u044f\u0401\u0451\xc0-\xff\xb5]",cardinality:1}},ignorables:[8,9,13,19,27,33,34,35,36,37,38,39,40,45,46,93,112,113,114,115,116,117,118,119,120,121,122,123],isComplete:null,canClearPosition:a.noop,postValidation:null,staticDefinitionSymbol:void 0,jitMasking:!1,nullable:!0},masksCache:{},mask:function(c){var d=this;return"string"==typeof c&&(c=document.getElementById(c)||document.querySelectorAll(c)),c=c.nodeName?[c]:c,a.each(c,function(c,e){var i=a.extend(!0,{},d.opts);f(e,i,a.extend(!0,{},d.userOptions));var j=g(i,d.noMasksCache);void 0!==j&&(void 0!==e.inputmask&&e.inputmask.remove(),e.inputmask=new b,e.inputmask.opts=i,e.inputmask.noMasksCache=d.noMasksCache,e.inputmask.userOptions=a.extend(!0,{},d.userOptions),e.inputmask.el=e,e.inputmask.maskset=j,e.inputmask.isRTL=!1,a.data(e,"_inputmask_opts",i),h({action:"mask",el:e}))}),c&&c[0]?c[0].inputmask||this:this},option:function(b,c){return"string"==typeof b?this.opts[b]:"object"==typeof b?(a.extend(this.userOptions,b),this.el&&c!==!0&&this.mask(this.el),this):void 0},unmaskedvalue:function(a){return h({action:"unmaskedvalue",el:this.el,value:a},this.el&&this.el.inputmask?this.el.inputmask.maskset:g(this.opts,this.noMasksCache),this.opts)},remove:function(){return this.el?(h({action:"remove",el:this.el}),this.el.inputmask=void 0,this.el):void 0},getemptymask:function(){return h({action:"getemptymask"},this.maskset||g(this.opts,this.noMasksCache),this.opts)},hasMaskedValue:function(){return!this.opts.autoUnmask},isComplete:function(){return h({action:"isComplete",el:this.el},this.maskset||g(this.opts,this.noMasksCache),this.opts)},getmetadata:function(){return h({action:"getmetadata"},this.maskset||g(this.opts,this.noMasksCache),this.opts)},isValid:function(a){return h({action:"isValid",value:a},this.maskset||g(this.opts,this.noMasksCache),this.opts)},format:function(a,b){return h({action:"format",value:a,metadata:b},this.maskset||g(this.opts,this.noMasksCache),this.opts)}},b.extendDefaults=function(c){a.extend(!0,b.prototype.defaults,c)},b.extendDefinitions=function(c){a.extend(!0,b.prototype.defaults.definitions,c)},b.extendAliases=function(c){a.extend(!0,b.prototype.defaults.aliases,c)},b.format=function(a,c,d){return b(c).format(a,d)},b.unmask=function(a,c){return b(c).unmaskedvalue(a)},b.isValid=function(a,c){return b(c).isValid(a)},b.remove=function(b){a.each(b,function(a,b){b.inputmask&&b.inputmask.remove()})},b.escapeRegex=function(a){var b=["/",".","*","+","?","|","(",")","[","]","{","}","\\","$","^"];return a.replace(new RegExp("(\\"+b.join("|\\")+")","gim"),"\\$1")},b.keyCode={ALT:18,BACKSPACE:8,BACKSPACE_SAFARI:127,CAPS_LOCK:20,COMMA:188,COMMAND:91,COMMAND_LEFT:91,COMMAND_RIGHT:93,CONTROL:17,DELETE:46,DOWN:40,END:35,ENTER:13,ESCAPE:27,HOME:36,INSERT:45,LEFT:37,MENU:93,NUMPAD_ADD:107,NUMPAD_DECIMAL:110,NUMPAD_DIVIDE:111,NUMPAD_ENTER:108,NUMPAD_MULTIPLY:106,NUMPAD_SUBTRACT:109,PAGE_DOWN:34,PAGE_UP:33,PERIOD:190,RIGHT:39,SHIFT:16,SPACE:32,TAB:9,UP:38,WINDOWS:91,X:88};var i=navigator.userAgent,j=/mobile/i.test(i),k=/iemobile/i.test(i),l=/iphone/i.test(i)&&!k;/android.*safari.*/i.test(i)&&!k;return window.Inputmask=b,b}(jQuery),function(a,b){return void 0===a.fn.inputmask&&(a.fn.inputmask=function(c,d){var e,f=this[0];if(void 0===d&&(d={}),"string"==typeof c)switch(c){case"unmaskedvalue":return f&&f.inputmask?f.inputmask.unmaskedvalue():a(f).val();case"remove":return this.each(function(){this.inputmask&&this.inputmask.remove()});case"getemptymask":return f&&f.inputmask?f.inputmask.getemptymask():"";case"hasMaskedValue":return f&&f.inputmask?f.inputmask.hasMaskedValue():!1;case"isComplete":return f&&f.inputmask?f.inputmask.isComplete():!0;case"getmetadata":return f&&f.inputmask?f.inputmask.getmetadata():void 0;case"setvalue":a(f).val(d),f&&void 0!==f.inputmask&&a(f).triggerHandler("setvalue");break;case"option":if("string"!=typeof d)return this.each(function(){return void 0!==this.inputmask?this.inputmask.option(d):void 0});if(f&&void 0!==f.inputmask)return f.inputmask.option(d);break;default:return d.alias=c,e=new b(d),this.each(function(){e.mask(this)})}else{if("object"==typeof c)return e=new b(c),void 0===c.mask&&void 0===c.alias?this.each(function(){return void 0!==this.inputmask?this.inputmask.option(c):void e.mask(this)}):this.each(function(){e.mask(this)});if(void 0===c)return this.each(function(){e=new b(d),e.mask(this)})}}),a.fn.inputmask}(jQuery,Inputmask),function(a,b){return b.extendDefinitions({h:{validator:"[01][0-9]|2[0-3]",cardinality:2,prevalidator:[{validator:"[0-2]",cardinality:1}]},s:{validator:"[0-5][0-9]",cardinality:2,prevalidator:[{validator:"[0-5]",cardinality:1}]},d:{validator:"0[1-9]|[12][0-9]|3[01]",cardinality:2,prevalidator:[{validator:"[0-3]",cardinality:1}]},m:{validator:"0[1-9]|1[012]",cardinality:2,prevalidator:[{validator:"[01]",cardinality:1}]},y:{validator:"(19|20)\\d{2}",cardinality:4,prevalidator:[{validator:"[12]",cardinality:1},{validator:"(19|20)",cardinality:2},{validator:"(19|20)\\d",cardinality:3}]}}),b.extendAliases({"dd/mm/yyyy":{mask:"1/2/y",placeholder:"dd/mm/yyyy",regex:{val1pre:new RegExp("[0-3]"),val1:new RegExp("0[1-9]|[12][0-9]|3[01]"),val2pre:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[1-9]|[12][0-9]|3[01])"+c+"[01])")},val2:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[1-9]|[12][0-9])"+c+"(0[1-9]|1[012]))|(30"+c+"(0[13-9]|1[012]))|(31"+c+"(0[13578]|1[02]))")}},leapday:"29/02/",separator:"/",yearrange:{minyear:1900,maxyear:2099},isInYearRange:function(a,b,c){if(isNaN(a))return!1;var d=parseInt(a.concat(b.toString().slice(a.length))),e=parseInt(a.concat(c.toString().slice(a.length)));return(isNaN(d)?!1:d>=b&&c>=d)||(isNaN(e)?!1:e>=b&&c>=e)},determinebaseyear:function(a,b,c){var d=(new Date).getFullYear();if(a>d)return a;if(d>b){for(var e=b.toString().slice(0,2),f=b.toString().slice(2,4);e+c>b;)e--;var g=e+f;return a>g?a:g}if(d>=a&&b>=d){for(var h=d.toString().slice(0,2);h+c>b;)h--;var i=h+c;return a>i?a:i}return d},onKeyDown:function(c,d,e,f){var g=a(this);if(c.ctrlKey&&c.keyCode===b.keyCode.RIGHT){var h=new Date;g.val(h.getDate().toString()+(h.getMonth()+1).toString()+h.getFullYear().toString()),g.trigger("setvalue")}},getFrontValue:function(a,b,c){for(var d=0,e=0,f=0;f<a.length&&"2"!==a.charAt(f);f++){var g=c.definitions[a.charAt(f)];g?(d+=e,e=g.cardinality):e++}return b.join("").substr(d,e)},definitions:{1:{validator:function(a,b,c,d,e){var f=e.regex.val1.test(a);return d||f||a.charAt(1)!==e.separator&&-1==="-./".indexOf(a.charAt(1))||!(f=e.regex.val1.test("0"+a.charAt(0)))?f:(b.buffer[c-1]="0",{refreshFromBuffer:{start:c-1,end:c},pos:c,c:a.charAt(0)})},cardinality:2,prevalidator:[{validator:function(a,b,c,d,e){var f=a;isNaN(b.buffer[c+1])||(f+=b.buffer[c+1]);var g=1===f.length?e.regex.val1pre.test(f):e.regex.val1.test(f);if(!d&&!g){if(g=e.regex.val1.test(a+"0"))return b.buffer[c]=a,b.buffer[++c]="0",{pos:c,c:"0"};if(g=e.regex.val1.test("0"+a))return b.buffer[c]="0",c++,{pos:c}}return g},cardinality:1}]},2:{validator:function(a,b,c,d,e){var f=e.getFrontValue(b.mask,b.buffer,e);-1!==f.indexOf(e.placeholder[0])&&(f="01"+e.separator);var g=e.regex.val2(e.separator).test(f+a);if(!d&&!g&&(a.charAt(1)===e.separator||-1!=="-./".indexOf(a.charAt(1)))&&(g=e.regex.val2(e.separator).test(f+"0"+a.charAt(0))))return b.buffer[c-1]="0",{refreshFromBuffer:{start:c-1,end:c},pos:c,c:a.charAt(0)};if(e.mask.indexOf("2")===e.mask.length-1&&g){var h=b.buffer.join("").substr(4,4)+a;if(h!==e.leapday)return!0;var i=parseInt(b.buffer.join("").substr(0,4),10);return i%4===0?i%100===0?i%400===0?!0:!1:!0:!1}return g},cardinality:2,prevalidator:[{validator:function(a,b,c,d,e){isNaN(b.buffer[c+1])||(a+=b.buffer[c+1]);var f=e.getFrontValue(b.mask,b.buffer,e);-1!==f.indexOf(e.placeholder[0])&&(f="01"+e.separator);var g=1===a.length?e.regex.val2pre(e.separator).test(f+a):e.regex.val2(e.separator).test(f+a);return d||g||!(g=e.regex.val2(e.separator).test(f+"0"+a))?g:(b.buffer[c]="0",c++,{pos:c})},cardinality:1}]},y:{validator:function(a,b,c,d,e){if(e.isInYearRange(a,e.yearrange.minyear,e.yearrange.maxyear)){var f=b.buffer.join("").substr(0,6);if(f!==e.leapday)return!0;var g=parseInt(a,10);return g%4===0?g%100===0?g%400===0?!0:!1:!0:!1}return!1},cardinality:4,prevalidator:[{validator:function(a,b,c,d,e){var f=e.isInYearRange(a,e.yearrange.minyear,e.yearrange.maxyear);if(!d&&!f){var g=e.determinebaseyear(e.yearrange.minyear,e.yearrange.maxyear,a+"0").toString().slice(0,1);if(f=e.isInYearRange(g+a,e.yearrange.minyear,e.yearrange.maxyear))return b.buffer[c++]=g.charAt(0),{pos:c};if(g=e.determinebaseyear(e.yearrange.minyear,e.yearrange.maxyear,a+"0").toString().slice(0,2),f=e.isInYearRange(g+a,e.yearrange.minyear,e.yearrange.maxyear))return b.buffer[c++]=g.charAt(0),b.buffer[c++]=g.charAt(1),{pos:c}}return f},cardinality:1},{validator:function(a,b,c,d,e){var f=e.isInYearRange(a,e.yearrange.minyear,e.yearrange.maxyear);if(!d&&!f){var g=e.determinebaseyear(e.yearrange.minyear,e.yearrange.maxyear,a).toString().slice(0,2);if(f=e.isInYearRange(a[0]+g[1]+a[1],e.yearrange.minyear,e.yearrange.maxyear))return b.buffer[c++]=g.charAt(1),{pos:c};if(g=e.determinebaseyear(e.yearrange.minyear,e.yearrange.maxyear,a).toString().slice(0,2),e.isInYearRange(g+a,e.yearrange.minyear,e.yearrange.maxyear)){var h=b.buffer.join("").substr(0,6);if(h!==e.leapday)f=!0;else{var i=parseInt(a,10);f=i%4===0?i%100===0?i%400===0?!0:!1:!0:!1}}else f=!1;if(f)return b.buffer[c-1]=g.charAt(0),b.buffer[c++]=g.charAt(1),b.buffer[c++]=a.charAt(0),{refreshFromBuffer:{start:c-3,end:c},pos:c}}return f},cardinality:2},{validator:function(a,b,c,d,e){return e.isInYearRange(a,e.yearrange.minyear,e.yearrange.maxyear)},cardinality:3}]}},insertMode:!1,autoUnmask:!1},"mm/dd/yyyy":{placeholder:"mm/dd/yyyy",alias:"dd/mm/yyyy",regex:{val2pre:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[13-9]|1[012])"+c+"[0-3])|(02"+c+"[0-2])")},val2:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[1-9]|1[012])"+c+"(0[1-9]|[12][0-9]))|((0[13-9]|1[012])"+c+"30)|((0[13578]|1[02])"+c+"31)")},val1pre:new RegExp("[01]"),val1:new RegExp("0[1-9]|1[012]")},leapday:"02/29/",onKeyDown:function(c,d,e,f){var g=a(this);if(c.ctrlKey&&c.keyCode===b.keyCode.RIGHT){var h=new Date;g.val((h.getMonth()+1).toString()+h.getDate().toString()+h.getFullYear().toString()),g.trigger("setvalue")}}},"yyyy/mm/dd":{mask:"y/1/2",placeholder:"yyyy/mm/dd",alias:"mm/dd/yyyy",leapday:"/02/29",onKeyDown:function(c,d,e,f){var g=a(this);if(c.ctrlKey&&c.keyCode===b.keyCode.RIGHT){var h=new Date;g.val(h.getFullYear().toString()+(h.getMonth()+1).toString()+h.getDate().toString()),g.trigger("setvalue")}}},"dd.mm.yyyy":{mask:"1.2.y",placeholder:"dd.mm.yyyy",leapday:"29.02.",separator:".",alias:"dd/mm/yyyy"},"dd-mm-yyyy":{mask:"1-2-y",placeholder:"dd-mm-yyyy",leapday:"29-02-",separator:"-",alias:"dd/mm/yyyy"},"mm.dd.yyyy":{mask:"1.2.y",placeholder:"mm.dd.yyyy",leapday:"02.29.",separator:".",alias:"mm/dd/yyyy"},"mm-dd-yyyy":{mask:"1-2-y",placeholder:"mm-dd-yyyy",leapday:"02-29-",separator:"-",alias:"mm/dd/yyyy"},"yyyy.mm.dd":{mask:"y.1.2",placeholder:"yyyy.mm.dd",leapday:".02.29",separator:".",alias:"yyyy/mm/dd"},"yyyy-mm-dd":{mask:"y-1-2",placeholder:"yyyy-mm-dd",leapday:"-02-29",separator:"-",alias:"yyyy/mm/dd"},datetime:{mask:"1/2/y h:s",placeholder:"dd/mm/yyyy hh:mm",alias:"dd/mm/yyyy",regex:{hrspre:new RegExp("[012]"),hrs24:new RegExp("2[0-4]|1[3-9]"),hrs:new RegExp("[01][0-9]|2[0-4]"),ampm:new RegExp("^[a|p|A|P][m|M]"),mspre:new RegExp("[0-5]"),ms:new RegExp("[0-5][0-9]")},timeseparator:":",hourFormat:"24",definitions:{h:{validator:function(a,b,c,d,e){if("24"===e.hourFormat&&24===parseInt(a,10))return b.buffer[c-1]="0",b.buffer[c]="0",{refreshFromBuffer:{start:c-1,end:c},c:"0"};var f=e.regex.hrs.test(a);if(!d&&!f&&(a.charAt(1)===e.timeseparator||-1!=="-.:".indexOf(a.charAt(1)))&&(f=e.regex.hrs.test("0"+a.charAt(0))))return b.buffer[c-1]="0",b.buffer[c]=a.charAt(0),c++,{refreshFromBuffer:{start:c-2,end:c},pos:c,c:e.timeseparator};if(f&&"24"!==e.hourFormat&&e.regex.hrs24.test(a)){var g=parseInt(a,10);return 24===g?(b.buffer[c+5]="a",b.buffer[c+6]="m"):(b.buffer[c+5]="p",b.buffer[c+6]="m"),g-=12,10>g?(b.buffer[c]=g.toString(),b.buffer[c-1]="0"):(b.buffer[c]=g.toString().charAt(1),b.buffer[c-1]=g.toString().charAt(0)),{refreshFromBuffer:{start:c-1,end:c+6},c:b.buffer[c]}}return f},cardinality:2,prevalidator:[{validator:function(a,b,c,d,e){var f=e.regex.hrspre.test(a);return d||f||!(f=e.regex.hrs.test("0"+a))?f:(b.buffer[c]="0",c++,{pos:c})},cardinality:1}]},s:{validator:"[0-5][0-9]",cardinality:2,prevalidator:[{validator:function(a,b,c,d,e){var f=e.regex.mspre.test(a);return d||f||!(f=e.regex.ms.test("0"+a))?f:(b.buffer[c]="0",c++,{pos:c})},cardinality:1}]},t:{validator:function(a,b,c,d,e){return e.regex.ampm.test(a+"m")},casing:"lower",cardinality:1}},insertMode:!1,autoUnmask:!1},datetime12:{mask:"1/2/y h:s t\\m",placeholder:"dd/mm/yyyy hh:mm xm",alias:"datetime",hourFormat:"12"},"mm/dd/yyyy hh:mm xm":{mask:"1/2/y h:s t\\m",placeholder:"mm/dd/yyyy hh:mm xm",alias:"datetime12",regex:{val2pre:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[13-9]|1[012])"+c+"[0-3])|(02"+c+"[0-2])")},val2:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[1-9]|1[012])"+c+"(0[1-9]|[12][0-9]))|((0[13-9]|1[012])"+c+"30)|((0[13578]|1[02])"+c+"31)")},val1pre:new RegExp("[01]"),val1:new RegExp("0[1-9]|1[012]")},leapday:"02/29/",onKeyDown:function(c,d,e,f){var g=a(this);if(c.ctrlKey&&c.keyCode===b.keyCode.RIGHT){var h=new Date;g.val((h.getMonth()+1).toString()+h.getDate().toString()+h.getFullYear().toString()),g.trigger("setvalue")}}},"hh:mm t":{mask:"h:s t\\m",placeholder:"hh:mm xm",alias:"datetime",hourFormat:"12"},"h:s t":{mask:"h:s t\\m",placeholder:"hh:mm xm",alias:"datetime",hourFormat:"12"},"hh:mm:ss":{mask:"h:s:s",placeholder:"hh:mm:ss",alias:"datetime",autoUnmask:!1},"hh:mm":{mask:"h:s",placeholder:"hh:mm",alias:"datetime",autoUnmask:!1},date:{alias:"dd/mm/yyyy"},"mm/yyyy":{mask:"1/y",placeholder:"mm/yyyy",leapday:"donotuse",separator:"/",alias:"mm/dd/yyyy"},shamsi:{regex:{val2pre:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[1-9]|1[012])"+c+"[0-3])")},val2:function(a){var c=b.escapeRegex.call(this,a);return new RegExp("((0[1-9]|1[012])"+c+"(0[1-9]|[12][0-9]))|((0[1-9]|1[012])"+c+"30)|((0[1-6])"+c+"31)")},val1pre:new RegExp("[01]"),val1:new RegExp("0[1-9]|1[012]")},yearrange:{minyear:1300,maxyear:1499},mask:"y/1/2",leapday:"/12/30",placeholder:"yyyy/mm/dd",alias:"mm/dd/yyyy",clearIncomplete:!0}}),b}(jQuery,Inputmask),function(a,b){return b.extendDefinitions({A:{validator:"[A-Za-z\u0410-\u044f\u0401\u0451\xc0-\xff\xb5]",cardinality:1,casing:"upper"},"&":{validator:"[0-9A-Za-z\u0410-\u044f\u0401\u0451\xc0-\xff\xb5]",cardinality:1,casing:"upper"},"#":{validator:"[0-9A-Fa-f]",cardinality:1,casing:"upper"}}),b.extendAliases({url:{definitions:{i:{validator:".",cardinality:1}},mask:"(\\http://)|(\\http\\s://)|(ftp://)|(ftp\\s://)i{+}",insertMode:!1,autoUnmask:!1},ip:{mask:"i[i[i]].i[i[i]].i[i[i]].i[i[i]]",definitions:{i:{validator:function(a,b,c,d,e){return c-1>-1&&"."!==b.buffer[c-1]?(a=b.buffer[c-1]+a,a=c-2>-1&&"."!==b.buffer[c-2]?b.buffer[c-2]+a:"0"+a):a="00"+a,new RegExp("25[0-5]|2[0-4][0-9]|[01][0-9][0-9]").test(a)},cardinality:1}},onUnMask:function(a,b,c){return a}},email:{mask:"*{1,64}[.*{1,64}][.*{1,64}][.*{1,63}]@-{1,63}.-{1,63}[.-{1,63}][.-{1,63}]",greedy:!1,onBeforePaste:function(a,b){return a=a.toLowerCase(),a.replace("mailto:","")},definitions:{"*":{validator:"[0-9A-Za-z!#$%&'*+/=?^_`{|}~-]",cardinality:1,casing:"lower"},"-":{validator:"[0-9A-Za-z-]",cardinality:1,casing:"lower"}},onUnMask:function(a,b,c){return a}},mac:{mask:"##:##:##:##:##:##"},vin:{mask:"V{13}9{4}",definitions:{V:{validator:"[A-HJ-NPR-Za-hj-npr-z\\d]",cardinality:1,casing:"upper"}},clearIncomplete:!0,autoUnmask:!0}}),b}(jQuery,Inputmask),function(a,b){return b.extendAliases({numeric:{mask:function(a){function c(b){for(var c="",d=0;d<b.length;d++)c+=a.definitions[b.charAt(d)]||a.optionalmarker.start===b.charAt(d)||a.optionalmarker.end===b.charAt(d)||a.quantifiermarker.start===b.charAt(d)||a.quantifiermarker.end===b.charAt(d)||a.groupmarker.start===b.charAt(d)||a.groupmarker.end===b.charAt(d)||a.alternatormarker===b.charAt(d)?"\\"+b.charAt(d):b.charAt(d);return c}if(0!==a.repeat&&isNaN(a.integerDigits)&&(a.integerDigits=a.repeat),a.repeat=0,a.groupSeparator===a.radixPoint&&("."===a.radixPoint?a.groupSeparator=",":","===a.radixPoint?a.groupSeparator=".":a.groupSeparator="")," "===a.groupSeparator&&(a.skipOptionalPartCharacter=void 0),a.autoGroup=a.autoGroup&&""!==a.groupSeparator,a.autoGroup&&("string"==typeof a.groupSize&&isFinite(a.groupSize)&&(a.groupSize=parseInt(a.groupSize)),isFinite(a.integerDigits))){var d=Math.floor(a.integerDigits/a.groupSize),e=a.integerDigits%a.groupSize;a.integerDigits=parseInt(a.integerDigits)+(0===e?d-1:d),a.integerDigits<1&&(a.integerDigits="*")}a.placeholder.length>1&&(a.placeholder=a.placeholder.charAt(0)),a.radixFocus=a.radixFocus&&""!==a.placeholder&&a.integerOptional===!0,a.definitions[";"]=a.definitions["~"],a.definitions[";"].definitionSymbol="~",a.numericInput===!0&&(a.radixFocus=!1,a.digitsOptional=!1,isNaN(a.digits)&&(a.digits=2),a.decimalProtect=!1);var f=c(a.prefix);return f+="[+]",f+=a.integerOptional===!0?"~{1,"+a.integerDigits+"}":"~{"+a.integerDigits+"}",void 0!==a.digits&&(isNaN(a.digits)||parseInt(a.digits)>0)&&(a.decimalProtect&&(a.radixPointDefinitionSymbol=":"),f+=a.digitsOptional?"["+(a.decimalProtect?":":a.radixPoint)+";{1,"+a.digits+"}]":(a.decimalProtect?":":a.radixPoint)+";{"+a.digits+"}"),f+="[-]",f+=c(a.suffix),a.greedy=!1,null!==a.min&&(a.min=a.min.toString().replace(new RegExp(b.escapeRegex(a.groupSeparator),"g"),""),","===a.radixPoint&&(a.min=a.min.replace(a.radixPoint,"."))),null!==a.max&&(a.max=a.max.toString().replace(new RegExp(b.escapeRegex(a.groupSeparator),"g"),""),","===a.radixPoint&&(a.max=a.max.replace(a.radixPoint,"."))),f},placeholder:"",greedy:!1,digits:"*",digitsOptional:!0,radixPoint:".",radixFocus:!0,groupSize:3,groupSeparator:"",autoGroup:!1,allowPlus:!0,allowMinus:!0,negationSymbol:{front:"-",back:""},integerDigits:"+",integerOptional:!0,prefix:"",suffix:"",rightAlign:!0,decimalProtect:!0,min:null,max:null,step:1,insertMode:!0,autoUnmask:!1,unmaskAsNumber:!1,postFormat:function(c,d,e){e.numericInput===!0&&(c=c.reverse(),isFinite(d)&&(d=c.join("").length-d-1));var f,g,h=!1;c.length>=e.suffix.length&&c.join("").indexOf(e.suffix)===c.length-e.suffix.length&&(c.length=c.length-e.suffix.length,h=!0),d=d>=c.length?c.length-1:d<e.prefix.length?e.prefix.length:d;var i=!1,j=c[d],k=c.slice();j===e.groupSeparator&&(k.splice(d--,1),j=k[d]),j!==e.radixPoint&&j!==e.negationSymbol.front&&j!==e.negationSymbol.back&&(k[d]="?");var l=k.join(""),m=l;if(l.length>0&&e.autoGroup||-1!==l.indexOf(e.groupSeparator)){var n=b.escapeRegex(e.groupSeparator);i=0===l.indexOf(e.groupSeparator),l=l.replace(new RegExp(n,"g"),"");var o=l.split(e.radixPoint);if(l=""===e.radixPoint?l:o[0],l!==e.prefix+"?0"&&l.length>=e.groupSize+e.prefix.length)for(var p=new RegExp("([-+]?[\\d?]+)([\\d?]{"+e.groupSize+"})");p.test(l)&&""!==e.groupSeparator;)l=l.replace(p,"$1"+e.groupSeparator+"$2"),l=l.replace(e.groupSeparator+e.groupSeparator,e.groupSeparator);""!==e.radixPoint&&o.length>1&&(l+=e.radixPoint+o[1])}for(i=m!==l,c.length=l.length,f=0,g=l.length;g>f;f++)c[f]=l.charAt(f);var q=a.inArray("?",c);if(-1===q&&(q=a.inArray(j,c)),c[q]=j,!i&&h)for(f=0,g=e.suffix.length;g>f;f++)c.push(e.suffix.charAt(f));return q=e.numericInput&&isFinite(d)?c.join("").length-q-1:q,e.numericInput&&(c=c.reverse(),a.inArray(e.radixPoint,c)<q&&c.join("").length-e.suffix.length!==q&&(q-=1)),{pos:q,refreshFromBuffer:i,buffer:c}},onBeforeWrite:function(c,d,e,f){var g;if(c&&("blur"===c.type||"checkval"===c.type||"keydown"===c.type)){var h=f.numericInput?d.slice().reverse().join(""):d.join(""),i=h.replace(f.prefix,"");i=i.replace(f.suffix,""),i=i.replace(new RegExp(b.escapeRegex(f.groupSeparator),"g"),""),","===f.radixPoint&&(i=i.replace(f.radixPoint,"."));var j=i.match(new RegExp("[-"+b.escapeRegex(f.negationSymbol.front)+"]","g"));if(j=null!==j&&1===j.length,i=i.replace(new RegExp("[-"+b.escapeRegex(f.negationSymbol.front)+"]","g"),""),i=i.replace(new RegExp(b.escapeRegex(f.negationSymbol.back)+"$"),""),isNaN(f.placeholder)&&(i=i.replace(new RegExp(b.escapeRegex(f.placeholder),"g"),"")),i=i===f.negationSymbol.front?i+"0":i,""!==i&&isFinite(i)){var k=parseFloat(i),l=j?-1*k:k;if(null!==f.min&&isFinite(f.min)&&l<parseFloat(f.min)?(k=Math.abs(f.min),j=f.min<0,h=void 0):null!==f.max&&isFinite(f.max)&&l>parseFloat(f.max)&&(k=Math.abs(f.max),j=f.max<0,h=void 0),i=k.toString().replace(".",f.radixPoint).split(""),isFinite(f.digits)){var m=a.inArray(f.radixPoint,i),n=a.inArray(f.radixPoint,h);-1===m&&(i.push(f.radixPoint),m=i.length-1);for(var o=1;o<=f.digits;o++)f.digitsOptional||void 0!==i[m+o]&&i[m+o]!==f.placeholder.charAt(0)?-1!==n&&void 0!==h[n+o]&&(i[m+o]=i[m+o]||h[n+o]):i[m+o]="0";i[i.length-1]===f.radixPoint&&delete i[i.length-1]}if(k.toString()!==i&&k.toString()+"."!==i||j)return!j||0===k&&"blur"===c.type||(i.unshift(f.negationSymbol.front),i.push(f.negationSymbol.back)),i=(f.prefix+i.join("")).split(""),f.numericInput&&(i=i.reverse()),g=f.postFormat(i,f.numericInput?e:e-1,f),g.buffer&&(g.refreshFromBuffer=g.buffer.join("")!==d.join("")),g}}return f.autoGroup?(g=f.postFormat(d,f.numericInput?e:e-1,f),g.caret=e<=f.prefix.length?g.pos:g.pos+1,g):void 0},regex:{integerPart:function(a){return new RegExp("["+b.escapeRegex(a.negationSymbol.front)+"+]?\\d+")},integerNPart:function(a){return new RegExp("[\\d"+b.escapeRegex(a.groupSeparator)+b.escapeRegex(a.placeholder.charAt(0))+"]+")}},signHandler:function(a,b,c,d,e){if(!d&&e.allowMinus&&"-"===a||e.allowPlus&&"+"===a){var f=b.buffer.join("").match(e.regex.integerPart(e));if(f&&f[0].length>0)return b.buffer[f.index]===("-"===a?"+":e.negationSymbol.front)?"-"===a?""!==e.negationSymbol.back?{pos:f.index,c:e.negationSymbol.front,remove:f.index,caret:c,insert:{pos:b.buffer.length-e.suffix.length-1,c:e.negationSymbol.back}}:{pos:f.index,c:e.negationSymbol.front,remove:f.index,caret:c}:""!==e.negationSymbol.back?{pos:f.index,c:"+",remove:[f.index,b.buffer.length-e.suffix.length-1],caret:c}:{pos:f.index,c:"+",remove:f.index,caret:c}:b.buffer[f.index]===("-"===a?e.negationSymbol.front:"+")?"-"===a&&""!==e.negationSymbol.back?{remove:[f.index,b.buffer.length-e.suffix.length-1],caret:c-1}:{remove:f.index,caret:c-1}:"-"===a?""!==e.negationSymbol.back?{pos:f.index,c:e.negationSymbol.front,caret:c+1,insert:{pos:b.buffer.length-e.suffix.length,c:e.negationSymbol.back}}:{pos:f.index,c:e.negationSymbol.front,caret:c+1}:{pos:f.index,c:a,caret:c+1}}return!1},radixHandler:function(b,c,d,e,f){if(!e&&f.numericInput!==!0&&b===f.radixPoint&&void 0!==f.digits&&(isNaN(f.digits)||parseInt(f.digits)>0)){var g=a.inArray(f.radixPoint,c.buffer),h=c.buffer.join("").match(f.regex.integerPart(f));if(-1!==g&&c.validPositions[g])return c.validPositions[g-1]?{caret:g+1}:{pos:h.index,c:h[0],caret:g+1};if(!h||"0"===h[0]&&h.index+1!==d)return c.buffer[h?h.index:d]="0",{pos:(h?h.index:d)+1,c:f.radixPoint}}return!1},leadingZeroHandler:function(b,c,d,e,f,g){if(!e)if(f.numericInput===!0){var h=c.buffer.slice("").reverse(),i=h[f.prefix.length];if("0"===i&&void 0===c.validPositions[d-1])return{pos:d,remove:h.length-f.prefix.length-1}}else{var j=a.inArray(f.radixPoint,c.buffer),k=c.buffer.slice(0,-1!==j?j:void 0).join("").match(f.regex.integerNPart(f));if(k&&(-1===j||j>=d)){var l=-1===j?0:parseInt(c.buffer.slice(j+1).join(""));if(0===k[0].indexOf(""!==f.placeholder?f.placeholder.charAt(0):"0")&&(k.index+1===d||g!==!0&&0===l))return c.buffer.splice(k.index,1),{pos:k.index,remove:k.index};if("0"===b&&d<=k.index&&k[0]!==f.groupSeparator)return!1}}return!0},definitions:{"~":{validator:function(c,d,e,f,g,h){var i=g.signHandler(c,d,e,f,g);if(!i&&(i=g.radixHandler(c,d,e,f,g),!i&&(i=f?new RegExp("[0-9"+b.escapeRegex(g.groupSeparator)+"]").test(c):new RegExp("[0-9]").test(c),i===!0&&(i=g.leadingZeroHandler(c,d,e,f,g,h),i===!0)))){var j=a.inArray(g.radixPoint,d.buffer);i=-1!==j&&(g.digitsOptional===!1||d.validPositions[e])&&g.numericInput!==!0&&e>j&&!f?{pos:e,remove:e}:{pos:e}}return i},cardinality:1},"+":{validator:function(a,b,c,d,e){var f=e.signHandler(a,b,c,d,e);return!f&&(d&&e.allowMinus&&a===e.negationSymbol.front||e.allowMinus&&"-"===a||e.allowPlus&&"+"===a)&&(f=d||"-"!==a?!0:""!==e.negationSymbol.back?{pos:c,c:"-"===a?e.negationSymbol.front:"+",caret:c+1,insert:{pos:b.buffer.length,c:e.negationSymbol.back}}:{pos:c,c:"-"===a?e.negationSymbol.front:"+",
caret:c+1}),f},cardinality:1,placeholder:""},"-":{validator:function(a,b,c,d,e){var f=e.signHandler(a,b,c,d,e);return!f&&d&&e.allowMinus&&a===e.negationSymbol.back&&(f=!0),f},cardinality:1,placeholder:""},":":{validator:function(a,c,d,e,f){var g=f.signHandler(a,c,d,e,f);if(!g){var h="["+b.escapeRegex(f.radixPoint)+"]";g=new RegExp(h).test(a),g&&c.validPositions[d]&&c.validPositions[d].match.placeholder===f.radixPoint&&(g={caret:d+1})}return g?{c:f.radixPoint}:g},cardinality:1,placeholder:function(a){return a.radixPoint}}},onUnMask:function(a,c,d){var e=a.replace(d.prefix,"");return e=e.replace(d.suffix,""),e=e.replace(new RegExp(b.escapeRegex(d.groupSeparator),"g"),""),d.unmaskAsNumber?(""!==d.radixPoint&&-1!==e.indexOf(d.radixPoint)&&(e=e.replace(b.escapeRegex.call(this,d.radixPoint),".")),Number(e)):e},isComplete:function(a,c){var d=a.join(""),e=a.slice();if(c.postFormat(e,0,c),e.join("")!==d)return!1;var f=d.replace(c.prefix,"");return f=f.replace(c.suffix,""),f=f.replace(new RegExp(b.escapeRegex(c.groupSeparator),"g"),""),","===c.radixPoint&&(f=f.replace(b.escapeRegex(c.radixPoint),".")),isFinite(f)},onBeforeMask:function(a,c){if(""!==c.radixPoint&&isFinite(a))a=a.toString().replace(".",c.radixPoint);else{var d=a.match(/,/g),e=a.match(/\./g);e&&d?e.length>d.length?(a=a.replace(/\./g,""),a=a.replace(",",c.radixPoint)):d.length>e.length?(a=a.replace(/,/g,""),a=a.replace(".",c.radixPoint)):a=a.indexOf(".")<a.indexOf(",")?a.replace(/\./g,""):a=a.replace(/,/g,""):a=a.replace(new RegExp(b.escapeRegex(c.groupSeparator),"g"),"")}if(0===c.digits&&(-1!==a.indexOf(".")?a=a.substring(0,a.indexOf(".")):-1!==a.indexOf(",")&&(a=a.substring(0,a.indexOf(",")))),""!==c.radixPoint&&isFinite(c.digits)&&-1!==a.indexOf(c.radixPoint)){var f=a.split(c.radixPoint),g=f[1].match(new RegExp("\\d*"))[0];if(parseInt(c.digits)<g.toString().length){var h=Math.pow(10,parseInt(c.digits));a=a.replace(b.escapeRegex(c.radixPoint),"."),a=Math.round(parseFloat(a)*h)/h,a=a.toString().replace(".",c.radixPoint)}}return a.toString()},canClearPosition:function(a,b,c,d,e){var f=a.validPositions[b].input,g=f!==e.radixPoint||null!==a.validPositions[b].match.fn&&e.decimalProtect===!1||isFinite(f)||b===c||f===e.groupSeparator||f===e.negationSymbol.front||f===e.negationSymbol.back;return g},onKeyDown:function(c,d,e,f){var g=a(this);if(c.ctrlKey)switch(c.keyCode){case b.keyCode.UP:g.val(parseFloat(this.inputmask.unmaskedvalue())+parseInt(f.step)),g.trigger("setvalue");break;case b.keyCode.DOWN:g.val(parseFloat(this.inputmask.unmaskedvalue())-parseInt(f.step)),g.trigger("setvalue")}}},currency:{prefix:"$ ",groupSeparator:",",alias:"numeric",placeholder:"0",autoGroup:!0,digits:2,digitsOptional:!1,clearMaskOnLostFocus:!1},decimal:{alias:"numeric"},integer:{alias:"numeric",digits:0,radixPoint:""},percentage:{alias:"numeric",digits:2,radixPoint:".",placeholder:"0",autoGroup:!1,min:0,max:100,suffix:" %",allowPlus:!1,allowMinus:!1}}),b}(jQuery,Inputmask),function(a,b){return b.extendAliases({phone:{url:"phone-codes/phone-codes.js",countrycode:"",phoneCodeCache:{},mask:function(b){if(void 0===b.phoneCodeCache[b.url]){var c=[];b.definitions["#"]=b.definitions[9],a.ajax({url:b.url,async:!1,type:"get",dataType:"json",success:function(a){c=a},error:function(a,c,d){alert(d+" - "+b.url)}}),b.phoneCodeCache[b.url]=c.sort(function(a,b){return(a.mask||a)<(b.mask||b)?-1:1})}return b.phoneCodeCache[b.url]},keepStatic:!1,nojumps:!0,nojumpsThreshold:1,onBeforeMask:function(a,b){var c=a.replace(/^0{1,2}/,"").replace(/[\s]/g,"");return(c.indexOf(b.countrycode)>1||-1===c.indexOf(b.countrycode))&&(c="+"+b.countrycode+c),c}},phonebe:{alias:"phone",url:"phone-codes/phone-be.js",countrycode:"32",nojumpsThreshold:4}}),b}(jQuery,Inputmask),function(a,b){return b.extendAliases({Regex:{mask:"r",greedy:!1,repeat:"*",regex:null,regexTokens:null,tokenizer:/\[\^?]?(?:[^\\\]]+|\\[\S\s]?)*]?|\\(?:0(?:[0-3][0-7]{0,2}|[4-7][0-7]?)?|[1-9][0-9]*|x[0-9A-Fa-f]{2}|u[0-9A-Fa-f]{4}|c[A-Za-z]|[\S\s]?)|\((?:\?[:=!]?)?|(?:[?*+]|\{[0-9]+(?:,[0-9]*)?\})\??|[^.?*+^${[()|\\]+|./g,quantifierFilter:/[0-9]+[^,]/,isComplete:function(a,b){return new RegExp(b.regex).test(a.join(""))},definitions:{r:{validator:function(b,c,d,e,f){function g(a,b){this.matches=[],this.isGroup=a||!1,this.isQuantifier=b||!1,this.quantifier={min:1,max:1},this.repeaterPart=void 0}function h(){var a,b,c=new g,d=[];for(f.regexTokens=[];a=f.tokenizer.exec(f.regex);)switch(b=a[0],b.charAt(0)){case"(":d.push(new g(!0));break;case")":k=d.pop(),d.length>0?d[d.length-1].matches.push(k):c.matches.push(k);break;case"{":case"+":case"*":var e=new g(!1,!0);b=b.replace(/[{}]/g,"");var h=b.split(","),i=isNaN(h[0])?h[0]:parseInt(h[0]),j=1===h.length?i:isNaN(h[1])?h[1]:parseInt(h[1]);if(e.quantifier={min:i,max:j},d.length>0){var l=d[d.length-1].matches;a=l.pop(),a.isGroup||(k=new g(!0),k.matches.push(a),a=k),l.push(a),l.push(e)}else a=c.matches.pop(),a.isGroup||(k=new g(!0),k.matches.push(a),a=k),c.matches.push(a),c.matches.push(e);break;default:d.length>0?d[d.length-1].matches.push(b):c.matches.push(b)}c.matches.length>0&&f.regexTokens.push(c)}function i(b,c){var d=!1;c&&(m+="(",o++);for(var e=0;e<b.matches.length;e++){var f=b.matches[e];if(f.isGroup===!0)d=i(f,!0);else if(f.isQuantifier===!0){var g=a.inArray(f,b.matches),h=b.matches[g-1],k=m;if(isNaN(f.quantifier.max)){for(;f.repeaterPart&&f.repeaterPart!==m&&f.repeaterPart.length>m.length&&!(d=i(h,!0)););d=d||i(h,!0),d&&(f.repeaterPart=m),m=k+f.quantifier.max}else{for(var l=0,n=f.quantifier.max-1;n>l&&!(d=i(h,!0));l++);m=k+"{"+f.quantifier.min+","+f.quantifier.max+"}"}}else if(void 0!==f.matches)for(var p=0;p<f.length&&!(d=i(f[p],c));p++);else{var q;if("["==f.charAt(0)){q=m,q+=f;for(var r=0;o>r;r++)q+=")";var s=new RegExp("^("+q+")$");d=s.test(j)}else for(var t=0,u=f.length;u>t;t++)if("\\"!==f.charAt(t)){q=m,q+=f.substr(0,t+1),q=q.replace(/\|$/,"");for(var r=0;o>r;r++)q+=")";var s=new RegExp("^("+q+")$");if(d=s.test(j))break}m+=f}if(d)break}return c&&(m+=")",o--),d}var j,k,l=c.buffer.slice(),m="",n=!1,o=0;null===f.regexTokens&&h(),l.splice(d,0,b),j=l.join("");for(var p=0;p<f.regexTokens.length;p++){var q=f.regexTokens[p];if(n=i(q,q.isGroup))break}return n},cardinality:1}}}}),b}(jQuery,Inputmask);
/* End */
;
; /* Start:"a:4:{s:4:"full";s:60:"/bitrix/templates/elasto_start/js/moreMenu.js?14658030863336";s:6:"source";s:45:"/bitrix/templates/elasto_start/js/moreMenu.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
(function (factory) {
	if(typeof define === "function" && define.amd) {		
		define(["jquery"], factory);
	} else {		
		factory(jQuery);
	}
}(function ($) {
	var moreObjects = [];
	
	function adjustMoreMenu() {
		$(moreObjects).each(function () {
			$(this).moreMenu({
				"undo": true
			}).moreMenu(this.options);
		});
	}	

	$(window).resize(function () {		
		adjustMoreMenu();
	});

	$.fn.moreMenu = function (options) {
		var checkMoreObject,
			s = $.extend({
				"threshold": 2,				
				"linkText": "...",				
				"undo": false
			}, options);
		this.options = s;
		checkMoreObject = $.inArray(this, moreObjects);
		if(checkMoreObject >= 0) {
			moreObjects.splice(checkMoreObject, 1);
		} else {
			moreObjects.push(this);
		}
		return this.each(function () {
			var $this = $(this),				
				$items = $this.find("> li"),
				$self = $this,
				$firstItem = $items.first(),
				$lastItem = $items.last(),
				numItems = $this.find("li").length,
				firstItemTop = Math.floor($firstItem.offset().top),
				firstItemHeight = Math.floor($firstItem.outerHeight(true)),
				$lastChild,
				keepLooking,
				$moreItem,				
				numToRemove,				
				$menu,
				i;
			
			function needsMenu($itemOfInterest) {
				var result = (Math.ceil($itemOfInterest.offset().top) >= (firstItemTop + firstItemHeight) && $(".navbar-toggle").css("display") == "none") ? true : false;
				return result;
			}

			if(needsMenu($lastItem) && numItems > s.threshold && !s.undo && $this.is(":visible")) {
				var $popup = $("<ul class='dropdown-menu more-menu'></ul>");
				
				for(i = numItems; i > 1; i--) {					
					$lastChild = $this.find("> li:last-child");
					keepLooking = (needsMenu($lastChild));
					if(keepLooking) {
						$lastChild.appendTo($popup);
					} else {
						break;
					}					
				}				
				$this.append("<li class='dropdown more'><a href='javascript:void(0)'>" + s.linkText + "</a></li>");				
				
				$moreItem = $this.find("> li.more");				
				if(needsMenu($moreItem)) {
					$this.find("> li:nth-last-child(2)").appendTo($popup);
				}				
				
				$popup.children().each(function(i, li) {
					$popup.prepend(li);
				});
				
				$moreItem.append($popup);

				$moreItem.hover(function() {
					var menu = $(this).closest(".navbar"),
						menuWidth = menu.outerWidth(),
						menuLeft = menu.offset().left,
						menuRight = menuLeft + menuWidth,											
						dropdownMenu = $(this).children(".dropdown-menu"),
						dropdownMenuWidth = dropdownMenu.outerWidth(),					
						dropdownMenuLeft = $(this).offset().left,
						dropdownMenuRight = dropdownMenuLeft + dropdownMenuWidth;
					if(dropdownMenuRight > menuRight) {
						dropdownMenu.css({"right": "0"});
					}
					$(this).addClass("open").children(".dropdown-menu").stop(true, true).delay(200).fadeIn(150);
				}, function() {
					$(this).removeClass("open").children(".dropdown-menu").stop(true, true).delay(200).fadeOut(150);
				});
			} else if(s.undo && $this.find("ul.more-menu")) {
				$menu = $this.find("ul.more-menu");
				numToRemove = $menu.find("li").length;
				for(i = 1; i <= numToRemove; i++) {
					$menu.find("> li:first-child").appendTo($this);
				}
				$menu.remove();
				$this.find("> li.more").remove();
			}
		});
	};
}));
/* End */
;
; /* Start:"a:4:{s:4:"full";s:56:"/bitrix/templates/elasto_start/js/main.js?14951818345978";s:6:"source";s:41:"/bitrix/templates/elasto_start/js/main.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
/***DROPDOWN***/
function hideDropdown() {
	if($(".navbar-toggle").css("display") == "none") {
		$(".navbar-nav .dropdown-menu").each(function() {
			if($(this).css("display") == "block")
				$(this).hide();				
		});
		$(".navbar-nav .dropdown").each(function() {
			if($(this).hasClass("open"))
				$(this).removeClass("open");
		})
	}
}

/***FIXED_MENU***/
function fixedMenu() {	
	var menuWrap = $(".top-menu-wrapper"),
		menuWrapTop = menuWrap.offset().top,		
		menu = $(".top-menu"),
		menuHeight = menu.outerHeight(),
		top = $(document).scrollTop();			
	if(top > menuWrapTop && $(".navbar-toggle").css("display") == "none") {
		menu.addClass("fixed");
		menuWrap.css({height: menuHeight + "px"});			
	} else {
		menu.removeClass("fixed");
		menuWrap.css({height: ""});			
	}	
}

/***SLIDER***/
function adjustSlider() {		
	var slide = $(".slider .slider-item"),		
		ks = 3.2;	
	slide.height(Math.ceil($(window).width() / ks));
}

/***SERVICES_SECTIONS***/
function adjustServicesSections() {		
	var pic = $(".services-item .item-pic"),
		picSec = $(".section-item .item-pic"),
		kss = 4 / 3;	
	pic.height(Math.ceil(pic.width() / kss));
	picSec.height(Math.ceil(picSec.width() / kss));
}

/***GALLERY***/	
function adjustGalleryBg() {		
	$(".gallery").each(function() {		
		var elemsBgSearch = $(this).find(".gallery-item-bg");
		if(!!elemsBgSearch) {
			elemsBgSearch.parent().remove();
		}		
		var i, elemsCount = $(this).find(".fancyimage").length;
		if($(".navbar-toggle").css("display") == "none") {
			var elemsRowCount = 4;
		} else {
			var elemsRowCount = 2;
		}
		var rowsCount = Math.ceil(elemsCount / elemsRowCount),
			elemsBgCount = (elemsRowCount * rowsCount) - elemsCount,
			pagination = $(this).find(".pagination"),
			elemsBg = "<div class='col-xs-6 col-sm-6 col-md-3'><div class='gallery-item-bg'></div></div>";
		for(i = 0; i < elemsBgCount; i++) {	
			if(pagination.length) {
				pagination.parent().before(elemsBg);
			} else {
				$(this).append(elemsBg);
			}
		}
	})
}

/***GALLERY***/
function adjustGallery() {
	var img = $(".fancyimage .item-image"),
		cap = $(".fancyimage .item-caption-wrap"),
		bg = $(".gallery-item-bg"),
		kg = 4 / 3;	
	img.height(Math.ceil(img.width() / kg));
	cap.height(Math.ceil(cap.width() / kg));
	bg.height(Math.ceil(bg.width() / kg));
}

/***DETAIL_BANNER***/
function adjustBanner() {
	var ban = $(".services-detail-banner"),		
		kb = 3.33;
	ban.height(Math.ceil($(window).width() / kb));
}

$(function() {
	/***MENU***/
	if($(".navbar-nav").length) {
		/***MOREMENU***/
		$(".navbar-nav").moreMenu();	

		/***DROPDOWN***/	
		$(".navbar-nav .dropdown:not(.more)").on({		
			mouseenter: function() {			
				if($(".navbar-toggle").css("display") == "none") {
					var menu = $(this).closest(".navbar"),
						menuWidth = menu.outerWidth(),
						menuLeft = menu.offset().left,
						menuRight = menuLeft + menuWidth,
						isParentDropdownMenu = $(this).closest(".dropdown-menu"),					
						dropdownMenu = $(this).children(".dropdown-menu"),
						dropdownMenuWidth = dropdownMenu.outerWidth(),					
						dropdownMenuLeft = isParentDropdownMenu.length ? $(this).offset().left + $(this).outerWidth() : $(this).offset().left,
						dropdownMenuRight = dropdownMenuLeft + dropdownMenuWidth;
					if(dropdownMenuRight > menuRight) {
						if(isParentDropdownMenu.length) {
							dropdownMenu.css({"left": "auto", "right": "100%"});
						} else {
							dropdownMenu.css({"right": "0"});
						}
					}
					$(this).addClass("open").children(".dropdown-menu").stop(true, true).delay(200).fadeIn(150);				
				}
			},
			mouseleave: function() {			
				if($(".navbar-toggle").css("display") == "none") {			
					$(this).removeClass("open").children(".dropdown-menu").stop(true, true).delay(200).fadeOut(150);				
				}
			}
		});
		$(".navbar-nav .caret").on("click", function() {
			if($(".navbar-toggle").css("display") != "none") {		
				var li = $(this).closest(".dropdown"),
					isLiActive = li.hasClass("open");
				if(!!li) {
					if(!isLiActive) {
						li.addClass("open").children(".dropdown-menu").stop(true, true).show();
						return false;
					} else {
						li.removeClass("open").children(".dropdown-menu").stop(true, true).hide();					
						return false;
					}
				}
			}
		});
		$(window).resize(function() {	
			hideDropdown();
		});
		hideDropdown();
	}

	/***FIXED_MENU***/	
	if($(".top-menu-wrapper").length && $(".top-menu").length) {
		$(window).resize(function() {
			$(window).scroll(function() {
				fixedMenu();
			});
			fixedMenu();
		});
		$(window).scroll(function() {
			fixedMenu();
		});
		fixedMenu();
	}
	
	/***SLIDER***/
	if($(".slider").length) {	
		$(window).resize(function() {
			adjustSlider();		
		});
		adjustSlider();
	}

	/***SERVICES_SECTIONS***/
	if($(".services .item-pic").length || $(".sections .item-pic").length) {		
		$(window).resize(function() {		
			adjustServicesSections();		
		});	
		adjustServicesSections();
	}
	
	/***GALLERY***/	
	if($(".gallery").length) {		
		$(window).resize(function() {		
			adjustGalleryBg();
			adjustGallery();
		});	
		adjustGalleryBg();		
		adjustGallery();
	}

	/***DETAIL_BANNER***/
	if($(".services-detail-banner").length) {		
		$(window).resize(function() {		
			adjustBanner();
		});
		adjustBanner();
	}

	/***SCROLL_UP***/
	var top_show = 150,
		delay = 500;	
	$("body").append($("<a />").addClass("scroll-up").attr({"href": "javascript:void(0)", "id": "scrollUp"}).append($("<i />").addClass("fa fa-angle-up")));
	$("#scrollUp").click(function(e) {
		e.preventDefault();
		$("body, html").animate({scrollTop: 0}, delay);
		return false;
    });		
	$(window).scroll(function () {
		if($(this).scrollTop() > top_show) {
			$("#scrollUp").fadeIn();
		} else {
			$("#scrollUp").fadeOut();
		}
    });
});
/* End */
;
; /* Start:"a:4:{s:4:"full";s:65:"/bitrix/templates/elasto_start/js/jquery.cookie.js?14622757213121";s:6:"source";s:50:"/bitrix/templates/elasto_start/js/jquery.cookie.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
/*!
 * jQuery Cookie Plugin v1.4.1
 * https://github.com/carhartl/jquery-cookie
 *
 * Copyright 2013 Klaus Hartl
 * Released under the MIT license
 */
(function (factory) {
	if (typeof define === 'function' && define.amd) {
		// AMD
		define(['jquery'], factory);
	} else if (typeof exports === 'object') {
		// CommonJS
		factory(require('jquery'));
	} else {
		// Browser globals
		factory(jQuery);
	}
}(function ($) {

	var pluses = /\+/g;

	function encode(s) {
		return config.raw ? s : encodeURIComponent(s);
	}

	function decode(s) {
		return config.raw ? s : decodeURIComponent(s);
	}

	function stringifyCookieValue(value) {
		return encode(config.json ? JSON.stringify(value) : String(value));
	}

	function parseCookieValue(s) {
		if (s.indexOf('"') === 0) {
			// This is a quoted cookie as according to RFC2068, unescape...
			s = s.slice(1, -1).replace(/\\"/g, '"').replace(/\\\\/g, '\\');
		}

		try {
			// Replace server-side written pluses with spaces.
			// If we can't decode the cookie, ignore it, it's unusable.
			// If we can't parse the cookie, ignore it, it's unusable.
			s = decodeURIComponent(s.replace(pluses, ' '));
			return config.json ? JSON.parse(s) : s;
		} catch(e) {}
	}

	function read(s, converter) {
		var value = config.raw ? s : parseCookieValue(s);
		return $.isFunction(converter) ? converter(value) : value;
	}

	var config = $.cookie = function (key, value, options) {

		// Write

		if (value !== undefined && !$.isFunction(value)) {
			options = $.extend({}, config.defaults, options);

			if (typeof options.expires === 'number') {
				var days = options.expires, t = options.expires = new Date();
				t.setTime(+t + days * 864e+5);
			}

			return (document.cookie = [
				encode(key), '=', stringifyCookieValue(value),
				options.expires ? '; expires=' + options.expires.toUTCString() : '', // use expires attribute, max-age is not supported by IE
				options.path    ? '; path=' + options.path : '',
				options.domain  ? '; domain=' + options.domain : '',
				options.secure  ? '; secure' : ''
			].join(''));
		}

		// Read

		var result = key ? undefined : {};

		// To prevent the for loop in the first place assign an empty array
		// in case there are no cookies at all. Also prevents odd result when
		// calling $.cookie().
		var cookies = document.cookie ? document.cookie.split('; ') : [];

		for (var i = 0, l = cookies.length; i < l; i++) {
			var parts = cookies[i].split('=');
			var name = decode(parts.shift());
			var cookie = parts.join('=');

			if (key && key === name) {
				// If second argument (value) is a function it's a converter...
				result = read(cookie, value);
				break;
			}

			// Prevent storing a cookie that we couldn't decode.
			if (!key && (cookie = read(cookie)) !== undefined) {
				result[name] = cookie;
			}
		}

		return result;
	};

	config.defaults = {};

	$.removeCookie = function (key, options) {
		if ($.cookie(key) === undefined) {
			return false;
		}

		// Must not alter options, thus extending a fresh object...
		$.cookie(key, '', $.extend({}, options, { expires: -1 }));
		return !$.cookie(key);
	};

}));

/* End */
;
; /* Start:"a:4:{s:4:"full";s:70:"/bitrix/templates/elasto_start/js/spectrum/spectrum.js?146363863579101";s:6:"source";s:54:"/bitrix/templates/elasto_start/js/spectrum/spectrum.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
// Spectrum Colorpicker v1.7.1
// https://github.com/bgrins/spectrum
// Author: Brian Grinstead
// License: MIT

(function (factory) {
    "use strict";

    if (typeof define === 'function' && define.amd) { // AMD
        define(['jquery'], factory);
    }
    else if (typeof exports == "object" && typeof module == "object") { // CommonJS
        module.exports = factory;
    }
    else { // Browser
        factory(jQuery);
    }
})(function($, undefined) {
    "use strict";

    var defaultOpts = {

        // Callbacks
        beforeShow: noop,
        move: noop,
        change: noop,
        show: noop,
        hide: noop,

        // Options
        color: false,
        flat: false,
        showInput: false,
        allowEmpty: false,
        showButtons: true,
        clickoutFiresChange: true,
        showInitial: false,
        showPalette: false,
        showPaletteOnly: false,
        hideAfterPaletteSelect: false,
        togglePaletteOnly: false,
        showSelectionPalette: true,
        localStorageKey: false,
        appendTo: "body",
        maxSelectionSize: 7,
        cancelText: "cancel",
        chooseText: "choose",
        togglePaletteMoreText: "more",
        togglePaletteLessText: "less",
        clearText: "Clear Color Selection",
        noColorSelectedText: "No Color Selected",
        preferredFormat: false,
        className: "", // Deprecated - use containerClassName and replacerClassName instead.
        containerClassName: "",
        replacerClassName: "",
        showAlpha: false,
        theme: "sp-light",
        palette: [["#ffffff", "#000000", "#ff0000", "#ff8000", "#ffff00", "#008000", "#0000ff", "#4b0082", "#9400d3"]],
        selectionPalette: [],
        disabled: false,
        offset: null
    },
    spectrums = [],
    IE = !!/msie/i.exec( window.navigator.userAgent ),
    rgbaSupport = (function() {
        function contains( str, substr ) {
            return !!~('' + str).indexOf(substr);
        }

        var elem = document.createElement('div');
        var style = elem.style;
        style.cssText = 'background-color:rgba(0,0,0,.5)';
        return contains(style.backgroundColor, 'rgba') || contains(style.backgroundColor, 'hsla');
    })(),
    replaceInput = [
        "<div class='sp-replacer'>",
            "<div class='sp-preview'><div class='sp-preview-inner'></div></div>",
            "<div class='sp-dd'>&#9660;</div>",
        "</div>"
    ].join(''),
    markup = (function () {

        // IE does not support gradients with multiple stops, so we need to simulate
        //  that for the rainbow slider with 8 divs that each have a single gradient
        var gradientFix = "";
        if (IE) {
            for (var i = 1; i <= 6; i++) {
                gradientFix += "<div class='sp-" + i + "'></div>";
            }
        }

        return [
            "<div class='sp-container sp-hidden'>",
                "<div class='sp-palette-container'>",
                    "<div class='sp-palette sp-thumb sp-cf'></div>",
                    "<div class='sp-palette-button-container sp-cf'>",
                        "<button type='button' class='sp-palette-toggle'></button>",
                    "</div>",
                "</div>",
                "<div class='sp-picker-container'>",
                    "<div class='sp-top sp-cf'>",
                        "<div class='sp-fill'></div>",
                        "<div class='sp-top-inner'>",
                            "<div class='sp-color'>",
                                "<div class='sp-sat'>",
                                    "<div class='sp-val'>",
                                        "<div class='sp-dragger'></div>",
                                    "</div>",
                                "</div>",
                            "</div>",
                            "<div class='sp-clear sp-clear-display'>",
                            "</div>",
                            "<div class='sp-hue'>",
                                "<div class='sp-slider'></div>",
                                gradientFix,
                            "</div>",
                        "</div>",
                        "<div class='sp-alpha'><div class='sp-alpha-inner'><div class='sp-alpha-handle'></div></div></div>",
                    "</div>",
                    "<div class='sp-input-container sp-cf'>",
                        "<input class='sp-input' type='text' spellcheck='false'  />",
                    "</div>",
                    "<div class='sp-initial sp-thumb sp-cf'></div>",
                    "<div class='sp-button-container sp-cf'>",						
                        "<button type='button' class='btn btn-primary sp-choose'></button>",
						"<button type='button' class='btn btn-cancel sp-cancel pop-up-close'></button>",
                    "</div>",
                "</div>",
            "</div>"
        ].join("");
    })();

    function paletteTemplate (p, color, className, opts) {
        var html = [];
        for (var i = 0; i < p.length; i++) {
            var current = p[i];
            if(current) {
                var tiny = tinycolor(current);
                var c = tiny.toHsl().l < 0.5 ? "sp-thumb-el sp-thumb-dark" : "sp-thumb-el sp-thumb-light";
                c += (tinycolor.equals(color, current)) ? " sp-thumb-active" : "";
                var formattedString = tiny.toString(opts.preferredFormat || "rgb");
                var swatchStyle = rgbaSupport ? ("background-color:" + tiny.toRgbString()) : "filter:" + tiny.toFilter();
                html.push('<span title="' + formattedString + '" data-color="' + tiny.toRgbString() + '" class="' + c + '"><span class="sp-thumb-inner" style="' + swatchStyle + ';" /></span>');
            } else {
                var cls = 'sp-clear-display';
                html.push($('<div />')
                    .append($('<span data-color="" style="background-color:transparent;" class="' + cls + '"></span>')
                        .attr('title', opts.noColorSelectedText)
                    )
                    .html()
                );
            }
        }
        return "<div class='sp-cf " + className + "'>" + html.join('') + "</div>";
    }

    function hideAll() {
        for (var i = 0; i < spectrums.length; i++) {
            if (spectrums[i]) {
                spectrums[i].hide();
            }
        }
    }

    function instanceOptions(o, callbackContext) {
        var opts = $.extend({}, defaultOpts, o);
        opts.callbacks = {
            'move': bind(opts.move, callbackContext),
            'change': bind(opts.change, callbackContext),
            'show': bind(opts.show, callbackContext),
            'hide': bind(opts.hide, callbackContext),
            'beforeShow': bind(opts.beforeShow, callbackContext)
        };

        return opts;
    }

    function spectrum(element, o) {

        var opts = instanceOptions(o, element),
            flat = opts.flat,
            showSelectionPalette = opts.showSelectionPalette,
            localStorageKey = opts.localStorageKey,
            theme = opts.theme,
            callbacks = opts.callbacks,
            resize = throttle(reflow, 10),
            visible = false,
            isDragging = false,
            dragWidth = 0,
            dragHeight = 0,
            dragHelperHeight = 0,
            slideHeight = 0,
            slideWidth = 0,
            alphaWidth = 0,
            alphaSlideHelperWidth = 0,
            slideHelperHeight = 0,
            currentHue = 0,
            currentSaturation = 0,
            currentValue = 0,
            currentAlpha = 1,
            palette = [],
            paletteArray = [],
            paletteLookup = {},
            selectionPalette = opts.selectionPalette.slice(0),
            maxSelectionSize = opts.maxSelectionSize,
            draggingClass = "sp-dragging",
            shiftMovementDirection = null;

        var doc = element.ownerDocument,
            body = doc.body,
            boundElement = $(element),
            disabled = false,
            container = $(markup, doc).addClass(theme),
            pickerContainer = container.find(".sp-picker-container"),
            dragger = container.find(".sp-color"),
            dragHelper = container.find(".sp-dragger"),
            slider = container.find(".sp-hue"),
            slideHelper = container.find(".sp-slider"),
            alphaSliderInner = container.find(".sp-alpha-inner"),
            alphaSlider = container.find(".sp-alpha"),
            alphaSlideHelper = container.find(".sp-alpha-handle"),
            textInput = container.find(".sp-input"),
            paletteContainer = container.find(".sp-palette"),
            initialColorContainer = container.find(".sp-initial"),
            cancelButton = container.find(".sp-cancel"),
            clearButton = container.find(".sp-clear"),
            chooseButton = container.find(".sp-choose"),
            toggleButton = container.find(".sp-palette-toggle"),
            isInput = boundElement.is("input"),
            isInputTypeColor = isInput && boundElement.attr("type") === "color" && inputTypeColorSupport(),
            shouldReplace = isInput && !flat,
            replacer = (shouldReplace) ? $(replaceInput).addClass(theme).addClass(opts.className).addClass(opts.replacerClassName) : $([]),
            offsetElement = (shouldReplace) ? replacer : boundElement,
            previewElement = replacer.find(".sp-preview-inner"),
            initialColor = opts.color || (isInput && boundElement.val()),
            colorOnShow = false,
            preferredFormat = opts.preferredFormat,
            currentPreferredFormat = preferredFormat,
            clickoutFiresChange = !opts.showButtons || opts.clickoutFiresChange,
            isEmpty = !initialColor,
            allowEmpty = opts.allowEmpty && !isInputTypeColor;

        function applyOptions() {

            if (opts.showPaletteOnly) {
                opts.showPalette = true;
            }

            toggleButton.text(opts.showPaletteOnly ? opts.togglePaletteMoreText : opts.togglePaletteLessText);

            if (opts.palette) {
                palette = opts.palette.slice(0);
                paletteArray = $.isArray(palette[0]) ? palette : [palette];
                paletteLookup = {};
                for (var i = 0; i < paletteArray.length; i++) {
                    for (var j = 0; j < paletteArray[i].length; j++) {
                        var rgb = tinycolor(paletteArray[i][j]).toRgbString();
                        paletteLookup[rgb] = true;
                    }
                }
            }

            container.toggleClass("sp-flat", flat);
            container.toggleClass("sp-input-disabled", !opts.showInput);
            container.toggleClass("sp-alpha-enabled", opts.showAlpha);
            container.toggleClass("sp-clear-enabled", allowEmpty);
            container.toggleClass("sp-buttons-disabled", !opts.showButtons);
            container.toggleClass("sp-palette-buttons-disabled", !opts.togglePaletteOnly);
            container.toggleClass("sp-palette-disabled", !opts.showPalette);
            container.toggleClass("sp-palette-only", opts.showPaletteOnly);
            container.toggleClass("sp-initial-disabled", !opts.showInitial);
            container.addClass(opts.className).addClass(opts.containerClassName);

            reflow();
        }

        function initialize() {

            if (IE) {
                container.find("*:not(input)").attr("unselectable", "on");
            }

            applyOptions();

            if (shouldReplace) {
                boundElement.after(replacer).hide();
            }

            if (!allowEmpty) {
                clearButton.hide();
            }

            if (flat) {
                boundElement.after(container).hide();
            }
            else {

                var appendTo = opts.appendTo === "parent" ? boundElement.parent() : $(opts.appendTo);
                if (appendTo.length !== 1) {
                    appendTo = $("body");
                }

                appendTo.append(container);
            }

            updateSelectionPaletteFromStorage();

            offsetElement.bind("click.spectrum touchstart.spectrum", function (e) {
                if (!disabled) {
                    toggle();
                }

                e.stopPropagation();

                if (!$(e.target).is("input")) {
                    e.preventDefault();
                }
            });

            if(boundElement.is(":disabled") || (opts.disabled === true)) {
                disable();
            }

            // Prevent clicks from bubbling up to document.  This would cause it to be hidden.
            container.click(stopPropagation);

            // Handle user typed input
            textInput.change(setFromTextInput);
            textInput.bind("paste", function () {
                setTimeout(setFromTextInput, 1);
            });
            textInput.keydown(function (e) { if (e.keyCode == 13) { setFromTextInput(); } });
            
			cancelButton.html(opts.cancelText);
            cancelButton.bind("click.spectrum", function (e) {
                e.stopPropagation();
                e.preventDefault();
                revert();
                hide();
            });

            clearButton.attr("title", opts.clearText);
            clearButton.bind("click.spectrum", function (e) {
                e.stopPropagation();
                e.preventDefault();
                isEmpty = true;
                move();

                if(flat) {
                    //for the flat style, this is a change event
                    updateOriginalInput(true);
                }
            });

            chooseButton.text(opts.chooseText);
            chooseButton.bind("click.spectrum", function (e) {
                e.stopPropagation();
                e.preventDefault();

                if (IE && textInput.is(":focus")) {
                    textInput.trigger('change');
                }

                if (isValid()) {
                    updateOriginalInput(true);
                    hide();
                }
            });

            toggleButton.text(opts.showPaletteOnly ? opts.togglePaletteMoreText : opts.togglePaletteLessText);
            toggleButton.bind("click.spectrum", function (e) {
                e.stopPropagation();
                e.preventDefault();

                opts.showPaletteOnly = !opts.showPaletteOnly;

                // To make sure the Picker area is drawn on the right, next to the
                // Palette area (and not below the palette), first move the Palette
                // to the left to make space for the picker, plus 5px extra.
                // The 'applyOptions' function puts the whole container back into place
                // and takes care of the button-text and the sp-palette-only CSS class.
                if (!opts.showPaletteOnly && !flat) {
                    container.css('left', '-=' + (pickerContainer.outerWidth(true) + 5));					
                }
                applyOptions();
            });

            draggable(alphaSlider, function (dragX, dragY, e) {
                currentAlpha = (dragX / alphaWidth);
                isEmpty = false;
                if (e.shiftKey) {
                    currentAlpha = Math.round(currentAlpha * 10) / 10;
                }

                move();
            }, dragStart, dragStop);

            draggable(slider, function (dragX, dragY) {
                currentHue = parseFloat(dragY / slideHeight);
                isEmpty = false;
                if (!opts.showAlpha) {
                    currentAlpha = 1;
                }
                move();
            }, dragStart, dragStop);

            draggable(dragger, function (dragX, dragY, e) {

                // shift+drag should snap the movement to either the x or y axis.
                if (!e.shiftKey) {
                    shiftMovementDirection = null;
                }
                else if (!shiftMovementDirection) {
                    var oldDragX = currentSaturation * dragWidth;
                    var oldDragY = dragHeight - (currentValue * dragHeight);
                    var furtherFromX = Math.abs(dragX - oldDragX) > Math.abs(dragY - oldDragY);

                    shiftMovementDirection = furtherFromX ? "x" : "y";
                }

                var setSaturation = !shiftMovementDirection || shiftMovementDirection === "x";
                var setValue = !shiftMovementDirection || shiftMovementDirection === "y";

                if (setSaturation) {
                    currentSaturation = parseFloat(dragX / dragWidth);
                }
                if (setValue) {
                    currentValue = parseFloat((dragHeight - dragY) / dragHeight);
                }

                isEmpty = false;
                if (!opts.showAlpha) {
                    currentAlpha = 1;
                }

                move();

            }, dragStart, dragStop);

            if (!!initialColor) {
                set(initialColor);

                // In case color was black - update the preview UI and set the format
                // since the set function will not run (default color is black).
                updateUI();
                currentPreferredFormat = preferredFormat || tinycolor(initialColor).format;

                addColorToSelectionPalette(initialColor);
            }
            else {
                updateUI();
            }

            if (flat) {
                show();
            }

            function paletteElementClick(e) {
                if (e.data && e.data.ignore) {
                    set($(e.target).closest(".sp-thumb-el").data("color"));
                    move();
                }
                else {
                    set($(e.target).closest(".sp-thumb-el").data("color"));
                    move();
                    updateOriginalInput(true);
                    if (opts.hideAfterPaletteSelect) {
                      hide();
                    }
                }

                return false;
            }

            var paletteEvent = IE ? "mousedown.spectrum" : "click.spectrum touchstart.spectrum";
            paletteContainer.delegate(".sp-thumb-el", paletteEvent, paletteElementClick);
            initialColorContainer.delegate(".sp-thumb-el:nth-child(1)", paletteEvent, { ignore: true }, paletteElementClick);
        }

        function updateSelectionPaletteFromStorage() {

            if (localStorageKey && window.localStorage) {

                // Migrate old palettes over to new format.  May want to remove this eventually.
                try {
                    var oldPalette = window.localStorage[localStorageKey].split(",#");
                    if (oldPalette.length > 1) {
                        delete window.localStorage[localStorageKey];
                        $.each(oldPalette, function(i, c) {
                             addColorToSelectionPalette(c);
                        });
                    }
                }
                catch(e) { }

                try {
                    selectionPalette = window.localStorage[localStorageKey].split(";");
                }
                catch (e) { }
            }
        }

        function addColorToSelectionPalette(color) {
            if (showSelectionPalette) {
                var rgb = tinycolor(color).toRgbString();
                if (!paletteLookup[rgb] && $.inArray(rgb, selectionPalette) === -1) {
                    selectionPalette.push(rgb);
                    while(selectionPalette.length > maxSelectionSize) {
                        selectionPalette.shift();
                    }
                }

                if (localStorageKey && window.localStorage) {
                    try {
                        window.localStorage[localStorageKey] = selectionPalette.join(";");
                    }
                    catch(e) { }
                }
            }
        }

        function getUniqueSelectionPalette() {
            var unique = [];
            if (opts.showPalette) {
                for (var i = 0; i < selectionPalette.length; i++) {
                    var rgb = tinycolor(selectionPalette[i]).toRgbString();

                    if (!paletteLookup[rgb]) {
                        unique.push(selectionPalette[i]);
                    }
                }
            }

            return unique.reverse().slice(0, opts.maxSelectionSize);
        }

        function drawPalette() {

            var currentColor = get();

            var html = $.map(paletteArray, function (palette, i) {
                return paletteTemplate(palette, currentColor, "sp-palette-row sp-palette-row-" + i, opts);
            });

            updateSelectionPaletteFromStorage();

            if (selectionPalette) {
                html.push(paletteTemplate(getUniqueSelectionPalette(), currentColor, "sp-palette-row sp-palette-row-selection", opts));
            }

            paletteContainer.html(html.join(""));
        }

        function drawInitial() {
            if (opts.showInitial) {
                var initial = colorOnShow;
                var current = get();
                initialColorContainer.html(paletteTemplate([initial, current], current, "sp-palette-row-initial", opts));
            }
        }

        function dragStart() {
            if (dragHeight <= 0 || dragWidth <= 0 || slideHeight <= 0) {
                reflow();
            }
            isDragging = true;
            container.addClass(draggingClass);
            shiftMovementDirection = null;
            boundElement.trigger('dragstart.spectrum', [ get() ]);
        }

        function dragStop() {
            isDragging = false;
            container.removeClass(draggingClass);
            boundElement.trigger('dragstop.spectrum', [ get() ]);
        }

        function setFromTextInput() {

            var value = textInput.val();

            if ((value === null || value === "") && allowEmpty) {
                set(null);
                updateOriginalInput(true);
            }
            else {
                var tiny = tinycolor(value);
                if (tiny.isValid()) {
                    set(tiny);
                    updateOriginalInput(true);
                }
                else {
                    textInput.addClass("sp-validation-error");
                }
            }
        }

        function toggle() {
            if (visible) {
                hide();
            }
            else {
                show();
            }
        }

        function show() {
            var event = $.Event('beforeShow.spectrum');

            if (visible) {
                reflow();
                return;
            }

            boundElement.trigger(event, [ get() ]);

            if (callbacks.beforeShow(get()) === false || event.isDefaultPrevented()) {
                return;
            }

            hideAll();
            visible = true;

            $(doc).bind("keydown.spectrum", onkeydown);
            $(doc).bind("click.spectrum", clickout);
            $(window).bind("resize.spectrum", resize);
            replacer.addClass("sp-active");
            container.removeClass("sp-hidden");

            reflow();
            updateUI();

            colorOnShow = get();

            drawInitial();
            callbacks.show(colorOnShow);
            boundElement.trigger('show.spectrum', [ colorOnShow ]);
        }

        function onkeydown(e) {
            // Close on ESC
            if (e.keyCode === 27) {
                hide();
            }
        }

        function clickout(e) {
            // Return on right click.
            if (e.button == 2) { return; }

            // If a drag event was happening during the mouseup, don't hide
            // on click.
            if (isDragging) { return; }

            if (clickoutFiresChange) {
                updateOriginalInput(true);
            }
            else {
                revert();
            }
            hide();
        }

        function hide() {
            // Return if hiding is unnecessary
            if (!visible || flat) { return; }
            visible = false;

            $(doc).unbind("keydown.spectrum", onkeydown);
            $(doc).unbind("click.spectrum", clickout);
            $(window).unbind("resize.spectrum", resize);

            replacer.removeClass("sp-active");
            container.addClass("sp-hidden");

            callbacks.hide(get());
            boundElement.trigger('hide.spectrum', [ get() ]);
        }

        function revert() {
            set(colorOnShow, true);
        }

        function set(color, ignoreFormatChange) {
            if (tinycolor.equals(color, get())) {
                // Update UI just in case a validation error needs
                // to be cleared.
                updateUI();
                return;
            }

            var newColor, newHsv;
            if (!color && allowEmpty) {
                isEmpty = true;
            } else {
                isEmpty = false;
                newColor = tinycolor(color);
                newHsv = newColor.toHsv();

                currentHue = (newHsv.h % 360) / 360;
                currentSaturation = newHsv.s;
                currentValue = newHsv.v;
                currentAlpha = newHsv.a;
            }
            updateUI();

            if (newColor && newColor.isValid() && !ignoreFormatChange) {
                currentPreferredFormat = preferredFormat || newColor.getFormat();
            }
        }

        function get(opts) {
            opts = opts || { };

            if (allowEmpty && isEmpty) {
                return null;
            }

            return tinycolor.fromRatio({
                h: currentHue,
                s: currentSaturation,
                v: currentValue,
                a: Math.round(currentAlpha * 100) / 100
            }, { format: opts.format || currentPreferredFormat });
        }

        function isValid() {
            return !textInput.hasClass("sp-validation-error");
        }

        function move() {
            updateUI();

            callbacks.move(get());
            boundElement.trigger('move.spectrum', [ get() ]);
        }

        function updateUI() {

            textInput.removeClass("sp-validation-error");

            updateHelperLocations();

            // Update dragger background color (gradients take care of saturation and value).
            var flatColor = tinycolor.fromRatio({ h: currentHue, s: 1, v: 1 });
            dragger.css("background-color", flatColor.toHexString());

            // Get a format that alpha will be included in (hex and names ignore alpha)
            var format = currentPreferredFormat;
            if (currentAlpha < 1 && !(currentAlpha === 0 && format === "name")) {
                if (format === "hex" || format === "hex3" || format === "hex6" || format === "name") {
                    format = "rgb";
                }
            }

            var realColor = get({ format: format }),
                displayColor = '';

             //reset background info for preview element
            previewElement.removeClass("sp-clear-display");
            previewElement.css('background-color', 'transparent');

            if (!realColor && allowEmpty) {
                // Update the replaced elements background with icon indicating no color selection
                previewElement.addClass("sp-clear-display");
            }
            else {
                var realHex = realColor.toHexString(),
                    realRgb = realColor.toRgbString();

                // Update the replaced elements background color (with actual selected color)
                if (rgbaSupport || realColor.alpha === 1) {
                    previewElement.css("background-color", realRgb);
                }
                else {
                    previewElement.css("background-color", "transparent");
                    previewElement.css("filter", realColor.toFilter());
                }

                if (opts.showAlpha) {
                    var rgb = realColor.toRgb();
                    rgb.a = 0;
                    var realAlpha = tinycolor(rgb).toRgbString();
                    var gradient = "linear-gradient(left, " + realAlpha + ", " + realHex + ")";

                    if (IE) {
                        alphaSliderInner.css("filter", tinycolor(realAlpha).toFilter({ gradientType: 1 }, realHex));
                    }
                    else {
                        alphaSliderInner.css("background", "-webkit-" + gradient);
                        alphaSliderInner.css("background", "-moz-" + gradient);
                        alphaSliderInner.css("background", "-ms-" + gradient);
                        // Use current syntax gradient on unprefixed property.
                        alphaSliderInner.css("background",
                            "linear-gradient(to right, " + realAlpha + ", " + realHex + ")");
                    }
                }

                displayColor = realColor.toString(format);
            }

            // Update the text entry input as it changes happen
            if (opts.showInput) {
                textInput.val(displayColor);
            }

            if (opts.showPalette) {
                drawPalette();
            }

            drawInitial();
        }

        function updateHelperLocations() {
            var s = currentSaturation;
            var v = currentValue;

            if(allowEmpty && isEmpty) {
                //if selected color is empty, hide the helpers
                alphaSlideHelper.hide();
                slideHelper.hide();
                dragHelper.hide();
            }
            else {
                //make sure helpers are visible
                alphaSlideHelper.show();
                slideHelper.show();
                dragHelper.show();

                // Where to show the little circle in that displays your current selected color
                var dragX = s * dragWidth;
                var dragY = dragHeight - (v * dragHeight);
                dragX = Math.max(
                    -dragHelperHeight,
                    Math.min(dragWidth - dragHelperHeight, dragX - dragHelperHeight)
                );
                dragY = Math.max(
                    -dragHelperHeight,
                    Math.min(dragHeight - dragHelperHeight, dragY - dragHelperHeight)
                );
                dragHelper.css({
                    "top": dragY + "px",
                    "left": dragX + "px"					
                });

                var alphaX = currentAlpha * alphaWidth;
                alphaSlideHelper.css({
                    "left": (alphaX - (alphaSlideHelperWidth / 2)) + "px"
                });

                // Where to show the bar that displays your current selected hue
                var slideY = (currentHue) * slideHeight;
                slideHelper.css({
                    "top": (slideY - slideHelperHeight) + "px"
                });
            }
        }

        function updateOriginalInput(fireCallback) {
            var color = get(),
                displayColor = '',
                hasChanged = !tinycolor.equals(color, colorOnShow);

            if (color) {
                displayColor = color.toString(currentPreferredFormat);
                // Update the selection palette with the current color
                addColorToSelectionPalette(color);
            }

            if (isInput) {
                boundElement.val(displayColor);
            }

            if (fireCallback && hasChanged) {
                callbacks.change(color);
                boundElement.trigger('change', [ color ]);
            }
        }

        function reflow() {
            dragWidth = dragger.width();
            dragHeight = dragger.height();
            dragHelperHeight = dragHelper.height();
            slideWidth = slider.width();
            slideHeight = slider.height();
            slideHelperHeight = slideHelper.height();
            alphaWidth = alphaSlider.width();
            alphaSlideHelperWidth = alphaSlideHelper.width();

            if (!flat) {
                container.css("position", "absolute");
                if (opts.offset) {
                    container.offset(opts.offset);
                } else {
                    container.offset(getOffset(container, offsetElement));
                }
            }

            updateHelperLocations();

            if (opts.showPalette) {
                drawPalette();
            }

            boundElement.trigger('reflow.spectrum');
        }

        function destroy() {
            boundElement.show();
            offsetElement.unbind("click.spectrum touchstart.spectrum");
            container.remove();
            replacer.remove();
            spectrums[spect.id] = null;
        }

        function option(optionName, optionValue) {
            if (optionName === undefined) {
                return $.extend({}, opts);
            }
            if (optionValue === undefined) {
                return opts[optionName];
            }

            opts[optionName] = optionValue;
            applyOptions();
        }

        function enable() {
            disabled = false;
            boundElement.attr("disabled", false);
            offsetElement.removeClass("sp-disabled");
        }

        function disable() {
            hide();
            disabled = true;
            boundElement.attr("disabled", true);
            offsetElement.addClass("sp-disabled");
        }

        function setOffset(coord) {
            opts.offset = coord;
            reflow();
        }

        initialize();

        var spect = {
            show: show,
            hide: hide,
            toggle: toggle,
            reflow: reflow,
            option: option,
            enable: enable,
            disable: disable,
            offset: setOffset,
            set: function (c) {
                set(c);
                updateOriginalInput();
            },
            get: get,
            destroy: destroy,
            container: container
        };

        spect.id = spectrums.push(spect) - 1;

        return spect;
    }

    /**
    * checkOffset - get the offset below/above and left/right element depending on screen position
    * Thanks https://github.com/jquery/jquery-ui/blob/master/ui/jquery.ui.datepicker.js
    */
    function getOffset(picker, input) {
        var extraY = 0;
        var dpWidth = picker.outerWidth();
        var dpHeight = picker.outerHeight();
        var inputHeight = input.outerHeight();
        var doc = picker[0].ownerDocument;
        var docElem = doc.documentElement;
        var viewWidth = docElem.clientWidth + $(doc).scrollLeft();
        var viewHeight = docElem.clientHeight + $(doc).scrollTop();
        var offset = input.offset();
        offset.top += inputHeight;

        offset.left -=            
			Math.min(offset.left, (offset.left + dpWidth > viewWidth && viewWidth > dpWidth) ?
            Math.abs(offset.left + dpWidth - viewWidth) : 0) + 20;

        offset.top -=            
			Math.min(offset.top, ((offset.top + dpHeight > viewHeight && viewHeight > dpHeight) ?
            Math.abs(dpHeight + inputHeight - extraY) : extraY)) - 9;

        return offset;
    }

    /**
    * noop - do nothing
    */
    function noop() {

    }

    /**
    * stopPropagation - makes the code only doing this a little easier to read in line
    */
    function stopPropagation(e) {
        e.stopPropagation();
    }

    /**
    * Create a function bound to a given object
    * Thanks to underscore.js
    */
    function bind(func, obj) {
        var slice = Array.prototype.slice;
        var args = slice.call(arguments, 2);
        return function () {
            return func.apply(obj, args.concat(slice.call(arguments)));
        };
    }

    /**
    * Lightweight drag helper.  Handles containment within the element, so that
    * when dragging, the x is within [0,element.width] and y is within [0,element.height]
    */
    function draggable(element, onmove, onstart, onstop) {
        onmove = onmove || function () { };
        onstart = onstart || function () { };
        onstop = onstop || function () { };
        var doc = document;
        var dragging = false;
        var offset = {};
        var maxHeight = 0;
        var maxWidth = 0;
        var hasTouch = ('ontouchstart' in window);

        var duringDragEvents = {};
        duringDragEvents["selectstart"] = prevent;
        duringDragEvents["dragstart"] = prevent;
        duringDragEvents["touchmove mousemove"] = move;
        duringDragEvents["touchend mouseup"] = stop;

        function prevent(e) {
            if (e.stopPropagation) {
                e.stopPropagation();
            }
            if (e.preventDefault) {
                e.preventDefault();
            }
            e.returnValue = false;
        }

        function move(e) {
            if (dragging) {
                // Mouseup happened outside of window
                if (IE && doc.documentMode < 9 && !e.button) {
                    return stop();
                }

                var t0 = e.originalEvent && e.originalEvent.touches && e.originalEvent.touches[0];
                var pageX = t0 && t0.pageX || e.pageX;
                var pageY = t0 && t0.pageY || e.pageY;

                var dragX = Math.max(0, Math.min(pageX - offset.left, maxWidth));
                var dragY = Math.max(0, Math.min(pageY - offset.top, maxHeight));

                if (hasTouch) {
                    // Stop scrolling in iOS
                    prevent(e);
                }

                onmove.apply(element, [dragX, dragY, e]);
            }
        }

        function start(e) {
            var rightclick = (e.which) ? (e.which == 3) : (e.button == 2);

            if (!rightclick && !dragging) {
                if (onstart.apply(element, arguments) !== false) {
                    dragging = true;
                    maxHeight = $(element).height();
                    maxWidth = $(element).width();
                    offset = $(element).offset();

                    $(doc).bind(duringDragEvents);
                    $(doc.body).addClass("sp-dragging");

                    move(e);

                    prevent(e);
                }
            }
        }

        function stop() {
            if (dragging) {
                $(doc).unbind(duringDragEvents);
                $(doc.body).removeClass("sp-dragging");

                // Wait a tick before notifying observers to allow the click event
                // to fire in Chrome.
                setTimeout(function() {
                    onstop.apply(element, arguments);
                }, 0);
            }
            dragging = false;
        }

        $(element).bind("touchstart mousedown", start);
    }

    function throttle(func, wait, debounce) {
        var timeout;
        return function () {
            var context = this, args = arguments;
            var throttler = function () {
                timeout = null;
                func.apply(context, args);
            };
            if (debounce) clearTimeout(timeout);
            if (debounce || !timeout) timeout = setTimeout(throttler, wait);
        };
    }

    function inputTypeColorSupport() {
        return $.fn.spectrum.inputTypeColorSupport();
    }

    /**
    * Define a jQuery plugin
    */
    var dataID = "spectrum.id";
    $.fn.spectrum = function (opts, extra) {

        if (typeof opts == "string") {

            var returnValue = this;
            var args = Array.prototype.slice.call( arguments, 1 );

            this.each(function () {
                var spect = spectrums[$(this).data(dataID)];
                if (spect) {
                    var method = spect[opts];
                    if (!method) {
                        throw new Error( "Spectrum: no such method: '" + opts + "'" );
                    }

                    if (opts == "get") {
                        returnValue = spect.get();
                    }
                    else if (opts == "container") {
                        returnValue = spect.container;
                    }
                    else if (opts == "option") {
                        returnValue = spect.option.apply(spect, args);
                    }
                    else if (opts == "destroy") {
                        spect.destroy();
                        $(this).removeData(dataID);
                    }
                    else {
                        method.apply(spect, args);
                    }
                }
            });

            return returnValue;
        }

        // Initializing a new instance of spectrum
        return this.spectrum("destroy").each(function () {
            var options = $.extend({}, opts, $(this).data());
            var spect = spectrum(this, options);
            $(this).data(dataID, spect.id);
        });
    };

    $.fn.spectrum.load = true;
    $.fn.spectrum.loadOpts = {};
    $.fn.spectrum.draggable = draggable;
    $.fn.spectrum.defaults = defaultOpts;
    $.fn.spectrum.inputTypeColorSupport = function inputTypeColorSupport() {
        if (typeof inputTypeColorSupport._cachedResult === "undefined") {
            var colorInput = $("<input type='color'/>")[0]; // if color element is supported, value will default to not null
            inputTypeColorSupport._cachedResult = colorInput.type === "color" && colorInput.value !== "";
        }
        return inputTypeColorSupport._cachedResult;
    };

    $.spectrum = { };
    $.spectrum.localization = { };
    $.spectrum.palettes = { };

    $.fn.spectrum.processNativeColorInputs = function () {
        var colorInputs = $("input[type=color]");
        if (colorInputs.length && !inputTypeColorSupport()) {
            colorInputs.spectrum({
                preferredFormat: "hex6"
            });
        }
    };

    // TinyColor v1.1.2
    // https://github.com/bgrins/TinyColor
    // Brian Grinstead, MIT License

    (function() {

    var trimLeft = /^[\s,#]+/,
        trimRight = /\s+$/,
        tinyCounter = 0,
        math = Math,
        mathRound = math.round,
        mathMin = math.min,
        mathMax = math.max,
        mathRandom = math.random;

    var tinycolor = function(color, opts) {

        color = (color) ? color : '';
        opts = opts || { };

        // If input is already a tinycolor, return itself
        if (color instanceof tinycolor) {
           return color;
        }
        // If we are called as a function, call using new instead
        if (!(this instanceof tinycolor)) {
            return new tinycolor(color, opts);
        }

        var rgb = inputToRGB(color);
        this._originalInput = color,
        this._r = rgb.r,
        this._g = rgb.g,
        this._b = rgb.b,
        this._a = rgb.a,
        this._roundA = mathRound(100*this._a) / 100,
        this._format = opts.format || rgb.format;
        this._gradientType = opts.gradientType;

        // Don't let the range of [0,255] come back in [0,1].
        // Potentially lose a little bit of precision here, but will fix issues where
        // .5 gets interpreted as half of the total, instead of half of 1
        // If it was supposed to be 128, this was already taken care of by `inputToRgb`
        if (this._r < 1) { this._r = mathRound(this._r); }
        if (this._g < 1) { this._g = mathRound(this._g); }
        if (this._b < 1) { this._b = mathRound(this._b); }

        this._ok = rgb.ok;
        this._tc_id = tinyCounter++;
    };

    tinycolor.prototype = {
        isDark: function() {
            return this.getBrightness() < 128;
        },
        isLight: function() {
            return !this.isDark();
        },
        isValid: function() {
            return this._ok;
        },
        getOriginalInput: function() {
          return this._originalInput;
        },
        getFormat: function() {
            return this._format;
        },
        getAlpha: function() {
            return this._a;
        },
        getBrightness: function() {
            var rgb = this.toRgb();
            return (rgb.r * 299 + rgb.g * 587 + rgb.b * 114) / 1000;
        },
        setAlpha: function(value) {
            this._a = boundAlpha(value);
            this._roundA = mathRound(100*this._a) / 100;
            return this;
        },
        toHsv: function() {
            var hsv = rgbToHsv(this._r, this._g, this._b);
            return { h: hsv.h * 360, s: hsv.s, v: hsv.v, a: this._a };
        },
        toHsvString: function() {
            var hsv = rgbToHsv(this._r, this._g, this._b);
            var h = mathRound(hsv.h * 360), s = mathRound(hsv.s * 100), v = mathRound(hsv.v * 100);
            return (this._a == 1) ?
              "hsv("  + h + ", " + s + "%, " + v + "%)" :
              "hsva(" + h + ", " + s + "%, " + v + "%, "+ this._roundA + ")";
        },
        toHsl: function() {
            var hsl = rgbToHsl(this._r, this._g, this._b);
            return { h: hsl.h * 360, s: hsl.s, l: hsl.l, a: this._a };
        },
        toHslString: function() {
            var hsl = rgbToHsl(this._r, this._g, this._b);
            var h = mathRound(hsl.h * 360), s = mathRound(hsl.s * 100), l = mathRound(hsl.l * 100);
            return (this._a == 1) ?
              "hsl("  + h + ", " + s + "%, " + l + "%)" :
              "hsla(" + h + ", " + s + "%, " + l + "%, "+ this._roundA + ")";
        },
        toHex: function(allow3Char) {
            return rgbToHex(this._r, this._g, this._b, allow3Char);
        },
        toHexString: function(allow3Char) {
            return '#' + this.toHex(allow3Char);
        },
        toHex8: function() {
            return rgbaToHex(this._r, this._g, this._b, this._a);
        },
        toHex8String: function() {
            return '#' + this.toHex8();
        },
        toRgb: function() {
            return { r: mathRound(this._r), g: mathRound(this._g), b: mathRound(this._b), a: this._a };
        },
        toRgbString: function() {
            return (this._a == 1) ?
              "rgb("  + mathRound(this._r) + ", " + mathRound(this._g) + ", " + mathRound(this._b) + ")" :
              "rgba(" + mathRound(this._r) + ", " + mathRound(this._g) + ", " + mathRound(this._b) + ", " + this._roundA + ")";
        },
        toPercentageRgb: function() {
            return { r: mathRound(bound01(this._r, 255) * 100) + "%", g: mathRound(bound01(this._g, 255) * 100) + "%", b: mathRound(bound01(this._b, 255) * 100) + "%", a: this._a };
        },
        toPercentageRgbString: function() {
            return (this._a == 1) ?
              "rgb("  + mathRound(bound01(this._r, 255) * 100) + "%, " + mathRound(bound01(this._g, 255) * 100) + "%, " + mathRound(bound01(this._b, 255) * 100) + "%)" :
              "rgba(" + mathRound(bound01(this._r, 255) * 100) + "%, " + mathRound(bound01(this._g, 255) * 100) + "%, " + mathRound(bound01(this._b, 255) * 100) + "%, " + this._roundA + ")";
        },
        toName: function() {
            if (this._a === 0) {
                return "transparent";
            }

            if (this._a < 1) {
                return false;
            }

            return hexNames[rgbToHex(this._r, this._g, this._b, true)] || false;
        },
        toFilter: function(secondColor) {
            var hex8String = '#' + rgbaToHex(this._r, this._g, this._b, this._a);
            var secondHex8String = hex8String;
            var gradientType = this._gradientType ? "GradientType = 1, " : "";

            if (secondColor) {
                var s = tinycolor(secondColor);
                secondHex8String = s.toHex8String();
            }

            return "progid:DXImageTransform.Microsoft.gradient("+gradientType+"startColorstr="+hex8String+",endColorstr="+secondHex8String+")";
        },
        toString: function(format) {
            var formatSet = !!format;
            format = format || this._format;

            var formattedString = false;
            var hasAlpha = this._a < 1 && this._a >= 0;
            var needsAlphaFormat = !formatSet && hasAlpha && (format === "hex" || format === "hex6" || format === "hex3" || format === "name");

            if (needsAlphaFormat) {
                // Special case for "transparent", all other non-alpha formats
                // will return rgba when there is transparency.
                if (format === "name" && this._a === 0) {
                    return this.toName();
                }
                return this.toRgbString();
            }
            if (format === "rgb") {
                formattedString = this.toRgbString();
            }
            if (format === "prgb") {
                formattedString = this.toPercentageRgbString();
            }
            if (format === "hex" || format === "hex6") {
                formattedString = this.toHexString();
            }
            if (format === "hex3") {
                formattedString = this.toHexString(true);
            }
            if (format === "hex8") {
                formattedString = this.toHex8String();
            }
            if (format === "name") {
                formattedString = this.toName();
            }
            if (format === "hsl") {
                formattedString = this.toHslString();
            }
            if (format === "hsv") {
                formattedString = this.toHsvString();
            }

            return formattedString || this.toHexString();
        },

        _applyModification: function(fn, args) {
            var color = fn.apply(null, [this].concat([].slice.call(args)));
            this._r = color._r;
            this._g = color._g;
            this._b = color._b;
            this.setAlpha(color._a);
            return this;
        },
        lighten: function() {
            return this._applyModification(lighten, arguments);
        },
        brighten: function() {
            return this._applyModification(brighten, arguments);
        },
        darken: function() {
            return this._applyModification(darken, arguments);
        },
        desaturate: function() {
            return this._applyModification(desaturate, arguments);
        },
        saturate: function() {
            return this._applyModification(saturate, arguments);
        },
        greyscale: function() {
            return this._applyModification(greyscale, arguments);
        },
        spin: function() {
            return this._applyModification(spin, arguments);
        },

        _applyCombination: function(fn, args) {
            return fn.apply(null, [this].concat([].slice.call(args)));
        },
        analogous: function() {
            return this._applyCombination(analogous, arguments);
        },
        complement: function() {
            return this._applyCombination(complement, arguments);
        },
        monochromatic: function() {
            return this._applyCombination(monochromatic, arguments);
        },
        splitcomplement: function() {
            return this._applyCombination(splitcomplement, arguments);
        },
        triad: function() {
            return this._applyCombination(triad, arguments);
        },
        tetrad: function() {
            return this._applyCombination(tetrad, arguments);
        }
    };

    // If input is an object, force 1 into "1.0" to handle ratios properly
    // String input requires "1.0" as input, so 1 will be treated as 1
    tinycolor.fromRatio = function(color, opts) {
        if (typeof color == "object") {
            var newColor = {};
            for (var i in color) {
                if (color.hasOwnProperty(i)) {
                    if (i === "a") {
                        newColor[i] = color[i];
                    }
                    else {
                        newColor[i] = convertToPercentage(color[i]);
                    }
                }
            }
            color = newColor;
        }

        return tinycolor(color, opts);
    };

    // Given a string or object, convert that input to RGB
    // Possible string inputs:
    //
    //     "red"
    //     "#f00" or "f00"
    //     "#ff0000" or "ff0000"
    //     "#ff000000" or "ff000000"
    //     "rgb 255 0 0" or "rgb (255, 0, 0)"
    //     "rgb 1.0 0 0" or "rgb (1, 0, 0)"
    //     "rgba (255, 0, 0, 1)" or "rgba 255, 0, 0, 1"
    //     "rgba (1.0, 0, 0, 1)" or "rgba 1.0, 0, 0, 1"
    //     "hsl(0, 100%, 50%)" or "hsl 0 100% 50%"
    //     "hsla(0, 100%, 50%, 1)" or "hsla 0 100% 50%, 1"
    //     "hsv(0, 100%, 100%)" or "hsv 0 100% 100%"
    //
    function inputToRGB(color) {

        var rgb = { r: 0, g: 0, b: 0 };
        var a = 1;
        var ok = false;
        var format = false;

        if (typeof color == "string") {
            color = stringInputToObject(color);
        }

        if (typeof color == "object") {
            if (color.hasOwnProperty("r") && color.hasOwnProperty("g") && color.hasOwnProperty("b")) {
                rgb = rgbToRgb(color.r, color.g, color.b);
                ok = true;
                format = String(color.r).substr(-1) === "%" ? "prgb" : "rgb";
            }
            else if (color.hasOwnProperty("h") && color.hasOwnProperty("s") && color.hasOwnProperty("v")) {
                color.s = convertToPercentage(color.s);
                color.v = convertToPercentage(color.v);
                rgb = hsvToRgb(color.h, color.s, color.v);
                ok = true;
                format = "hsv";
            }
            else if (color.hasOwnProperty("h") && color.hasOwnProperty("s") && color.hasOwnProperty("l")) {
                color.s = convertToPercentage(color.s);
                color.l = convertToPercentage(color.l);
                rgb = hslToRgb(color.h, color.s, color.l);
                ok = true;
                format = "hsl";
            }

            if (color.hasOwnProperty("a")) {
                a = color.a;
            }
        }

        a = boundAlpha(a);

        return {
            ok: ok,
            format: color.format || format,
            r: mathMin(255, mathMax(rgb.r, 0)),
            g: mathMin(255, mathMax(rgb.g, 0)),
            b: mathMin(255, mathMax(rgb.b, 0)),
            a: a
        };
    }


    // Conversion Functions
    // --------------------

    // `rgbToHsl`, `rgbToHsv`, `hslToRgb`, `hsvToRgb` modified from:
    // <http://mjijackson.com/2008/02/rgb-to-hsl-and-rgb-to-hsv-color-model-conversion-algorithms-in-javascript>

    // `rgbToRgb`
    // Handle bounds / percentage checking to conform to CSS color spec
    // <http://www.w3.org/TR/css3-color/>
    // *Assumes:* r, g, b in [0, 255] or [0, 1]
    // *Returns:* { r, g, b } in [0, 255]
    function rgbToRgb(r, g, b){
        return {
            r: bound01(r, 255) * 255,
            g: bound01(g, 255) * 255,
            b: bound01(b, 255) * 255
        };
    }

    // `rgbToHsl`
    // Converts an RGB color value to HSL.
    // *Assumes:* r, g, and b are contained in [0, 255] or [0, 1]
    // *Returns:* { h, s, l } in [0,1]
    function rgbToHsl(r, g, b) {

        r = bound01(r, 255);
        g = bound01(g, 255);
        b = bound01(b, 255);

        var max = mathMax(r, g, b), min = mathMin(r, g, b);
        var h, s, l = (max + min) / 2;

        if(max == min) {
            h = s = 0; // achromatic
        }
        else {
            var d = max - min;
            s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
            switch(max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }

            h /= 6;
        }

        return { h: h, s: s, l: l };
    }

    // `hslToRgb`
    // Converts an HSL color value to RGB.
    // *Assumes:* h is contained in [0, 1] or [0, 360] and s and l are contained [0, 1] or [0, 100]
    // *Returns:* { r, g, b } in the set [0, 255]
    function hslToRgb(h, s, l) {
        var r, g, b;

        h = bound01(h, 360);
        s = bound01(s, 100);
        l = bound01(l, 100);

        function hue2rgb(p, q, t) {
            if(t < 0) t += 1;
            if(t > 1) t -= 1;
            if(t < 1/6) return p + (q - p) * 6 * t;
            if(t < 1/2) return q;
            if(t < 2/3) return p + (q - p) * (2/3 - t) * 6;
            return p;
        }

        if(s === 0) {
            r = g = b = l; // achromatic
        }
        else {
            var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
            var p = 2 * l - q;
            r = hue2rgb(p, q, h + 1/3);
            g = hue2rgb(p, q, h);
            b = hue2rgb(p, q, h - 1/3);
        }

        return { r: r * 255, g: g * 255, b: b * 255 };
    }

    // `rgbToHsv`
    // Converts an RGB color value to HSV
    // *Assumes:* r, g, and b are contained in the set [0, 255] or [0, 1]
    // *Returns:* { h, s, v } in [0,1]
    function rgbToHsv(r, g, b) {

        r = bound01(r, 255);
        g = bound01(g, 255);
        b = bound01(b, 255);

        var max = mathMax(r, g, b), min = mathMin(r, g, b);
        var h, s, v = max;

        var d = max - min;
        s = max === 0 ? 0 : d / max;

        if(max == min) {
            h = 0; // achromatic
        }
        else {
            switch(max) {
                case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                case g: h = (b - r) / d + 2; break;
                case b: h = (r - g) / d + 4; break;
            }
            h /= 6;
        }
        return { h: h, s: s, v: v };
    }

    // `hsvToRgb`
    // Converts an HSV color value to RGB.
    // *Assumes:* h is contained in [0, 1] or [0, 360] and s and v are contained in [0, 1] or [0, 100]
    // *Returns:* { r, g, b } in the set [0, 255]
     function hsvToRgb(h, s, v) {

        h = bound01(h, 360) * 6;
        s = bound01(s, 100);
        v = bound01(v, 100);

        var i = math.floor(h),
            f = h - i,
            p = v * (1 - s),
            q = v * (1 - f * s),
            t = v * (1 - (1 - f) * s),
            mod = i % 6,
            r = [v, q, p, p, t, v][mod],
            g = [t, v, v, q, p, p][mod],
            b = [p, p, t, v, v, q][mod];

        return { r: r * 255, g: g * 255, b: b * 255 };
    }

    // `rgbToHex`
    // Converts an RGB color to hex
    // Assumes r, g, and b are contained in the set [0, 255]
    // Returns a 3 or 6 character hex
    function rgbToHex(r, g, b, allow3Char) {

        var hex = [
            pad2(mathRound(r).toString(16)),
            pad2(mathRound(g).toString(16)),
            pad2(mathRound(b).toString(16))
        ];

        // Return a 3 character hex if possible
        if (allow3Char && hex[0].charAt(0) == hex[0].charAt(1) && hex[1].charAt(0) == hex[1].charAt(1) && hex[2].charAt(0) == hex[2].charAt(1)) {
            return hex[0].charAt(0) + hex[1].charAt(0) + hex[2].charAt(0);
        }

        return hex.join("");
    }
        // `rgbaToHex`
        // Converts an RGBA color plus alpha transparency to hex
        // Assumes r, g, b and a are contained in the set [0, 255]
        // Returns an 8 character hex
        function rgbaToHex(r, g, b, a) {

            var hex = [
                pad2(convertDecimalToHex(a)),
                pad2(mathRound(r).toString(16)),
                pad2(mathRound(g).toString(16)),
                pad2(mathRound(b).toString(16))
            ];

            return hex.join("");
        }

    // `equals`
    // Can be called with any tinycolor input
    tinycolor.equals = function (color1, color2) {
        if (!color1 || !color2) { return false; }
        return tinycolor(color1).toRgbString() == tinycolor(color2).toRgbString();
    };
    tinycolor.random = function() {
        return tinycolor.fromRatio({
            r: mathRandom(),
            g: mathRandom(),
            b: mathRandom()
        });
    };


    // Modification Functions
    // ----------------------
    // Thanks to less.js for some of the basics here
    // <https://github.com/cloudhead/less.js/blob/master/lib/less/functions.js>

    function desaturate(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.s -= amount / 100;
        hsl.s = clamp01(hsl.s);
        return tinycolor(hsl);
    }

    function saturate(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.s += amount / 100;
        hsl.s = clamp01(hsl.s);
        return tinycolor(hsl);
    }

    function greyscale(color) {
        return tinycolor(color).desaturate(100);
    }

    function lighten (color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.l += amount / 100;
        hsl.l = clamp01(hsl.l);
        return tinycolor(hsl);
    }

    function brighten(color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var rgb = tinycolor(color).toRgb();
        rgb.r = mathMax(0, mathMin(255, rgb.r - mathRound(255 * - (amount / 100))));
        rgb.g = mathMax(0, mathMin(255, rgb.g - mathRound(255 * - (amount / 100))));
        rgb.b = mathMax(0, mathMin(255, rgb.b - mathRound(255 * - (amount / 100))));
        return tinycolor(rgb);
    }

    function darken (color, amount) {
        amount = (amount === 0) ? 0 : (amount || 10);
        var hsl = tinycolor(color).toHsl();
        hsl.l -= amount / 100;
        hsl.l = clamp01(hsl.l);
        return tinycolor(hsl);
    }

    // Spin takes a positive or negative amount within [-360, 360] indicating the change of hue.
    // Values outside of this range will be wrapped into this range.
    function spin(color, amount) {
        var hsl = tinycolor(color).toHsl();
        var hue = (mathRound(hsl.h) + amount) % 360;
        hsl.h = hue < 0 ? 360 + hue : hue;
        return tinycolor(hsl);
    }

    // Combination Functions
    // ---------------------
    // Thanks to jQuery xColor for some of the ideas behind these
    // <https://github.com/infusion/jQuery-xcolor/blob/master/jquery.xcolor.js>

    function complement(color) {
        var hsl = tinycolor(color).toHsl();
        hsl.h = (hsl.h + 180) % 360;
        return tinycolor(hsl);
    }

    function triad(color) {
        var hsl = tinycolor(color).toHsl();
        var h = hsl.h;
        return [
            tinycolor(color),
            tinycolor({ h: (h + 120) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 240) % 360, s: hsl.s, l: hsl.l })
        ];
    }

    function tetrad(color) {
        var hsl = tinycolor(color).toHsl();
        var h = hsl.h;
        return [
            tinycolor(color),
            tinycolor({ h: (h + 90) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 180) % 360, s: hsl.s, l: hsl.l }),
            tinycolor({ h: (h + 270) % 360, s: hsl.s, l: hsl.l })
        ];
    }

    function splitcomplement(color) {
        var hsl = tinycolor(color).toHsl();
        var h = hsl.h;
        return [
            tinycolor(color),
            tinycolor({ h: (h + 72) % 360, s: hsl.s, l: hsl.l}),
            tinycolor({ h: (h + 216) % 360, s: hsl.s, l: hsl.l})
        ];
    }

    function analogous(color, results, slices) {
        results = results || 6;
        slices = slices || 30;

        var hsl = tinycolor(color).toHsl();
        var part = 360 / slices;
        var ret = [tinycolor(color)];

        for (hsl.h = ((hsl.h - (part * results >> 1)) + 720) % 360; --results; ) {
            hsl.h = (hsl.h + part) % 360;
            ret.push(tinycolor(hsl));
        }
        return ret;
    }

    function monochromatic(color, results) {
        results = results || 6;
        var hsv = tinycolor(color).toHsv();
        var h = hsv.h, s = hsv.s, v = hsv.v;
        var ret = [];
        var modification = 1 / results;

        while (results--) {
            ret.push(tinycolor({ h: h, s: s, v: v}));
            v = (v + modification) % 1;
        }

        return ret;
    }

    // Utility Functions
    // ---------------------

    tinycolor.mix = function(color1, color2, amount) {
        amount = (amount === 0) ? 0 : (amount || 50);

        var rgb1 = tinycolor(color1).toRgb();
        var rgb2 = tinycolor(color2).toRgb();

        var p = amount / 100;
        var w = p * 2 - 1;
        var a = rgb2.a - rgb1.a;

        var w1;

        if (w * a == -1) {
            w1 = w;
        } else {
            w1 = (w + a) / (1 + w * a);
        }

        w1 = (w1 + 1) / 2;

        var w2 = 1 - w1;

        var rgba = {
            r: rgb2.r * w1 + rgb1.r * w2,
            g: rgb2.g * w1 + rgb1.g * w2,
            b: rgb2.b * w1 + rgb1.b * w2,
            a: rgb2.a * p  + rgb1.a * (1 - p)
        };

        return tinycolor(rgba);
    };


    // Readability Functions
    // ---------------------
    // <http://www.w3.org/TR/AERT#color-contrast>

    // `readability`
    // Analyze the 2 colors and returns an object with the following properties:
    //    `brightness`: difference in brightness between the two colors
    //    `color`: difference in color/hue between the two colors
    tinycolor.readability = function(color1, color2) {
        var c1 = tinycolor(color1);
        var c2 = tinycolor(color2);
        var rgb1 = c1.toRgb();
        var rgb2 = c2.toRgb();
        var brightnessA = c1.getBrightness();
        var brightnessB = c2.getBrightness();
        var colorDiff = (
            Math.max(rgb1.r, rgb2.r) - Math.min(rgb1.r, rgb2.r) +
            Math.max(rgb1.g, rgb2.g) - Math.min(rgb1.g, rgb2.g) +
            Math.max(rgb1.b, rgb2.b) - Math.min(rgb1.b, rgb2.b)
        );

        return {
            brightness: Math.abs(brightnessA - brightnessB),
            color: colorDiff
        };
    };

    // `readable`
    // http://www.w3.org/TR/AERT#color-contrast
    // Ensure that foreground and background color combinations provide sufficient contrast.
    // *Example*
    //    tinycolor.isReadable("#000", "#111") => false
    tinycolor.isReadable = function(color1, color2) {
        var readability = tinycolor.readability(color1, color2);
        return readability.brightness > 125 && readability.color > 500;
    };

    // `mostReadable`
    // Given a base color and a list of possible foreground or background
    // colors for that base, returns the most readable color.
    // *Example*
    //    tinycolor.mostReadable("#123", ["#fff", "#000"]) => "#000"
    tinycolor.mostReadable = function(baseColor, colorList) {
        var bestColor = null;
        var bestScore = 0;
        var bestIsReadable = false;
        for (var i=0; i < colorList.length; i++) {

            // We normalize both around the "acceptable" breaking point,
            // but rank brightness constrast higher than hue.

            var readability = tinycolor.readability(baseColor, colorList[i]);
            var readable = readability.brightness > 125 && readability.color > 500;
            var score = 3 * (readability.brightness / 125) + (readability.color / 500);

            if ((readable && ! bestIsReadable) ||
                (readable && bestIsReadable && score > bestScore) ||
                ((! readable) && (! bestIsReadable) && score > bestScore)) {
                bestIsReadable = readable;
                bestScore = score;
                bestColor = tinycolor(colorList[i]);
            }
        }
        return bestColor;
    };


    // Big List of Colors
    // ------------------
    // <http://www.w3.org/TR/css3-color/#svg-color>
    var names = tinycolor.names = {
        aliceblue: "f0f8ff",
        antiquewhite: "faebd7",
        aqua: "0ff",
        aquamarine: "7fffd4",
        azure: "f0ffff",
        beige: "f5f5dc",
        bisque: "ffe4c4",
        black: "000",
        blanchedalmond: "ffebcd",
        blue: "00f",
        blueviolet: "8a2be2",
        brown: "a52a2a",
        burlywood: "deb887",
        burntsienna: "ea7e5d",
        cadetblue: "5f9ea0",
        chartreuse: "7fff00",
        chocolate: "d2691e",
        coral: "ff7f50",
        cornflowerblue: "6495ed",
        cornsilk: "fff8dc",
        crimson: "dc143c",
        cyan: "0ff",
        darkblue: "00008b",
        darkcyan: "008b8b",
        darkgoldenrod: "b8860b",
        darkgray: "a9a9a9",
        darkgreen: "006400",
        darkgrey: "a9a9a9",
        darkkhaki: "bdb76b",
        darkmagenta: "8b008b",
        darkolivegreen: "556b2f",
        darkorange: "ff8c00",
        darkorchid: "9932cc",
        darkred: "8b0000",
        darksalmon: "e9967a",
        darkseagreen: "8fbc8f",
        darkslateblue: "483d8b",
        darkslategray: "2f4f4f",
        darkslategrey: "2f4f4f",
        darkturquoise: "00ced1",
        darkviolet: "9400d3",
        deeppink: "ff1493",
        deepskyblue: "00bfff",
        dimgray: "696969",
        dimgrey: "696969",
        dodgerblue: "1e90ff",
        firebrick: "b22222",
        floralwhite: "fffaf0",
        forestgreen: "228b22",
        fuchsia: "f0f",
        gainsboro: "dcdcdc",
        ghostwhite: "f8f8ff",
        gold: "ffd700",
        goldenrod: "daa520",
        gray: "808080",
        green: "008000",
        greenyellow: "adff2f",
        grey: "808080",
        honeydew: "f0fff0",
        hotpink: "ff69b4",
        indianred: "cd5c5c",
        indigo: "4b0082",
        ivory: "fffff0",
        khaki: "f0e68c",
        lavender: "e6e6fa",
        lavenderblush: "fff0f5",
        lawngreen: "7cfc00",
        lemonchiffon: "fffacd",
        lightblue: "add8e6",
        lightcoral: "f08080",
        lightcyan: "e0ffff",
        lightgoldenrodyellow: "fafad2",
        lightgray: "d3d3d3",
        lightgreen: "90ee90",
        lightgrey: "d3d3d3",
        lightpink: "ffb6c1",
        lightsalmon: "ffa07a",
        lightseagreen: "20b2aa",
        lightskyblue: "87cefa",
        lightslategray: "789",
        lightslategrey: "789",
        lightsteelblue: "b0c4de",
        lightyellow: "ffffe0",
        lime: "0f0",
        limegreen: "32cd32",
        linen: "faf0e6",
        magenta: "f0f",
        maroon: "800000",
        mediumaquamarine: "66cdaa",
        mediumblue: "0000cd",
        mediumorchid: "ba55d3",
        mediumpurple: "9370db",
        mediumseagreen: "3cb371",
        mediumslateblue: "7b68ee",
        mediumspringgreen: "00fa9a",
        mediumturquoise: "48d1cc",
        mediumvioletred: "c71585",
        midnightblue: "191970",
        mintcream: "f5fffa",
        mistyrose: "ffe4e1",
        moccasin: "ffe4b5",
        navajowhite: "ffdead",
        navy: "000080",
        oldlace: "fdf5e6",
        olive: "808000",
        olivedrab: "6b8e23",
        orange: "ffa500",
        orangered: "ff4500",
        orchid: "da70d6",
        palegoldenrod: "eee8aa",
        palegreen: "98fb98",
        paleturquoise: "afeeee",
        palevioletred: "db7093",
        papayawhip: "ffefd5",
        peachpuff: "ffdab9",
        peru: "cd853f",
        pink: "ffc0cb",
        plum: "dda0dd",
        powderblue: "b0e0e6",
        purple: "800080",
        rebeccapurple: "663399",
        red: "f00",
        rosybrown: "bc8f8f",
        royalblue: "4169e1",
        saddlebrown: "8b4513",
        salmon: "fa8072",
        sandybrown: "f4a460",
        seagreen: "2e8b57",
        seashell: "fff5ee",
        sienna: "a0522d",
        silver: "c0c0c0",
        skyblue: "87ceeb",
        slateblue: "6a5acd",
        slategray: "708090",
        slategrey: "708090",
        snow: "fffafa",
        springgreen: "00ff7f",
        steelblue: "4682b4",
        tan: "d2b48c",
        teal: "008080",
        thistle: "d8bfd8",
        tomato: "ff6347",
        turquoise: "40e0d0",
        violet: "ee82ee",
        wheat: "f5deb3",
        white: "fff",
        whitesmoke: "f5f5f5",
        yellow: "ff0",
        yellowgreen: "9acd32"
    };

    // Make it easy to access colors via `hexNames[hex]`
    var hexNames = tinycolor.hexNames = flip(names);


    // Utilities
    // ---------

    // `{ 'name1': 'val1' }` becomes `{ 'val1': 'name1' }`
    function flip(o) {
        var flipped = { };
        for (var i in o) {
            if (o.hasOwnProperty(i)) {
                flipped[o[i]] = i;
            }
        }
        return flipped;
    }

    // Return a valid alpha value [0,1] with all invalid values being set to 1
    function boundAlpha(a) {
        a = parseFloat(a);

        if (isNaN(a) || a < 0 || a > 1) {
            a = 1;
        }

        return a;
    }

    // Take input from [0, n] and return it as [0, 1]
    function bound01(n, max) {
        if (isOnePointZero(n)) { n = "100%"; }

        var processPercent = isPercentage(n);
        n = mathMin(max, mathMax(0, parseFloat(n)));

        // Automatically convert percentage into number
        if (processPercent) {
            n = parseInt(n * max, 10) / 100;
        }

        // Handle floating point rounding errors
        if ((math.abs(n - max) < 0.000001)) {
            return 1;
        }

        // Convert into [0, 1] range if it isn't already
        return (n % max) / parseFloat(max);
    }

    // Force a number between 0 and 1
    function clamp01(val) {
        return mathMin(1, mathMax(0, val));
    }

    // Parse a base-16 hex value into a base-10 integer
    function parseIntFromHex(val) {
        return parseInt(val, 16);
    }

    // Need to handle 1.0 as 100%, since once it is a number, there is no difference between it and 1
    // <http://stackoverflow.com/questions/7422072/javascript-how-to-detect-number-as-a-decimal-including-1-0>
    function isOnePointZero(n) {
        return typeof n == "string" && n.indexOf('.') != -1 && parseFloat(n) === 1;
    }

    // Check to see if string passed in is a percentage
    function isPercentage(n) {
        return typeof n === "string" && n.indexOf('%') != -1;
    }

    // Force a hex value to have 2 characters
    function pad2(c) {
        return c.length == 1 ? '0' + c : '' + c;
    }

    // Replace a decimal with it's percentage value
    function convertToPercentage(n) {
        if (n <= 1) {
            n = (n * 100) + "%";
        }

        return n;
    }

    // Converts a decimal to a hex value
    function convertDecimalToHex(d) {
        return Math.round(parseFloat(d) * 255).toString(16);
    }
    // Converts a hex value to a decimal
    function convertHexToDecimal(h) {
        return (parseIntFromHex(h) / 255);
    }

    var matchers = (function() {

        // <http://www.w3.org/TR/css3-values/#integers>
        var CSS_INTEGER = "[-\\+]?\\d+%?";

        // <http://www.w3.org/TR/css3-values/#number-value>
        var CSS_NUMBER = "[-\\+]?\\d*\\.\\d+%?";

        // Allow positive/negative integer/number.  Don't capture the either/or, just the entire outcome.
        var CSS_UNIT = "(?:" + CSS_NUMBER + ")|(?:" + CSS_INTEGER + ")";

        // Actual matching.
        // Parentheses and commas are optional, but not required.
        // Whitespace can take the place of commas or opening paren
        var PERMISSIVE_MATCH3 = "[\\s|\\(]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")\\s*\\)?";
        var PERMISSIVE_MATCH4 = "[\\s|\\(]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")[,|\\s]+(" + CSS_UNIT + ")\\s*\\)?";

        return {
            rgb: new RegExp("rgb" + PERMISSIVE_MATCH3),
            rgba: new RegExp("rgba" + PERMISSIVE_MATCH4),
            hsl: new RegExp("hsl" + PERMISSIVE_MATCH3),
            hsla: new RegExp("hsla" + PERMISSIVE_MATCH4),
            hsv: new RegExp("hsv" + PERMISSIVE_MATCH3),
            hsva: new RegExp("hsva" + PERMISSIVE_MATCH4),
            hex3: /^([0-9a-fA-F]{1})([0-9a-fA-F]{1})([0-9a-fA-F]{1})$/,
            hex6: /^([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/,
            hex8: /^([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})([0-9a-fA-F]{2})$/
        };
    })();

    // `stringInputToObject`
    // Permissive string parsing.  Take in a number of formats, and output an object
    // based on detected format.  Returns `{ r, g, b }` or `{ h, s, l }` or `{ h, s, v}`
    function stringInputToObject(color) {

        color = color.replace(trimLeft,'').replace(trimRight, '').toLowerCase();
        var named = false;
        if (names[color]) {
            color = names[color];
            named = true;
        }
        else if (color == 'transparent') {
            return { r: 0, g: 0, b: 0, a: 0, format: "name" };
        }

        // Try to match string input using regular expressions.
        // Keep most of the number bounding out of this function - don't worry about [0,1] or [0,100] or [0,360]
        // Just return an object and let the conversion functions handle that.
        // This way the result will be the same whether the tinycolor is initialized with string or object.
        var match;
        if ((match = matchers.rgb.exec(color))) {
            return { r: match[1], g: match[2], b: match[3] };
        }
        if ((match = matchers.rgba.exec(color))) {
            return { r: match[1], g: match[2], b: match[3], a: match[4] };
        }
        if ((match = matchers.hsl.exec(color))) {
            return { h: match[1], s: match[2], l: match[3] };
        }
        if ((match = matchers.hsla.exec(color))) {
            return { h: match[1], s: match[2], l: match[3], a: match[4] };
        }
        if ((match = matchers.hsv.exec(color))) {
            return { h: match[1], s: match[2], v: match[3] };
        }
        if ((match = matchers.hsva.exec(color))) {
            return { h: match[1], s: match[2], v: match[3], a: match[4] };
        }
        if ((match = matchers.hex8.exec(color))) {
            return {
                a: convertHexToDecimal(match[1]),
                r: parseIntFromHex(match[2]),
                g: parseIntFromHex(match[3]),
                b: parseIntFromHex(match[4]),
                format: named ? "name" : "hex8"
            };
        }
        if ((match = matchers.hex6.exec(color))) {
            return {
                r: parseIntFromHex(match[1]),
                g: parseIntFromHex(match[2]),
                b: parseIntFromHex(match[3]),
                format: named ? "name" : "hex"
            };
        }
        if ((match = matchers.hex3.exec(color))) {
            return {
                r: parseIntFromHex(match[1] + '' + match[1]),
                g: parseIntFromHex(match[2] + '' + match[2]),
                b: parseIntFromHex(match[3] + '' + match[3]),
                format: named ? "name" : "hex"
            };
        }

        return false;
    }

    window.tinycolor = tinycolor;
    })();

    $(function () {
        if ($.fn.spectrum.load) {
            $.fn.spectrum.processNativeColorInputs();
        }
    });

});

/* End */
;
; /* Start:"a:4:{s:4:"full";s:114:"/bitrix/templates/elasto_start/components/bitrix/main.userconsent.request/.default/user_consent.js?149941621510749";s:6:"source";s:98:"/bitrix/templates/elasto_start/components/bitrix/main.userconsent.request/.default/user_consent.js";s:3:"min";s:0:"";s:3:"map";s:0:"";}"*/
;(function(){

	function UserConsentControl (params)
	{
		this.caller = params.caller;
		this.formNode = params.formNode;
		this.controlNode = params.controlNode;
		this.inputNode = params.inputNode;
		this.config = params.config;
	}
	UserConsentControl.prototype = {

	};

	BX.UserConsent = {
		msg: {
			'title': BX.message('MAIN_USER_CONSENT_REQUEST_TITLE'),
			'btnAccept': BX.message('MAIN_USER_CONSENT_REQUEST_BTN_ACCEPT'),
			'btnReject': BX.message('MAIN_USER_CONSENT_REQUEST_BTN_REJECT'),
			'loading': BX.message('MAIN_USER_CONSENT_REQUEST_LOADING'),
			'errTextLoad': BX.message('MAIN_USER_CONSENT_REQUEST_ERR_TEXT_LOAD')
		},
		events: {
			'save': 'main-user-consent-request-save',
			'refused': 'main-user-consent-request-refused',
			'accepted': 'main-user-consent-request-accepted'
		},
		textList: {},
		current: null,
		autoSave: false,
		isFormSubmitted: false,
		isConsentSaved: false,
		attributeControl: 'data-bx-user-consent',
		load: function (context)
		{
			var item = this.find(context)[0];
			if (!item)
			{
				return null;
			}

			this.bind(item);
			return item;
		},
		loadAll: function (context, limit)
		{
			this.find(context, limit).forEach(this.bind, this);
		},
		loadFromForms: function ()
		{
			var formNodes = document.getElementsByTagName('FORM');
			formNodes = BX.convert.nodeListToArray(formNodes);
			formNodes.forEach(this.loadAll, this);
		},
		find: function (context)
		{
			if (!context)
			{
				return [];
			}

			var controlNodes = context.querySelectorAll('[' + this.attributeControl + ']');
			controlNodes = BX.convert.nodeListToArray(controlNodes);
			return controlNodes.map(this.createItem.bind(this, context)).filter(function (item) { return !!item });
		},
		bind: function (item)
		{
			if (item.config.submitEventName)
			{
				BX.addCustomEvent(item.config.submitEventName, this.onSubmit.bind(this, item));
			}
			else if(item.formNode)
			{
				BX.bind(item.formNode, 'submit', this.onSubmit.bind(this, item));
			}

			BX.bind(item.controlNode, 'click', this.onClick.bind(this, item));
		},
		createItem: function (context, controlNode)
		{
			var inputNode = controlNode.querySelector('input[type="checkbox"]');
			if (!inputNode)
			{
				return;
			}

			try
			{
				var config = JSON.parse(controlNode.getAttribute(this.attributeControl));
				var parameters = {
					'formNode': null,
					'controlNode': controlNode,
					'inputNode': inputNode,
					'config': config
				};

				if (context.tagName == 'FORM')
				{
					parameters.formNode = context;
				}
				else
				{
					parameters.formNode = BX.findParent(inputNode, {tagName: 'FORM'})
				}

				parameters.caller = this;
				return new UserConsentControl(parameters);
			}
			catch (e)
			{
				return null;
			}
		},
		onClick: function (item, e)
		{
			this.requestForItem(item);
			e.preventDefault();
		},
		onSubmit: function (item, e)
		{
			this.isFormSubmitted = true;
			if (this.check(item))
			{
				return true;
			}
			else
			{
				if (e)
				{
					e.preventDefault();
				}

				return false;
			}
		},
		check: function (item)
		{
			if (item.inputNode.checked)
			{
				this.saveConsent(item);
				return true;
			}

			this.requestForItem(item);
			return false;
		},
		requestForItem: function (item)
		{
			this.setCurrent(item);
			this.requestConsent(
				item.config.id,
				{
					'sec': item.config.sec,
					'replace': item.config.replace
				},
				this.onAccepted,
				this.onRefused
			);
		},
		setCurrent: function (item)
		{
			this.current = item;
			this.autoSave = item.config.autoSave;
			this.actionRequestUrl = item.config.actionUrl;
		},
		onAccepted: function ()
		{
			if (!this.current)
			{
				return;
			}

			var item = this.current;
			this.saveConsent(
				this.current,
				function ()
				{
					BX.onCustomEvent(item, this.events.accepted, []);
					BX.onCustomEvent(this, this.events.accepted, [item]);

					this.isConsentSaved = true;

					if (this.isFormSubmitted && item.formNode && !item.config.submitEventName)
					{
						BX.submit(item.formNode);
					}
				}
			);

			this.current.inputNode.checked = true;
			BX.onCustomEvent(this.current.formNode, 'OnFormInputUserConsentChange');			
			this.current = null;
		},
		onRefused: function ()
		{
			BX.onCustomEvent(this.current, this.events.refused, []);
			BX.onCustomEvent(this, this.events.refused, [this.current]);
			this.current.inputNode.checked = false;
			BX.onCustomEvent(this.current.formNode, 'OnFormInputUserConsentChange');
			this.current = null;
			this.isFormSubmitted = false;
		},
		initPopup: function ()
		{
			if (this.popup)
			{
				return;
			}


			this.popup = {

			};
		},
		popup: {
			isInit: false,
			caller: null,
			nodes: {
				container: null,
				shadow: null,
				head: null,
				loader: null,
				content: null,
				textarea: null,
				buttonAccept: null,
				buttonReject: null
			},
			onAccept: function ()
			{
				this.hide();
				BX.onCustomEvent(this, 'accept', []);
			},
			onReject: function ()
			{
				this.hide();
				BX.onCustomEvent(this, 'reject', []);
			},
			init: function ()
			{
				if (this.isInit)
				{
					return true;
				}

				var tmplNode = document.querySelector('script[data-bx-template]');
				if (!tmplNode)
				{
					return false;
				}

				var popup = document.createElement('DIV');
				popup.innerHTML = tmplNode.innerHTML;
				popup = popup.children[0];
				if (!popup)
				{
					return false;
				}
				document.body.insertBefore(popup, document.body.children[0]);

				this.isInit = true;
				this.nodes.container = popup;
				this.nodes.shadow = this.nodes.container.querySelector('[data-bx-shadow]');
				this.nodes.head = this.nodes.container.querySelector('[data-bx-head]');
				this.nodes.loader = this.nodes.container.querySelector('[data-bx-loader]');
				this.nodes.content = this.nodes.container.querySelector('[data-bx-content]');
				this.nodes.textarea = this.nodes.container.querySelector('[data-bx-textarea]');

				this.nodes.buttonAccept = this.nodes.container.querySelector('[data-bx-btn-accept]');
				this.nodes.buttonReject = this.nodes.container.querySelector('[data-bx-btn-reject]');
				this.nodes.buttonAccept.textContent = this.caller.msg.btnAccept;
				this.nodes.buttonReject.textContent = this.caller.msg.btnReject;
				BX.bind(this.nodes.buttonAccept, 'click', this.onAccept.bind(this));
				BX.bind(this.nodes.buttonReject, 'click', this.onReject.bind(this));

				return true;
			},
			setTitle: function (text)
			{
				if (!this.nodes.head)
				{
					return;
				}
				this.nodes.head.textContent = text;
			},
			setContent: function (text)
			{
				if (!this.nodes.textarea)
				{
					return;
				}
				this.nodes.textarea.textContent = text;
			},
			show: function (isContentVisible)
			{
				if (typeof isContentVisible == 'boolean')
				{
					this.nodes.loader.style.display = !isContentVisible ? '' : 'none';
					this.nodes.content.style.display = isContentVisible ? '' : 'none';
				}

				this.nodes.container.style.display = '';
			},
			hide: function ()
			{
				this.nodes.container.style.display = 'none';
			}
		},
		requestConsent: function (id, sendData, onAccepted, onRefused)
		{
			sendData = sendData || {};
			sendData.id = id;

			if (!this.popup.isInit)
			{
				this.popup.caller = this;
				if (!this.popup.init())
				{
					return;
				}

				BX.addCustomEvent(this.popup, 'accept', onAccepted.bind(this));
				BX.addCustomEvent(this.popup, 'reject', onRefused.bind(this));
			}

			if (this.current && this.current.config.text)
			{
				this.textList[id] = this.current.config.text;
			}

			if (this.textList.hasOwnProperty(id))
			{
				this.setTextToPopup(this.textList[id]);
			}
			else
			{
				this.popup.setTitle(this.msg.loading);
				this.popup.show(false);
				this.sendActionRequest(
					'getText', sendData,
					function (data)
					{
						this.textList[id] = data.text || '';
						this.setTextToPopup(this.textList[id]);
					},
					function ()
					{
						this.popup.hide();
						alert(this.msg.errTextLoad);
					}
				);
			}
		},
		setTextToPopup: function (text)
		{
			// set title from a first line from text.
			var titleBar = '';
			var textTitlePos = text.indexOf("\n");
			var textTitleDotPos = text.indexOf(".");
			textTitlePos = textTitlePos < textTitleDotPos ? textTitlePos : textTitleDotPos;
			if (textTitlePos >= 0 && textTitlePos <= 100)
			{
				titleBar = text.substr(0, textTitlePos).trim();
				titleBar  = titleBar.split(".").map(Function.prototype.call, String.prototype.trim).filter(String)[0];
			}
			this.popup.setTitle(titleBar ? titleBar : this.msg.title);
			this.popup.setContent(text);
			this.popup.show(true);
		},
		saveConsent: function (item, callback)
		{
			this.setCurrent(item);

			var data = {
				'id': item.config.id,
				'sec': item.config.sec,
				'url': window.location.href
			};
			if (item.config.originId)
			{
				var originId = item.config.originId;
				if (item.formNode && originId.indexOf('%') >= 0)
				{
					var inputs = item.formNode.querySelectorAll('input[type="text"], input[type="hidden"]');
					inputs = BX.convert.nodeListToArray(inputs);
					inputs.forEach(function (input) {
						if (!input.name)
						{
							return;
						}
						originId = originId.replace('%' + input.name +  '%', input.value ? input.value : '');
					});
				}
				data.originId = originId;
			}
			if (item.config.originatorId)
			{
				data.originatorId = item.config.originatorId;
			}

			BX.onCustomEvent(item, this.events.save, [data]);
			BX.onCustomEvent(this, this.events.save, [item, data]);

			if (this.isConsentSaved || !item.config.autoSave)
			{
				if (callback)
				{
					callback.apply(this, []);
				}
			}
			else
			{
				this.sendActionRequest(
					'saveConsent',
					data,
					callback,
					callback
				);
			}
		},
		sendActionRequest: function (action, sendData, callbackSuccess, callbackFailure)
		{
			callbackSuccess = callbackSuccess || null;
			callbackFailure = callbackFailure || null;

			sendData.action = action;
			sendData.sessid = BX.bitrix_sessid();
			sendData.action = action;

			BX.ajax({
				url: this.actionRequestUrl,
				method: 'POST',
				data: sendData,
				timeout: 10,
				dataType: 'json',
				processData: true,
				onsuccess: BX.proxy(function(data){
					data = data || {};
					if(data.error)
					{
						callbackFailure.apply(this, [data]);
					}
					else if(callbackSuccess)
					{
						callbackSuccess.apply(this, [data]);
					}
				}, this),
				onfailure: BX.proxy(function(){
					var data = {'error': true, 'text': ''};
					if (callbackFailure)
					{
						callbackFailure.apply(this, [data]);
					}
				}, this)
			});
		}
	};

	BX.ready(function () {
		BX.UserConsent.loadFromForms();
	});

})();
/* End */
;; /* /bitrix/templates/elasto_start/js/bootstrap.min.js?145950725436868*/
; /* /bitrix/templates/elasto_start/js/formValidation.min.js?1474014364129735*/
; /* /bitrix/templates/elasto_start/js/jquery.inputmask.bundle.min.js?146131939870473*/
; /* /bitrix/templates/elasto_start/js/moreMenu.js?14658030863336*/
; /* /bitrix/templates/elasto_start/js/main.js?14951818345978*/
; /* /bitrix/templates/elasto_start/js/jquery.cookie.js?14622757213121*/
; /* /bitrix/templates/elasto_start/js/spectrum/spectrum.js?146363863579101*/
; /* /bitrix/templates/elasto_start/components/bitrix/main.userconsent.request/.default/user_consent.js?149941621510749*/
