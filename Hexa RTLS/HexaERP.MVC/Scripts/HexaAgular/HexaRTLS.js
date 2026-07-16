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

    // NEW: Two-level navigation state
    // showShelfMap = false means we show the floor cards dashboard
    // showShelfMap = true means we show the warehouse shelf map
    $scope.showShelfMap = false;
    $scope.selectedFloorName = '';
    $scope.selectedZoneName = '';
    $scope.shelfCount = 0;

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
            console.log("===== GetTrackData Response =====");
            console.log(response);

            $scope.Shelf = response.data.objText;
            $scope.Location = response.data.IZoneData;
            $scope.Areas = response.data.IsubZoneData;
            $scope.PortColl = response.data.IPortsData;

            // NEW: Show floor cards dashboard by default
            $scope.showShelfMap = false;
            
            $timeout(function () {
                console.log("Shelf Count:", $scope.Shelf.length);
                
                if (UIkit && UIkit.Utils) {
                    UIkit.Utils.checkDisplay(document.getElementById("contact_list"));
                }
            }, 300);
            
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function SetControll() {
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();

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
                $timeout(function () {
                    if (UIkit && UIkit.Utils) {
                        UIkit.Utils.checkDisplay(document.getElementById("contact_list"));
                    }
                }, 300);
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    };

    // NEW: Open floor map - navigates from dashboard to shelf map view
    // When user clicks "View" on a floor card, it loads shelves for that zone
    // and switches to the warehouse map view
    $scope.openFloorMap = function (floorItem) {
        console.log("Opening floor map for:", floorItem);

        // Store selected floor details for the header
        $scope.selectedFloorName = floorItem.FloorName || 'Floor';
        $scope.selectedZoneName = floorItem.Zone || '';
        $scope.SelectedLocation = floorItem.mZoneId;

        // Load shelf data for this floor/zone
        $http({
            method: 'GET',
            url: '../HexaRTLS/GetTrackData',
            params: {
                mZoneId: floorItem.mZoneId
            }
        }).then(function (response) {
            $scope.Shelf = response.data.objText;
            $scope.shelfCount = $scope.Shelf ? $scope.Shelf.length : 0;
            
            // NEW: Switch to shelf map view
            $scope.showShelfMap = true;

            $timeout(function () {
                console.log("Shelf Loaded :", $scope.Shelf.length);
                
                if (UIkit && UIkit.Utils) {
                    UIkit.Utils.checkDisplay(document.getElementById("contact_list"));
                }
                
                if (UIkit && UIkit.filter) {
                    var filterElements = document.querySelectorAll('[data-uk-filter]');
                    if (filterElements.length > 0) {
                        console.log("UIkit filter elements found:", filterElements.length);
                    }
                }
            }, 100);

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    // NEW: Go back to dashboard from shelf map view
    $scope.goBackToDashboard = function () {
        $scope.showShelfMap = false;
        $scope.selectedFloorName = '';
        $scope.selectedZoneName = '';
        $scope.shelfCount = 0;
        console.log("Returned to floor dashboard");
    };

    $scope.ShowProduct = function (p) {

        $scope.SelectedLocation = p.mZoneId;

        $http({
            method: 'GET',
            url: '../HexaRTLS/GetTrackData',
            params: {
                mZoneId: p.mZoneId
            }
        }).then(function (response) {

            $scope.Shelf = response.data.objText;
            
            // FIXED: Use $timeout to ensure DOM is updated before UIkit refresh
            $timeout(function () {
                console.log("Shelf Loaded :", $scope.Shelf.length);
                
                if (UIkit && UIkit.Utils) {
                    UIkit.Utils.checkDisplay(document.getElementById("contact_list"));
                }
                
                if (UIkit && UIkit.filter) {
                    var filterElements = document.querySelectorAll('[data-uk-filter]');
                    if (filterElements.length > 0) {
                        console.log("UIkit filter elements found:", filterElements.length);
                    }
                }
            }, 100);

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };
});