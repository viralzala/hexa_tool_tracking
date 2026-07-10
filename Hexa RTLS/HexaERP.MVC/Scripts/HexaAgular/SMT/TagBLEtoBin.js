// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("TagBLEtoBinCtrl", function ($timeout, $scope, $http, $window) {

    //$(function () {
    //    document.getElementById('CreatedBy').focus();
    //    // $('#tt')[0].focus();
    //    // console.log($('#CreatedBy')[0].focus());
    //    toastr.options = {
    //        positionClass: 'toast-top-center',
    //        timeOut: 10000
    //    };
    //});

    initializeComponets();

    function reset(id) { $(`#${id}`).val(""); };
    function focus(id) {
        document.getElementById(`${id}`).focus();
    };
    //
    function initializeComponets() {
        setInterval(function () {
            ResetEmployee();
        }, 600000);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        setTimeout(function () {
            modal.hide();
            //$('#CreatedBy').focus();
            document.getElementById('CreatedBy').focus();
            toastr.options = {
                positionClass: 'toast-top-center',
                timeOut: 10000
            };
        }, 1000);
    };

    function ResetEmployee() { reset(`CreatedBy`); document.getElementById('CreatedBy').focus(); };

    function parseBool(str) {
        //console.log(str);

        if (typeof str === "undefined") {
            return false;
        }

        if (str) {
            return true;
        }
    }

    $("#CreatedBy").on("keypress", function (event) {
        if (event.which == 13) {
            focus(`Ble`);
        }
    });
    //p.length - 1
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

    $("#Quantity").on("keypress", function (event) {
        if (event.which == 13) {
            var q = $("#Quantity").val();
            var c = q.substr(0, 1);
            if (c == "Q") {
                $("#Quantity").val(q.substr(1, q.length - 1));
                focus(`DateAndTime`);
            } else {
                $("#Quantity").val('');
                toastr.error('Wrong Quantity Scanned');
                return false;
            }
        }
    });


    $("#DateAndTime").on("keypress", function (event) {
        if (event.which == 13) {
            focus(`Lot`);
        }
    });

    $("#DateAndTime").on("keypress", function (event) {
        if (event.which == 13) {
            focus(`Lot`);
        }
    });

    $scope.SubmitSMTBinPack = function () {

        if ((angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null) && (angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null)) {
            toastr.error('Employee ID/Name is required');
            return false;
        }

        if ((angular.isUndefined($scope.Ble) || $scope.Ble === null) && (angular.isUndefined($scope.Ble) || $scope.Ble === null)) {
            toastr.error('Ble is required');
            return false;
        }

        var _form = $("#_formColl");
        var _formData = JSON.stringify(_form.serializeObject(), null, 2);
        //JSON.parse(_formData)
        console.log(JSON.parse(_formData));
        //return false;

        $http({
            method: 'POST',
            url: '../TagBLEtoBin/SubmitSMTProduct',
            data: JSON.parse(_formData),
            contentType: 'application/json; charset=utf-8',
            dataType: "json",
        }).then(function successCallback(response) {
            //console.log(response.data);
            if (response.data.Flag == true) {
                toastr.success(response.data.message);
                //$("#_formColl").reset();
                let c = $("#CreatedBy").val();
                console.log(c);
                $('#_formColl')[0].reset();
                $("#CreatedBy").val(c);
                //reset(`Ble`);
                focus(`Ble`);
                var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a class="uk-text-success" href="#">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-success">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                $("#Niftify").append(t);
            }
            else {
                reset(`Ble`);
                focus(`Ble`);
                toastr.error(response.data.message);
                var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a href="#" class="uk-text-danger">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-danger">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                $("#Niftify").append(t);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $("#Lot").on("keypress", function (event) {

        if (event.which == 13) {

            var l = $("#Lot").val();
            var c = l.substr(0, 1);
            if (c == "L") {
                $("#Lot").val(l.substr(1, l.length - 1));
            } else {
                $("#Lot").val('');
                toastr.error('Wrong Lot Scanned');
                return false;
            }


            if ((angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null) && (angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null)) {
                toastr.error('Employee ID/Name is required');
                return false;
            }

            if ((angular.isUndefined($scope.Ble) || $scope.Ble === null) && (angular.isUndefined($scope.Ble) || $scope.Ble === null)) {
                toastr.error('Ble is required');
                return false;
            }

            var _form = $("#_formColl");
            var _formData = JSON.stringify(_form.serializeObject(), null, 2);
            //JSON.parse(_formData)
            console.log(JSON.parse(_formData));
            //return false;

            $http({
                method: 'POST',
                url: '../TagBLEtoBin/SubmitSMTProduct',
                data: JSON.parse(_formData),
                contentType: 'application/json; charset=utf-8',
                dataType: "json",
            }).then(function successCallback(response) {
                //console.log(response.data);
                if (response.data.Flag == true) {
                    toastr.success(response.data.message);
                    //$("#_formColl").reset();
                    $('#_formColl')[0].reset();

                    //reset(`Ble`);
                    focus(`CreatedBy`);
                    var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a class="uk-text-success" href="#">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-success">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                    $("#Niftify").append(t);
                }
                else {
                    reset(`Ble`);
                    focus(`Ble`);
                    toastr.error(response.data.message);
                    var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a href="#" class="uk-text-danger">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-danger">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                    $("#Niftify").append(t);
                }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });

        }
    });

    $("#Ble").on("keypress", function (event) {


        if (event.which == 13) {

            /* console.log($(`#Ble`).val());*/

            if ((angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null) && (angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null)) {
                toastr.error('Employee ID/Name is required');
                return false;
            }

            if ((angular.isUndefined($scope.Ble) || $scope.Ble === null) && (angular.isUndefined($scope.Ble) || $scope.Ble === null)) {
                toastr.error('Ble is required');
                return false;
            }

            // console.log(parseBool($("input[id='IsMaster']:checked").val()));

            $http({
                method: 'GET',
                url: '../TagBLEtoBin/PushFileTODB',
                params: { Ble: $("#Ble").val(), _CreatedBy: $("#CreatedBy").val(), IsMaster: parseBool($("input[id='IsMaster']:checked").val()) }
            }).then(function successCallback(response) {
                //console.log(response.data);
                if (response.data.Flag == true) {
                    toastr.success(response.data.message);
                    reset(`Ble`);
                    focus(`Ble`);
                    var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a class="uk-text-success" href="#">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-success">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                    $("#Niftify").append(t);
                }
                else {
                    reset(`Ble`);
                    focus(`Ble`);
                    toastr.error(response.data.message);
                    var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a href="#" class="uk-text-danger">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-danger">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                    $("#Niftify").append(t);
                }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    });

    $scope.ReadFileLocation = function () {
        //  console.log('TagBLEtoBin');
        $http({
            method: 'GET',
            url: '../TagBLEtoBin/ReadFileFromLocation'
        }).then(function successCallback(response) {
            // console.log(response.data);
            if (response.data.Flag == true) {
                SetForData(response.data.items[0]);
                $scope.FileItems = response.data.items;
                //focus('Ble');
                toastr.success(response.data.message);
            }
            else {
                toastr.error(response.data.message);
            }

        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function SetForData(items) {
        //console.log(items);
        $("#SerialNumber").val(items.SerialNumber);
        $("#Quantity").val(items.Quantity);
        if (items.DateAndTime !== null) {
            //console.log(items.DateAndTime);
            $("#DateAndTime").val(ConvertJsonDatetoanyformat(items.DateAndTime, 'dd-mm-yyyy'));
        }
        // $("#DateTime").val(items.DateAndTime);
        $("#CustomerCode").val(items.CustomerCode);
        $("#PartNumber").val(items.PartNumber);
        $("#QADLine").val(items.QADLine);
        $("#Station").val(items.Station);
        $("#Lot").val(items.Lot);
        $("#StatusId").val(items.Status);
        $("#Id").val(items.Id);
        $("#PalletId").val(items.PalletId);
        $("#PartId").val(items.PartId);
        $("#ContainerId").val(items.ContainerId);
        $("#CustomerId").val(items.CustomerId);
        $("#ShiftId").val(items.ShiftId);
    };

    $scope.SubmitSMTProduct = function () {

        if ((angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null) && (angular.isUndefined($scope.CreatedBy) || $scope.CreatedBy === null)) {
            toastr.error('Employee ID/Name is required');
            return false;
        }

        if ((angular.isUndefined($scope.Ble) || $scope.Ble === null) && (angular.isUndefined($scope.Ble) || $scope.Ble === null)) {
            toastr.error('Ble is required');
            return false;
        }

        var _form = $("#_formColl");
        var _formData = JSON.stringify(_form.serializeObject(), null, 2);
        //JSON.parse(_formData)
        console.log(JSON.parse(_formData));
        //return false;
        $http({
            method: 'POST',
            url: '../TagBLEtoBin/SubmitSMTProduct',
            data: JSON.parse(_formData),
            contentType: 'application/json; charset=utf-8',
            dataType: "json",
        }).then(function successCallback(response) {
            //console.log(response.data);
            if (response.data.Flag == true) {
                toastr.success(response.data.message);

                $("#_formColl").reset();
                reset(`Ble`);
                focus(`Ble`);
                var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a class="uk-text-success" href="#">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-success">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                $("#Niftify").append(t);
            }
            else {
                reset(`Ble`);
                focus(`Ble`);
                toastr.error(response.data.message);
                var t = `<li>\
        <div class="md-list-content">\
        <span class="md-list-heading"><a href="#" class="uk-text-danger">${response.data.message}</a></span>\
        <div class="uk-margin-small-top">\
        <span class="uk-margin-right">\
        <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small uk-text-danger">${response.data.t}</span>\
        </span>\
        </div>\
        </div>\
        </li>`;
                $("#Niftify").append(t);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.PushFileTODB = function () {
        //console.log('PushFileTODB');
        $http({
            method: 'GET',
            url: '../TagBLEtoBin/PushFileTODB',
            params: { Ble: $("#Ble").val() }
        }).then(function successCallback(response) {
            //console.log(response.data);
            if (response.data.Flag == true) {
                toastr.success(response.data.message);
                $("#Ble").val("");
                $('#FileItems').find("tr:gt(0)").remove();
            }
            else {
                toastr.error(response.data.message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };


    function ConvertJsonDatetoanyformat(jsondate, format) {
        var yourdate = '';
        var dateAsFromServerSide = jsondate ///Date(1291374337981)/
        //Now let's convert it to js format
        //Example: Fri Dec 03 2010 16:37:32 GMT+0530 (India Standard Time)
        var parsedDate = new Date(parseInt(dateAsFromServerSide.substr(6)));

        var jsDate = new Date(parsedDate); //Date object

        //Play with jsDate properties getDate(), getDay() etc

        var fulldate = dateAsFromServerSide;
        var ParsedDate = parsedDate;
        var GetDay = jsDate.getDay();
        var GetDate = jsDate.getDate();
        var GetFullYear = jsDate.getFullYear();
        var GetHours = jsDate.getHours();
        var GetMilliseconds = jsDate.getMilliseconds();
        var GetMinutes = jsDate.getMinutes();
        var GetMonth = jsDate.getMonth() + 1;
        var GetSeconds = jsDate.getSeconds();
        var GetTime = jsDate.getTime();
        var GetTimezoneOffset = jsDate.getTimezoneOffset();
        var GetUTCDate = jsDate.getUTCDate();
        var GetUTCDay = jsDate.getUTCDay();
        var GetUTCFullYear = jsDate.getUTCFullYear();
        var GetUTCHours = jsDate.getUTCHours();
        var GetUTCMilliseconds = jsDate.getUTCMilliseconds();
        var GetUTCMinutes = jsDate.getUTCMinutes();
        var GetUTCMonth = jsDate.getUTCMonth();
        var GetUTCSeconds = jsDate.getUTCSeconds();
        var GetYear = jsDate.getYear();

        if (format == 'mm/dd/yyyy') {
            yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear;
        }
        else if (format == 'dd/mm/yyyy') {
            yourdate = GetDate + '/' + GetMonth + '/' + GetFullYear;
        }
        else if (format == 'dd-mm-yyyy') {
            var _date = (jsDate.getFullYear() + "-" + zeroPadded(jsDate.getMonth() + 1) + "-" + zeroPadded(jsDate.getDate()));
            //console.log(_date);
            //yourdate = GetDate + '-' + GetMonth + '-' + GetFullYear;
            yourdate = _date;
        }
        else if (format == 'mm/dd/yyyy hh:mm:ss') {
            yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear + " " + GetHours + ":" + GetMinutes + ":" + GetSeconds;
        }
        else if (format == 'hh:mm:ss') {
            yourdate = GetHours + ":" + GetMinutes + ":" + GetSeconds;
        }
        else if (format == 'hh:mm 24hour') {
            yourdate = GetHours + ":" + GetMinutes;
        }
        else if (format == 'hh:mm ampm') {
            yourdate = formatAMPM(jsDate)
        }
        else if (format == 'mm/dd/yyyy hh:mm ampm') {
            var timeampm = formatAMPM(jsDate);
            yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear + ' ' + timeampm;
        }
        return yourdate;
    };

    function zeroPadded(val) {
        if (val >= 10)
            return val;
        else
            return '0' + val;
    }

    function formatAMPM(date) {
        var hours = date.getHours();
        var minutes = date.getMinutes();
        var ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12; // the hour '0' should be '12'
        minutes = minutes < 10 ? '0' + minutes : minutes;
        var strTime = hours + ':' + minutes + ' ' + ampm;
        return strTime;
    };
});