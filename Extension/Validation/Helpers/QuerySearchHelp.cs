using IBatisSuperHelper.Indexers;
using IBatisSuperHelper.Indexers.Models;
using IBatisSuperHelper.Storage;
using Ruzzie.FuzzyStrings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Validation.Helpers
{
    public class QuerySearchHelper
    {
        public List<IndexerKey> GetPropositionsByStatmentName(string statment)
        {
            var keys = PackageStorage.XmlQueries.GetAllKeys();

            var coefficiencyValuePairs = keys.Select(e => new {
                coefficiency = statment.DiceCoefficientUncached(e.StatmentName),
                value = e,
            }).Where(e => e.coefficiency > 0.33).OrderByDescending(e => e.coefficiency);

            return coefficiencyValuePairs.Take(3).Select(e=>e.value).ToList();
        }

    }
}
