// ** Mudassar I **
//

//var app = angular.module("HexaToolsTrackReportApp", []);

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ControllReaderCtrl", function ($timeout, $scope, $http) {
    initializeComponets();

    //
    function GetData() {
        $http({
            method: 'GET',
            url: '/ControllReader/GetReaderData'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available      
            //console.log(response.data);
            //var obj = JSON.stringify(response.data);           
            //console.log(obj);
            $scope.DataLists = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }

    $scope.setAction = function (_id, _st) {
        console.log(_id);
        $http({
            method: 'GET',
            url: '/ControllReader/setReaderContrl',
            params: { ID: _id, status: _st }
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available                         
            alert(response.data.Msg);
            GetData();
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function initializeComponets() {
        GetData();
    };

});

