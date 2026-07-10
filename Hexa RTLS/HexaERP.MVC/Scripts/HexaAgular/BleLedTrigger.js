// ** **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("BleLedTriggerCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();
    function reset(id) { $(`#${id}`).val(""); };
    function focus(id) {
        document.getElementById(`${id}`).focus();
    };

    //
    function initializeComponets() {
        // focus('PartNumber');
    };
    $(function () {
        document.getElementById('PartNumber').focus();
        toastr.options = {
            positionClass: 'toast-top-center',
            timeOut: 10000
        };
    });

    $("#PartNumber").on("keypress", function (event) {
        if (event.which == 13) {
            //console.log($(`#mZoneId`).val());
            $http({
                method: 'GET',
                url: '../BleLedTrigger/SearchProductByBle',
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