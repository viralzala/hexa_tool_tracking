/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:24-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    $('#toSupplierId').kendoComboBox({});

    $('#mGroupMasterId').kendoComboBox({});
    $('#mIteamTypeMasterId').kendoComboBox({});
    GetIteams();
    GetGroups();
    GetTypes();
    GetUnits();
    $('#mRoomMasterId').kendoComboBox({});
    GetFloors();
    $("#btnupdate").hide();
    $("#getRfid").hide();
    GetData();
    GetReaders();
});
//
function GetReaders() {
    $.getJSON("../ToolsTag/getGetReadersData", function (data) {
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
function GetUnits() {
    $.getJSON("../ToolsTag/getGetUnitsData", function (data) {
        $('#mUnitMasterId').kendoComboBox({
            dataTextField: "UnitName",
            dataValueField: "mUnitMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
}
//
function GetIteams() {
    $.getJSON("../ToolsTag/getIteamsData", function (data) {
        $('#mIteamMasterId').kendoComboBox({
            dataTextField: "IteamName",
            dataValueField: "mIteamMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
}
//
function GetGroups() {
    $.getJSON("../ToolsTag/getGroupsData", function (data) {
        $('#mGroupMasterId').kendoComboBox({
            dataTextField: "GroupName",
            dataValueField: "mGroupMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
}
//
function GetTypes() {
    $.getJSON("../ToolsTag/getTypesData", function (data) {
        //console.log(data);
        $('#mIteamTypeMasterId').kendoComboBox({
            dataTextField: "IteamType",
            dataValueField: "mIteamTypeMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
}
//
//
function GetFloors() {
    $.getJSON("../ToolsTag/getGetFloorsData", function (data) {
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
        $.getJSON("../ToolsTag/getGetRoomsData", { FloorId: $('#mFloorMasterId').val() }, function (data) {
            if (data == "") {
                $('#mRoomMasterId').kendoComboBox({
                    dataTextField: "RoomName",
                    dataValueField: "mRoomMasterId",
                    filter: "contains",
                    dataSource: [{ RoomName: "", mRoomMasterId: "" }]
                });
            }

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
   
    //
    $("#btnsave").click(function () {
        var toTooltagIds = $("#toTooltagIds").val();
        var UIDs = $("#UIDs").val();
        var RFIDs = $("#RFIDs").val();
        var Sup = $("#Stockup").val();
        if (Sup == "") { return false; }
        UIkit.modal.confirm('Are you sure to update quantity?', function () {
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            if (toTooltagIds == "" || toTooltagIds == null || UIDs == "" || UIDs == null || RFIDs == "" || RFIDs == null) {
                UIkit.modal.alert('some thing went wrong!');
                return false;
            }
            else {
                $.get("../ToolsTag/setQTYData", { toTooltagId: toTooltagIds, UID: UIDs, RFID: RFIDs, qty: $("#Stockup").val() }, function (data) {
                    setTimeout(function () {
                        modal.hide()
                        UIkit.modal.alert(data);
                    }, 3000)
                    GetData();
                    $("#Stockup").val("");
                });
            }

        });
    });


    var oTable;
    oTable = $('#dt_tableExport').dataTable();
    $('#global_filter').on('keyup click', function () {
        oTable.fnFilter($(this).val());
    });

});

//
function test() {
    UIkit.modal.confirm('Are you sure to Start Reader?', function () {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $("#lblerr").text("");
        $.get("../ToolsTag/ReaderInit", { Reader: $("#Reader").val() }, function (data) {

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
    $.get("../ToolsTag/ReaderClear", function (data) {
        $("#lblerr").text(data);
    });
}
//
function StopReader() {
    $("#lblerr").text("");
    $.get("../ToolsTag/StopReaders", function (data) {
        $("#lblerr").text(data);
    });
}
//
function GetId() {
    modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
    $.getJSON("../ToolsTag/GetIds", function (data) {
        $.each(data, function (i, item) {
            $("#RFID").val(item.RFID); $("#global_filter").val(item.RFID);
            $("#PORTID").val(item.PORTID);
        });
        setTimeout(function () {
            modal.hide()
        }, 2000)
    });
}
//Get Data
function GetData() {
    $.getJSON("../ToolsTag/getData", function (data) {        
        $('#dt_tableExport').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "toTooltagId" },
                { "mData": "ToolName" },
                 { "mData": "Code" },
                  { "mData": "Manufacturer" },
                 { "mData": "Model" },
                  { "mData": "ModelNo" },
                   { "mData": "Serial" },
                   { "mData": "Condition" },
                   //{ "mData": "IteamStatus" },
                   //{ "mData": "Description" },
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
                     { "mData": "UnitName" },
                      { "mData": "Stock" },
                       {
                           "render": function (aaData, type, row, meta) {

                               if (row.bStock != null) {

                                   return '<a href="#mailbox_new_message" onclick="setqty(\'' + row.toTooltagId + '\',\'' + row.UID + '\',\'' + row.RFID + '\');"  data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" data-uk-modal="{center:true}"><h3>' + parseFloat(row.bStock) + '</h3></a>';
                               }
                               else {
                                   return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                               }

                           }
                       },
                        { "mData": "RoomName" },                         
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

function setqty(toTooltagId, UID, RFID) {
    $("#toTooltagIds").val(toTooltagId);
    $("#UIDs").val(UID);
    $("#RFIDs").val(RFID);
}
//Edit Data
$(document).on('click', '#Editbtn', function (e) {
    var Ids = $(this).closest("tr").find('td:eq(0)').text();
    var Confm = confirm('Do you want to Edit this Record?');
    if (Confm) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $("#lbltext").text("Edit Data");
        //$.getJSON("/ToolsTag/getDataWithId", { ID: Ids }, function (data) {
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
        $.get("../ToolsTag/DeleteData", { ID: Ids }, function (data) {
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});