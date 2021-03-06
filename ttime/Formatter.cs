﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ttime
{
    public abstract class Formatter
    {
        public static Formatter Create(Format format)
        {
            switch (format)
            {
                case Format.Text:
                    return new TextFormatter();
                case Format.Csv:
                    return new CsvFormatter();
                case Format.Xml:
                    return new XmlFormatter();
                case Format.Json:
                    return new JsonFormatter();
                default:
                    return null;
            }
        }

        public abstract void Write(IEnumerable<Report> reports, TextWriter @out, int? nestingLevel);

        public abstract void Write(IEnumerable<TimeEntry> entries, TextWriter @out);

        public abstract List<TimeEntry> DeserializeEntries(TextReader reader);
    }
}