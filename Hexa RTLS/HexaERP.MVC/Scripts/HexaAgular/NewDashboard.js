var app = angular.module('app');

app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("NewDashboardCtrl", function ($scope, $http, $timeout) {
    $scope.currentDate = new Date();
    $scope.userName = 'User';
    $scope.orgName = 'Hexa ERP';

    // Load all dashboard data
    $scope.loadDashboard = function () {
        $http({
            method: 'GET',
            url: '../NewDashboard/GetDashboardData'
        }).then(function successCallback(response) {
            var d = response.data;

            // KPI
            $scope.totalAssets = d.TotalAssets || 0;
            $scope.activeAssets = d.ActiveAssets || 0;
            $scope.calibrationDue = d.CalibrationDue || 0;
            $scope.inspectionDue = d.InspectionDue || 0;
            $scope.maintenanceDue = d.MaintenanceDue || 0;
            $scope.assetsIssued = d.AssetsIssued || 0;
            $scope.assetsAvailable = d.AssetsAvailable || 0;
            $scope.expiredAssets = d.ExpiredAssets || 0;

            // Recent
            $scope.latestAssets = d.LatestAssets || [];
            $scope.latestCalibrations = d.LatestCalibrations || [];
            $scope.latestInspections = d.LatestInspections || [];
            $scope.latestMaintenance = d.LatestMaintenance || [];

            // Alerts
            $scope.upcomingCalibrations = d.UpcomingCalibrations || [];
            $scope.upcomingInspections = d.UpcomingInspections || [];

            // Transactions
            $scope.recentTransactions = d.RecentTransactions || [];

            // Charts
            $scope.assetsByDepartment = d.AssetsByDepartment || [];
            $scope.monthlyCalibrations = d.MonthlyCalibrations || [];
            $scope.maintenanceByType = d.MaintenanceByType || [];
            $scope.assetStatus = d.AssetStatus || [];

        }, function errorCallback(response) {
            console.log("Dashboard load error: " + (response.data ? response.data.ExceptionMessage : 'Unknown'));
        });
    };

    // Quick action navigation
    $scope.goTo = function (path) {
        window.location.href = path;
    };

    // Initialize
    $scope.loadDashboard();

    // Update clock every minute
    setInterval(function () {
        $scope.$apply(function () {
            $scope.currentDate = new Date();
        });
    }, 60000);
});