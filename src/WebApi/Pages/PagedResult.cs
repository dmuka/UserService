namespace WebApi.Pages;

public class PagedResult<T>
{
    public PagedResult()
    {
        TotalPages = (int)Math.Ceiling(TotalItems / (double)PageSize);
    }

    public IList<T> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
}