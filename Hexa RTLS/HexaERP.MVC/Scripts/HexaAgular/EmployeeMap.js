// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("EmployeeMapCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    //
    function initializeComponets() {
        //$scope.isEdit = true;     
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();

        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var myVar;
        InitDataBind();
        setTimeout(function () {
            modal.hide()
        }, 1000);
        SetControll();
        myVar = setInterval(function () {
            $scope.$apply(SetControll());
        }, 5000);
    }

    $scope.setInformation = function (iData, Locat) {
        //console.log(iData);
        $scope._EmpId = iData.EmployeeId;
        $scope._Name = iData.Name;
        $scope._RFID = iData.Epc;
        $scope._Agency = iData.Agency;
        $scope._Designation = iData.Designation;
        $scope._SkillCategory = iData.SkillCategory;
        $scope._WorkCategory = iData.WorkCategory;
        $scope._Activity = iData.Activity;
        $scope._trackWork = Locat;
        $scope._tDate = iData.tDate;
    };

    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../EmployeeMap/getlocationdata'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available  
            //console.log(response.data);
            $scope.Location = response.data.IZoneData;
            $scope.Areas = response.data.IsubZoneData;
            $scope.PortColl = response.data.IPortsData;
            //console.log(response.data.IsubZoneData);
            //console.log(response.data.IZoneData);
            //console.log(response.data.IPortsData);

            //console.log($scope.PortColl);
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    function SetControll() {
        //console.log('Called:HexaTracker');
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
        $http({
            method: 'GET',
            url: '../EmployeeMap/getGetToTrackData'
        }).then(function successCallback(response) {           
            $scope.FillterIData = response.data;
            //console.log(response.data);
            responseLog(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});

function BindArea() {

    console.log('BindArea');
    var heightArray = $(".post-container").map(function () {
        return $(this).height();
    }).get();

    var maxHeight = Math.max.apply(Math, heightArray);
    $(".room").height(maxHeight);
    $(".room").height(maxHeight);

    var ticket = "<div class='ticket'><i class='glyphicon glyphicon-map-marker'></i></div>";
    var ticket = "<i class='glyphicon glyphicon-map-marker ticket'></i>";
    var numTickets = 10;
    for (var x = 1; x <= numTickets; x++) {
        $(ticket).appendTo("#room1");
    }
    for (var x = 1; x <= 20; x++) {
        $(ticket).appendTo("#room");
    }
    // get window dimentions

    var ww = $(window).width();
    console.log(maxHeight);
    var wh = $(window).height();
    $(".ticket").each(function (i) {
        var rotationNum = Math.round((Math.random() * 360) + 1);
        var rotation = "rotate(" + rotationNum + "deg)";
        var posx = Math.round(Math.random() * maxHeight) - 20;
        var posy = Math.round(Math.random() * heightArray) - 20;
        $(this).css("top", posy + "px").css("left", posx + "px").css("transform", rotation).css("-ms-transform", rotation).css("-webkit-transform", rotation);
    });

}

function responseLog(mData) {  
 
    $('.boxDiv').remove();
    $.each(mData, function (i, item) {
        if (item.eZoneId == item.mZoneId) {
            $('#Zone' + item.mFloorMasterId).append('<i class="material-icons md-36 uk-text-success boxDiv" data-uk-modal="{target:"#modal_overflow"}" data-uk-tooltip="{cls:"long- text"}" title="Name :' + item.Name + '</br> Agency: ' + item.Agency + '</br> Designation: ' + item.Designation + '</br> SkillCategory :' + item.SkillCategory + '</br> WorkCategory :' + item.WorkCategory + '</br> Activity :' + item.Activity + '">&#xE0C8;</i>');
        } else { $('#Zone' + item.mFloorMasterId).append('<i class="material-icons md-36 uk-text-danger boxDiv" data-uk-modal="{target:"#modal_overflow"}" data-uk-tooltip="{cls:"long- text"}" title="Name :' + item.Name + '</br> Agency: ' + item.Agency + '</br> Designation: ' + item.Designation + '</br> SkillCategory :' + item.SkillCategory + '</br> WorkCategory :' + item.WorkCategory + '</br> Activity :' + item.Activity + '">&#xE0C8;</i>');}
        
    });

    function BindArea() {

        console.log('BindArea');
        var heightArray = $(".post-container").map(function () {
            return $(this).height();
        }).get();

        var maxHeight = Math.max.apply(Math, heightArray);
        $(".room").height(maxHeight);
        $(".room").height(maxHeight);

        var ticket = "<div class='ticket'><i class='glyphicon glyphicon-map-marker'></i></div>";
        var ticket = "<i class='glyphicon glyphicon-map-marker ticket'></i>";
        var numTickets = 10;
        for (var x = 1; x <= numTickets; x++) {
            $(ticket).appendTo("#room1");
        }
        for (var x = 1; x <= 20; x++) {
            $(ticket).appendTo("#room");
        }
        // get window dimentions

        var ww = $(window).width();
        console.log(maxHeight);
        var wh = $(window).height();
        $(".ticket").each(function (i) {
            var rotationNum = Math.round((Math.random() * 360) + 1);
            var rotation = "rotate(" + rotationNum + "deg)";
            var posx = Math.round(Math.random() * maxHeight) - 20;
            var posy = Math.round(Math.random() * heightArray) - 20;
            $(this).css("top", posy + "px").css("left", posx + "px").css("transform", rotation).css("-ms-transform", rotation).css("-webkit-transform", rotation);
        });

    }
    $('.boxDiv').each(function (index) {
        $(this).css({
            left: ((Math.random() * $('.mainDiv').width())),
            top: ((Math.random() * $('.mainDiv').height()))
        });
    });
}


