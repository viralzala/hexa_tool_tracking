var app = angular.module('app');

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("AssetCalibrationCtrl", function ($timeout, $scope, $http) {
    initializeComponets();

    $scope.CalibCollData = function () {
        var _formCSV = $("#_formCalibInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../AssetCalibration/CreateCalib',
            data: _eData
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                document.getElementById("_formCalibInfo").reset();
                GetInitComp();
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.success(response.data.Message);
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)

                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.GetAssetInfo = function () {
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../AssetCalibration/Create',
            data: _eData
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                $scope.tAssetTagId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.CalibInfo = response.data.CalibData;
                $scope.AssetMasterId = response.data._AssetList.tAssetTagId;
                $scope.AssetId = response.data._AssetList.RFID;
                $scope.AssetName = response.data._AssetList.IteamName;

                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.success(response.data.Message);
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)

                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.GetbyTabInfo = function () {
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        $http({
            method: 'POST',
            url: '../AssetCalibration/Create',
            data: _eData
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                $scope.tAssetTagId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.CalibInfo = response.data.CalibData;
                $scope.AssetMasterId = response.data._AssetList.tAssetTagId;
                $scope.AssetId = response.data._AssetList.RFID;
                $scope.AssetName = response.data._AssetList.IteamName;
            }
            else {
                $scope.tAssetTagId = null;
                $scope.AssetInfo = null;
                $scope.CalibInfo = null;
                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function getAssetByFun() {

        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        $http({
            method: 'POST',
            url: '../AssetCalibration/Create',
            data: _eData
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                $scope.tAssetTagId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.CalibInfo = response.data.CalibData;
            }
            else {
                $scope.tAssetTagId = null;
                $scope.AssetInfo = null;
                $scope.CalibInfo = null;
                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function GetInitComp() {
        $http({
            method: 'GET',
            url: '../AssetCalibration/InitData'
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                $('#mIteamMasterId').kendoDropDownList({
                    dataTextField: "IteamName",
                    dataValueField: "mIteamMasterId",
                    filter: "contains",
                    dataSource: response.data.AssetList,
                    suggest: true,
                    index: 2
                });

                var mIteamMasterId = $("#mIteamMasterId").data("kendoDropDownList");
                mIteamMasterId.value(-1);

                setTimeout(function () {
                    modal.hide()
                }, 1000)
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)

                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function initializeComponets() {
        $scope.loading = true;
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        GetInitComp();
        GetStatistics();
        setTimeout(function () {
            modal.hide()
            $scope.$apply(function() {
                $scope.loading = false;
            });
        }, 1000)

    };

    function GetStatistics() {
        $http({
            method: 'GET',
            url: '../AssetCalibration/GetStatistics'
        }).then(function successCallback(response) {
            $scope.statTotal = response.data.Total;
            $scope.statCompleted = response.data.Completed;
            $scope.statPending = response.data.Pending;
            $scope.statExpired = response.data.Expired;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

});