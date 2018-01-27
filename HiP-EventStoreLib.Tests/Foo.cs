using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Tests
{
    public class Foo : IEntity<int>
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Parent { get; set; }

        public List<int> Bars { get; set; }

        public override bool Equals(object obj)
        {
            var foo = obj as Foo;
            return foo != null &&
                   Id == foo.Id &&
                   Name == foo.Name &&
                   Parent == foo.Parent &&
                   EqualityComparer<List<int>>.Default.Equals(Bars, foo.Bars);
        }

        public override int GetHashCode()
        {
            var hashCode = 1778032246;
            hashCode = hashCode * -1521134295 + Id.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            hashCode = hashCode * -1521134295 + Parent.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<List<int>>.Default.GetHashCode(Bars);
            return hashCode;
        }
    }
}
