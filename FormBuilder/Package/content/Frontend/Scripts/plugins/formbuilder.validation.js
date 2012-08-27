(function ($)
{
    $.fn.formValidation = function (opt)
    {
        var form = this;
        opt = $.extend(opt, { autoValidate: false });

        form.submit(function (e)
        {
            var formSerialized = form.serializeArray();

            form.data('error', false);

            form.find('.control-group').removeClass('error');
            form.find('.error_notification').remove();

            $.ajax({
                type: 'POST',
                url: '/validateform?formname=' + form.attr('id'),
                data: formSerialized,
                dataType: 'json',
                async: false,
                success: function (data)
                {
                    var errors = data.errors;
                    if (errors.length > 0)
                    {
                        form.data('error', true);

                        var errorDiv = '<div class="error_notification"><p>Du mangler at udfylde nogle felter</p><ul>';

                        $.each(errors, function (i, itm)
                        {
                            var keys = itm.affectedKeys;

                            $.each(keys, function (i, key)
                            {
                                var el = form.find('#' + key);

                                el.parents('.control-group').addClass('error');
                            });

                            errorDiv += '<li>' + itm.validationMessage + '</li>';
                        });

                        errorDiv += '</ul><p>Udfyld venligst felterne og send igen.</p></div>';

                        form.prepend($(errorDiv));

                        e.preventDefault();
                    }
                }
            });
        });

        if (opt.autovalidate)
        {
            var inp = form.find('input, select, textarea').not(':input[type="submit"]').not(':input[type="image"]');
            var submit = false;
            $.each(inp, function (i, itm)
            {
                var $itm = $(itm);
                var val = $itm.val();
                if (val != '' && val != 'choose')
                {
                    submit = true;
                }
            });

            if (submit)
            {
                form.submit();
            }
        }
    };
})(jQuery);