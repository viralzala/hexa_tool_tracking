// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("EmployeeLocatorCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    //
    function initializeComponets() {
        //$scope.isEdit = true;     
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var myVar;
        InitDataBind();
        setTimeout(function () {
            modal.hide()
        }, 1000);

        myVar = setInterval(function () {
            $scope.$apply(SetControll());
        }, 3000);
    }

    $scope.setInformation = function (iData,Locat) {
        console.log(iData);
        $scope._EmpId = iData.EmployeeId;
       
        $scope._Name = iData.Name;
        $scope._RFID = iData.Epc;
        $scope._Agency = iData.Agency;
        $scope._Designation = iData.Designation;
        $scope._SkillCategory = iData.SkillCategory;
        $scope._WorkCategory = iData.WorkCategory;
        $scope._Activity = iData.Activity;
        $scope._trackWork = Locat;   
        $scope._tDate = iData.tDate;
    };

    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../EmployeeLocator/getlocationdata'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);
            $scope.Location = response.data.IZoneData;
            $scope.Areas = response.data.IsubZoneData;
            $scope.PortColl = response.data.IPortsData;
           // console.log(response.data.IsubZoneData);
            //console.log(response.data.IZoneData);
            //console.log(response.data.IPortsData);

            //console.log($scope.PortColl);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function SetControll() {
        //console.log('Called:HexaTracker');
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
        $http({
            method: 'GET',
            url: '../EmployeeLocator/getGetToTrackData'
        }).then(function successCallback(response) {
            //console.log(response.data);
            $scope.FillterIData = response.data;
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});

