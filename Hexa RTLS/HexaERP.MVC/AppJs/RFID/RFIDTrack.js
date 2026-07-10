/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:07-03-2017
///Description:
/// </summary>
$(document).ready(function () {
    //Call the yourAjaxCall() function every 1000 millisecond
    var mdata;

    LocationDatas();
    GetData();

    var timer;
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

    StartIntr();
  

    function StartIntr() {       
        mdata = setInterval("GetData()", 10000);
    }

});

function LocationDatas() {    
    $.getJSON("../RFIDTrack/LocationData", function (data) {
        $.each(data, function (i, item) {
            if (item.FloorNo == 1) {
                $("#lblfli").text(item.FloorName + " " + item.FloorNo);
                if (item.RoomNo == 1) {
                    $("#Roomfiri").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 2) {
                    $("#Roomfirii").text(item.RoomName + " " + item.RoomNo);
               }
                else if (item.RoomNo == 3) {
                   $("#Roomfiriii").text(item.RoomName + " " + item.RoomNo);
               }
                else if (item.RoomNo == 4) {
                   $("#Roomfiriv").text(item.RoomName + " " + item.RoomNo);
               }
            }
            if (item.FloorNo == 2) {
                $("#lblflii").text(item.FloorName + " " + item.FloorNo);
                if (item.RoomNo == 1) {
                    $("#Roomfiiri").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 2) {
                    $("#Roomfiirii").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 3) {
                    $("#Roomfiiriii").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 4) {
                    $("#Roomfiiriv").text(item.RoomName + " " + item.RoomNo);
                }
            }
            if (item.FloorNo == 3) {
                $("#lblfliii").text(item.FloorName + " " + item.FloorNo);
                if (item.RoomNo == 1) {
                    $("#Roomfiiiri").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 2) {
                    $("#Roomfiiirii").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 3) {
                    $("#Roomfiiiriii").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 4) {
                    $("#Roomfiiiriv").text(item.RoomName + " " + item.RoomNo);
                }
            }
            if (item.FloorNo == 4) {
                $("#lblfliv").text(item.FloorName + " " + item.FloorNo);
                if (item.RoomNo == 1) {
                    $("#Roomfivri").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 2) {
                    $("#Roomfivrii").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 3) {
                    $("#Roomfivriii").text(item.RoomName + " " + item.RoomNo);
                }
                else if (item.RoomNo == 4) {
                    $("#Roomfivriv").text(item.RoomName + " " + item.RoomNo);
                }
            }

        });
    });
}
function GetData() {

    var cfiri = 0, cfirii = 0, cfiriii = 0, cfiriv = 0; var cfiiri = 0, cfiirii = 0, cfiiriii = 0, cfiiriv = 0;
    var cfiiiri = 0, cfiiirii = 0, cfiiiriii = 0, cfiiiriv = 0; var cfivri = 0, cfivrii = 0, cfivriii = 0, cfivriv = 0;

    clearTags();

    $.getJSON("../RFIDTrack/MonitoringData", function (data) {       
        BindArea();
        $.each(data, function (i, item) {
            var info = item.Name + " | " + item.MYRoomName;
            if (item.Types === true) {
                if (item.Color == 0) {
                    var ticket = "<a  title='" + info + "' data-uk-tooltip='{cls:'uk-tooltip-small',pos:'left'}'><i class='material-icons md-30 md-color-green-700 ticket'>&#xE837;</i></a>";

                }
                else {
                    var ticket = "<a title='" + info + "' data-uk-tooltip='{cls:'uk-tooltip-small',pos:'left'}'><i class='material-icons md-30 md-color-red-700 ticket'>&#xE837;</i></a>";
                }
            }
            else {
                if (item.Color == 0) {
                    var ticket = "<a title='" + info + "' data-uk-tooltip='{cls:'uk-tooltip-small',pos:'left'}'><i class='material-icons md-color-green-700 ticket'>&#xE835;</i></a>";
                }
                else {
                    var ticket = "<a title='" + info + "' data-uk-tooltip='{cls:'uk-tooltip-small',pos:'left'}'><i class='material-icons md-color-red-700 ticket'>&#xE835;</i></a></a>";
                }
            }
            //
            if (item.INFloorNo == 1) {               
                if (item.INRoomNo == 1) {
                    cfiri += isNaN(1) ? 0 : parseInt(1);                  
                    $(ticket).appendTo("#mapfiri");
                }
                else if (item.INRoomNo == 2) {
                    cfirii += isNaN(1) ? 0 : parseInt(1);               
                    $(ticket).appendTo("#mapfirii");
                }
                else if (item.INRoomNo == 3) {
                    cfiriii += isNaN(1) ? 0 : parseInt(1);              
                    $(ticket).appendTo("#mapfiriii");
                }
                else if (item.INRoomNo == 4) {
                    cfiriv += isNaN(1) ? 0 : parseInt(1);                
                    $(ticket).appendTo("#mapfiriv");
                }
            }
            //
            else if (item.INFloorNo == 2) {               
                if (item.INRoomNo == 1) {
                    cfiiri += isNaN(1) ? 0 : parseInt(1);                 
                    $(ticket).appendTo("#mapfiiri");
                }
                else if (item.INRoomNo == 2) {
                    cfiirii += isNaN(1) ? 0 : parseInt(1);                
                    $(ticket).appendTo("#mapfiirii");
                }
                else if (item.INRoomNo == 3) {
                    cfiiriii += isNaN(1) ? 0 : parseInt(1);                 
                    $(ticket).appendTo("#mapfiiriii");
                }
                else if (item.INRoomNo == 4) {
                    cfiiriv += isNaN(1) ? 0 : parseInt(1);                 
                    $(ticket).appendTo("#mapfiiriv");
                }
            }
                //
            else if (item.INFloorNo == 3) {               
                if (item.INRoomNo == 1) {
                    cfiiiri += isNaN(1) ? 0 : parseInt(1);                  
                    $(ticket).appendTo("#mapfiiiri");
                }
                else if (item.INRoomNo == 2) {
                    cfiiirii += isNaN(1) ? 0 : parseInt(1);
                    $(ticket).appendTo("#mapfiiirii");
                }
                else if (item.INRoomNo == 3) {
                    cfiiiriii += isNaN(1) ? 0 : parseInt(1);                   
                    $(ticket).appendTo("#mapfiiiriii");
                }
                else if (item.INRoomNo == 4) {
                    cfiiiriv += isNaN(1) ? 0 : parseInt(1);                
                    $(ticket).appendTo("#mapfivriv");
                }
            }
                //
            else if (item.INFloorNo == 4) {
              
                if (item.INRoomNo == 1) {
                    cfivri += isNaN(1) ? 0 : parseInt(1);                
                    $(ticket).appendTo("#mapfivri");
                }
                else if (item.INRoomNo == 2) {
                    cfivrii += isNaN(1) ? 0 : parseInt(1);                 
                    $(ticket).appendTo("#mapfivrii");
                }
                else if (item.INRoomNo == 3) {
                    cfivriii += isNaN(1) ? 0 : parseInt(1);                 
                    $(ticket).appendTo("#mapfivriii");
                }
                else if (item.INRoomNo == 4) {
                    cfivriv += isNaN(1) ? 0 : parseInt(1);               
                    $(ticket).appendTo("#mapfivriv");
                }
            }           
        });

        $("#Countfiri").text(cfiri);
        $("#Countfirii").text(cfirii);
        $("#Countfiriii").text(cfiriii);
        $("#Countfiriv").text(cfiriv);

        $("#Countfiiri").text(cfiiri);
        $("#Countfiirii").text(cfiirii);
        $("#Countfiiriii").text(cfiiriii);
        $("#Countfiiriv").text(cfiiriv);

        $("#Countfiiiri").text(cfiiiri);
        $("#Countfiiirii").text(cfiiirii);
        $("#Countfiiiriii").text(cfiiiriii);
        $("#Countfiiiriv").text(cfiiiriv);

        $("#Countfivri").text(cfivri);
        $("#Countfivrii").text(cfivrii);
        $("#Countfivriii").text(cfivriii);
        $("#Countfivriv").text(cfivriv);
        BindArea();
    });
}
function BindArea() {
    var heightArray = $(".timeline_content_addon").map(function () {
        return $(this).height();
    }).get();

    var maxHeight = Math.max.apply(Math, heightArray);
    $(".room").height(maxHeight);
    $(".room").height(maxHeight);
    //console.log(maxHeight);
    //var ticket = "<div class='ticket'><i class='glyphicon glyphicon-map-marker'></i></div>";
    //var ticket = "<i class='glyphicon glyphicon-map-marker ticket'></i>";
    //var numTickets = 10;
    //for (var x = 1; x <= numTickets; x++) {
    //    $(ticket).appendTo("#room1");
    //}
    //for (var x = 1; x <= 20; x++) {
    //    $(ticket).appendTo("#room");
    //}
    // get window dimentions
    var ww = $(window).width();
    //console.log(ww);
    var wh = $(window).height();
    $(".ticket").each(function (i) {
        var rotationNum = Math.round((Math.random() * 360) + 1);
        var rotation = "rotate(" + rotationNum + "deg)";
        var posx = Math.round(Math.random() * maxHeight) - 20;
        var posy = Math.round(Math.random() * heightArray) - 20;
        $(this).css("top", posy + "px").css("left", posx + "px").css("transform", rotation).css("-ms-transform", rotation).css("-webkit-transform", rotation);
    });

}
function clearTags() {
    // $(".room").height(""); $(".room").height("");

    $('#mapfiri').html(''); $('#mapfirii').html('');
    $('#mapfiriii').html(''); $('#mapfiriv').html('');

    $('#mapfiiri').html(''); $('#mapfiirii').html('');
    $('#mapfiiriii').html(''); $('#mapfiiriv').html('');

    $('#mapfiiiri').html(''); $('#mapfiiirii').html('');
    $('#mapfiiiriii').html(''); $('#mapfiiiriv').html('');

    $('#mapfivri').html(''); $('#mapfivrii').html('');
    $('#mapfivriii').html(''); $('#mapfivriv').html('');
    //console.log(data);
}