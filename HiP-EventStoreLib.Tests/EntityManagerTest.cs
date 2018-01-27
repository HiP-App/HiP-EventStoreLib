using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PaderbornUniversity.SILab.Hip.EventSourcing.EventStoreLlp;
using PaderbornUniversity.SILab.Hip.EventSourcing.FakeStore;
using System;
using System.Collections.Generic;
using Xunit;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Tests
{
    /// <summary>
    /// Validates the correct behaviour of <see cref="EntityManager"/>
    /// </summary>
    public class EntityManagerTest
    {
        private FakeEventStore _eventStore = new FakeEventStore();
        private EventStoreService _service;

        public EntityManagerTest()
        {
            var config = new EventStoreConfig() { Host = "", Stream = "stream" };

            _service = new EventStoreService(_eventStore, new InMemoryCache(new List<IDomainIndex>(), new Logger<InMemoryCache>()), Options.Create(config), new Logger<EventStoreService>());
        }

        [Fact]
        public async void Test()
        {
            var resourceType = ResourceType.Register("TestObject", typeof(TestObject));
            var testObject1 = new TestObject { Name = "Test1", Foo = new Foo { Id = 5, Parent = 3, Name = "Foo1", Bars = new List<int> { 1, 2, 3, 4 } } };
            var testObject2 = new TestObject { Object = testObject1, Name = "Test2", Foo = new Foo { Id = 6, Parent = 7, Name = "Foo2", Bars = new List<int> { 1, 5, 3, 4 } } };
            int id = 1;
            string userId = "";

            await EntityManager.CreateEntityAsync(_service, testObject1, resourceType, id, userId);

            //compare the two testObjects
            await EntityManager.UpdateEntityAsync(_service, testObject1, testObject2, resourceType, id, userId);
            var entity = await _service.EventStream.GetCurrentEntityAsync<TestObject>(resourceType, id);

            Assert.Equal(testObject2, entity);

            testObject1.Object = testObject2;

            //check if the cycle is detected
            await Assert.ThrowsAsync<InvalidOperationException>(() => EntityManager.UpdateEntityAsync(_service, testObject1, testObject2, resourceType, id, userId));
        }

    }

    class Logger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {

        }
    }
}
