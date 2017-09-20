using Abp.AutoMapper;
using MyCompanyName.AbpZeroTemplate.Editions.Dto;
using MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Models.Common;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Models.Editions
{
    [AutoMapFrom(typeof(GetEditionForEditOutput))]
    public class CreateOrEditEditionModalViewModel : GetEditionForEditOutput, IFeatureEditViewModel
    {
        public bool IsEditMode
        {
            get { return Edition.Id.HasValue; }
        }

        public CreateOrEditEditionModalViewModel(GetEditionForEditOutput output)
        {
            output.MapTo(this);
        }
    }
}