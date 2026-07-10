// ** Mudassar I **
//

//var app = angular.module("HexaToolsTrackReportApp", []);

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("EmployeeTrackReportCtrl", function ($timeout, $scope, $http) {
    initializeComponets();
    //
    $scope.oK = function () {
        $scope.bindfromDate = $("#uk_dp_start").val(); $scope.bindtoDate = $("#uk_dp_end").val();
        console.log($scope.bindfromDate, $scope.bindtoDate);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        $http({
            method: 'GET',
            url: '/EmployeeTrackReport/GetEmployeeTrack',
            params: { FromDate: $scope.bindfromDate, EndDate: $scope.bindtoDate }
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
           // console.log(response.data);
            $scope.DataLists = response.data;
            setTimeout(function () {
                modal.hide()
            }, 2000)
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function initializeComponets() {

    }

});

