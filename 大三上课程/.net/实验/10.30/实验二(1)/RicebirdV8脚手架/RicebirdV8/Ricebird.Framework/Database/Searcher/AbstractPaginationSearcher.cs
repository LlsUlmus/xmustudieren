namespace Ricebird.Framework.Database.Searcher
{
    public abstract class AbstractPaginationSearcher<TEntity> : AbstractSearcher<TEntity>
    {
        #region 字段
        public int Page
        {
            get; set;
        } = 1;

        public int PageSize
        {
            get; set;
        } = 10;
        #endregion

        /// <summary>
        /// 后处理函数，在查询完成后，对数据进行处理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual TEntity PostProcessing(TEntity data)
        {
            return data;
        }

        /// <summary>
        /// 后处理函数，在查询完成后，对数据进行处理
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public virtual List<TEntity> ListPostProcessing(List<TEntity> data)
        {
            data.ForEach(e =>
            {
                PostProcessing(e);
            });
            return data;
        }

        public virtual TEntity? FirstOrDefault()
        {
            IQueryable<TEntity> query = BuildQuery();
            TEntity? data = query.FirstOrDefault();
            if (data != null)
            {
                PostProcessing(data);
            }
            return data;
        }

        public virtual List<TEntity> BuildData()
        {
            IQueryable<TEntity> query = BuildQuery();
            List<TEntity> data = query.ToList();
            data = ListPostProcessing(data);
            return data;
        }

        public virtual (int totalRow, int page, int pageSize, List<TEntity> data) BuildPaginationData()
        {
            IQueryable<TEntity> query = BuildQuery();
            int totalRow = query.Count();
            query = query.Skip((Page - 1) * PageSize).Take(PageSize);
            List<TEntity> data = query.ToList();
            data = ListPostProcessing(data);
            return (totalRow, Page, PageSize, data);
        }

        public virtual (int totalRow, int page, int pageSize, List<TEntity> data) BuildPaginationData(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
            return BuildPaginationData();
        }

        public virtual (int totalRow, int page, int pageSize, IQueryable<TEntity> query) BuildPaginationQuery()
        {
            IQueryable<TEntity> query = BuildQuery();
            int totalRow = query.Count();
            query = query.Skip((Page - 1) * PageSize).Take(PageSize);
            return (totalRow, Page, PageSize, query);
        }

        public virtual (int totalRow, int page, int pageSize, IQueryable<TEntity> data) BuildPaginationQuery(int page, int pageSize)
        {
            Page = page;
            PageSize = pageSize;
            return BuildPaginationQuery();
        }
    }
}
