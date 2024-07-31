using MongoDB.Bson;
using MongoDB.Driver;


public class Database
{
    private static MongoClient? client ;
    private static IMongoDatabase? db;
    public static IMongoCollection<BsonDocument>? collection;
    private static string? connectionString;
    private static dynamic? documents;

    
    public Database(string ApiConnection)
    {
        connectionString = ApiConnection;
    }

    public static async Task connectionToDB()
    {
        var setting = MongoClientSettings.FromConnectionString(connectionString);
        setting.ServerApi = new ServerApi(ServerApiVersion.V1);
        try
        {
            Console.WriteLine("connectiong to DB");
            client = new MongoClient(setting);
            var result = client.GetDatabase("test").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
            Console.WriteLine(result);
            db = client.GetDatabase("test");
            collection = db.GetCollection<BsonDocument>("exceldatas");
            Console.WriteLine("connected to DB");
        }
        catch(Exception e)
        {
            Console.WriteLine("failed to connect to database");
        }

    }

    public static async Task<List<BsonDocument>> getDocuments()
    {
        documents = await collection.Find(new BsonDocument()).ToListAsync();
        return documents;
    }
}
