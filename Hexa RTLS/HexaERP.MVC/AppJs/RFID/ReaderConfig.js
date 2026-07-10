/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:24-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    GetOrganization();
    $("#btnupdate").hide();
    //GetData();
});
function GetOrganization() {
    $.getJSON("../ReaderConfig/GetOrganizations", function (data) {
        $('#OrgInfoId').kendoComboBox({
            dataTextField: "OrgInfoName",
            dataValueField: "OrgInfoId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 3
        });
    });
}

//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var mFloorMasterId = $("#mFloorMasterIdi").val(); var OrgInfoId = $("#OrgInfoIdi").val();
        var ReaderNo = $("#ReaderNo").val(); var mReaderId = $("#mReaderId").val();
        var Vfd = $("#kUI_datetimepicker_range_start").val(); var Vtd = $("#kUI_datetimepicker_range_end").val();
        if (mFloorMasterId == "" || OrgInfoId == "" || ReaderNo == "" || Vfd == "" || Vtd == "" || mReaderId == "") {
        }
        else {
            i = $("#page_settings");
            var t = JSON.stringify(i.serializeObject(), null, 2);
            $.get("../ReaderConfig/SaveData", { formData: t }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                GetData();
            });
        }
    });
    $("#btnNew").click(function () {

    });
    //Update Data
    $("#btnupdate").click(function () {
        var ReaderNo = $("#ReaderNo").val();
        var Vfd = $("#kUI_datetimepicker_range_start").val();
        var Vtd = $("#kUI_datetimepicker_range_end").val(); var mReaderId = $("#mReaderIds").val();
        var ReaderIP = $("#ReaderIP").val();
        var mAttPortId = $("#mAttPortId").val();

        if (ReaderNo == "" || Vfd == "" || Vtd == "") {
        }
        else {
            $.get("../ReaderConfig/UpdateData", { mReaderId: mReaderId, ReaderNo: ReaderNo, Vfd: Vfd, Vtd: Vtd, ReaderIP: ReaderIP, mAttPortId: mAttPortId }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                $("#btnupdate").hide();
                $("#btnsave").show();
                GetData();

            });
        }
    });
    $("#OrgInfoId").change(function () {
        GetData();
    });
    //Save Data
    $("#btngetmac").click(function () {
        var ipda = $("#ReaderIP").val();
        if (ipda == "" || ipda == null) {
            alert("Enter Reader IP Address");
            return false;
        }
        else {
            $.get("../ReaderConfig/getReaderMac", { Ipaddress: $("#ReaderIP").val() }, function (data) {
                console.log(data);
                $("#ReaderNo").val(data);
            });
        }

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
    $.getJSON("../ReaderConfig/getData", { _orgId: $("#OrgInfoId").val() }, function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                  { "mData": "mReaderId" },
                 { "mData": "OrgInfoId" },
                { "mData": "OrgInfoName" },
                  { "mData": "mRoomMasterId" },
                  {
                      "render": function (aaData, type, row, meta) {

                          if (row.FloorName != null) {

                              return '<span class="uk-badge uk-badge-primary">' + row.FloorName + '</span>';
                          }
                          else {
                              return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                          }

                      }
                  },
                  {
                      "render": function (aaData, type, row, meta) {

                          if (row.FloorNo != null) {

                              return '<span class="uk-badge uk-badge-primary">' + row.FloorNo + '</span>';
                          }
                          else {
                              return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                          }

                      }
                  },
                   {
                       "render": function (aaData, type, row, meta) {

                           if (row.RoomName != null) {

                               return '<span class="uk-badge uk-badge-primary">' + row.RoomName + '</span>';
                           }
                           else {
                               return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                           }

                       }
                   },
                   {
                       "render": function (aaData, type, row, meta) {

                           if (row.RoomNo != null) {

                               return '<span class="uk-badge uk-badge-primary">' + row.RoomNo + '</span>';
                           }
                           else {
                               return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                           }

                       }
                   },
                   {
                       "render": function (aaData, type, row, meta) {

                           if (row.ReaderNo != null) {

                               return '<span class="uk-badge uk-badge-primary">' + row.ReaderNo + '</span>';
                           }
                           else {
                               return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                           }

                       }
                   },
                    {
                        "render": function (aaData, type, row, meta) {

                            if (row.ReaderIP != null) {

                                return '<span class="uk-badge uk-badge-primary">' + row.ReaderIP + '</span>';
                            }
                            else {
                                return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                            }

                        }
                    },
                    {
                        "render": function (aaData, type, row, meta) {

                            if (row.mAttPortId != null) {

                                return '<span class="uk-badge uk-badge-primary">' + row.mAttPortId + '</span>';
                            }
                            else {
                                return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                            }

                        }
                    },
                   {
                       "render": function (aaData, type, row, meta) {

                           if (row.Vfd != null) {

                               return '<span class="uk-badge uk-badge-primary">' + row.Vfd + '</span>';
                           }
                           else {
                               return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                           }

                       }
                   },
                    {
                        "render": function (aaData, type, row, meta) {

                            if (row.Vtd != null) {

                                return '<span class="uk-badge uk-badge-primary">' + row.Vtd + '</span>';
                            }
                            else {
                                return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                            }

                        }
                    },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a id="install" href="#mailbox_new_message" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Install" data-uk-modal="{center:true}" class="md-btn md-btn-primary md-btn-mini md-btn-wave-light md-btn-icon waves-effect waves-button waves-light"><i  class="material-icons">&#xE884;</i></a>';
                    }
                },
               {
                   'mRender': function (aaData, type, row, meta) {
                       return '<a href="#mailbox_new_message" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit" data-uk-modal="{center:true}"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a><i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                   }
               },
            ]
        });
    });
}
$(document).on('click', '#install', function (e) {
    var mReaderId = $(this).closest("tr").find('td:eq(0)').text();
    console.log(mReaderId);
    var OrgId = $(this).closest("tr").find('td:eq(1)').text();
    var FloorId = $(this).closest("tr").find('td:eq(3)').text();
    $("#mFloorMasterIdi").val(FloorId);
    $("#OrgInfoIdi").val(OrgId);
    $("#mReaderId").val(mReaderId);
    $("#btnupdate").hide();
    $("#btnsave").show();
    $("#lbltext").text("Install Reader Mac");

});
//Edit Data
$(document).on('click', '#Editbtn', function (e) {
    var Ids = $(this).closest("tr").find('td:eq(0)').text();
    var Confm = confirm('Do you want to Edit this Record?');
    if (Confm) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $("#lbltext").text("Edit Data");
        $.getJSON("../ReaderConfig/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#ReaderNo").val(item.ReaderNo);
                $("#kUI_datetimepicker_range_start").val(item.Vfd);
                $("#kUI_datetimepicker_range_end").val(item.Vtd);
                $("#mReaderIds").val(item.mReaderId);
                $("#ReaderIP").val(item.ReaderIP);
                $("#mAttPortId").val(item.mAttPortId);

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
        $.get("../ReaderConfig/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});