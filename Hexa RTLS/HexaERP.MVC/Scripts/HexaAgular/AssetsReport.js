// ****
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("AssetsReportCtrl", function ($timeout, $scope, $http) {
    initializeComponets();
    //

    $scope.currentPage = 1;
    //$scope.nextPage = 1;
    app.itemPerPage = 1000;

    $scope.pageChanged = function (NextPage, Action) {
        if ($scope.currentPage != NextPage) {
            $scope.currentPage = + NextPage;
            //console.log($scope.currentPage); console.log(Action);
            if (Action == 'ASSETREPORT')
                GetAssetReport(NextPage);
            else if (Action == 'ASSETMOREDETAILREPORT')
                AssetMoreDetailReport(NextPage);
            else
                console.log('Else');
        }
    };

    $("#formSearchEngineSubmit").on('click', function (e) {
        e.preventDefault();
        $scope.currentPage = 1;
        GetAssetReport($scope.currentPage);
    });

    function GetAssetReport(pageNo) {


        var fDate = $("#dateFrom").val();
        var tDate = $("#toDate").val();
        console.log(fDate); console.log(tDate);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var form = $("#FormSearchEngin");
        var data = new FormData();
        var form_serialized = JSON.stringify(form.serializeObject(), null, 2);
        var d = JSON.parse(form_serialized);
        data.append("Ble", d.Ble);
        data.append("SerialNumber", d.SerialNumber);
        data.append("PartNumber", d.PartNumber);
        data.append("PageSize", $scope.itemPerPage);
        data.append("PageIndex", pageNo);

        data.append("Status", d.Status);
        data.append("Lot", d.Lot);
        data.append("dateFrom", $("#dateFrom").val());
        data.append("toDate", $("#toDate").val());


        //console.log(d);
        //return false;

        $scope.totalRecords = 0;

        $http({
            method: 'POST',
            url: '../AssetsReport/GetAssetReport',
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined },
            data: data
        }).then(function successCallback(response) {
            //console.log(response);
            if (response.data.Flag === true) {
                $scope.AssetReportList = response.data.Result;
                if (IsNullCheck(response.data.Result)) {
                    $scope.totalRecords = response.data.TotalRecords;
                    toastr.success("Total Records<h1>" + $scope.totalRecords + "</h1> Result Showing : " + response.data.Result.length + "",
                        '',
                        { timeOut: 3000, positionClass: "toast-top-right", closeButton: true });

                    setTimeout(function () {
                        modal.hide()
                    }, 1000);
                } else {
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)
                    toastr.error("<h1>" + response.data.Result.length + "</h1>",
                        ' Not Found',
                        { timeOut: 3000, positionClass: "toast-top-right", closeButton: true });
                }
            } else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.error(response.data.Message,
                    ' Not Found',
                    { timeOut: 3000, positionClass: "toast-top-right", closeButton: true });
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.AssetMoreDetail = function (id) {
        // console.log(id);
        $scope.currentPage = 1;
        $scope.MoreDetailId = id;
        AssetMoreDetailReport($scope.currentPage);
    };
    //
    function AssetMoreDetailReport(pageNo) {

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var data = new FormData();
        data.append("id", $scope.MoreDetailId);
        data.append("PageSize", $scope.itemPerPage);
        data.append("PageIndex", pageNo);
        data.append("searchValue", "");
        //return false;

        $http({
            method: 'POST',
            url: '../AssetsReport/GetAssetMoreDetailsReport',
            transformRequest: angular.identity,
            headers: { 'Content-Type': undefined },
            data: data
        }).then(function successCallback(response) {
            //console.log(response.data);
            if (response.data.Flag === true) {
                if (IsNullCheck(response.data.Result)) {
                    $scope.TotalAuditRecord = response.data.recordsTotal;
                    $scope.AssetDetailList = [];
                    let displayKeys = response.data.keys;

                    angular.forEach(response.data.Result, function (value, key) {
                        //console.log(value['TrxTimestamp']);
                        var entity = {};
                        entity.DmlType = value['DmlType'];
                        entity.DmlTimestamp = value['DmlTimestamp'];
                        entity.DmlCreatedBy = value['DmlCreatedBy'];
                        entity.TrxTimestamp = value['TrxTimestamp'];


                        var myObj1 = JSON.parse(value['OldRowData']);
                        var myObj2 = JSON.parse(value['NewRowData']);

                        var InnerlistObject = [];
                        if (myObj1 !== null) {
                            var keys = Object.keys(displayKeys);
                            for (var i = 0; i < keys.length; i++) {
                                var key = keys[i];
                                if (myObj1[key] != myObj2[key])
                                    InnerlistObject.push(`${key}: ${myObj1[key]} > ${myObj2[key]}`);

                            }
                        }
                        else if (myObj2 !== null) {
                            var keys = Object.keys(displayKeys);
                            for (var i = 0; i < keys.length; i++) {
                                var key = keys[i];
                                if (typeof (myObj2[key]) !== "undefined" && myObj2[key] !== null && myObj2[key] !== '')
                                    InnerlistObject.push(`${key}: ${myObj2[key]}`);
                            }
                        }
                        else { return false; }

                        entity.outerChange = InnerlistObject;
                        $scope.AssetDetailList.push(entity);
                    });

                    toastr.success("<h1>" + response.data.Result.length + "</h1>",
                        'Record Found',
                        { timeOut: 3000, positionClass: "toast-top-right", closeButton: true });

                    setTimeout(function () {
                        modal.hide();
                    }, 1000);
                } else {
                    setTimeout(function () {
                        modal.hide();
                    }, 1000);
                    toastr.error("<h1>" + response.data.Result.length + "</h1>",
                        ' Not Found',
                        { timeOut: 3000, positionClass: "toast-top-right", closeButton: true });
                }
            } else {
                $scope.AssetDetailList = [];
                setTimeout(function () {
                    modal.hide();
                }, 1000);
                toastr.error(response.data.Message,
                    ' Not Found',
                    { timeOut: 3000, positionClass: "toast-top-right", closeButton: true });
            }
        }, function errorCallback(response) {
            setTimeout(function () {
                modal.hide();
            }, 1000);
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function IsNullCheck(arryList) {
        if (typeof arryList !== 'undefined' && arryList.length > 0) {
            return true;
        } else {
            return false;
        }
    };

    //
    function initializeComponets() {

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        setTimeout(function () {
            SetControlProperty();
            modal.hide();
        }, 1000);
    };

    //
    function BindZone(_mSiteMasterId) {

        $http({
            method: 'GET',
            url: '../SiteTimeSpend/getZones',
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
                    // BindSubZone(dataItem.mZoneId);
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
            url: '../SiteTimeSpend/getSubZones',
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

            var mFloorMasterId = $("#mFloorMasterId").data("kendoDropDownList");
            mFloorMasterId.value(-1);

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
            url: '../SiteTimeSpend/getArea',
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

    function SetControlProperty() {

        $('#Ble').selectize({
            plugins: {
                'remove_button': {
                    label: ''
                }
            },
            placeholder: 'Enter',
            options: [

            ],
            render: {
                option: function (data, escape) {
                    return '<div class="option">' +
                        '<span>' + escape(data.title) + '</span>' +
                        '</div>';
                },
                item: function (data, escape) {
                    return '<div class="item">' + escape(data.title) + '</div>';
                }
            },
            maxItems: null,
            valueField: 'value',
            labelField: 'title',
            searchField: 'title',
            create: true,
            onDropdownOpen: function ($dropdown) {
                $dropdown
                    .hide()
                    .velocity('slideDown', {
                        begin: function () {
                            $dropdown.css({ 'margin-top': '0' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            },
            onDropdownClose: function ($dropdown) {
                $dropdown
                    .show()
                    .velocity('slideUp', {
                        complete: function () {
                            $dropdown.css({ 'margin-top': '' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            }
        });

        $('#SerialNumber').selectize({
            plugins: {
                'remove_button': {
                    label: ''
                }
            },
            placeholder: 'Enter',
            options: [

            ],
            render: {
                option: function (data, escape) {
                    return '<div class="option">' +
                        '<span>' + escape(data.title) + '</span>' +
                        '</div>';
                },
                item: function (data, escape) {
                    return '<div class="item">' + escape(data.title) + '</div>';
                }
            },
            maxItems: null,
            valueField: 'value',
            labelField: 'title',
            searchField: 'title',
            create: true,
            onDropdownOpen: function ($dropdown) {
                $dropdown
                    .hide()
                    .velocity('slideDown', {
                        begin: function () {
                            $dropdown.css({ 'margin-top': '0' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            },
            onDropdownClose: function ($dropdown) {
                $dropdown
                    .show()
                    .velocity('slideUp', {
                        complete: function () {
                            $dropdown.css({ 'margin-top': '' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            }
        });

        $('#PartNumber').selectize({
            plugins: {
                'remove_button': {
                    label: ''
                }
            },
            placeholder: 'Enter',
            options: [

            ],
            render: {
                option: function (data, escape) {
                    return '<div class="option">' +
                        '<span>' + escape(data.title) + '</span>' +
                        '</div>';
                },
                item: function (data, escape) {
                    return '<div class="item">' + escape(data.title) + '</div>';
                }
            },
            maxItems: null,
            valueField: 'value',
            labelField: 'title',
            searchField: 'title',
            create: true,
            onDropdownOpen: function ($dropdown) {
                $dropdown
                    .hide()
                    .velocity('slideDown', {
                        begin: function () {
                            $dropdown.css({ 'margin-top': '0' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            },
            onDropdownClose: function ($dropdown) {
                $dropdown
                    .show()
                    .velocity('slideUp', {
                        complete: function () {
                            $dropdown.css({ 'margin-top': '' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            }
        });

        $('#Lot').selectize({
            plugins: {
                'remove_button': {
                    label: ''
                }
            },
            placeholder: 'Enter',
            options: [

            ],
            render: {
                option: function (data, escape) {
                    return '<div class="option">' +
                        '<span>' + escape(data.title) + '</span>' +
                        '</div>';
                },
                item: function (data, escape) {
                    return '<div class="item">' + escape(data.title) + '</div>';
                }
            },
            maxItems: null,
            valueField: 'value',
            labelField: 'title',
            searchField: 'title',
            create: true,
            onDropdownOpen: function ($dropdown) {
                $dropdown
                    .hide()
                    .velocity('slideDown', {
                        begin: function () {
                            $dropdown.css({ 'margin-top': '0' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            },
            onDropdownClose: function ($dropdown) {
                $dropdown
                    .show()
                    .velocity('slideUp', {
                        complete: function () {
                            $dropdown.css({ 'margin-top': '' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            }
        });

        $('#mZoneId').kendoDropDownList({
        });
        $('#mFloorMasterId').kendoDropDownList({
        });
        $('#mRoomMasterId').kendoDropDownList({
        });

        $http({
            method: 'GET',
            url: '../AssetsReport/getMasterData'
        }).then(function successCallback(response) {
            //console.log(response.data);
            var objNullStatusName = { "StatusName": "ALL", "mStatusMasterId": null };
            var StatusName = response.data.mststu.concat(objNullStatusName);
            $('#Status').kendoDropDownList({
                dataTextField: "StatusName",
                dataValueField: "StatusName",
                filter: "contains",
                dataSource: StatusName,
                suggest: true,
                index: 3
            });

            var mStatusMasterId = $("#Status").data("kendoDropDownList");
            mStatusMasterId.value(null);

            var objNullSite = { "Site": "ALL", "mSiteMasterId": null };
            var Site = response.data.mSite.concat(objNullSite);
            $('#mSiteMasterId').kendoDropDownList({
                dataTextField: "Site",
                dataValueField: "mSiteMasterId",
                filter: "contains",
                select: onSelect,
                dataSource: Site,
                suggest: true,
                index: 3
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