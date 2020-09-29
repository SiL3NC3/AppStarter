using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AppStarter.Models
{
    public class AppStartItem
    {
        public string Text { get; set; }
        public string Category { get; set; }
        public string Path { get; set; }
        public Icon IconID { get; set; }


        public AppStartItem() { }
    }
}
