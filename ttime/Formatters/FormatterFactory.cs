using System;

namespace ttime.Formatters;

public static class FormatterFactory
{
    public static IReportFormatter GetReportFormatter(ReportFormat fmt, TimeFormat timeFormat)
    {
        return fmt switch
        {
            ReportFormat.Text => new TextReportFormatter(new TimeFormatter(timeFormat)),
            ReportFormat.CsvSimple => new CsvSimpleReportFormatter(new TimeFormatter(timeFormat)),
            ReportFormat.CsvRollup => new CsvRollupReportFormatter(),
            ReportFormat.Xml => new XmlReportFormatter(),
            ReportFormat.Json => new JsonReportFormatter(),
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), fmt, null)
        };
    }

    public static IExportFormatter GetExportFormatter(ExportFormat fmt)
    {
        return fmt switch
        {
            ExportFormat.Text => new TextExportFormatter(),
            ExportFormat.Csv => new CsvExportFormatter(),
            ExportFormat.Xml => new XmlExportFormatter(),
            ExportFormat.Json => new JsonExportFormatter(),
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), fmt, null)
        };
    }

    public static IImportFormatter GetImportFormatter(ImportFormat fmt)
    {
        return fmt switch
        {
            ImportFormat.Csv => new CsvImportFormatter(),
            ImportFormat.Xml => new XmlImportFormatter(),
            ImportFormat.Json => new JsonImportFormatter(),
            _ => throw new ArgumentOutOfRangeException(nameof(fmt), fmt, null)
        };
    }
}