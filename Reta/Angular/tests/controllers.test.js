describe('controllers', function () {

  beforeEach(module('coduco'));

  var $controller;

  beforeEach(inject(function(_$controller_){
    $controller = _$controller_;
  }));

  describe('mainCtrlTest', function () {
		it('has notif template set up', function () {
			var $scope = {};
			var controller = $controller('MainCtrl', { $scope: $scope });
			expect($scope.notifTemplate).toEqual('views/common/notify.html');
		});	
	});

});