(function () {
    angular.module('coduco', [
        
        'ui.router',                    // Routing
        'oc.lazyLoad',                  // ocLazyLoad
        'ui.bootstrap',                 // Ui Bootstrap
        'pascalprecht.translate',       // Angular Translate
        'ngIdle',                       // Idle timer
        'ngSanitize',                    // ngSanitize
        'ngStorage',                   // ngStorage 
        'ngResource',
        'ngMaterial',
		'ngCordovaOauth',
        'webcam',
        'angularResizable',
        'cgNotify',
    ])
})();

// Other libraries are loaded dynamically in the config.js file using the library ocLazyLoad