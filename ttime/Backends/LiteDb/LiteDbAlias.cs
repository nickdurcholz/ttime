using System.Collections.Generic;
using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbAlias(Alias alias)
{
    public LiteDbAlias() : this(new Alias()) { }

    public ObjectId Id
    {
        get => string.IsNullOrEmpty(alias.Id) ? null : new(alias.Id);
        set => alias.Id = value.ToString();
    }

    public string Name
    {
        get => alias.Name;
        set => alias.Name = value;
    }

    public List<string> Args
    {
        get => alias.Args;
        set => alias.Args = value;
    }

    public Alias Alias => alias;
}