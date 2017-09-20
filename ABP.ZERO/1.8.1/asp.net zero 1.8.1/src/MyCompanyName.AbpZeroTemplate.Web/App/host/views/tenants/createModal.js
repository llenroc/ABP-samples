(function () {
    appModule.controller('host.views.tenants.createModal', [
        '$scope', '$uibModalInstance', 'abp.services.app.tenant', 'abp.services.app.commonLookup',
        function ($scope, $uibModalInstance, tenantService, commonLookupService) {
            var vm = this;

            vm.saving = false;
            vm.setRandomPassword = true;
            vm.editions = [];
            vm.tenant = {
                isActive: true,
                shouldChangePasswordOnNextLogin: true,
                sendActivationEmail: true,
                editionId: 0
            };

            vm.getEditionValue = function (item) {
                return parseInt(item.value);
            };

            vm.save = function () {
                if (vm.setRandomPassword) {
                    vm.tenant.adminPassword = null;
                }

                if (vm.tenant.editionId == 0) {
                    vm.tenant.editionId = null;
                }

                vm.saving = true;
                tenantService.createTenant(vm.tenant)
                    .success(function () {
                        abp.notify.info(app.localize('SavedSuccessfully'));
                        $uibModalInstance.close();
                    }).finally(function () {
                        vm.saving = false;
                    });
            };

            vm.cancel = function () {
                $uibModalInstance.dismiss();
            };

            commonLookupService.getEditionsForCombobox({}).success(function (result) {
                vm.editions = result.items;
                vm.editions.unshift({ value: "0", displayText: app.localize('NotAssigned') });
            });
        }
    ]);
})();