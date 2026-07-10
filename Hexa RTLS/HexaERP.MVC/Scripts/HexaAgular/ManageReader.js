// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ManageReaderCtrl", function ($scope, GetRegService, $http, $timeout) {
    initializeComponets();
    var demo;

    //var FillterIData;

    //
    $scope.GetData = function () {
        GetRegService.getGetToTrack().then(function (result) {
            var myJsonString = JSON.stringify(result.data);
            $scope.FillterIData = result.data;
            //console.log($scope.FillterIData);
        }, function (error) {
            console.log(error);
        });
    };
    //
    function initializeComponets() {
        //SetControll();  

        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var myVar;
        GetTitles();
        //myVar = setInterval(function () {
        //    $scope.$apply(SetControll());
        //}, 1000);

        setTimeout(function () {
            modal.hide()
        }, 1000);
    };
    //
    function SetControll() {
        console.log('Called:HexaTracker');
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
        GetRegService.getGetToTrack().then(function (result) {
            $scope.FillterIData = result.data;
            //console.log($scope.FillterIData);
        }, function (error) {
            console.log(error);
        });
    };

    //StartReader
    $scope.StartReader = function () {
        UIkit.modal.confirm('Are you sure to Start Reader?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            $scope.lblMsg = "";

            $http({
                method: 'GET',
                url: '../ManageReader/ReaderInit'
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                     
                //$scope.lblMsg = response.data;
                //console.log(response.data);

                if (response.data.Flag == true) {
                    myVar = setInterval(function () {
                        $scope.$apply(SetControll());
                    }, 5000);

                    setTimeout(function () {
                        modal.hide()
                    }, 1000)

                    $scope.startbtn = false;
                    $scope.lblMsg = response.data.Message;
                }
                else if (response.data.Flag == false) {
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)
                    $scope.lblMsg = response.data.Message;
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
            url: '../ManageReader/StopReaders'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available 
            if (response.data.Flag == true) {
                $scope.lblMsg = response.data.Message;
                clearInterval(myVar);
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
            url: '../ManageReader/ReaderClear'
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
            url: '../ManageReader/getlocationdata'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);
            $scope.Location = response.data.IFloorData;
            $scope.Areas = response.data.IObjData;
            $scope.PortColl = response.data.IPortsData;
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
        return $http.get('../ManageReader/getGetFloorsData');
    };

    GetRegService._getRdata = function () {
        return $http.get('../ManageReader/getGetRoomsData');
    };

    GetRegService.getGetToTrack = function () {
        return $http.get('../ManageReader/getGetToTrackData');
    };

    return GetRegService;
}]);
