// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("EmployeeSummaryCtrl", function ($timeout, $scope, $http) {
    initializeComponets();
    //

    $scope.FormCollSubZone = function () {
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        //console.log(_eData);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        $http({
            method: 'POST',
            url: '../EmployeeSummary/CreateSubZone',
            data: _eData
        }).then(function successCallback(response) {
            //console.log(response.data);
            $scope.showZone = true;
            $scope.showSubZone = false;
            $scope.EmpColl = null;
            $scope.EmpColl = response.data.data;
            //BindJqueryTable(response.data.data);
            setTimeout(function () {
                modal.hide()
            }, 1000)
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };
   
    $scope.FormCollZone = function () {
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        //console.log(_eData);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        $http({
            method: 'POST',
            url: '../EmployeeSummary/CreateZone',
            data: _eData
        }).then(function successCallback(response) {
            //console.log(response.data);
            $scope.showZone = false;
            $scope.showSubZone = true;
            $scope.EmpColl = null;
            $scope.EmpColl = response.data.data;
            //BindJqueryTable(response.data.data);
            setTimeout(function () {
                modal.hide()
            }, 1000)
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };

    function BindJqueryTable(data) {
        //console.log(data);
        var table = $('#dt_default').DataTable();
        table.clear().draw();
        $('#dt_default').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                { "mData": "EmployeeId" },
                { "mData": "EmployeeName" },
                { "mData": "Agency" },
                { "mData": "Designation" },
                { "mData": "SkillCategory" },
                { "mData": "WorkCategory" },
                { "mData": "Activity" },
                { "mData": "TrackSite" },
                { "mData": "TrackZone" },
                { "mData": "TrackSubZone" },
                {
                    "mData": "InTime",
                    'mRender': function (data, type, full) {
                        return ((full.InTime == "" || full.InTime == null) ? 'N/A' : ConvertJsonDatetoanyformat(full.InTime, 'mm/dd/yyyy hh:mm:ss'));
                    }
                },
                {
                    "mData": "OutTime",
                    'mRender': function (data, type, full) {
                        return ((full.OutTime == "" || full.OutTime == null) ? 'N/A' : ConvertJsonDatetoanyformat(full.OutTime, 'mm/dd/yyyy hh:mm:ss'));
                    }
                },
                { "mData": "TimeSpend" }
            ]
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
        //$('#mDesignationId').kendoComboBox({});
        //$('#mSkillCategoryId').kendoComboBox({});
        //$('#mWorkCategoryId').kendoComboBox({});

        $http({
            method: 'GET',
            url: '../EmployeeSummary/getMasterData'
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


        setTimeout(function () {
            modal.hide()
        }, 1000)

    }

    //
    function BindZone(_mSiteMasterId) {
        $http({
            method: 'GET',
            url: '../EmployeeSummary/getZones',
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

            var mZoneId = $("#mZoneId").data("kendoDropDownList");
            mZoneId.value(-1);

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
            url: '../EmployeeSummary/getSubZones',
            params: { id: _mZoneId }
        }).then(function successCallback(response) {
            $('#mFloorMasterId').kendoDropDownList({
                autoBind: true,
                dataTextField: "FloorName",
                dataValueField: "mFloorMasterId",
                filter: "contains",
                dataSource: response.data.DZone,
                suggest: true,
                index: 1
            });

            var mFloorMasterId = $("#mFloorMasterId").data("kendoDropDownList");
            mFloorMasterId.value(-1);


        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function BindArea(_mFloorMasterId) {
        $http({
            method: 'GET',
            url: '../EmployeeSummary/getArea',
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

            var mRoomMasterId = $("#mRoomMasterId").data("kendoDropDownList");
            mRoomMasterId.value(-1);

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

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

});