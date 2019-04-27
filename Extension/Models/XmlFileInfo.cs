using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IBatisSuperHelper.Models
{
    public class XmlFileInfo
    {
        public string FilePath { get; set; }
        public string ProjectName { get; set; }

        public override bool Equals(object obj)
        {
            var info = obj as XmlFileInfo;
            return info != null &&
                   FilePath == info.FilePath &&
                   ProjectName == info.ProjectName;
        }
    }
}
