using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;

namespace DeveloperStore.Infrastructure.ReadModel;

public class SalesReadModel
{
    private readonly IMongoDatabase _db;
    public SalesReadModel(IMongoDatabase db) => _db = db;

    private IMongoCollection<SaleDoc> Coll => _db.GetCollection<SaleDoc>("sales");

    public async Task UpsertSaleAsync(SaleDoc doc, CancellationToken ct)
        => await Coll.ReplaceOneAsync(x => x.Id == doc.Id, doc, new ReplaceOptions { IsUpsert = true }, ct);

    public async Task DeleteSaleAsync(int id, CancellationToken ct)
        => await Coll.DeleteOneAsync(x => x.Id == id, ct);

    public async Task<List<BranchDailySummary>> GetDailySummaryAsync(DateOnly? from, DateOnly? to, CancellationToken ct)
    {
        var filter = Builders<SaleDoc>.Filter.Empty;
        if (from.HasValue) filter &= Builders<SaleDoc>.Filter.Gte(x => x.Date, from.Value.ToDateTime(TimeOnly.MinValue));
        if (to.HasValue) filter &= Builders<SaleDoc>.Filter.Lte(x => x.Date, to.Value.ToDateTime(TimeOnly.MaxValue));

        var results = await Coll.Aggregate()
            .Match(filter)
            .Group(new BsonDocument {
                { "_id", new BsonDocument {
                    { "branchId", "$BranchId" },
                    { "branchName", "$BranchName" },
                    { "date", new BsonDocument("$dateToString", new BsonDocument { { "format", "%Y-%m-%d" }, { "date", "$Date" } }) }
                }},
                { "total", new BsonDocument("$sum", "$Total") }
            })
            .Project(new BsonDocument {
                { "BranchId", "$_id.branchId" },
                { "BranchName", "$_id.branchName" },
                { "Date", "$_id.date" },
                { "Total", "$total" },
                { "_id", 0 }
            })
            .Sort(new BsonDocument { { "BranchId", 1 }, { "Date", 1 } })
            .As<BranchDailySummary>()
            .ToListAsync(ct);

        return results;
    }
}

public class SaleDoc
{
    [BsonId] public int Id { get; set; }
    public string Number { get; set; } = "";
    public DateTime Date { get; set; }
    public int CustomerId { get; set; }
    public string CustomerName { get; set; } = "";
    public int BranchId { get; set; }
    public string BranchName { get; set; } = "";
    public decimal Total { get; set; }
    public bool Cancelled { get; set; }
}

public class BranchDailySummary
{
    public int BranchId { get; set; }
    public string BranchName { get; set; } = "";
    public string Date { get; set; } = "";
    public decimal Total { get; set; }
}
