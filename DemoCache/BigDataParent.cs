using System;
using System.Collections.Generic;

namespace DemoCache
{
    [Serializable]
    public class BigDataParent
    {
        public string Description { get; set; }
        public List<BigDataChild> BigDataChildren { get; set; }        
    }
}
