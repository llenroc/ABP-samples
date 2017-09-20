using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using Abp.Auditing;
using Abp.Domain.Uow;
using Abp.Extensions;
using Abp.IO.Extensions;
using Abp.Runtime.Session;
using Abp.UI;
using Abp.Web.Models;
using Abp.Web.Mvc.Authorization;
using Abp.Web.Mvc.Models;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Net.MimeTypes;
using MyCompanyName.AbpZeroTemplate.Storage;

namespace MyCompanyName.AbpZeroTemplate.Web.Controllers
{
    [AbpMvcAuthorize]
    public class ProfileController : AbpZeroTemplateControllerBase
    {
        private readonly UserManager _userManager;
        private readonly IBinaryObjectManager _binaryObjectManager;

        public ProfileController(UserManager userManager, IBinaryObjectManager binaryObjectManager)
        {
            _userManager = userManager;
            _binaryObjectManager = binaryObjectManager;
        }

        [DisableAuditing]
        public async Task<FileResult> GetProfilePicture()
        {
            var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());
            if (user.ProfilePictureId == null)
            {
                return GetDefaultProfilePicture();
            }

            return await GetProfilePictureById(user.ProfilePictureId.Value);
        }

        [DisableAuditing]
        public async Task<FileResult> GetProfilePictureById(string id = "")
        {
            if (id.IsNullOrEmpty())
            {
                return GetDefaultProfilePicture();                
            }

            return await GetProfilePictureById(Guid.Parse(id));
        }

        [UnitOfWork]
        public async virtual Task<JsonResult> ChangeProfilePicture()
        {
            try
            {
                //Check input
                if (Request.Files.Count <= 0 || Request.Files[0] == null)
                {
                    throw new UserFriendlyException(L("ProfilePicture_Change_Error"));
                }

                var file = Request.Files[0];

                if (file.ContentLength > 30720) //30KB.
                {
                    throw new UserFriendlyException(L("ProfilePicture_Warn_SizeLimit"));
                }

                //Get user
                var user = await _userManager.GetUserByIdAsync(AbpSession.GetUserId());

                //Delete old picture
                if (user.ProfilePictureId.HasValue)
                {
                    await _binaryObjectManager.DeleteAsync(user.ProfilePictureId.Value);
                }

                //Save new picture
                var storedFile = new BinaryObject(file.InputStream.GetAllBytes());
                await _binaryObjectManager.SaveAsync(storedFile);

                //Update new picture on the user
                user.ProfilePictureId = storedFile.Id;

                //Return success
                return Json(new MvcAjaxResponse());
            }
            catch (UserFriendlyException ex)
            {
                //Return error message
                return Json(new MvcAjaxResponse(new ErrorInfo(ex.Message)));
            }
        }

        private FileResult GetDefaultProfilePicture()
        {
            return File(Server.MapPath("~/Common/Images/default-profile-picture.png"), MimeTypeNames.ImagePng);
        }

        private async Task<FileResult> GetProfilePictureById(Guid profilePictureId)
        {
            var file = await _binaryObjectManager.GetOrNullAsync(profilePictureId);
            if (file == null)
            {
                return GetDefaultProfilePicture();
            }

            return File(file.Bytes, MimeTypeNames.ImageJpeg);
        }
    }
}