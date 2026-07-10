$(document).ready(function () {
    $.getJSON("../WindowManagement/getmodulename", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.moduleID, text: item.modulename });
        });
        $('#moduleIDid').kendoComboBox({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: arr,
            filter: "contains",
            suggest: true,
            index: 3
        });
    });
    BindWindowDatatable();
    $("#btnupdate").hide();
});

function SaveWindow() {
    var MenuName = $("#Menunameid").val();
    var ModuleID = $("#moduleIDid").val();
    var PageNameid = $("#PageNameid").val();
    if (ModuleID == "" || MenuName == "" || PageNameid == "") {
    }
    else {
        var formData = JSON.stringify($("#formdataforwindowmanagement").serializeObject(), null, 2);
        $.get("../WindowManagement/SaveWindow", { JsonData: formData }, function (data) {
            $("#WindowIDFOREDITINGHIDDENid").val("");
            $("#Menunameid").val("");
            $("#Descriptionid").val("");
            $("#PageNameid").val("");
            $("#MenuUrlid").val("");
            $("#moduleIDid").val("").change();
            BindWindowDatatable();
            $("#btnupdate").hide();
            $("#btnsave").show();
            BindWindowDatatable();
            document.getElementById("formdataforwindowmanagement").reset();
        })
    }
}
function UpdateWindow() {
    var MenuName = $("#Menunameid").val();
    var ModuleID = $("#moduleIDid").val();
    var PageNameid = $("#PageNameid").val();
    if (ModuleID == "" || MenuName == "" || PageNameid == "") {
    }
    else {
        var formData = JSON.stringify($("#formdataforwindowmanagement").serializeObject(), null, 2);
        var AppMenuIdhidden = $("#WindowIDFOREDITINGHIDDENid").val();
        $.get("../WindowManagement/UpdateWindow", { JsonData: formData, AppMenuIdhidden: AppMenuIdhidden }, function (data) {
            console.log(formData);
            $("#WindowIDFOREDITINGHIDDENid").val("");
            $("#Menunameid").val("");
            $("#Descriptionid").val("");
            $("#PageNameid").val("");
            $("#MenuUrlid").val("");
            $("#moduleIDid").val("").change();
            BindWindowDatatable();
            $("#btnupdate").hide();
            $("#btnsave").show();
        })
    }
}
function CancelWindow() {
    window.location = "./ViewPage/WindowManagement.aspx";
}
function BindWindowDatatable() {
    $.ajax({
        type: "POST",
        url: "../WindowManagement/BindWindowDatatable",
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
                    { "mData": "AppMenuId" },
                     { "mData": "moduleName" },
                    { "mData": "MenuName" },
                     { "mData": "PageName" },
                   
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
$(document).on('click', '#Editbtn', function (e) {
    var windowiddddd = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to Edit this Record?');
    if (answer) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $.getJSON("../WindowManagement/getwindowdata", { windowid: windowiddddd }, function (data)
        {           
            $.each(data, function (i, itemdata) {
               $("#WindowIDFOREDITINGHIDDENid").val(itemdata.AppMenuId);
                $("#Menunameid").val(itemdata.MenuName);
                $("#Descriptionid").val(itemdata.Description);
                $("#PageNameid").val(itemdata.PageName);
                $("#MenuUrlid").val(itemdata.MenuUrl);
                 $("#moduleIDid").val(itemdata.ModuleID).change();
            });           
        });
    }
    else {
        console.log('cancel');
    }
});
$(document).on('click', '#Deletebtn', function (e) {
    var WindowId = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        console.log('yes');
        $.ajax({
            type: "POST",
            url: "../WindowManagement/DeleteWindowData",
            data: "{WindowId:'" + WindowId + "'}",
            contentType: "application/json; charset=utf-8",
            datatype: "jsondata",
            async: "true",
            success: function (response) {
                BindWindowDatatable();
            },
            error: function (response) {
            }
        });
    }
    else {
        console.log('cancel');
    }
});