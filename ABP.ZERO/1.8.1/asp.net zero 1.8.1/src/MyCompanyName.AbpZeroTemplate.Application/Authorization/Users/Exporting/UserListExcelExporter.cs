using System.Collections.Generic;
using System.Linq;
using Abp.Collections.Extensions;
using MyCompanyName.AbpZeroTemplate.Authorization.Users.Dto;
using MyCompanyName.AbpZeroTemplate.DataExporting.Excel.EpPlus;
using MyCompanyName.AbpZeroTemplate.Dto;

namespace MyCompanyName.AbpZeroTemplate.Authorization.Users.Exporting
{
    public class UserListExcelExporter : EpPlusExcelExporterBase, IUserListExcelExporter
    {
        public FileDto ExportToFile(List<UserListDto> userListDtos)
        {
            return CreateExcelPackage(
                "UserList.xlsx",
                excelPackage =>
                {
                    var sheet = excelPackage.Workbook.Worksheets.Add(L("Users"));
                    sheet.OutLineApplyStyle = true;

                    AddHeader(
                        sheet,
                        L("Name"),
                        L("Surname"),
                        L("UserName"),
                        L("EmailAddress"),
                        L("EmailConfirm"),
                        L("Roles"),
                        L("LastLoginTime"),
                        L("Active"),
                        L("CreationTime")
                        );

                    AddObjects(
                        sheet, 2, userListDtos,
                        _ => _.Name,
                        _ => _.Surname,
                        _ => _.UserName,
                        _ => _.EmailAddress,
                        _ => _.IsEmailConfirmed,
                        _ => _.Roles.Select(r => r.RoleName).JoinAsString(", "),
                        _ => _.LastLoginTime,
                        _ => _.IsActive,
                        _ => _.CreationTime
                        );

                    //Formatting cells

                    var lastLoginTimeColumn = sheet.Column(7);
                    lastLoginTimeColumn.Style.Numberformat.Format = "mm-dd-yy";

                    var creationTimeColumn = sheet.Column(9);
                    creationTimeColumn.Style.Numberformat.Format = "mm-dd-yy";

                    for (var i = 1; i <= 7; i++)
                    {
                        sheet.Column(i).AutoFit();
                    }
                });
        }
    }
}
