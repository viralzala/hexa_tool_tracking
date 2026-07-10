// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("AgencyMasterCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();

    //
    function initializeComponets() {
        //$scope.isEdit = true;       
        InitDataBind();
    }
    //
    function InitDataBind() {
        $http({
            method: 'GET',
            url: '../AgencyMaster/GetAllAgency'
        }).then(function successCallback(response) {
            BindJqueryTable(response.data.IData);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.SaveIAgencyData = function () {
        if (angular.isUndefined($scope.Agency) || $scope.Agency === null) {
            toastr.error('Enter Agency Name');
            return false;
        }
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        _formCSV = $("#_formAgency");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {
            $http({
                method: 'POST',
                url: '../AgencyMaster/Create',
                data: _eData
            }).then(function successCallback(response) {
                console.log(response.data);
                if (response.data.Flag == true) {
                    document.getElementById("_formAgency").reset();
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
        }
    };

    function BindJqueryTable(pData) {
        var table = $('#tbls').DataTable();
        table.clear().draw();
        $("#tbls").dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": pData,
            "aoColumns": [
                { "mData": "mAgencyId" },
                { "mData": "Agency" },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a href="#EditIdata" id="EditIdata" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit" data-uk-modal="{center:true}"> <i id="Editbtn"  class="md-icon material-icons">&#xE254;</i></a><i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                    }
                }
            ]
        });
    };

    //
    $('body').on('click', '#EditIdata', function () {
        var table;
        $(document).ready(function () {
            table = $('#tbls').DataTable();
        });
        //to get currently clicked row object
        var row = $(this).parents('tr')[0];
        //for row data
        var isp = table.row(row).data();
        //console.log(isp.mAgencyId);
        $http({
            method: 'GET',
            url: '../AgencyMaster/Edit',
            params: { id: isp.mAgencyId }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                $scope.Agency = response.data.Idata.Agency;
                $scope.mAgencyId = response.data.Idata.mAgencyId;
                $scope.isEdit = false; $scope.isAdd = true;
            }
            else { toastr.error(response.data.Message); }
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    });

    //
    $('body').on('click', '#Deletebtn', function () {
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            var table;
            $(document).ready(function () {
                table = $('#tbls').DataTable();
            });
            //to get currently clicked row object
            var row = $(this).parents('tr')[0];
            //for row data
            var isp = table.row(row).data();
            DeleteRecord(isp.mAgencyId);
        }
        else { console.log('Cancelled'); return false; }
    });


    $scope.EdIagencyEdit = function () {
        if (angular.isUndefined($scope.Agency) || $scope.Agency === null || angular.isUndefined($scope.mAgencyId) || $scope.mAgencyId === null) {
            toastr.error('Some Thing Went Wrong Please Refresh Page');
            return false;
        }
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        _formCSV = $("#_formAgency");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {
            $http({
                method: 'POST',
                url: '../AgencyMaster/Edit',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    document.getElementById("_formAgency").reset();
                    $scope.isEdit = true; $scope.isAdd = false;
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
        }
    };

    //
    function DeleteRecord(_id) {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '/AgencyMaster/Delete',
            params: { id: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                InitDataBind();
                setTimeout(function () {
                    modal.hide()
                }, 1000)
                toastr.warning(response.data.Message);
            }
            else {
                setTimeout(function () {
                    modal.hide()
                }, 1000)

                toastr.error(response.data.Message);
            }
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    //
    $(function () {
        var oTable;
        oTable = $('#tbls').dataTable();
        $('#global_filter').on('keyup click', function () {
            oTable.fnFilter($(this).val());
        });
    });
});

