using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo.Test
{
    /// <summary>
    /// Provides methods to construct a fake update definition which, on execution,
    /// updates entities via reflection. To be used for testing purposes.
    /// </summary>
    public class FakeMongoUpdater<T> : IMongoUpdater<T>
    {
        private readonly List<Action<T>> _updateActions = new List<Action<T>>();

        public IReadOnlyList<Action<T>> UpdateActions => _updateActions;

        public void Add<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            _updateActions.Add(o => AddToCollection(o, GetMemberName(field), Enumerable.Repeat(value, 1), semantic));
        }

        public void Add(string field, object value, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            _updateActions.Add(o => AddToCollection(o, field, Enumerable.Repeat(value, 1), semantic));
        }

        public void AddRange<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            _updateActions.Add(o => AddToCollection(o, GetMemberName(field), values, semantic));
        }

        public void AddRange(string field, IEnumerable<object> values, CollectionSemantic semantic = CollectionSemantic.Bag)
        {
            _updateActions.Add(o => AddToCollection(o, field, values, semantic));
        }

        public void Remove<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, TItem value)
        {
            _updateActions.Add(o => RemoveFromCollection(o, GetMemberName(field), Enumerable.Repeat(value, 1)));
        }

        public void Remove(string field, object value)
        {
            _updateActions.Add(o => RemoveFromCollection(o, field, Enumerable.Repeat(value, 1)));
        }

        public void RemoveRange<TItem>(Expression<Func<T, IEnumerable<TItem>>> field, IEnumerable<TItem> values)
        {
            _updateActions.Add(o => RemoveFromCollection(o, GetMemberName(field), values));
        }

        public void RemoveRange(string field, IEnumerable<object> values)
        {
            _updateActions.Add(o => RemoveFromCollection(o, field, values));
        }

        public void Set<TField>(Expression<Func<T, TField>> field, TField value)
        {
            _updateActions.Add(o => AssignValue(o, GetMemberName(field), value));
        }

        public void Set(string field, object value)
        {
            _updateActions.Add(o => AssignValue(o, field, value));
        }


        private string GetMemberName<TField>(Expression<Func<T, TField>> expression) =>
            expression?.Body is MemberExpression memberExp
                ? memberExp.Member.Name
                : throw new ArgumentException("The expression is not a valid member expression", nameof(expression));

        private MemberInfo GetPropertyOrField(T entity, string propertyName) =>
            entity.GetType().GetProperty(propertyName) ??
            entity.GetType().GetField(propertyName) as MemberInfo ??
            throw new InvalidOperationException($"The entity does not have a property or field '{propertyName}'");

        private IList GetList(T entity, string propertyName)
        {
            var property = GetPropertyOrField(entity, propertyName);

            var collection =
                property is PropertyInfo prop ? prop.GetValue(entity) :
                property is FieldInfo field ? field.GetValue(entity) :
                throw new ArgumentException();

            if (collection is IList list)
                return list;

            throw new InvalidOperationException($"'{entity.GetType().Name}.{property.Name}' does not refer to a mutable list structure");
        }

        private void AddToCollection(T entity, string propertyName, IEnumerable values, CollectionSemantic collectionSemantic)
        {
            var list = GetList(entity, propertyName);

            foreach (var value in values)
            {
                switch (collectionSemantic)
                {
                    case CollectionSemantic.Bag:
                        list.Add(value);
                        break;

                    case CollectionSemantic.Set:
                        if (!list.Contains(value))
                            list.Add(value);
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void RemoveFromCollection(T entity, string propertyName, IEnumerable values)
        {
            var list = GetList(entity, propertyName);

            foreach (var value in values)
            {
                while (list.Contains(value))
                    list.Remove(value);
            }
        }

        private void AssignValue(T entity, string propertyName, object value)
        {
            var propertyOrField = GetPropertyOrField(entity, propertyName);

            if (propertyOrField is PropertyInfo property)
            {
                property.SetValue(entity, value);
            }
            else if (propertyOrField is FieldInfo field)
            {
                field.SetValue(entity, value);
            }
        }
    }
}
