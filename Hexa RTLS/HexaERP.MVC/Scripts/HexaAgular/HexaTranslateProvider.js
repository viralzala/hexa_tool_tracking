var app = angular.module('app', ['pascalprecht.translate', 'ui.router', 'ngCookies','ui.filters'])

app.config(['$stateProvider', '$urlRouterProvider', '$translateProvider', function ($stateProvider, $urlRouterProvider, $translateProvider) {
    //$translateProvider
    //    .useStaticFilesLoader({
    //        prefix: '../Scripts/LangFiles/',
    //        suffix: '.json'
    //    });
    //$translateProvider.preferredLanguage('en_US');
    //$scope._dir = "auto";

    //$translateProvider.useLocalStorage();
    //app.run(function ($rootScope, $translate) {
    //    $rootScope.$on('$translatePartialLoaderStructureChanged', function () {
    //        $translate.refresh();
    //    });
    //});
    //app.controller('ctrl', ['$scope', '$translate', function ($scope, $translate) {
    //    $scope.setLang = function (key) {
    //        $translate.use(key);
    //    };
    //}]);
}]).controller('ctrl', ['$scope', '$http', '$translate', function ($scope, $http, $translate) {
        //$scope.setLang = function (langKey) {
        //    // You can change the language during runtime
        //    $translate.use(langKey);
        //    if (langKey == "ar-AR") {
        //        $scope._dir = "rtl";
        //        console.log(langKey);
        //    }
        //    else if (langKey == "en_US") { $scope._dir = "ltr"; }
        //    else { $scope._dir = "ltr"; }
        //};
        angular.element(document).ready(function () {
            //$http.get("../AdminMaster/getCookie")
            //    .then(function (response) {
            //        $scope.iLogedUser = response.data.LogedUser;
            //    })
            //    .catch(function (error) {
            //        $scope.error = error;
            //    });
        });
}]);