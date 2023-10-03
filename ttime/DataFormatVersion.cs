using LiteDB;

namespace ttime;

public class DataFormatVersion
{
    public ObjectId Id { get; set; }
    public int Version { get; set; }
}