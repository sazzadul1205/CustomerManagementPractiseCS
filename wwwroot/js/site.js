// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Shared helpers for every page. This file is loaded by Views/Shared/_Layout.cshtml.

// Show a small message in the corner of the screen.
// The AJAX endpoints return their message with the JSON response, so it appears right
// where the user just clicked instead of popping up later on some other page.
function showToast(message, isError) {
    // Create the holder for the messages only once
    if ($('#toastHolder').length === 0) {
        $('body').append('<div id="toastHolder" class="position-fixed bottom-0 end-0 p-3" style="z-index: 1080;"></div>');
    }

    var cssClass = isError ? 'alert-danger' : 'alert-success';

    var toast = $('<div class="alert ' + cssClass + ' shadow-sm" role="alert"></div>');
    toast.text(message);   // text() not html(), so a message can never inject markup

    $('#toastHolder').append(toast);

    // Fade it away again after a few seconds
    setTimeout(function () {
        toast.fadeOut(300, function () {
            toast.remove();
        });
    }, 4000);
}
