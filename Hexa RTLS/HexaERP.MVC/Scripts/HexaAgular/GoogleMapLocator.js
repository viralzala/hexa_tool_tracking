// ** **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("GoogleMapLocatorCtrl", function ($scope, $http, $timeout) {
    initializeComponets();
    $scope.CurrentDate = new Date();
    //
    function initializeComponets() {
        //$scope.isEdit = true;      
        console.log('Called:HexaTracker');
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var myVar;
      
        setTimeout(function () {
            modal.hide()
        }, 1000);

        //SetControll();

        //myVar = setInterval(function () {
        //    $scope.$apply(SetControll());
        //}, 30000);


    };

    function SetControll() {
        console.log('Called:HexaTracker');
        var d = new Date();
        $scope.lastTracked = d.toLocaleTimeString();
        $http({
            method: 'GET',
            url: '../GoogleMapLocator/getGetToTrackData'
        }).then(function successCallback(response) {
            console.log(response.data);
            //$scope.FillterIData = response.data;
            LoadMap(response.data);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});


function LoadMap(markers) {

    var mapOptions = {
        center: new google.maps.LatLng(markers[0].lat, markers[0].lng),
        zoom: 8,
        mapTypeId: google.maps.MapTypeId.ROADMAP
    };

    var infoWindow = new google.maps.InfoWindow();
    var latlngbounds = new google.maps.LatLngBounds();
    var map = new google.maps.Map(document.getElementById("dvMap"), mapOptions);

    for (var i = 0; i < markers.length; i++) {
        var data = markers[i]
        var myLatlng = new google.maps.LatLng(data.lat, data.lng);
        var marker = new google.maps.Marker({
            position: myLatlng,
            map: map,
            title: data.title
        });
        (function (marker, data) {
            google.maps.event.addListener(marker, "click", function (e) {
                infoWindow.setContent("<div style = 'width:200px;min-height:40px'>" + data.title, data.description + "</div>");
                infoWindow.open(map, marker);
            });
        })(marker, data);
        latlngbounds.extend(marker.position);
    }
    var bounds = new google.maps.LatLngBounds();
    map.setCenter(latlngbounds.getCenter());
    map.fitBounds(latlngbounds);
}