using System;
using System.IO;

namespace ttime
{
    public abstract class Command
    {
        public abstract void Run(Span<string> args);

        public abstract void PrintUsage(TextWriter @out);
    }
}