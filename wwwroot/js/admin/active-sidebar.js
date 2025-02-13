// for active sidebar menus 
$(document).ready(function () {
    // Get the current page URL
    var currentPageURL = window.location.href;

    // Remove the active class from all navigation links
    $('.sidebar-link').removeClass('active');

    // Loop through each navigation link and check if it matches the current page URL
    $('.sidebar-link').each(function () {
        var linkURL = $(this).attr('href');

        // Check if the current link matches the current page URL
        if (currentPageURL.indexOf(linkURL) !== -1) {
            // Add the active class to the matching link
            $(this).addClass('active');
        }
    })

    var currentPageURL2 = window.location.href;

    // Remove the active class from all navigation links
    $('.sidebar-dropdown-link').removeClass('active');

    // Loop through each navigation link and check if it matches the current page URL
    $('.sidebar-dropdown-link').each(function () {

        var linkURL2 = $(this).attr('href');

        // Check if the current link matches the current page URL
        if (currentPageURL2.indexOf(linkURL2) !== -1) {
            // Add the active class to the matching link
            $(this).addClass('active');

            $(this).closest('.sidebar-menu-item').find('.sidebar-dropdown-menu').toggleClass('showDisplay');
            $(this).closest('.sidebar-menu-item').toggleClass('active-focused')
        }
    })
});