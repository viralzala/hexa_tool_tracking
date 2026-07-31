// ** BLE Battery Alert Module **
//

app.controller("BLEBatteryAlertCtrl", function ($scope, $http, $timeout) {
    // Initialize loading state
    $scope.statsLoading = true;
    $scope.CurrentDate = new Date();

    // Load statistics
    $scope.loadStatistics = function () {
        $scope.statsLoading = true;
        $http({
            method: 'GET',
            url: '../BLEBatteryAlert/GetDashboard'
        }).then(function successCallback(response) {
            if (response.data) {
                $scope.statTotal = response.data.Total;
                $scope.statHealthy = response.data.Healthy;
                $scope.statMedium = response.data.Medium;
                $scope.statLow = response.data.Low;
            }
            $scope.statsLoading = false;
        }, function errorCallback(response) {
            console.log("Error loading statistics: " + response.data.ExceptionMessage);
            $scope.statsLoading = false;
        });
    };

    // Load grid data
    $scope.loadGridData = function () {
        $http({
            method: 'GET',
            url: '../BLEBatteryAlert/GetData',
            params: {
                bleId: $scope.searchBLEId || "",
                assetName: $scope.searchAssetName || ""
            }
        }).then(function successCallback(response) {
            if (response.data) {
                BindJqueryTable(response.data);
            }
        }, function errorCallback(response) {
            console.log("Error loading data: " + response.data.ExceptionMessage);
        });
    };

    // Search data
    $scope.searchData = function () {
        $scope.loadGridData();
    };

    // Refresh data
    $scope.refreshData = function () {
        $scope.searchBLEId = "";
        $scope.searchAssetName = "";
        $scope.loadStatistics();
        $scope.loadGridData();
    };

    // Initialize components
    function initializeComponents() {
        $scope.loadStatistics();
        $scope.loadGridData();
    };

    // Bind data to jQuery DataTable
    function BindJqueryTable(pData) {
        if ($('#dt_tableExport').length === 0) { return; }
        if (!Array.isArray(pData) || pData.length === 0) { return; }
        console.log("Data :", pData);
        console.log("Rows :", pData.length);

        if ($.fn.DataTable && $.fn.DataTable.isDataTable('#dt_tableExport')) {
            $('#dt_tableExport').DataTable().destroy();
        }

        if ($('#dt_tableExport tbody').length === 0) {
            $('#dt_tableExport').append('<tbody></tbody>');
        }

        $('#dt_tableExport').dataTable({
            "bProcessing": true,
            "aaData": pData,
            "aoColumns": [
                { "mData": "BLEId" },
                { "mData": "AssetName" },
                {
                    "mData": "BatteryLevel",
                    "render": function (aaData, type, row, meta) {
                        var batteryLevel = parseInt(row.BatteryLevel) || 0;
                        var badgeClass = 'premium-badge-success';
                        var badgeText = batteryLevel + '%';

                        if (batteryLevel >= 70) {
                            badgeClass = 'premium-badge-success';
                        }
                        else if (batteryLevel >= 30 && batteryLevel <= 69) {
                            badgeClass = 'premium-badge-warning';
                        }
                        else if (batteryLevel < 30) {
                            badgeClass = 'premium-badge-danger';
                        }

                        return '<span class="premium-badge ' + badgeClass + '">' + badgeText + '</span>';
                    }
                }
            ]
        });
    }

    // Initialize components after all functions are defined
    initializeComponents();
});