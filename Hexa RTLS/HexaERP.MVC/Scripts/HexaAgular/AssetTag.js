// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("AssetTagCtrl", function ($scope, $http, $timeout) {
    // Initialize loading state
    $scope.statsLoading = true;
    $scope.CurrentDate = new Date();
    $scope.Asset = {};
    //
    //
    $scope.hub = $.connection.getTags;

    $scope.initNotifications = function () {

        $scope.hub.client.GetRFID = function (_readerL, _readerR) {
            console.log(_readerR);
            GetRFIDs(_readerR);
        }

        $.connection.hub.start();
    }

    function BindSubCategory1(_mGroupMasterId) {
        return $http({
            method: 'GET',
            url: '../AssetTag/getSubCategory1',
            params: { categoryId: _mGroupMasterId }
        }).then(function successCallback(response) {
            var mIteamTypeMasterId = $("#mIteamTypeMasterId").data("kendoDropDownList");
            if (mIteamTypeMasterId) {
                mIteamTypeMasterId.setDataSource(response.data.DSubCategory1);
                mIteamTypeMasterId.value(-1);
            }
            var AssetSubCategory2Id = $("#AssetSubCategory2Id").data("kendoDropDownList");
            if (AssetSubCategory2Id) {
                AssetSubCategory2Id.setDataSource([]);
                AssetSubCategory2Id.value(-1);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }

    function BindSubCategory2(_mIteamTypeMasterId) {
        return $http({
            method: 'GET',
            url: '../AssetTag/getSubCategory2',
            params: { subCategoryId: _mIteamTypeMasterId }
        }).then(function successCallback(response) {
            var AssetSubCategory2Id = $("#AssetSubCategory2Id").data("kendoDropDownList");
            if (AssetSubCategory2Id) {
                AssetSubCategory2Id.setDataSource(response.data.DSubCategory2);
                AssetSubCategory2Id.value(-1);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }

    $scope.initNotifications();

    // Load statistics
    $scope.loadStatistics = function () {
        $scope.statsLoading = true;
        $http({
            method: 'GET',
            url: '../AssetTag/GetStatistics'
        }).then(function successCallback(response) {
            if (response.data) {
                $scope.statTotal = response.data.Total;
                $scope.statActive = response.data.Active;
                $scope.statPending = response.data.Pending;
                $scope.statUnderMaintenance = response.data.UnderMaintenance;
            }
            $scope.statsLoading = false;
        }, function errorCallback(response) {
            console.log("Error loading statistics: " + response.data.ExceptionMessage);
            $scope.statsLoading = false;
        });
    };

    //
    function initializeComponets() {

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $('#mZoneId').kendoDropDownList({
        });
        $('#mFloorMasterId').kendoDropDownList({
        });
        $('#mRoomMasterId').kendoDropDownList({
        });

        $http({
            method: 'GET',
            url: '../AssetTag/getMasterData'
        }).then(function successCallback(response) {
            //console.log(response.data);

            $('#mGroupMasterId').kendoDropDownList({
                dataTextField: "GroupName",
                dataValueField: "mGroupMasterId",
                filter: "contains",
                select: onGroupSelect,
                dataSource: response.data.ObjGroup,
                suggest: true,
                index: 2
            });

            var mGroupMasterId = $("#mGroupMasterId").data("kendoDropDownList");
            if (mGroupMasterId) { mGroupMasterId.value(-1); }

            function onGroupSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    BindSubCategory1(dataItem.mGroupMasterId);
                }
            }

            $('#mIteamTypeMasterId').kendoDropDownList({
                dataTextField: "IteamType",
                dataValueField: "mIteamTypeMasterId",
                filter: "contains",
                select: onTypeSelect,
                dataSource: response.data.ObjIteamType,
                suggest: true,
                index: 2
            });

            function onTypeSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    BindSubCategory2(dataItem.mIteamTypeMasterId);
                }
            }

            var mIteamTypeMasterId = $("#mIteamTypeMasterId").data("kendoDropDownList");
            if (mIteamTypeMasterId) { mIteamTypeMasterId.value(-1); }

            $('#AssetSubCategory2Id').kendoDropDownList({
                dataTextField: "AssetSubCategory2Name",
                dataValueField: "AssetSubCategory2Id",
                filter: "contains",
                dataSource: [],
                suggest: true,
                index: 2
            });

            var AssetSubCategory2Id = $("#AssetSubCategory2Id").data("kendoDropDownList");
            if (AssetSubCategory2Id) { AssetSubCategory2Id.value(-1); }

            $('#mUnitMasterId').kendoDropDownList({
                dataTextField: "UnitName",
                dataValueField: "mUnitMasterId",
                filter: "contains",
                dataSource: response.data.ObjUnit,
                suggest: true,
                index: 2
            });

            var mUnitMasterId = $("#mUnitMasterId").data("kendoDropDownList");
            if (mUnitMasterId) { mUnitMasterId.value(-1); }


            $('#mSiteMasterId').kendoDropDownList({
                dataTextField: "Site",
                dataValueField: "mSiteMasterId",
                filter: "contains",
                select: onSelect,
                dataSource: response.data.mSite,
                suggest: true,
                index: 2
            });

            var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
            if (mSiteMasterId) { mSiteMasterId.value(-1); }


            $('#mVendorId').kendoDropDownList({
                dataTextField: "VendorName",
                dataValueField: "mVendorId",
                filter: "contains",
                dataSource: response.data.vendor,
                suggest: true,
                index: 2
            });

            var mVendorId = $("#mVendorId").data("kendoDropDownList");
            if (mVendorId) { mVendorId.value(-1); }

            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    BindZone(dataItem.mSiteMasterId);
                }
            };

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        }).finally(function () {
            // Ensure modal is hidden after all AJAX calls complete
            setTimeout(function () {
                if (modal && modal.hide) {
                    modal.hide();
                }
            }, 1000);
        });

        $http({
            method: 'GET',
            url: '../AssetTag/GetPorts'
        }).then(function successCallback(response) {
            //console.log(response.data);
            $scope.ListPort = response.data;
            $('#ddTapRfid').kendoDropDownList({
                dataTextField: "Key",
                dataValueField: "Key",
                filter: "contains",
                dataSource: response.data,
                suggest: true,
                index: 3
            });

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

        $http({
            method: 'GET',
            url: '../AssetTag/getGetReadersData'
        }).then(function successCallback(response) {
            // console.log(response.data)
            $('#Reader').kendoDropDownList({
                dataTextField: "ReaderIP",
                dataValueField: "ReaderIP",
                filter: "contains",
                dataSource: response.data,
                suggest: true,
                index: 3
            });
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

        getInitData();
        $scope.loadStatistics();
        setTimeout(function () {
            modal.hide()
        }, 1000)
    };

    //
    $scope.startImpinj = function () {
        console.log('startImpinj');
        UIkit.modal.confirm('Are you sure to Start Reader?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            var mAgencyId = $("#Reader").data("kendoDropDownList");
            //console.log(mAgencyId.value);
            if (mAgencyId.value == null) {
                setTimeout(function () {
                    modal.hide()
                }, 3000)
                alert('Ip Address Required to Start The Reader');
                return false;
            } else {
                $http({
                    method: 'GET',
                    url: '../AssetTag/ReaderInit',
                    params: { Reader: mAgencyId.value }
                }).then(function successCallback(response) {
                    $scope.msg = response;
                    setTimeout(function () {
                        modal.hide()
                    }, 3000)
                }, function errorCallback(response) {
                    console.log("Error : " + response.data.ExceptionMessage);
                });
            }
        });
    };

    //   
    $scope.stopImpinj = function () {
        $.get("../AssetTag/StopReaders", function (data) {
            alert(data);
        });
    };
    //   
    $scope.clearImpinj = function () {
        $.get("../AssetTag/ReaderClear", function (data) {
            alert(data);
            $scope.RFID = "";
        });
    };

    //   
    $scope.getImpinjRFID = function () {
        $http({
            method: 'GET',
            url: '../AssetTag/GetIds'
        }).then(function successCallback(response) {
            $scope.RFID = response.data.RFID;
            $scope.PORTID = response.data.PORTID;
            $("#global_filter").val(response.data.RFID);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function getInitData() {
        $http({
            method: 'GET',
            url: '../AssetTag/getData'
        }).then(function successCallback(response) {
            //console.log(response.data);
            BindJqueryTable(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };


    //
    function BindJqueryTable(pData) {
        if ($('#dt_tableExport').length === 0) { return; }
        if (!Array.isArray(pData) || pData.length === 0) { return; }
        console.log("Data :", pData);
        console.log("Rows :", pData.length);

        if ($.fn.DataTable && $.fn.DataTable.isDataTable('#dt_tableExport')) {
            $('#dt_tableExport').DataTable().destroy();
        }

        if ($('#dt_tableExport tbody').length === 0) {
            $('#dt_tableExport').append('<tbody></tbody>');
        }

        $('#dt_tableExport').dataTable({
            "bProcessing": true,
            "aaData": pData,
            "aoColumns": [
                { "mData": "tAssetTagId" },
                { "mData": "IteamName" },
                { "mData": "UID" },
                { "mData": "ModelNo" },
                { "mData": "IteamDescription" },
                {
                    "render": function (aaData, type, row, meta) {

                        if (row.RFID != null) {

                            return '<span class="uk-badge uk-badge-primary"><b>' + row.RFID + '</b></span>';
                        }
                        else {
                            return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                        }

                    }
                },
                { "mData": "BarCode" },


                {
                    "render": function (aaData, type, row, meta) {

                        if (row.bStock != null) {

                            return '<a href="#mailbox_new_message" onclick="setqty(\'' + row.tAssetTagId + '\',\'' + row.UID + '\');"  data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" data-uk-modal="{center:true}"><h3>' + parseFloat(row.bStock) + '</h3></a>';
                        }
                        else {
                            return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                        }

                    }
                },
                { "mData": "RoomName" },
                {
                    "render": function (aaData, type, row, meta) {

                        if (row.FloorName != null) {

                            return 'Rack:<b>' + row.FloorName + '</b><br/>Shelf:<b>' + row.RoomName + '</b>';
                        }
                        else {
                            return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                        }

                    }
                },
                {
                    'mRender': function (aaData, type, row, meta) {
                        var html = '';
                        html += '<a href="/AssetTag/CarryParam/' + row.UID + '" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="More Detail"><i class="md-icon material-icons">&#xE89C;</i></a>';
                        html += '<a id="EditIdata" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>';
                        html += '<a id="Deletebtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i class="md-icon material-icons">&#xE872;</i></a>';
                        return html;
                    }
                }

            ]
        });
    }
    //
    function BindZone(_mSiteMasterId) {
        $http({
            method: 'GET',
            url: '../AssetTag/getZones',
            params: { id: _mSiteMasterId }
        }).then(function successCallback(response) {
            $('#mZoneId').kendoDropDownList({
                autoBind: true,
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
            url: '../AssetTag/getSubZones',
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
            url: '../AssetTag/getArea',
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
    $scope.putPorts = function (Idata) {
        //console.log(Idata[0].Key);
        //console.log($scope.ddlFruits);
        $http({
            method: 'GET',
            url: '../AssetTag/PutStart',
            params: {
                _Port: Idata[0].Key
            }
        }).then(function successCallback(response) {
            //console.log(response.data);
            $scope.msg = "";
            if (response.data.Flag == true) {
                $scope.msg = response.data.Msg;
                var myVar;
                myVar = setInterval(function () {
                    $scope.$apply(SetControll());
                }, 1000);
            }
            else { $scope.msg = response.data.Msg; }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function SetControll() {
        $http({
            method: 'GET',
            url: '../AssetTag/getDataTap'
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                // console.log(response.data);
                $scope.DataList = response.data.Datas;
            }
            else {
                console.log(response.data);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    $scope.DeletePorts = function () {
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            $http({
                method: 'GET',
                url: '../AssetTag/Delete',
                params: { ID: _iDa }
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                    
                //console.log(response.data);
                if (response.data.result == true) {
                    alert(response.data.Message);
                    getRoomsPorts($("#mRoomMasterId").val());
                }
                else { alert(response.data.Message); }
            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                console.log("Error : " + response.data.ExceptionMessage);
            });
        } else { alert("Canceled"); }

    };
    //
    $('body').on('click', '#EditIdata', function () {
        var table = $('#dt_tableExport').DataTable();
        //to get currently clicked row object
        var row = $(this).parents('tr')[0];
        //for row data
        var isp = table.row(row).data();

        $http({
            method: 'GET',
            url: '../AssetTag/Edit',
            params: { id: isp.tAssetTagId }
        }).then(function successCallback(response) {

            //console.log(response.data);
            if (response.data.Flag == true) {

                var ToolIdEl = $("#ToolId");
                if (ToolIdEl.length > 0) {
                    ToolIdEl.val(response.data.Idata.UID);
                    $scope.Asset.ToolId = response.data.Idata.UID;
                }

                var ToolNameEl = $("#IteamName");
                if (ToolNameEl.length > 0) {
                    ToolNameEl.val(response.data.Idata.IteamName);
                    $scope.Asset.ToolName = response.data.Idata.IteamName;
                }
                var mGroupMasterId = $("#mGroupMasterId").data("kendoDropDownList");
                if (mGroupMasterId) { mGroupMasterId.value(response.data.Idata.mGroupMasterId); }

                // Load Sub Category 1 filtered by the selected category, then set its value
                BindSubCategory1(response.data.Idata.mGroupMasterId).then(function () {
                    var mIteamTypeMasterId = $("#mIteamTypeMasterId").data("kendoDropDownList");
                    if (mIteamTypeMasterId) { mIteamTypeMasterId.value(response.data.Idata.mIteamTypeMasterId); }

                    // Load Sub Category 2 filtered by the selected Sub Category 1, then set its value
                    return BindSubCategory2(response.data.Idata.mIteamTypeMasterId);
                }).then(function () {
                    var AssetSubCategory2Id = $("#AssetSubCategory2Id").data("kendoDropDownList");
                    if (response.data.Idata.AssetSubCategory2Id != null && AssetSubCategory2Id) {
                        AssetSubCategory2Id.value(response.data.Idata.AssetSubCategory2Id);
                    }
                });

                var mUnitMasterId = $("#mUnitMasterId").data("kendoDropDownList");
                if (mUnitMasterId) { mUnitMasterId.value(response.data.Idata.mUnitMasterId); }

                var mVendorId = $("#mVendorId").data("kendoDropDownList");
                if (mVendorId) { mVendorId.value(response.data.Idata.mVendorId); }


                $("#Model").val(response.data.Idata.Model);
                $("#ModelNo").val(response.data.Idata.ModelNo);
                $("#SerialNo").val(response.data.Idata.SerialNo);
                $("#Manufacturer").val(response.data.Idata.Manufacturer);
                $("#BarCode").val(response.data.Idata.BarCode);
                $("#PurchaseCost").val(response.data.Idata.PurchaseCost);
                $("#InvNo").val(response.data.Idata.InvNo);



                $("#Depreciation").val(response.data.Idata.Depreciation);
                $("#Receivedby").val(response.data.Idata.Receivedby);
                $("#DefaultWarranty").val(response.data.Idata.DefaultWarranty);



                $("#IteamName").val(response.data.Idata.IteamName);
                $("#IteamCode").val(response.data.Idata.IteamCode);
                $("#IteamDescription").val(response.data.Idata.IteamDescription);
                $("#RFID").val(response.data.Idata.RFID);
                $("#BLETagSerialNumber").val(response.data.Idata.BLETagSerialNumber);
                $("#tAssetTagId").val(response.data.Idata.tAssetTagId);

                if (response.data.Idata.PurchaseDate != null) { $("#PurchaseDate").val(ConvertJsonDatetoanyformat(response.data.Idata.PurchaseDate, 'dd-mm-yyyy')); }


                var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
                if (mSiteMasterId) { mSiteMasterId.value(response.data.Idata.mSiteMasterId); }

                BindZone(response.data.Idata.mSiteMasterId);
                BindSubZone(response.data.Idata.mZoneId);
                BindArea(response.data.Idata.mFloorMasterId);

                var mZoneId = $("#mZoneId").data("kendoDropDownList");
                if (mZoneId) { mZoneId.value(response.data.Idata.mZoneId); }
                var mFloorMasterId = $("#mFloorMasterId").data("kendoDropDownList");
                if (mFloorMasterId) { mFloorMasterId.value(response.data.Idata.mFloorMasterId); }
                var mRoomMasterId = $("#mRoomMasterId").data("kendoDropDownList");
                if (mRoomMasterId) { mRoomMasterId.value(response.data.Idata.mRoomMasterId); }

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
            var table = $('#dt_tableExport').DataTable();
            //to get currently clicked row object
            var row = $(this).parents('tr')[0];
            //for row data
            var isp = table.row(row).data();
            DeleteRecord(isp.tAssetTagId);
        }
        else { console.log('Cancelled'); return false; }
    });
    //
    function DeleteRecord(_id) {
        console.log(_id);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../AssetTag/DeleteData',
            params: { ID: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                getInitData();
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

    $scope.setQuantity = function () {

        UIkit.modal.confirm('Are you sure to update quantity?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

            $http({
                method: 'GET',
                url: '../AssetTag/setQTYData',
                params: { tAssetTagId: $("#tAssetTagIds").val(), UID: $("#UIDs").val(), qty: $("#Stockup").val() }
            }).then(function successCallback(response) {
                console.log(response.data);

                if (response.data.Flag == true) {
                    getInitData();
                    $("#Stockup").val("");
                    $("#tAssetTagIds").val("");
                    $("#UIDs").val("");
                    setTimeout(function () {
                        modal.hide()
                    }, 3000)
                } else {
                    UIkit.modal.alert(response.data.Message);
                    setTimeout(function () {
                        modal.hide()
                    }, 3000)
                }

            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        });
    };
    
    // Initialize components after all functions are defined
    initializeComponets();
});

function setqty(tAssetTagId, UID) {
    $("#tAssetTagIds").val(tAssetTagId);
    $("#UIDs").val(UID);
}


function ConvertJsonDatetoanyformat(jsondate, format) {
    var yourdate = '';
    var dateAsFromServerSide = jsondate ///Date(1291374337981)/
    //Now let's convert it to js format
    //Example: Fri Dec 03 2010 16:37:32 GMT+0530 (India Standard Time)
    var parsedDate = new Date(parseInt(dateAsFromServerSide.substr(6)));

    var jsDate = new Date(parsedDate); //Date object

    //Play with jsDate properties getDate(), getDay() etc

    var fulldate = dateAsFromServerSide;
    var ParsedDate = parsedDate;
    var GetDay = jsDate.getDay();
    var GetDate = jsDate.getDate();
    var GetFullYear = jsDate.getFullYear();
    var GetHours = jsDate.getHours();
    var GetMilliseconds = jsDate.getMilliseconds();
    var GetMinutes = jsDate.getMinutes();
    var GetMonth = jsDate.getMonth() + 1;
    var GetSeconds = jsDate.getSeconds();
    var GetTime = jsDate.getTime();
    var GetTimezoneOffset = jsDate.getTimezoneOffset();
    var GetUTCDate = jsDate.getUTCDate();
    var GetUTCDay = jsDate.getUTCDay();
    var GetUTCFullYear = jsDate.getUTCFullYear();
    var GetUTCHours = jsDate.getUTCHours();
    var GetUTCMilliseconds = jsDate.getUTCMilliseconds();
    var GetUTCMinutes = jsDate.getUTCMinutes();
    var GetUTCMonth = jsDate.getUTCMonth();
    var GetUTCSeconds = jsDate.getUTCSeconds();
    var GetYear = jsDate.getYear();

    if (format == 'mm/dd/yyyy') {
        yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear;
    }
    else if (format == 'dd/mm/yyyy') {
        yourdate = GetDate + '/' + GetMonth + '/' + GetFullYear;
    }
    else if (format == 'dd-mm-yyyy') {
        var _date = (jsDate.getFullYear() + "-" + zeroPadded(jsDate.getMonth() + 1) + "-" + zeroPadded(jsDate.getDate()));
        //console.log(_date);
        //yourdate = GetDate + '-' + GetMonth + '-' + GetFullYear;
        yourdate = _date;
    }
    else if (format == 'mm/dd/yyyy hh:mm:ss') {
        yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear + " " + GetHours + ":" + GetMinutes + ":" + GetSeconds;
    }
    else if (format == 'hh:mm:ss') {
        yourdate = GetHours + ":" + GetMinutes + ":" + GetSeconds;
    }
    else if (format == 'hh:mm 24hour') {
        yourdate = GetHours + ":" + GetMinutes;
    }
    else if (format == 'hh:mm ampm') {
        yourdate = formatAMPM(jsDate)
    }
    else if (format == 'mm/dd/yyyy hh:mm ampm') {
        var timeampm = formatAMPM(jsDate);
        yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear + ' ' + timeampm;
    }
    return yourdate;
};
function zeroPadded(val) {
    if (val >= 10)
        return val;
    else
        return '0' + val;
}

function formatAMPM(date) {

    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
};


function GetRFIDs(_rfid) {
    $("#RFID").val("");
    $("#RFID").val(_rfid);
    // console.log(_rfid);
}
