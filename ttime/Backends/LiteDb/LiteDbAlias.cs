using System.Collections.Generic;
using LiteDB;

namespace ttime.Backends.LiteDb;

public class LiteDbAlias
{
    private Alias _alias;
    public LiteDbAlias() : this(new Alias()) { }
    public LiteDbAlias(Alias alias)
    {
        _alias = alias;
    }

    public ObjectId Id { get; set; }

    public string Name
    {
        get => _alias.Name;
        set => _alias.Name = value;
    }

    public List<string> Args
    {
        get => _alias.Args;
        set => _alias.Args = value;
    }

    public Alias Alias
    {
        get => _alias;
        set => _alias = value;
    }
}