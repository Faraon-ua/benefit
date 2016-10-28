

var quick_buy = {
    'add': function(form) {
        var  testPattern = /.\d{2}\s.\d{3}.\s\d{3}.\d{2}.\d{2}/;



       if(testPattern.test($('#one-click-phone').val()) === true){
        
                $.ajax({
                    url: 'index.php?route=module/quick_buy/adds',
                    type: 'post',
                    data: $('#input-quantity, #test_id, #product input[type=\'hidden\'], #product input[type=\'radio\']:checked, #product input[type=\'checkbox\']:checked, #product select, #product textarea'),
                    dataType: 'json',
                   
                    success: function(json) {
//             	 $('.alert, .text-danger').remove();
                        $('.form-group').removeClass('has-error');

                        if (json['error']) {
                            if (json['error']['option']) {
                                for (i in json['error']['option']) {
                                    var element = $('#input-option' + i.replace('_', '-'));

                                    if (element.parent().hasClass('input-group')) {
                                        element.parent().after('<div class="text-danger">' + json['error']['option'][i] + '</div>');
                                    } else {
                                        element.after('<div class="text-danger">' + json['error']['option'][i] + '</div>');
                                    }
                                }
                            }

                            if (json['error']['recurring']) {
                                $('select[name=\'recurring_id\']').after('<div class="text-danger">' + json['error']['recurring'] + '</div>');
                            }

                            // Highlight any found errors
                            $('.text-danger').parent().addClass('has-error');
                        }

                        if (json['success']) {

							 $.ajax({
								url: 'index.php?route=module/quick_buy',
								type: 'post',
								data: $(form).serialize(),
								dataType: 'json',
								success: function(json) {
									console.log(json);
									var errors = $('.buy-denger').find('p.text-danger');
									errors.addClass('hidden');
									errors.html("");

									if(json.status) {
										$('#quick_buy .modal-body').html("");
										$('#quick_buy .modal-footer').html("");
										//$('#quick_buy .modal-header h4').remove();
										
										 $('.alert, .alert-success').remove();
										 $('.alert, .alert-warning').remove();
										 $('.buy-denger').find('p.text-danger').html('');
										 $('.one-click-wrapper').html(json.msg)
										 setTimeout(function(){
										 $('.one-click-wrapper').html('');	 
										 },4000)
										//$('.breadcrumb').after('<div class="alert alert-success">' + json.msg + '<button type="button" class="close" data-dismiss="alert">&times;</button></div>');
										//$('#quick_buy').html(json.total);

									}
									//else {
									//    errors.removeClass('hidden');
									//    var message = "";
									//    for(var key in json.msg) {
									//        message += json.msg[key] + "<br />";
									//    }
									//    errors.html(message);
									//}
								}
							});


                        }
                    }
                });
				
				  
               
           
        }
        else {

            $.ajax({
                url: 'index.php?route=module/quick_buy',
                type: 'post',
                data: $(form).serialize(),
                dataType: 'json',
                success: function (json) {
					 $('.buy-denger').find('p.text-successr').html('');
                    var errors = $('.buy-denger').find('p.text-danger');
                    errors.addClass('hidden');
                    errors.html("");
                    errors.removeClass('hidden');
                    var message = "";
                    for (var key in json.msg) {
                        message += json.msg[key] + "<br />";
                    }
                    errors.html(message);

                }
            });
        }
    }
}
$(document).ready(function(){
   $('#one-click-phone').mask("+38 (999) 999-99-99",{placeholder:"+38 (___) ___-__-__"});
});
