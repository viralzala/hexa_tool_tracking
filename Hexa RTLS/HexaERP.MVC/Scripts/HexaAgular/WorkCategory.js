// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("WorkCategoryCtrl", function ($timeout, $scope, $http, $window) {
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
            url: '../WorkCategory/GetCollData'
        }).then(function successCallback(response) {
            BindJqueryTable(response.data.IData);
        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.SaveFormCollData = function () {
        if (angular.isUndefined($scope.WorkCategory) || $scope.WorkCategory === null) {
            toastr.error('Enter Work Category');
            return false;
        }
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {
            $http({
                method: 'POST',
                url: '../WorkCategory/Create',
                data: _eData
            }).then(function successCallback(response) {
                //console.log(response.data);
                if (response.data.Flag == true) {
                    document.getElementById("_formColl").reset();
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
                { "mData": "mWorkCategoryId" },
                { "mData": "WorkCategory" },
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
        //console.log(isp.mWorkCategoryId);
        $http({
            method: 'GET',
            url: '../WorkCategory/Edit',
            params: { id: isp.mWorkCategoryId }
        }).then(function successCallback(response) {
           // console.log(response.data);
            if (response.data.Flag == true) {
                $scope.WorkCategory = response.data.Idata.WorkCategory;
                $scope.mWorkCategoryId = response.data.Idata.mWorkCategoryId;
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
            DeleteRecord(isp.mWorkCategoryId);
        }
        else { console.log('Cancelled'); return false; }
    });


    $scope.EditFormCollData = function () {
        if (angular.isUndefined($scope.WorkCategory) || $scope.WorkCategory === null || angular.isUndefined($scope.mWorkCategoryId) || $scope.mWorkCategoryId === null) {
            toastr.error('Some Thing Went Wrong Please Refresh Page');
            return false;
        }
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);

        if (angular.isUndefined(_eData) || _eData === null) {
            console.log('Error');
            alert("Please all the fileds");
        }
        else {
            $http({
                method: 'POST',
                url: '../WorkCategory/Edit',
                data: _eData
            }).then(function successCallback(response) {
                if (response.data.Flag == true) {
                    document.getElementById("_formColl").reset();
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
            url: '../WorkCategory/Delete',
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

