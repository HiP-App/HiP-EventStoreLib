using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Tests
{
    public class Foo : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Parent { get; set; }

        public List<int> Bars { get; set; }
    }
}
