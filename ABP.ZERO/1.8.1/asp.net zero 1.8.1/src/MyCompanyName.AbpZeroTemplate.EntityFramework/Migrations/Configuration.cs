using System.Data.Entity.Migrations;
using MyCompanyName.AbpZeroTemplate.Migrations.Seed;

namespace MyCompanyName.AbpZeroTemplate.Migrations
{
    internal sealed class Configuration : DbMigrationsConfiguration<AbpZeroTemplate.EntityFramework.AbpZeroTemplateDbContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "AbpZeroTemplate";
        }

        protected override void Seed(AbpZeroTemplate.EntityFramework.AbpZeroTemplateDbContext context)
        {
            new InitialDbBuilder(context).Create();
        }
    }
}
