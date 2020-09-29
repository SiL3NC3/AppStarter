using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AppStarter.Models
{
    public class AppStartData
    {
        public AppStartData()
        {
            Items = new List<AppStartItem>();
        }
        public List<AppStartItem> Items { get; set; }

        public List<String> Categories
        {
            get
            {
                return Items.Select(i => i.Category).Distinct().ToList();
            }
        }
    }
}
