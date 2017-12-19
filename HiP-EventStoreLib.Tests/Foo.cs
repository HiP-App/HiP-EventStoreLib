using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Tests
{
    public class Foo : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }

        [ResourceReferenceAttribute("Bar")]
        public int Parent { get; set; }

        [ResourceReferenceAttribute("Bar")]
        public List<int> Bars { get; set; }
    }
}
