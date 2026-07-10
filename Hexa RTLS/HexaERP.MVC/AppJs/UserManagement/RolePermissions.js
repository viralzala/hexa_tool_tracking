$(document).ready(function () {

    $('#moduleID123').kendoComboBox({});
    $('#AppUserId123').kendoComboBox({});
    //$('#AppMenuId123').kendoComboBox({});

    $.getJSON("../RolePermissions/getmodulename", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.RolemoduleId, text: item.moduleName });
        });
        $('#moduleID123').kendoComboBox({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: arr,
            filter: "contains",
            suggest: true,
            index: 3
        });
    });
    $.getJSON("../RolePermissions/getusername", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.AppUserId, text: item.AppUserName });
        });
        $('#AppUserId123').kendoComboBox({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: arr,
            filter: "contains", 
            suggest: true,
            index: 3
        });
    });
    BindRolesDatatable();
     $("#btnupdatewindows").hide();
});
function moduleonselected() {
    var Rolemoduleid = $("#moduleID123").val(); 
   $.getJSON("../RolePermissions/getwindowsnamesaccordingtomodule", { Rolemoduleid: Rolemoduleid }, function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.AppMenuId +"!"+ item.RoleMenuId, text: item.MenuName });
        });
        console.log(arr);
        $('#AppMenuId123').html('');
        $('#AppMenuId123').selectize({
            plugins: {
                'remove_button': {
                    label: ''
                }
            },
            placeholder: 'Select Windows(s)',
            options: arr,
            render: {
                option: function (data, escape) {
                    return '<div class="option">' +
                        '<span>' + escape(data.text) + '</span>' +
                        '</div>';
                },
                item: function (data, escape) {
                    return '<div class="item">' + escape(data.text) + '</div>';
                }
            },
            maxItems: null,
            dataTextField: "text",
            dataValueField: "value",
            create: true,
            onDropdownOpen: function ($dropdown) {
                $dropdown
                    .hide()
                    .velocity('slideDown', {
                        begin: function () {
                            $dropdown.css({ 'margin-top': '0' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            },
            onDropdownClose: function ($dropdown) {
                $dropdown
                    .show()
                    .velocity('slideUp', {
                        complete: function () {
                            $dropdown.css({ 'margin-top': '' })
                        },
                        duration: 200,
                        easing: easing_swiftOut
                    })
            }
        });
    });
}
function SaveRolePermissions() {
    var AppUserId = $("#AppUserId123").val();
    var moduleID = $("#moduleID123").val();
    var c = $('#AppMenuId123');
    var AppMenuId = c.val();
    var ccc = $('#IsRead');
    var IsRead = ccc.val();
    var cccc = $('#Iswrite');
    var Iswrite = cccc.val();
    var cccc = $('#AppMenuId123');
    var arr = cccc.val();
    var str1 = arr.toString();
    var str2 = String(arr);
    if ($("#Iswrite").prop("checked") == true) {
        var Iswrite = $("#Iswrite").val();
    }
    if ($("#IsRead").prop("checked") == true) {
        var IsRead = $("#IsRead").val();
    }
    if (AppUserId == "" || moduleID == "" || AppMenuId == "" || IsRead == "" || Iswrite=="") { }
    else {

    var formdata = JSON.stringify($("#RolePermissions").serializeObject(), null, 2);
    $.post("../RolePermissions/SaveRolePermissions", { formdata: formdata, str2: str2, Iswrite: Iswrite, IsRead: IsRead }, function (data) {
        BindRolesDatatable();
         $("#AppUserId").val("");
         $("#moduleID").val("");
         $('#IsRead').val("");
         $('#Iswrite').val("");

         var multiSelect = $('#AppMenuId').data("kendoMultiSelect");
        multiSelect.value([]);
    })
    }
}
function BindRolesDatatable()
{
     $.ajax({
        type: "POST",
        url: "../RolePermissions/BindRolesDatatable",
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
                    { "mData": "RoleAcessId" },
                     { "mData": "RoleMenuId" },
                    { "mData": "OrgInfoName" },
                     { "mData": "AppUserName" },
                     { "mData": "moduleName" },
                        { "mData": "MenuName" },
                        { "mData": "IsRead" },
                        { "mData": "Iswrite" },           
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<i id="Deletebtnassign" class="md-icon material-icons">&#xE872;</i>';
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
$(document).on('click', '#Deletebtnassign', function (e) {
    var raccessid = $(this).closest("tr").find('td:eq(0)').text();
    var RMenuid = $(this).closest("tr").find('td:eq(1)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        console.log('yes');
        $.ajax({
            type: "POST",
            url: "../RolePermissions/Deletebtnassign",
            data: "{raccessid:'" + raccessid + "',RMenuid:'" + RMenuid + "'}",
            contentType: "application/json; charset=utf-8",
            datatype: "jsondata",
            async: "true",
            success: function (response) {
                BindRolesDatatable();
               $("#AppUserId123").val("");
               $("#moduleID123").val("");
               $('#AppMenuId123').val("");
               $('#IsRead').val("");
               $('#Iswrite').val("");
             },
            error: function (response) {
            }
        });
    }
    else {
        console.log('cancel');
    }
});