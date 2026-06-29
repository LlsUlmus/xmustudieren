namespace Ricebird.Framework.Database
{
    public abstract class TreeRepositoryBase<TNode>(RicebirdContext ctx, IServiceProvider scoped) : RepositoryBase<TNode>(ctx, scoped)
        where TNode : TreeEntityBase<TNode>, new()
    {
        protected object lck = new object();
        protected List<TNode> _allNodes = [];
        private bool init = false;

        public virtual IEnumerable<TNode> AllNodes
        {
            get
            {
                lock (lck)
                {
                    if (!init)
                    {
                        _allNodes = LoadAllNodes();
                        init = true;
                    }

                    return _allNodes;
                }
            }
        }

        #region 树的剪切复制和删除
        /// <summary>
        /// 移动分类
        /// </summary>
        /// <param name="to"></param>
        /// <param name="value"></param>
        /// <param name="opera"></param>
        /// <param name="client"></param>
        /// <returns></returns>
        public virtual (bool success, string msg, List<TNode> affectRows) MoveNodes(Guid to, List<Guid> value, string opera)
        {
            if (to != Guid.Empty && opera == "cut" && AllNodes.Where(e => value.Contains(e.ID)).Any(x => x.ID == to || x.AllChildren.Any(y => y.ID == to)))
            {
                return (false, "剪切的目标位置不能是源位置，也不能在源位置的内部。", new List<TNode>());
            }

            List<TNode> affects = [];
            List<TNode> existNodes = opera switch
            {
                "copy" => AllNodes.Where(e => value.Contains(e.ID)).ToList(),
                "cut" => AllNodes.Where(e => value.Contains(e.ID)).ToList(),
                "delete" => AllNodes.Where(e => value.Contains(e.ID)).ToList(),
                _ => []
            };

            foreach (var node in existNodes)
            {
                TNode tmp = node;

                switch (opera)
                {
                    case "copy":
                        tmp = node.CopyTo();
                        DbSet.Add(tmp);
                        tmp.ParentId = to;
                        affects.Add(tmp);

                        var children = AllNodes.Where(e => e.ParentId == node.ID).Select(e => e.ID).ToList();
                        var (_, _, copyRows) = MoveNodes(tmp.ID, children, opera);
                        affects.AddRange(copyRows);
                        break;
                    case "delete":
                        DbSet.Remove(tmp);
                        affects.Add(tmp);
                        var (_, _, deletedRows) = MoveNodes(node.ID, AllNodes.Where(e => e.ParentId == node.ID).Select(e => e.ID).ToList(), opera);
                        affects.AddRange(deletedRows);
                        break;
                    case "cut":
                        tmp.ParentId = to;
                        affects.Add(tmp);
                        break;
                    default:
                        break;
                }
            } // foreach

            switch (opera)
            {
                case "cut":
                case "copy":
                case "delete":
                    SaveChanges();
                    break;
                default:
                    return (false, $"不允许执行名为{opera}的操作", affects);
            }

            return (true, "", affects);
        }
        #endregion

        #region 加载和创建结点
        /// <summary>
        /// 加载所有结点，加载并且重新整理所有内部编号
        /// </summary>
        /// <returns></returns>
        public virtual List<TNode> LoadAllNodes()
        {
            List<TNode> result = [];

            result = [.. DbSet.OrderBy(e => e.DisplayOrder)];

            ////确保一些必要的节点是存在的
            //var nodes = EnsureNodeMustExist();
            //bool addFlag = false;
            //foreach (var node in nodes)
            //{
            //    if (!result.Any(e => e.Name == node.Name))
            //    {
            //        addFlag = true;
            //        DbSet.Add(node);
            //        result.Add(node);
            //    }
            //}

            //if (addFlag)
            //{
            //    SaveChanges();
            //}

            //result = [.. result.OrderBy(e => e.DisplayOrder)];

            var parentNodes = result.Where(e => e.ParentId == Guid.Empty);
            int i = 1;
            foreach (var item in parentNodes)
            {
                item.Parent = null;
                string code = CreateCode(i);
                if (item.InternalTreeCode != code)
                {
                    item.InternalTreeCode = code;
                }
                LoadChildren(item, result);
                i++;
            }

            SaveChanges();
            _allNodes = result;
            return _allNodes;
        }

        /// <summary>
        /// 载入子部门
        /// </summary>
        /// <param name="node"></param>
        protected void LoadChildren(TNode node, List<TNode> allNodes)
        {
            if (node == null)
            {
                return;
            }

            var children = allNodes.Where(e => e.ParentId == node.ID).ToList();

            if (children.Count == 0)
            {
                return;
            }

            node.Children = new List<TNode>();
            node.AllChildren = new List<TNode>();

            string parentCode = node.InternalTreeCode;
            int i = 1;
            foreach (TNode item in children)
            {
                item.Parent = node;
                string code = AppendCode(parentCode, CreateCode(i));
                if (item.InternalTreeCode != code)
                {
                    item.InternalTreeCode = code;
                }
                i++;
                node.AddChild(item);

                //将该部门添加到所有的母部门中
                for (TNode? d = item.Parent; d != null; d = d.Parent)
                {
                    d.AddAllChildren(item);
                }

                LoadChildren(item, allNodes);
            }
        }
        #endregion

        #region 一些基本的查询
        public virtual List<TNode> GetChildren(Guid parentId)
        {
            return [.. DbSet.Where(e => e.ID == parentId)];
        }

        public virtual List<TNode> GetAllChildren(Guid parentId)
        {
            var parentNode = DbSet.FirstOrDefault(e => e.ID == parentId);
            if (parentNode == null)
            {
                return [];
            }
            return
            [
                .. DbSet
                    .Where(e => e.ID != parentId)
                    .Where(e => e.InternalTreeCode.StartsWith(parentNode.InternalTreeCode))
,
            ];
        }

        public virtual TNode? GetLastChild(Guid parentId)
        {
            var children = GetChildren(parentId);
            return children.OrderBy(e => e.InternalTreeCode).LastOrDefault();
        }

        public virtual string GetNextChildCode(Guid parentId)
        {
            var lastChild = GetLastChild(parentId);
            if (lastChild != null)
            {
                return CalculateNextCode(lastChild.InternalTreeCode);
            }

            string parentCode = parentId != Guid.Empty
                ? DbSet.FirstOrDefault(e => e.ID == parentId)?.InternalTreeCode ?? ""
                : "";

            return AppendCode(
                parentCode,
                CreateCode(1)
            );
        }

        public virtual TNode? GetEntityByName(string name)
        {
            return DbSet.FirstOrDefault(e => e.Name == name);
        }
        #endregion

        #region 基于缓存的查询
        /// <summary>
        /// 获取节点
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public TNode? GetNode(Guid id)
        {
            return AllNodes.FirstOrDefault(e => e.ID == id);
        }
        #endregion

        #region Code操作函数
        /// <summary>
        /// 创建树查询的内部编号
        /// </summary>
        public string CreateCode(params int[] numbers)
        {
            if (numbers == null || numbers.Length == 0)
            {
                return "";
            }

            return numbers.Select(number => number.ToString(new string('0', 5))).JoinAsString(".");
        }

        /// <summary>
        /// 将子编号添加到总编号中
        /// </summary>
        /// <param name="parentCode">元素的母编号.</param>
        /// <param name="childCode">待增加的子编号.</param>
        public string AppendCode(string parentCode, string childCode)
        {
            if (childCode.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(childCode), "子编号不能为空.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return childCode;
            }

            return parentCode + "." + childCode;
        }

        /// <summary>
        /// 取关联编号
        /// 如: if code = "00019.00055.00001" and parentCode = "00019" then returns "00055.00001".
        /// </summary>
        public string GetRelativeCode(string code, string parentCode)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            if (parentCode.IsNullOrEmpty())
            {
                return code;
            }

            if (code.Length == parentCode.Length)
            {
                return "";
            }

            return code[(parentCode.Length + 1)..];
        }

        /// <summary>
        /// 根据编号计算下一个编号
        /// Example: if code = "00019.00055.00001" returns "00019.00055.00002".
        /// </summary>
        /// <param name="code">The code.</param>
        public string CalculateNextCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "编号不能为空");
            }

            var parentCode = GetParentCode(code);
            var lastUnitCode = GetLastUnitCode(code);

            return AppendCode(parentCode, CreateCode(Convert.ToInt32(lastUnitCode) + 1));
        }

        /// <summary>
        /// 返回最后一段编号
        /// Example: if code = "00019.00055.00001" returns "00001".
        /// </summary>
        /// <param name="code">The code.</param>
        public string GetLastUnitCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            return splittedCode[^1];
        }

        /// <summary>
        /// 返回母编号
        /// Example: if code = "00019.00055.00001" returns "00019.00055".
        /// </summary>
        /// <param name="code">The code.</param>
        public string GetParentCode(string code)
        {
            if (code.IsNullOrEmpty())
            {
                throw new ArgumentNullException(nameof(code), "code can not be null or empty.");
            }

            var splittedCode = code.Split('.');
            if (splittedCode.Length == 1)
            {
                return "";
            }

            return splittedCode.Take(splittedCode.Length - 1).JoinAsString(".");
        }
        #endregion
    }
}
