using System.Collections.Generic;
using MongoDB.Bson;

namespace ttime.Backends.MongoDb;

public class MongoAlias
{
    private Alias _alias;
    public MongoAlias() : this(new Alias()) { }
    public MongoAlias(Alias alias)
    {
        _alias = alias;
    }

    public ObjectId _id { get; set; }

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