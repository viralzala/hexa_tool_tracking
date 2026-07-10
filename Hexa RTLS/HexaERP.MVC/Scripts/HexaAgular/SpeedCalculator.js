// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("SpeedCalculatorCtrl", function ($scope, GetRegService, $http, $timeout) {
    initializeComponets();

    $(function () {

    });
    //
    $scope.oK = function () {

    };
    //
    function initializeComponets() {
        //SetControll();   
        var myVar;
        GetReaders();
    }
    //
    function SetControll() {

    }
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
                    url: '../SpeedCalculator/ReaderInit',
                    params: {
                        Reader: $scope.ReaderIp
                    }
                }).then(function successCallback(response) {
                    // this callback will be called asynchronously
                    // when the response is available                     
                    $scope.lblMsg = response.data;
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)

                    myVar = setInterval(function () {
                        $scope.$apply(ToolsTrackFun());
                    }, 5000);

                }, function errorCallback(response) {
                    // called asynchronously if an error occurs
                    // or server returns response with an error status.
                    console.log("Error : " + response.data.ExceptionMessage);
                });

            });
        }

       
        //
        function ToolsTrackFun() {
            console.log(Date.now);
            console.log($scope.StartPort); console.log($scope.EndPort);
            GetTracklist();
        }       
    };
    //StopReader
    $scope.StopReader = function () {
        $http({
            method: 'GET',
            url: '../SpeedCalculator/StopReaders'
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
            url: '../SpeedCalculator/ReaderClear'
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

    //
    function GetTracklist() {
       

        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
    }
    
    function GetReaders() {
        GetRegService._getDatas().then(function (result) {
            $("#ReaderIp").kendoAutoComplete({
                dataSource: result.data,
                dataTextField: "ReaderIP",
                dataValueField: "ReaderIP",
                noDataTemplate: 'No Data!'
            });            
        }, function (error) {
            console.log(error);
        });
    }    
});
app.factory('GetRegService', ['$http', function ($http) {
    var GetRegService = {};

    GetRegService._getDatas = function () {
        return $http.get('../SpeedCalculator/getGetReadersData');
    };

    return GetRegService;
}]);
