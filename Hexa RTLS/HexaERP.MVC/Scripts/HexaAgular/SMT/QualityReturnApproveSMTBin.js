// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("QualityReturnApproveSMTBinCtrl", function ($timeout, $scope, $http, $window) {


    $scope.empIds = [];
    $scope.SubmitQualityApprovalRequest = function () {

        var lots = $(".chkTakeAway-ichecked-id:checked").map(function () {
            console.log(this.value);
            return this.value;
        }).get();
        console.log(lots);

        if (lots.length == 0) {
            //alert('Select Lot');
            toastr.error('Select Lot');
            return false
        }

        //if ((angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null) && (angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null)) {
        //    toastr.error('Employee Id is required');
        //    return false;
        //}

        if ((angular.isUndefined($scope.Remark) || $scope.Remark === null) && (angular.isUndefined($scope.Remark) || $scope.Remark === null)) {
            toastr.error('Enter Remark');
            return false;
        }

        var data = new FormData();
        data.append("SMTProductIds", lots);
        data.append("Remark", $scope.Remark);

        console.log(...data);
        //return false;
        $http({
            method: 'POST',
            url: '../QualityReturnApproveSMTBin/QualityReturnSMTBinRequest',
            TransformStream: angular.identity,
            headers: { 'Content-Type': undefined },
            data: data
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                toastr.success(response.data.Message);
                reset('PartNumber');
                focus('PartNumber');
                reset('Remark');
                //$('#example1').find("tr:gt(0)").remove();
            }
            else {
                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };

    initializeComponets();
    //
    function initializeComponets() {

        $http({
            method: 'GET',
            url: '../QualityReturnApproveSMTBin/GetQualityReturnApproveSMTBin'
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {
                $scope.SmtProducts = response.data._product;
                //toastr.success(`${response.data.message}`);
            } else {
                toastr.error(response.data.message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };

    function reset(id) { $(`#${id}`).val(""); };
    function focus(id) {
        document.getElementById(`${id}`).focus();
    };
});

