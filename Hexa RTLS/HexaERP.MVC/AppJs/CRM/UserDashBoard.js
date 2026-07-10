// ** Mudassar I **
//

app.controller("UserHomeCtrl", function ($scope, $http, $timeout) {
    initializeComponets();
    //
    function initializeComponets() {
        $.getJSON("../UserHome/getCookie", function (data) {
            $scope.iLogedUser = data.LogedUser;
        });
    }   
    // 
    function getMenus() {
       

        $http({
            method: 'GET',
            url: '../UserHome/GetAllMenus'
        }).then(function successCallback(response) {
            // this callback will be called asynchronously
            // when the response is available        
            console.log('this callback will be called asynchronously');
            $scope.MainMenus = response.data.reduceRight(function (r, a) {
                r.some(function (b) { return a.moduleName === b.moduleName; }) || r.push(a);
                return r;
            }, []);
            $scope.menuIdata = response.data;
        }, function errorCallback(response) {
            // called asynchronously if an error occurs
            // or server returns response with an error status.
            console.log("Error : " + response.data.ExceptionMessage);
        });
    }
});




//$(document).ready(function () {

//});
//function getallmodules() {
//    //Mudassar I Edited On 30/01/2017
//    $.getJSON("../AdminMaster/getallmodules", function (data) {

//        $.each(data, function (i, obj) {          
//            $("#Modellist").append('<a class="uk-margin-top" onclick="mclick(' + obj.RolemoduleId + ')">\
//                                            <i class="material-icons md-36 md-color-cyan-600">&#xE8D8;</i>\
//                                            <span class="uk-text-muted uk-display-block" >' + obj.moduleName + '</span>\
//                                        </a>');

//        });
//    });
//}

////Mudassar I added On 30/01/2017
//function mclick(RolemoduleId) {
//    $.getJSON("../AdminMaster/getallwindows", { moduleid: RolemoduleId }, function (data) {    
//        $('#menulink').html('');
//        $.each(data, function (i, obj) {
//            $('#menulink').append('<li><a href=' + obj.MenuUrl + '>' + obj.MenuName + '</a></li>');
//        });
//    });
//}
