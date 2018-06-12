using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VSIXProject5.Actions.Abstracts
{
    public abstract class BaseActions
    {
        public GotoPackage package { get; set; }
        public abstract void BeforeQuery(object sender, EventArgs e);
        public abstract void Change(object sender, EventArgs e);
        public abstract void MenuItemCallback(object sender, EventArgs e);
    }
}
