// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

document.addEventListener('DOMContentLoaded', function () {
    var toastEl = document.querySelector('.toast');
    var toastBody = toastEl.querySelector('.toast-body').textContent.trim();

    if (toastBody) {
        var toast = new bootstrap.Toast(toastEl, {
            delay: 3000,
            autohide: true
        });
        toast.show();
    }
});