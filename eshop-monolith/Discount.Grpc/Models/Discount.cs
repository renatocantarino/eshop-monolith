using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Discount.Grpc.Models;

public class Discount
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string? Id { get; set; }

    public int ProductId { get; set; }
    public string Code { get; set; }

    [BsonRepresentation(BsonType.Decimal128)]
    public decimal Amount { get; set; }
}