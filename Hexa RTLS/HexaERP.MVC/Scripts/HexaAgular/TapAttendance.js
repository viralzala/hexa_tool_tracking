// ** Mudassar I **
//

//var app = angular.module("HexaToolsTrackReportApp", []);

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("TapAttendanceCtrl", function ($timeout, $scope, $http) {
    initializeComponets();

    function initializeComponets() {
        getTools();

    }

    function SetControll() {
        $http({
            method: 'GET',
            url: '../TapAttendance/getData'
        }).then(function successCallback(response) {

            if (response.data.Flag == true) {
                //console.log(response.data);
                $scope.DataList = response.data.Datas;
            }
            else {
                console.log(response.data);               
            }
          
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.putPorts = function (Idata) {
        console.log(Idata[0].Key);
        //console.log($scope.ddlFruits);
        $http({
            method: 'GET',
            url: '../TapAttendance/PutStart',
            params: {
                _Port: Idata[0].Key
            }
        }).then(function successCallback(response) {
            console.log(response.data);
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

    function getTools() {
        $http({
            method: 'GET',
            url: '../TapAttendance/GetPorts'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);
            $scope.ListPort = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});