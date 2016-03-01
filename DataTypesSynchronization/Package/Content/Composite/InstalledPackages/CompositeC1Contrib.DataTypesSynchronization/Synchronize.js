var lastIsRunningValue = 'no';

var update = function() {
    var jobId = $('body').data('jobid');

    $.ajax({
        type: 'POST',
        url: 'Synchronize.aspx/JobStatus',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: '{ jobId: "' + jobId + '" }',

        success: function(data) {
            if (data.d === 'yes' || (!data.d && lastIsRunningValue === 'yes')) {
                writeLog();
            }

            lastIsRunningValue = data.d;
            $('.status').html(lastIsRunningValue.toString());
        }
    });

    setTimeout(update, 1000);
};

var writeLog = function() {
    var jobId = $('body').data('jobid');

    $.ajax({
        type: 'POST',
        url: 'Synchronize.aspx/GetJobLog',
        contentType: 'application/json; charset=utf-8',
        dataType: 'json',
        data: '{ jobId: "' + jobId + '" }',

        success: function(data) {
            $('.log').html(data.d);
        }
    });
};

$(document).ready(function() {
    update();
});