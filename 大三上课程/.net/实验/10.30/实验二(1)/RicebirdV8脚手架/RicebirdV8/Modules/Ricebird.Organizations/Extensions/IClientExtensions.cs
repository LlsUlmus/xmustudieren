namespace Ricebird.Organizations.Extensions
{
    public static class IClientExtensions
    {
        public static void EnsureCreateDepartment(this IClient client, string name, string schema)
        {
            Department department = new Department()
            {
                Name = name,
                SchemaName = schema,
                Source = "系统生成",
                ParentId = Guid.Empty,
            };

            OrgService.EnsureCreateDepartment(client, department);
        }

        //public static void EnsureCreateUser(this IClient client, string name, string code, string mobile, string email, string depart, string role)
        //{
        //    User user = new User()
        //    {
        //        RealName = name,
        //        Code = code,
        //        Mobile = mobile,
        //        Email = email,
        //        Level = AccessLevel.AllAccess
        //    };
        //}
    }
}
