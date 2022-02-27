using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OliveToast.Utilities
{
    public class Exceptions
    {

        public class RequirePermissionException : Exception
        {
            public const string Emoji = "<:PermissionDenied:907628900266434620>";

            public RequirePermissionException(RequirePermission.PermissionType perm) : base($"{Emoji} 이 커맨드를 실행하려면 {RequirePermission.PermissionToString(perm)} 권한이 필요해요\n권한 설정 커맨드를 사용해보세요")
            {

            }
        }

        public class CategoryNotEnabledException : Exception
        {
            public const string Emoji = "<:CategoryNotEnabled:907628900564209705>";

            public CategoryNotEnabledException(RequireCategoryEnable.CategoryType cat) : base($"{Emoji} 이 커맨드를 실행하려면 {RequireCategoryEnable.CategoryToString(cat)} 타입의 활성화가 필요해요\n`{CommandEventHandler.prefix}활성화 {RequireCategoryEnable.CategoryToString(cat).을를("`")} 입력해보세요")
            {

            }
        }
    }
}
