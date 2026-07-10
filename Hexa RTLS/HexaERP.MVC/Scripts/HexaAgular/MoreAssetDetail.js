/*  */
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("MoreAssetDetailCtrl", function ($timeout, $scope, $http) {
    initializeComponets();
    //


    //
    $scope.CheckIn = function (_eid, _aId) {

        UIkit.modal.confirm('Do you want to checkin asset & remove this user?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            $http({
                method: 'GET',
                url: '../MoreAssetDetail/CheckIn',
                params: { eId: _eid, aId: _aId }
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {

                    getAssetByFun();
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
        });

    };
    //
    $scope.IssueForm = function () {
        UIkit.modal.confirm('Are you sure to Isuue Asset?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            var _formCSV = $("#_formIssue");
            var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

            $http({
                method: 'POST',
                url: '../MoreAssetDetail/CheckOutAsset',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    //document.getElementById("_formIssue").reset();
                    getAssetByFun();
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
        });

    };
    //
    $scope.MaintCollData = function () {
        var _formCSV = $("#_formMaintInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../MoreAssetDetail/CreateMaint',
            data: _eData
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                document.getElementById("_formMaintInfo").reset();
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
    //
    $scope.StatusCollData = function () {
        var _formCSV = $("#_formStatusInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../MoreAssetDetail/CreateSatatus',
            data: _eData
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                //document.getElementById("_formStatusInfo").reset();
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
    //
    $scope.GetAssetInfo = function () {
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../MoreAssetDetail/Create',
            data: _eData
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                $scope.tAssetTagId = response.data._AssetList.tAssetTagId;               
                $scope.AssetInfo = response.data._AssetList;
                $scope.MaintInfo = response.data.MaintData;
                $scope.AssetHist = response.data.inoutHitor;

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

    //
    $scope.GetbyTabInfo = function () {
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        $http({
            method: 'POST',
            url: '../MoreAssetDetail/Create',
            data: _eData
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                $scope.tAssetTagId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.MaintInfo = response.data.MaintData;
                $scope.AssetHist = response.data.inoutHitor;
            }
            else {
                $scope.tAssetTagId = null;
                $scope.AssetInfo = null;
                $scope.MaintInfo = null;
                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };
    //
    function getAssetByFun() {
        
        var _formCSV = $("#_formAssetInfo");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        $http({
            method: 'POST',
            url: '../MoreAssetDetail/Create',
            data: _eData
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                $scope.tAssetTagId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
                $scope.MaintInfo = response.data.MaintData;
                $scope.AssetHist = response.data.inoutHitor;
            }
            else {
                $scope.tAssetTagId = null;
                $scope.AssetInfo = null;
                $scope.MaintInfo = null;
                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };
    //
    function GetInitComp() {
        $http({
            method: 'GET',
            url: '../MoreAssetDetail/InitData'
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                $('#mMaintenanceTypeId').kendoDropDownList({
                    dataTextField: "MaintenanceName",
                    dataValueField: "mMaintenanceTypeId",
                    filter: "contains",
                    dataSource: response.data.MaintType,
                    suggest: true,
                    index: 2
                });

                var mMaintenanceTypeId = $("#mMaintenanceTypeId").data("kendoDropDownList");
                mMaintenanceTypeId.value(-1);


                $('#mStatusMasterId').kendoDropDownList({
                    dataTextField: "StatusName",
                    dataValueField: "mStatusMasterId",
                    filter: "contains",
                    dataSource: response.data.statusType,
                    suggest: true,
                    index: 2
                });

                var mStatusMasterId = $("#mStatusMasterId").data("kendoDropDownList");
                mStatusMasterId.value(-1);


                $('#tEmployeeTagId').kendoDropDownList({
                    dataTextField: "EmployeeName",
                    dataValueField: "tEmployeeTagId",
                    filter: "contains",
                    dataSource: response.data.EmpList,
                    suggest: true,
                    index: 2
                });

                var tEmployeeTagId = $("#tEmployeeTagId").data("kendoDropDownList");
                tEmployeeTagId.value(-1);

                setTimeout(function () {
                    modal.hide()
                }, 1000)
                //toastr.success(response.data.Message);
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
    //
    function initializeComponets() {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        GetInitComp();
        GetStatistics();
        setTimeout(function () {
            modal.hide()
        }, 1000)

    };
    //
    function GetStatistics() {
        $http({
            method: 'GET',
            url: '../MoreAssetDetail/GetStatistics'
        }).then(function successCallback(response) {
            $scope.statTotal = response.data.Total;
            $scope.statCompleted = response.data.Completed;
            $scope.statInProgress = response.data.InProgress;
            $scope.statOverdue = response.data.Overdue;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    $scope.uploadAssetImg = function () {

        var data = new FormData();
        var files = $("#user_edit_avatar_control").get(0).files;

        if (files.length > 0) {
            data.append("HelpSectionImagess", files[0]);
        }
        else {
            UIkit.modal.alert('<p>Please select Image to upload.</p>');
            return false;
        }
        var extension = $("#user_edit_avatar_control").val().split('.').pop().toUpperCase();

        if (extension != "JPG" && extension != "JPEG" && extension != "PNG" && extension != "PDF") {
            UIkit.modal.alert('<p>Imvalid file format.</p>');
            return false;
        } else {

            $.ajax({
                url: '../MoreAssetDetail/uploadImg', type: "POST", processData: false,
                data: new FormData($('#ImgUpl')[0]),
                dataType: 'json',
                contentType: false,
                success: function (response, textStatus, xhr) {

                    console.log(response);
                    if (response.result == true) {
                        getAssetByFun();
                        toastr.success(response.message);
                    }
                    else {
                        toastr.error(response.message);
                    }
                    //document.getElementById("#_DocumentUploadForm").reset();
                },
                error: function () {
                    //console.log(response);
                }
            });
        }

    };

});