$(document).ready(function () {

    $('#OrgInfoId111').kendoComboBox({});
    $('#moduleID111').kendoComboBox({});

    $.getJSON("../WindowManagement/getmodulename", function (data) {
        $('#moduleID111').kendoComboBox({
            dataTextField: "modulename",
            dataValueField: "moduleID",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
    $.getJSON("../OrganizationManagement/getOrganizationname", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.OrgInfoId, text: item.OrgInfoName });
        });
        $('#OrgInfoId111').kendoComboBox({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: arr,
            filter: "contains",
            suggest: true,
            index: 3
        });
    });
    BindmodulesandwindowsassignedDatatable();
    $("#btnupdatewindows").hide();
});
var array = [];
var jdata;
function moduleonselected() {
    var c = $('#moduleID111');
    var moduleid = c.val();
    var ccc22 = $('#OrgInfoId111');
    var orgid = ccc22.val();
    if (orgid == "") { return false; }
    $.getJSON("../OrganizationManagement/getwindowsnamesaccordingtomodule", { moduleid: moduleid, orgid: orgid }, function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.AppMenuId, text: item.MenuName });
        });
        console.log(data);
        //$('#product_edit_tags_controlcustomizedbyme').html('');
        //$('#product_edit_tags_controlcustomizedbyme').children().removeProp('selected');
        //$("#product_edit_tags_controlcustomizedbyme").val("").selectmenu("refresh");
        //document.getElementById("ResetIteams").reset();
        //$('#product_edit_tags_controlcustomizedbyme').val("");

        //var multiSelect = $('#product_edit_tags_controlcustomizedbyme').data("kendoMultiSelect");
        //console.log(multiSelect);
        //multiSelect.value([]);

        $('#product_edit_tags_controlcustomizedbyme').selectize({
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
function btnassignwindowssave() {

    var Datefrom = $("#kUI_datetimepicker_range_start").val();
    var Dateto = $("#kUI_datetimepicker_range_end").val();
    var c = $('#OrgInfoId111');
    var orgid = c.val();
    var ccc = $('#moduleID111');
    var moduleid = ccc.val();
    var cccc = $('#product_edit_tags_controlcustomizedbyme');
    var arr = cccc.val();
    var str1 = arr.toString();
    var str2 = String(arr);

    if ($("#Iswrite").prop("checked") == true) {
        //  values.push("!" + $this.val() + "!");
        var Iswrite = $("#Iswrite").val();
    }
    if ($("#IsRead").prop("checked") == true) {
        //  values.push("!" + $this.val() + "!");
        var IsRead = $("#IsRead").val();
    }
    $.get("../OrganizationManagement/btnassignwindowssave", { Iswrite: Iswrite, IsRead: IsRead, Datefrom: Datefrom, Dateto: Dateto, orgid: orgid, moduleid: moduleid, windowid: str2 }, function (data) {
        BindmodulesandwindowsassignedDatatable();
        $("#OrgInfoId111").val("");
        $("#moduleID111").val("");
        //var multiSelect = $('#product_edit_tags_controlcustomizedbyme').data("kendoMultiSelect");
        //multiSelect.value([]);
        $("#kUI_datetimepicker_range_end").val("");
        $("#kUI_datetimepicker_range_start").val("");
    })
}
function btnupdatewindowsupdate() {
    var formData = JSON.stringify($("#assigningmoduletoorganizationform").serializeObject(), null, 2);
    var rolemenuhidden = $("#rolemenuhidden").val();
    var rolemodulehidden = $("#rolemodulehidden").val();
    $.get("../OrganizationManagement/btnupdatewindowsupdate", { JsonData: formData, rolemodulehidden: rolemodulehidden, rolemenuhidden: rolemenuhidden }, function (data) {
        BindmodulesandwindowsassignedDatatable();
        $("#OrgInfoId111").val("");
        $("#moduleID111").val("");
        $("#product_edit_tags_controlcustomizedbyme").val("");
        $("#kUI_datetimepicker_range_end").val("");
        $("#kUI_datetimepicker_range_start").val("");
        $("#btnupdatewindows").hide();
        $("#btnassignwindows").show();
    })
}
function btnCancelwindowscancel() {
    window.location = "../../ViewPage/OrganizationReg.aspx";
}
function BindmodulesandwindowsassignedDatatable() {
    $.ajax({
        type: "POST",
        url: "../OrganizationManagement/BindmodulesandwindowsassignedDatatable",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: "true",
        error: function (response) { },
        success: function (response) {
            var parseJSONResult = response;
            $('#exampleassigningtoorganization').dataTable({
                "bDestroy": true,
                "bProcessing": true,
                "aaData": parseJSONResult,// <-- your array of objects
                "aoColumns": [
                    { "mData": "RolemoduleId" },
                     { "mData": "RoleMenuId" },
                    { "mData": "OrgInfoName" },
                     { "mData": "moduleName" },
                        { "mData": "MenuName" },
                        {
                            "mData": "IsRead"
                        },
                        //    , 'mRender': function (aaData, type, row, meta)
                        //    {

                        //        if (row.IsRead == "true"||"1")
                        //        {
                        //            return '<i class="material-icons">&#xE876;</i>';
                        //        }
                        //        else if (row.IsRead == "false"||"0") {

                        //            return '<i class="material-icons">&#xE14C;</i>';
                        //        }
                        //        else
                        //        {

                        //            return '';
                        //        }

                        //    }
                        //},

                          { "mData": "IsWrite" },
                            //'mRender': function (aaData, type, row, meta) {
                        //       alert(row.IsWrite)
                        //        if (row.IsWrite == "true"||"1") {
                        //            return '<i class="material-icons">&#xE876;</i>';
                        //        }
                        //        else if (row.IsWrite == "false"||"0")
                        //        {
                        //            return '<i class="material-icons">&#xE14C;</i>';
                        //        }
                        //        else
                        //        {

                        //            return '';
                        //        }

                        //    }
                        //},

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
    var RModuleid = $(this).closest("tr").find('td:eq(0)').text();
    var RMenuid = $(this).closest("tr").find('td:eq(1)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        console.log('yes');
        $.ajax({
            type: "POST",
            url: "../OrganizationManagement/Deletebtnassign",
            data: "{RModuleid:'" + RModuleid + "',RMenuid:'" + RMenuid + "'}",
            contentType: "application/json; charset=utf-8",
            datatype: "jsondata",
            async: "true",
            success: function (response) {
                BindmodulesandwindowsassignedDatatable();
                $("#OrgInfoId111").val("");
                $("#moduleID111").val("");
                $("#product_edit_tags_controlcustomizedbyme").val("");
                $("#kUI_datetimepicker_range_end").val("");
                $("#kUI_datetimepicker_range_start").val("");
                $("#btnupdatewindows").hide();
                $("#btnassignwindows").show();
            },
            error: function (response) {
            }
        });
    }
    else {
        console.log('cancel');
    }
});