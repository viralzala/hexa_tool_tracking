// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("RFIDMonotoringCtrl", function ($scope, $http, $timeout) {
    initializeComponets();
    $scope.CurrentDate = new Date();    
    //
    function initializeComponets() {
        //SetControll();   
        var myVar;      
        GetInvData();
    }

    //
    $scope.GetSpeedList = function () {

        var myRadioStartPort = $('input[name=StartPort]');
        var StartPort = myRadioStartPort.filter(':checked').val();
        var myRadioEndPort = $('input[name=EndPort]');
        var EndPort = myRadioEndPort.filter(':checked').val();
        $http({
            method: 'GET',
            url: '../SpeedCalculator/getGetToTrackData',
            params: { StartPort: StartPort, EndPort: EndPort }
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available   
            $scope.TrackedList = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //StartReader
    $scope.StartReader = function () {

        if ($scope.ReaderIp == "" || $scope.ReaderIp == null) {
            UIkit.modal.alert('Please Select IP Address');
            return false;
        } else {
            UIkit.modal.confirm('Are you sure to Start Reader?', function () {
                modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
                $scope.lblMsg = "";

                $http({
                    method: 'GET',
                    url: '../RFIDMonitoring/ReaderInit',
                    params: {
                        Reader: $scope.ReaderIp
                    }
                }).then(function successCallback(response) {
                    // this callback will be called asynchronously
                    // when the response is available                     
                    $scope.lblMsg = response.data;
                   // console.log(response.data);

                    if (response.data.Flag == true) {
                        setTimeout(function () {
                            modal.hide()
                        }, 1000)

                        myVar = setInterval(function () {
                            $scope.$apply(ToolsTrackFun());
                        }, 5000);
                    }
                    else {
                        setTimeout(function () {
                            modal.hide()
                        }, 1000)
                    }
                   
                }, function errorCallback(response) {
                    // called asynchronously if an error occurs
                    // or server returns response with an error status.
                    console.log("Error : " + response.data.ExceptionMessage);
                });

            });
        }


        //
        function ToolsTrackFun() {

            var d = new Date();
            $scope.lastTracked = d.toLocaleTimeString();
            console.log($scope.lastTracked);
            $scope.lblMsg = null;
            $http({
                method: 'GET',
                url: '../RFIDMonitoring/getMonitorData'
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                    
                //console.log(response.data);
                $scope.ScanedInv = response.data.TodayInv;
                $scope.MonitorData = response.data.MonitorInv;
            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    };

    //StopReader
    $scope.StopReader = function () {
        $http({
            method: 'GET',
            url: '../RFIDMonitoring/StopReaders'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available 
            $scope.lblMsg = response.data;
            clearInterval(myVar);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
    
    //ClearData
    $scope.ClearData = function () {
        $scope.lblMsg = "";
        $("#lblerr").text(""); $("#RFID").val(""); $("#PORTID").val(""); $("#global_filter").val("");
        $http({
            method: 'GET',
            url: '../RFIDMonitoring/ReaderClear'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available            
            $scope.lblMsg = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

     

    function GetInvData() {
        $http({
            method: 'GET',
            url: '../RFIDMonitoring/getInvDatas'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available                    
            //console.log(response.data.InvData);
            $scope.InvTotal = response.data.InvData;
            $("#ReaderIp").kendoAutoComplete({
                dataSource: response.data.ReaderData,
                dataTextField: "ReaderIP",
                dataValueField: "ReaderIP",
                noDataTemplate: 'No Data!'
            });

        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
});