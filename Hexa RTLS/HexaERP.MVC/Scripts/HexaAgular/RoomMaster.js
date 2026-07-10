// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

//
app.controller("RoomMasterCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    //
    function initializeComponets() {
        InitDataBind();

        setBindDropDown();
        $('#mZoneId').kendoDropDownList({
            dataSource: [],
            //enable: false
        });
        $('#mFloorMasterId').kendoDropDownList({
            dataSource: [],
            //enable: false
        });
    }

    function setBindDropDown() {
        $http({
            method: 'GET',
            url: '../RoomMaster/setDropData'
        }).then(function successCallback(response) {
            //console.log(response.data);
            var categories = $('#mSiteMasterId').kendoDropDownList({
                dataTextField: "Site",
                dataValueField: "mSiteMasterId",
                filter: "contains",
                select: onSelect,
                dataSource: response.data.DSite,
                suggest: true,
                index: 1
            });

            var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
            mSiteMasterId.value(-1);

            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    //var dropdownList = $("#mZoneId").data("kendoDropDownList");
                    //dropdownList.value(dataItem.mSiteMasterId);
                    BindZone(dataItem.mSiteMasterId);
                }
            };
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function BindZone(_mSiteMasterId) {
        $http({
            method: 'GET',
            url: '../RoomMaster/getZones',
            params: { id: _mSiteMasterId }
        }).then(function successCallback(response) {
            $('#mZoneId').kendoDropDownList({
                autoBind: false,
                dataTextField: "Zone",
                dataValueField: "mZoneId",
                filter: "contains",
                select: onSelect,
                dataSource: response.data.DZone,
                suggest: true,
                index: 1
            });
            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    BindSubZone(dataItem.mZoneId);
                }
            };
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function BindSubZone(_mZoneId) {
        $http({
            method: 'GET',
            url: '../RoomMaster/getSubZones',
            params: { id: _mZoneId }
        }).then(function successCallback(response) {
            $('#mFloorMasterId').kendoDropDownList({
                //autoBind: false,
                dataTextField: "FloorName",
                dataValueField: "mFloorMasterId",
                filter: "contains",
                dataSource: response.data.DZone,
                suggest: true,
                index: 1
            });
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../RoomMaster/GetCollData'
        }).then(function successCallback(response) {
            BindJqueryTable(response.data.IData);
            //console(response.data.IData);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.SaveFormCollData = function () {

        if (angular.isUndefined($scope.RoomName) || $scope.RoomName === null) {
            toastr.error('Missing Location/Zone/Sub Zone/Antenna Location');
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
                url: '../RoomMaster/Create',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    document.getElementById("_formColl").reset();
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

    function BindJqueryTable(pData) {
        var table = $('#tbls').DataTable();
        table.clear().draw();
        $("#tbls").dataTable({           
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": pData,
            "aoColumns": [
                { "mData": "mRoomMasterId" },
                { "mData": "Site" },
                { "mData": "Zone" },
                { "mData": "FloorName" },
                { "mData": "RoomName" },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a href="#EditIdata" id="EditIdata" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit" data-uk-modal="{center:true}"> <i id="Editbtn"  class="md-icon material-icons">&#xE254;</i></a><i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                    }
                }
            ]
        });
    };

    //
    $('body').on('click', '#EditIdata', function () {
        var table;
        $(document).ready(function () {
            table = $('#tbls').DataTable();
        });
        //to get currently clicked row object
        var row = $(this).parents('tr')[0];
        //for row data
        var isp = table.row(row).data();
        $http({
            method: 'GET',
            url: '../RoomMaster/Edit',
            params: { id: isp.mRoomMasterId }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {

                var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
                mSiteMasterId.value(response.data.Idata.mSiteMasterId);
                mSiteMasterId.enable(false);

                BindZone(response.data.Idata.mSiteMasterId);
                var mZoneId = $("#mZoneId").data("kendoDropDownList");
                mZoneId.value(response.data.Idata.mZoneId);
                mZoneId.enable(false);

                BindSubZone(response.data.Idata.mZoneId);
                var mFloorMasterId = $("#mFloorMasterId").data("kendoDropDownList");
                mFloorMasterId.value(response.data.Idata.mFloorMasterId);
                mFloorMasterId.enable(false);

                $scope.mRoomMasterId = response.data.Idata.mRoomMasterId;
                $scope.RoomName = response.data.Idata.RoomName;

                $scope.isEdit = false; $scope.isAdd = true;
                $scope.enableMe = true;


            }
            else { toastr.error(response.data.Message); }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    });

    //
    $('body').on('click', '#Deletebtn', function () {
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            var table;
            $(document).ready(function () {
                table = $('#tbls').DataTable();
            });
            //to get currently clicked row object
            var row = $(this).parents('tr')[0];
            //for row data
            var isp = table.row(row).data();
            DeleteRecord(isp.mRoomMasterId);
        }
        else { console.log('Cancelled'); return false; }
    });


    $scope.EditFormCollData = function () {

        if (angular.isUndefined($scope.RoomName) || $scope.RoomName === null || angular.isUndefined($scope.mRoomMasterId) || $scope.mRoomMasterId === null) {
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
                url: '../RoomMaster/Edit',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    document.getElementById("_formColl").reset();
                    $scope.isEdit = true; $scope.isAdd = false;
                    InitDataBind();
                    var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
                    mSiteMasterId.enable(true);
                    var mZoneId = $("#mZoneId").data("kendoDropDownList");
                    mZoneId.enable(true);
                    var mFloorMasterId = $("#mFloorMasterId").data("kendoDropDownList");
                    mFloorMasterId.enable(true);
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)
                    toastr.success(response.data.Message);
                    $scope.enableMe = false;
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

    //
    function DeleteRecord(_id) {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../RoomMaster/Delete',
            params: { id: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                InitDataBind();
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.warning(response.data.Message);
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)

                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    $(function () {
        var oTable;
        oTable = $('#tbls').dataTable();
        $('#global_filter').on('keyup click', function () {
            oTable.fnFilter($(this).val());
        });
    });
});