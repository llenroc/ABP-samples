using EntityFramework.DynamicFilters;
using MyCompanyName.AbpZeroTemplate.EntityFramework;

namespace MyCompanyName.AbpZeroTemplate.Tests.TestDatas
{
    public class TestDataBuilder
    {
        private readonly AbpZeroTemplateDbContext _context;
        private readonly int _tenantId;

        public TestDataBuilder(AbpZeroTemplateDbContext context, int tenantId)
        {
            _context = context;
            _tenantId = tenantId;
        }

        public void Create()
        {
            _context.DisableAllFilters();

            new TestOrganizationUnitsBuilder(_context, _tenantId).Create();

            _context.SaveChanges();
        }
    }
}
