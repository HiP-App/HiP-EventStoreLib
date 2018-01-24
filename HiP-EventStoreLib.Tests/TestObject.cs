using System.Collections.Generic;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Tests
{
    public class TestObject
    {
        [NestedObject]
        public TestObject Object { get; set; }

        [NestedObject]
        public Foo Foo { get; set; }

        public string Name { get; set; }

        public override bool Equals(object obj)
        {
            var @object = obj as TestObject;
            return @object != null &&
                   EqualityComparer<TestObject>.Default.Equals(Object, @object.Object) &&
                   EqualityComparer<Foo>.Default.Equals(Foo, @object.Foo) &&
                   Name == @object.Name;
        }

        public override int GetHashCode()
        {
            var hashCode = 869543567;
            hashCode = hashCode * -1521134295 + EqualityComparer<TestObject>.Default.GetHashCode(Object);
            hashCode = hashCode * -1521134295 + EqualityComparer<Foo>.Default.GetHashCode(Foo);
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Name);
            return hashCode;
        }
    }
}
