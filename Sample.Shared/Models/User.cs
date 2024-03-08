using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Sample.Shared.Models
{
     
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        public string Name { get; set; }

        public string Email { get; set; }
        public bool IsActive { get; set; }

         
    }

}
