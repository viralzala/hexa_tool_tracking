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
        var IteamName = $("#IteamName").val();
        var IteamCode = $("#IteamCode").val();
        if (IteamName == "" || IteamCode == "") {
        }
        else {
            i = $("#page_settings");
            var _formData = JSON.stringify(i.serializeObject(), null, 2);
            console.log(_formData);

            $.post("../AssetMaster/SaveData", { _IteamName: IteamName, _IteamCode: IteamCode }, function (data) {
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
        var IteamName = $("#IteamName").val(); var IteamCode = $("#IteamCode").val();
        if (IteamName == "" || IteamCode == "") {
        }
        else {
            $.get("../AssetMaster/UpdateData", { _IteamName: IteamName, _IteamCode: IteamCode, ID: $("#mIteamMasterId").val() }, function (data) {
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
    $.getJSON("../AssetMaster/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                 { "mData": "mIteamMasterId" },
                { "mData": "IteamName" },
                 { "mData": "IteamCode" },
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
        $.getJSON("/AssetMaster/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#mIteamMasterId").val(item.mIteamMasterId);
                $("#IteamName").val(item.IteamName);
                $("#IteamCode").val(item.IteamCode);

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
        $.get("../AssetMaster/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
        });
    }
    else {
        console.log('cancel');
    }
});