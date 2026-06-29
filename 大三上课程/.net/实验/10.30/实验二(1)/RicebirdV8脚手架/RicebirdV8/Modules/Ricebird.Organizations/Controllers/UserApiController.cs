using Ricebird.Framework.SystemExtensions;
using Ricebird.Security.Services;
using System.Collections.Frozen;
namespace Ricebird.Organizations.Controllers
{
    [Route("~/api/users/[action]"), ApiGroup("用户管理")]
    public class UserApiController(UserRepository UserRepository, IOrgService orgService, HostEnv hostEnv) : ApiController
    {
        #region 用户增删查改
        [ApiLinkTo("查询用户", "/manage/security/users")]
        public ActionResult GetUsers()
        {
            bool protect = true;
            if (CurrentUser.Succeed(Permissions.InformationProtected))
            {
                protect = Get("proctected", true);
            }

            UserSearcher searcher = Client.FillResolveObject<UserSearcher>();

            var (totalRow, page, pageSize, query) = searcher.BuildPaginationQuery();
            var data = query
                .Select(e => new
                {
                    e.ID,
                    e.RealName,
                    e.Code,
                    e.Mobile,
                    e.Email
                })
                .ToList();

            if (protect)
            {
                data = data.Select(e => new
                {
                    e.ID,
                    e.RealName,
                    e.Code,
                    Mobile = ProctectMobile(e.Mobile),
                    Email = ProctectEmail(e.Email),
                }).ToList();
            }

            return Ok(new
            {
                success = true,
                msg = "",
                totalRow,
                page,
                pageSize,
                data
            });
        }

        [ApiLinkTo("获取用户", "/manage/security/users")]
        public ActionResult GetUser()
        {
            Guid id = Get("id", Guid.Empty);
            var user = UserRepository.GetUser(id);
            if (user == null)
            {
                return Ok(new
                {
                    success = false,
                    msg = $"找不到ID对应的用户",
                    id
                });
            }

            var rules = user.BuildValidator();

            return Ok(new
            {
                success = true,
                msg = "",
                data = user,
                rules = rules.ToJsonObject(),
                //roles
            }, "yyyy-MM-dd");
        }

        [ApiShouldAuthorize("保存用户")]
        public ActionResult SaveUser()
        {
            #region 读取用户
            bool addFlag = false;
            Guid id = Get("ID", Guid.Empty);
            User? user;
            if (id == Guid.Empty)
            {
                addFlag = true;
                user = new User()
                {
                    UserSource = "后台添加",
                    Level = AccessLevel.Max,
                };
                UserRepository.DbSet.Add(user);
            }
            else
            {
                user = UserRepository.GetUser(id);
                if (user == null)
                {
                    return Fail(new
                    {
                        success = false,
                        msg = "该用户已经被删除",
                        errorStrings = "该用户已经被删除"
                    });
                }
            }
            #endregion

            string[] basicInfo = [nameof(user.RealName), nameof(user.Code), nameof(user.CodeType), nameof(user.Mobile), nameof(user.Email), nameof(user.UserName),
                                   nameof(user.AuditStatus), nameof(user.UserSource), nameof(user.Gender), nameof(user.Birthday)];
            string[] studentInfo = [nameof(user.Grade), nameof(user.Major), nameof(user.Department), nameof(user.GraduadeTime), nameof(user.ReformType), nameof(user.Nationality),
                                    nameof(user.Ethnic), nameof(user.BirthPlace), nameof(user.Politics), nameof(user.StudentFrom), nameof(user.Gatq), nameof(user.StudentSource)];
            string[] teacherInfo = [nameof(user.Title), nameof(user.Position), nameof(user.Education), nameof(user.QQ), nameof(user.OfficePhone)];

            #region 填写用户信息
            string[] @params = [.. basicInfo, .. studentInfo, .. teacherInfo];
            foreach (var item in @params)
            {
                UserSetter.SetValue(item, user, Client);
            }

            var isValid = user.Validate(Client);
            if (!isValid)
            {
                return ValidateError(isValid);
            }
            #endregion

            #region 保存用户
            UserRepository.SaveChanges();

            if (addFlag)
            {
                //Client.Logger.User.CreateUser(Client, user);
                Client.Log(MODULE_NAME, "SaveUser", id, $"新建了{user}");
                // TODO: 在这里处理用户默认的组织机构关系，仅新建用户时需要
                RoleService rService = Resolve<RoleService>();
                var rId = rService.DefaultSchema.ID;
                IOrgService orgService = Resolve<IOrgService>();
                var depart = orgService.DefaultDepart;
                if (depart.ID != Guid.Empty && rId != Guid.Empty)
                {
                    UserRelationship relationship = new UserRelationship()
                    {
                        UserId = user.ID,
                        DepartId = depart.ID,
                        RoleId = rId,
                    };
                    UserRepository.UserRelationships.Add(relationship);
                    UserRepository.SaveChanges();
                }
            }
            else
            {
                //Client.Logger.User.EditUser(Client, user);
                Client.Log(MODULE_NAME, "SaveUser", id, $"修改了{user}");
            }
            hostEnv.CredentialService.UpdateUser(user.ToCommonUser());
            #endregion

            return Ok(new
            {
                success = true,
                msg = "",
                data = user
            }, "yyyy-MM-dd");
        }

        [ApiShouldAuthorize("删除用户")]
        public ActionResult RemoveUser()
        {
            Guid id = Get("id", Guid.Empty);

            User? user = UserRepository.GetUser(id);
            if (user == null)
            {
                return Fail(new
                {
                    success = false,
                    msg = "该用户已经被删除",
                    errorStrings = "该用户已经被删除"
                });
            }

            UserRepository.RemoveWhere(e => e.ID == id, true);
            //Client.Logger.User.RemoveUser(Client, user);
            Client.Log(MODULE_NAME, "RemoveUser", id, $"删除了ID为{id}的用户");

            return Ok(new
            {
                success = true,
                msg = ""
            });
        }
        #endregion

        #region 个人中心类接口
        [ApiShouldLogin("查询个人信息")]
        public ActionResult GetProfile()
        {
            Guid id = CurrentUser.ID;
            var user = UserRepository.GetUser(id);
            if (user == null)
            {
                return Ok(new
                {
                    success = false,
                    msg = $"找不到ID对应的用户",
                    id
                });
            }

            var rules = user.BuildValidator();

            return Ok(new
            {
                success = true,
                msg = "",
                data = user,
                rules = rules.ToJsonObject(),
                //roles
            }, "yyyy-MM-dd");
        }

        [ApiAuthorizeInCode("保存个人信息")]
        public ActionResult SaveProfile()
        {
            #region 读取用户
            Guid id = CurrentUser.ID;
            User? user = UserRepository.GetUser(id);
            if (user == null)
            {
                return Fail(new
                {
                    success = false,
                    msg = "该用户已经被删除",
                    errorStrings = "该用户已经被删除"
                });
            }
            #endregion

            string[] basicInfo = [nameof(user.Mobile), nameof(user.Email)];

            string[] extraInfo = [nameof(user.Gender), nameof(user.Birthday)];

            string[] studentInfo = [nameof(user.Grade), nameof(user.Major), nameof(user.Department), nameof(user.GraduadeTime), nameof(user.ReformType), nameof(user.Nationality),
                                    nameof(user.Ethnic), nameof(user.BirthPlace), nameof(user.Politics), nameof(user.StudentFrom), nameof(user.Gatq), nameof(user.StudentSource)];

            string[] teacherInfo = [nameof(user.Title), nameof(user.Position), nameof(user.Education), nameof(user.QQ), nameof(user.OfficePhone)];

            #region 填写用户信息
            string[] @params = basicInfo;
            foreach (var item in @params)
            {
                UserSetter.SetValue(item, user, Client);
            }

            @params = [.. extraInfo, .. studentInfo, .. teacherInfo];
            foreach (var item in @params)
            {
                UserSetter.SetValueIfEmpty(item, user, Client);
            }

            var isValid = user.Validate(Client);
            if (!isValid)
            {
                return ValidateError(isValid);
            }
            #endregion

            #region 保存用户
            UserRepository.SaveChanges();
            hostEnv.CredentialService.UpdateUser(user.ToCommonUser());
            Client.Log(MODULE_NAME, "SaveProfile", id, $"修改了{user}的个人信息");
            #endregion

            return Ok(new
            {
                success = true,
                msg = "",
                data = user
            });
        }
        #endregion

        #region 修改密码
        [ApiShouldAuthorize("保存用户")]
        public ActionResult ResetPassword()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            string password = Get(nameof(password), string.Empty);
            User? user = UserRepository.GetUser(id);
            if (user == null)
            {
                return Fail(new
                {
                    success = false,
                    msg = "该用户已经被删除",
                    errorStrings = "该用户已经被删除"
                });
            }

            user.UserPassword = password;
            UserRepository.SaveChanges();
            Client.Log(MODULE_NAME, "ResetPassword", id, $"修改了{user}的用户的密码");

            return Ok("修改成功");
        }
        #endregion

        #region 组织机构
        [ApiLinkTo("获取用户", "/manage/security/users")]
        public ActionResult GetUserOrganizations()
        {
            Guid id = Get(nameof(id), Guid.Empty); // 用户ID
            var data = UserRepository.GetRelationships(id);

            return Ok(new
            {
                success = true,
                msg = "",
                data
            });
        }

        [ApiLinkTo("删除用户所在部门", "保存用户")]
        public ActionResult RemoveUserOrganization()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            UserRepository.UserRelationships.Where(e => e.ID == id).ExecuteDelete();
            UserRepository.SaveChanges();
            return Ok("删除成功");
        }

        [ApiLinkTo("设置用户的根部门", "保存用户")]
        public ActionResult SetRootDepart()
        {
            Guid userId = Get(nameof(userId), Guid.Empty);
            Guid departId = Get(nameof(departId), Guid.Empty);

            User? user = UserRepository.GetUser(userId);
            if (user == null)
            {
                return Fail(new
                {
                    success = false,
                    msg = "该用户已经被删除",
                    errorStrings = "该用户已经被删除"
                });
            }

            IDepart? depart = orgService.GetOrgByDeptId(departId);
            if (depart == null)
            {
                return Fail(new
                {
                    success = false,
                    msg = "该部门已经被删除",
                    errorStrings = "找不到该部门"
                });
            }

            user.RootDepartId = departId;
            user.Department = depart.Name;
            UserRepository.SaveChanges();

            return Ok(new
            {
                success = true,
                msg = $"已将用户的主要部门设置为{depart.Name}",
                rootDepartId = departId
            });
        }

        [ApiLinkTo("设置用户组织机构关系", "保存用户")]
        public ActionResult SetUserOrganization()
        {
            Guid id = Get(nameof(id), Guid.Empty);
            Guid userId = Get(nameof(userId), Guid.Empty);
            Guid roleId = Get(nameof(roleId), Guid.Empty);
            Guid departId = Get(nameof(departId), Guid.Empty);

            User? user = UserRepository.GetUser(userId);
            if (user == null)
            {
                return Fail(new
                {
                    success = false,
                    msg = "该用户已经被删除",
                    errorStrings = "该用户已经被删除"
                });
            }

            UserRelationship? ur;
            if (id != Guid.Empty)
            {
                // 更新逻辑
                ur = UserRepository.UserRelationships.FirstOrDefault(e => e.ID == id);
                if (ur == null)
                {
                    return Fail("无法找到此项，请检查是否已经被其它人删除，可以尝试刷新。");
                }

                ur.UserId = userId;
                ur.RoleId = roleId;
                ur.DepartId = departId;
                ur.LastUpdatedOn = DateTime.Now;
                UserRepository.SaveChanges();
            }
            else
            {
                ur = new UserRelationship()
                {
                    UserId = userId,
                    RoleId = roleId,
                    DepartId = departId
                };
                UserRepository.UserRelationships.Add(ur);
                UserRepository.SaveChanges();
            }

            // 如果没有主要部门，则设置一个主要部门
            if (user.RootDepartId == Guid.Empty && departId != Guid.Empty)
            {
                IDepart? depart = orgService.GetOrgByDeptId(departId);
                if (depart != null)
                {
                    user.RootDepartId = ur.DepartId;
                    UserRepository.SaveChanges();
                }
            }

            return Ok(new
            {
                success = true,
                msg = "保存成功",
                data = ur,
                rootDepartId = user.RootDepartId
            });
        }
        #endregion

        #region 数据验证
        [Api("验证用户名是否存在")]
        public ActionResult UserNameValidate()
        {
            Guid userId = Get("userId", Guid.Empty);
            string userName = Get(nameof(userName), string.Empty).Trim();

            if (UserRepository.ExistUserName(userName, userId))
            {
                return Ok(new { success = false, msg = "该用户已存在。" });
            }

            return Ok();
        }

        [Api("验证手机号是否存在")]
        public ActionResult MobileValidate()
        {
            Guid userId = Get("userId", Guid.Empty);
            string mobile = Get(nameof(mobile), string.Empty).Trim();

            if (UserRepository.ExistMobile(mobile, userId))
            {
                return Ok(new { success = false, msg = "该手机已经被其它用户使用。" });
            }

            return Ok();
        }

        [Api("验证电子邮箱是否存在")]
        public ActionResult EmailValidate()
        {
            Guid userId = Get("userId", Guid.Empty);
            string email = Get(nameof(email), string.Empty).Trim();

            if (UserRepository.ExistEmail(email, userId))
            {
                return Ok(new { success = false, msg = "该邮箱已经被其它用户使用。" });
            }

            return Ok();
        }

        [Api("验证证件号是否存在")]
        public ActionResult CodeValidate()
        {
            Guid userId = Get("userId", Guid.Empty);
            string code = Get(nameof(code), string.Empty).Trim();

            if (UserRepository.ExistCode(code, userId))
            {
                return Ok(new { success = false, msg = "该证件号已经被其它用户使用。" });
            }

            return Ok();
        }
        #endregion
    }

    file static class UserSetter
    {
        internal delegate void SetValueDelegate(object? obj, object? value);
        internal delegate object? GetValueDelegate(object? obj);

        internal static FrozenDictionary<string, (Type dataType, SetValueDelegate setter, GetValueDelegate getter)> Setters
        {
            get; set;
        }

        static UserSetter()
        {
            Dictionary<string, (Type dataType, SetValueDelegate, GetValueDelegate)> setter = [];
            var props = typeof(User).GetProperties();
            foreach (var item in props)
            {
                if (item.CanWrite)
                {
                    setter.Add(item.Name, (item.PropertyType, item.SetValue, item.GetValue));
                }
            }
            Setters = setter.ToFrozenDictionary();
        }

        internal static void SetValue(string prop, User user, IClient client)
        {
            var ele = Setters[prop];
            Type type = ele.dataType;
            SetValueDelegate func = ele.setter;

            if (client.TryGet(prop, type, out object? value))
            {
                object? finalValue = ValueUtils.ChangeToType(value, type, type.GetDefaultValue()!);
                func(user, finalValue);
            }
        }

        internal static void SetValueIfEmpty(string prop, User user, IClient client)
        {
            var ele = Setters[prop];
            Type type = ele.dataType;
            SetValueDelegate func = ele.setter;
            object? srcValue = ele.getter(user);

            TypeCode code = Type.GetTypeCode(type);
            object? value;
            switch (code)
            {
                case TypeCode.Empty:
                case TypeCode.Object:
                case TypeCode.DBNull:
                case TypeCode.Boolean:
                    break;
                case TypeCode.Char:
                case TypeCode.SByte:
                case TypeCode.Byte:
                case TypeCode.Int16:
                case TypeCode.UInt16:
                case TypeCode.Int32:
                case TypeCode.UInt32:
                case TypeCode.Int64:
                case TypeCode.UInt64:
                case TypeCode.Single:
                case TypeCode.Double:
                case TypeCode.Decimal:
                    if (client.TryGet(prop, type, out value) && srcValue != null && (double)srcValue == 0)
                    {
                        func(user, value);
                    }
                    break;
                case TypeCode.DateTime:
                    if (srcValue is DateTime dt && dt == ConstKeys.MinDate && client.TryGet(prop, dt, out DateTime destValue))
                    {
                        func(user, destValue);
                    }
                    break;
                case TypeCode.String:
                    if (srcValue is string str && str.IsNullOrWhiteSpace() && client.TryGet(prop, str, out value))
                    {
                        func(user, value);
                    }
                    break;
                default:
                    break;
            }

        }
    }
}
