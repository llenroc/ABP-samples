namespace MyCompanyName.AbpZeroTemplate.Debugging
{
    public static class DebugHelper
    {
        public static bool IsDebug
        {
            get
            {
#if DEBUG
                return true;
#endif
                return false;
            }
        }
    }
}
