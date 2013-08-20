(function ($, document, undefined) {
    var getFormFieldValue = function (fieldName) {
        var field = $('[name="' + fieldName + '"]');
        var type = field.attr('type');

        if (type) {
            if (type.toLowerCase() == 'radio') {
                field = field.filter(":checked");
            }
        }

        var element = field.get(0);
        var displayNone = false;

        while (element != null) {
            if (element.style) {
                displayNone = displayNone || element.style.display == 'none';
            }

            element = element.parentNode;
        }

        return (displayNone ? null : field.val());
    }

    var dependecyFunction = function () {
        $('[data-dependency]').each(function (ix, itm) {
            var $itm = $(itm);
            var json = $itm.data('dependency');
            var show = showFunction(json);

            if (!show) {
                $itm.hide();
            } else {
                $itm.show();
            }
        });
    };

    var showFunction = function (json) {
        var show = false;

        $.each(json, function (ix, itm) {
            var field = itm.field;

            $.each(itm.value, function (ix, itm) {
                var regexp = new RegExp("^" + itm + "$", "i");

                show = show || getFormFieldValue(field).match(regexp)
            });
        });

        return show;
    };

    $(document).ready(function () {
        $(':input').change(dependecyFunction);

        dependecyFunction();
    });
})(jQuery, document);