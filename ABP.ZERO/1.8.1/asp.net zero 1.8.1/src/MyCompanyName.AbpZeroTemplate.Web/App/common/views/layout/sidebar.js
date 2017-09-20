(function () {
    appModule.controller('common.views.layout.sidebar', [
        '$scope',
        function ($scope) {
            var vm = this;

            vm.menu = abp.nav.menus.MainMenu;

            $scope.$on('$includeContentLoaded', function () {
                Layout.initSidebar(); // init sidebar
            });
        }
    ]);
})();