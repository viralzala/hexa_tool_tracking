/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:06-01-2017
///Description:
/// </summary>s

//==== To show data when page initially loads.
$(document).ready(function () {
    //Dynamic Column creation and dataSet binding:
    $('#ddLeadTypeq').kendoComboBox({});
    $('#ddInquiryTypeq').kendoComboBox({});
    $('#ddLeadDispositionq').kendoComboBox({});

    GetLeadsData();
    GetLeadType();
    GetInquirySource();
    GetFollowUpActions();
    Getmails();
});
function Getmails() {
    // $("#processmail").css("display", "block");
    $.getJSON("/CRM/Getmails", function (data) {

        data.reverse();
        if (data == "") {
            $("#maillist").append('<li><span class="uk-badge uk-badge-danger">Might be your email not configured!</span></li>');
        }

        $.each(data, function (i, iteam) {     // bind the dropdown list using json result  

            var Sub;
            if (iteam.HtmlDataText == "") {
                Sub = "No Body Content";
            }
            else {
                Sub = iteam.HtmlDataText;
            }

            $("#maillist").append('<li>\
                                        <div class="md-card-list-item-menu" data-uk-dropdown="{mode:"click",pos:"bottom-right"}">\
                                            <a class="md-icon material-icons">&#xE5D4;</a>\
                                            <div class="uk-dropdown uk-dropdown-small">\
                                                <ul class="uk-nav">\
                                                    <li><a onclick="EmailLeads(\'' + iteam.SenderName + '\',\'' + iteam.SenderAddress + '\',\'' + i + '\');"><i class="material-icons">&#xE15E;</i> Create Lead</a></li>\
                                                </ul>\
                                            </div>\
                                        </div>\
                                        <span class="md-card-list-item-date" data-uk-tooltip="{pos:"right"}" title="' + ConvertJsonDatetoanyformat(iteam.Date, 'mm/dd/yyyy hh:mm ampm') + '">' + ConvertJsonDatetoanyformat(iteam.Date, 'mm/dd/yyyy hh:mm ampm') + '</span>\
                                        <div class="md-card-list-item-select">\
                                        </div>\
                                        <div class="md-card-list-item-avatar-wrapper">\
                                            <span class="md-card-list-item-avatar md-bg-cyan" data-uk-tooltip="{pos:"right"}"  title="' + iteam.SenderName + '">' + iteam.SenderName.charAt(0) + '</span>\
                                        </div>\
                                        <div class="md-card-list-item-sender">\
                                            <span data-uk-tooltip="{pos:"right"}" title="' + iteam.SenderAddress + '">' + iteam.SenderAddress + '</span>\
                                        </div>\
                                        <div class="md-card-list-item-subject">\
                                            <div class="md-card-list-item-sender-small">\
                                                <span></span>\
                                            </div>\
                                            <span data-uk-tooltip="{cls:"long-text"}" title="' + iteam.Subject.replace('"', " ") + '"><b>' + iteam.Subject + '</b></span>\
                                        </div>\
                                        <div class="md-card-list-item-content-wrapper">\
                                            <div class="md-card-list-item-content" id="' + i + '">\
                                            ' + Sub + '\
                                            </div>\
                                            <form class="md-card-list-item-reply">\
                                                <label for="mailbox_reply_1295">Reply to <span></span></label>\
                                                <textarea class="md-input md-input-full" name="mailbox_reply_1295" id="mailbox_reply_1295" cols="30" rows="4"></textarea>\
                                                <button class="md-btn md-btn-flat md-btn-flat-primary">Send</button>\
                                            </form>\
                                        </div>\
                                    </li>');
        });
        $("#processmail").css("display", "none");
        $("#mbtnrefresh").css("display", "block");
    })
}
//
function EmailLeads(SenderName, SenderAddress, i) {
    var Name = SenderName.substr(0, SenderName.indexOf(' '));
    var Lastname = SenderName.substr(SenderName.indexOf(' ') + 1);
    $("#Nameq").val(Name); $("#LastNameq").val(Lastname); $("#Emailidq").val(SenderAddress);
}
//get Leads
function GetLeadsData() {
    $.getJSON("/CRM/LeadDatas", function (data) {
        $('#dt_tableExport').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "paging":   false,
            "ordering": false,
            "info":     false,
            "aaData": data,
            "aoColumns": [
                 {
                     "render": function (aaData, type, row, meta) {
                         return '<input type="checkbox" data-md-icheck class="check_row">';
                     }
                 },
                {
                    "render": function (aaData, type, row, meta) {
                        return '<a href="#mailbox_new_message" onclick="CustData(\'' + row.TC_CustomerId + '\');"  data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" data-uk-modal="{center:true}">' + row.Name + ' ' + row.LastName + '</a><input type="hidden" value="' + row.Name + ' ' + row.LastName + '">';
                    }
                },
                 {
                     "render": function (aaData, type, row, meta) {
                         return '<a ><i class="md-list-addon-icon material-icons">&#xE0CD;</i></a> ' + row.Contact + '<br> <a ><i class="md-list-addon-icon material-icons">&#xE158;</i></a> ' + row.EmailId + '<input type="hidden" value="' + row.EmailId + '">';
                     }
                 },

                 {
                     "render": function (aaData, type, row, meta) {
                         return '<a ><i class="uk-input-group-icon uk-icon-calendar"></i></a> ' + ConvertJsonDatetoanyformat(row.LeadCreationDate, "mm/dd/yyyy") + '<br><span class="uk-margin-right"><i class="material-icons"></i> <span class="uk-text-muted uk-text-small">' + ConvertJsonDatetoanyformat(row.LeadCreationDate, 'hh:mm ampm') + '</span></span>';
                     }
                 },

                { "mData": "CreatedBy" },
                  {
                      "render": function (aaData, type, row, meta) {

                          if (row.LeadType != null) {

                              return '<span class="uk-badge uk-badge-primary">' + row.LeadType + '</span>';
                          }
                          else {
                              return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                          }

                      }
                  },
                  {
                      "render": function (aaData, type, row, meta) {

                          if (row.InquiryType != null) {

                              return '<span class="uk-badge uk-badge-primary">' + row.InquiryType + '</span>';
                          }
                          else {
                              return '';
                          }

                      }
                  },
                  {
                      "render": function (aaData, type, row, meta) {

                          if (row.DisName != null) {

                              return '<span class="uk-badge uk-badge-primary">' + row.DisName + '</span>';
                          }
                          else {
                              return '';
                          }

                      }
                  },
                    {
                        "render": function (aaData, type, row, meta) {

                            if (row.NextFollowUpDate != null) {

                                return '<a ><i class="uk-input-group-icon uk-icon-calendar"></i></a> ' + ConvertJsonDatetoanyformat(row.NextFollowUpDate, "mm/dd/yyyy") + '<br><span class="uk-margin-right"><i class="material-icons"></i> <span class="uk-text-muted uk-text-small">' + ConvertJsonDatetoanyformat(row.NextFollowUpDate, 'hh:mm ampm') + '</span></span>';
                            }
                            else {
                                return '';
                            }

                        }
                    },
                     {
                         "render": function (aaData, type, row, meta) {

                             if (row.NextFollowUpAssinged != null) {

                                 return '<a >' + row.NextFollowUpAssinged + '</a> <br>' + row.ActionName + '';
                             }
                             else {
                                 return '';
                             }

                         }
                     },
                     {
                         "render": function (aaData, type, row, meta) {

                             if (row.AppointmentDate != null) {

                                 return '<a ><i class="uk-input-group-icon uk-icon-calendar"></i></a> ' + ConvertJsonDatetoanyformat(row.AppointmentDate, "mm/dd/yyyy") + '<br><span class="uk-margin-right"><i class="material-icons"></i> <span class="uk-text-muted uk-text-small">' + ConvertJsonDatetoanyformat(row.AppointmentDate, 'hh:mm ampm') + '</span></span>';
                             }
                             else {
                                 return '';
                             }

                         }
                     },
            {
                "render": function (aaData, type, row, meta) {
                    return '<a class="md-btn" href="#modal_large" onclick="SetData(\'' + row.TC_LeadId + '\',\'' + row.Contact + '\',\'' + row.TC_CustomerId + '\',\'' + row.Name + ' ' + row.LastName + '\');"  data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" data-uk-modal="{center:true}"><i class="md-list-addon-icon material-icons">&#xE163;</i></a>';


                }
            }           
            ]
        });
    })
}
//
function CustData(CustId) {
    document.getElementById("page_settings").reset();
    $("#lbltext").text("Edit Details"); $("#user_edit_save").css("display", "none"); $("#user_edit").css("display", "block"); $("#custid").val(CustId);
    $.getJSON("/CRM/GetCustDetails", { CustIds: CustId }, function (data) {
        $.each(data, function (i, Iteam) {      // bind the dropdown list using json result              
            $("#Name").val(Iteam.Name); $("#LastName").val(Iteam.LastName); $("#CompanyName").val(Iteam.CompanyName);
            $("#Designation").val(Iteam.Designation); $("#EmailId").val(Iteam.EmailId); $("#Contact").val(Iteam.Contact);
            $("#Address").val(Iteam.Address); $("#City").val(Iteam.City); $("#State").val(Iteam.State); $("#PinCode").val(Iteam.PinCode);

            if ($.trim(Iteam.Gender) == "Male") {
                $('input:radio[name=Gender]')[0].checked = true; $('input:radio[name=Gender]')[1].checked = false;
            }
            else if ($.trim(Iteam.Gender) == "Female") {
                $('input:radio[name=Gender]')[0].checked = false; $('input:radio[name=Gender]')[1].checked = true;
            }
            if (Iteam.LeadType != "" && Iteam.LeadType != null) {
                $("#ddLeadType").val(Iteam.LeadType).change();
                //$("#ddInquiryType").empty(); $("#ddLeadDisposition").empty();
                //GetInquiryType();               
            }
            if (Iteam.TC_InquiryTypeId != "" && Iteam.TC_InquiryTypeId != null) {
                //$("#ddLeadDisposition").empty();                 
                $("#ddInquiryType").val(Iteam.TC_InquiryTypeId).change();
                //GetLeadDisposition();
            }
            if (Iteam.TC_LeadDispositionId != "" && Iteam.TC_LeadDispositionId != null) {
                $("#ddLeadDisposition").val(Iteam.TC_LeadDispositionId).change();
            }
            if (Iteam.TC_InquirySourceId != "" && Iteam.TC_InquirySourceId != null) {
                $("#ddInquirySource").val(Iteam.TC_InquirySourceId).change();
            }
        });
    })
}
//get follow ups
function GetFollowUps() {
    $('#followups li').remove();
    $.getJSON("/CRM/FollowUpData", { LeadId: $("#TC_LeadId").val() }, function (data) {
        if (data == "") {
            $("#followups").append('<li>Not Yet Followed This Lead</li>');
        }
        $.each(data, function (i, iteam) {      // bind the dropdown list using json result              
            $("#followups").append('<li>\
                <div class="md-list-content">\
                 <span class="md-list-heading"><a >' + iteam.FollowUpAssinged + '</a> ' + iteam.PostActionCall + ' <b>' + iteam.ActionName + '</b></span>\
                <div class="uk-margin-small-top">\
               <span class="uk-margin-right">\
              <i class="material-icons">&#xE192;</i> <span class="uk-text-muted uk-text-small">' + ConvertJsonDatetoanyformat(iteam.FollowUpDate, 'mm/dd/yyyy hh:mm ampm') + '</span>\
             </span>\
             <span class="uk-margin-right">\
             <i class="material-icons">&#xE0B9;</i> <span class="uk-text-muted uk-text-small"><b>' + iteam.Title + ' </b>' + iteam.Comments + '</span>\
             </span>\
            </div>\
           </div>\
          </li>');
        });

    })
}
//
function SetData(LeadId, Contact, Id, Name) {
    document.getElementById("pageFollowup").reset();
    $("#txtName").text(Name); $("#contact").text(Contact);
    $("#TC_LeadId").val(LeadId); $("#TC_NextLeadId").val(LeadId); $("#AppointLeadId").val(LeadId);
    GetFollowUps();
}
//
function GetLeadType() {
    $.getJSON("/CRM/LeadType", function (data) {
        $.each(data, function (i, data) {      // bind the dropdown list using json result              
            $('<option>',
               {
                   value: data.TC_LeadTypeId,
                   text: data.LeadType
               }).html(data.LeadType).appendTo("#ddLeadType");

            //$('<option>',
            // {
            //     value: data.TC_LeadTypeId,
            //     text: data.LeadType
            // }).html(data.LeadType).appendTo("#ddLeadTypeq");
        });

        $('#ddLeadTypeq').kendoComboBox({
            dataTextField: "LeadType",
            dataValueField: "TC_LeadTypeId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    })
}
//
function GetFollowUpActions() {
    $.getJSON("/CRM/FollowUpActions", function (data) {       

        $('#DDAction').kendoComboBox({
            dataTextField: "ActionName",
            dataValueField: "TC_ActionId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });

        $('#DDNextAction').kendoComboBox({
            dataTextField: "ActionName",
            dataValueField: "TC_ActionId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    })
}
//
function GetInquirySource() {
    // Calling Controller
    $.getJSON("/CRM/InquirySource", function (data) {
        $.each(data, function (i, data) {// bind the dropdown list using json result              
            $('<option>',
               {
                   value: data.TC_InquirySourceId,
                   text: data.Source
               }).html(data.Source).appendTo("#ddInquirySource");
           
        });

        $('#ddInquirySourceq').kendoComboBox({
            dataTextField: "Source",
            dataValueField: "TC_InquirySourceId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });

    })
}
//
function GetInquiryType() {
    // Calling Controller
    $.getJSON("/CRM/InquiryType", { LeadTypeId: $("#ddLeadType option:selected").val() }, function (data) {
        $.each(data, function (i, data) {      // bind the dropdown list using json result              
            $('<option>',
               {
                   value: data.TC_InquiryTypeId,
                   text: data.InquiryType
               }).html(data.InquiryType).appendTo("#ddInquiryType");
        });
    })
}
//
function GetInquiryTypeq() {
    // Calling Controller
    $.getJSON("/CRM/InquiryType", { LeadTypeId: $("#ddLeadTypeq").val() }, function (data) {
        $('#ddInquiryTypeq').kendoComboBox({
            dataTextField: "InquiryType",
            dataValueField: "TC_InquiryTypeId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });      
    })
}
//
function GetLeadDisposition() {
    var s = $("#ddInquiryType option:selected").text();
    // Calling Controller
    $.getJSON("/CRM/LeadDisposition", { InquiryTypeId: $("#ddInquiryType option:selected").val() }, function (data) {
        $.each(data, function (i, data) {      // bind the dropdown list using json result              
            $('<option>',
               {
                   value: data.TC_LeadDispositionId,
                   text: data.Name
               }).html(data.Name).appendTo("#ddLeadDisposition");
        });
    })
}
//
function GetLeadDispositionq() {
    var s = $("#ddInquiryTypeq option:selected").text();
    // Calling Controller
    $.getJSON("/CRM/LeadDisposition", { InquiryTypeId: $("#ddInquiryTypeq").val() }, function (data) {
        $('#ddLeadDispositionq').kendoComboBox({
            dataTextField: "Name",
            dataValueField: "TC_LeadDispositionId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });       
    })
}
//
$(function () {
    // init settings
    altair_page_settings.init();

    //
    $("#btnmails").click(function () {
        //e.preventDefault();
        var TableData = []; //initialize array;
        var data = ""; //empty var;
        //Here traverse and  read input/select values present in each td of each tr, ;
        //"SenderAddress": $(this).find('td:eq(12)').html(),
        //"SenderName": $(this).find('td:eq(13)').html()
        $("table#dt_tableExport > tbody > tr:has(:checked)").each(function (row, tr) {
            TableData[row] = {
                "SenderName": $('td:eq(1) input', this).val(),
                "SenderAddress": $('td:eq(2) input', this).val()
                
            };
        });
        Edata = JSON.stringify(TableData);
        var body = $("#wysiwyg_ckeditor").val();
           encodeURIComponent(Edata);
           console.log(Edata);
           $.get("/Mail/demo", { JsonData: Edata, Message: encodeURIComponent(body), Subject: $("#mail_Sub").val() }, function (data) {
               MsgSucess("Notification: "+data);            
        })
        
    })

    //
    $("#ddLeadTypeq").change(function () {
        var selectedValue = $(this).val();
        if (selectedValue != "") {
            $("#ddInquiryTypeq").empty();
            $("#ddLeadDispositionq").empty();
            GetInquiryTypeq();
        }
        else {
            return false;
        }
    });

    //
    $("#ddLeadType").change(function () {
        var selectedValue = $(this).val();
        if (selectedValue != "") {
            $("#ddInquiryType").empty();
            $("#ddLeadDisposition").empty();
            GetInquiryType();
        }
        else {
            return false;
        }
    });

    //
    $("#ddInquiryType").change(function () {
        var selectedValue = $(this).val();
        if (selectedValue !== "" || selectedValue !== null) {
            $("#ddLeadDisposition").empty();
            GetLeadDisposition();
        }
        else {
            return false;
        }
    });
    //
    $("#ddInquiryTypeq").change(function () {
        var selectedValue = $(this).val();
        if (selectedValue !== "" || selectedValue !== null) {

            $("#ddLeadDispositionq").empty();
            GetLeadDispositionq();
        }
        else {
            return false;
        }
    });
    //
    $("#btnnewlead").click(function () {
        document.getElementById("page_settings").reset();
        $("#lbltext").text("New Lead");
        $("#user_edit_save").css("display", "block"); $("#user_edit").css("display", "none");
    })
    //
    $("#user_edit_delete").click(function () {
        document.getElementById("page_settings").reset();
        Msginfo("Notification: Form Reseted")
    })
    // save followup
    $('#saveleads').on('click', function (e) {
        e.preventDefault();
        var formData = JSON.stringify($("#pageFollowup").serializeObject(), null, 2);
        $.get("/CRM/LeadFollowup", { JsonData: formData }, function (data) {
            MsgSucess("Notification: " + data);
            document.getElementById("pageFollowup").reset();
            GetFollowUps();
        })
    })
    // Next followup
    $('#btnNextfollow').on('click', function (e) {
        e.preventDefault();
        var formData = JSON.stringify($("#formnextfollowup").serializeObject(), null, 2);
        $.get("/CRM/NextFollowup", { JsonData: formData }, function (data) {
            MsgSucess("Notification: " + data);
            document.getElementById("formnextfollowup").reset();
            GetLeadsData();
        })
    })
    // Appointment
    $('#btnappointment').on('click', function (e) {
        e.preventDefault();
        var formData = JSON.stringify($("#formappointment").serializeObject(), null, 2);
        $.get("/CRM/Appointment", { JsonData: formData }, function (data) {
            MsgSucess("Notification: " + data);
            document.getElementById("formappointment").reset();
            GetLeadsData();
        })
    })
    // Edit Data
    $('#user_edit').on('click', function (e) {
        e.preventDefault();
        var formData = JSON.stringify($("#page_settings").serializeObject(), null, 2);
        $.get("/CRM/EditData", { JsonData: formData, CustIds: $("#custid").val() }, function (data) {
            MsgSucess("Notification: " + data);
            document.getElementById("page_settings").reset();
            GetLeadsData();
        })
    })

    // Quick lead create
    $('#btnquickl').on('click', function (e) {
        e.preventDefault();
        var formData = JSON.stringify($("#quickdata").serializeObject(), null, 2);
        $.get("CRM/QuickLeadSave", { JsonData: formData }, function (data) {
            MsgSucess("Notification: " + data);
            document.getElementById("quickdata").reset();
            GetLeadsData();
        })
    })

    //
    $("#mbtnrefresh").click(function () {
        $('#maillist > li').remove();
        $("#processmail").css("display", "block");
        $("#mbtnrefresh").css("display", "none");
        Getmails();
    })

   
    $('#global_filter').on('keyup click', function () {
        var oTable;
        oTable = $('#dt_tableExport').dataTable();
        oTable.fnFilter($(this).val());
    });

    $('#header_main_search_input').on('keyup click', function () {
        var oTable;
        oTable = $('#dt_tableExport').dataTable();
        oTable.fnFilter($(this).val());
    });

});
//
altair_page_settings = {
    init: function () {
        var $settings_form = $('#page_settings');
        if ($settings_form.length) {
            // show serialized form
            $('#user_edit_save').on('click', function (e) {
                e.preventDefault();
                var form_serialized = JSON.stringify($settings_form.serializeObject(), null, 2);
                var url = "/CRM/Save";
                $.get(url, { Json: form_serialized }, function (data) {
                    if (data != null) {
                        MsgSucess("Notification: " + data);
                        document.getElementById("page_settings").reset();
                        GetLeadsData();
                    }
                    else {

                    }
                });
            })
        }
    }
};
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

