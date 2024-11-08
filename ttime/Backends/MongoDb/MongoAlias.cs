using System.Collections.Generic;

namespace ttime.Backends.MongoDb;

public class MongoAlias
{
    public MongoAlias() : this(new Alias()) { }

    public MongoAlias(Alias alias)
    {
        Alias = alias;
    }

    public string _id => Name;

    public string Name
    {
        get => Alias.Name;
        set => Alias.Name = value;
    }

    public List<string> Args
    {
        get => Alias.Args;
        set => Alias.Args = value;
    }

    public Alias Alias { get; set; }
}