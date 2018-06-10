using System.Collections.Generic;
using iBatisSuperHelper.Models;

namespace iBatisSuperHelper.Services
{
    public sealed class IndexerSingleton
    {
        static IndexerSingleton()
        {
        }

        private IndexerSingleton()
        {
        }

        public static IndexerSingleton Instance { get; } = new IndexerSingleton();
        //TODO: Dodanie jakieś klasay

        private readonly Dictionary<IndexerKey, StatmentInfo> _statmentInfosDictionary = new Dictionary<IndexerKey, StatmentInfo>();

        public void RegisterStatmentInfo(StatmentInfo info)
        {
            //Todo: validacja
            _statmentInfosDictionary.Add(info.Key, info);
        }

        public void UnregisterStatmentInfo(StatmentInfo info)
        {
            //Todo: validacja
            _statmentInfosDictionary.Remove(info.Key);
        }

        public StatmentInfo GetStatmentInfo(IndexerKey key)
        {
            //Todo: validacja
            return _statmentInfosDictionary[key];
        }

    }
}