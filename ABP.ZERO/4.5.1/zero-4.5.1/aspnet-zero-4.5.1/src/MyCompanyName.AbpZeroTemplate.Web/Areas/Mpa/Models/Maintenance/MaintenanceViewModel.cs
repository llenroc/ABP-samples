using System.Collections.Generic;
using MyCompanyName.AbpZeroTemplate.Caching.Dto;

namespace MyCompanyName.AbpZeroTemplate.Web.Areas.Mpa.Models.Maintenance
{
    public class MaintenanceViewModel
    {
        public IReadOnlyList<CacheDto> Caches { get; set; }
    }
}