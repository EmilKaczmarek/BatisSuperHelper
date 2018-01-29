namespace iBatisSuperHelper.Services.Search
{
    public interface IFileSearcher
    {
        SearchResult Execute(string searchString, string directory);
    }
}
