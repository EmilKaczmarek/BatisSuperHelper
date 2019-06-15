using BatisSuperHelper.Indexers;
using Ruzzie.FuzzyStrings;
using System.Collections.Generic;
using System.Linq;

namespace BatisSuperHelper.Validation.Helpers
{
    public class QuerySearchHelper
    {
        public List<IndexerKey> GetPropositionsByStatmentName(string statment)
        {
            var keys = GotoAsyncPackage.Storage.XmlQueries.GetAllKeys();

            var coefficiencyValuePairs = keys.Select(e => new {
                coefficiency = statment.DiceCoefficientUncached(e.StatmentName),
                value = e,
            }).Where(e => e.coefficiency > 0.33).OrderByDescending(e => e.coefficiency);

            return coefficiencyValuePairs.Take(3).Select(e=>e.value).ToList();
        }

    }
}
