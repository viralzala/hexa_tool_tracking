// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("UploadMapCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    //
    $scope.deleteMap = function (_indId) {
        console.log(_indId);
        UIkit.modal.confirm('Are you sure to delete Indoor Map?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            $http({
                method: 'GET',
                url: '../UploadMap/DeleteIndMap',
                params: { _id: _indId}
            }).then(function successCallback(response) {
                console.log(response);
                if (response.data.Flag == true) {
                    InitDataBind();
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)
                    toastr.success(response.data.Message);
                }
                else {
                    setTimeout(function () {
                        modal.hide()
                    }, 1000)
                    toastr.error(response.data.Message);
                }
            }, function errorCallback(response) {
                console.log("Error : " + response.data.ExceptionMessage);
            });
        });
    };

    //
    function initializeComponets() {
        //$scope.isEdit = true;       
        InitDataBind();
    }

    //
    $scope.setImgPara = function (_iData) {
        $scope._thumImg = _iData.ImgPath; $scope.SetmIndooMapsId = _iData.mIndooMapsId;
    };

    $scope.putLocationInd = function (_iData) {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '../UploadMap/SetAxisMaps',
            params: { Xaxi: $("#form_x").text(), Yaxi: $("#form_y").text(), mIndoMapId: $scope.SetmIndooMapsId, mAtteId: _iData }
        }).then(function successCallback(response) {
            console.log(response);
            if (response.data.Flag == true) {
                InitDataBind();
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.success(response.data.Message);
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });

    };
    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../UploadMap/GetIndoorMaps'
        }).then(function successCallback(response) {
            $scope.IndoorMaps = response.data.mData;
            $scope.AttenasData = response.data.ObjData;
            setIndoor();
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };
});

function point_it(event, _scr) {
    pos_x = event.offsetX ? (event.offsetX) : event.pageX - document.getElementById("pointer_div").offsetLeft;
    pos_y = event.offsetY ? (event.offsetY) : event.pageY - document.getElementById("pointer_div").offsetTop;
    $("#form_x").text(pos_x); $("#form_y").text(pos_y);
}

function setIndoor() {
    $.getJSON("../UploadMap/GetAttenaLoc", function (data) {
        //console.log(data.mData);
        for (i = 0; i < data.mData.length; i++) {
            if (data.mData[i].mIndooMapsId !== null && typeof data.mData[i].mIndooMapsId !== "object") {
                //console.log(data.mData[i].Xaxis);
                //var ticket = '<i class="material-icons md-36 uk-text-success myElement" data-uk-modal="{target:'#gmap_route_modal'}">&#xE0C8;</i>';
                var ticket = '<span id=' + data.mData[i].mReaderSettupId + ' class="uk-badge" title="Sub Zone :' + data.mData[i].subZone + ', Att. Loc :' + data.mData[i].AttLoc + '" >' + data.mData[i].ReaderNo + ', Port:' + data.mData[i].AttPortId + '</span>';
                $("#" + data.mData[i].mIndooMapsId).append(ticket);
                var myElement = $('#' + data.mData[i].mReaderSettupId);
                myElement.css({
                    position: 'absolute',
                    left: data.mData[i].Xaxis + 'px',
                    top: data.mData[i].Yaxis + 'px',
                    textalign: 'center',
                });
            }
        }
    });
}