using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace PaderbornUniversity.SILab.Hip.EventSourcing.Mongo
{
    /// <summary>
    /// Converts between <see cref="EntityId"/> and BSON.
    /// An <see cref="EntityId"/> is represented as a string "ResourceType ID", e.g. "Exhibit 5".
    /// </summary>
    public class EntityIdSerializer : StructSerializerBase<EntityId>
    {
        public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, EntityId value)
        {
            if (value == EntityId.None)
            {
                context.Writer.WriteNull();
            }
            else
            {
                context.Writer.WriteString($"{value.Type.Name} {value.Id}");
            }
        }

        public override EntityId Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
        {
            switch (context.Reader.CurrentBsonType)
            {
                case BsonType.Null:
                    context.Reader.ReadNull();
                    return EntityId.None;

                case BsonType.String:
                    var parts = context.Reader.ReadString().Split(new[] { ' ' }, 2);

                    if (parts.Length == 2 &&
                        ResourceType.TryParse(parts[0], out var resourceType) &&
                        int.TryParse(parts[1], out var id))
                    {
                        return new EntityId(resourceType, id);
                    }
                    break;
            }

            throw CreateCannotBeDeserializedException();
        }
    }
}
