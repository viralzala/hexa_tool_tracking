// ** Mudassar I **
//

//var app = angular.module("HexaToolsTrackReportApp", []);

// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ToolsTrackReportCtrl", function (Excel, $timeout, $scope, GetRegService, $http) {
    initializeComponets();

    $(function () {

    });
    //
    $scope.oK = function () {
        $scope.bindfromDate = $("#uk_dp_start").val(); $scope.bindtoDate = $("#uk_dp_end").val();
        getDataList();
    };

    //
    $scope.oKs = function () {
        var toTooltagId = $("#toTooltagId").val();
        var bindtoDate = $("#kUI_datepicker_a").val();
       // console.log(toTooltagId); console.log(bindtoDate);
        getEmpTrackList(toTooltagId, bindtoDate);
    };
    //
    function initializeComponets() {
        getTools();
    }
    $scope.export = function () {
        //html2canvas(document.getElementById('tblReport'), {
        //    onrendered: function (canvas) {
        //        var data = canvas.toDataURL();
        //        var docDefinition = {
        //            content: [{
        //                image: data,
        //                width: 500,
        //            }]
        //        };
        //        pdfMake.createPdf(docDefinition).download("test.pdf");
        //    }
        //});

        $http({
            method: 'GET',
            url: '/ToolsTrackReport/pusher'          
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
           // console.log(response.data);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    //
    function getEmpTrackList(_id,_Tdate) {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $scope.onDates = _Tdate;
        GetRegService._getEmpTrackDatas(_id, _Tdate).then(function (result) {
            console.log(result.data);
            $scope.EmpTrackList = result.data;
            setTimeout(function () {
                modal.hide()
            }, 2000)
        }, function (error) {
            console.log('Error' + error);
        });
    }
    //
    function getDataList() {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        GetRegService._getDatas().then(function (result) {          
            $scope.DataLists = result.data;
            setTimeout(function () {
                modal.hide()
            }, 2000)
        }, function (error) {
            console.log('Error' +error);
        });
    }
    //
    function getTools() {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        GetRegService._getTools().then(function (result) {          
            $('#toTooltagId').kendoComboBox({
                dataTextField: "ToolName",
                dataValueField: "toTooltagId",               
                dataSource: result.data,
                filter: "contains",
                suggest: true,
                index: 3
            });
            setTimeout(function () {
                modal.hide()
            }, 2000)
        }, function (error) {
            console.log('Error' + error);
        });
    }
    //
    $scope.exportToExcel = function (tableId) { // ex: '#my-table'

        var exportHref = Excel.tableToExcel(tableId, 'HexaTrackDataExport');
        $timeout(function () { location.href = exportHref; }, 100); // trigger download
    }
    //
   
});
app.factory('GetRegService', ['$http', function ($http) {
    var GetRegService = {};

    GetRegService._getDatas = function () {
        return $http.get('/ToolsTrackReport/getToolsReport');
    };

    GetRegService._getTools = function () {
        return $http.get('/ToolsTrackReport/getTools');
    };  

    GetRegService._getEmpTrackDatas = function (__id, _Tdate) {
        var response = $http({
            method: "GET",
            url: "/ToolsTrackReport/getEmplTrackList",
            params: {
                toTooltagId: __id, tDate: _Tdate
            }
        });
        return response;
    };

    return GetRegService;
}]);
app.factory('Excel', function ($window) {
    var uri = 'data:application/vnd.ms-excel;base64,',
        template = '<html xmlns:o="urn:schemas-microsoft-com:office:office" xmlns:x="urn:schemas-microsoft-com:office:excel" xmlns="http://www.w3.org/TR/REC-html40"><head><!--[if gte mso 9]><xml><x:ExcelWorkbook><x:ExcelWorksheets><x:ExcelWorksheet><x:Name>{worksheet}</x:Name><x:WorksheetOptions><x:DisplayGridlines/></x:WorksheetOptions></x:ExcelWorksheet></x:ExcelWorksheets></x:ExcelWorkbook></xml><![endif]--></head><body><table>{table}</table></body></html>',
        base64 = function (s) { return $window.btoa(unescape(encodeURIComponent(s))); },
        format = function (s, c) { return s.replace(/{(\w+)}/g, function (m, p) { return c[p]; }) };
    return {
        tableToExcel: function (tableId, worksheetName) {
            var table = $(tableId),
                ctx = { worksheet: worksheetName, table: table.html() },
                href = uri + base64(format(template, ctx));           
            return href;
        }
    };
});

