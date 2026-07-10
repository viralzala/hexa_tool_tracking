/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:08-03-2017
///Description:
/// </summary>
$(document).ready(function () {
    //Call the yourAjaxCall() function every 1000 millisecond    
    GetData(); 
});
function GetData() {
    $.getJSON("../MonthReport/getMonthsReport", function (data) {

    });
}