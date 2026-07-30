/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:24-02-2017
///Description: Asset Sub Category 1 Master
/// </summary>
$(document).ready(function () {
    $("#btnupdate").hide();
    GetData();
    LoadCategoryDropdown();
});

//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var IteamType = $("#IteamType").val();
        var GroupMasterId = $("#GroupMasterId").val();
        if (IteamType == "" || GroupMasterId == "") {
            alert("Please select Asset Category and enter Asset Sub Category 1");
        }
        else {
            i = $("#page_settings");
            var _formData = JSON.stringify(i.serializeObject(), null, 2);
            console.log(_formData);

            $.post("../AssetType/SaveData", { _IteamType: IteamType, _GroupMasterId: GroupMasterId }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                GetData();
                LoadCategoryDropdown();
                alert(data);
            });
        }
    });
    $("#btnNew").click(function () {
        $("#btnupdate").hide();
        $("#btnsave").show();
        $("#lbltext").text("Create New");
        LoadCategoryDropdown();
    });
    //Update Data
    $("#btnupdate").click(function () {
        var IteamType = $("#IteamType").val();
        var GroupMasterId = $("#GroupMasterId").val();
        if (IteamType == "" || GroupMasterId == "") {
            alert("Please select Asset Category and enter Asset Sub Category 1");
        }
        else {
            $.get("../AssetType/UpdateData", { _IteamType: IteamType, _GroupMasterId: GroupMasterId, ID: $("#mIteamTypeMasterId").val() }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                $("#btnupdate").hide();
                $("#btnsave").show();
                GetData();
                LoadCategoryDropdown();
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

//Load Asset Category Dropdown
function LoadCategoryDropdown() {
    $.getJSON("../AssetType/getAssetCategoryList", function (data) {
        $('#GroupMasterId').kendoDropDownList({
            dataTextField: "GroupName",
            dataValueField: "mGroupMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 2
        });
        var GroupMasterId = $("#GroupMasterId").data("kendoDropDownList");
        if (GroupMasterId) {
            GroupMasterId.value(-1);
        }
    });
}

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
                { "mData": "GroupName" },
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
        LoadCategoryDropdown();
        $.getJSON("/AssetType/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#mIteamTypeMasterId").val(item.mIteamTypeMasterId);
                $("#IteamType").val(item.IteamType);

                // Set Category dropdown
                var categoryDropdown = $("#GroupMasterId").data("kendoDropDownList");
                if (categoryDropdown && item.mGroupMasterId) {
                    categoryDropdown.value(item.mGroupMasterId);
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
        $.get("../AssetType/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
            alert(data);
        });
    }
    else {
        console.log('cancel');
    }
});