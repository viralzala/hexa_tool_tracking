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
                $scope.AssetId = response.data._AssetList.tAssetTagId;
                $scope.AssetInfo = response.data._AssetList;
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
        getInspectionData();
    };

    // DataTable functions for Registered Inspections
    function getInspectionData() {
        console.log("GetData called");
        console.log("AssetInspectionCtrl: getInspectionData() called - fetching from ../AssetInspection/GetData");
        $http({
            method: 'GET',
            url: '../AssetInspection/GetData'
        }).then(function successCallback(response) {
            console.log("AJAX Response:", response);
            console.log("Response data:", response.data);
            console.log("Response Flag:", response.data.Flag);
            BindInspectionTable(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }

    function BindInspectionTable(pData) {
        console.log("Binding table");
        console.log("Data :", pData);
        console.log("Rows :", pData.length);

        // Check if DataTable already exists, if not create it
        if ($.fn.DataTable.isDataTable('#tblAssetInspection')) {
            var table = $('#tblAssetInspection').DataTable();
            table.clear().draw();
            $('#tblAssetInspection').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": pData,
                "aoColumns": [
                    { "mData": "AssetInspectionId" },
                    { "mData": "AssetName" },
                    { "mData": "InspectionNo" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.InspectionDate != null) {
                                var date = new Date(parseInt(row.InspectionDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    { "mData": "Inspector" },
                    { "mData": "PhysicalCondition" },
                    { "mData": "SafetyLabels" },
                    { "mData": "FitForUse" },
                    { "mData": "Observation" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.Status == "Passed") {
                                return '<span class="uk-badge uk-badge-success"><b>Passed</b></span>';
                            } else if (row.Status == "Failed") {
                                return '<span class="uk-badge uk-badge-danger"><b>Failed</b></span>';
                            } else {
                                return '<span class="uk-badge uk-badge-warning"><b>' + (row.Status || 'Pending') + '</b></span>';
                            }
                        }
                    },
                    { "mData": "CreatedBy" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return '<a id="EditInspectionBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>' +
                                   '<a id="DeleteInspectionBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                        }
                    }
                ]
            });
        } else {
            // First time initialization
            $('#tblAssetInspection').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": pData,
                "aoColumns": [
                    { "mData": "AssetInspectionId" },
                    { "mData": "AssetName" },
                    { "mData": "InspectionNo" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.InspectionDate != null) {
                                var date = new Date(parseInt(row.InspectionDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    { "mData": "Inspector" },
                    { "mData": "PhysicalCondition" },
                    { "mData": "SafetyLabels" },
                    { "mData": "FitForUse" },
                    { "mData": "Observation" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.Status == "Passed") {
                                return '<span class="uk-badge uk-badge-success"><b>Passed</b></span>';
                            } else if (row.Status == "Failed") {
                                return '<span class="uk-badge uk-badge-danger"><b>Failed</b></span>';
                            } else {
                                return '<span class="uk-badge uk-badge-warning"><b>' + (row.Status || 'Pending') + '</b></span>';
                            }
                        }
                    },
                    { "mData": "CreatedBy" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return '<a id="EditInspectionBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>' +
                                   '<a id="DeleteInspectionBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                        }
                    }
                ]
            });
        }
    }

    // Search binding for inspection table
    $('#txtInspectionSearch').on('keyup', function () {
        var table = $('#tblAssetInspection').DataTable();
        table.search($(this).val()).draw();
    });

    // Edit event
    $('body').on('click', '#EditInspectionBtn', function () {
        var table;
        $(document).ready(function () {
            table = $('#tblAssetInspection').DataTable();
        });
        var row = $(this).parents('tr')[0];
        var isp = table.row(row).data();

        $http({
            method: 'GET',
            url: '../AssetInspection/Edit',
            params: { id: isp.AssetInspectionId }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                var d = response.data.Idata;
                toastr.info('Edit functionality for inspection ID: ' + d.AssetInspectionId);
                console.log('Edit data:', d);
            }
            else {
                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    });

    // Delete event
    $('body').on('click', '#DeleteInspectionBtn', function () {
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            var table;
            $(document).ready(function () {
                table = $('#tblAssetInspection').DataTable();
            });
            var row = $(this).parents('tr')[0];
            var isp = table.row(row).data();
            DeleteInspectionRecord(isp.AssetInspectionId);
        }
        else { console.log('Cancelled'); return false; }
    });

    function DeleteInspectionRecord(_id) {
        console.log(_id);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../AssetInspection/DeleteData',
            params: { ID: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                getInspectionData();
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