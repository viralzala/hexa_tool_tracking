// ** Mudassar I **
app.filter('jsonDate', ['$filter', function ($filter) {
    return function (input, format) {
        return (input) ? $filter('date')(parseInt(input.substr(6)), format) : '';
    };
}]);
//
app.controller("UserManagmentCtrl", function ($timeout, $scope, $http, $window) {
    initializeComponets();
    var dataS = [
        { "AppRoleId": 1, "text": "Admin" },
        { "AppRoleId": 3, "text": "User" },
        { "AppRoleId": 2, "text": "Super Admin" },
        { "AppRoleId": 4, "text": "Request Approval" },
        { "AppRoleId": 5, "text": "Quality Approval" }];
    //
    function initializeComponets() {
        InitDataBind(); InitUserBind();
    }

    $scope.AccessAllow = function (_id, _access) {
      //  console.log(_id); console.log(_access);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '/UserManageMentForm/AllowUser',
            params: { id: _id, Access: _access }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                InitUserBind();
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
    $scope.DeleteUser = function (_id) {
       // console.log(_id);
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'GET',
            url: '/UserManageMentForm/Delete',
            params: { id: _id }
        }).then(function successCallback(response) {
            if (response.data.Flag == true) {
                InitUserBind();
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
    function InitUserBind() {
        $http({
            method: 'GET',
            url: '../UserManageMentForm/getUserData'
        }).then(function successCallback(response) {
            //console.log(response);
            $scope.UserLists = response.data.Userd;
        });
    };
    //
    function InitDataBind() {

        $http({
            method: 'GET',
            url: '../UserManageMentForm/getMasterData'
        }).then(function successCallback(response) {
            console.log(response.data);

            $('#mDesignationId').kendoDropDownList({
                dataTextField: "Designation",
                dataValueField: "mDesignationId",
                filter: "contains",
                dataSource: response.data.Desig,
                suggest: true,
                index: 3
            });

            var mDesignationId = $("#mDesignationId").data("kendoDropDownList");
            mDesignationId.value(-1);

            $('#DepartMentID').kendoDropDownList({
                dataTextField: "DepartMentName",
                dataValueField: "DepartMentID",
                filter: "contains",
                dataSource: response.data.Dept,
                suggest: true,
                index: 3
            });

            var DepartMentID = $("#DepartMentID").data("kendoDropDownList");
            DepartMentID.value(-1);

            $('#AppRoleId').kendoDropDownList({
                dataTextField: "AppRoleName",
                dataValueField: "AppRoleId",
                filter: "contains",
                dataSource: response.data._AppRoles,
                suggest: true,
                index: 3
            });

            var AppRoleId = $("#AppRoleId").data("kendoDropDownList");
            AppRoleId.value(-1);


        }, function errorCallback(response) {
            console.log("Error : " + response.data.ExceptionMessage);
        });
    };

    $scope.SaveFormCollData = function () {
        _formCSV = $("#_formColl");
        var _eData = JSON.stringify(_formCSV.serializeObject(), null, 2);
        console.log(_eData);


        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $http({
            method: 'POST',
            url: '../UserManageMentForm/Create',
            data: _eData
        }).then(function successCallback(response) {
           // console.log(response.data);
            if (response.data.Flag == true) {
                document.getElementById("_formColl").reset();
                InitUserBind();
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

    function BindJqueryTable(pData) {
        var table = $('#tbls').DataTable();
        table.clear().draw();
        $("#tbls").dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": pData,
            "aoColumns": [
                { "mData": "mActivityId" },
                { "mData": "Activity" },
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
        //console.log(isp.mActivityId);
        $http({
            method: 'GET',
            url: '../Activity/Edit',
            params: { id: isp.mActivityId }
        }).then(function successCallback(response) {
            //console.log(response.data);
            if (response.data.Flag == true) {
                $scope.Activity = response.data.Idata.Activity;
                $scope.mActivityId = response.data.Idata.mActivityId;
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
            DeleteRecord(isp.mActivityId);
        }
        else { console.log('Cancelled'); return false; }
    });


    $scope.EditFormCollData = function () {
        if (angular.isUndefined($scope.Activity) || $scope.Activity === null || angular.isUndefined($scope.mActivityId) || $scope.mActivityId === null) {
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
                url: '../Activity/Edit',
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

