/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:07-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    GetData();
    $("#btnupdate").hide();
});
//
$(function () {
    //Save Data
    $("#btnSave").click(function () {
        var formData = JSON.stringify($("#rmailsetup").serializeObject(), null, 2);
        //console.log(formData);
        $.ajax({
            type: "POST",
            url: "../EmailConfiguration/Config",
            data: formData,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                //console.log(response);
                if (response.Flag == true) {
                    GetData();
                    document.getElementById("rmailsetup").reset();
                    toastr.success(response.Message);
                } else { toastr.error(response.Message); }

            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });

        //$.get("../EmailConfiguration/Save", { Json: formData }, function (data) {
        //    GetData();
        //    document.getElementById("rmailsetup").reset();
        //});
    });
    //Update Data
    $("#btnupdate").click(function () {
        var formDatas = JSON.stringify($("#rmailsetup").serializeObject(), null, 2);
        //console.log(formDatas);

        $.ajax({
            type: "POST",
            url: "../EmailConfiguration/UpdateConfig",
            data: formDatas,
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                //console.log(response);
                if (response.Flag == true) {
                    GetData();
                    document.getElementById("rmailsetup").reset();
                    toastr.success(response.Message);
                } else { toastr.error(response.Message); }

            },
            failure: function (response) {
                alert(response.responseText);
            },
            error: function (response) {
                alert(response.responseText);
            }
        });

        //$.get("../EmailConfiguration/UpdateData", { formData: formDatas, ID: $("#EmailConfigurationId").val() }, function (data) {
        //    console.log(data);
        //    GetData();
        //    document.getElementById("rmailsetup").reset();
        //    $("#btnupdate").hide(); $("#btnSave").show();
        //});
    });

    $("#tst").click(function () {
        var dropdownlist = $("#TC_InquiryTypeId").data("kendoDropDownList");
        dropdownlist.value(2);
    });
});
//Get Data
function GetData() {
    $.getJSON("../EmailConfiguration/getData", function (data) {
        //console.log(data);
        $('#tblData').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                { "mData": "mMailConfigId" },
                  {
                      "render": function (aaData, type, row, meta) {
                          if (row.IsOut == true) {

                              return '<span class="uk-badge uk-badge-warning">' + row.EmailId + '</span>';
                          }
                          else {
                              return '<span class="uk-badge uk-badge-primary">' + row.EmailId + '</span>';
                          }

                      }
                  },
                { "mData": "EmailServer" },
                 { "mData": "DisplayName" },
                  {
                      "render": function (aaData, type, row, meta) {
                          if (row.IsOut == true) {

                              return '<i class="md-icon material-icons">check_circle</i>';
                          }
                          else {
                              return '';
                          }

                      }
                  },

                      {
                          "render": function (aaData, type, row, meta) {
                              if (row.IsAction == true) {

                                  return '<span class="uk-text-success">' + row.IsAction + '</span>';
                              }
                              else {
                                  return '<span class="uk-text-danger">' + row.IsAction + '</span>';
                              }

                          }
                      },
                      {
                          "render": function (aaData, type, row, meta) {
                              if (row.IsAdd == true) {

                                  return '<i class="md-icon material-icons">check_circle</i>';
                              }
                              else {
                                  return '<i class="md-icon material-icons">highlight_off</i>';
                              }

                          }
                      },
                      {
                          "render": function (aaData, type, row, meta) {
                              if (row.IsModify == true) {

                                  return '<i class="md-icon material-icons">check_circle</i>';
                              }
                              else {
                                  return '<i class="md-icon material-icons">highlight_off</i>';
                              }

                          }
                      },
                       {
                           "render": function (aaData, type, row, meta) {
                               if (row.IsDelete == true) {

                                   return '<i class="md-icon material-icons">check_circle</i>';
                               }
                               else {
                                   return '<i class="md-icon material-icons">highlight_off</i>';
                               }

                           }
                       },
                       {
                           "render": function (aaData, type, row, meta) {
                               if (row.IsNotify == true) {

                                   return '<i class="md-icon material-icons">check_circle</i>';
                               }
                               else {
                                   return '<i class="md-icon material-icons">highlight_off</i>';
                               }

                           }
                       },

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
        $("#btnSave").hide();
        $("#btnupdate").show();
        $.getJSON("../EmailConfiguration/getDataWithId", { ID: Ids }, function (data) {

            //console.log(data);
            $.each(data, function (i, item) {
                // console.log(item.mMailConfigId);
                $("#mMailConfigId").val(item.mMailConfigId);
                $("#EmailId").val(item.EmailId);
                $("#EmailServer").val(item.EmailServer);
                $("#DisplayName").val(item.DisplayName);
                $("#Password").val(item.Password);
                $("#Port").val(item.Port);

                if (item.IsOut === true) {
                    $('input:radio[name=IsOut]')[0].checked = true; $('input:radio[name=IsOut]')[1].checked = false;
                }
                else if (item.IsOut === false) {
                    $('input:radio[name=IsOut]')[0].checked = false; $('input:radio[name=IsOut]')[1].checked = true;
                }

                if (item.IsAdd === true) {
                    $('input:radio[name=IsAdd]')[0].checked = true; $('input:radio[name=IsAdd]')[1].checked = false;
                }
                else if (item.IsAdd === false) {
                    $('input:radio[name=IsAdd]')[0].checked = false; $('input:radio[name=IsAdd]')[1].checked = true;
                }

                if (item.IsModify === true) {
                    $('input:radio[name=IsModify]')[0].checked = true; $('input:radio[name=IsModify]')[1].checked = false;
                }
                else if (item.IsModify === false) {
                    $('input:radio[name=IsModify]')[0].checked = false; $('input:radio[name=IsModify]')[1].checked = true;
                }

                if (item.IsDelete === true) {
                    $('input:radio[name=IsDelete]')[0].checked = true; $('input:radio[name=IsDelete]')[1].checked = false;
                }
                else if (item.IsDelete === false) {
                    $('input:radio[name=IsDelete]')[0].checked = false; $('input:radio[name=IsDelete]')[1].checked = true;
                }

                if (item.IsSSL === true) {
                    $('input:radio[name=IsSSL]')[0].checked = true; $('input:radio[name=IsSSL]')[1].checked = false;
                }
                else if (item.IsSSL === false) {
                    $('input:radio[name=IsSSL]')[0].checked = false; $('input:radio[name=IsSSL]')[1].checked = true;
                }

                if (item.IsAction === true) {
                    $('input:radio[name=IsAction]')[0].checked = true; $('input:radio[name=IsAction]')[1].checked = false;
                }
                else if (item.IsAction === false) {
                    $('input:radio[name=IsAction]')[0].checked = false; $('input:radio[name=IsAction]')[1].checked = true;
                }

                if (item.IsNotify === true) {
                    $('input:radio[name=IsNotify]')[0].checked = true; $('input:radio[name=IsNotify]')[1].checked = false;
                }
                else if (item.IsNotify === false) {
                    $('input:radio[name=IsNotify]')[0].checked = false; $('input:radio[name=IsNotify]')[1].checked = true;
                }

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
        $.get("../EmailConfiguration/DeleteData", { ID: Ids }, function (data) {
            //console.log(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});