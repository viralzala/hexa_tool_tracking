// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("AssignWorkCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();
    $scope.empIds = [];

    $scope.submitworkList = function () {

        var ep = $(".employee-ichecked-id:checked").map(function () {
            return this.value;
        }).get();
        if (ep.length == 0) {
            alert('Select Employee');
            return false
        }

        var data = new FormData();
        var _form = $("#formzonecollection");
        var _formData = JSON.stringify(_form.serializeObject(), null, 2);
        var d = JSON.parse(_formData);
        data.append("EmployeeIds", ep);
        data.append("ZoneIds", d.mZoneId);
        data.append("mShiftId", parseInt($("#mShiftId").value()));
        console.log(...data);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../AssignWork/AssigWorkZone',
            TransformStream: angular.identity,
            headers: { 'Content-Type': undefined },
            data: data
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                toastr.success(response.data.Message);
                setTimeout(function () {
                    modal.hide()
                }, 1000);
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000);
                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });


    };
    //
    function initializeComponets() {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        BindZone();
        $http({
            method: 'GET',
            url: '../EmployeeTag/getMasterData'
        }).then(function successCallback(response) {
            //console.log(response.data);
            $('#mShiftId').kendoDropDownList({
                dataTextField: "Shift",
                dataValueField: "mShiftId",
                filter: "contains",
                dataSource: response.data.mShift,
                suggest: true,
                index: 3
            });
            var mShiftId = $("#mShiftId").data("kendoDropDownList");
            mShiftId.value(-1);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
        setTimeout(function () {
            modal.hide()
        }, 1000)

    };

    //
    function BindZone() {
        $http({
            method: 'GET',
            url: '../AssignWork/getZones'
        }).then(function successCallback(response) {
            //console.log(response);
            $('#mZoneId').kendoMultiSelect({
                dataTextField: "Zone",
                dataValueField: "mZoneId",
                dataSource: response.data.DZone
            });
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function BindSubZone(_mZoneId) {
        $http({
            method: 'GET',
            url: '../EmployeeTag/getSubZones',
            params: { id: _mZoneId }
        }).then(function successCallback(response) {
            $('#mFloorMasterId').kendoDropDownList({
                autoBind: true,
                dataTextField: "FloorName",
                dataValueField: "mFloorMasterId",
                filter: "contains",
                select: onSelect,
                dataSource: response.data.DZone,
                suggest: true,
                index: 1
            });
            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    BindArea(dataItem.mFloorMasterId);
                }
            };
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function BindArea(_mFloorMasterId) {
        $http({
            method: 'GET',
            url: '../EmployeeTag/getArea',
            params: { id: _mFloorMasterId }
        }).then(function successCallback(response) {
            $('#mRoomMasterId').kendoDropDownList({
                dataTextField: "RoomName",
                dataValueField: "mRoomMasterId",
                filter: "contains",
                dataSource: response.data.DArea,
                suggest: true,
                index: 1
            });

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    $scope.SaveFormCollData = function () {
        if ((angular.isUndefined($scope.EmployeeName) || $scope.EmployeeName === null) && (angular.isUndefined($scope.EmployeeId) || $scope.EmployeeId === null)) {
            toastr.error('Enter Employee Name/ID');
            return false;
        }
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        // console.log(_eData);
        $http({
            method: 'POST',
            url: '../AssignWork/Create',
            data: _eData
        }).then(function successCallback(response) {
            // console.log(response.data);
            if (response.data.Flag == true) {
                // document.getElementById("_formColl").reset();
                console.log(response.data._courseList);
                $scope.EmplyeesLists = response.data._courseList;
                //InitDataBind();
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
    $scope.EditFormCollData = function () {
        if (angular.isUndefined($scope.Site) || $scope.Site === null || angular.isUndefined($scope.mSiteMasterId) || $scope.mSiteMasterId === null) {
            toastr.error('Some Thing Went Wrong Please Refresh Page');
            return false;
        }
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {
            $http({
                method: 'POST',
                url: '../SiteMaster/Edit',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    document.getElementById("_formColl").reset();
                    $scope.isEdit = true; $scope.isAdd = false;
                    InitDataBind();
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
        }
    };

});

