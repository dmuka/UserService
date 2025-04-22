namespace WebApi.Pages;

public class PagedResult<T>
{
    public IList<T> Items { get; set; } = [];
    public int TotalItems { get; set; }
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    public int TotalPages { get; set; }
    public int VisiblePagesRange { get; set; }
    public int FirstVisiblePage { get; set; }
    public int LastVisiblePage { get; set; }
}