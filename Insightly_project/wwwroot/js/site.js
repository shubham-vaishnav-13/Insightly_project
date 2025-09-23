// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Modern Navbar Enhancement
$(function() {
    // Initialize tooltips
    $('[data-toggle="tooltip"]').tooltip();
    
    // Navbar shrink effect on scroll
    $(window).scroll(function() {
        if ($(document).scrollTop() > 50) {
            $('.modern-navbar').addClass('navbar-shrink');
        } else {
            $('.modern-navbar').removeClass('navbar-shrink');
        }
    });
    
    // Add smooth scrolling to nav links
    $('.navbar a[href*="#"]:not([href="#"])').click(function() {
        if (location.pathname.replace(/^\//, '') == this.pathname.replace(/^\//, '') && 
            location.hostname == this.hostname) {
            var target = $(this.hash);
            target = target.length ? target : $('[name=' + this.hash.slice(1) + ']');
            if (target.length) {
                $('html, body').animate({
                    scrollTop: target.offset().top - 70
                }, 800);
                return false;
            }
        }
    });
    
    // Close responsive menu when a menu item is clicked
    $('.navbar-collapse a').click(function(){
        if ($(window).width() < 992) {
            $('.navbar-collapse').collapse('hide');
        }
    });
    
    // Add animation to dropdown menus
    $('.dropdown').on('show.bs.dropdown', function() {
        $(this).find('.dropdown-menu').first().stop(true, true).slideDown(200);
    });
    $('.dropdown').on('hide.bs.dropdown', function() {
        $(this).find('.dropdown-menu').first().stop(true, true).slideUp(100);
    });
});
