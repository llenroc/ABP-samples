using Abp.AutoMapper;
using MyCompanyName.AbpZeroTemplate.MultiTenancy.Payments;

namespace MyCompanyName.AbpZeroTemplate.Sessions.Dto
{
    [AutoMapFrom(typeof(SubscriptionPayment))]
    public class SubscriptionPaymentInfoDto
    {
        public decimal Amount { get; set; }
    }
}