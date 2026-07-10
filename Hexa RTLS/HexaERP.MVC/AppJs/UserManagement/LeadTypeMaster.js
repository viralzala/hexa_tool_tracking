
//Mudassar I Added On 03/02/2017

$(document).ready(function () {
    console.log('Calling Form Load');
    GetLeadTypes();
    $("#btnupdate").hide();
});
//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var LeadTypeName = $("#LeadType").val();
        if (LeadTypeName == "") {
        }
        else {
            $.get("../LeadTypeMaster/SaveLeadType", { LeadTypeName: LeadTypeName }, function (data) {
                console.log(data);
                GetLeadTypes();
            });
        }
    });
    //Update Data
    $("#btnupdate").click(function () {
        var LeadTypeName = $("#LeadType").val();
        if (LeadTypeName == "") {
        }
        else {
            $.get("../LeadTypeMaster/UpdateLeadType", { LeadTypeName: LeadTypeName, ID: $("#TC_LeadTypeId").val() }, function (data) {
                console.log(data);
                $("#LeadType").val(""); $("#TC_LeadTypeId").val("");
                GetLeadTypes();
            });
        }
    });

    var oTable;
    oTable = $('#tblLead').dataTable();
    $('#global_filter').on('keyup click', function () {
        oTable.fnFilter($(this).val());
    });
});

//Get Data
function GetLeadTypes() {
    $.getJSON("../LeadTypeMaster/getLeadType", function (data) {
        $('#tblLead').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                { "mData": "TC_LeadTypeId" },
                 { "mData": "LeadType" },

            {
                'mRender': function (aaData, type, row, meta) {
                    return '<i id="Editbtn"  class="md-icon material-icons">&#xE254;</i>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                }
            },
            ]
        });
    });
}

//Edit Data
$(document).on('click', '#Editbtn', function (e) {
    var LeadId = $(this).closest("tr").find('td:eq(0)').text();
    var Confm = confirm('Do you want to Edit this Record?');
    if (Confm) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $.getJSON("/LeadTypeMaster/getLeadTypeWithId", { LeadId: LeadId }, function (data) {
            $.each(data, function (i, item) {
                console.log(item.TC_LeadTypeId);
                $("#LeadType").val(item.LeadType);
                $("#TC_LeadTypeId").val(item.TC_LeadTypeId);
                ///$("#moduleIDid").val(item.moduleID).change();
            });
        });
    }
    else {
    }
});

//Delete
$(document).on('click', '#Deletebtn', function (e) {
    var LeadTypeId = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        $.get("../LeadTypeMaster/DeleteLeadType", { ID: LeadTypeId }, function (data) {
            console.log(data);
            GetLeadTypes();
        });
    }
    else {
        console.log('cancel');
    }
});