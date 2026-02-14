namespace PetVerse.Core.DTOs;

/// <summary>
/// 统一API响应模型
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }
    
    /// <summary>
    /// 响应消息
    /// </summary>
    public string Message { get; set; } = string.Empty;
    
    /// <summary>
    /// 响应数据
    /// </summary>
    public T? Data { get; set; }
    
    /// <summary>
    /// 错误信息列表
    /// </summary>
    public List<string> Errors { get; set; } = new();
    
    /// <summary>
    /// HTTP状态码
    /// </summary>
    public int StatusCode { get; set; }
    
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 创建成功的响应
    /// </summary>
    public static ApiResponse<T> CreateSuccess(T data, string message = "操作成功", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data,
            StatusCode = statusCode
        };
    }
    
    /// <summary>
    /// 创建错误响应
    /// </summary>
    public static ApiResponse<T> CreateError(string message, List<string>? errors = null, int statusCode = 400)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Errors = errors ?? new List<string>(),
            StatusCode = statusCode
        };
    }
}

/// <summary>
/// 分页信息
/// </summary>
public class PaginationInfo
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}