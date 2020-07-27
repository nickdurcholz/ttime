using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace ttime
{
    public class Report
    {
        private readonly decimal _rounding;
        private static IReadOnlyList<string> UnspecifiedTags = new[] {"Unspecified"};

        public Report(decimal rounding)
        {
            _rounding = rounding;
        }

        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public List<Item> Items { get; } = new List<Item>();

        public long Milliseconds { get; set; }
        public decimal Hours { get; set; }

        public void Add(IReadOnlyList<string> tags, long milliseconds)
        {
            tags ??= UnspecifiedTags;
            var items = Items;
            Item parent = null;
            foreach (var tag in tags)
            {
                var item = GetOrCreateItem(tag, items, parent);
                item.Milliseconds += milliseconds;
                item.Hours = RoundMillisecondsToHours(item.Milliseconds);
                items = item.Children;
                parent = item;
            }

            Milliseconds += milliseconds;
            Hours = RoundMillisecondsToHours(Milliseconds);
        }

        private Item GetOrCreateItem(string tag, List<Item> items, Item parent)
        {
            var item = items.FirstOrDefault(i => i.Tag == tag);
            if (item == null)
            {
                item = new Item {Tag = tag, Parent = parent};
                items.Add(item);
            }

            return item;
        }

        private decimal RoundMillisecondsToHours(long ms)
        {
            if (_rounding == 0m)
                return ms / 3600000m;

            var roundingFactor = (long) (3600000 * _rounding);
            ms = ms / roundingFactor * roundingFactor;
            return ms / 3600000m;
        }

        [DebuggerDisplay("{Tag} - {Hours}")]
        public class Item
        {
            public string Tag { get; set; }
            public List<Item> Children { get; } = new List<Item>();
            [JsonIgnore]
            public Item Parent { get; set; }
            public long Milliseconds { get; set; }
            public decimal Hours { get; set; }

            public string GetName()
            {
                var names = new List<string> {Tag};
                var current = this;
                while (current.Parent != null)
                {
                    names.Add(current.Parent.Tag);
                    current = current.Parent;
                }

                names.Reverse();
                return string.Join(' ', names);
            }
        }
    }
}