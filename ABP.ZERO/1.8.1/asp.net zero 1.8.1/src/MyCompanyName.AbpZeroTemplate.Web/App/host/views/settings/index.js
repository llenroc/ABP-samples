(function() {
    appModule.controller('host.views.settings.index', [
        '$scope', 'abp.services.app.hostSettings',
        function($scope, hostSettingsService) {
            var vm = this;

            $scope.$on('$viewContentLoaded', function () {
                App.initAjax();
            });

            vm.loading = false;
            vm.settings = null;

            vm.getSettings = function() {
                vm.loading = true;
                hostSettingsService.getAllSettings()
                    .success(function(result) {
                        vm.settings = result;
                    }).finally(function() {
                        vm.loading = false;
                    });
            };

            vm.saveAll = function() {
                hostSettingsService.updateAllSettings(
                    vm.settings
                ).success(function() {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                });
            };

            vm.getSettings();
        }
    ]);
})();