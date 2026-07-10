$(document).ready(function () {
    $('#DepartmentId').kendoComboBox({}); 
    onorganizationchange();
    BindUserDatatable();
    $("#btnupdate").hide();
});

function onorganizationchange() {
    $.getJSON("../UserManageMentForm/getDepartment", function (data) {
        var arr = [];
        $.each(data, function (i, item) {
            arr.push({ value: item.DepartMentID, text: item.DepartMentName });
        });
        $('#DepartmentId').kendoComboBox({
            dataTextField: "text",
            dataValueField: "value",
            dataSource: arr,
            filter: "contains",
            suggest: true,
            index: 3
        });
    });
}

function SaveUserData() {    
    var AppUserName = $("#AppUserName").val();
    var EMail = $("#EMail").val();
    var Password = $("#Password").val();
    var Mobile = $("#Mobile").val();
    //var OrgInfoId = $("#OrgInfoId").val();
    var DepartmentId = $("#DepartmentId").val();
    //var DOJ = $("#DOJ").val();
    //var DOB = $("#DOB").val();
    var Address = $("#Address").val();   

    if (AppUserName === "" || EMail === "" || Password === "" || Mobile === ""  || DepartmentId === "" || Address === "") {
       
    }
    else {
        var formdata = JSON.stringify($("#form_validation").serializeObject(), null, 2);
        $.post("../UserManageMentForm/SaveUserData", { formdata: formdata }, function (data) {           
            BindUserDatatable();
            $("#AppUserName").val("");
            $("#EMail").val("");
            $("#Password").val("");
            $("#Mobile").val("");
            $("#OrgInfoId").val("");
            $("#DepartmentId").val("");
            $("#DOJ").val("");
            $("#DOB").val("");
            $("#Address").val("");
            UIkit.modal.alert('<p></p><pre>' + data + '</pre>');
        })
    }
}
function UpdateUser() {
   

    var AppUserName = $("#AppUserName").val();
    var EMail = $("#EMail").val();
    var Password = $("#Password").val();
    var Mobile = $("#Mobile").val();
    //var OrgInfoId = $("#OrgInfoId").val();
    var DepartmentId = $("#DepartmentId").val();
    //var DOJ = $("#DOJ").val();
    //var DOB = $("#DOB").val();
    var Address = $("#Address").val();
    if (AppUserName === "" || EMail === "" || Password === "" || Mobile === "" || DepartmentId === "" || Address === "") {
    }
    else {
        var formData = JSON.stringify($("#form_validation").serializeObject(), null, 2);
        var APPUserID = $("#AppUserIDForHidden").val();
        //console.log(formData); console.log(APPUserID);

        $.post("../UserManageMentForm/UpdateUserData", { formData: formData, APPUserID: APPUserID }, function (data) {
            $("#AppUserName").val("");
            $("#EMail").val("");
            $("#Password").val("");
            $("#Mobile").val("");
            //$("#OrgInfoId").val("");
            //$("#DepartmentId").val("");
            $("#DOJ").val("");
            $("#DOB").val("");
            $("#Address").val("");
            $("#btnupdate").hide();
            $("#btnsave").show();
            BindUserDatatable();
        })
    }
}
function CancelUser() {
    window.location = "./ViewPage/UserManageMent.aspx";
}
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
$(document).on('click', '#Editbtn', function (e) {

    var appuserid = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to Edit this Record?');
    if (answer) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $.getJSON("../UserManageMentForm/getuserdata", { appuserid: appuserid }, function (data) {

            $.each(data, function (i, itemdata) {
                $("#AppUserIDForHidden").val(itemdata.AppUserId);
                $("#AppUserName").val(itemdata.AppUserName);
                $("#EMail").val(itemdata.EMail);
                $("#Password").val(itemdata.Password);
                $("#Mobile").val(itemdata.Mobile);             
                //$("#OrgInfoId").val(itemdata.OrgInfoId).change();
                //$("#DepartmentId").val(itemdata.DepartmentId).change();
                //var dob = ConvertJsonDatetoanyformat(itemdata.DOB, 'mm/dd/yyyy');
                //var doj = ConvertJsonDatetoanyformat(itemdata.DOJ, 'mm/dd/yyyy');
                //$("#DOJ").val(dob);
                //$("#DOB").val(doj);
                $("#Address").val(itemdata.Address);              
                if (itemdata.Sex == "F") {
                    var $radios = $('input:radio[name=Sex]');
                    if ($radios.is(':checked') === false) {
                        $radios.filter('[value=F]').prop('checked', true);
                    }
                }
                else if (itemdata.Sex == "M") {
                    var $radios = $('input:radio[name=Sex]');
                    if ($radios.is(':checked') === false) {
                        $radios.filter('[value=M]').prop('checked', true);
                    }
                }
                else {
                }
            });
        });
    }
    else {
        console.log('cancel');
    }
});
$(document).on('click', '#Deletebtn', function (e) {
    var appuserid = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        console.log('yes');
        $.ajax({
            type: "POST",
            url: "../UserManageMentForm/DeleteUsersData",
            data: "{appuserid:'" + appuserid + "'}",
            contentType: "application/json; charset=utf-8",
            datatype: "jsondata",
            async: "true",
            success: function (response) {
                BindUserDatatable();
            },
            error: function (response) {
            }
        });
    }
    else {
        console.log('cancel');
    }
});
function BindUserDatatable() {
    $.ajax({
        type: "POST",
        url: "../UserManageMentForm/BindUserDatatable",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: "true",
        error: function (response) { },
        success: function (response) {
            var parseJSONResult = response;
            //console.log(parseJSONResult);
            $('#example1').dataTable({
                "bDestroy": true,
                "bProcessing": true,
                "aaData": parseJSONResult,// <-- your array of objects
                "aoColumns": [
                    { "mData": "AppUserId" },
                     { "mData": "AppUserName" },
                    { "mData": "OrgInfoName" },
                     { "mData": "DepartMentName" },
                      { "mData": "Mobile" },
                     { "mData": "EMail" },
                    { "mData": "Sex" },
                           {

                               'mRender': function (aaData, type, row, meta) {
                                   return '<i id="Editbtn"  class="md-icon material-icons">&#xE254;</i>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
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