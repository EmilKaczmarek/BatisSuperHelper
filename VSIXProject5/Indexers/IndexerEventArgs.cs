public class IndexerEventArgs:EventArgs
{
    public BaseIndexer Indexer { get; private set; }
    public IndexerEventArgs(BaseIndexer indexer)
    {

    }
}