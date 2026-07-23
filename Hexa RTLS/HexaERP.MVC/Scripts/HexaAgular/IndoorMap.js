// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("IndoorMapCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();
    //
    function initializeComponets() {
        //$scope.isEdit = true;    
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        InitDataBind();
        setTimeout(function () {
            modal.hide()
        }, 1000)
    };
    $scope.updateCustomRequest = function (data, type, res) {
        $scope.customParams.data = data;
        $scope.customParams.type = type;
        $scope.customParams.res = res;
        $scope.sampletext = "input text: " + data;
    };
    
    // NEW: Asset Search Variables
    $scope.assetSearch = '';
    $scope.assetMarker = null;
    $scope.showAssetMarker = false;
    $scope.assetNotFound = false;
    
    // NEW: Search Asset Function
    $scope.searchAsset = function () {
        if (!$scope.assetSearch || $scope.assetSearch.trim() === '') {
            return;
        }

        $scope.assetMarker = null;
        $scope.showAssetMarker = false;
        $scope.assetNotFound = false;

        $http.get("../IndoorMap/getGetToTrackData").then(function (response) {
            console.log("API RESPONSE:", response.data);
            var data = response.data;
            if (data && data.tAsset) {
                var searchTerm = $scope.assetSearch.trim().toLowerCase();
                var foundAsset = null;
                var latestDate = null;
                console.log("SEARCHING EPC/Asset:", searchTerm);

                for (var i = 0; i < data.tAsset.length; i++) {
                    var asset = data.tAsset[i];
                    if ((asset.Asset && asset.Asset.toString().toLowerCase().indexOf(searchTerm) !== -1) ||
                        (asset.IteamName && asset.IteamName.toLowerCase().indexOf(searchTerm) !== -1) ||
                        (asset.Epc && asset.Epc.toString().toLowerCase().indexOf(searchTerm) !== -1)) {

                        if (!latestDate || asset.tDate > latestDate) {
                            latestDate = asset.tDate;
                            foundAsset = asset;
                        }
                    }
                }

                if (foundAsset) {
                    console.log("SEARCH FOUND ASSET:", foundAsset);
                    console.log("Calling ShowTracks:", foundAsset.mReaderSettupId, foundAsset.mIndooMapsId, foundAsset.Xaxis, foundAsset.Yaxis);
                    $scope.assetMarker = foundAsset;
                    $scope.showAssetMarker = true;
                    $scope.assetNotFound = false;
                    ShowTracks(foundAsset.Xaxis, foundAsset.Yaxis);
                    console.log("assetMarker:", $scope.assetMarker);
                    console.log("MARKER DISPLAY TRUE");
                    console.log("IndoorMaps:", $scope.IndoorMaps);
                } else {
                    console.log("ASSET NOT FOUND");
                    $scope.assetMarker = null;
                    $scope.showAssetMarker = false;
                }
            }
        }, function (error) {
            console.log("API ERROR:", error);
        });
    };
    
    // NEW: Clear Search Function
    $scope.clearSearch = function () {
        $scope.assetSearch = '';
        $scope.assetMarker = null;
        $scope.showAssetMarker = false;
        $scope.assetNotFound = false;
    };
    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../IndoorMap/GetIndoorMaps'
        }).then(function successCallback(response) {
            //console.log(response);
            $scope.IndoorMaps = response.data.mData;
            $scope.AttenasData = response.data.ObjData;
            setIndoor();
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});

function setIndoor() {
    $.getJSON("../IndoorMap/getGetToTrackData", function (data) {
        localStorage["tEmp"] = JSON.stringify(data.tEmp); localStorage["tAsse"] = JSON.stringify(data.tAsset);
        for (i = 0; i < data.totals.length; i++) {
            if (data.totals[i].mIndooMapsId !== null && typeof data.totals[i].mIndooMapsId !== "object") {
                var tag = '<a title="Click on to see details of Employee & Asset" id=' + data.totals[i].mReaderSettupId + ' onClick="ShowTracks(' + data.totals[i].Xaxis + ',' + data.totals[i].Yaxis + ')" >\
                        '+ '<i  class="material-icons">&#xE55E;</i>\
                        ' + '<span class="uk-icon-button"><h2>' + data.totals[i].Count + '</h2></span>\
                        '+ '</a >';
                $("#" + data.totals[i].mIndooMapsId).append(tag);
                var myElement = $('#' + data.totals[i].mReaderSettupId);
                myElement.css({
                    position: 'absolute',
                    left: data.totals[i].Xaxis + 'px',
                    top: data.totals[i].Yaxis + 'px',
                    textalign: 'center',
                });
            }
        }
    });
}

function ShowTracks(_Xaxis, _Yaxis) {
    var tEmpJson = JSON.parse(localStorage["tEmp"]);
    var tEmpbuket = [];
    JSON.stringify(tEmpJson, function (key, value) {
        if (value.Xaxis === _Xaxis && value.Yaxis === _Yaxis)
            tEmpbuket.push(value);
        return value;
    })

    var tAsseJson = JSON.parse(localStorage["tAsse"]);
    var tAssebuket = [];
    JSON.stringify(tAsseJson, function (key, value) {
        if (value.Xaxis === _Xaxis && value.Yaxis === _Yaxis)
            tAssebuket.push(value);
        return value;
    })

    jQuery(document).ready(function ($) {
        document.querySelectorAll("#lEmp li").forEach(function (e) { e.remove() })
        document.querySelectorAll("#lAst li").forEach(function (e) { e.remove() })
        document.getElementById("tEmTot").innerHTML = tEmpbuket.length;

        aL = ""; eL = "";
        $.each(tEmpbuket, function (index, obj) {
            eL += ' <li><div class="md-list-addon-element">\
                                ' + '<img class="img_thumb" src="/Content/assets/img/avatars/user@2x.png" alt="" />\
                                '+ '</div>\
                                '+ '<div class="md-list-content">\
                                 '+ '<span class="md-list-heading">' + obj.Name + '</span>\
                                 ' + '<span class="uk-text-small uk-text-muted">' + obj.Zone + ' </br>' + ConvertJsonDatetoanyformat(obj.tDate, 'mm/dd/yyyy hh:mm ampm') + '</span>\
                                 ' + '</div></li>';

        });
        $("#lEmp").append(eL);
        document.getElementById("tAsTot").innerHTML = tAssebuket.length;
        $.each(tAssebuket, function (index, obj) {
            aL += ' <li><div class="md-list-addon-element">\
                                ' + '<img class="img_thumb" src=' + obj.img + ' alt="" />\
                                '+ '</div>\
                                '+ '<div class="md-list-content">\
                                 ' + '<span class="md-list-heading">' + obj.Asset + ', ' + obj.Model + '</span>\
                                 ' + '<span class="uk-text-small uk-text-muted">' + obj.Zone + ' </br>' + ConvertJsonDatetoanyformat(obj.tDate, 'mm/dd/yyyy hh:mm ampm') + '</span>\
                                 ' + '</div></li>';
        });
        $("#lAst").append(aL);
    });
}

//
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
}
//
function formatAMPM(date) {

    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
}


function myfun() {
    console.log('Working');
    //angular.element(document.getElementById('page_content')).scope().Demo();

    var scope = angular.element(
     document.
     getElementById("page_content")).
     scope();
    scope.$apply(function () {
        scope.updateCustomRequest('Working', 'Working', 'Working');
    });
}