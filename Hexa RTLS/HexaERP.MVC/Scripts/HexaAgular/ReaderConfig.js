// ** Mudassar I **
//

var ReaderConfigApp = angular.module("HexaReaderConfigApp", []);

// ** Mudassar I **


ReaderConfigApp.controller("ReaderConfigCtrl", function ($scope, GetRegService) {
    initializeComponets();

    $(function () {

    });
    //
    $scope.oK = function () {

    };
    //
    function initializeComponets() {       
        GetOrganization();
    }
    //
    function GetOrganization() {
        GetRegService._getDatas().then(function (result) {
            $('#OrgInfoId').kendoComboBox({
                dataTextField: "OrgInfoName",
                dataValueField: "OrgInfoId",
                filter: "contains",
                dataSource: data,
                select: onSelect,
                suggest: true,
                index: 3
            });
            function onSelect(e) {
                if (e.item) {
                    var dataItem = this.dataItem(e.item.index());
                    getDataList(dataItem.OrgInfoId);
                }
            };
        }, function (error) {
            console.log(error);
        });
    }
    function getDataList(Id) {

        $http({
            method: 'GET',
            url: '../ReaderConfig/getData',
            params: {
                _orgId: Id
            }
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
});
ReaderConfigApp.factory('GetRegService', ['$http', function ($http) {
    var GetRegService = {};

    GetRegService._getDatas = function () {
        return $http.get('../ReaderConfig/GetOrganizations');
    };

    return GetRegService;
}]);
