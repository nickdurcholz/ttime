using System;

namespace ttime;

public class CommandError : Exception
{
    public CommandError(string message) : base(message) { }
}