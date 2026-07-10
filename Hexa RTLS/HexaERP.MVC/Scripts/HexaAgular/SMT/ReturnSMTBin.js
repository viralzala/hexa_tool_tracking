// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("ReturnSMTBinCtrl", function ($timeout, $scope, $http, $window) {

    var AssemblyData = [{ "key": 1, "Remark": "Line -01" }, { "key": 3, "Remark": "Line -02" }, { "key": 2, "Remark": "Line -03" }];

    $(function () {
        document.getElementById('ModifiedBy').focus();
        toastr.options = {
            positionClass: 'toast-top-center',
            timeOut: 10000
        };
    });

    $scope.empIds = [];
    $scope.SubmitTakeAwayList = function () {

        // Use .map() to iterate through each row and get values
        //var values = $('#myTable tbody tr').map(function () {
        //    var name = $(this).find('.name').val();
        //    var email = $(this).find('.email').val();
        //    // Add more variables for other columns as needed
        //    return {
        //        name: name,
        //        email: email
        //        // Add more properties for other columns as needed
        //    };
        //}).get();

        //console.log(values);

        var dataToSend = [];

        $('#example1 tbody').find('tr').each(function () {
            var checkbox = $(this).find('.chkTakeAway-ichecked-id');
            var numberControl = $(this).find('.number-control');
            if (checkbox.prop('checked')) {
                var numberValue = parseFloat(numberControl.val()) || 0;
                var rowData = {
                    RowNumber: parseFloat(checkbox.val()) || 0, // assuming the first column is the row number
                    NumberControlValue: numberValue
                };
                dataToSend.push(rowData);
            }
        });

        // console.log(dataToSend);
        // return false;


        //var lots = $(".chkTakeAway-ichecked-id:checked").map(function () {
        //    // console.log(this.value);
        //    // var t = $(this).closest('tr').find('td:first').text();
        //    var numberControlValue = $(this).find('.number-control');
        //    return parseFloat(numberControlValue);

        //    //return this.value;
        //}).get();

        //console.log(lots);
        //return false;


        if (dataToSend.length == 0) {
            //alert('Select Lot');
            toastr.error('Select Lot');
            return false
        }

        if ((angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null) && (angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null)) {
            toastr.error('Employee Id is required');
            return false;
        }

        if ((angular.isUndefined($scope.Comment) || $scope.Comment === null) && (angular.isUndefined($scope.Comment) || $scope.Comment === null)) {
            toastr.error('Enter Remark');
            return false;
        }

        var data = new FormData();
        data.append("data", JSON.stringify(dataToSend));
        data.append("ModifiedBy", $scope.ModifiedBy);
        data.append("Remark", $("#Remark").data("kendoDropDownList").text());
        data.append("Comment", $scope.Comment);

        console.log(...data);
        //return false;
        $http({
            method: 'POST',
            url: '../ReturnSMTBin/ReturnSMTBinRequest',
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

        $('#Remark').kendoDropDownList({
            dataTextField: "Remark",
            dataValueField: "key",
            filter: "contains",
            dataSource: AssemblyData,
            suggest: true,
            index: 3
        });

        var Remark = $("#Remark").data("kendoDropDownList");
        Remark.value(1);
    };

    function reset(id) { $(`#${id}`).val(""); };
    function focus(id) {
        document.getElementById(`${id}`).focus();
    };

    $("#ModifiedBy").on("keypress", function (event) {
        if (event.which == 13) {
            focus('PartNumber');
        }
    });
    $("#PartNumber").on("keypress", function (event) {
        if (event.which == 13) {
            //console.log($(`#mZoneId`).val());
            $http({
                method: 'GET',
                url: '../ReturnSMTBin/GetReturnSMTBin',
                params: { Search: $(`#PartNumber`).val() }
            }).then(function successCallback(response) {
                console.log(response.data);
                if (response.data.Flag == true) {
                    $scope.SmtProducts = response.data._product;
                    toastr.success(`${response.data.message}`);
                } else {
                    toastr.error(response.data.message);
                    reset('PartNumber');
                    focus('PartNumber');
                }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    });
});

