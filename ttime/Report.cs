using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Newtonsoft.Json;

namespace ttime
{
    public class Report : TimeContainer
    {
        private static readonly IReadOnlyList<string> UnspecifiedTags = new[] { "unspecified" };

        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public void Add(IReadOnlyList<string> tags, long milliseconds)
        {
            tags ??= UnspecifiedTags;
            var items = Items;
            Item parent = null;
            foreach (var tag in tags)
            {
                var item = GetOrCreateItem(tag, items, parent);
                item.Milliseconds += milliseconds;
                items = item.Items;
                parent = item;
            }

            Milliseconds += milliseconds;
        }

        private Item GetOrCreateItem(string tag, List<Item> items, Item parent)
        {
            var item = items.FirstOrDefault(i => i.Tag == tag);
            if (item == null)
            {
                item = new Item { Tag = tag, Parent = parent };
                items.Add(item);
            }

            return item;
        }
    }

    [DebuggerDisplay("{Tag} - {Hours}")]
    public class Item : TimeContainer
    {
        public string Tag { get; set; }

        [JsonIgnore]
        public Item Parent { get; set; }
    }

    public class TimeContainer
    {
        private const decimal MsPerHour = 3600000m;
        public List<Item> Items { get; } = new List<Item>();
        public long Milliseconds { get; set; }
        public decimal Hours { get; set; }

        /// <summary>
        ///     Rounds time spent on this item and all children in a way that ensures total rounding error for all categories is
        ///     &lt;= roundingFactor. The total rounding error may be carried over from other reports.
        /// </summary>
        /// <param name="roundingError">Total rounding error in milliseconds</param>
        /// <param name="roundingFactor">Desired precision in fractional hours</param>
        /// <returns>Returns the total rounding error in milliseconds</returns>
        public long SetRoundedHours(long roundingError, decimal roundingFactor) => SetRoundedHours(roundingError, (long)(MsPerHour * Math.Abs(roundingFactor)));

        private long SetRoundedHours(long roundingError, long roundingMultiple)
        {
            //This function takes the mental gymnastics out of entering accurate-as-possible time into a system that requires you
            //to do silly things like round to the nearest quarter hour.

            foreach (var child in Items)
                roundingError = child.SetRoundedHours(roundingError, roundingMultiple);

            if (roundingMultiple > 0)
            {
                var timeExcludingChildren = Math.Max(0L, Milliseconds - Items.Sum(i => i.Milliseconds));
                var roundedMs = GetRoundedMilliseconds(timeExcludingChildren, roundingMultiple);
                var myError = roundedMs - timeExcludingChildren;
                if (roundingError + myError > roundingMultiple)
                    roundedMs -= roundingMultiple;
                else if (roundingError + myError < -roundingMultiple)
                    roundedMs += roundingMultiple;

                Hours = roundedMs / MsPerHour + Items.Sum(i => i.Hours);

                return roundingError + roundedMs - timeExcludingChildren;
            }

            Hours = Milliseconds / MsPerHour;
            return 0;
        }

        private static long GetRoundedMilliseconds(long ms, long roundingMultiple)
        {
            var floor = ms / roundingMultiple * roundingMultiple;
            var midpoint = roundingMultiple / 2;
            if (ms - floor > midpoint)
                return floor + roundingMultiple;
            return floor;
        }
    }
}