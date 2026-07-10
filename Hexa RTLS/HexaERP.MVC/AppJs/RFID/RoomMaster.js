/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:24-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    GetFloors();
    $("#btnupdate").hide();
    GetData();
});
function GetFloors() {
    //$.getJSON("../RoomMaster/getFloorsData", function (data) {
    //    $('#mFloorMasterId').kendoComboBox({
    //        dataTextField: "FloorName",
    //        dataValueField: "mFloorMasterId",
    //        filter: "contains",
    //        dataSource: data,
    //        suggest: true,
    //        index: 3
    //    });
    //});
}

//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var mFloorMasterId = $("#mFloorMasterId").val();
        var RoomName = $("#RoomName").val();
        //var RoomNo = $("#RoomNo").val();
        if (mFloorMasterId == "" || RoomName == "") {
        }
        else {
            $.get("../RoomMaster/SaveData", { mFloorMasterId: mFloorMasterId, RoomName: RoomName}, function (data) {
                console.log(data);
                alert(data);
                document.getElementById("page_settings").reset();
                GetData();
            });
        }
    });

    $("#btnNew").click(function () {
        $("#btnupdate").hide();
        $("#btnsave").show();
        $("#lbltext").text("Create New");
    });
    //Update Data
    $("#btnupdate").click(function () {
        var mFloorMasterId = $("#mFloorMasterId").val();
        var RoomName = $("#RoomName").val();
        //var RoomNo = $("#RoomNo").val();

        if (RoomName == "") {
        }
        else {
            var FloorId = $("#TC_InquiryTypeId").val(); var _FloorId;

            if (FloorId == "") {
                _FloorId = $("#mFloorMasterIds").val();
            }
            else {
                _FloorId = $("#mFloorMasterId").val();
            }

            $.get("../RoomMaster/UpdateData", { _FloorId: _FloorId, RoomName: RoomName, ID: $("#mRoomMasterId").val() }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                $("#btnupdate").hide();
                $("#btnsave").show();
                GetData();
                alert(data);

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
    var table = $('#tbl').DataTable();
    table.clear().draw();
    $.getJSON("../RoomMaster/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "mRoomMasterId" },
                { "mData": "FloorName" },               
                  { "mData": "RoomName" },                
               {
                   'mRender': function (aaData, type, row, meta) {
                       return '<a id="btnNew" href="#mailbox_new_message" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Create New" data-uk-modal="{center:true}"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
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
        $("#lbltext").text("Edit Data");
        $.getJSON("/RoomMaster/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#mRoomMasterId").val(item.mRoomMasterId);
                $("#mFloorMasterId").val(item.mFloorMasterId);
                $("#mFloorMasterIds").val(item.mFloorMasterId);
                $("#RoomName").val(item.RoomName);               
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
        $.get("../RoomMaster/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            alert(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});