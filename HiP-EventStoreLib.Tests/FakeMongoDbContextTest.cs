using PaderbornUniversity.SILab.Hip.EventSourcing.Mongo.Test;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Tests
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

            var foo3 = new Foo { Id = 3, Bars = new List<int> { 3, 1 } };
            var foo4 = new Foo { Id = 4, Bars = new List<int> { 3, 1 } };

            db.Add(tFoo, foo4);

            Assert.Equal(new[] { foo4 }, db.GetCollection<Foo>(tFoo));

            db.Add<IEntity<int>>(tFoo, foo3);

            // After adding entities, we should be able to retrieve them
            Assert.Equal(new[] { foo3, foo4 }, db.GetCollection<Foo>(tFoo).OrderBy(o => o.Id));
            Assert.Same(foo3, db.Get<Foo>((tFoo, 3)));
            Assert.Same(foo4, db.Get<Foo>((tFoo, 4)));

            // Replacing an entity with another one having a different ID should throw an exception
            var foo3b = new Foo { Id = 33 };

            Assert.Throws<ArgumentException>(() =>
                db.Replace((tFoo, 3), foo3b));

            Assert.Null(db.Get<Foo>((tFoo, 33)));

            // Replacing an entity with another one having the same ID should work
            foo3b.Id = 3;
            db.Replace((tFoo, 3), foo3b);

            Assert.Same(foo3b, db.Get<Foo>((tFoo, 3)));
            Assert.Same(foo3b, db.Get<Foo>((tFoo, 3)));

            // Updating entities should work correctly
            db.Update<Foo>((tFoo, 4), up =>
            {
                up.Add(nameof(Foo.Bars), 2); // Bars == [3, 1, 2]
                up.Add(nameof(Foo.Bars), 3, Mongo.CollectionSemantic.Set); // Bars == [3, 1, 2]
                up.AddRange(foo => foo.Bars, new[] { 1, 2, 4, 5 }, Mongo.CollectionSemantic.Set); // Bars == [3, 1, 2, 4, 5]
                up.Set(nameof(Foo.Name), "Test");
                up.Set(foo => foo.Name, "Test2");
                up.Remove(foo => foo.Bars, 4);
            });

            var foo4updated = db.Get<Foo>((tFoo, 4));
            Assert.Equal("Test2", foo4updated.Name);
            Assert.Equal(new[] { 3, 1, 2, 5 }, foo4updated.Bars);

            // Removing entities should work correctly
            Assert.False(db.Delete((tFoo, 33)));
            Assert.True(db.Delete((tFoo, 3)));

            Assert.Null(db.Get<Foo>((tFoo, 3)));
            Assert.Null(db.Get<Foo>((tFoo, 33)));
            Assert.NotNull(db.Get<Foo>((tFoo, 4)));

            Assert.Equal(new[] { foo4 }, db.GetCollection<Foo>(tFoo));
        }
    }
}
