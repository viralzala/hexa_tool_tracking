/// <summary>
///Author: Mr. Mudassar A. Inamdar
///Created Date:12-29-2016
///Description:
/// </summary>

//==== To show data when page initially loads.
$(document).ready(function () {
    //Dynamic Column creation and dataSet binding:     
    
});


$(function () {
    $("#btnlogin").click(function (e) {
        e.preventDefault();      
        var username = $("#login_username").val(); var password = $("#login_password").val();
        var url = "/AppUser/Login";
        if (username == "" || password == "") {
            return false;
        }
        $.post(url, { UserName: username, Password: password }, function (data) {
            if (data == 0) {                        
                UIkit.modal.alert('<p>Log In Failed:</p><pre>Verify User & Password</pre>');
            }
           else if (data == 1) {
               UIkit.modal.alert('<p>Log In Failed:</p><pre>Enter Your UserName/Password</pre>');
            }
            else {      
               
                window.location = data;                            
            }
          
        });
    });   
});