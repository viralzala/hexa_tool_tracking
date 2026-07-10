// ** Mudassar I **
//

//var app = angular.module("HexaToolsTrackReportApp", []);

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("BarChartReportCtrl", function ($timeout, $scope, $http) {
    initializeComponets();
    //

    $scope.GetEmployess = function () {
        var fDate = $("#kUI_datetimepicker_range_start").val();
        var tDate = $("#kUI_datetimepicker_range_end").val();
        //console.log(fDate); console.log(tDate);
        $http({
            method: 'GET',
            url: '../BarChartReport/GerEmployess',
            params: { fDate: fDate, toDate: tDate }
        }).then(function successCallback(response) {
            //console.log(response.data);
            $scope.EmpColl = response.data.data;

            setTimeout(function () {
                modal.hide()
            }, 1000)
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.SaveFormCollData = function () {
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        //console.log(_eData);

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment we are preparing analysis report for you<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        $http({
            method: 'POST',
            url: '../BarChartReport/GetCount',
            data: _eData
        }).then(function successCallback(response) {
           // console.log(JSON.parse(response.data.data));
            //var jsonObj = $.JSON.stringify(response.data.CompartWise);          
            //console.log(jsonObj);
            responseLog(response.data);
            setTimeout(function () {
                modal.hide()
            }, 1000)
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };


    //
    function initializeComponets() {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Please Wait Form is preparing...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');

        setTimeout(function () {
            modal.hide()
        }, 1000)

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

function responseLog(data) {

    compWiseFun(data.CompData, data.CompHeader);
    compAgenFun(data.AgenData, data.AgenHeader);   
    WorkCatWiseFun(data.WorkCatData, data.WorkCatHeader);
    DesigWiseFun(data.DesCatData, data.DesCatHeader);

};


function TestFun(processedjson, HeaderArray) {
    //console.log(HeaderArray);
    //console.log(JSON.parse(processedjson));
    Highcharts.chart('TestBarchart', {
        chart: {
            type: 'column'
        },
        title: {
            text: 'Stacked column chart'
        },
        xAxis: {
            categories: HeaderArray
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Total fruit consumption'
            },
            stackLabels: {
                enabled: true,
                style: {
                    fontWeight: 'bold',
                    color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                }
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            align: 'right',
            x: -30,
            verticalAlign: 'top',
            y: 25,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
            borderColor: '#CCC',
            borderWidth: 1,
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white'
                }
            }
        },
        series: JSON.parse(processedjson)
    });
};

function compWiseFun(processedjson, HeaderArray) {

    Highcharts.chart('compWise', {

        chart: {
            type: 'column'
        },
        title: {
            text: 'Compartment Attendance'
        },
        xAxis: {
            categories: HeaderArray
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Compartment Attendance in Days'
            },
            stackLabels: {
                enabled: true,
                style: {
                    fontWeight: 'bold',
                    color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                }
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            align: 'right',
            x: -30,
            verticalAlign: 'top',
            y: 25,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
            borderColor: '#CCC',
            borderWidth: 1,
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white'
                }
            }
        },
        series: processedjson
    });
};

function compAgenFun(processedjson, HeaderArray) {
    Highcharts.chart('AgenWise', {
        chart: {
            type: 'column'
        },
        title: {
            text: 'Agency Attendance'
        },
        xAxis: {
            categories: HeaderArray
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Agency Attendance in Days'
            },
            stackLabels: {
                enabled: true,
                style: {
                    fontWeight: 'bold',
                    color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                }
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            align: 'right',
            x: -30,
            verticalAlign: 'top',
            y: 25,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
            borderColor: '#CCC',
            borderWidth: 1,
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white'
                }
            }
        },
        series: processedjson
    });

   
};



function WorkCatWiseFun(processedjson, HeaderArray) {
    Highcharts.chart('WorkCatWise', {
        chart: {
            type: 'column'
        },
        title: {
            text: 'Work Category Attendance'
        },
        xAxis: {
            categories: HeaderArray
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Work Category Attendance in Days'
            },
            stackLabels: {
                enabled: true,
                style: {
                    fontWeight: 'bold',
                    color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                }
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            align: 'right',
            x: -30,
            verticalAlign: 'top',
            y: 25,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
            borderColor: '#CCC',
            borderWidth: 1,
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white'
                }
            }
        },
        series: processedjson
    });
};
function DesigWiseFun(processedjson, HeaderArray) {
    Highcharts.chart('DesigWise', {
        chart: {
            type: 'column'
        },
        title: {
            text: 'Designation Attendance'
        },
        xAxis: {
            categories: HeaderArray
        },
        yAxis: {
            min: 0,
            title: {
                text: 'Designation Attendance in Days'
            },
            stackLabels: {
                enabled: true,
                style: {
                    fontWeight: 'bold',
                    color: (Highcharts.theme && Highcharts.theme.textColor) || 'gray'
                }
            }
        },
        credits: {
            enabled: false
        },
        legend: {
            align: 'right',
            x: -30,
            verticalAlign: 'top',
            y: 25,
            floating: true,
            backgroundColor: (Highcharts.theme && Highcharts.theme.background2) || 'white',
            borderColor: '#CCC',
            borderWidth: 1,
            shadow: false
        },
        tooltip: {
            headerFormat: '<b>{point.x}</b><br/>',
            pointFormat: '{series.name}: {point.y}<br/>Total: {point.stackTotal}'
        },
        plotOptions: {
            column: {
                stacking: 'normal',
                dataLabels: {
                    enabled: true,
                    color: (Highcharts.theme && Highcharts.theme.dataLabelsColor) || 'white'
                }
            }
        },
        series: processedjson
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