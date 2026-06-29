global using Microsoft.AspNetCore.Mvc;
global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.DependencyInjection;
global using Ricebird.Framework;
global using Ricebird.Framework.Clients;
global using Ricebird.Framework.Database;
global using Ricebird.Framework.Organizations;
global using Ricebird.Framework.Security;
global using Ricebird.Organizations.Models;
global using Ricebird.Organizations.Services;
global using System.ComponentModel.DataAnnotations.Schema;
global using static Ricebird.Framework.Utils;
global using static Ricebird.Organizations.OrganizationModule;

namespace Ricebird.Organizations
{
    internal class OrganizationModule : WebModule
    {
        public override string Name => "Ricebird.Organizations";

        public override int Priority => 70;

        public override string DisplayName => "组织机构模块";

        public override void Register(IServiceCollection services)
        {

        }

        public override void Use(WebApplication app)
        {

        }

        public const string MODULE_NAME = "组织机构模块";
        public const string SPECIAL_SCHEMA = "学院";
    }
}
