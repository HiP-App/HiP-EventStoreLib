using PaderbornUniversity.SILab.Hip.EventSourcing;
using PaderbornUniversity.SILab.Hip.EventSourcing.Mongo;
using PaderbornUniversity.SILab.Hip.EventSourcing.Mongo.Test;
using System.Linq;
using Xunit;

namespace HiP_EventStoreLib.Tests
{
    /// <summary>
    /// Validates the correct behavior of <see cref="FakeMongoDbContext"/>. Correctness is
    /// important here because the class is used as a MongoDB mock class in further tests.
    /// </summary>
    public class FakeMongoDbContextTest
    {
        [Fact]
        public void TestCrud()
        {
            var tFoo = ResourceType.Register("Foo", null);
            var tBar = ResourceType.Register("Bar", null);

            var db = new FakeMongoDbContext();

            var foo3 = new Foo { Id = 3, Bars = { Ids = { 3, 1 } } };
            var foo4 = new Foo { Id = 4, Bars = { Ids = { 3, 1 } } };

            db.Add(tFoo, foo4);

            Assert.Equal(new[] { foo4 }, db.GetCollection<Foo>(tFoo));

            db.Add<IEntity<int>>(tFoo, foo3);

            Assert.Equal(new[] { foo3, foo4 }, db.GetCollection<Foo>(tFoo).OrderBy(o => o.Id));
            Assert.Same(foo3, db.Get<Foo>((tFoo, 3)));
            Assert.Same(foo4, db.Get<Foo>((tFoo, 4)));

            // TODO: Test update, replace, delete
        }
    }

    class Foo : IEntity<int>
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DocRef<Foo> Parent { get; set; } = new DocRef<Foo>("Foo");
        public DocRefList<Bar> Bars { get; set; } = new DocRefList<Bar>("Bar");
    }

    class Bar : IEntity<int>
    {
        public int Id { get; set; }
    }
}
