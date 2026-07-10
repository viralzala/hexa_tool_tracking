
/// <reference path="C:\Users\Administrator\Documents\Visual Studio 2015\Projects\HexaERP\HexaERP.MVC\AngularJs/angular.js" />
/// <reference path="AngularController.js" />
/// <reference path="AngularModules.js" />
$(document).ready(function () {
    $.getJSON("../ModuleManagement/GetAllIcons", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.IconID, text: item.IconCode });
        });
        $('#icon').kendoComboBox({
            dataTextField: "text",
           dataValueField: "value",
            dataSource: arr,
            filter: "contains",
            suggest: true,
            index: 3
        });
    });
    BindModuleDatatable();
    $("#btnupdate").hide();
});

function SaveModuledata() {
    var iconval = $("#icon").val();
    var icon = $("#icon").data("kendoComboBox").text();
    var description = $("#description").val();
    var moduleName = $("#moduleName").val();
    if (moduleName == "" || description == "" || icon == "") {
    }
    else {
      //  var formData = JSON.stringify($("#form_validation").serializeObject(), null, 2);
        $.get("../ModuleManagement/SaveModuledata", { icon: icon, description: description, moduleName: moduleName }, function (data) {
            $("#icon").data("kendoComboBox").text("");
            $("#icon").val("");
            $("#description").val("");
            $("#moduleName").val("");
            $("#btnupdate").hide();
            $("#btnsave").show();
            BindModuleDatatable();
        })
    }
}
function UpdateModule() {
    var icon = $("#icon").data("kendoComboBox").text();
    var iconval = $("#icon").val();
   var description = $("#description").val();
    var moduleName = $("#moduleName").val();
    if (moduleName == "" || description == "" || icon == "") {
    }
    else {
       // var formData = JSON.stringify($("#form_validation").serializeObject(), null, 2);
        var ModuleIDFOREDITINGHIDDEN = $("#ModuleIDFOREDITINGHIDDEN").val();
        $.get("../ModuleManagement/UpdateModule", { icon: icon, description: description, moduleName: moduleName, ModuleIDFOREDITINGHIDDEN: ModuleIDFOREDITINGHIDDEN }, function (data) {
            //    console.log(formData);
           $("#icon").data("kendoComboBox").text("");
            $("#icon").val("");
            $("#description").val("");
            $("#moduleName").val("");
            $("#btnupdate").hide();
            $("#btnsave").show();
            BindModuleDatatable();
         })
    }
}
function CancelModule() {
    window.location = "./ViewPage/ModuleManagement.aspx";
}
$(document).on('click', '#Editbtn', function (e) {
    var ModuleId = $(this).closest("tr").find('td:eq(0)').text();
    var ModuleName = $(this).closest("tr").find('td:eq(1)').text();
    var ModuleDescription = $(this).closest("tr").find('td:eq(2)').text();
    var Icon = $(this).closest("tr").find('td:eq(3)').text();
    var answer = confirm('Do you want to Edit this Record?');
   
    if (answer) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $("#ModuleIDFOREDITINGHIDDEN").val(ModuleId);
        $("#moduleName").val(ModuleName);
        $("#description").val(ModuleDescription);
        $("#icon").val(Icon);
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