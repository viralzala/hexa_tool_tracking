/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:05-04-2017
///Description:
/// </summary>
$(document).ready(function () {   
    GetReaders();
    //Call the yourAjaxCall() function every 1000 millisecond    

    $("#btnstart").click(function () {
        StartIntr();
        alert('Tracking Started....');
    });
    $("#btnstop").click(function () {
        clearInterval(mdata);
        alert('Tracking Stoped!!');
    });
    $("#btnrefresh").click(function () {
        LocationDatas();
    });
    
});
//
function GetReaders() {
    $.getJSON("../ToolsTrack/getGetReadersData", function (data) {
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
    $.getJSON("../ToolsTrack/getGetFloorsData", function (data) {
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
        $.getJSON("../ToolsTrack/getGetRoomsData", { FloorId: $('#mFloorMasterId').val() }, function (data) {
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

function test() {
    UIkit.modal.confirm('Are you sure to Start Reader?', function () {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $("#lblerr").text("");

        $.get("../ToolsTrack/ReaderInit", { Reader: $("#ReaderIp").val() }, function (data) {
            console.log(data);
            $("#lblerr").text(data);
            $("#getRfid").show();
            setTimeout(function () {
                modal.hide()              
            }, 3000)

            var mdata;
            var timer;
            StartIntr();
            function StartIntr() {
                mdata = setInterval("GetDatsTesting()", 60000);
            }
        });


    });
}
//
function ClearRdata() {
    $("#lblerr").text(""); $("#RFID").val(""); $("#PORTID").val(""); $("#global_filter").val("");
    $.get("../ToolsTrack/ReaderClear", function (data) {
        $("#lblerr").text(data);
    });
}
//
function StopReader() {
    $("#lblerr").text("");
    $.get("../ToolsTrack/StopReaders", function (data) {
        $("#lblerr").text(data);
    });
}
//
function GetId() {    
    $.getJSON("../ToolsTrack/getGetToTrackData", function (data) {
        console.log(data);
        $('#dt_tableTrack').empty();
        $('#dt_tableTrack').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "RFID" },
                { "mData": "tDate" }
            ]
        });
    });

    $.getJSON("../ToolsTrack/GetIdsAll", function (data) {
        console.log(data);
        $('#dt_tableInv').empty();
        $('#dt_tableInv').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "Key" },
                { "mData": "Value" }
            ]
        });
    });
}
//$timeout.cancel(mytimeout);


function GetDatsTesting() {
   
    console.log('called');
    $.getJSON("ToolsTrack/GetAllCount", function (data) {
        $("#txtTot").text(data);
    });

    $.getJSON("ToolsTrack/GetTrackCount", function (data) {
        $("#peity_live_text").text(data);
    });

    var tot = $("#txtTot").text();
    var Trac = $("#peity_live_text").text();
    if (tot < Trac) {
        $("#txtmix").text(tot);
    }
    else {
        $("#txtmix").text(tot - Trac);
    }


    var d = new Date();
    document.getElementById("lbldt").innerHTML = d.toLocaleTimeString();

    $.getJSON("../ToolsTrack/getGetToTrackData", function (data) {
        console.log(data);
        $('#dt_tableTrack').empty();
        $('#dt_tableTrack').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "RFID" },
                 {
                     "render": function (aaData, type, row, meta) {
                         return '<a ><i class="uk-input-group-icon uk-icon-calendar"></i></a> ' + ConvertJsonDatetoanyformat(row.tDate, "mm/dd/yyyy") + '<span class="uk-margin-right"><i class="material-icons"></i> <span class="uk-text-muted uk-text-small">' + ConvertJsonDatetoanyformat(row.tDate, 'hh:mm ampm') + '</span></span>';
                     }
                 },
            ]
        });
    });

    
    
   

    //$.getJSON("../ToolsTrack/GetListtagsRead", function (data) {
    //    console.log("------------tags Read----------");
    //    console.log(data);
    //    console.log("-------------------------------");
    //});
    //$.getJSON("../ToolsTrack/GetListtagsMontor", function (data) {
    //    console.log("------------tags Montor----------");
    //    console.log(data);
    //    console.log("-------------------------------");
    //});

    //$.getJSON("../ToolsTrack/GetListtagsOparation", function (data) {
    //    console.log("------------tags Different----------");
    //    console.log(data);
    //    console.log("-------------------------------");
    //});
}
//Get Data
function GetData() {
    $.getJSON("../ToolsTrack/getData", function (data) {
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
        //$.getJSON("/ToolsTrack/getDataWithId", { ID: Ids }, function (data) {
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
        $.get("../ToolsTrack/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
            MsgSucess("Notification: " + data);
        });
    }
    else {
        console.log('cancel');
    }
});

//
function ConvertJsonDatetoanyformat(jsondate, format) {
    var yourdate = '';
    var dateAsFromServerSide = jsondate ///Date(1291374337981)/
    //Now let's convert it to js format
    //Example: Fri Dec 03 2010 16:37:32 GMT+0530 (India Standard Time)
    var parsedDate = new Date(parseInt(dateAsFromServerSide.substr(6)));

    var jsDate = new Date(parsedDate); //Date object

    //Play with jsDate properties getDate(), getDay() etc

    var fulldate = dateAsFromServerSide;
    var ParsedDate = parsedDate;
    var GetDay = jsDate.getDay();
    var GetDate = jsDate.getDate();
    var GetFullYear = jsDate.getFullYear();
    var GetHours = jsDate.getHours();
    var GetMilliseconds = jsDate.getMilliseconds();
    var GetMinutes = jsDate.getMinutes();
    var GetMonth = jsDate.getMonth() + 1;
    var GetSeconds = jsDate.getSeconds();
    var GetTime = jsDate.getTime();
    var GetTimezoneOffset = jsDate.getTimezoneOffset();
    var GetUTCDate = jsDate.getUTCDate();
    var GetUTCDay = jsDate.getUTCDay();
    var GetUTCFullYear = jsDate.getUTCFullYear();
    var GetUTCHours = jsDate.getUTCHours();
    var GetUTCMilliseconds = jsDate.getUTCMilliseconds();
    var GetUTCMinutes = jsDate.getUTCMinutes();
    var GetUTCMonth = jsDate.getUTCMonth();
    var GetUTCSeconds = jsDate.getUTCSeconds();
    var GetYear = jsDate.getYear();

    if (format == 'mm/dd/yyyy') {
        yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear;
    }
    else if (format == 'dd/mm/yyyy') {
        yourdate = GetDate + '/' + GetMonth + '/' + GetFullYear;
    }
    else if (format == 'mm/dd/yyyy hh:mm:ss') {
        yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear + " " + GetHours + ":" + GetMinutes + ":" + GetSeconds;
    }
    else if (format == 'hh:mm:ss') {
        yourdate = GetHours + ":" + GetMinutes + ":" + GetSeconds;
    }
    else if (format == 'hh:mm 24hour') {
        yourdate = GetHours + ":" + GetMinutes;
    }
    else if (format == 'hh:mm ampm') {
        yourdate = formatAMPM(jsDate)
    }
    else if (format == 'mm/dd/yyyy hh:mm ampm') {
        var timeampm = formatAMPM(jsDate);
        yourdate = GetMonth + '/' + GetDate + '/' + GetFullYear + ' ' + timeampm;
    }
    return yourdate;
}
//
function formatAMPM(date) {

    var hours = date.getHours();
    var minutes = date.getMinutes();
    var ampm = hours >= 12 ? 'PM' : 'AM';
    hours = hours % 12;
    hours = hours ? hours : 12; // the hour '0' should be '12'
    minutes = minutes < 10 ? '0' + minutes : minutes;
    var strTime = hours + ':' + minutes + ' ' + ampm;
    return strTime;
}