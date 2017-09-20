using System.Collections.Generic;
using System.Linq;
using Abp.Localization;
using MyCompanyName.AbpZeroTemplate.EntityFramework;

namespace MyCompanyName.AbpZeroTemplate.Migrations.Seed
{
    public class DefaultLanguagesCreator
    {
        public static List<ApplicationLanguage> InitialLanguages { get; private set; }

        private readonly AbpZeroTemplateDbContext _context;

        static DefaultLanguagesCreator()
        {
            InitialLanguages = new List<ApplicationLanguage>
            {
                new ApplicationLanguage(null, "en", "English", "famfamfam-flag-gb"),
                new ApplicationLanguage(null, "ar", "العربية", "famfamfam-flag-sa"),
                new ApplicationLanguage(null, "de", "German", "famfamfam-flag-de"),
                new ApplicationLanguage(null, "pt-BR", "Portuguese", "famfamfam-flag-br"),
                new ApplicationLanguage(null, "tr", "Türkçe", "famfamfam-flag-tr"),
                new ApplicationLanguage(null, "zh-CN", "简体中文", "famfamfam-flag-cn")
            };
        }

        public DefaultLanguagesCreator(AbpZeroTemplateDbContext context)
        {
            _context = context;
        }

        public void Create()
        {
            CreateLanguages();
        }

        private void CreateLanguages()
        {
            foreach (var language in InitialLanguages)
            {
                AddLanguageIfNotExists(language);
            }
        }

        private void AddLanguageIfNotExists(ApplicationLanguage language)
        {
            if (_context.Languages.Any(l => l.TenantId == language.TenantId && l.Name == language.Name))
            {
                return;
            }

            _context.Languages.Add(language);

            _context.SaveChanges();
        }
    }
}