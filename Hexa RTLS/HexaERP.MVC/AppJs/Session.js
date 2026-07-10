/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:12-30-2016
///Description:
/// </summary>

//==== To show data when page initially loads.
$(document).ready(function () {
    //Dynamic Column creation and dataSet binding:
});

$(function () {
    $(".btnlogout").click(function (e) {
        e.preventDefault();     
        var url = "/AppUser/Logout";
        $.get(url, function (data) {                       
            window.location = data;           
        });
    });
});