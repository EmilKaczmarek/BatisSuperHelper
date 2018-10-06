﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Indexers.Models
{
    public class XmlIndexerResult : BaseIndexerValue
    {
        public string MapNamespace { get; set; }
        public string FullQuerryName => $"{MapNamespace}.{base.QueryId}";
    }
}
