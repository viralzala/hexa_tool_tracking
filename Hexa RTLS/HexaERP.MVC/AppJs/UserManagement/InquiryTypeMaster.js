/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:06-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    GetLoadDatas();
    GetData();
});
function GetLoadDatas() {
    $.getJSON("../InquiryTypeMaster/getLoadData", function (data) {
        $('#TC_LeadTypeId').kendoComboBox({
            dataTextField: "LeadType",
            dataValueField: "TC_LeadTypeId",
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
        var NameValue = $("#InquiryType").val();
        var TC_LeadTypeIds = $("#TC_LeadTypeId").val();
        console.log(TC_LeadTypeIds);
        if (NameValue == "") {
        }
        else {
            $.get("../InquiryTypeMaster/SaveData", { NameValue: NameValue, TC_LeadTypeId: TC_LeadTypeIds }, function (data) {
                console.log(data);
                GetData();
            });
        }
    });
    //Update Data
    $("#btnupdate").click(function () {
        var DataValues = $("#InquiryType").val();
        if (DataValues == "") {
        }
        else {
            var inqid = $("#TC_LeadTypeId").val(); var GetSecodId;
           
            if (inqid == "") {
                GetSecodId = $("#TC_LeadTypeIds").val();
            }
            else {
                GetSecodId = $("#TC_LeadTypeId").val();
            }

            $.get("../InquiryTypeMaster/UpdateData", { DataValue: DataValues, IIds: GetSecodId, ID: $("#TC_InquiryTypeId").val() }, function (data) {
                console.log(data);             
                GetData();
            });
        }
    });

    $("#tst").click(function () {
        var dropdownlist = $("#TC_InquiryTypeId").data("kendoDropDownList");
        dropdownlist.value(2);
    });

    var oTable;
    oTable = $('#tbl').dataTable();
    $('#global_filter').on('keyup click', function () {
        oTable.fnFilter($(this).val());
    });
});

//Get Data
function GetData() {
    $.getJSON("../InquiryTypeMaster/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "TC_InquiryTypeId" },
                { "mData": "InquiryType" },
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
    var Ids = $(this).closest("tr").find('td:eq(0)').text();
    var Confm = confirm('Do you want to Edit this Record?');
    if (Confm) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $.getJSON("/InquiryTypeMaster/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#InquiryType").val(item.InquiryType);
                $("#TC_InquiryTypeId").val(item.TC_InquiryTypeId);
                $("#TC_LeadTypeIds").val(item.TC_LeadTypeId);
               
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
        $.get("../InquiryTypeMaster/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});