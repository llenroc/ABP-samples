(function () {
    appModule.controller('common.views.profile.changePicture', [
        '$scope', 'appSession', '$uibModalInstance', 'FileUploader',
        function ($scope, appSession, $uibModalInstance, fileUploader) {
            var vm = this;

            vm.uploader = new fileUploader({
                url: abp.appPath + 'Profile/ChangeProfilePicture',
                queueLimit: 1,
                filters: [{
                    name: 'imageFilter',
                    fn: function (item, options) {
                        //File type check
                        var type = '|' + item.type.slice(item.type.lastIndexOf('/') + 1) + '|';
                        if ('|jpg|jpeg|'.indexOf(type) === -1) {
                            abp.message.warn(app.localize('ProfilePicture_Warn_FileType'));
                            return false;
                        }

                        //File size check
                        if (item.size > 30720) //30KB
                        {
                            abp.message.warn(app.localize('ProfilePicture_Warn_SizeLimit'));
                            return false;
                        }

                        return true;
                    }
                }]
            });

            vm.save = function () {
                vm.uploader.uploadAll();
            };

            vm.cancel = function () {
                $uibModalInstance.dismiss();
            };

            vm.uploader.onSuccessItem = function (fileItem, response, status, headers) {
                $uibModalInstance.close();

                if (response.success) {
                    var profileFilePath = abp.appPath + 'Profile/GetProfilePicture?t=' + new Date().getTime();
                    $('#HeaderProfilePicture').attr('src', profileFilePath);
                } else {
                    abp.message.error(response.error.message);
                }
            };
        }
    ]);
})();