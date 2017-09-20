namespace MyCompanyName.AbpZeroTemplate.Web
{
    public interface IWebUrlService
    {
        string GetSiteRootAddress(string tenancyName = null);
    }
}
