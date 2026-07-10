// ****
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ZoneTimeSpendCtrl", function ($timeout, $scope, $http) {
    initializeComponets();
    //



    $scope.FormCollZone = function () {
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        //console.log(_eData);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        $http({
            method: 'POST',
            url: '../ZoneTimeSpend/CreateZone',
            data: _eData
        }).then(function successCallback(response) {
            // console.log(response.data.data);

            $scope.showZone = false;
            $scope.showSubZone = true;
            //$scope.EmpColl = null;
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
            url: '../ZoneTimeSpend/getMasterData'
        }).then(function successCallback(response) {
            //console.log(response.data);
            var agobj = { "Agency": "ALL", "mAgencyId": null };
            var agobjcol = response.data.mAgency.concat(agobj);

            $('#mAgencyId').kendoDropDownList({
                dataTextField: "Agency",
                dataValueField: "mAgencyId",
                filter: "contains",
                dataSource: agobjcol,
                suggest: true,
                index: 2
            });

            var mAgencyId = $("#mAgencyId").data("kendoDropDownList");
            mAgencyId.value(null);


            var desobj = { "Designation": "ALL", "mDesignationId": null };
            var desobjcol = response.data.mDesignation.concat(desobj);

            $('#mDesignationId').kendoDropDownList({
                dataTextField: "Designation",
                dataValueField: "mDesignationId",
                filter: "contains",
                dataSource: desobjcol,
                suggest: true,
                index: 2
            });

            var mDesignationId = $("#mDesignationId").data("kendoDropDownList");
            mDesignationId.value(null);


            var skilobj = { "SkillCategory": "ALL", "mSkillCategoryId": null };
            var skilobjcol = response.data.mSkillCategory.concat(skilobj);

            $('#mSkillCategoryId').kendoDropDownList({
                dataTextField: "SkillCategory",
                dataValueField: "mSkillCategoryId",
                filter: "contains",
                dataSource: skilobjcol,
                suggest: true,
                index: 2
            });

            var mSkillCategoryId = $("#mSkillCategoryId").data("kendoDropDownList");
            mSkillCategoryId.value(null);

            var wkobj = { "WorkCategory": "ALL", "mWorkCategoryId": null };
            var wkobjcol = response.data.mWorkCategory.concat(wkobj);


            $('#mWorkCategoryId').kendoDropDownList({
                dataTextField: "WorkCategory",
                dataValueField: "mWorkCategoryId",
                filter: "contains",
                dataSource: wkobjcol,
                suggest: true,
                index: 2
            });

            var mWorkCategoryId = $("#mWorkCategoryId").data("kendoDropDownList");
            mWorkCategoryId.value(null);


            var shitobj = { "WorkCategory": "ALL", "mWorkCategoryId": null };
            var shitobjcol = response.data.mShift.concat(shitobj);

            $('#mShiftId').kendoDropDownList({
                dataTextField: "Shift",
                dataValueField: "mShiftId",
                filter: "contains",
                dataSource: shitobjcol,
                suggest: true,
                index: 2
            });



            var actiobj = { "Activity": "ALL", "mActivityId": null };
            var actiobjcol = response.data.mActivity.concat(actiobj);

            $('#mActivityId').kendoDropDownList({
                dataTextField: "Activity",
                dataValueField: "mActivityId",
                filter: "contains",
                dataSource: actiobjcol,
                suggest: true,
                index: 3
            });


            var mActivityId = $("#mActivityId").data("kendoDropDownList");
            mActivityId.value(null);

            var obj = { "Site": "ALL", "mSiteMasterId": null };
            var jsonArray1 = response.data.mSite.concat(obj);


            $('#mSiteMasterId').kendoDropDownList({
                dataTextField: "Site",
                dataValueField: "mSiteMasterId",
                filter: "contains",
                select: onSelect,
                dataSource: jsonArray1,
                suggest: true,
                index: 1

            });

            var mSiteMasterId = $("#mSiteMasterId").data("kendoDropDownList");
            mSiteMasterId.value(null);

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
            url: '../ZoneTimeSpend/getZones',
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
            url: '../ZoneTimeSpend/getSubZones',
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
            url: '../ZoneTimeSpend/getArea',
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