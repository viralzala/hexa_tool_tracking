// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ToolsTrackDemoCtrl", function ($scope, GetRegService, $http, $timeout) {
    initializeComponets();
    var demo;

    //
    $scope.hub = $.connection.readerStatusHub;

    $scope.initNotifications = function () {
        //console.log('Called Hub');
        $scope.hub.client.readerstatus = function (_readerPhysic, _readerStatus) {
            //if (_readerPhysic == "False") {
            //    $("#Lia").append("<li>Connection Lost :<code>" + _readerStatus.Name + "</code></li>");
            //}     

            //console.log("Status Events" + _readerStatus);   
            $scope.readerPhysic = _readerStatus;
            $scope.ReaderStatus = _readerStatus;

            //console.log("_readerPhysic" + _readerPhysic); console.log("_readerStatus" + _readerStatus);
        }
        //
        $scope.hub.client._attenaEvents = function (message, _attEvent) {
            //console.log("Att Events" + _attEvent);   
            $("#Lia").append("<li><code>" + _attEvent + "</code></li>");
            //angular.forEach(_attEvent, function (value, key) {

            //    //document.write('\n');
            //});
        }

        $.connection.hub.start();
    }

    $scope.initNotifications();

    //var FillterIData;
    $scope.TxValw = function () {
        $scope._Txval = $scope.TxVal;
        //console.log("call");
    };

    //
    function GetStatus() {
        console.log('Called:Reader Status');
        //$scope.ReaderStatus = null;
        $http({
            method: 'GET',
            url: '../ToolsTrackDemo/ReaderStatusDetail'
        }).then(function successCallback(response) {
            // console.log(response);
            //$scope.ReaderStatus = response.data;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function initializeComponets() {
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
        $scope.lblMsg = "Wait moment...";
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var myVar;
        GetTitles();
        //myVar = setInterval(function () {
        //    $scope.$apply(SetControll());
        //}, 1000);
        // SetControll();         

        setTimeout(function () {
            modal.hide()
        }, 1000);
        $scope.lblMsg = "Now application is ready please click on start button.";
    };
    //
    function SetControll() {
        //console.log('Called:HexaTracker');
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
        GetRegService.getGetToTrack().then(function (result) {
            $scope.FillterIData = result.data;
            //console.log(result);
        }, function (error) {
            console.log(error);
        });
    };

    //StartReader
    $scope.StartReader = function () {

        $scope._Rssival = $("#ionslider_2").val();
        $scope._Txval = $("#ionslider_1").val();

        UIkit.modal.confirm('Are you sure to Start Reader?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please wait for a moment reader configuring and starting.<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            $scope.lblMsg = "";

            $http({
                method: 'GET',
                url: '../ToolsTrackDemo/ReaderInit',
                params: { _Rssival: $("#ionslider_2").val(), TxVal: $("#ionslider_1").val(), _isXspan: false }
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                     
                //$scope.lblMsg = response.data;
                console.log(response.data);

                if (response.data.Flag == true) {

                    myVar = setInterval(function () {
                        $scope.$apply(SetControll());
                    }, 5000);

                    //myVar = setInterval(function () {
                    //    $scope.$apply(GetStatus());
                    //}, 5000);

                    setTimeout(function () {
                        modal.hide()
                    }, 1000)

                    $scope.startbtn = false;
                    $scope.lblMsg = response.data.Message;
                    toastr.success(response.data.Message);
                }
                else if (response.data.Flag == false) {

                    setTimeout(function () {
                        modal.hide()
                    }, 1000)
                    $scope.lblMsg = response.data.Message;
                    toastr.error(response.data.Message);
                }

                //mytimeout = $timeout($scope.onTimeout, 15000);
                //var mdata;
                //var timer;
                //StartIntr();
                //function StartIntr() {
                //    mdata = setInterval("ToolsTrackFun()", 10000);
                //}
            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                console.log("Error : " + response.data.ExceptionMessage);
            });

        });
    };
    //StopReader
    $scope.StopReader = function () {
        // var conf = alert.confirm();
        $http({
            method: 'GET',
            url: '../ToolsTrackDemo/StopReaders'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available 
            if (response.data.Flag == true) {
                $scope.lblMsg = response.data.Message;
                clearInterval(myVar);
                $scope.startbtn = true;
            }
            else if (response.data.Flag == false) {
                $scope.lblMsg = response.data.Message;
            }
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //ClearData
    $scope.ClearData = function () {
        $scope.lblMsg = "";

        $http({
            method: 'GET',
            url: '../ToolsTrackDemo/ReaderClear'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available   
            if (response.data.Flag == true) {
                $scope.lblMsg = response.data.Message;
            }
            else if (response.data.Flag == false) {
                $scope.lblMsg = response.data.Message;
            }
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    function GetTracklist() {
        $http({
            method: 'GET',
            url: '../ToolsTrack/getGetToTrackData'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available             
            $scope.TrackedList = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });

        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
    }
    //
    function GetTrackCountData() {
        $http({
            method: 'GET',
            url: '../ToolsTrack/GetTrackCount'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  

            $scope.TrackedData = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
    //
    function GetAllCountsData() {
        $http({
            method: 'GET',
            url: '../ToolsTrack/GetAllCount'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available    

            $scope.getAllCount = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
    //
    function GetTitles() {
        $http({
            method: 'GET',
            url: '../ToolsTrackDemo/getlocationdata'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);

            $scope.Location = response.data.IZoneData;
            $scope.Areas = response.data.IsubZoneData;
            $scope.PortColl = response.data.IPortsData;

            //console.log(response.data.IPortsData);
            //console.log(response.data.IZoneData);
            //console.log(response.data.IPortsData);

            //console.log($scope.PortColl);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
    //
    function getDataList(Id) {

        $http({
            method: 'GET',
            url: '../ReaderConfig/getData',
            params: {
                _orgId: Id
            }
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available            
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }

});

app.factory('GetRegService', ['$http', function ($http) {
    var GetRegService = {};

    GetRegService._getDatas = function () {
        return $http.get('../ToolsTrackDemo/getGetFloorsData');
    };

    GetRegService._getRdata = function () {
        return $http.get('../ToolsTrackDemo/getGetRoomsData');
    };

    GetRegService.getGetToTrack = function () {
        return $http.get('../ToolsTrackDemo/getGetToTrackData');
    };

    return GetRegService;
}]);