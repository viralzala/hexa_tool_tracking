var app = angular.module('app');

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("AssetCalibrationCtrl", function ($timeout, $scope, $http) {
    console.log("AssetCalibrationCtrl: Controller instantiated");
    try {
        initializeComponets();
    } catch (e) {
        console.error("AssetCalibrationCtrl: initializeComponets() FAILED:", e);
    }

    $scope.CalibCollData = function () {
        var ddl = $("#mIteamMasterId").data("kendoDropDownList");

        $("#AssetId").val(parseInt(ddl.value()));
        var _formCSV = $("#_formCalibInfo");
        var data = _formCSV.serializeObject();

        data.AssetId = parseInt(ddl.value());

        var _eData = JSON.stringify(data);
        console.log(_eData);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../AssetCalibration/CreateCalib',
            data: _eData
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                document.getElementById("_formCalibInfo").reset();
                GetInitComp();
                getCalibrationData();
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
                $scope.AssetId = response.data._AssetList.mIteamMasterId;
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
                $scope.AssetId = response.data._AssetList.mIteamMasterId;
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
        console.log("AssetCalibrationCtrl: GetInitComp() - calling ../AssetCalibration/InitData");
        $http({
            method: 'GET',
            url: '../AssetCalibration/InitData'
        }).then(function successCallback(response) {
            console.log("AssetCalibrationCtrl: GetInitComp() success", response.data);
            if (response.data.Flag == true) {
                $('#mIteamMasterId').kendoDropDownList({
                    dataTextField: "IteamName",
                    dataValueField: "mIteamMasterId",
                    filter: "contains",
                    dataSource: response.data.AssetList,
                    suggest: true,

                    change: function () {

                        var value = this.value();

                        $("#AssetId").val(value);

                        $scope.$applyAsync(function () {
                            $scope.AssetId = value;
                        });
                    }
                });

                var mIteamMasterId = $("#mIteamMasterId").data("kendoDropDownList");
                mIteamMasterId.value(-1);

                setTimeout(function () {
                    modal.hide()
                }, 1000)
            }
            else {
                console.log("AssetCalibrationCtrl: GetInitComp() returned Flag=false");
                setTimeout(function () {
                    modal.hide()
                }, 1000)

                toastr.error(response.data.Message);
            }

        }, function errorCallback(response) {
            console.log("AssetCalibrationCtrl: GetInitComp() ERROR - " + (response.data ? response.data.ExceptionMessage : "No response"));
        });
    };

    function initializeComponets() {
        console.log("AssetCalibrationCtrl: initializeComponets() called");
        $scope.loading = true;
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        try {
            GetInitComp();
            console.log("AssetCalibrationCtrl: GetInitComp() completed without error");
        } catch (e) {
            console.log("AssetCalibrationCtrl: GetInitComp() THREW ERROR:", e);
        }
        try {
            GetStatistics();
            console.log("AssetCalibrationCtrl: GetStatistics() completed without error");
        } catch (e) {
            console.log("AssetCalibrationCtrl: GetStatistics() THREW ERROR:", e);
        }
        console.log("AssetCalibrationCtrl: About to call getCalibrationData()");
        try {
            getCalibrationData();
            console.log("AssetCalibrationCtrl: getCalibrationData() call completed");
        } catch (e) {
            console.log("AssetCalibrationCtrl: getCalibrationData() THREW ERROR:", e);
        }
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

    // DataTable functions for Registered Calibrations - EXACT same pattern as AssetTag
    function getCalibrationData() {
        console.log("AssetCalibrationCtrl: getCalibrationData() called - fetching from ../AssetCalibration/GetData");
        $http({
            method: 'GET',
            url: '../AssetCalibration/GetData'
        }).then(function successCallback(response) {
            BindCalibrationTable(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function BindCalibrationTable(pData) {
        console.log("Data :", pData);
        console.log("Rows :", pData.length);

        // Check if DataTable already exists, if not create it
        if ($.fn.DataTable.isDataTable('#tblAssetCalibration')) {
            var table = $('#tblAssetCalibration').DataTable();
            table.clear().draw();
            $('#tblAssetCalibration').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": pData,
                "aoColumns": [
                    { "mData": "AssetCalibrationId" },
                    { "mData": "AssetName" },
                    { "mData": "CertificateNo" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.CalibrationDate != null) {
                                var date = new Date(parseInt(row.CalibrationDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.NextDueDate != null) {
                                var date = new Date(parseInt(row.NextDueDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.Result == "Pass") {
                                return '<span class="uk-badge uk-badge-success"><b>Pass</b></span>';
                            } else if (row.Result == "Fail") {
                                return '<span class="uk-badge uk-badge-danger"><b>Fail</b></span>';
                            } else {
                                return '<span class="uk-badge uk-badge-warning"><b>' + (row.Result || 'N/A') + '</b></span>';
                            }
                        }
                    },
                     { "mData": "Agency", "defaultContent": "" },
                    { "mData": "Remarks" },
                    { "mData": "CreatedBy" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return'<a id="DeleteCalibBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                        }
                    }
                    // <a id="EditCalibBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>
                ]
            });
        } else {
            // First time initialization
            $('#tblAssetCalibration').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": pData,
                "aoColumns": [
                    { "mData": "AssetCalibrationId" },
                    { "mData": "AssetName" },
                    { "mData": "CertificateNo" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.CalibrationDate != null) {
                                var date = new Date(parseInt(row.CalibrationDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.NextDueDate != null) {
                                var date = new Date(parseInt(row.NextDueDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.Result == "Pass") {
                                return '<span class="uk-badge uk-badge-success"><b>Pass</b></span>';
                            } else if (row.Result == "Fail") {
                                return '<span class="uk-badge uk-badge-danger"><b>Fail</b></span>';
                            } else {
                                return '<span class="uk-badge uk-badge-warning"><b>' + (row.Result || 'N/A') + '</b></span>';
                            }
                        }
                    },
                     { "mData": "Agency", "defaultContent": "" },
                    { "mData": "Remarks" },
                    { "mData": "CreatedBy" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return  '<a id="DeleteCalibBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                        }
                        // '<a id="EditCalibBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>'
                    }
                ]
            });
        }
    };

    // Search binding for calibration table - EXACT same pattern as AssetTag
    $('#txtCalibrationSearch').on('keyup', function () {
        var table = $('#tblAssetCalibration').DataTable();
        table.search($(this).val()).draw();
    });

    // Edit event - EXACT same pattern as AssetTag
    $('body').on('click', '#EditCalibBtn', function () {
        var table;
        $(document).ready(function () {
            table = $('#tblAssetCalibration').DataTable();
        });
        //to get currently clicked row object
        var row = $(this).parents('tr')[0];
        //for row data
        var isp = table.row(row).data();

        $http({
            method: 'GET',
            url: '../AssetCalibration/Edit',
            params: { id: isp.AssetCalibrationId }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                var d = response.data.Idata;
                // Populate the form fields for editing
                // For now, show an alert with the data
                toastr.info('Edit functionality for calibration ID: ' + d.AssetCalibrationId);
                console.log('Edit data:', d);
            }
            else {
                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    });

    // Delete event - EXACT same pattern as AssetTag
    $('body').on('click', '#DeleteCalibBtn', function () {
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            var table;
            $(document).ready(function () {
                table = $('#tblAssetCalibration').DataTable();
            });
            //to get currently clicked row object
            var row = $(this).parents('tr')[0];
            //for row data
            var isp = table.row(row).data();
            DeleteCalibrationRecord(isp.AssetCalibrationId);
        }
        else { console.log('Cancelled'); return false; }
    });

    function DeleteCalibrationRecord(_id) {
        console.log(_id);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../AssetCalibration/DeleteData',
            params: { ID: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                getCalibrationData();
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
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

});