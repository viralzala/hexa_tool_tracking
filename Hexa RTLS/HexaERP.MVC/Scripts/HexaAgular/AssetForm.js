$(function () {

    altair_wizard.advanced_wizard(),
        altair_wizard.vertical_wizard()

}),

    altair_wizard = {
        content_height: function (t, i) {
            var e = $(t).find(".step-" + i).actual("outerHeight");
            $(t).children(".content").animate({ height: e }, 140, bez_easing_swiftOut)
        }, advanced_wizard: function () {
            var t = $("#wizard_advanced"),
                i = $("#wizard_advanced_form");
            t.length && (t.steps({
                headerTag: "h3", bodyTag: "section", transitionEffect: "slideLeft", trigger: "change", onInit: function (i, e) {
                    altair_wizard.content_height(t, e), altair_forms.textarea_autosize(), altair_md.checkbox_radio($(".wizard-icheck")),
                        $(".wizard-icheck").on("ifChecked", function (t) { console.log(t.currentTarget.value) }), altair_uikit.reinitialize_grid_margin(), altair_forms.select_elements(t), t.find("span.switchery").remove(), altair_forms.switches(), $(".uk-accordion").on("toggle.uk.accordion", function () { $window.resize() }), setTimeout(function () { $window.resize() }, 100)
                }, onStepChanged: function (i, e) {
                    altair_wizard.content_height(t, e), setTimeout(function () {
                        $window.resize()
                    }, 100)
                }, onStepChanging: function (i, e, n) {
                    var a = t.find(".body.current").attr("data-step"), r = $('.body[data-step="' + a + '"]');
                    return r.find(".parsley-row").each(function () {
                        $(this).find("input,textarea,select").each(function () {
                            $(this).parsley().validate()
                        })
                    }), $window.resize(), !r.find(".md-input-danger").length
                }, onFinished: function () {

                    modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
                    var iData = JSON.stringify(i.serializeObject(), null, 2);

                    var tAssetTagId = $("#tAssetTagId").val();

                    if (tAssetTagId === 'undefined' || tAssetTagId === null || tAssetTagId == "") {
                        $.ajax({
                            type: 'POST',
                            url: '../AssetTag/Create',
                            data: iData,
                            contentType: 'application/json; charset=utf-8',
                            dataType: "json",
                            success: function (response) {
                                //console.log(response);
                                if (response.Flag == true) {
                                    setTimeout(function () {
                                        modal.hide()
                                    }, 1000)
                                    toastr.success(response.Message);
                                    //window.location.href = response.redirectUrl; 
                                    document.getElementById("wizard_advanced_form").reset();
                                    BindJqueryTable();
                                } else {
                                    setTimeout(function () {
                                        modal.hide()
                                    }, 1000)
                                    toastr.error(response.Message);
                                }
                            },
                            failure: function (response) {
                                alert(response.responseText);
                            },
                            error: function (response) {
                                alert(response.responseText);
                            }
                        });
                    }
                    else if (tAssetTagId !== 'undefined' && tAssetTagId != null && tAssetTagId != "") {
                        var iData = JSON.stringify(i.serializeObject(), null, 2);
                        console.log(iData);
                        $.ajax({
                            type: 'POST',
                            url: '../AssetTag/Edit',
                            data: iData,
                            contentType: 'application/json; charset=utf-8',
                            dataType: "json",
                            success: function (response) {
                                if (response.Flag == true) {
                                    setTimeout(function () {
                                        modal.hide()
                                    }, 1000)
                                    toastr.success(response.Message);
                                    //window.location.href = response.redirectUrl; 
                                    document.getElementById("wizard_advanced_form").reset();
                                    BindJqueryTable();
                                } else {
                                    setTimeout(function () {
                                        modal.hide()
                                    }, 1000)
                                    toastr.error(response.Message);
                                }
                            },
                            failure: function (response) {
                                alert(response.responseText);
                            },
                            error: function (response) {
                                alert(response.responseText);
                            }
                        });

                    }

                }
            }), $window.on("debouncedresize", function () { var i = t.find(".body.current").attr("data-step"); altair_wizard.content_height(t, i) }), i.parsley().on("form:validated", function () { setTimeout(function () { altair_md.update_input(i.find(".md-input")), $window.resize() }, 100) }).on("field:validated", function (i) { var e = $(i.$element); setTimeout(function () { altair_md.update_input(e); var i = t.find(".body.current").attr("data-step"); altair_wizard.content_height(t, i) }, 100) }))
        }, vertical_wizard: function () { var t = $("#wizard_vertical"); t.length && t.steps({ headerTag: "h3", bodyTag: "section", enableAllSteps: !0, enableFinishButton: !1, transitionEffect: "slideLeft", stepsOrientation: "vertical", onInit: function (i, e) { altair_wizard.content_height(t, e) }, onStepChanged: function (i, e) { altair_wizard.content_height(t, e) } }) }
    };


function startImpinj() {
    UIkit.modal.confirm('Are you sure to Start Reader?', function () {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        $("#lblerr").text("");
        $.get("../AssetTag/ReaderInit", { Reader: $("#Reader").val() }, function (data) {
            //console.log(data);
            $("#lblerr").text(data);
            //$("#getRfid").show();
            setTimeout(function () {
                modal.hide()
            }, 2000)
        });
    });
};

function putPorts() {
    UIkit.modal.confirm('Are you sure to Connect Desktop Reader?', function () {
        modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
        var TapRfid = $("#ddTapRfid").data("kendoDropDownList");
        var _Portm = TapRfid.value();
        console.log(_Portm);
        $.get("../AssetTag/PutStart", { _Port: _Portm }, function (response) {
            //console.log(response);
            $("#errdtr").text(response.Msg);
            setTimeout(function () {
                modal.hide()
            }, 2000)
        });
    });
}
//
function clearImpinj() {
    $("#lblerr").text(""); $("#RFID").val(""); $("#PORTID").val(""); $("#global_filter").val("");
    $.get("../AssetTag/ReaderClear", function (data) {
        // console.log(data);
        $("#lblerr").text(data);
    });
}
//
function stopImpinj() {
    $("#lblerr").text("");
    $.get("../AssetTag/StopReaders", function (data) {
        $("#lblerr").text(data);
    });
}
//
function getImpinjRFID() {
    modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
    $.getJSON("../AssetTag/GetIds", function (data) {
        $("#RFID").val(); $("#PORTID").val();
        if (data.length === null || data.length <= 0) {
            setTimeout(function () {
                modal.hide()
            }, 1000)

            alert('Data Not Found');
            $("#lblerr").text('Data Not Found');

        } else {
            $.each(data, function (i, item) {
                $("#RFID").val(item.RFID);
                $("#global_filter").val(item.RFID);
                $("#PORTID").val(item.PORTID);
            });
            setTimeout(function () {
                modal.hide()
            }, 1000)
        }
    });
}

//
function DataTap() {
    modal = UIkit.modal.blockUI('<div class=\'uk-text-center\'>Wait moment...<br/><img class=\'uk-margin-top\' src=\'../Content/assets/img/spinners/spinner.gif\' alt=\'\'>');
    $.getJSON("../AssetTag/getDataTap", function (data) {
        $("#RFID").val(); $("#PORTID").val();
        if (data.Flag == false) {
            setTimeout(function () {
                modal.hide()
            }, 1000)
            alert(data.Msg);
            $("#errdtr").text(data.Msg);

        } else {
            $.each(data.Datas, function (i, item) {
                $("#RFID").val(item.RFID);
                $("#global_filter").val(item.RFID);
            });
            setTimeout(function () {
                modal.hide()
            }, 1000)
        }
    });
}

//
function BindJqueryTable() {
    $.get("../AssetTag/getData", function (data) {
        var table = $('#dt_tableExport').DataTable();
        table.clear().draw();
        $('#dt_tableExport').dataTable({
            "destroy": true,
            "bDestroy": true,
            "bProcessing": true,
            "aaData": data,
            "aoColumns": [
                { "mData": "tAssetTagId" },
                { "mData": "IteamName" },
                { "mData": "UID" },
                { "mData": "ModelNo" },
                { "mData": "IteamDescription" },
                {
                    "render": function (aaData, type, row, meta) {

                        if (row.RFID != null) {

                            return '<span class="uk-badge uk-badge-primary"><b>' + row.RFID + '</b></span>';
                        }
                        else {
                            return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                        }

                    }
                },
                { "mData": "BarCode" },


                {
                    "render": function (aaData, type, row, meta) {

                        if (row.bStock != null) {

                            return '<a href="#mailbox_new_message" onclick="setqty(\'' + row.tAssetTagId + '\',\'' + row.UID + '\');"  data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" data-uk-modal="{center:true}"><h3>' + parseFloat(row.bStock) + '</h3></a>';
                        }
                        else {
                            return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                        }

                    }
                },
                { "mData": "RoomName" },
                {
                    "render": function (aaData, type, row, meta) {

                        if (row.FloorName != null) {

                            return 'Rack:<b>' + row.FloorName + '</b><br/>Shelf:<b>' + row.RoomName + '</b>';
                        }
                        else {
                            return '<span class="uk-badge uk-badge-warning">Not Assined</span>';
                        }

                    }
                },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a href="/AssetTag/CarryParam/' + row.UID + '" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="More Detail"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE89C;</i></a>';
                    }
                },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a id="EditIdata" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Edit"> <i id="Editbtn" class="md-icon material-icons">&#xE254;</i></a>';
                    }
                },
                {
                    'mRender': function (aaData, type, row, meta) {
                        return '<a id="Deletebtn" data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete"><i data-uk-tooltip="{cls:"uk-tooltip-small",pos:"left"}" title="Delete" class="md-icon material-icons">&#xE872;</i></a>';
                    }
                }

            ]
        });
    });
};

//
$(function () {

    var oTable;
    oTable = $('#dt_tableExport').dataTable();
    $('#global_filter').on('keyup click', function () {
        oTable.fnFilter($(this).val());
    });

});

