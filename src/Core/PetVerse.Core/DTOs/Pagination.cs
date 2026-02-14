namespace PetVerse.Core.DTOs;

/// <summary>
/// 分页请求参数
/// </summary>
public class PaginationRequest
{
    private const int MaxPageSize = 100;
    private const int DefaultPageSize = 10;
    private int _pageSize = DefaultPageSize;
    private int _page = 1;

    /// <summary>
    /// 页码（从1开始）
    /// </summary>
    public int Page
    {
        get => _page;
        set => _page = value < 1 ? 1 : value;
    }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? DefaultPageSize : value;
    }

    /// <summary>
    /// 排序字段
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// 是否降序排列
    /// </summary>
    public bool IsDescending { get; set; } = false;

    /// <summary>
    /// 搜索关键词
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// 跳过的记录数
    /// </summary>
    public int Skip => (Page - 1) * PageSize;

    /// <summary>
    /// 创建分页信息
    /// </summary>
    /// <param name="totalCount">总记录数</param>
    /// <returns>PaginationInfo实例</returns>
    public PaginationInfo ToPaginationInfo(int totalCount)
    {
        return new PaginationInfo
        {
            Page = Page,
            PageSize = PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / PageSize)
        };
    }
}

/// <summary>
/// 分页结果
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// 数据列表
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

    /// <summary>
    /// 分页信息
    /// </summary>
    public PaginationInfo Pagination { get; set; } = new();

    /// <summary>
    /// 创建分页结果
    /// </summary>
    /// <param name="items">数据项</param>
    /// <param name="pagination">分页信息</param>
    /// <returns>PagedResult实例</returns>
    public static PagedResult<T> Create(IEnumerable<T> items, PaginationInfo pagination)
    {
        return new PagedResult<T>
        {
            Items = items,
            Pagination = pagination
        };
    }
}