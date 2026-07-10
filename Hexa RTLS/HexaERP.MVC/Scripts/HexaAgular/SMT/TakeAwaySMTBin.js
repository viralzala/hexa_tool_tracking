// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("TakeAwaySMTBinCtrl", function ($timeout, $scope, $http, $window) {

    $(function () {
        document.getElementById('ModifiedBy').focus();
        toastr.options = {
            positionClass: 'toast-top-center',
            timeOut: 10000
        };
    });

    $scope.empIds = [];

    $scope.SubmitTakeAwayList = function () {

    };

    initializeComponets();
    //
    function initializeComponets() {
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
            var p = $("#PartNumber").val();
            var c = p.substr(0, 1);
            if (c == "P") {
                $("#PartNumber").val(p.substr(1, p.length - 1));
                focus(`Quantity`);
            } else {
                $("#PartNumber").val('');
                toastr.error('Wrong Part Number');
                return false;
            }
        };
    });

    localStorage.setItem('Quantity', 0);

    $("#Ble").on("keypress", function (event) {

        if (event.which == 13) {
            //event.preventDefault();
            //console.log($(`#Ble`).val());
            var t = $(`#Ble`).val();
            var m = $(`#ModifiedBy`).val();
            //var q = $(`#Quantity`).val();

            var f = $("#example1").find(".nr:first").text();
            // console.log(f);


            //var f;
            //$('#example1 > tbody  > tr').each(function () {
            //    f = $(`#${t}`).text();
            //});

            //console.log(f);
            //console.log(t);
            //return false;

            if (t == f) {
                //toastr.success(`${f} ${t} not matching FIFO basis.`);

                var data = new FormData();
                data.append("Ble", t);
                data.append("ModifiedBy", m);
                data.append("SMTProductIds", f);
                //data.append("Quantity", q);
                //console.log(...data);
                $http({
                    method: 'POST',
                    url: '../TakeAwaySMTBin/SubmitBinOut',
                    TransformStream: angular.identity,
                    headers: { 'Content-Type': undefined },
                    data: data
                }).then(function successCallback(response) {
                    // console.log(response.data);
                    if (response.data.Flag == true) {
                        toastr.success(response.data.Message);
                        GetBinDetailsReset(parseInt(response.data.Less));
                        reset('Ble');
                        focus('Ble');
                    }
                    else {
                        toastr.error(response.data.Message);
                    }
                }, function errorCallback(response) {
                    console.log("Error : " + response.data.ExceptionMessage);
                });

                // return false;
            } else {
                toastr.error(`${f} ${t} not matching FIFO basis.`);
                return false;
            }
        }
    });

    $("#ModifiedBy1").on("keypress", function (event) {
        if (event.which == 13) {
            focus('Ble');

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

            if ((angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null) && (angular.isUndefined($scope.ModifiedBy) || $scope.ModifiedBy === null)) {
                toastr.error('Employee Id is required');
                return false;
            }

            var data = new FormData();
            data.append("SMTProductIds", lots);
            data.append("ModifiedBy", $scope.ModifiedBy);

            $http({
                method: 'POST',
                url: '../TakeAwaySMTBin/SubmitTakeAway',
                TransformStream: angular.identity,
                headers: { 'Content-Type': undefined },
                data: data
            }).then(function successCallback(response) {
                console.log(response.data);
                if (response.data.Flag == true) {
                    toastr.success(response.data.Message);
                    $('#resetForm')[0].reset();
                    reset('PartNumber');
                    focus('PartNumber');
                    reset('Quantity');
                    GetBinDetailsReset();
                    //$('#example1').find("tr:gt(0)").remove();
                }
                else {
                    toastr.error(response.data.Message);
                }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    });

    function GetBinOutEvent() {
        //console.log($(`#Quantity`).val());

        localStorage.clear();

        localStorage.setItem('Quantity', $(`#Quantity`).val());

        $scope._avialbeQuantity = 0;

        $http({
            method: 'GET',
            url: '../TakeAwaySMTBin/GetTakeAwaySMTBin',
            params: { PartNumber: $scope.PartNumber, Quantity: parseInt($(`#Quantity`).val()) }
        }).then(function successCallback(response) {
            console.log(response.data);
            if (response.data.Flag == true) {                
                $scope.GetAwayList = response.data._TakewayList;
               // toastr.success(`${response.data.message}`);
                focus('Ble');
                $scope._avialbeQuantity = response.data._avialbeQuantity;
            } else {
                toastr.error(response.data.message);
                reset('Quantity');
                reset('PartNumber');
                focus('PartNumber');
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.GetTakeAwayList = function () {
        GetBinOutEvent();
    };

    function GetBinDetailsReset(less) {

        var t = (parseInt(localStorage.getItem('Quantity')) - less);
        localStorage.setItem('Quantity', t);
        console.log(t);

        $http({
            method: 'GET',
            url: '../TakeAwaySMTBin/GetTakeAwaySMTBin',
            params: { PartNumber: $scope.PartNumber, Quantity: t }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                $scope.GetAwayList = response.data._TakewayList;
            } else {
                $scope.GetAwayList = response.data._TakewayList;
                reset('Quantity');
                reset('PartNumber');
                reset('Ble');
                focus('PartNumber');
            }

        }, function errorCallback(response) {
            $scope.GetAwayList = response.data._TakewayList;
            reset('Quantity');
            reset('PartNumber');
            reset('Ble');
            focus('PartNumber');
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $("#Quantity").on("keypress", function (event) {
        if (event.which == 13) {
            GetBinOutEvent();
        }
    });
});

