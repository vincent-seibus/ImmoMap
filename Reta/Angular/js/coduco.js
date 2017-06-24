

$(document).ready(function () {

    // Append config box / Only for demo purpose
    //$.get("views/skin-config.html", function (data) {
    //    $('body').append(data);
    //});

    // Full height of sidebar
    function fix_height() {
        var heightWithoutNavbar = $("body > #wrapper").height() - 61;
        $(".sidebard-panel").css("min-height", heightWithoutNavbar + "px");

        var navbarHeigh = $('nav.navbar-default').height();
        var wrapperHeigh = $('#page-wrapper').height();

        if(navbarHeigh > wrapperHeigh){
            $('#page-wrapper').css("min-height", navbarHeigh + "px");
        }

        if(navbarHeigh < wrapperHeigh){
            $('#page-wrapper').css("min-height", $(window).height()  + "px");
        }

        if ($('body').hasClass('fixed-nav')) {
            if (navbarHeigh > wrapperHeigh) {
                $('#page-wrapper').css("min-height", navbarHeigh - 60 + "px");
            } else {
                $('#page-wrapper').css("min-height", $(window).height() - 60 + "px");
            }
        }

    }


    $(window).bind("load resize scroll", function() {
        if(!$("body").hasClass('body-small')) {
            fix_height();
        }
    });

    // Move right sidebar top after scroll
    $(window).scroll(function(){
        if ($(window).scrollTop() > 0 && !$('body').hasClass('fixed-nav') ) {
            $('#right-sidebar').addClass('sidebar-top');
        } else {
            $('#right-sidebar').removeClass('sidebar-top');
        }
    });


    setTimeout(function(){
        fix_height();
    })
    /*/
    if (document.getElementById("map-canvas") != undefined && document.getElementById("map-canvas") != null) {
        var latitude = 48.8671658;
        var longitude = 2.3496949;
        var options = {
            enableHighAccuracy: true,
            timeout: 5000,
            maximumAge: 0
        };
        function getLocation() {
            if (navigator.geolocation) {
                navigator.geolocation.getCurrentPosition(GetPositionSuccess, GetPositionError, options);
            }
        }
        function GetPositionSuccess(position) {
            latitude = position.coords.latitude;
            longitude = position.coords.longitude;
            var options = {
                zoom: 10,
                center: new google.maps.LatLng(latitude, longitude), // Current position
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                mapTypeControl: false
            };
            var map = new google.maps.Map(document.getElementById("map-canvas"), options);
        }
        function GetPositionError(error) {
            var options = {
                zoom: 10,
                center: new google.maps.LatLng(latitude, longitude), // Paris
                mapTypeId: google.maps.MapTypeId.ROADMAP,
                mapTypeControl: false
            };
            var map = new google.maps.Map(document.getElementById("map-canvas"), options);
        }
        $("#map-canvas").addClass("map-canvas");
        getLocation();

    }/*/
});

// Minimalize menu when screen is less than 768px
$(function() {
    $(window).bind("load resize", function() {
        if ($(document).width() < 769) {
            $('body').addClass('body-small')
        } else {
            $('body').removeClass('body-small')
        }
    })
})
