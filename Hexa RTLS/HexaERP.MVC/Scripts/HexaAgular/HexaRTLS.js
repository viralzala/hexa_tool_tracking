// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("HexaRTLSCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    $scope.SelectedLocation = null;
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
        InitDataBind();
        myVar = setInterval(function () {
            $scope.$apply(SetControll());
            // $scope.$apply(InitDataBind());
        }, 10000);
    }

    $scope.setInformation = function (iData, Locat) {
        $scope.EmpDetails = null;
        $scope.EmpDetails = iData;

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

    $scope.setAssetInformation = function (iData, Locat) {
        $scope.AssetDetail = null;
        $scope.AsRrack = Locat;
        $scope.AssetDetail = iData;
    };

    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../HexaRTLS/getlocationdata'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            console.log(response.data);
            $scope.Location = response.data.IZoneData;
            $scope.Areas = response.data.IsubZoneData;
            $scope.PortColl = response.data.IPortsData;
            // $scope.Shelf = response.data.objText;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function SetControll() {
        // console.log($scope.SelectedLocation);
        var d = new Date();
        //$scope.lastTracked = d.toLocaleTimeString();

        if ((angular.isUndefined($scope.SelectedLocation) || $scope.SelectedLocation === null) && (angular.isUndefined($scope.SelectedLocation) || $scope.SelectedLocation === null)) {
            return false;
        } else {
            $http({
                method: 'GET',
                url: '../HexaRTLS/GetTrackData',
                params: {
                    mZoneId: parseInt($scope.SelectedLocation)
                }
            }).then(function successCallback(response) {
                // console.log(response);
                $scope.Shelf = response.data.objText;
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    };

    $scope.ShowProduct = function (p) {
        console.log(p);
        $scope.SelectedLocation = p.mZoneId;
        SetControll();
    };
});

