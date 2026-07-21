/// <summary>
///Author: Automation
///Created Date: 2026
///Description: Asset Sub Category 2 Master
/// </summary>
$(document).ready(function () {
    $("#btnupdate").hide();
    GetData();
    LoadCategoryDropdown();
    LoadSubCategoryDropdown();
});

//
$(function () {
    //Save Data
    $("#btnsave").click(function () {
        var AssetSubCategory2Name = $("#AssetSubCategory2Name").val();
        var AssetSubCategoryId = $("#AssetSubCategoryId").val();
        var Description = $("#Description").val();
        if (AssetSubCategory2Name == "" || AssetSubCategoryId == "") {
            alert("Please fill all required fields");
        }
        else {
            $.post("../AssetSubCategory2/SaveData", { _AssetSubCategory2Name: AssetSubCategory2Name, _AssetSubCategoryId: AssetSubCategoryId, _Description: Description }, function (data) {
                console.log(data);
                document.getElementById("page_settings").reset();
                GetData();
                alert(data);
            });
        }
    });
    $("#btnNew").click(function () {
        $("#btnupdate").hide();
        $("#btnsave").show();
        $("#lbltext").text("Create New");
        LoadCategoryDropdown();
        LoadSubCategoryDropdown();
    });
    //Update Data
    $("#btnupdate").click(function () {
        var AssetSubCategory2Name = $("#AssetSubCategory2Name").val();
        var AssetSubCategoryId = $("#AssetSubCategoryId").val();
        var Description = $("#Description").val();
        if (AssetSubCategory2Name == "" || AssetSubCategoryId == "") {
            alert("Please fill all required fields");
        }
        else {
            $.get("../AssetSubCategory2/UpdateData", { _AssetSubCategory2Name: AssetSubCategory2Name, _AssetSubCategoryId: AssetSubCategoryId, _Description: Description, ID: $("#AssetSubCategory2Id").val() }, function (data) {
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

//Load Category Dropdown
function LoadCategoryDropdown() {
      console.log("LoadCategoryDropdown Called");

    $.getJSON("../AssetTag/getMasterData", function (data) {
        console.log("Response :", data);

        var categories = (data.ObjGroup || []).slice();
        $('#GroupMasterId').kendoDropDownList({
            dataTextField: "GroupName",
            dataValueField: "mGroupMasterId",
            filter: "contains",
            dataSource: categories,
            suggest: true,
            index: 2,
            change: onCategoryChange
        });
        var GroupMasterId = $("#GroupMasterId").data("kendoDropDownList");
        if (GroupMasterId) {
            GroupMasterId.value(-1);
        }
    });
}

function onCategoryChange(e) {
    var categoryId = this.value();
    var subCategory1 = $("#AssetSubCategoryId").data("kendoDropDownList");
    if (subCategory1 && categoryId) {
        $.getJSON("../AssetSubCategory2/getSubCategory1ByCategory", { categoryId: categoryId }, function (data) {
            if (data && data.Flag) {
                subCategory1.setDataSource(data.DSubCategory1 || []);
                subCategory1.value(-1);
            }
        });
    } else if (subCategory1) {
        subCategory1.setDataSource([]);
        subCategory1.value(-1);
    }
}

//Load Sub Category Dropdown
function LoadSubCategoryDropdown() {
    $.getJSON("../AssetSubCategory2/getSubCategoryList", function (data) {
        $('#AssetSubCategoryId').kendoDropDownList({
            dataTextField: "IteamType",
            dataValueField: "mIteamTypeMasterId",
            filter: "contains",
            dataSource: data,
            suggest: true,
            index: 2
        });
        var AssetSubCategoryId = $("#AssetSubCategoryId").data("kendoDropDownList");
        if (AssetSubCategoryId) {
            AssetSubCategoryId.value(-1);
        }
    });
}

//Get Data
function GetData() {
    var table = $('#tbl').DataTable();
    table.clear().draw();
    $.getJSON("../AssetSubCategory2/getData", function (data) {
        $('#tbl').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                { "mData": "AssetSubCategory2Id" },
                { "mData": "AssetCategory" },
                { "mData": "SubCategoryName" },
                { "mData": "AssetSubCategory2Name" },
                { "mData": "Description" },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a id="btnNew" href="#mailbox_new_message" data-uk-tooltip="{cls:\"uk-tooltip-small\",pos:\"left\"}" title="Create New" data-uk-modal="{center:true}"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
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
        LoadSubCategoryDropdown();
        $.getJSON("/AssetSubCategory2/getDataWithId", { ID: Ids }, function (data) {
            $.each(data, function (i, item) {
                $("#AssetSubCategory2Id").val(item.AssetSubCategory2Id);
                $("#AssetSubCategory2Name").val(item.AssetSubCategory2Name);
                $("#Description").val(item.Description);
                
                // Set Category first
                var categoryDropdown = $("#GroupMasterId").data("kendoDropDownList");
                if (categoryDropdown && item.AssetCategory) {
                    var categoryItem = categoryDropdown.dataSource.view().find(function(c) { return c.GroupName === item.AssetCategory; });
                    if (categoryItem) {
                        categoryDropdown.value(categoryItem.mGroupMasterId);
                        onCategoryChange.call(categoryDropdown);
                    }
                }
                
                // Set Sub Category 1 after category cascade loads
                setTimeout(function() {
                    var subCategory1Dropdown = $("#AssetSubCategoryId").data("kendoDropDownList");
                    if (subCategory1Dropdown) {
                        subCategory1Dropdown.value(item.AssetSubCategoryId);
                    }
                }, 100);
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
        $.get("../AssetSubCategory2/DeleteData", { ID: Ids }, function (data) {
            console.log(data);
            GetData();
            alert(data);
        });
    }
    else {
        console.log('cancel');
    }
});
