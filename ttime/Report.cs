using System;
using System.Collections.Generic;

namespace ttime
{
    public class Report
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<Item> Items { get; set; }
        public decimal Total { get; set; }

        public class Item
        {
            public string Name { get; set; }
            public decimal Hours { get; set; }
        }
    }
}