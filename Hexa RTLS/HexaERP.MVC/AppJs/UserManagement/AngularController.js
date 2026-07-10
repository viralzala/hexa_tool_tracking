/// <reference path="C:\Users\Administrator\Documents\Visual Studio 2015\Projects\HexaERP\HexaERP.MVC\Content/assets/js/pages/kendoui.min.js" />

/// <reference path="C:\Users\Administrator\Documents\Visual Studio 2015\Projects\HexaERP\HexaERP.MVC\Scripts/jquery-1.10.2.js" />
/// <reference path="C:\Users\Administrator\Documents\Visual Studio 2015\Projects\HexaERP\HexaERP.MVC\AngularJs/angular.js" />
/// <reference path="AngularModules.js" />
/// <reference path="ModuleManagement.js" />

ModuleManagementModule.controller("ModuleManagementController", function ($scope, ModuleManagement) {
    $("#btnupdate").hide();
    //GetAllIcons(); // page load get all
    BindModuleDatatable();

    $.getJSON("../ModuleManagement/GetAllIcons", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.IconID, text: item.IconCode });
        });
        $('#ddInquirySource').kendoComboBox({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: arr,
            filter: "contains",
            suggest: true,
            index: 3
        });
    });
    $scope.SaveModule = function () {
        if ($scope.Modulename == "undefined" || $scope.ICONSSELECTddddd == "undefined")
        {
            alert("hi how r u ");
            alert("hhhhhhempty");
        }
        else {
            var Module = {
                moduleName: $scope.Modulename,
                description: $scope.ModuleDescription,
                icon: $scope.ICONSSELECTddddd,
            };
            var getRegisterData = ModuleManagement.SaveModuledata(Module);
            BindModuleDatatable();
        }  
    }
    $scope.CancelModule = function () {
        var url = window.location.href;
        var pathArray = url.split('/');;
        var protocol = pathArray[0];
        var host = pathArray[2];
        var urlp = protocol + '//' + host;
        var go = urlp + "/ViewPage/ModuleMgmt.aspx";
        window.location = go;
    }
    $scope.UpdateModule = function () {
        var Modulehhhhh = {
            moduleName: $scope.Modulename,
            description: $scope.ModuleDescription,
            ModuleId: $scope.ModuleIDFOREDITINGHIDDEN,
            icon: $scope.ICONSSELECTddddd,
        };
        var getRegisterData = ModuleManagement.UpdateModule(Modulehhhhh);
      
        BindModuleDatatable();
        $("#btnupdate").hide();
        $("#btnsave").show();
    }
    $(document).on('click', '#Editbtn', function (e) {
        var ModuleId = $(this).closest("tr").find('td:eq(0)').text();
        var answer = confirm('Do you want to Edit this Record?');
        var ModuleName = $(this).closest("tr").find('td:eq(1)').text();
        var ModuleDescription = $(this).closest("tr").find('td:eq(2)').text();
        var Icon = $(this).closest("tr").find('td:eq(3)').text();
        if (answer) {
            $("#btnsave").hide();
            $("#btnupdate").show();
            $scope.$apply(function () {
                $scope.Modulename = ModuleName;
                $scope.ModuleDescription = ModuleDescription;
                $scope.ModuleIDFOREDITINGHIDDEN = ModuleId;
            });
        }
        else {
            console.log('cancel');
        }


    });
    $(document).on('click', '#Deletebtn', function (e) {
        var ModuleId = $(this).closest("tr").find('td:eq(0)').text();
        var answer = confirm('Do you want to delete this Record?');
        if (answer) {
            console.log('yes');
            $.ajax({
                type: "POST",
                url: "../ModuleManagement/DeleteModuleData",
                data: "{ModuleId:'" + ModuleId + "'}",
                contentType: "application/json; charset=utf-8",
                datatype: "jsondata",
                async: "true",
                success: function (response) {
                    BindModuleDatatable();
                },
                error: function (response) {
                }
            });
        }
        else {
            console.log('cancel');
        }
    });
    function BindModuleDatatable() {
        $.ajax({
            type: "POST",
            url: "../ModuleManagement/BindModuleDatatable",
            data: "{}",
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            async: "true",
            error: function (response) { },
            success: function (response) {
                var parseJSONResult = response;
                $('#example1').dataTable({
                    "bDestroy": true,
                    "bProcessing": true,
                    "aaData": parseJSONResult,// <-- your array of objects
                    "aoColumns": [
                        { "mData": "moduleID" },
                         { "mData": "moduleName" },
                        { "mData": "description" },
                         {
                             "mData": "icon", 'mRender': function (aaData, type, row, meta) {
                                 return '<i class="material-icons">' + row.icon + '</i>';
                             }
                         },
                    {

                        'mRender': function (aaData, type, row, meta) {
                            return '<i id="Editbtn"  class="md-icon material-icons">&#xE254;</i>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                        }
                    },
                    ]
                });
            },
            error: function (response, textStatus, errorThrown) {
                console.log(errorThrown);
            }
        });
    }
})




