using Ricebird.Framework;
using Ricebird.Framework.Clients;
using Ricebird.Framework.Security;
using Ricebird.Organizations.Extensions;
using Ricebird.Organizations.Models;

// Add services to the container.
var builder = WebApplication.CreateBuilder(args);
HostEnv hostEnv = builder.Services.AddHostEnv();
builder.Services.AddSignalR();

//#if DEBUG
//builder.Services
//    .AddControllersWithViews()
//    .AddRazorRuntimeCompilation();
//#endif

WebApplication app = builder.Build();
hostEnv.WriteLog("初始化", $"框架构建成功，开始运行服务器");

// ASP.NET CORE 推荐管道顺序：错误处理器，HSTS，Https重定向，静态文件，路由，CORS，认证，授权，终结点
// 米雀框架管道顺序：错误处理器，（HSTS，Https重定向），静态文件，路由，CORS，客户端构建，认证，授权，终结点
// Configure the HTTP request pipeline.
if (!hostEnv.IsDevelopment())
{
    app.UseExceptionHandler("/404");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
else
{
    app.UseDeveloperExceptionPage();
}

if (hostEnv.FrameworkOptions.UseHttps)
{
    hostEnv.WriteLog("StartUp", "正在启用https强制转换模式");
    app.UseHttpsRedirection();
}

// 这里的顺序必须是这样的，ScheduleStorage 是有可能需要用户验证的
app.UseStaticFiles();
app.UseFileStorage();
app.UseRicebirdModules();
app.UseScheduleStorage();
hostEnv.WriteLog("初始化", $"所有模块已经加载完毕");

#region EnsureCreate
using (var scope = app.Services.CreateScope())
{
    IClient client = scope.CreateClient("EnsureCreate");
    // 确认必须存在的数据字典
    client.EnsureCreateDataDictionary("用户来源", dict =>
    {
        dict.CreateEntries("任意来源", "后台添加", "自行注册", "数据同步", "微信注册", "统一认证", "后台导入", "系统生成");
    });

    client.EnsureCreateDataDictionary("部门类型", dict =>
    {
        dict.CreateEntry("普通部门");
        dict.CreateEntry("学院");
    });

    client.EnsureCreateDataDictionary("证件类型", dict =>
    {
        dict.CreateEntries("学工号", "居民身份证", "外国护照", "中国护照", "港澳居民来往内地通行证", "台湾居民来往大陆通行证", "外国人永久居留身份证");
    });

    // 确认必须存在的角色
    client.EnsureCreateRoleSchema("系统管理员", RuleFor.User, "管理员", AuthorizeResult.Access, false, -100);
    client.EnsureCreateRoleSchema("游客", RuleFor.User, "游客", AuthorizeResult.Deny, true, 99999);

    // 确认必须存在的部门
    client.EnsureCreateDepartment("厦门大学", "普通部门");

    // 清理所有不合理的用户关系，数据库里查询语句如下
    RelationshipRepository rr = client.Resolve<RelationshipRepository>();
    rr.ClearRelationships();

    var reslut = client.GetDbDiagnostic();
    hostEnv.WriteLog("初始化", $"已确保所有内容全部生成，执行查询{reslut.SqlCount}次，总耗时{reslut.TotalMilliseconds}ms");
}
#endregion

app.UseRouting();

if (hostEnv.IsDevelopment())
{
    app.UseCors(ConstKeys.CorsAny);
}
else
{
    app.UseCors();
}
app.UseOutputCache();
app.MapControllers();
app.MapFallbackToFile("index.html");

hostEnv.InitialEnd();
app.Run();
