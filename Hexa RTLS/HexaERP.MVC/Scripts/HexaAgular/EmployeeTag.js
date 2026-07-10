// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("EmployeeTagCtrl", function ($scope, $http, $timeout) {
    initializeComponets();
    $scope.CurrentDate = new Date();



    //
    $scope.hub = $.connection.getTags;

    $scope.initNotifications = function () {

        $scope.hub.client.GetRFID = function (_readerL, _readerR) {
            GetRFIDs(_readerR);
        }
        //
        $.connection.hub.start();
    }

    $scope.initNotifications();

    //
    function initializeComponets() {

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');



        $http({
            method: 'GET',
            url: '../EmployeeTag/getADusers'
        }).then(function successCallback(response) {
            //console.log(response.data.User);
            if (response.data.False == true) {

                $("#adUser").kendoAutoComplete({
                    dataSource: response.data.User,
                    dataTextField: "Name",
                    dataValueField: "EmployeeId",
                    noDataTemplate: 'No Data!',
                    minLength: 0,
                    height: 404,
                    select: onSelectAd,
                });

                function onSelectAd(e) {
                    if (e.item) {
                        var dataItem = this.dataItem(e.item.index());
                        console.log(dataItem.Name);
                        angular.element($('#EmployeeName')).val(dataItem.Name);
                        angular.element($('#EmployeeId')).val(dataItem.EmployeeId);
                        angular.element($('#ContactNo')).val(dataItem.VoiceTelephoneNumber);
                        angular.element($('#EmailId')).val(dataItem.EmailAddress);

                    }
                };
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

        $('#mZoneId').kendoDropDownList({
        });
        $('#mFloorMasterId').kendoDropDownList({
        });
        $('#mRoomMasterId').kendoDropDownList({
        });
        //$('#mDesignationId').kendoComboBox({});
        //$('#mSkillCategoryId').kendoComboBox({});
        //$('#mWorkCategoryId').kendoComboBox({});

        $http({
            method: 'GET',
            url: '../EmployeeTag/getMasterData'
        }).then(function successCallback(response) {
            //console.log(response.data);

            $('#mAgencyId').kendoDropDownList({
                dataTextField: "Agency",
                dataValueField: "mAgencyId",
                filter: "contains",
                dataSource: response.data.mAgency,
                suggest: true,
                index: 3
            });

            var mAgencyId = $("#mAgencyId").data("kendoDropDownList");
            mAgencyId.value(-1);
            //mAgencyId.ul.width(500);

            $('#mDesignationId').kendoDropDownList({
                dataTextField: "Designation",
                dataValueField: "mDesignationId",
                filter: "contains",
                dataSource: response.data.mDesignation,
                suggest: true,
                index: 3
            });

            var mDesignationId = $("#mDesignationId").data("kendoDropDownList");
            mDesignationId.value(-1);

            $('#mSkillCategoryId').kendoDropDownList({
                dataTextField: "SkillCategory",
                dataValueField: "mSkillCategoryId",
                filter: "contains",
                dataSource: response.data.mSkillCategory,
                suggest: true,
                index: 3
            });

            var mSkillCategoryId = $("#mSkillCategoryId").data("kendoDropDownList");
            mSkillCategoryId.value(-1);

            $('#mWorkCategoryId').kendoDropDownList({
                dataTextField: "WorkCategory",
                dataValueField: "mWorkCategoryId",
                filter: "contains",
                dataSource: response.data.mWorkCategory,
                suggest: true,
                index: 3
            });

            var mWorkCategoryId = $("#mWorkCategoryId").data("kendoDropDownList");
            mWorkCategoryId.value(-1);

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

            $('#mActivityId').kendoDropDownList({
                dataTextField: "Activity",
                dataValueField: "mActivityId",
                filter: "contains",
                dataSource: response.data.mActivity,
                suggest: true,
                index: 3
            });

            var mActivityId = $("#mActivityId").data("kendoDropDownList");
            mActivityId.value(-1);

            $('#mSiteMasterId').kendoDropDownList({
                dataTextField: "Site",
                dataValueField: "mSiteMasterId",
                filter: "contains",
                select: onSelect,
                dataSource: response.data.mSite,
                suggest: true,
                index: 3
            });

            var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
            mSiteMasterId.value(-1);


            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    BindZone(dataItem.mSiteMasterId);
                }
            };

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });


        $http({
            method: 'GET',
            url: '../EmployeeTag/GetPorts'
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
            url: '../EmployeeTag/getGetReadersData'
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
            console.log(mAgencyId.value);
            if (mAgencyId.value == null) {
                setTimeout(function () {
                    modal.hide()
                }, 3000)
                alert('Ip Address Required to Start The Reader');
                return false;
            } else {
                $http({
                    method: 'GET',
                    url: '../EmployeeTag/ReaderInit',
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
        $.get("../EmployeeTag/StopReaders", function (data) {
            alert(data);
        });
    };
    //   
    $scope.clearImpinj = function () {
        $.get("../EmployeeTag/ReaderClear", function (data) {
            alert(data);
            $scope.RFID = "";
        });
    };

    //   
    $scope.getImpinjRFID = function () {
        $http({
            method: 'GET',
            url: '../EmployeeTag/GetIds',
            params: { Reader: mAgencyId.value }
        }).then(function successCallback(response) {
            $scope.RFID = response.RFID;
            $scope.PORTID = response.PORTID;
            $("#global_filter").val(response.RFID);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function getInitData() {
        $http({
            method: 'GET',
            url: '../EmployeeTag/getData'
        }).then(function successCallback(response) {
            // console.log(response.data);
            BindJqueryTable(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function BindJqueryTable(pData) {
        var table = $('#tbl').DataTable();
        table.clear().draw();
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": pData,
            "aoColumns": [
                { "mData": "tEmployeeTagId" },
                { "mData": "EmployeeName" },
                { "mData": "EmployeeId" },

                { "mData": "Agency" },
                { "mData": "Designation" },
                { "mData": "SkillCategory" },
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

                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a id="EditIdata" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>';
                    }
                },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a id="Deletebtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                    }
                }
            ]
        });
    };
    //
    function BindZone(_mSiteMasterId) {
        $http({
            method: 'GET',
            url: '../EmployeeTag/getZones',
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
    $scope.putPorts = function (Idata) {
        //console.log(Idata[0].Key);
        //console.log($scope.ddlFruits);
        $http({
            method: 'GET',
            url: '../EmployeeTag/PutStart',
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
            url: '../EmployeeTag/getDataTap'
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
                url: '../EmployeeTag/Delete',
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
        var table;
        $(document).ready(function () {
            table = $('#tbl').DataTable();
        });
        //to get currently clicked row object
        var row = $(this).parents('tr')[0];
        //for row data
        var isp = table.row(row).data();

        $http({
            method: 'GET',
            url: '../EmployeeTag/Edit',
            params: { id: isp.tEmployeeTagId }
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {

                var mAgencyId = $("#mAgencyId").data("kendoDropDownList");
                mAgencyId.value(response.data.Idata.mAgencyId);

                var mDesignationId = $("#mDesignationId").data("kendoDropDownList");
                mDesignationId.value(response.data.Idata.mDesignationId);

                var mSkillCategoryId = $("#mSkillCategoryId").data("kendoDropDownList");
                mSkillCategoryId.value(response.data.Idata.mSkillCategoryId);

                var mWorkCategoryId = $("#mWorkCategoryId").data("kendoDropDownList");
                mWorkCategoryId.value(response.data.Idata.mWorkCategoryId);

                var mActivityId = $("#mActivityId").data("kendoDropDownList");
                mActivityId.value(response.data.Idata.mActivityId);

                var mShiftId = $("#mShiftId").data("kendoDropDownList");
                mShiftId.value(response.data.Idata.mShiftId);



                $("#EmployeeName").val(response.data.Idata.EmployeeName);
                $("#EmployeeId").val(response.data.Idata.EmployeeId);
                $("#ContactNo").val(response.data.Idata.ContactNo);
                $("#EmailId").val(response.data.Idata.EmailId);
                $("#RFID").val(response.data.Idata.RFID);
                $("#tEmployeeTagId").val(response.data.Idata.tEmployeeTagId);

                $("#EmailId").val(response.data.Idata.EmailId);
                $("#ContactNo").val(response.data.Idata.ContactNo);





                BindZone(response.data.Idata.mSiteMasterId);
                BindSubZone(response.data.Idata.mZoneId);
                BindArea(response.data.Idata.mFloorMasterId);

                var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
                mSiteMasterId.value(response.data.Idata.mSiteMasterId);

                var mZoneId = $("#mZoneId").data("kendoDropDownList");
                mZoneId.value(response.data.Idata.mZoneId);
                var mFloorMasterId = $("#mFloorMasterId").data("kendoDropDownList");
                mFloorMasterId.value(response.data.Idata.mFloorMasterId);
                var mRoomMasterId = $("#mRoomMasterId").data("kendoDropDownList");
                mRoomMasterId.value(response.data.Idata.mRoomMasterId);
                //console.log(response.data.Idata.mFloorMasterId);
                //console.log(response.data.Idata.mZoneId);
                //console.log(response.data.Idata.mFloorMasterId);
                //console.log(response.data.Idata.mRoomMasterId);   
                // $scope.isEdit = false; $scope.isAdd = true;
                //$scope.enableMe = true;



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
                table = $('#tbl').DataTable();
            });
            //to get currently clicked row object
            var row = $(this).parents('tr')[0];
            //for row data
            var isp = table.row(row).data();
            DeleteRecord(isp.tEmployeeTagId);
        }
        else { console.log('Cancelled'); return false; }
    });
    //
    function DeleteRecord(_id) {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../EmployeeTag/Delete',
            params: { id: _id }
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

});

function GetRFIDs(_rfid) {
    $("#RFID").val("");
    $("#RFID").val(_rfid);
    // console.log(_rfid);
}