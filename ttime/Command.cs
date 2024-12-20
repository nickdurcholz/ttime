﻿using System;
using System.IO;
using ttime.Backends;

namespace ttime;

public abstract class Command
{
    public abstract void Run(Span<string> args);

    public abstract void PrintUsage();

    public abstract string Name { get; }

    public abstract string Description { get; }
    public IStorage Storage { get; set; }
    public Configuration Configuration { get; set; }
    public TextWriter Out { get; set; }
    public TextWriter Error { get; set; }
    public TextReader In { get; set; }
}