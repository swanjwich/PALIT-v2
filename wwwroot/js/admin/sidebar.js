$(document).ready(function () {

    //ga provide ug focused class sa isa ka sidebar-menu-item na gi click sa user
    $('.sidebar-menu-item.has-dropdown > a').click(function (e) {
        e.preventDefault()

        if (!($(this).parent().hasClass('focused'))) {
            $(this).parent().parent().find('.sidebar-dropdown-menu').slideUp('fast')
            $(this).parent().parent().find('.has-dropdown').removeClass('focused')
        }

        $(this).next().slideToggle('fast')
        $(this).parent().toggleClass('focused')
    })

    // toggle para mu collapse ang sidebar
    $('.sidebar-toggle').click(function () {
        $('.sidebar').toggleClass('collapsed')
    })

    // toggle para mu expand ang navbar
    $('.sidebar-toggle').click(function () {
        $('.navbar').toggleClass('collapsed')
    })

    // toggle para mu expand ang content
    $('.sidebar-toggle').click(function () {
        $('.content').toggleClass('collapsed')
    })

    // toggle para mu expand ang footer
    $('.sidebar-toggle').click(function () {
        $('.footer').toggleClass('collapsed')
    })

    // kani sya mu toggle once naay sidebar-menu-item na naay dropdown 
    // tapos naka drop lng ang items unya gi click ang sidebar-toggle
    $('.sidebar-toggle').click(function () {
        $('.sidebar-dropdown-menu').slideUp('fast')
        $('.sidebar-menu-item.has-dropdown, .sidebar-dropdown-menu-item.has-dropdown').removeClass('focused')
    })

    // ga toggle paras sidebar menu items with dropdown nga if currently collapsed ang sidebar 
    // tapos ni click syas isa ka sidebar-menu-item, mu expand ang sidebar
    $('.sidebar-menu-item.has-dropdown > a').click(function (e) {
        e.preventDefault();

        // Check if the sidebar is currently collapsed
        if ($('.sidebar').hasClass('collapsed')) {
            // Sidebar is collapsed, toggle it
            $('.sidebar').toggleClass('collapsed');
            $('.navbar').toggleClass('collapsed')
            $('.content').toggleClass('collapsed')
        } else {
            // Sidebar is not collapsed
        }
    });
})


