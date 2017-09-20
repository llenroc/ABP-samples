using System.ComponentModel.DataAnnotations;

namespace MyCompanyName.AbpZeroTemplate.Web.Models.Account
{
    public class EmailConfirmationViewModel
    {
        /// <summary>
        /// Encrypted user id.
        /// </summary>
        [Required]
        public string UserId { get; set; }

        [Required]
        public string ConfirmationCode { get; set; }
    }
}