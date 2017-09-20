(function () {
    appModule.controller('common.views.roles.createOrEditModal', [
        '$scope', '$uibModalInstance', 'abp.services.app.role', 'roleId',
        function ($scope, $uibModalInstance, roleService, roleId) {
            var vm = this;

            vm.saving = false;
            vm.role = null;
            vm.permissionEditData = null;

            vm.save = function () {
                vm.saving = true;
                roleService.createOrUpdateRole({
                    role: vm.role,
                    grantedPermissionNames: vm.permissionEditData.grantedPermissionNames
                }).success(function () {
                    abp.notify.info(app.localize('SavedSuccessfully'));
                    $uibModalInstance.close();
                }).finally(function() {
                    vm.saving = false;
                });
            };

            vm.cancel = function () {
                $uibModalInstance.dismiss();
            };

            function init() {
                roleService.getRoleForEdit({
                    id: roleId
                }).success(function (result) {
                    vm.role = result.role;
                    vm.permissionEditData = {
                        permissions: result.permissions,
                        grantedPermissionNames: result.grantedPermissionNames
                    };
                });
            }

            init();
        }
    ]);
})();