// ** Mudassar I **
//

//var ToolsTrackApp = angular.module("HexaToolsTrackApp", []);

// ** Mudassar I **

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ToolsTrackCtrl", function ($scope, GetRegService, $http, $timeout) {
    initializeComponets();

    $(function () {

    });
    //
    $scope.oK = function () {

    };
    //
    function initializeComponets() {
        //SetControll();   
        var myVar;
        GetReaders();
    }
    //
    function SetControll() {

    }
    //StartReader
    $scope.StartReader = function () {

        if ($scope.ReaderIp == "" || $scope.ReaderIp == null) {
            UIkit.modal.alert('Please Select IP Address');
            return false;
        } else {
            UIkit.modal.confirm('Are you sure to Start Reader?', function () {
                modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
                $scope.lblMsg = "";

                $http({
                    method: 'GET',
                    url: '../ToolsTrack/ReaderInit',
                    params: {
                        Reader: $scope.ReaderIp
                    }
                }).then(function successCallback(response) {
                    // this callback will be called asynchronously
                    // when the response is available                     
                    $scope.lblMsg = response.data;
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)

                    myVar = setInterval(function () {
                        $scope.$apply(ToolsTrackFun());
                    }, 5000);

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
        }
        //
        function ToolsTrackFun() {
            console.log(Date.now);
            GetAllCountsData();
            GetTrackCountData();
            GetTracklist();

        }

        ////
        //$scope.onTimeout = function ToolsTrackFun() {
        //    console.log(Date.now);
        //    GetAllCountsData();
        //    GetTrackCountData();
        //    GetTracklist();
        //};
    };
    //StopReader
    $scope.StopReader = function () {
        $http({
            method: 'GET',
            url: '../ToolsTrack/StopReaders'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available 
            $scope.lblMsg = response.data;
            clearInterval(myVar);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //ClearData
    $scope.ClearData = function () {
        $scope.lblMsg = "";
        $("#lblerr").text(""); $("#RFID").val(""); $("#PORTID").val(""); $("#global_filter").val("");
        $http({
            method: 'GET',
            url: '../ToolsTrack/ReaderClear'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available            
            $scope.lblMsg = response.data;
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
            console.log(response.data);
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
            console.log(response.data);
            $scope.getAllCount = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
    //
    function GetReaders() {
        GetRegService._getDatas().then(function (result) {
            $("#ReaderIp").kendoAutoComplete({
                dataSource: result.data,
                dataTextField: "ReaderIP",
                dataValueField: "ReaderIP",
                noDataTemplate: 'No Data!'
            });
            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    getDataList(dataItem.ReaderIP);
                }
            };
        }, function (error) {
            console.log(error);
        });
    }

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
            console.log(response.data);
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
        return $http.get('../ToolsTrack/getGetReadersData');
    };

    return GetRegService;
}]);
