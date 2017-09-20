(function () {
    appModule.controller('common.views.profile.mySettingsModal', [
        '$scope', 'appSession', '$uibModalInstance', 'abp.services.app.profile',
        function ($scope, appSession, $uibModalInstance, profileService) {
            var vm = this;

            vm.saving = false;
            vm.user = null;
            vm.canChangeUserName = true;

            vm.save = function () {
                vm.saving = true;
                profileService.updateCurrentUserProfile(vm.user)
                    .success(function () {
                        appSession.user.name = vm.user.name;
                        appSession.user.surname = vm.user.surname;
                        appSession.user.userName = vm.user.userName;
                        appSession.user.emailAddress = vm.user.emailAddress;

                        abp.notify.info(app.localize('SavedSuccessfully'));

                        $uibModalInstance.close();
                    }).finally(function () {
                        vm.saving = false;
                    });
            };

            vm.cancel = function () {
                $uibModalInstance.dismiss();
            };

            function init() {
                profileService.getCurrentUserProfileForEdit({
                    id: appSession.user.id
                }).success(function (result) {
                    vm.user = result;
                    vm.canChangeUserName = vm.user.userName != app.consts.userManagement.defaultAdminUserName;
                });
            }

            init();
        }
    ]);
})();