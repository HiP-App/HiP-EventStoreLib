using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    public class MongoUpdater<T> : IMongoUpdater<T>
    {
        public UpdateDefinition<T> Update { get; private set; } = "";

        public void Add<TField>(Expression<Func<T, IEnumerable<TField>>> field, TField value, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            switch (semantic)
            {
                case CollectionSemantic.Bag: Update = Update.Push(field, value); break;
                case CollectionSemantic.Set: Update = Update.AddToSet(field, value); break;
                default: throw new NotImplementedException();
            }
        }

        public void Add(string field, object value, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            switch (semantic)
            {
                case CollectionSemantic.Bag: Update = Update.Push(field, value); break;
                case CollectionSemantic.Set: Update = Update.AddToSet(field, value); break;
                default: throw new NotImplementedException();
            }
        }

        public void AddRange<TField>(Expression<Func<T, IEnumerable<TField>>> field, IEnumerable<TField> values, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            switch (semantic)
            {
                case CollectionSemantic.Bag: Update = Update.PushEach(field, values); break;
                case CollectionSemantic.Set: Update = Update.AddToSetEach(field, values); break;
                default: throw new NotImplementedException();
            }
        }

        public void AddRange(string field, IEnumerable<object> values, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            switch (semantic)
            {
                case CollectionSemantic.Bag: Update = Update.PushEach(field, values); break;
                case CollectionSemantic.Set: Update = Update.AddToSetEach(field, values); break;
                default: throw new NotImplementedException();
            }
        }

        public void Remove<TField>(Expression<Func<T, IEnumerable<TField>>> field, TField value)
        {
            Update = Update.Pull(field, value);
        }

        public void Remove(string field, object value)
        {
            Update = Update.Pull(field, value);
        }

        public void RemoveRange<TField>(Expression<Func<T, IEnumerable<TField>>> field, IEnumerable<TField> values)
        {
            Update = Update.PullAll(field, values);
        }

        public void RemoveRange(string field, IEnumerable<object> values)
        {
            Update = Update.PullAll(field, values);
        }

        public void Set<TField>(Expression<Func<T, TField>> field, TField value)
        {
            Update = Update.Set(field, value);
        }

        public void Set(string field, object value)
        {
            Update = Update.Set(field, value);
        }
    }

}
