app.controller("AdminHomeCtrl", function ($scope, $http) {
    initializeComponets();


    //
    $scope.StartService = function () {
        UIkit.modal.confirm('Do you want Start Service?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please wait...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            $http({
                method: 'GET',
                url: '../SuperAdmin/startService'
            }).then(function successCallback(response) {
                console.log(response);
                GetStatus();
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                if (response.data.Flag == true) { toastr.success(response.data.Message); } else { toastr.error(response.data.Message); }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        });
    };

    //
    $scope.StopService = function () {
        UIkit.modal.confirm('Do you want Stop Service?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please wait...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            $http({
                method: 'GET',
                url: '../SuperAdmin/StopService'
            }).then(function successCallback(response) {
                //console.log(response);
                GetStatus();
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                if (response.data.Flag == true) { toastr.success(response.data.Message); } else { toastr.error(response.data.Message); }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        });
    };
    //
    function initializeComponets() {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please wait...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        GetStatus();
        setTimeout(function () {
            modal.hide()
        }, 1000)
    };

    function GetStatus() {
        $http({
            method: 'GET',
            url: '../SuperAdmin/HexaServiceStatus'
        }).then(function successCallback(response) {
            console.log(response.data);
            $scope.HexaService = response.data;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});