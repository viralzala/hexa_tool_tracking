
//Mudassar I Added On 03/02/2017

$(document).ready(function () {
    GetDepartments();
    $("#btnupdate").hide();
});
//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var DepartMentName = $("#DepartMentName").val();
        if (DepartMentName == "") {
        }
        else {           
            $.get("../DepartmentMaster/SaveDepartment", { DepartMentName: DepartMentName }, function (data) {
                console.log(data);
                GetDepartments();
            });
        }
    });
    //Update Data
    $("#btnupdate").click(function () {
        var DepartMentName = $("#DepartMentName").val();
        if (DepartMentName == "") {
        }
        else {
            $.get("../DepartmentMaster/UpdateDepartment", { DepartMentName: DepartMentName, ID: $("#DepartMentID").val() }, function (data) {
                console.log(data);
                $("#DepartMentName").val(""); $("#DepartMentID").val("");
                GetDepartments();
            });
        }
    });

    var oTable;
    oTable = $('#tblDept').dataTable();
    $('#global_filter').on('keyup click', function () {
        oTable.fnFilter($(this).val());
    });
});

//Get Data
function GetDepartments() {
    $.getJSON("../DepartmentMaster/getDepartment", function (data) {
        $('#tblDept').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                { "mData": "DepartMentID" },
                 { "mData": "DepartMentName" },

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
    var DeptId = $(this).closest("tr").find('td:eq(0)').text();
    var Confm = confirm('Do you want to Edit this Record?');
    if (Confm) {
        $("#btnsave").hide();
        $("#btnupdate").show();
        $.getJSON("/DepartmentMaster/getDepartmentWithId", { DeptId: DeptId }, function (data) {
            $.each(data, function (i, item) {
                $("#DepartMentName").val(item.DepartMentName);
                $("#DepartMentID").val(item.DepartMentID);
                ///$("#moduleIDid").val(item.moduleID).change();
            });
        });
    }
    else {        
    }
});

//Delete
$(document).on('click', '#Deletebtn', function (e) {
    var DeptId = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        $.get("../DepartmentMaster/DeleteDepartment", { ID: DeptId }, function (data) {
            console.log(data);          
            GetDepartments();
        });
    }
    else {
        console.log('cancel');
    }
});