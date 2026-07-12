var app = angular.module('app');

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("AssetInspectionCtrl", function ($timeout, $scope, $http) {
    $scope.loading = false;
    initializeComponents();

    $scope.InspectionCollData = function () {
        console.log($scope.AssetId);
        var _formCSV = $("#_formInspectionInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        console.log(_eData);
        $scope.loading = true;
        $http({
            method: 'POST',
            url: '../AssetInspection/CreateInspection',
            data: _eData
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                document.getElementById("_formInspectionInfo").reset();
                GetInitComp();
                toastr.success(response.data.Message);
            }
            else {
                toastr.error(response.data.Message);
            }
            $scope.loading = false;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
            $scope.loading = false;
        });
    };

    $scope.GetAssetInfo = function () {
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        $scope.loading = true;
        $http({
            method: 'POST',
            url: '../AssetInspection/Create',
            data: _eData
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                $scope.AssetId = response.data._AssetList.tAssetTagId;                $scope.AssetInfo = response.data._AssetList;
                var ddl = $("#mIteamMasterId").data("kendoDropDownList");

                if (ddl) {

                    ddl.value(response.data._AssetList.tAssetTagId);

                }
                $scope.InspInfo = response.data.InspData;
                toastr.success(response.data.Message);
            }
            else {
                toastr.error(response.data.Message);
            }
            $scope.loading = false;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
            $scope.loading = false;
        });
    };

    $scope.GetbyTabInfo = function () {
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        $http({
            method: 'POST',
            url: '../AssetInspection/Create',
            data: _eData
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                $scope.AssetId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.InspInfo = response.data.InspData;
            }
            else {
                $scope.AssetMasterId = null;
                $scope.AssetInfo = null;
                $scope.InspInfo = null;
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
            url: '../AssetInspection/Create',
            data: _eData
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                $scope.AssetId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.InspInfo = response.data.InspData;
            }
            else {
                $scope.AssetMasterId = null;
                $scope.AssetInfo = null;
                $scope.InspInfo = null;
                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function GetInitComp() {
        $http({
            method: 'GET',
            url: '../AssetInspection/InitData'
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                $timeout(function() {
                   $('#mIteamMasterId').kendoDropDownList({

                        dataTextField: "IteamName",
                        dataValueField: "mIteamMasterId",
                        filter: "contains",
                        dataSource: response.data.AssetList,
                        suggest: true,

                        change: function () {

                            var value = this.value();

                            $scope.$apply(function () {
                                $scope.AssetId = value;
                            });

                            $("input[name='AssetId']").val(value);

                            console.log("AssetId = " + value);
                        }

                    });

                    var mIteamMasterId = $("#mIteamMasterId").data("kendoDropDownList");
                    if (mIteamMasterId) {
                        mIteamMasterId.value(-1);
                    }
                }, 100);
            }
            else {
                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function initializeComponents() {
        GetInitComp();
    };

});
