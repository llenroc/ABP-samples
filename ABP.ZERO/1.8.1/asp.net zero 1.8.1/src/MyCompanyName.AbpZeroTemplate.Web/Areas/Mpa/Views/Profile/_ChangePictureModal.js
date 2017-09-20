(function($) {
    app.modals.ChangeProfilePictureModal = function () {

        var _modalManager;

        this.init = function(modalManager) {
            _modalManager = modalManager;

            $('#ChangeProfilePictureModalForm').ajaxForm({
                beforeSubmit: function (formData, jqForm, options) {

                    var $fileInput = $('#ChangeProfilePictureModalForm input[name=ProfilePicture]');
                    var files = $fileInput.get()[0].files;

                    if (!files.length) {
                        return false;
                    }

                    var file = files[0];

                    //File type check
                    var type = '|' + file.type.slice(file.type.lastIndexOf('/') + 1) + '|';
                    if ('|jpg|jpeg|'.indexOf(type) === -1) {
                        abp.message.warn(app.localize('ProfilePicture_Warn_FileType'));
                        return false;
                    }

                    //File size check
                    if (file.size > 30720) //30KB
                    {
                        abp.message.warn(app.localize('ProfilePicture_Warn_SizeLimit'));
                        return false;
                    }

                    return true;
                },
                success: function (response) {
                    _modalManager.close();

                    if (response.success) {
                        var profileFilePath = abp.appPath + 'Profile/GetProfilePicture?t=' + new Date().getTime();
                        $('#HeaderProfilePicture').attr('src', profileFilePath);
                    } else {
                        abp.message.error(response.error.message);
                    }
                }
            });
        };

        this.save = function () {
            $('#ChangeProfilePictureModalForm').submit();
        };
    };
})(jQuery);