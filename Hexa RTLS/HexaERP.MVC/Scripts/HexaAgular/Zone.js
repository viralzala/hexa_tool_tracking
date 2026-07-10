// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("ZoneMasterCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    //
    function initializeComponets() {
        //$scope.isEdit = true; 
        //$('#mSiteMasterId').kendoMultiSelect();
        InitDataBind();
        getSites();
    }
    function getSites() {
        $http({
            method: 'GET',
            url: '../Zone/SiteDataColl'
        }).then(function successCallback(response) {
            $('#mSiteMasterId').kendoDropDownList({
                optionLabel: "",
                dataTextField: "Site",
                dataValueField: "mSiteMasterId",
                filter: "contains",
                dataSource: response.data.IData,
                suggest: true,
                index: 1
            });
            var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
            mSiteMasterId.value(-1);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../Zone/GetCollData'
        }).then(function successCallback(response) {
            BindJqueryTable(response.data.IData);
            //console(response.data.IData);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.SaveFormCollData = function () {
        if (angular.isUndefined($scope.Zone) || $scope.Zone === null) {
            toastr.error('Missing Zone/Location');
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
                url: '../Zone/Create',
                data: _eData
            }).then(function successCallback(response) {
               // console.log(response.data);
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
                { "mData": "mZoneId" },
                { "mData": "Site" },
                { "mData": "Zone" },
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
        //console.log(isp.mZoneId);
        $http({
            method: 'GET',
            url: '../Zone/Edit',
            params: { id: isp.mZoneId }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                //console.log(response.data.Idata);
                $scope.Zone = response.data.Idata.Zone;
                $scope.mZoneId = response.data.Idata.mZoneId;
                $scope.isEdit = false; $scope.isAdd = true;
                $scope.enableMe = true;

                var dropdownList = $("#mSiteMasterId").data("kendoDropDownList");
                dropdownList.value(response.data.Idata.mSiteMasterId);
                dropdownList.enable(false);

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
            DeleteRecord(isp.mZoneId);
        }
        else { console.log('Cancelled'); return false; }
    });


    $scope.EditFormCollData = function () {
        if (angular.isUndefined($scope.Zone) || $scope.Zone === null || angular.isUndefined($scope.mZoneId) || $scope.mZoneId === null) {
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
                url: '../Zone/Edit',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    document.getElementById("_formColl").reset();
                    $scope.isEdit = true; $scope.isAdd = false;
                    InitDataBind();
                    var dropdownList = $("#mSiteMasterId").data("kendoDropDownList");
                    dropdownList.enable(true);
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
            url: '../Zone/Delete',
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

