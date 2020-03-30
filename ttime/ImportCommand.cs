using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ttime
{
    public class ImportCommand : Command
    {
        public override void Run(Span<string> args)
        {
            Format format = default;
            bool formatFound = false;
            string file = null;
            bool valid = true;

            foreach (var arg in args)
            {
                if (arg.StartsWith("format="))
                {
                    if (formatFound)
                    {
                        Error.WriteLine("Duplicate format specification found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (arg.Length == 7)
                    {
                        Error.WriteLine("Invalid format: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!Enum.TryParse(arg.Substring(7), true, out format))
                    {
                        Error.WriteLine("Invalid format: " + arg);
                        valid = false;
                        continue;
                    }

                    formatFound = true;
                }
                else
                {
                    if (file != null)
                    {
                        Error.WriteLine("Duplicate input file found: " + arg);
                        valid = false;
                        continue;
                    }

                    if (!File.Exists(arg))
                        Error.WriteLine($"The path '{arg}' does not exist or is not a file.");

                    file = arg;
                }
            }

            if (!valid)
                return;

            if (!formatFound)
            {
                var extension = Path.GetExtension(file);
                if (".csv".EqualsOIC(extension))
                    format = Format.Csv;
                else if (".xml".EqualsOIC(extension))
                    format = Format.Xml;
                else if (".json".EqualsOIC(extension))
                    format = Format.Json;
                else
                    format = Configuration.DefaultExportFormat;
            }

            if (format == Format.Text)
            {
                Error.WriteLine("The text formatter cannot be used with the import command. " +
                                "Please specify a different formatter with format=<xml|json|csv>.");
                return;
            }

            var formatter = Formatter.Create(format);
            List<TimeEntry> entries;
            var importReader = In;
            if (file != null)
                importReader = new StreamReader(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));

            try
            {
                entries = formatter.DeserializeEntries(importReader).ToList();
            }
            finally
            {
                if (file != null)
                {
                    importReader.Close();
                    importReader.Dispose();
                }
            }

            foreach (var entry in entries.Where(e => e.Id != null && e.Time == default))
                Storage.DeleteEntry(entry.Id);

            Storage.Save(entries.Where(e => e.Id == null || e.Time != default));
        }

        public override void PrintUsage()
        {
            Out.WriteLine("usage: ttime import [<file>] [format=csv|xml|json] [<file>]");
            Out.WriteLine();
            Out.WriteLine("    Import tracked time entries. This supports a workflow to correct");
            Out.WriteLine("    mistakes. You can use the export command to export data for a");
            Out.WriteLine("    given period, edit the file to change / add / remove entries, and");
            Out.WriteLine("    then import it. ttime will change existing entries to match the");
            Out.WriteLine("    import file.");
            Out.WriteLine();
            Out.WriteLine("    To create a new entry via import, simply omit the id field or ");
            Out.WriteLine("    provide an empty value for the Id.");
            Out.WriteLine();
            Out.WriteLine("    To delete an existing entry via import, provide a null for its");
            Out.WriteLine("    date/time.");
            Out.WriteLine();
            Out.WriteLine("    If file is omitted, the import is read from stdin. If format is");
            Out.WriteLine("    omitted, the format is inferred from the extension of the file if");
            Out.WriteLine("    present, and if the format cannot be inferred from the file name,");
            Out.WriteLine("    then the default export format specified in configuration is");
            Out.WriteLine("    used.");
        }

        public override string Name => "import";
        public override string Description => "Import time data from a file";
    }
}