public class PaginatedQueryResponseModel<T>
{
    public int PageNum { get; set; }
    public int TotalPages { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public IEnumerable<T> Data { get; set; }

    public PaginatedQueryResponseModel(int pageNum, int pageSize, int totalElements, IEnumerable<T> pageData)
    {
        PageNum = pageNum;
        PageSize = pageSize;
        TotalItems = totalElements;
        TotalPages = (int)Math.Ceiling(TotalItems / (double)pageSize);
        Data = pageData;
    }
}
