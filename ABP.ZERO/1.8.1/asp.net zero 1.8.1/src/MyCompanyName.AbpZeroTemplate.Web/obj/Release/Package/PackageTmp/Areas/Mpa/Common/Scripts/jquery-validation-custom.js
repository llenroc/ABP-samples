(function($) {
    $.validator.setDefaults({
        highlight: function (element) {
            $(element).closest('.form-group').addClass('has-error');
        },

        unhighlight: function (element) {
            $(element).closest('.form-group').removeClass('has-error');
        },

        errorElement: 'span',

        errorClass: 'help-block help-block-validation-error',

        errorPlacement: function (error, element) {
            $(element).closest('.form-group').append(error);
        }
    });
})(jQuery);