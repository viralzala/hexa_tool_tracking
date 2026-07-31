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
        getMaintenanceData();
        setTimeout(function () {
            modal.hide()
        }, 1000)

    };

    // DataTable functions for Registered Maintenances
    function getMaintenanceData() {
        console.log("GetData called");
        console.log("MoreAssetDetailCtrl: getMaintenanceData() called - fetching from ../MoreAssetDetail/GetData");
        $http({
            method: 'GET',
            url: '../MoreAssetDetail/GetData'
        }).then(function successCallback(response) {
            console.log("AJAX Response:", response);
            console.log("Response data:", response.data);
            console.log("Response Flag:", response.data.Flag);
            BindMaintenanceTable(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }

    function BindMaintenanceTable(pData) {
        console.log("Binding table");
        console.log("Data :", pData);
        console.log("Rows :", pData.length);

        // Check if DataTable already exists, if not create it
        if ($.fn.DataTable.isDataTable('#tblAssetMaintenance')) {
            var table = $('#tblAssetMaintenance').DataTable();
            table.clear().draw();
            $('#tblAssetMaintenance').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": pData,
                "aoColumns": [
                    { "mData": "tMaintenanceId" },
                    { "mData": "AssetName" },
                    { "mData": "Title" },
                    { "mData": "MaintenanceName" },
                    { "mData": "MaintenanPart" },
                    {
                        "render": function (aaData, type, row, meta) {
                            return '<i class="uk-icon-inr"></i><b> ' + (row.Cost || '') + '</b>';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            return row.IsWarranty == true ? '<span class="uk-badge uk-badge-success"><b>Yes</b></span>' : '<span class="uk-badge uk-badge-default"><b>No</b></span>';
                        }
                    },
                    { "mData": "AdditionalPart" },
                    { "mData": "CreatedBy" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.StartDate != null) {
                                var date = new Date(parseInt(row.StartDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.EndDate != null) {
                                var date = new Date(parseInt(row.EndDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    { "mData": "Note" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return  +
                                   '<a id="DeleteMaintBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                        }
                        // <a id="EditMaintBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>'
                    }
                ]
            });
        } else {
            // First time initialization
            $('#tblAssetMaintenance').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": pData,
                "aoColumns": [
                    { "mData": "tMaintenanceId" },
                    { "mData": "AssetName" },
                    { "mData": "Title" },
                    { "mData": "MaintenanceName" },
                    { "mData": "MaintenanPart" },
                    {
                        "render": function (aaData, type, row, meta) {
                            return '<i class="uk-icon-inr"></i><b> ' + (row.Cost || '') + '</b>';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            return row.IsWarranty == true ? '<span class="uk-badge uk-badge-success"><b>Yes</b></span>' : '<span class="uk-badge uk-badge-default"><b>No</b></span>';
                        }
                    },
                    { "mData": "AdditionalPart" },
                    { "mData": "CreatedBy" },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.StartDate != null) {
                                var date = new Date(parseInt(row.StartDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {
                            if (row.EndDate != null) {
                                var date = new Date(parseInt(row.EndDate.substr(6)));
                                return ('0' + date.getDate()).slice(-2) + '/' + ('0' + (date.getMonth() + 1)).slice(-2) + '/' + date.getFullYear();
                            }
                            return '';
                        }
                    },
                    { "mData": "Note" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return '<a id="DeleteMaintBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                        // '<a id="EditMaintBtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>' +
                        }
                    }
                ]
            });
        }
    }

    // Search binding for maintenance table
    $('#txtMaintenanceSearch').on('keyup', function () {
        var table = $('#tblAssetMaintenance').DataTable();
        table.search($(this).val()).draw();
    });

    // Edit event
    $('body').on('click', '#EditMaintBtn', function () {
        var table;
        $(document).ready(function () {
            table = $('#tblAssetMaintenance').DataTable();
        });
        var row = $(this).parents('tr')[0];
        var isp = table.row(row).data();

        $http({
            method: 'GET',
            url: '../MoreAssetDetail/Edit',
            params: { id: isp.tMaintenanceId }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                var d = response.data.Idata;
                toastr.info('Edit functionality for maintenance ID: ' + d.tMaintenanceId);
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
    $('body').on('click', '#DeleteMaintBtn', function () {
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            var table;
            $(document).ready(function () {
                table = $('#tblAssetMaintenance').DataTable();
            });
            var row = $(this).parents('tr')[0];
            var isp = table.row(row).data();
            DeleteMaintenanceRecord(isp.tMaintenanceId);
        }
        else { console.log('Cancelled'); return false; }
    });

    function DeleteMaintenanceRecord(_id) {
        console.log(_id);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../MoreAssetDetail/DeleteData',
            params: { ID: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                getMaintenanceData();
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