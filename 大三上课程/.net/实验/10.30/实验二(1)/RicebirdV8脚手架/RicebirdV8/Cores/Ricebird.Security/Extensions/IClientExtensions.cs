namespace Ricebird.Framework.Clients
{
    public static class IClientExtensions
    {
        public static void EnsureCreateRoleSchema(this IClient client, string Name, RuleFor For, string DisplayAs, AuthorizeResult NotSetEquals, bool isDefault, int displayOrder = 0)
        {
            RoleService rService = client.Resolve<RoleService>();
            RoleSchema schema = new RoleSchema()
            {
                Name = Name,
                For = For,
                DisplayAs = DisplayAs,
                NotSetEquals = NotSetEquals,
                DisplayOrder = displayOrder,
                CanEdit = false,
                IsDefaultRole = isDefault,
                UseAsPrincipal = true,
            };

            rService.EnsureRoleSchema(client, schema);
        }
    }
}
