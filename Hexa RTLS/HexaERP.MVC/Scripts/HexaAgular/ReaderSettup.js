// ** Mudassar I **
//
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);

app.controller("ReaderSettupCtrl", function ($scope, $http, $timeout) {
    initializeComponets();
    $scope.CurrentDate = new Date();
    //
    function initializeComponets() {
        $scope.btnportUp = false;
        GetRoomsData();
    }

    $scope.$watch('form1.$valid', function (newValue) {
        // form1 is our form name
        $scope.isFormValid = newValue;
    });

    $scope.GetMacAdd = function () {
        var ipda = $("#ReaderIP").val();
        if (ipda == "" || ipda == null) {
            toastr.error("Enter Reader IP Address");
            return false;
        }
        else {
            $scope.btngetmac = false;
            $.get("../ReaderSettup/getReaderMac", { Ipaddress: $("#ReaderIP").val() }, function (data) {

                if (data.result == true) {
                    $("#ReaderNo").val(data.IData);
                    $scope.btngetmac = true;
                    //console.log(data);
                    toastr.success(data.IData);
                }
                else { alert(data.Message); $scope.btngetmac = true; }

            });
        }
    };

    $scope.DeletePorts = function (_iDa) {

        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            $http({
                method: 'GET',
                url: '../ReaderSettup/DeleteReaderData',
                params: { ID: _iDa }
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                    
                //console.log(response.data);
                if (response.data.result == true) {
                    toastr.warning(response.data.Message);
                    getRoomsPorts($("#mRoomMasterId").val());
                }
                else { alert(response.data.Message); }
            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                console.log("Error : " + response.data.ExceptionMessage);
            });
        } else { alert("Canceled"); }

    };
    $scope.EditPorts = function (_iDa) {
        $("#ReaderNo").val(_iDa.ReaderNo);
        $("#ReaderIP").val(_iDa.ReaderIP);
        $("#AttPortId").val(_iDa.AttPortId);
        $("#mReaderSettupId").val(_iDa.mReaderSettupId);
        //
        $("#lati").val(_iDa.lat);
        $("#long").val(_iDa.lng);
        $("#add").val(_iDa.description);

        $scope.btnportUp = true; $scope.btnportput = false;
    };
    $scope.readerSettupDataUpdate = function () {

        _formCSV = $("#myForm");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {
            $http({
                method: 'POST',
                url: '../ReaderSettup/updateReaderData',
                data: _eData
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                    
               // console.log(response.data);
                if (response.data.result == true) {
                    toastr.success(response.data.Message);
                    $scope.setupForm.ReaderNo = "";
                    getRoomsPorts($("#mRoomMasterId").val());
                    $scope.btnportUp = false;
                }
                else { alert(response.data.Message); }
            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    };

    $scope.readerSettupData = function () {
        _formCSV = $("#myForm");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
       // console.log(_eData);
        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {

            $http({
                method: 'POST',
                url: '../ReaderSettup/putReaderData',
                data: _eData
            }).then(function successCallback(response) {
                // this callback will be called asynchronously
                // when the response is available                    
                //console.log(response.data);
                if (response.data.result == true) {
                    toastr.success(response.data.Message);
                    $scope.setupForm.ReaderNo = "";
                    getRoomsPorts($("#mRoomMasterId").val());
                }
                else { alert(response.data.Message); }

            }, function errorCallback(response) {
                // called asynchronously if an error occurs
                // or server returns response with an error status.
                console.log("Error : " + response.data.ExceptionMessage);
            });
        }
    };


    function GetRoomsData() {
        $http({
            method: 'GET',
            url: '../ReaderSettup/GetRoomsData'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available                    
            //console.log(response.data);   
            var table = $('#tbl').DataTable();
            table.clear().draw();
            $('#tbl').dataTable({
                "destroy": true,
                "bDestroy": true,
                "bProcessing": true,
                "aaData": response.data,
                "aoColumns": [
                    { "mData": "mRoomMasterId" },
                    { "mData": "Site" },
                    { "mData": "Zone" },
                    { "mData": "FloorName" },
                    { "mData": "RoomName" },
                    {
                        'mRender': function (aaData, type, row, meta) {
                            return '<a id="install" href="#mailbox_new_message" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Install" data-uk-modal="{center:true}" class="md-btn md-btn-primary md-btn-mini md-btn-wave-light md-btn-icon waves-effect waves-button waves-light"><i  class="material-icons">&#xE884;</i></a>';
                        }
                    }
                ]
            });
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    $(function () {
        var oTable;
        oTable = $('#tbl').dataTable();
        $('#global_filter').on('keyup click', function () {
            oTable.fnFilter($(this).val());
        });
    });

    function getRoomsPorts(_RId) {
        $http({
            method: 'GET',
            url: '../ReaderSettup/GetReadersSettup',
            params: { Rid: _RId }
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available                               
            $scope.ReaderForRoom = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });

    }
    $(document).on('click', '#install', function (e) {
        var _RId = $(this).closest("tr").find('td:eq(0)').text();
        document.getElementById("myForm").reset();
        $("#mReaderSettupId").val("");
        $scope.btnportUp = false; $scope.btnportput = true;
        $("#mRoomMasterId").val(_RId);
        getRoomsPorts(_RId);
    });
});