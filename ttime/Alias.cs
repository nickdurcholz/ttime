using System.Collections.Generic;
using LiteDB;

namespace ttime
{
    public class Alias
    {
        public ObjectId Id { get; set; }
        public string Name { get; set; }
        public List<string> Args { get; set; }
    }
}