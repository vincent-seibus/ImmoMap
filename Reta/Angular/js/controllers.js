/**
 * MainCtrl - controller
 * Contains several global data used in different view
 *
 */
function MainCtrl($scope, $rootScope, $http, $localStorage, $uibModal, $state, $rootScope, $sce, $translate, languageManager, notify) {
    // General variable used in the whole plateform
    $rootScope.defaultUrl = "/login";
    $rootScope.globalConfiguration = {
        forgotPasswordEnabled: true,
        registerEnabled: true,
        captchaEnabled:false,
        endpointUrl: "",
        externalLoginEnabled : true,
        defaultHomePage: "building.list",
        minimalizeMenuOnItemClicked : false,
    };
    $rootScope.globalConfiguration = $rootScope.globalConfiguration;
    $rootScope.notifTemplate = 'views/common/notif/notify-bottom-fullwidth.html';
    $scope.notifDuration = 4000;

    var isRegister = window.location.href.indexOf("register") > 0;
    var isChangePassword = window.location.href.indexOf("change_password") > 0;
    var isForgotPassword = window.location.href.indexOf("forgot_password") > 0;
    var isVerifyEmail = window.location.href.indexOf("verify_email") > 0;
    var isAutoLogin = window.location.href.indexOf("auto_login") > 0;
    $scope.space = {};
    $scope.user = {};
    $localStorage.ui = {};
    
    languageManager.setLanguageWithBrowserDefaultOrStoredValue();

    $scope.returnUrl = $state.current.name;

    $scope.logout = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/logout";        
        $http.post(url).success(function (dataOut) {
            if (dataOut.error == false)            
                $state.go("login");
                var space = {};
                $rootScope.$emit('spaceUpdated', space);
                $localStorage.ui = {};
        });
        $localStorage.$reset();
        languageManager.setLanguageWithBrowserDefaultOrStoredValue();
    };

    $scope.userIsConnected = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/isconnected";
        $http.get(url).success(function (dataOut) {
            if (dataOut.isConnected == false) {
                $scope.returnUrl = $state.current.name;
                $state.go("login");
            }
            else
            {
                $scope.userInfo();
                $scope.userRoles();
                /*/
                if ($scope.returnUrl != null && $scope.returnUrl != "")
                    $state.go($scope.returnUrl);
                else
                    $state.go("candidats.my_candidats");/*/
            }
        }).error(function (dataOut) {
            console.log(dataOut); 
            $state.go("login");
        });
    };

    $scope.userInfo = function()
    {
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/userinfo";
        $http.get(url).success(function (dataOut) {
            if (dataOut.error == false) {
                if (dataOut.user != null) {
                    $scope.user = dataOut.user;
                    $scope.space = dataOut.space;                    
                    if($scope.space != null)
                        $scope.space.Configuration = JSON.parse($scope.space.Configuration);

                    $localStorage.user = dataOut.user;                    
                    $localStorage.space = $scope.space;
                    $scope.userRoles();
                    $scope.spaceSize();
                    var space = $scope.space;
                    $rootScope.$emit('spaceUpdated',space);
                }
                else
                {
                    $state.go("login");
                }
            }

        }).error(function (dataOut) {
            console.log(dataOut);            
        });
    };

    $scope.userRoles = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/Account/getUserRoles";
        $http.get(url).success(function (dataOut) {           
                    $scope.roles = dataOut;
                    $localStorage.userRoles = dataOut;  
                    }).error(function (dataOut) {
            console.log(dataOut);
        });
    };

    $scope.spaceSize = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/spaces/GetSpaceDirectorySizeByUser";
        $http.get(url).success(function (dataOut) {
            if (dataOut.error == false) {
                $scope.space.size = $scope.formatSizeBytes(dataOut.sizeInBytes);
                $localStorage.space = $scope.space;
            }
        });
    }

    $scope.isInRole = function (roleNames) {
        if ($localStorage.userRoles != null) {
            for (var i = 0 ; i < roleNames.length; i++) {
                if ($localStorage.userRoles.indexOf(roleNames[i]) > -1) {
                    return true;
                }
            }
        }
        return false;
    };

    if ($localStorage.user == null && $state.includes('login') == false)
    {
        if (!isRegister && !isChangePassword && !isForgotPassword && !isVerifyEmail && !isAutoLogin) {
            $state.go("login");
        }
    }

    if (!$state.includes('login') && !isRegister && !isChangePassword && !isForgotPassword && !isVerifyEmail && !isAutoLogin)
       $scope.userIsConnected();  
       
    // HELPERS 
    $scope.textAsHtml = function (text)
    {
        return $sce.trustAsHtml(text);
    }

    $scope.truncateOptions = {
        watch: 'window'
    }

    $scope.getUrlProfileImg = function(user)
    {
        if(user == null)
            return null;
    }  

    $scope.minimalizeMenuOnItemClicked = function () {
        if (!$rootScope.globalConfiguration.minimalizeMenuOnItemClicked)
            return;

        $("body").toggleClass("mini-navbar");
        if (!$('body').hasClass('mini-navbar') || $('body').hasClass('body-small')) {
            // Hide menu in order to smoothly turn on when maximize menu
            $('#side-menu').hide();
            // For smoothly turn on menu
            setTimeout(
                function () {
                    $('#side-menu').fadeIn(400);
                }, 200);
        } else if ($('body').hasClass('fixed-sidebar')){
            $('#side-menu').hide();
            setTimeout(
                function () {
                    $('#side-menu').fadeIn(400);
                }, 100);
        } else {
            // Remove all inline style from jquery fadeIn function to reset menu state
            $('#side-menu').removeAttr('style');
        }
    }

    $scope.removeItemFromList = function (item,items) {
        items.splice(items.indexOf(item), 1);
    }

    $scope.select = function (object, list) {
        var index = list.map(function (a) { return a.Id }).indexOf(object.Id);
        if (index < 0) {
            list.push(object);
        }
        else {
            list.splice(index, 1);
        }
    }

    $scope.isSelected = function (object, list) {
        if (list.map(function (a) { return a.Id }).indexOf(object.Id) > -1)
            return true;
        return false;
    }

    $scope.unselect = function (list) {
        list = [];
    }

    $scope.removeItems = function (items, list) {
        for (var i = 0; i < items.length; i++) {
            var index = list.map(function (a) { return a.Id }).indexOf(items[i].Id);
            list.splice(index, 1);
        }
    }

    $scope.deleteMulti = function (items, list, urlRelativeApi) {
        $scope.removeItems(items, list);
        var url = $rootScope.globalConfiguration.endpointUrl + urlRelativeApi;
        var data = { value: JSON.stringify(items.map(function (a) { return a.Id })) };
        $http.post(url, data)
            .success(function (data) {
                if (data.error == false) {
                    $translate(data.notification).then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate(data.notification).then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function (data) {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log(response);
                });
            });
    }

    $scope.delete = function (item, urlRelativeApi) {
        var url = $rootScope.globalConfiguration.endpointUrl + urlRelativeApi;
        var data = { value: item.Id };
        $http.post(url, data)
            .success(function (data) {
                if (data.error == false) {
                    $translate(data.notification).then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate(data.notification).then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function (data) {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log(response);
                });
            });
    }

    $scope.loginSuccess = function () {
        $scope.userIsConnected();
        $translate('loginSuccessNotif').then(function (translation) {
            notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
        });
        if ($scope.returnUrl != null && $scope.returnUrl != "")
            $state.go($scope.returnUrl);
        else
            $state.go($rootScope.globalConfiguration.defaultHomePage);
    }

    $scope.goTo = function (route, params)
    {
        if (params == null)
            $state.go(route);
        else
            $state.go(route,params);
    }

    $scope.formatSizeBytes = function (bytes, precision) {
        if (bytes == "0")
            return "";
        if (isNaN(parseFloat(bytes)) || !isFinite(bytes)) return '-';
        if (typeof precision === 'undefined') precision = 1;
        var units = ['octet', 'Ko', 'Mo', 'Go', 'To', 'Po'],
			number = Math.floor(Math.log(bytes) / Math.log(1024));
        return (bytes / Math.pow(1024, Math.floor(number))).toFixed(precision) +  ' ' + units[number];
    }

    $scope.strToDate = function (str) {
        if (str != null && str != "")
            return new Date(str);
        else
            return new Date();
    }
};

function headerCtrl($scope, $rootScope, $http, $localStorage, $uibModal, $state, $rootScope)
{
    $scope.specificCss = 'css/specific.css';
    $scope.versionCss = 1;
    $rootScope.$on('spaceUpdated', function (event, space) {
        $scope.space = space;
        if ($scope.space != null && $scope.space.Id != null && $scope.space.Configuration != null && $scope.space.Configuration.componentColor != null && $scope.space.Configuration.componentColor != "") {
            var url = '../Files/CssSpecific/' + $scope.space.Id + '.css';
            $http.get(url).success(function () {               
                $scope.specificCss = url + "?v=" + $scope.versionCss;
                $scope.changeCss();
                $scope.versionCss += 1;
            });
        }
        $scope.changeCss();
    });   
    $scope.changeCss = function ()
    {
        return $scope.specificCss;
    }
       
}
/**
Security controllers
*/
function isAuthenticatedCtrl($scope, $rootScope, $translate, notify, $state, $localStorage)
{
    if ($localStorage.user == null) {       
        $scope.userIsConnected();
    }    
}

function loginCtrl($scope, $rootScope, $http, $localStorage, $translate, notify, $state, $uibModal, $cordovaOauth)
{
    $scope.userCredential = {};
    $scope.returnUrl = "";    
    $scope.loadingLoginForm = false;

    $scope.processFormLogin = function () {
        $scope.userCredential.RememberMe = false;
        $scope.loadingLoginForm = true;
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postlogin";
        var data = { model: $scope.userCredential, returnUrl: $scope.returnUrl };
        $http.post(url, data).success(function (dataout) {
            if (dataout.error == false) {
                loginSuccess();
            }
            else if (dataout.errorMessage == "invalidLoginAttempt")
            {
                $translate('loginInvalidNotif').then(function (translation) {
                    notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
            else if (dataout.errorMessage == "accountHasToBeVerified")
            {
                $translate('loginAccountToBeVerifiedInvalidNotif').then(function (translation) {
                    notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }            
             $scope.loadingLoginForm = false;
        })
        .error(function (msg) {
            $translate('GeneralMessageError').then(function (translation) {
                notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            $scope.loadingLoginForm = false;
        })
    }

    $scope.loginWithLinkedin = function()
    {
        $cordovaOauth.linkedin("75twwl42z2opmj", "Ck7eoHvilKWGbclZ", ["r_basicprofile", "r_emailaddress"], "coucou").then(function (result) {
            $localStorage.authorization = result;
            loginSuccess();
        }, function (error) {
            alert("an error happen");
            // error
        });       
    }

    $scope.loginWithFacebook = function () {
        $cordovaOauth.facebook("1956922984538672", ["email", "public_profile", "user_friends", "user_work_history", "user_education_history"]).then(function (result) {
            $localStorage.authorization = result;
            loginSuccess();
        }, function (error) {
            alert(JSON.stringify(error));
            // error
        });
    }

    $scope.loginWithGoogle = function () {
        $cordovaOauth.google("1034774541356-8kiof7ve5vumpih7psos8f387vq6jad5.apps.googleusercontent.com", ["email"]).then(function (result) {
            $localStorage.authorization = result;
            loginSuccess();
        }, function (error) {
            alert(JSON.stringify(error));
            // error
        });
    }
    
    var loginSuccess = function () {
        $scope.userIsConnected();
        $translate('loginSuccessNotif').then(function (translation) {
            notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
        });
        if ($scope.returnUrl != null && $scope.returnUrl != "")
            $state.go($scope.returnUrl);
        else
            $state.go($rootScope.globalConfiguration.defaultHomePage);
    }
}

function registerCtrl($scope, $rootScope, $state, $http,notify, $translate) {
    // All data will be store in this object
    $scope.formData = {};
    $scope.formResponse = {};
    $scope.loadingForm = false;    
    $scope.captchaShow = $rootScope.globalConfiguration.captchaEnabled;
    // After process wizard
    $scope.processFormRegister = function (isValid) {      
        if (!checkCaptcha() && $scope.captchaShow) {
            $translate('GeneralFormCaptchaNotValid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }
        if (!isValid) {
            $translate('GeneralFormInvalid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }

        $scope.loadingForm = true; 
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postregister";
        $http.post(url,{ model : $scope.formData })
            .success(function (dataOut, status) {
                if (dataOut.error == false) {
                    $translate('RegisterSuccessResponseText').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        window.location.href = '/';
                    });
                }
                else if (dataOut.errorMessage == "dataIssue") {
                    $translate('RegisterValidationPassword').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else if (dataOut.errorMessage == 'auto-rgeistration-disabled') {
                    $translate('GeneralMessageError').then(function (translation) {
                        notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate('RegisterValidationUsernameExist').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                 $scope.loadingForm = false; 

            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    $scope.loadingForm = false; 
                });
            });
    };

    $scope.backToLogin = function () {
        $state.go('login');
    };

}

function forgotPasswordCtrl($scope, $rootScope, $state, $http,notify, $translate)
{
    $scope.formData = {};
    $scope.loadingForm = false;
    $scope.captchaShow = $rootScope.globalConfiguration.captchaEnabled;
    // After process wizard
    $scope.processFormForgotPassword = function (isValid) {
        if ($scope.captchaShow && !checkCaptcha()) {
            $translate('GeneralFormCaptchaNotValid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }
        if (!isValid) {
            $translate('GeneralFormInvalid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }
        $scope.loadingForm = true;
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postforgotpassword";
        $http.post(url, { model: $scope.formData })

            .success(function (dataOut, status) {
                if (dataOut.error == false) {
                    $translate('forgotPasswordSuccessResponseText').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        $state.go("login");
                    });
                }
                else if (dataOut.errorMessage == "dataIssue") {
                    $translate('forgotPasswordGeneralIssue').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else if (dataOut.errorMessage == 'forgotPasswordNotActivated') {
                    $translate('GeneralMessageError').then(function (translation) {
                        notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate('forgotPasswordGeneralIssue').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                $scope.loadingForm = false;
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
                $scope.loadingForm = false;
            });

    };

    $scope.backToLogin = function () {
        $state.go('login');
    };
}

function changePasswordCtrl($scope, $rootScope, $state, $http, notify, $translate,$location)
{
    if ($location.search().userid == null || $location.search().code == null)
    {
        $translate('GeneralFormInvalid').then(function (translation) {
            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            $state.go("login");
        });
    }

    $scope.formData = { userid: $location.search().userid, code: $location.search().code };

    // After process wizard
    $scope.processFormChangePassword = function (isValid) {

        if (!isValid) {
            $translate('GeneralFormInvalid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postchangepasswordforgot";
        $http.post(url, { model: $scope.formData })

            .success(function (dataOut, status) {
                if (dataOut.error == false) {
                    $translate('changePasswordSuccessResponseText').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        $state.go("login");
                    });
                }
                else if (dataOut.errorMessage == "dataIssue") {
                    $translate('changePasswordGeneralIssue').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else if (dataOut.errorMessage == 'forgotPasswordNotActivated') {
                    $translate('GeneralFormInvalid').then(function (translation) {
                        notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate('changePasswordGeneralIssue').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }

            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            });

    };


}

function verifyEmailCtrl($scope, $rootScope, $state, $http, notify, $translate, $location)
{    
    $scope.VerificationSuccessfull = false;
    $scope.VerificationFailure = false;
    if ($location.search().userid == null || $location.search().code == null) {
        $translate('GeneralFormInvalid').then(function (translation) {
            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            $state.go("login");
        });
    }
    else {

        $scope.formData = { userid: $location.search().userid, code: $location.search().code };
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postverifyemail";
        $http.post(url, $scope.formData)
            .success(function (dataOut, status) {
                if (dataOut.error == false) {
                    $scope.VerificationSuccessfull = true;
                    $translate('verifyEmailSuccessResponseText').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });                       
                    });
                }              
                else {
                    $scope.VerificationFailure = true;
                    console.log(dataOut.errorMessage);
                    $translate('verifyEmailFailureResponseText').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }

            })
            .error(function () {
                $scope.VerificationFailure = true;
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            });
    }

}

function modifyPasswordCtrl($scope, $rootScope, $state, $http, notify, $translate, $location) {   
    $scope.loading = false;
    $scope.success = false;
    $scope.formData = {};
    $scope.processFormChangePassword = function (isValid) {

        if (!isValid) {
            $translate('GeneralFormInvalid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }
        $scope.loading = true;
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postchangepassword";
        var data = { value: JSON.stringify({ currentpassword: $scope.formData.currentPassword, newpassword: $scope.formData.password }) };
        $http.post(url, data)

            .success(function (dataOut, status) {
                if (dataOut.error == false) {
                    $translate('changePasswordSuccessResponseText').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $scope.success = true;
                    $state.go('profil.modify_success');
                }
                else if (dataOut.errorMessage == "dataIssue") {
                    $translate('changePasswordGeneralIssue').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else if (dataOut.errorMessage == 'forgotPasswordNotActivated') {
                    $translate('GeneralFormInvalid').then(function (translation) {
                        notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate('changePasswordGeneralIssue').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                $scope.loading = false;

            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
                $scope.loading = false;
            });

    };
}

function autoLoginCtrl($scope, $rootScope, $state, $http, notify, $translate, $location, $localStorage, $timeout)
{
    if ($location.search().code == null) {
        $translate('GeneralFormInvalid').then(function (translation) {
            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            $state.go("login");
        });
    }      

    var autologin = function () {
        var code = $location.search().code;
        var url = $rootScope.globalConfiguration.endpointUrl + "/account/postautologin";
        var data = { value: code };
        $http.post(url, data).success(function (dataout) {
            if (dataout.error == false) {
                $scope.loginSuccess();
            }
            else {
                $translate(dataout.notification).then(function (translation) {
                    notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
        }).error(function () {
            $translate('GeneralMessageError').then(function (translation) {
                notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
        })
    }

    $timeout(autologin(), 4000);
}
/**
 * translateCtrl - Controller for translate
 */
function translateCtrl($translate, $rootScope, $scope, $localStorage) {
    $scope.changeLanguage = function (langKey) {
        $translate.use(langKey);
        $scope.language = langKey;
        $localStorage.language = langKey;
    };   
}
/**
 * ALL Buildings related Controller 
 */
function mapCtrl($scope, $rootScope, $http, $translate, $uibModal, notify, $localStorage) {
    
}

function buildingListCtrl($scope, $rootScope, $http, $translate, $uibModal, notify, $localStorage, $timeout) {
    var map;
    var markers = [];
    var url = $rootScope.globalConfiguration.endpointUrl + "/buildings/Getbuildings";
    $scope.ui = $localStorage.ui.buildingList ? $localStorage.ui.buildingList : { showList: false };
    $scope.buildings = $localStorage.buildings ? $localStorage.buildings : [];
    $scope.buildingSelected = $localStorage.buildingSelected ? $localStorage.buildingSelected : [];
    $scope.quantity = 10;
    if (!$localStorage.buildings)
        $scope.loading = true;

    $scope.$watch('ui', function () {
        $localStorage.ui.buildingList = $scope.ui;
    });

    $http.get(url).success(function (dataOut) {
        $scope.buildings = dataOut.data;
        $scope.loading = false;
        $localStorage.buildings = dataOut.data;
        addMarkers($scope.buildings);
        for (var i = 0; i < $scope.buildings.length; i++) {
            $scope.buildings[i].selected = $scope.isSelected($scope.buildings[i], $scope.buildingSelected);
        }
    })
        .error(function (dataOut) {
            $scope.loading = false;
        });

    $scope.moreResult = function (quantity) {
        $scope.quantity = $scope.quantity + 10;
    }

    $scope.$watch(function () { return $scope.moreResultIsVisible }, function () {
        if ($scope.moreResultIsVisible)
            $scope.moreResult($scope.quantity);
    });

    $scope.$watch(function () { return $scope.buildingSelected }, function () {
        $localStorage.buildingSelected = $scope.buildingSelected;
    });

    // Map section
    var latitude = 48.8671658;
    var longitude = 2.3496949;
    var optionsGeoloc = {
        enableHighAccuracy: true,
        timeout: 5000,
        maximumAge: 0
    };
    var options = {
        zoom: 10,
        center: new google.maps.LatLng(latitude, longitude), // Current position
        mapTypeId: google.maps.MapTypeId.ROADMAP,
        mapTypeControl: false
    };
    map = new google.maps.Map(document.getElementById("map-canvas"), options);
    function getLocation() {
        if (navigator.geolocation) {
            navigator.geolocation.getCurrentPosition(GetPositionSuccess, GetPositionError, optionsGeoloc);
        }
    }
    function GetPositionSuccess(position) {
        latitude = position.coords.latitude;
        longitude = position.coords.longitude;
        options.center = new google.maps.LatLng(latitude, longitude);
        resizeAndCenter();
    }
    function GetPositionError(error) {
        resizeAndCenter();
    }
    var resizeAndCenter = function () {
        google.maps.event.trigger(map, 'resize');
        map.setCenter(options.center);
    }

    var addMarkers = function (list) {
        var latlngbounds = new google.maps.LatLngBounds();  
        for (var i = 0; i < list.length; i++)
        {
            var coords = new google.maps.LatLng(list[i].ConfigurationParsed.latitude, list[i].ConfigurationParsed.longitude)
            var marker = new google.maps.Marker({
                position: coords,
                map: map,
                title: list[i].Name,
                id: list[i].Id
            });

            //marker.setIcon('../Content/image/MarkerIconWhite.png');

            // process multiple info windows
            (function (marker, i) {
                // add click event
                google.maps.event.addListener(marker, 'click', function () {
                    infowindow = new google.maps.InfoWindow({
                        content: "<p>" + list[i].Name + "</p>"
                    });
                    infowindow.open(map, marker);
                });
            })(marker, i);

            markers.push(marker);
            latlngbounds.extend(coords);
        }
        map.setCenter(latlngbounds.getCenter());
        map.fitBounds(latlngbounds);
    }

    var removeMarkers = function () {
        for (var i = 0; i < markers.length; i++) {
            markers[i].setMap(null);
        }
    }
}

function buildingCreateOrUpdateCtrl($scope, $rootScope, $http, $translate, $uibModal, notify, $localStorage, $stateParams) {
    var map;
    $scope.markers = [];
    var geocoder = new google.maps.Geocoder();
    $scope.initMap = function (latitude, longitude) {
        var mapOptions = {
            zoom: 13,
            mapTypeId: google.maps.MapTypeId.ROADMAP,
            mapTypeControl: false
        };
        map = new google.maps.Map(document.getElementById("map-canvas"), mapOptions);

        if (latitude == null) {
            var optionsGeoloc = {
                enableHighAccuracy: true,
                timeout: 5000,
                maximumAge: 0
            };

            function getLocation() {
                if (navigator.geolocation) {
                    navigator.geolocation.getCurrentPosition(GetPositionSuccess, GetPositionError, optionsGeoloc);
                }
            }
            function GetPositionSuccess(position) {
                latitude = position.coords.latitude;
                longitude = position.coords.longitude;
                map.setCenter(new google.maps.LatLng(latitude, longitude));
            }
            function GetPositionError(error) {
                resizeAndCenter();
            }

            getLocation();
            return;
        }

        map.setCenter(new google.maps.LatLng(latitude, longitude));
        var marker = new google.maps.Marker({
            map: map,
            position: new google.maps.LatLng(latitude, longitude)
        });
        $scope.markers.push(marker);
    }
    if ($stateParams.building == null || $stateParams.building == undefined) {
        $scope.building = $localStorage.creatingBuilding ? $localStorage.creatingBuilding : {};
        if ($scope.building.ConfigurationParsed) {
            $scope.initMap($scope.building.ConfigurationParsed.latitude, $scope.building.ConfigurationParsed.longitude);
        }
        else {
            $scope.building.ConfigurationParsed = {};
            $scope.initMap();
        }
    }
    else {
        $scope.building = $stateParams.building;
        $scope.initMap($scope.building.ConfigurationParsed.latitude, $scope.building.ConfigurationParsed.longitude);
    }

    google.maps.event.addListener(map, 'click', function (event) {
        removeMarkers();
        geocoder.geocode({ 'location': event.latLng }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {

                $scope.building.ConfigurationParsed.fullAddress = results[0].formatted_address;

                var searchAddressComponents = results[0].address_components;
                $.each(searchAddressComponents, function () {
                    if (this.types[0] == "locality") {
                        $scope.building.ConfigurationParsed.city = this.short_name;
                    }
                    if (this.types[0] == "postal_code") {
                        $scope.building.ConfigurationParsed.postcode = this.short_name;
                    }
                    if (this.types[0] == "street_number") {
                        $scope.building.ConfigurationParsed.streetNumber = this.short_name;
                    }
                    if (this.types[0] == "route") {
                        $scope.building.ConfigurationParsed.streetName = this.short_name;
                    }
                    if (this.types[0] == "country") {
                        $scope.building.ConfigurationParsed.country = this.long_name;
                    }
                });
                $scope.building.ConfigurationParsed.latitude = results[0].geometry.location.lat();
                $scope.building.ConfigurationParsed.longitude = results[0].geometry.location.lng();

                $scope.$apply();

                map.setCenter(results[0].geometry.location);
                var marker = new google.maps.Marker({
                    map: map,
                    position: results[0].geometry.location
                });
                $scope.markers.push(marker);

            } else {
                console.log("Geocode was not successful for the following reason: " + status);
                return false;
            }
        });
    });

    $scope.$watch('building', function () {
        $scope.building.Configuration = JSON.stringify($scope.building.ConfigurationParsed);
        if ($scope.building.Id == null)
            $localStorage.creatingBuilding = $scope.building;
    });

    $scope.save = function (valid) {
        if (!valid) {
            return;
        }
        $scope.loading = true;
        $scope.building.Configuration = JSON.stringify($scope.building.ConfigurationParsed);
        var url = $rootScope.globalConfiguration.endpointUrl + "/Buildings/PostBuilding";
        var data = { value: JSON.stringify($scope.building) };
        $http.post(url, data).success(function (dataout) {
            if (dataout.error == false) {
                $scope.building = dataout.data;
                $localStorage.buildings.push(dataout.data);
                $localStorage.creatingBuilding = null;
                $translate('SuccessNotifSaved').then(function (translation) {
                    notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
            else {
                $translate('ErrorNotifSaved').then(function (translation) {
                    notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });  
                });
            }
            $scope.loading = false;
        }).error(function () {
            $translate('GeneralMessageError').then(function (translation) {
                notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                console.log(response);
            });
            $scope.loading = false;
        });
    }

    var removeMarkers = function() {
        for (var i = 0; i < $scope.markers.length; i++) {
            $scope.markers[i].setMap(null);
        }
        $scope.markers.length = 0;
    }
    var addMarker = function (location) {
        marker = new google.maps.Marker({
            position: location,
            map: map
        });
        $scope.markers.push(marker);
    }
    $scope.findAddress = function() {
        removeMarkers();
        geocoder.geocode({ 'address': $scope.building.ConfigurationParsed.fullAddress }, function (results, status) {
            if (status == google.maps.GeocoderStatus.OK) {
               
                $scope.building.ConfigurationParsed.fullAddress = results[0].formatted_address;

                var searchAddressComponents = results[0].address_components;
                $.each(searchAddressComponents, function () {
                    if (this.types[0] == "locality") {
                        $scope.building.ConfigurationParsed.city = this.short_name;
                    }
                    if (this.types[0] == "postal_code") {
                        $scope.building.ConfigurationParsed.postcode = this.short_name;
                    }
                    if (this.types[0] == "street_number") {
                        $scope.building.ConfigurationParsed.streetNumber = this.short_name;
                    }
                    if (this.types[0] == "route") {
                        $scope.building.ConfigurationParsed.streetName = this.short_name;
                    }
                    if (this.types[0] == "country") {
                        $scope.building.ConfigurationParsed.country = this.long_name;
                    }
                });
                $scope.building.ConfigurationParsed.latitude = results[0].geometry.location.lat();
                $scope.building.ConfigurationParsed.longitude = results[0].geometry.location.lng();

                $scope.$apply();
                
                map.setCenter(results[0].geometry.location);
                var marker = new google.maps.Marker({
                    map: map,
                    position: results[0].geometry.location
                });
                $scope.markers.push(marker);

            } else {
                console.log("Geocode was not successful for the following reason: " + status);
                return false;
            }
        });
    }
    
}
/*
* CONFIG related controller
**/
function configSmtpCtrl($scope, $rootScope, $http, $translate, notify, $localStorage)
{
    $localStorage.user.Claims.forEach(function (claim) {
        if (claim.ClaimType == "SmtpConfiguration") {
            $scope.smtp = JSON.parse(claim.ClaimValue);
            if ($scope.smtp.enableSsl == "true" || $scope.smtp.enableSsl == "True" || $scope.smtp.enableSsl == true)
                $scope.smtp.enableSsl = true;
            else
                $scope.smtp.enableSsl = false;
        }
    });

    $scope.processFormEditSmtp = function (isValid)
    {
        
        var url = $rootScope.globalConfiguration.endpointUrl + "/SmtpConfigurationUsers/PostEditSmtp";
        var data = { value: JSON.stringify($scope.smtp) };
        $http.post(url, data)
            .success(function (dataOut) {
                if(dataOut.error != true)
                {
                    $translate('smtpEditSuccessEdition').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });       
                        
                    });
                    $localStorage.user = dataOut.user;
                }                  
                else
                {
                    $translate('smtpEditionErrorEdition').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });

                    });                  
                }
            })
            .error(function (dataOut) {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            });
    };
}

function configEmailTemplatesCtrl($scope, $rootScope, $http, $translate, notify, $localStorage,$uibModal)
{
    var url = $rootScope.globalConfiguration.endpointUrl + "/EmailTemplates/GetEmailTemplates";
    

    $http.get(url).success(function (dataOut) {
        $scope.templates = dataOut;
    });   

    $scope.create = function (template) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/configuration/email_templates/create_template.html',
            controller: createTemplateEmailCtrl,
            windowClass: "",
            resolve: {
                template: function () {
                    return template;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.templates = dataOut;
            });
        }, function () {
          
        });
    };

    $scope.view = function (template) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/configuration/email_templates/view_template.html',
            controller: viewTemplateEmailCtrl,
            windowClass: "",
            resolve: {
                template: function () {
                    return template;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.templates = dataOut;
            });
        }, function () {
            
        });
    };

    $scope.edit = function (template) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/configuration/email_templates/edit_template.html',
            controller: editTemplateEmailCtrl,
            windowClass: "",
            resolve: {
                template: function () {
                    return template;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.templates = dataOut;
            });
        }, function () {
            
        });
    };

    $scope.delete = function(template) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/configuration/email_templates/delete_template.html',
            controller: deleteTemplateEmailCtrl,
            windowClass: "",
            resolve: {
                template: function () {
                    return template;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.templates = dataOut;
            });
        }, function () {
            
        });
    };
}

function createTemplateEmailCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance) {

    
    $scope.template = { Type : "candidat"};
    $scope.create = function () {
        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/EmailTemplates/PostCreateTemplate";     
        var data = { value: JSON.stringify($scope.template) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('EmailTemplateSuccessNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close();
                }
                else {
                    $translate('EmailTemplateErrorNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log(response);
                });
            });
    };

    $scope.close = function() {
        $uibModalInstance.close();
    }
    
}

function viewTemplateEmailCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, template, $uibModalInstance) {
    $scope.template = template;
    $scope.close = function () {
        $uibModalInstance.close();
    };
}

function editTemplateEmailCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, template, $uibModalInstance) {
    
    $scope.template = template;
    $scope.edit = function () {

        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/EmailTemplates/PostEditTemplate";
        var data = { value: JSON.stringify($scope.template) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('EmailTemplateSuccessNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close();
                }
                else {
                    $translate('EmailTemplateErrorNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log(response);
                });
            });
    };

    $scope.close = function () {
        $uibModalInstance.close();
    };
}

function deleteTemplateEmailCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, template, $uibModalInstance) {
        
    $scope.delete = function () {
        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/EmailTemplates/PostDeleteTemplate";
        var data = { value: JSON.stringify(template) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('EmailTemplateSuccessNotifDelete').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close();
                }
                else {
                    $translate('EmailTemplateErrorNotifDelete').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log(response);
                });
            });
    };
    $scope.close = function () {
        $uibModalInstance.close();
    };
}

function configSpaceCtrl($scope, $rootScope, $http, $translate, notify, $localStorage)
{
    $scope.space = $localStorage.space;
    
    $scope.doUpload = function () {

        var files = document.getElementById("upload").files;
        var filename = $("#upload").val().replace(/.*(\/|\\)/, '');
        var url = $rootScope.globalConfiguration.endpointUrl + "/Spaces/UploadSpaceLogo";

        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append(filename, files[x]);
                }

                $.ajax({
                    type: "POST",
                    url: url,
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (dataOut) {                        
                        $scope.$apply(function () { $scope.space.Configuration.spaceLogoUrl = JSON.parse(dataOut).result; });
                    },
                    error: function (xhr, status, p3, p4) {
                        $translate('GeneralMessageError').then(function (translation) {
                            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        });
                    }
                });
            } else {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
        }
    };

    $scope.saveConfig = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/Spaces/PostSpaceForCurrentUser";
        var data = { value: JSON.stringify($scope.space) };
        $http.post(url, data)
            .success(function (dataout) {
                if(dataout.error == false)
                {
                    $translate('spaceConfigSuccessNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $localStorage.space = $scope.space;
                    $rootScope.$emit('spaceUpdated', $scope.space);
                }
                else
                {
                    $translate('spaceConfigErrorNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        console.log(dataout.errorMessage);
                    });
                }
            })
            .error(function () {
                 $translate('GeneralMessageError').then(function (translation) {
                                notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                            });
            });
    }
       
}


/*/
ALL DIRECTORY related controllers
/*/

function directoryUsersCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModal, $state)
{
    var url = $rootScope.globalConfiguration.endpointUrl + "/Accounts/GetUsers";
    

    $http.get(url).success(function (dataOut) {
        $scope.users = dataOut;
    });

    $scope.create = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/users/create_user.html',
            controller: createUserCtrl,
            windowClass: "",
        });

        modalInstance.result.then(function (user) {
            $http.get(url).success(function (dataOut) {
                $scope.users = dataOut;
            });

            if (user != null && user != {})
                $scope.edit(user);

        }, function () {
          
        });
    };

    $scope.edit = function (user) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/users/edit_user.html',
            controller: editUserCtrl,
            windowClass: "",
            resolve: {
                user: function () {
                    return user;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.users = dataOut;
            });
        }, function () {
            
        });
    };

    $scope.delete = function (user) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/users/delete_user.html',
            controller: deleteUserCtrl,
            windowClass: "",
            resolve: {
                user: function () {
                    return user;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.users = dataOut;
            });
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    };

    $scope.view = function (user) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/users/view_user.html',
            controller: viewUserCtrl,
            windowClass: "",
            resolve: {
                user: function () {
                    return user;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.users = dataOut;
            });
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    };

    $scope.editRole = function (user) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/users/edit_user_roles.html',
            controller: editUserRoleCtrl,
            windowClass: "",
            resolve: {
                user: function () {
                    return user;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.users = dataOut;
            });
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    };

    $scope.logAs = function (user)
    {
        var urlLogAs = $rootScope.globalConfiguration.endpointUrl + "/accounts/PostLogAs";
        var data = { value: JSON.stringify({ id: user.Id }) };
        $http.post(urlLogAs, data)
            .success(function (dataOut) {
                if (dataOut.error == false || dataOut.error == "false" || dataOut.error == "False" ) {
                    $translate('userSuccessNotifLogAs').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $localStorage.$reset();
                    $localStorage.ui = {};
                    $state.go($rootScope.globalConfiguration.defaultHomePage);
                }
                else {
                    $translate('userFailureNotifLogAs').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    };

    $scope.editSpace = function(user)
    {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/users/edit_user_space.html',
            controller: editUserSpaceCtrl,
            windowClass: "",
            backdrop: 'static',
            resolve: {
                user: function () {
                    return user;
                }
            }
        });

        modalInstance.result.then(function () {
            $http.get(url).success(function (dataOut) {
                $scope.users = dataOut;
            });
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    }
};

function createUserCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance){
    
    $scope.user = {};
    $scope.spaces = [];
    var urlSpaces = $rootScope.globalConfiguration.endpointUrl + "/Spaces/GetSpaces";   
    $http.post(urlSpaces).success(function (data) {
        $scope.spaces = data;
    });
    

    $scope.create = function () {
        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/accounts/PostCreateUser";
        var data = { value: JSON.stringify($scope.user) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('userSuccessNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close(dataOut.user);
                }
                else {
                    $translate('userErrorNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    };

    $scope.close = function () {
        $uibModalInstance.close();
    }
};

function editUserCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance, user){  
    
    $scope.user = user;
    $scope.edit = function () {

        var urlEdit = $rootScope.globalConfiguration.endpointUrl + "/accounts/PostEditUser";
        var data = { value: JSON.stringify($scope.user)};
        $http.post(urlEdit, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('userSuccessNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close();
                }
                else {
                    $translate('userErrorNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    };

    $scope.close = function () {
        $uibModalInstance.close();
    };
};

function deleteUserCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance, user) {    
    
    $scope.delete = function () {
        var urlDelete = $rootScope.globalConfiguration.endpointUrl + "/Accounts/PostDeleteUser";
        var data = { value: JSON.stringify(user) };
        $http.post(urlDelete, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('UserSuccessNotifDelete').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close();
                }
                else {
                    $translate('UserErrorNotifDelete').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        console.log(dataOut.errorMessage);
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    };
    $scope.close = function () {
        $uibModalInstance.close();
    };
};

function viewUserCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance, user) {
     $scope.user = user;
     $scope.close = function () {
        $uibModalInstance.close();
    };
};

function editUserRoleCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance, user)
{
    var url = $rootScope.globalConfiguration.endpointUrl + "/Accounts/GetPrivileges";
    $scope.privileges = [];
    $scope.user = user;
    $http.get(url).success(function (dataOut) {
        $scope.privileges = dataOut.privileges;        
    });
   
    var url = $rootScope.globalConfiguration.endpointUrl + "/Accounts/GetRoles";
    var data = JSON.stringify({ userId: $scope.user.Id });   
    $http.get(url + "?value=" + data).success(function (dataOut) {
        $scope.userRoles = dataOut;       
    });

    $scope.close = function () {
        $uibModalInstance.close();
    };

    $scope.add = function(privilege)
    {
        var urlEditRole = $rootScope.globalConfiguration.endpointUrl + "/accounts/PostEditUserRoles";
        var data = { value: JSON.stringify({ name: privilege.code, id: $scope.user.Id }) };
        $http.post(urlEditRole, data)
            .success(function (dataOut) {
                if (dataOut.error == false) {
                    $translate('userSuccessNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });

                    $scope.userRoles = dataOut.response;
                }
                else {
                    $translate('userErrorNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    }

    $scope.remove = function (privilege)
    {
        var urlEditRole = $rootScope.globalConfiguration.endpointUrl + "/accounts/PostDeleteUserRoles";
        var data = { value: JSON.stringify({ name: privilege, id: $scope.user.Id }) };
        $http.post(urlEditRole, data)
            .success(function (dataOut) {
                if (dataOut.error == false) {
                    $translate('userSuccessNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $scope.userRoles = dataOut.response;
                }
                else {
                    $translate('userErrorNotifEdit').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    }

    $scope.getDiff = function ()
    {
        var baz = [];
        for (var i = $scope.privileges.length - 1; i >= 0; i--) {
          var key = $scope.privileges[i].code;
          if (-1 === $scope.userRoles.indexOf(key)) {
              baz.push($scope.privileges[i]);
          }
        }
        return baz;
    }
};

function editUserSpaceCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance, UserManager, user)
{
    $scope.spaces = [];
    $scope.user = user;
    $scope.spaceId = UserManager.getSpaceIdFromUser(user);
    

    $scope.getSpaces = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/spaces/GetSpaces";
        $http.get(url).success(function (dataOut) {
            $scope.spaces = dataOut;
            for (var i = 0; i < $scope.spaces.length; i++) {
                $scope.spaces[i].Configuration = JSON.parse($scope.spaces[i].Configuration);
            }
        });
    };

    $scope.processFormEditUserSpace = function (isValid)
    {
        if (!isValid) {
            $translate('GeneralFormInvalid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }

        var urlEditUserSpace = $rootScope.globalConfiguration.endpointUrl + "/accounts/PostEditUserSpace";
        var data = { value: JSON.stringify({ userId: user.Id, spaceId: $scope.spaceId }) };
        $http.post(urlEditUserSpace, data).success(function (dataOut) {
            if (dataOut.error == false) {
                $translate('userSuccessNotifEdit').then(function (translation) {
                    notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });                
            }
            else {
                $translate('userErrorNotifEdit').then(function (translation) {
                    notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
                console.log(dataOut.errorMessage);
            }
        }).error(function () {
            $translate('GeneralMessageError').then(function (translation) {
                notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
               
            });
        });
    }

    $scope.close = function () {
        $uibModalInstance.close();
    };

    $scope.getSpaces();
}

function spacesCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModal)
{
    $scope.spaces = [];      
    
    

    $scope.create = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/spaces/create_space.html',
            controller: createSpaceModalCtrl,
            size: 'lg',
            windowClass: "",
        });

        modalInstance.result.then(function (space) {
            $scope.getSpaces();
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    }

    $scope.edit = function (space) {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/spaces/edit_space.html',
            controller: editSpaceModalCtrl,
            size : "lg",
            windowClass: "",
            resolve: {
                space: function () {
                    return space;
                }
            }
        });

        modalInstance.result.then(function () {
            $scope.getSpaces();
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    }

    $scope.delete = function (space)
    {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/directory/spaces/delete_space.html',
            controller: deleteSpaceModalCtrl,
            size: "lg",
            windowClass: "",
            resolve: {
                space: function () {
                    return space;
                }
            }
        });

        modalInstance.result.then(function () {
            $scope.getSpaces();
        }, function () {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    }

    $scope.getSpaces = function () {
        var url = $rootScope.globalConfiguration.endpointUrl + "/spaces/GetSpaces";
        $http.get(url).success(function (dataOut) {
            $scope.spaces = dataOut;
            for (var i = 0; i < $scope.spaces.length; i++) {
                $scope.spaces[i].Configuration = JSON.parse($scope.spaces[i].Configuration);
            }
        });
    };

    $scope.getSpaces();
}

function createSpaceModalCtrl($scope, $rootScope, $http, $translate, notify, $localStorage, $uibModalInstance)
{
    
    $scope.space = { Configuration: {}};
   
    $scope.saveConfig = function () {
        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/spaces/PostSpace";
        var data = { value: JSON.stringify($scope.space) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('spaceSuccessNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close(dataOut.space);
                }
                else {
                    $translate('spaceErrorNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    };

    $scope.doUpload = function () {

        var files = document.getElementById("upload").files;
        var filename = $("#upload").val().replace(/.*(\/|\\)/, '');
        var url = $rootScope.globalConfiguration.endpointUrl + "/Spaces/UploadSpaceLogo";

        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append(filename, files[x]);
                }

                $.ajax({
                    type: "POST",
                    url: url,
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (dataOut) {
                        $scope.$apply(function ()                        {
                            $scope.space.Configuration.spaceLogoUrl = JSON.parse(dataOut).result;
                        });
                    },
                    error: function (xhr, status, p3, p4) {
                        $translate('GeneralMessageError').then(function (translation) {
                            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        });
                    }
                });
            } else {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
        }
    };

    $scope.close = function () {
        $uibModalInstance.close();
    }

    $scope.spinOptionSizeLimit = {
        min: 0,
        max: 100,
        initval: 0,
        step: 1,
        decimals: 1,
        boostat: 5,
        maxboostedstep: 10
    };
}

function editSpaceModalCtrl($scope, $rootScope, $http, $translate, notify, $localStorage,$uibModalInstance, space)
{
    
    $scope.space = space;

    $scope.saveConfig = function () {
        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/spaces/PostSpace";
        var data = { value: JSON.stringify($scope.space) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error != true) {
                    $translate('spaceSuccessNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close(dataOut.space);
                }
                else {
                    $translate('spaceErrorNotifCreate').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    };

    $scope.doUpload = function () {

        var files = document.getElementById("upload").files;
        var filename = $("#upload").val().replace(/.*(\/|\\)/, '');
        var url = $rootScope.globalConfiguration.endpointUrl + "/Spaces/UploadSpaceLogo";

        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append(filename, files[x]);
                }

                $.ajax({
                    type: "POST",
                    url: url,
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (dataOut) {
                        $scope.$apply(function () {
                            $scope.space.Configuration.spaceLogoUrl = JSON.parse(dataOut).result;
                        });
                    },
                    error: function (xhr, status, p3, p4) {
                        $translate('GeneralMessageError').then(function (translation) {
                            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        });
                    }
                });
            } else {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
        }
    };

    $scope.close = function () {
        $uibModalInstance.close();
    }

    $scope.spinOptionSizeLimit = {
        min: 0,
        max: 100,
        initval: 0,
        step: 1,
        decimals: 1,
        boostat: 5,
        maxboostedstep: 10
    };
}

function deleteSpaceModalCtrl($scope, $rootScope, $http, $translate, notify, $localStorage,$uibModalInstance, space)
{
    
    $scope.space = space;

     $scope.close = function () {
        $uibModalInstance.close();
    }
    
    $scope.delete = function()
    {
        var urlCreate = $rootScope.globalConfiguration.endpointUrl + "/spaces/DeleteSpace";
        var data = { value: JSON.stringify($scope.space) };
        $http.post(urlCreate, data)
            .success(function (dataOut) {
                if (dataOut.error == false) {
                    $translate('spaceSuccessNotifDelete').then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                    $uibModalInstance.close();
                }
                else {
                    $translate('spaceErrorNotifDelete').then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function () {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log();
                });
            });
    }
}

/*/
ALL TRASH related controller
/*/
function trashListCtrl($scope, $rootScope, $http, $translate, $uibModal, notify, $state, $localStorage) {
    var url = $rootScope.globalConfiguration.endpointUrl + "/trash/GetDeletedItems";
    $scope.trashs = $localStorage.trashs ? $localStorage.trashs : [];
    $scope.trashSelected = $localStorage.trashSelected ? $localStorage.trashSelected : [];
    $scope.quantity = 10;
    if (!$localStorage.trashs)
        $scope.loading = true;

    $http.get(url).success(function (dataOut) {
        $scope.trashs = dataOut.data;
        $scope.loading = false;
        $localStorage.trashs = dataOut.data;
    })
    .error(function (dataOut) {
        $scope.loading = false;
    });

    $scope.moreResult = function (quantity) {
        $scope.quantity = $scope.quantity + 10;
    }

    $scope.$watch(function () { return $scope.moreResultIsVisible }, function () {
        if ($scope.moreResultIsVisible)
            $scope.moreResult($scope.quantity);
    });

    $scope.$watch(function () { return $scope.trashSelected }, function () {
        $localStorage.trashSelected = $scope.trashSelected;
    });

    $scope.deleteForeverOrRestore = function (items, list, urlRelativeApi) {
        $scope.removeItems(items, list);
        var url = $rootScope.globalConfiguration.endpointUrl + urlRelativeApi;
        var data = { value: JSON.stringify(items) };
        $http.post(url, data)
            .success(function (data) {
                if (data.error == false) {
                    $translate(data.notification).then(function (translation) {
                        notify({ message: translation, classes: 'alert-info', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
                else {
                    $translate(data.notification).then(function (translation) {
                        notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    });
                }
            })
            .error(function (data) {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                    console.log(response);
                });
            });
    }
}


/*/
ALL PAYMENT related controller
/*/

function paymentByCardCtrl($scope, $rootScope, $rootScope, $state, $http, notify, $location, $translate, $localStorage) {
    $scope.formData = {};
    
    $scope.loadingForm = false;

    if ($localStorage.Orders != null)
        $scope.formData.amount = $localStorage.Orders[0].amount;
    else
        $state.go('purchase.purchase_bitcoin');

    // Function to process the payment form
    $scope.processForm = function (isValid) {
         
        if ($location.protocol() != "https" && !window.location.hostname.includes("localhost")) {
            $translate("PaymentCardProtocolInvalid").then(function (translation) {
                notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }

        if (!isValid) {
            $translate('GeneralFormInvalid').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
            });
            return;
        }

        $scope.loadingForm = true;
        var dataToSend = {};
        dataToSend.paymentCardDetails = $scope.formData;
        dataToSend.purchaseDetails = $localStorage.Orders[0];

        var urlPaymentByCard = $rootScope.globalConfiguration.endpointUrl + "/Payment/PostPaymentByCard";
        var data = { value : JSON.stringify(dataToSend)};
        $http.post(urlPaymentByCard,data)
          .success(function (dataOut, status) {
              if (dataOut.error == "false") {
                  $translate('paymentCardSuccessfull').then(function (translation) {
                      notify({ message: translation, classes: 'alert-success', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                  });
              }
              else {
                  $translate('paymentCardError').then(function (translation) {
                      notify({ message: translation, classes: 'alert-warning', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                  });
              }
              $scope.loadingForm = false;
          })
          .error(function () {
              $translate('GeneralMessageError').then(function (translation) {
                  notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
              });
              $scope.loadingForm = false;
          });

    }
    
    //StepBack
    $scope.stepBack = function () {        
        $state.go('purchase.purchase_bitcoin');
    }
}

/*/
ALL PROFILE related controller
/*/

function takeShotCtrl($scope, $rootScope, $state, $http, notify, $location, $translate, $localStorage)
{
    
}

/*/
CONTROLLER DIRECTIVE
/*/

function spaceLogoPickerCtrl($scope, $rootScope, $http, $translate, notify) {
    
    $scope.spaceLogoUrl = '';
    $scope.doUpload = function () {

        var files = document.getElementById("upload").files;
        var filename = $("#upload").val().replace(/.*(\/|\\)/, '');
        var url = $rootScope.globalConfiguration.endpointUrl + "/Spaces/UploadSpaceLogo";

        if (files.length > 0) {
            if (window.FormData !== undefined) {
                var data = new FormData();
                for (var x = 0; x < files.length; x++) {
                    data.append(filename, files[x]);
                }

                $.ajax({
                    type: "POST",
                    url: url,
                    contentType: false,
                    processData: false,
                    data: data,
                    success: function (dataOut) {                       
                        $scope.spaceLogoUrl = JSON.parse(dataOut).result;
                    },
                    error: function (xhr, status, p3, p4) {
                        $translate('GeneralMessageError').then(function (translation) {
                            notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                        });
                    }
                });
            } else {
                $translate('GeneralMessageError').then(function (translation) {
                    notify({ message: translation, classes: 'alert-danger', templateUrl: $rootScope.notifTemplate, duration: $scope.notifDuration });
                });
            }
        }
    };
}

function imageCrop($scope) {
    $scope.myImage='';
    $scope.myCroppedImage='';

    var handleFileSelect=function(evt) {
        var file=evt.currentTarget.files[0];
        var reader = new FileReader();
        reader.onload = function (evt) {
            $scope.$apply(function($scope){
                $scope.myImage=evt.target.result;
            });
        };
        reader.readAsDataURL(file);
    };
    angular.element(document.querySelector('#fileInput')).on('change',handleFileSelect);
};

/*/
HELPERS
/*/

function guid() {
    function s4() {
        return Math.floor((1 + Math.random()) * 0x10000)
          .toString(16)
          .substring(1);
    }
    return s4() + s4() + '-' + s4() + '-' + s4() + '-' +
      s4() + '-' + s4() + s4() + s4();
}


/**
 *
 * Pass all functions into module
 */
angular
    .module('coduco')
    .controller('MainCtrl', MainCtrl)
    .controller('headerCtrl', headerCtrl)
    .controller('translateCtrl', translateCtrl)
    .controller('spaceLogoPickerCtrl', spaceLogoPickerCtrl)
    .controller('imageCrop', imageCrop)
    .controller('paymentByCardCtrl', paymentByCardCtrl)
    .controller('isAuthenticatedCtrl', isAuthenticatedCtrl)
    .controller('buildingListCtrl', buildingListCtrl)
    .controller('buildingCreateOrUpdateCtrl', buildingCreateOrUpdateCtrl);
    

    