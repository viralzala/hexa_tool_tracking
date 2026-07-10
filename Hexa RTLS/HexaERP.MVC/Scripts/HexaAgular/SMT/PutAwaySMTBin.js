// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("PutAwaySMTBinCtrl", function ($timeout, $scope, $http, $window) {


    $scope._BinInfo = [];
    $scope.submit = false;


    $(function () {
        document.getElementById('ModifiedBy').focus();
        toastr.options = {
            positionClass: 'toast-top-center',
            timeOut: 10000
        };
    });

    initializeComponets();
    //
    function initializeComponets() {
        //$timeout(function () { $('#ReaderNo').focus(); });
        //modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        //setTimeout(function () {
        //    modal.hide();
        //}, 1000);        
    };

    function reset(id) { $(`#${id}`).val(""); };
    function focus(id) {
        document.getElementById(`${id}`).focus();
    };

    $("#ModifiedBy").on("keypress", function (event) {
        if (event.which == 13) {
            focus(`RFID`);
        }
    });

    $scope.GetBinIn = function () {
        GetBinDetails();
    };

    $scope.SubmitBinIn = function () {

        if ((angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null) && (angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null)) {
            toastr.error('Employee Id is required');
            focus(`ModifiedBy`);
            return false;
        }

        $http({
            method: 'GET',
            url: '../PutAwaySMTBin/GetBinPosition',
            params: { LocatonBleMac: $scope.ReaderNo, _ProductShelf: $scope.RFID }
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                //$scope.locationPosition = response.data;
                $(`#ShelfName`).html(response.data._location.ShelfName);
                //$(`#Zone`).html(response.data._location.ShelfName);
                reset('ReaderNo');
                //focus('RFID');
                focus('ModifiedBy');
                toastr.success(`${response.data.message}`);
                SubmiteRequest();
            } else {
                $scope.submit = false;
                toastr.error(response.data.message);
                reset('ReaderNo');
                focus('ReaderNo');
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $("#ReaderNo").on("keypress", function (event) {
        //console.log($scope.submit);
        if (event.which == 13) {

            if ((angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null) && (angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null)) {
                toastr.error('Employee Id is required');
                return false;
            }

            $http({
                method: 'GET',
                url: '../PutAwaySMTBin/GetBinPosition',
                params: { LocatonBleMac: $scope.ReaderNo, _ProductShelf: $scope.RFID }
            }).then(function successCallback(response) {
                console.log(response.data);
                if (response.data.Flag == true) {
                    //$scope.locationPosition = response.data;
                    $(`#ShelfName`).html(response.data._location.ShelfName);
                    //$(`#Zone`).html(response.data._location.ShelfName);
                    reset('ReaderNo');
                    //focus('RFID');
                    focus('ModifiedBy');
                    toastr.success(`${response.data.message}`);
                    SubmiteRequest();
                } else {
                    $scope.submit = false;
                    toastr.error(response.data.message);
                    reset('ReaderNo');                 
                    focus('ReaderNo');
                }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    });

    function GetBinDetailsReset() {
        $http({
            method: 'GET',
            url: '../PutAwaySMTBin/GetBinInfo',
            params: { BinNumber: $scope.RFID }
        }).then(function successCallback(response) {
            $scope.BinInfo = response.data._BinInfo;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function GetBinDetails() {

        $http({
            method: 'GET',
            url: '../PutAwaySMTBin/GetBinInfo',
            params: { BinNumber: $scope.RFID }
        }).then(function successCallback(response) {
            //console.log(response.data);
            if (response.data.Flag == true) {
                $scope.BinInfo = response.data._BinInfo;
                focus('ReaderNo');
            } else {
                toastr.error(response.data.message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $("#RFID").on("keypress", function (event) {
        if (event.which == 13) {
            //console.log($(`#ShelfName`).html());
            GetBinDetails();
        };
    });


    function SubmiteRequest() {
        $http({
            method: 'GET',
            url: '../PutAwaySMTBin/SubmitPutAwaySMTBin',
            params: {
                ModifiedBy: $("#ModifiedBy").val(), mSMTProductId: parseInt($('#example1 tr:first-child td:nth-child(1)').text())
            }
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                $('#resetForm')[0].reset();
                reset('mZoneId');
                $("#Zone").empty();
                $("#ShelfName").empty();
                GetBinDetailsReset();
                toastr.success(`${response.data.message}`);
            } else {
                toastr.error(response.data.message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
            toastr.error(`Product not found or internal error`);
        });
    };

    //
    $scope.submitBinTag = function () {

        if ((angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null) && (angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null)) {
            toastr.error('Employee Id is required');
            return false;
        }

        //if ((angular.isUndefined($scope.RFID) || $scope.RFID === null) && (angular.isUndefined($scope.RFID) || $scope.RFID === null)) {
        //    toastr.error('Bin BLE Tag ID Required');
        //    return false;
        //}

        //modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        //console.log($('#example1 tr:first-child td:nth-child(1)').text());
        //console.log($("#ModifiedBy").val());

        $http({
            method: 'GET',
            url: '../PutAwaySMTBin/SubmitPutAwaySMTBin',
            params: {
                ModifiedBy: $("#ModifiedBy").val(), mSMTProductId: parseInt($('#example1 tr:first-child td:nth-child(1)').text())
            }
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                reset('mZoneId');
                $("#Zone").empty();
                $("#ShelfName").empty();
                $('#resetForm')[0].reset();
                //$('#example1').find("tr:gt(0)").remove();
                toastr.success(`${response.data.message}`);
            } else {
                toastr.error(response.data.message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
            toastr.error(`Product not found or internal error`);
        });
    };


});

