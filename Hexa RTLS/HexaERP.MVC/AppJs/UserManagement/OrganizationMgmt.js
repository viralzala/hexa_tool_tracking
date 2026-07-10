$(document).ready(function () {
    BindOrganizationDatatable();
    $("#btnUpdateOrganization").hide();
});

function SaveOrganization() {
    var org = $("#OrgInfoName").val();
    var product = $("#ProductService").val();
    var adress = $("#Adress").val();
    if (org == "" || product == "" || adress == "") {
        alert('Fileds Are Null');
    }
    else {
        var formData = JSON.stringify($("#Organizationform").serializeObject(), null, 2);
        $.get("../OrganizationManagement/SaveOrganization", { JsonData: formData }, function (data) {
            $("#OrgInfoId111").val("");
            $("#OrgInfoName").val("");
            $("#ProductService").val("");
            $("#Adress").val("");
            BindOrganizationDatatable();
        })
    }
}
function UpdateOrganization() {
    var org = $("#OrgInfoName").val();
    var product = $("#ProductService").val();
    var adress = $("#Adress").val();
    if (org == "" || product == "" || adress == "") {
    }
    else {
        var formData = JSON.stringify($("#Organizationform").serializeObject(), null, 2);
        var orghidenid = $("#OrgInfoIdhidden").val();
        $.get("../OrganizationManagement/UpdateOrganization", { JsonData: formData, orghidenid: orghidenid }, function (data) {
            BindOrganizationDatatable();
            $("#OrgInfoId111").val("");
            $("#OrgInfoName").val("");
            $("#ProductService").val("");
            $("#Adress").val("");
            $("#btnUpdateOrganization").hide();
            $("#btnSaveOrganization").show();
        })
    }
}
function CancelOrganization() {
    window.location = "../../ViewPage/OrganizationReg.aspx";
}
function BindOrganizationDatatable() {
    $.ajax({
        type: "POST",
        url: "../OrganizationManagement/BindOrganizationDatatable",
        data: "{}",
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        async: "true",
        error: function (response) { },
        success: function (response) {
            var parseJSONResult = response;
            $('#exampleorganizationdata').dataTable({
                "bDestroy": true,
                "bProcessing": true,
                "aaData": parseJSONResult,// <-- your array of objects
                "aoColumns": [
                    { "mData": "OrgInfoId" },
                     { "mData": "OrgInfoName" },
                    { "mData": "ProductService" },
                     { "mData": "Adress" },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<i id="Editbtn"  class="md-icon material-icons">&#xE254;</i>  <i id="Deletebtn" class="md-icon material-icons">&#xE872;</i>';
                    }
                },
                {
                    'mRender': function (aaData, type, row, meta) {
                      
                        return '<i id="IdPass"  data-uk-modal="{target:"#modal_header_footer"}" class="md-icon material-icons">&#xE8F4;</i> ';
                        //return '<button class="md-btn" data-uk-modal="{target:"#modal_header_footer"}">Password</button>';
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

$(document).on('click', '#IdPass', function (e) {
    var organizationid = $(this).closest("tr").find('td:eq(0)').text();    
    var modal = UIkit.modal("#modal_header_footer");
    modal.show();
    $.getJSON("../OrganizationManagement/getorganizationPass", { OrgId: organizationid }, function (data) {        
        $.each(data, function (i, Iteam) {          
            $("#txtOrgp").text(Iteam.Password);           
        });
    });
});

$(document).on('click', '#Editbtn', function (e) {
    var organizationid = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to Edit this Record?');
    if (answer) {
        $("#btnSaveOrganization").hide();
        $("#btnUpdateOrganization").show();
        $.getJSON("../OrganizationManagement/getorganization", { organizationid: organizationid }, function (data) {
            $.each(data, function (i, data) {
                $("#OrgInfoIdhidden").val(data.OrgInfoId);
                $("#OrgInfoName").val(data.OrgInfoName);
                $("#ProductService").val(data.ProductService);
                $("#Adress").val(data.Adress);
            });
        });
    }
    else {
        console.log('cancel');
    }
});
$(document).on('click', '#Deletebtn', function (e) {
    var organizationid = $(this).closest("tr").find('td:eq(0)').text();
    var answer = confirm('Do you want to delete this Record?');
    if (answer) {
        console.log('yes');
        $.ajax({
            type: "POST",
            url: "../OrganizationManagement/Deleteorganization",
            data: "{organizationid:'" + organizationid + "'}",
            contentType: "application/json; charset=utf-8",
            datatype: "jsondata",
            async: "true",
            success: function (response) {
                BindOrganizationDatatable();
            },
            error: function (response) {
            }
        });
    }
    else {
        console.log('cancel');
    }
});


