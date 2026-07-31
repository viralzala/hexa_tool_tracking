/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:24-02-2017
///Description:
/// </summary>
$(document).ready(function () {
    $("#btnupdate").hide();
    GetData();
});

//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var GroupName = $("#GroupName").val();
        if (GroupName == "") {
        }
        else {
            i = $("#page_settings");
            var _formData = JSON.stringify(i.serializeObject(), null, 2);
            console.log(_formData);

            $.post("../AssetCategory/SaveData", { _GroupName: GroupName }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                GetData();
                alert(data);
            });
        }
    });
    $("#btnNew").click(function () {
        document.getElementById("page_settings").reset();
        $("#btnupdate").hide();
        $("#btnsave").show();
        $("#lbltext").text("Create New");
    });
    //Update Data
    $("#btnupdate").click(function () {
        var GroupName = $("#GroupName").val();
        if (GroupName == "") {
        }
        else {
            $.get("../AssetCategory/UpdateData", { _GroupName: GroupName, ID: $("#mGroupMasterId").val() }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                $("#btnupdate").hide();
                $("#btnsave").show();
                GetData();
                alert(data);
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
    $.getJSON("../AssetCategory/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "mGroupMasterId" },
                { "mData": "GroupName" },
               {
                   'mRender': function (aaData, type, row, meta) {
                        return '<i id="Editbtn" class="md-icon material-icons" style="cursor:pointer;" title="Edit">&#xE254;</i>  <i id="Deletebtn" class="md-icon material-icons" style="cursor:pointer;" title="Delete">&#xE872;</i>';
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
        $.getJSON("/AssetCategory/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#mGroupMasterId").val(item.mGroupMasterId);
                $("#GroupName").val(item.GroupName);

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
        $.get("../AssetCategory/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
            alert(data);
        });
    }
    else {
        console.log('cancel');
    }
});