using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

namespace ttime
{
    public class XmlFormatter : Formatter
    {
        public override void Write(Report report, TextWriter @out)
        {
            var reportElement = new XElement(
                "report",
                new XAttribute("start", report.Start.ToString("u")),
                new XAttribute("end", report.End.ToString("u")),
                new XAttribute("total", report.Total));
            foreach (var item in report.Items)
            {
                reportElement.Add(new XElement(
                    "task",
                    new XAttribute("name", item.Name),
                    new XAttribute("hours", item.Hours)));
            }

            using (var writer = XmlWriter.Create(@out, new XmlWriterSettings { Indent = true, OmitXmlDeclaration = true }))
            {
                reportElement.WriteTo(writer);
                writer.Flush();
                writer.Close();
            }
        }
    }
}