/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:02-03-2017
///Description:
/// </summary>
$(document).ready(function () {
    ResetDatas();
    $('#tbl').dataTable({});
    //GetRooms();     
    $("#btnupdate").hide();
    //$("#getRfid").hide();    
    GetReaders();

    //$('#mRoomMasterId').kendoComboBox({});
    GetFloors();
});

function ResetDatas() {
    $.get("../AssetIssue/DeleteAssetMonitor", function (data) {
    });
}
//
function GetRooms() {
    var _fid = $('#mFloorMasterId').val();
    if (_fid != "") {
        $.getJSON("../AssetIssue/getGetRoomsData", { FloorId: $('#mFloorMasterId').val() }, function (data) {
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
function GetFloors() {
    $.getJSON("../AssetIssue/getGetFloorsData", function (data) {
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
function GetReaders() {
    $.getJSON("../AssetIssue/getGetReadersData", function (data) {
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
        $("#lbltext").text("List Of RFID");
        GetId();
    });
    $("#btnrefresh").click(function () {
        GetId();
        alert("Reader Data Refreshed.");
    });
    //
    $(".button_finish").click(function () {

        // $.get("../AssetTag/ReaderInit", function (data) { });
    });

    $("#issQty").change(function () {
        console.log('onchange');
    });

    $("#btnIsuue").click(function () {
            UIkit.modal.confirm('Are you sure to isuue the asset?', function () {           
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            var mylist = [];
            $('#IsueeColl .md-list-addon-element .tAssetTagId').each(function () {
                //console.log($(this).val());
                mylist.push({
                    "tAssetTagId": $(this).val()
                });
            });
            var JsonData = JSON.stringify(mylist, null, 2);
            $.get("../AssetIssue/StockIssue", { IssuedTo: $("#IssuedTo").val(), IteamList: JsonData }, function (data) {
                console.log(data);

                $("#IsueeColl").empty();
                setTimeout(function () {
                    modal.hide()
                    UIkit.modal.alert('Sucessfully issued please refer tranfer details!');
                }, 3000)

                $.each(data, function (i, item) {
                    $("#ntfList").append('<li>\
                                    <div class="md-list-content">\
                                    <span class="md-list-heading">' + item.IteamName + '</span>\
                                    <span class="uk-text-muted uk-text-small">' + item.msg + '</span>\
                                    </div>\
                                </li>');
                });

            });
        });       
    });

    $("#btnTranfer").click(function () {
        UIkit.modal.confirm('Are you sure to tranfer the asset?', function () {           
            modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
            var mylist = [];
            $('#TranferColl .md-list-addon-element .tAssetTagId').each(function () {
                //console.log($(this).val());
                mylist.push({
                    "tAssetTagId": $(this).val()
                });
            });
            var JsonData = JSON.stringify(mylist, null, 2);
            $.get("../AssetIssue/StockTranfer", { mRoomMasterId: $("#mRoomMasterId").val(), IteamList: JsonData }, function (data) {
                console.log(data);
                $("#TranferColl").empty();
                setTimeout(function () {
                    modal.hide()
                    UIkit.modal.alert('Sucessfully tranfer please refer tranfer details!');                   
                }, 3000)

                $.each(data, function (i, item) {
                    $("#ntfList").append('<li>\
                                    <div class="md-list-content">\
                                    <span class="md-list-heading">' + item.IteamName + '</span>\
                                    <span class="uk-text-muted uk-text-small">' + item.msg + '</span>\
                                    </div>\
                                </li>');
                });
            });
        });
    });

    //$("#startb").click(function () {
    //    console.log('dddd');
    //});
});

function test() {
        UIkit.modal.confirm('Are you sure to Start Reader?', function () {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $("#lblerr").text("");
        $.get("../AssetIssue/ReaderInit", { Reader: $("#Reader").val() }, function (data) {          
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
    $.get("../AssetIssue/ReaderClear", function (data) {
        $("#lblerr").text(data);
    });
}
//
function StopReader() {
    $("#lblerr").text("");
    $.get("../AssetIssue/StopReaders", function (data) {
        $("#lblerr").text(data);
    });
}
//
function GetId() {
    modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
    $.getJSON("../AssetIssue/GetIds", function (data) {
        //console.log(data);
        $('#idmydb li').remove();
        $.each(data, function (i, item) {
            $("#idmydb").append('<li>\
                                 <div class="md-list-addon-element">\
                                 <input type="hidden" class="tAssetTagId" value=' + item.tAssetTagId + ' />\
                                                <i class="uk-icon-cubes"></i>\
                                            </div>\
                                            <div class="md-list-content">\
                                                <span class="md-list-heading">' + item.IteamName + ' | ' + item.AssetName + '</span>\
                                                <span class="uk-text-small uk-text-muted">' + item.IteamDescription + ', StocK: ' + item.bStock + '</span>\
                                        </div>\
                                                <span class="md-input-bar"></p></span></div>\
                                                </div>\
                                            </li>');
        });
        setTimeout(function () {
            modal.hide()
        }, 3000)
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
        //$.getJSON("/AssetIssue/getDataWithId", { ID: Ids }, function (data) {
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
        $.get("../AssetIssue/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            MsgSucess("Notification: " + data);
        });
    }
    else {
        console.log('cancel');
    }
});