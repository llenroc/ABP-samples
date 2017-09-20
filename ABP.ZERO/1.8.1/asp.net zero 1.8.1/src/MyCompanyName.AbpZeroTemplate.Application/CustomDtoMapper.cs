using AutoMapper;
using MyCompanyName.AbpZeroTemplate.Authorization.Users;
using MyCompanyName.AbpZeroTemplate.Authorization.Users.Dto;

namespace MyCompanyName.AbpZeroTemplate
{
    internal static class CustomDtoMapper
    {
        private static volatile bool _mappedBefore;
        private static readonly object SyncObj = new object();

        public static void CreateMappings()
        {
            lock (SyncObj)
            {
                if (_mappedBefore)
                {
                    return;
                }

                CreateMappingsInternal();

                _mappedBefore = true;
            }
        }

        private static void CreateMappingsInternal()
        {
            Mapper.CreateMap<User, UserEditDto>()
                .ForMember(dto => dto.Password, options => options.Ignore())
                .ReverseMap()
                .ForMember(user => user.Password, options => options.Ignore());
        }
    }
}