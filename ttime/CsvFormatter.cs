using System;
using System.IO;
using System.Linq;
using Csv;

namespace ttime
{
    public class CsvFormatter : Formatter
    {
        public override void Write(Report report, TextWriter @out)
        {
            CsvWriter.Write(
                @out,
                new [] {"Task","Hours"},
                report.Items.Select(i => new [] { i.Name, i.Hours.ToString("F")}));
        }
    }
}