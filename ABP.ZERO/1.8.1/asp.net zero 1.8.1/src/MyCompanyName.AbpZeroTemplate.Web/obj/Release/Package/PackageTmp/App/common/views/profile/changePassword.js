(function () {
    appModule.controller('common.views.profile.changePassword', [
        '$scope', 'appSession', '$uibModalInstance', 'abp.services.app.profile',
        function ($scope, appSession, $uibModalInstance, profileService) {
            var vm = this;

            vm.saving = false;
            vm.passwordInfo = null;

            vm.save = function () {
                vm.saving = true;
                profileService.changePassword(vm.passwordInfo)
                    .success(function () {
                        abp.notify.info(app.localize('YourPasswordHasChangedSuccessfully'));
                        $uibModalInstance.close();
                    }).finally(function () {
                        vm.saving = false;
                    });
            };

            vm.cancel = function () {
                $uibModalInstance.dismiss();
            };
        }
    ]);
})();