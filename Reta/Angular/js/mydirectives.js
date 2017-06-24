
// Candidat Selector

function candidatSelector() {
    return {
        restrict: 'A',
        templateUrl: 'views/directives/candidat_selector.html',
        controller: candidatSelectorCtrl      
    };
}

function candidatSelectorCtrl($scope, $http, $translate, notify, $uibModal, ObjectManager)
{
    $scope.notifTemplate = 'views/common/notify.html';
    $scope.candidats = [];   
    ObjectManager.object = [];
     var urlGetCandidats = window.location.protocol + '//' + window.location.hostname + ":" + window.location.port + "/candidats/GetCandidats";
    $http.get(urlGetCandidats).success(function (dataOut) {        
        $scope.candidats = dataOut.data;
    });

    $scope.addCandidat = function (candidat) {
        if (candidat == null || candidat != $scope.selectedItem)
            return;

        if (ObjectManager.object.indexOf(candidat) < 0)
            ObjectManager.object.push(candidat);
        else
        {
            $translate('directiveNotifCandidatAlreadySelected').then(function (translation) {
                notify({ message: translation, classes: 'alert-warning', templateUrl:$scope.notifTemplate });
            });
        }

        $scope.searchText = "";
    }

    $scope.showCreationCandidatForm = function () {
        var modalInstance = $uibModal.open({
            templateUrl: 'views/candidat_create.html',
            controller: candidatCreateCtrl,
            size: 'lg',
            backdrop: 'static',
            windowClass: "zindex-high"
        });

        modalInstance.result.then(function (candidat) {
            // add candidat to list of candidats for the edition
            $http.get(urlGetCandidats).success(function (dataOut) {
                $scope.candidats = dataOut.data;               
            });
            addCandidat(candidat);
        }, function (candidat) {
            // console.log("modal creation dismissed " + candidat.Id);
        });
    }        
}

// Translation relative to some stuff in directives ;)

function ngEnter() {
    return function (scope, element, attrs) {
        element.bind("keydown keypress", function (event) {
            if(event.which === 13) {
                scope.$apply(function (){
                    scope.$eval(attrs.ngEnter);
                });

                event.preventDefault();
            }
        });
    };
}

function editableLabel()
{
    return {
        restrict: 'A',
        scope: {
            mymodel: '=',
            labelclass: '@labelclass',
            inputclass: '@inputclass',

        },
        template: '<div ng-mouseover="titleMouseOver = true;" ng-mouseleave="titleMouseOver = false;" ng-click="titleClicked = true;" ng-show="!titleClicked" class="{{labelclass}}" ng-cloak> {{ mymodel }} <i ng-show="titleMouseOver" class="fa fa-edit"></i></div><input ng-show="titleClicked" show-focus="titleClicked" ng-blur="titleClicked = false;" class="{{inputclass}}" ng-model="mymodel" type="text" ng-cloak />',
        controller: editableLabelCtrl
    };
}

function editableLabelCtrl($scope)
{
    $scope.titleMouseOver = false;
    $scope.titleClicked = false;
}

function config($translateProvider) {
    $translateProvider
        .translations('en', {
            directiveNotifCandidatAlreadySelected : "This candidat is already selected",
        })
        .translations('fr', {
            directiveNotifCandidatAlreadySelected: "Ce candidat est déjà sélectionné",
        });
}

angular
    .module('coduco')
    .config(config)
    .controller('candidatSelectorCtrl', candidatSelectorCtrl)
    .directive('candidatSelector', candidatSelector)
    .directive('ngEnter', ngEnter)
    .controller('editableLabelCtrl', editableLabelCtrl)
    .directive('editableLabel', editableLabel);