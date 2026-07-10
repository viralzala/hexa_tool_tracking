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
        var IteamType = $("#IteamType").val();
        if (IteamType == "") {
        }
        else {
            i = $("#page_settings");
            var _formData = JSON.stringify(i.serializeObject(), null, 2);
            console.log(_formData);

            $.post("../AssetType/SaveData", { _IteamType: IteamType }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                GetData();
            });
        }
    });
    $("#btnNew").click(function () {
        $("#btnupdate").hide();
        $("#btnsave").show();
        $("#lbltext").text("Create New");
    });
    //Update Data
    $("#btnupdate").click(function () {
        var IteamType = $("#IteamType").val();
        if (IteamType == "") {
        }
        else {
            $.get("../AssetType/UpdateData", { _IteamType: IteamType, ID: $("#mIteamTypeMasterId").val() }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                $("#btnupdate").hide();
                $("#btnsave").show();
                GetData();
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
    $.getJSON("../AssetType/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "mIteamTypeMasterId" },
                { "mData": "IteamType" },
               {
                   'mRender': function (aaData, type, row, meta) {
                       return '<a id="btnNew" href="#mailbox_new_message" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Create New" data-uk-modal="{center:true}"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
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
        $.getJSON("/AssetType/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#mIteamTypeMasterId").val(item.mIteamTypeMasterId);
                $("#IteamType").val(item.IteamType);

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
        $.get("../AssetType/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});