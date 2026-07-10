// ** Mudassar I **
//

//var app = angular.module("HexaToolsTrackReportApp", []);

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("EmployeeDetailListCtrl", function ($timeout, $scope, $http) {
    initializeComponets();   

    //
    function initializeComponets() {
        $http({
            method: 'GET',
            url: '/EmployeeDetailList/GetEmployeeTrack'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);
            $scope.DataLists = response.data;

        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

});

