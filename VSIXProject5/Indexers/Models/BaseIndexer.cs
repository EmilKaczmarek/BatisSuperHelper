using System.Collections.Generic;
using VSIXProject5.Indexers.Models;

public abstract class BaseIndexer
{
    public virtual async void BuildIndexerAsync(List<BaseIndexerValue> documents)
    {

    }
}