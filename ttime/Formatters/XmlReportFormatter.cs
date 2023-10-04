using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace ttime.Formatters;

public class XmlReportFormatter : IReportFormatter
{
    public void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel)
    {
        var elements = reports.Select(CreateReportElement).ToArray();
        var root = elements.Length == 1 ? elements[0] : new XElement("reports", elements);

        var settings = new XmlWriterSettings {Indent = true, OmitXmlDeclaration = true};
        using (var writer = XmlWriter.Create(@out, settings))
        {
            root.WriteTo(writer);
            writer.Flush();
            writer.Close();
        }
    }

    private static XElement CreateReportElement(Report report)
    {
        var reportElement = new XElement(
            "report",
            new XAttribute("start", report.Start.ToString("s")),
            new XAttribute("end", report.End.ToString("s")),
            new XAttribute("hours", report.Hours));
        AddSubTasks(report.Items, reportElement);

        return reportElement;
    }

    private static void AddSubTasks(List<ReportItem> items, XElement container)
    {
        foreach (var item in items)
        {
            var task = new XElement(
                "task",
                new XAttribute("name", item.Tag),
                new XAttribute("hours", item.Hours));
            container.Add(task);
            AddSubTasks(item.Items, task);
        }
    }
}