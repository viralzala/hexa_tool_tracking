/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:06-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    GetLeadSubDispositions();
    GetData();
});
function GetLeadSubDispositions() {
    $.getJSON("../LeadSubDispositionMaster/getLeadSubDisposition", function (data) {
        $('#TC_LeadDispositionId').kendoComboBox({
            dataTextField: "Name",
            dataValueField: "TC_LeadDispositionId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
    $("#btnupdate").hide();
}

//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var DispositionName = $("#Name").val();
        var TC_InquiryTypeIds = $("#TC_InquiryTypeId").val();
        console.log(TC_InquiryTypeIds);
        if (DispositionName == "") {
        }
        else {
            $.get("../LeadSubDispositionMaster/SaveDisposition", { DispositionName: DispositionName, TC_InquiryTypeId: TC_InquiryTypeIds }, function (data) {
                console.log(data);
                GetData();
            });
        }
    });
    //Update Data
    $("#btnupdate").click(function () {
        var DispositionName = $("#Name").val();
        if (DispositionName == "") {
        }
        else {
            var inqid = $("#TC_InquiryTypeId").val(); var INQIds;
            if (inqid == "") {
                INQIds = $("#TC_InquiryTypeIds").val();
            }
            else {
                INQIds = $("#TC_InquiryTypeId").val();
            }

            $.get("../LeadSubDispositionMaster/UpdateDisposition", { DispositionName: DispositionName, InqID: INQIds, ID: $("#TC_LeadDispositionId").val() }, function (data) {
                console.log(data);
                $("#Name").val(""); $("#TC_LeadDispositionId").val("");
                GetData();
            });
        }
    });

    $("#tst").click(function () {
        var dropdownlist = $("#TC_InquiryTypeId").data("kendoDropDownList");
        dropdownlist.value(2);
    });
});

//Get Data
function GetData() {
    $.getJSON("../LeadSubDispositionMaster/getDisposition", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "TC_LeadDispositionId" },
                { "mData": "Name" },
                 { "mData": "InquiryType" },
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
    var Ids = $(this).closest("tr").find('td:eq(0)').text();
    var Confm = confirm('Do you want to Edit this Record?');
    if (Confm) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $.getJSON("/LeadSubDispositionMaster/getDispositionWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#Name").val(item.Name);
                $("#TC_LeadDispositionId").val(item.TC_LeadDispositionId); $("#TC_InquiryTypeIds").val(item.TC_LeadInquiryTypeId);
            });
        });
    }
    else {
    }
});
//Delete
$(document).on('click', '#Deletebtn', function (e) {
    var Ids = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        $.get("../LeadSubDispositionMaster/DeleteDisposition", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});