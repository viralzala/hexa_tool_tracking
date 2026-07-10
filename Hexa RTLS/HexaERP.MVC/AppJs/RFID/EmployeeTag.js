/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:02-03-2017
///Description:
/// </summary>
$(document).ready(function () {
    $('#mRoomMasterId').kendoComboBox({});
    GetFloors();
    //GetRooms();     
    $("#btnupdate").hide();
    $("#getRfid").hide();
    GetData();
    GetReaders();
});
//
function GetReaders() {
    $.getJSON("../EmployeeTag/getGetReadersData", function (data) {
        $('#Reader').kendoComboBox({
            dataTextField: "ReaderIP",
            dataValueField: "ReaderIP",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
}
//
function GetFloors() {
    $.getJSON("../EmployeeTag/getGetFloorsData", function (data) {
        $('#mFloorMasterId').kendoComboBox({
            dataTextField: "FloorName",
            dataValueField: "mFloorMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });


    });
}
//
function GetRooms() {   
    var _fid = $('#mFloorMasterId').val();
    if (_fid != "") {
        $.getJSON("../EmployeeTag/getGetRoomsData", { FloorId: $('#mFloorMasterId').val() }, function (data) {
            if (data == "") {
                $('#mRoomMasterId').kendoComboBox({
                    dataTextField: "RoomName",
                    dataValueField: "mRoomMasterId",
                    filter: "contains",
                    dataSource: [{ RoomName: "", mRoomMasterId: "" }]
                });
            }
            console.log(data);
            $('#mRoomMasterId').kendoComboBox({
                dataTextField: "RoomName",
                dataValueField: "mRoomMasterId",
                filter: "contains",
                dataSource: data,
                suggest: true,
                index: 3
            });
        });
    }
    else {
       // return false;
    }
}
//
function mWarehouseMasterIdChange() {
    var WarehId = $("#mWarehouseMasterId").val();
    if (WarehId != "") { GetLoactes(WarehId); }
}
//
$(function () {  
    //
    $("#btnNew").click(function () {
        $("#btnupdate").hide();
        $("#btnsave").show();
        $("#lbltext").text("Create New");
    });

    var oTable;
    oTable = $('#tbl').dataTable();
    $('#global_filter').on('keyup click', function () {
        oTable.fnFilter($(this).val());
    });

    //
    $(".button_finish").click(function () {
        console.log('Calling');
        // $.get("../AssetTag/ReaderInit", function (data) { });
    });
});
//
//$(document).on('click', '#startb', function () {
//    jQuery("#startb").hide();
//    console.log('Calling Jquery');
//    $(this).css('visibility:hidden;')
//})
//
function test() {
       UIkit.modal.confirm('Are you sure to Start Reader?', function () {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $("#lblerr").text("");
        $.get("../EmployeeTag/ReaderInit", { Reader: $("#Reader").val() }, function (data) {
            console.log(data);
            $("#lblerr").text(data);
            $("#getRfid").show();
            setTimeout(function () {
                modal.hide()
            }, 3000)
        });
    });
}
//
function ClearRdata() {
    $("#lblerr").text(""); $("#RFID").val(""); $("#PORTID").val(""); $("#global_filter").val("");
    $.get("../EmployeeTag/ReaderClear", function (data) {
        $("#lblerr").text(data);
    });
}
//
function StopReader() {
    $("#lblerr").text("");
    $.get("../EmployeeTag/StopReaders", function (data) {
        $("#lblerr").text(data);
    });
}
//
function GetId() {
    modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
    $.getJSON("../EmployeeTag/GetIds", function (data) {       
        $.each(data, function (i, item) {
            $("#RFID").val(item.RFID);
            $("#global_filter").val(item.RFID);
            $("#PORTID").val(item.PORTID);
        });
        setTimeout(function () {
            modal.hide()
        }, 2000)
    });
}
//Get Data
function GetData() {
    var table = $('#tbl').DataTable();
    table.clear().draw();
    $.getJSON("../EmployeeTag/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "tEmployeeTagId" },
                { "mData": "EmployeeName" },
                 { "mData": "EmployeeId" },
                  { "mData": "EmailId" },
                 { "mData": "ContactNo" },                                     
                    {
                        "render": function (aaData, type, row, meta) {

                            if (row.RFID != null) {

                                return '<span class="uk-badge uk-badge-primary"><b>' + row.RFID + '</b></span>';
                            }
                            else {
                                return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                            }

                        }
                    },                  
                        
                         {
                             "render": function (aaData, type, row, meta) {

                                 if (row.RoomName != null) {

                                     return '<span class="uk-badge uk-badge-warning"><b>' + row.RoomName + '</b></span><span class="uk-badge uk-badge-warning">No:<b>' + row.RoomNo + '</b></span>';
                                 }
                                 else {
                                     return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                                 }

                             }
                         },
               {
                   'mRender': function (aaData, type, row, meta) {
                       //return '<a id="btnNew" href="#mailbox_new_message" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Create New" data-uk-modal="{center:true}"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                       return '<i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
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
        //$.getJSON("/AssetTag/getDataWithId", { ID: Ids }, function (data) {
        //    $.each(data, function (i, item) {
        //        $("#mRoomMasterId").val(item.mRoomMasterId);
        //        $("#mFloorMasterId").val(item.mFloorMasterId); $("#mFloorMasterIds").val(item.mFloorMasterId);
        //        $("#RoomName").val(item.RoomName); $("#RoomNo").val(item.RoomNo);

        //    });
        //});
    }
    else {
    }
});
//Delete
$(document).on('click', '#Deletebtn', function (e) {
    var Ids = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        $.get("../EmployeeTag/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
            MsgSucess("Notification: " + data);
        });
    }
    else {
        console.log('cancel');
    }
});