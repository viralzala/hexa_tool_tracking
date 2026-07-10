
// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("TvAdminHomeCtrl", function ($scope, $http, $timeout) {

    initializeComponets();

    $scope.CurrentDate = new Date();

    $scope.hub = $.connection.aPIStatusHub;
    function initializeComponets() {
        // getallmodules();
        GetBatteryAlert();
        InitDataPartNumberProcess();
        getUserLoged();
        //  getTrendHour();      //
        InitDataBind();
        var myVar;

        myVar = setInterval(function () {
            $scope.$apply(InitDataPartNumberProcess());
        }, 60 * 1000);

        myVar = setInterval(function () {
            $scope.$apply(getUserLoged());
        }, 60 * 1000);

        setInterval(function () {
            $scope.$apply(GetBatteryAlert());
        }, 60000);
    }

    $scope.initNotifications = function () {
        //console.log('Called Hub');
        $scope.hub.client.getAPIStatus = function (_date, _flag) {
            testing(_date, _flag);
        }
        //
        $.connection.hub.start();
    }

    $scope.initNotifications();

    function GetBatteryAlert() {
        $http({
            method: 'GET',
            url: '../TVDisplayDashboard/BeaconBattery'
        }).then(function successCallback(response) {
            //console.log(response.data._BeaconsData);
            $scope.BeaconsData = response.data._BeaconsData;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function InitDataPartNumberProcess() {

        //console.log(new DATE());

        $http({
            method: 'GET',
            url: '../TVDisplayDashboard/PartNumberProcess'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            // console.log(response.data);
            $scope.PartNumberData = response.data._partNumberData;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function InitDataBind() {

        console.log(new Date());

        $http({
            method: 'GET',
            url: '../TVDisplayDashboard/GetNotifications'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            // console.log(response);
            $scope.Notifications = response.data.t;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function getTrendHour() {
        $http({
            method: 'GET',
            url: '../AdminMaster/getHourTrend'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available
            //console.log(response);
            bindCounts(response);
            $scope._TrendList = response.data.objadd;
            $scope._ReaderLog = response.data.ReaderLog;
            $scope._portLog = response.data.portLog;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
    function getUserLoged() {
        console.log(new Date());
        $http({
            method: 'GET',
            url: '../TVDisplayDashboard/GetCounts'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available
            //console.log(response);
            $scope._shelfDashboard = response.data._shelfData;
            $scope._smtProcutData = response.data._smtProcutData;
            //$scope._ReaderStatusData = response.data._ReaderStatusData
            $scope.ReaderStatus = response.data.ReaderStatus
            //document.getElementById("TotalShelf").className = "peity_orders peity_data";
            //console.log(document.getElementById("TotalShelf"));
            //document.getElementById("TotalShelf").innerHTML(response.data._shelfData.TotalShelf / 100);


            $scope.sitCount = response.data.locationCount;
            $scope.zCount = response.data.ZoneCount;
            $scope.subzCount = response.data.FloorCount;
            $scope.rCount = response.data.orders;
            $scope.aCount = response.data.att;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function getallmodules() {
        //Mudassar I Edited On 30/01/2017
        $.getJSON("../AdminMaster/getallmodules", function (data) {
            $scope.MainMenu = data;
            //   var $listSelector = $("#divforulandli");
            //   $.each(data, function (i, obj) {
            //       $("#Modellist").append('<h3 class="uk-accordion-title" ng-click="getLi(' + obj.RolemoduleId + ')">' + obj.moduleName + '</h3>\
            //                           <div class="uk-accordion-content">\
            //                               <ul class="uk-nav uk-nav-dropdown uk-panel"  ng-click="getLi(' + obj.RolemoduleId + ')"  id=' + obj.RolemoduleId + '></ul>\
            //</div>');
            //   });
        });
    };

    $scope.getLi = function (RolemoduleId) {
        $.getJSON("../AdminMaster/getallwindows", { moduleid: RolemoduleId }, function (data) {
            $('#' + RolemoduleId).html('');
            $.each(data, function (i, obj) {
                $('#' + RolemoduleId).append('<li><a href=' + obj.MenuUrl + '><span>' + obj.MenuName + '</span></a></li>');
            });
        });
    };
    //Mudassar I added On 30/01/2017
    //function mclick(RolemoduleId) {
    //    $.getJSON("../AdminMaster/getallwindows", { moduleid: RolemoduleId }, function (data) {
    //        $('#' + RolemoduleId).html('');
    //        $.each(data, function (i, obj) {
    //            $('#' + RolemoduleId).append('<li><a href=' + obj.MenuUrl + '><span>' + obj.MenuName + '</span></a></li>');
    //        });
    //    });
    //};
});

function testing(_data, _flag) {

    if (_flag === true) {
        $('#spAPI').append('<li>' + _data + '</li>').children().last().css('color', 'green');
    }
    else {
        $('#spAPI').append('<li>' + _data + '</li>').children().last().css('color', 'red');
    }
}

function bindCounts(_data) {

    var chart = c3.generate({
        bindto: '#epmChart',
        data: {
            columns: _data.data.assetemplCount,
            type: 'bar'
        },
        bar: {
            width: {
                ratio: 0.5 // this makes bar width 50% of length between ticks
            }
            // or
            //width: 100 // this makes bar width 100px
        }
    });

    var chart = c3.generate({
        bindto: '#asssetper',
        data: {
            columns: _data.data.assetemplCount,
            type: 'pie'
        },
        bar: {
            width: {
                ratio: 0.5 // this makes bar width 50% of length between ticks
            }
            // or
            //width: 100 // this makes bar width 100px
        }
    });

    chart = c3.generate({
        bindto: '#issuedDate',
        data: {
            columns: _data.data._IssueDate,
            type: 'bar'
        },
        axis: {
            x: {
                type: "category"
            }
        }
    });

    chart = c3.generate({
        bindto: '#AssetList',
        data: {
            columns: _data.data._AssetList,
            type: 'bar'
        },
        axis: {
            x: {
                type: "categorized"
            }
        }
    });



    chart = c3.generate({
        bindto: '#noOfdaysLeft',
        data: {
            columns: _data.data.noOfdaysLeft,
            type: 'bar'
        },
        width: {
            ratio: 0.5 // this makes bar width 50% of length between ticks
        }
        // or
        //width: 100 // this makes bar width 100px
        ,
        axis: {
            x: {
                type: "categorized"
            }
        }
    });

    chart = c3.generate({
        bindto: '#assetDesp',
        data: {
            columns: _data.data._AssetDep,
            type: 'bar'
        },
        width: {
            ratio: 0.5 // this makes bar width 50% of length between ticks
        }
        // or
        //width: 100 // this makes bar width 100px
        ,
        axis: {
            x: {
                type: "categorized"
            }
        }
    });
}