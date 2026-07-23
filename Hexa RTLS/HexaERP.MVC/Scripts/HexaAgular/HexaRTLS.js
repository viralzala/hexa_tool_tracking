// ** Mudassar I **
// jsonDate filter: Converts JSON date string to formatted date
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

//
app.controller("HexaRTLSCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    // ---- Existing Variables (PRESERVED) ----
    $scope.SelectedLocation = null;

    // ---- NEW: 3-Step Navigation State ----
    // Step 1: Location filter selected
    $scope.selectedLocationId = null;    // mZoneId of selected location
    $scope.selectedLocationName = '';    // Zone name of selected location
    
    // Step 2: Floor selected for asset view  
    $scope.selectedFloorId = null;       // mFloorMasterId of selected floor
    $scope.selectedFloorName = '';       // FloorName of selected floor
    
    // Step 3: Asset view toggle
    $scope.showAssetsView = false;       // true = show assets, false = show floor cards
    
    // Data containers (populated from API)
    $scope.Floors = [];                  // All floors from IsubZoneData
    $scope.Assets = [];                  // All shelves/items from objText
    $scope.DBAssets = [];                // FIXED: Actual assets from tAssetTag table (AssetTagData)
    
    // ---- EXISTING: Employee/Asset Detail Storage (PRESERVED) ----
    $scope.EmpDetails = null;
    $scope.AssetDetail = null;
    $scope.AsRrack = '';
    $scope._trackWork = '';

    // ---- NEW: Multi-Floor Display State ----
    $scope.selectedFloors = [];              // Array of selected floor IDs for multi-floor view
    $scope.showMultiFloorView = false;       // true = show multiple floor maps
    $scope.multiFloorData = null;            // Data for multiple floor maps
    $scope.searchQuery = '';                 // Asset search query
    $scope.searchResults = [];               // Asset search results
    $scope.showSearchResults = false;        // Show search results panel
    $scope.highlightedAsset = null;          // Currently highlighted asset
    $scope.assetPopupVisible = false;        // Show asset info popup

    //
    function initializeComponets() {
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var myVar;
        InitDataBind();
        setTimeout(function () {
            modal.hide()
        }, 1000);
        InitDataBind();
        
        // Auto-refresh every 10 seconds (PRESERVED)
        myVar = setInterval(function () {
            $scope.$apply(SetControll());
        }, 10000);
    }

    // ---- EXISTING: Employee Information Modal (PRESERVED - unchanged) ----
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

    // ---- EXISTING: Asset Information Modal (PRESERVED - unchanged) ----
    $scope.setAssetInformation = function (iData, Locat) {
        $scope.AssetDetail = null;
        $scope.AsRrack = Locat;
        $scope.AssetDetail = iData;
    };

    //
    // ---- EXISTING: Data Binding (PRESERVED + enhanced) ----
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../HexaRTLS/getlocationdata'
        }).then(function successCallback(response) {
            console.log("===== GetTrackData Response =====");
            console.log(response);

            // EXISTING: Store shelf data
            $scope.Shelf = response.data.objText;
            
            // EXISTING: Store location data (zone list for filter buttons)
            $scope.Location = response.data.IZoneData;
            
            // EXISTING: Store floor/zone data (PRESERVED variable name)
            $scope.Areas = response.data.IsubZoneData;
            
            // NEW: Store floors in a separate array for easier filtering
            $scope.Floors = response.data.IsubZoneData || [];
            
            // EXISTING: Store port data
            $scope.PortColl = response.data.IPortsData;
            
            // NEW: Store all shelves/assets for lookup
            $scope.Assets = response.data.objText || [];

            // FIXED: Load actual assets from database via AssetTagData
            // This data comes from tAssetTag table with mZoneId and mFloorMasterId
            // making it possible to filter assets by floor/zone correctly
            $scope.DBAssets = response.data.AssetTagData || [];

            // NEW: Show floor cards by default
            $scope.showAssetsView = false;

            $timeout(function () {
                console.log("Shelf Count:", $scope.Shelf.length);
                console.log("Floor Count:", $scope.Floors.length);
                console.log("Location Count:", $scope.Location.length);
                console.log("DB Asset Count:", $scope.DBAssets.length);
            }, 300);
            
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    // ---- EXISTING: Auto-refresh controller (PRESERVED) ----
    function SetControll() {
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();

        if ((angular.isUndefined($scope.SelectedLocation) || $scope.SelectedLocation === null)) {
            return false;
        } else {
            $http({
                method: 'GET',
                url: '../HexaRTLS/GetTrackData',
                params: {
                    mZoneId: parseInt($scope.SelectedLocation)
                }
            }).then(function successCallback(response) {
                // Reload shelf data on refresh
                $scope.Shelf = response.data.objText;
                $scope.Assets = response.data.objText || [];
                
                // Re-filter floors in case data changed
                $scope.Floors = $scope.Areas || [];
                
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

    // ============================================
    // NEW: STEP 1 - Select Location Filter
    // When a location button is clicked, filter 
    // floors to show only those in that zone.
    // ============================================
    $scope.selectLocation = function (location) {
        if (location == null) {
            // "All" selected - clear filter
            $scope.selectedLocationId = null;
            $scope.selectedLocationName = '';
        } else {
            // Specific location selected
            $scope.selectedLocationId = location.mZoneId;
            $scope.selectedLocationName = location.Zone;
        }
        
        // Reset to floor cards view when changing location
        $scope.showAssetsView = false;
        $scope.selectedFloorId = null;
        $scope.selectedFloorName = '';
        
        console.log("Location selected:", $scope.selectedLocationName, "ID:", $scope.selectedLocationId);
    };

    // ============================================
    // NEW: Computed - Filtered Floors
    // Returns only floors that belong to the 
    // selected location (zone).
    // ============================================
    $scope.filteredFloors = [];
    
    // Watch for changes in Areas or selectedLocationId and recompute filtered floors
    $scope.$watch('[Areas, selectedLocationId]', function () {
        if ($scope.selectedLocationId == null) {
            // Show all floors when "All" is selected
            $scope.filteredFloors = $scope.Floors;
        } else {
            // Filter floors by selected zone
            $scope.filteredFloors = $scope.Floors.filter(function (floor) {
                return floor.mZoneId === $scope.selectedLocationId;
            });
        }
    }, true);

    // ============================================
    // FIXED: STEP 2 - Select Floor
    // When "View Assets" is clicked, fetch assets
    // from the database that belong to this floor.
    // Uses the new GetAssetsByFloor API endpoint.
    // ============================================
    $scope.selectFloor = function (floor) {
        $scope.selectedFloorId = floor.mFloorMasterId;
        $scope.selectedFloorName = floor.FloorName;
        
        // FIXED: Call the new API to get actual assets from database by floor
        $http({
            method: 'GET',
            url: '../HexaRTLS/GetAssetsByFloor',
            params: {
                mFloorMasterId: floor.mFloorMasterId
            }
        }).then(function (response) {
            // Set the assets list from database response
            $scope.assetsList = response.data.assets || [];
            
            // Switch to assets view
            $scope.showAssetsView = true;
            
            console.log("Floor selected:", $scope.selectedFloorName, 
                        "Assets from DB:", $scope.assetsList.length);
            
        }, function errorCallback(response) {
            console.log("Error loading assets:", response.data);
            $scope.assetsList = [];
            $scope.showAssetsView = true;
        });
    };

    // ============================================
    // FIXED: Get Asset Count for a Floor
    // Counts actual assets from tAssetTag table 
    // that belong to this floor (by mFloorMasterId).
    // ============================================
    $scope.getFloorAssetCount = function (floor) {
        if (!$scope.DBAssets || $scope.DBAssets.length === 0) {
            return 0;
        }
        // Count assets from database that match this floor's mFloorMasterId
        var count = $scope.DBAssets.filter(function (asset) {
            return asset.mFloorMasterId === floor.mFloorMasterId;
        }).length;
        return count;
    };

    // ============================================
    // FIXED: Get Asset Status
    // Determines the display status based on 
    // database IsAction property.
    // ============================================
    $scope.getAssetStatus = function (asset) {
        if (asset.IsAction == false || asset.IsAction === false) {
            return 'Available';
        } else if (asset.mStatusMasterId != null) {
            return 'In Use';
        } else {
            return 'Available';
        }
    };

    // ============================================
    // FIXED: Get Asset Status CSS Class
    // Returns the CSS class for the asset card 
    // border color based on database status.
    // ============================================
    $scope.getAssetStatusClass = function (asset) {
        if (asset.IsAction == false || asset.IsAction === false) {
            return 'asset-available';
        } else if (asset.mStatusMasterId != null) {
            return 'asset-occupied';
        } else {
            return 'asset-available';
        }
    };

    // ============================================
    // FIXED: Get Status Badge CSS Class
    // Returns the CSS class for the status 
    // indicator dot.
    // ============================================
    $scope.getStatusClass = function (asset) {
        if (asset.IsAction == false || asset.IsAction === false) {
            return 'available';
        } else if (asset.mStatusMasterId != null) {
            return 'occupied';
        } else {
            return 'available';
        }
    };

    // ============================================
    // NEW: Reset to Locations View
    // Goes back from asset view to floor cards.
    // ============================================
    $scope.resetToLocations = function () {
        $scope.showAssetsView = false;
        $scope.selectedFloorId = null;
        $scope.selectedFloorName = '';
        $scope.assetsList = [];
        console.log("Returned to floor cards view");
    };

    // ============================================
    // NEW: Safety Report Download
    // Navigates to the safety report download URL.
    // ============================================
    $scope.downloadSafetyReport = function () {
        window.location.href = '../EmployeeTrackSummary/SafetyExportExcelAsync';
    };

    // ---- EXISTING: Show Product (PRESERVED + enhanced) ----
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
            $scope.Assets = response.data.objText || [];

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

    // ============================================
    // NEW: Multi-Floor Display Functions
    // ============================================

    // NEW: Toggle Floor Selection for Multi-Floor View
    // Toggles a floor in/out of the selected floors array
    $scope.toggleFloorSelection = function (floor) {
        var index = $scope.selectedFloors.indexOf(floor.mFloorMasterId);
        if (index > -1) {
            // Floor already selected, remove it
            $scope.selectedFloors.splice(index, 1);
        } else {
            // Floor not selected, add it
            $scope.selectedFloors.push(floor.mFloorMasterId);
        }
        console.log("Selected floors:", $scope.selectedFloors);
    };

    // NEW: Check if Floor is Selected
    // Returns true if the floor is in the selected floors array
    $scope.isFloorSelected = function (floorId) {
        return $scope.selectedFloors.indexOf(floorId) > -1;
    };

    // NEW: Show Multi-Floor Maps
    // Loads and displays multiple floor maps simultaneously
    $scope.showMultiFloorMaps = function () {
        if ($scope.selectedFloors.length === 0) {
            alert('Please select at least one floor to display.');
            return;
        }

        $http({
            method: 'GET',
            url: '../HexaRTLS/GetMultiFloorMaps',
            params: {
                floorIds: $scope.selectedFloors
            }
        }).then(function (response) {
            $scope.multiFloorData = response.data;
            $scope.showMultiFloorView = true;
            $scope.showAssetsView = false;
            console.log("Multi-floor maps loaded:", $scope.multiFloorData);
        }, function errorCallback(response) {
            console.log("Error loading multi-floor maps:", response.data);
            alert('Error loading floor maps. Please try again.');
        });
    };

    // NEW: Close Multi-Floor View
    // Returns to the floor cards view
    $scope.closeMultiFloorView = function () {
        $scope.showMultiFloorView = false;
        $scope.multiFloorData = null;
        $scope.selectedFloors = [];
        $scope.highlightedAsset = null;
        $scope.assetPopupVisible = false;
    };

    // ============================================
    // NEW: Asset Search Functions
    // ============================================

    // NEW: Search Asset
    // Searches for assets by ID, RFID, Barcode, or Name
    $scope.searchAsset = function () {
        if (!$scope.searchQuery || $scope.searchQuery.trim() === '') {
            $scope.searchResults = [];
            $scope.showSearchResults = false;
            return;
        }

        $http({
            method: 'GET',
            url: '../HexaRTLS/SearchAsset',
            params: {
                searchTerm: $scope.searchQuery.trim()
            }
        }).then(function (response) {
            $scope.searchResults = response.data.assets || [];
            $scope.showSearchResults = true;
            console.log("Search results:", $scope.searchResults.length, "assets found");
        }, function errorCallback(response) {
            console.log("Error searching asset:", response.data);
            $scope.searchResults = [];
            $scope.showSearchResults = true;
        });
    };

    // NEW: Locate Asset on Floor Map
    // Finds the asset's latest location and highlights it on the correct floor
    $scope.locateAsset = function (asset) {
        if (!asset.RFID) {
            alert('Asset does not have RFID tag information.');
            return;
        }

        $http({
            method: 'GET',
            url: '../HexaRTLS/GetAssetLatestLocation',
            params: {
                rfid: asset.RFID
            }
        }).then(function (response) {
            if (response.data.success) {
                var location = response.data.location;
                
                // Check if the asset's floor is in the currently displayed multi-floor view
                var floorInView = $scope.selectedFloors.indexOf(location.mFloorMasterId);
                
                if (floorInView === -1) {
                    // Asset is not on any displayed floor
                    alert('Asset is not present on the selected floor(s).');
                    return;
                }

                // Highlight the asset on the correct floor
                $scope.highlightedAsset = {
                    asset: asset,
                    location: location
                };

                // Show popup with asset information
                $scope.assetPopupVisible = true;
                $scope.assetPopupData = {
                    IteamName: asset.IteamName,
                    AssetID: asset.AssetID,
                    RFID: asset.RFID,
                    ZoneName: location.ZoneName,
                    FloorName: location.FloorName,
                    RoomName: asset.RoomName || '',
                    tDate: location.tDate
                };

                // Scroll to the floor containing the asset
                $timeout(function () {
                    var floorElement = document.getElementById('floor-map-' + location.mFloorMasterId);
                    if (floorElement) {
                        floorElement.scrollIntoView({ behavior: 'smooth', block: 'center' });
                        // Add highlight effect
                        floorElement.classList.add('floor-highlight');
                        setTimeout(function () {
                            floorElement.classList.remove('floor-highlight');
                        }, 2000);
                    }
                }, 100);

                console.log("Asset located on floor:", location.FloorName, "at coordinates:", location.Xaxis, location.Yaxis);
            } else {
                alert('Asset is not present on the selected floor(s).');
            }
        }, function errorCallback(response) {
            console.log("Error locating asset:", response.data);
            alert('Error locating asset. Please try again.');
        });
    };

    // NEW: Close Asset Popup
    // Hides the asset information popup
    $scope.closeAssetPopup = function () {
        $scope.assetPopupVisible = false;
        $scope.assetPopupData = null;
    };

    // NEW: Clear Search
    // Clears the search query and results
    $scope.clearSearch = function () {
        $scope.searchQuery = '';
        $scope.searchResults = [];
        $scope.showSearchResults = false;
    };
});
