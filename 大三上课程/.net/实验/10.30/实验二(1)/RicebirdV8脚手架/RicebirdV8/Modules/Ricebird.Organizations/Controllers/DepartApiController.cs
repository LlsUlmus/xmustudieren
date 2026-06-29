using Ricebird.Framework.DataValidator;

namespace Ricebird.Organizations.Controllers;

[Route("~/api/depart/[action]"), ApiGroup("部门管理")]
public class DepartApiController(IOrgService orgService) : ApiController
{
    // 必须转换，注册在 IOrgService 和 注册在OrgService 上的并不是同一个单例！
    public OrgService OrgService { get; set; } = (orgService as OrgService)!;

    [ApiShouldLogin("获取组织机构树")]
    public ActionResult GetDepartTree()
    {
        Guid parentId = Get(nameof(parentId), Guid.Empty);
        string category = Get("cate", string.Empty);
        bool force = Get("force", false) && CurrentUser.Succeed(Permissions.ReloadDeparments);
        (bool success, string msg, object tree) = OrgService.GetDepartTree(category, parentId, force, Client);
        return Ok(new
        {
            success,
            msg,
            tree
        });
    }

    [ApiLinkTo("获取部门节点", "/manage/security/departs")]
    public ActionResult GetDepart()
    {
        Guid id = Get("id", Guid.Empty);

        if (id == Guid.Empty)
        {
            var parentNodes = OrgService.AllDepartments.Where(e => e.ParentId == Guid.Empty).Select(e => e).ToList();
            return Ok(new
            {
                success = true,
                msg = "",
                data = new Department()
                {
                    ID = Guid.Empty,
                    Name = "所有部门",
                    Source = "系统生成"
                },
                children = parentNodes
            });
        }

        IDepart? depart = orgService.GetDepartById(id);
        if (depart is not Department dpt)
        {
            return Ok(new
            {
                success = false,
                msg = "找不到ID对应的部门",
                id
            });
        }

        var validator = dpt.BuildValidator();
        var children = dpt.Children.Select(e => new
        {
            e.ID,
            e.Name,
            e.SchemaName,
            e.DisplayOrder,
            e.Source,
        });

        return Ok(new
        {
            success = true,
            msg = "",
            data = depart,
            children,
            rules = validator.ToJsonObject()
        });
    }

    [ApiShouldAuthorize("保存部门")]
    public ActionResult SaveDepart()
    {
        (_, _, ValidateResult result, IDepart? data) = orgService.SaveDepartment(Client, false);

        if (!result)
        {
            return ValidateError(result);
        }

        Client.Log(MODULE_NAME, "SaveDepart", data?.ID.ToString() ?? "", $"保存了{data!}");

        return Ok(new
        {
            success = true,
            msg = "",
            data
        });
    }

    [ApiShouldAuthorize("删除部门")]
    public ActionResult RemoveDepart()
    {
        Guid id = Get("id", Guid.Empty);
        orgService.RemoveDepartment(id, Client);
        Client.Log(MODULE_NAME, "RemoveDepart", id, $"删除了ID为{id}的部门");

        return Ok(new
        {
            success = true,
            msg = ""
        });
    }

    [ApiShouldAuthorize("批量处理部门")]
    public ActionResult MoveDepartment()
    {
        Guid to = Get(nameof(to), Guid.Empty);
        string opera = Get(nameof(opera), "copy");
        Guid srcDepartId = Get(nameof(srcDepartId), Guid.Empty);
        List<Guid> value = GetList<Guid>("value");

        IDepart? cate = OrgService.AllDepartments.FirstOrDefault(e => e.ID == to);
        if (to != Guid.Empty && cate == null)
        {
            return Ok(new
            {
                success = false,
                msg = "找不到目标部门",
                to
            });
        }

        if (to != Guid.Empty && opera == "cut" && OrgService.AllDepartments.Where(e => value.Contains(e.ID)).Any(x => x.ID == to || (x as Department)!.AllChildren.Any(y => y.ID == to)))
        {
            return Ok(new
            {
                success = false,
                msg = "剪切的目标位置不能是源位置，也不能在源位置的内部。",
                to
            });
        }

        if (value.Count == 0)
        {
            return Ok();
        }

        var (success, msg, affectRows) = OrgService.MoveDepartment(to, value, opera, Client);

        string toCate = cate?.Name ?? "根部门";
        string desc = opera switch
        {
            "cut" => $"将id为“{value.JoinAsString("，")}”的部门剪切至“{toCate}”部门下",
            "copy" => $"将id为“{value.JoinAsString("，")}”的部门复制至“{toCate}”部门下",
            "delete" => $"删除id为“{value.JoinAsString("，")}”的部门",
            _ => "未知的操作"
        };
        Client.Log(MODULE_NAME, "MoveDepartment", to, desc);

        //if (opera == "delete")
        //{
        //    List<Guid> affects = affectRows.Select(e => e.ID).ToList();
        //    // 把被影响的新闻全部处理一遍
        //    var repo = Resolve<ArticleRepository>();
        //    repo.DbSet
        //    .Where(e => affects.Contains(e.ID))
        //    .ExecuteUpdate(set => set
        //        .SetProperty(e => e.CategoryId, v => Guid.Empty)
        //    );
        //    repo.SaveChanges();
        //}

        return Ok(new
        {
            success,
            msg,
        });
    }
} // class

