
class Program
{
    public static async Task Main(String[] args)
    {
        Console.WriteLine("starting Program:");
        string connectionString = "mongodb://localhost:27017/";
        Database db = new Database(connectionString);
        await Database.connectionToDB();
        List<float> embeddedText = await Embedding.EmbedText("Ukrain war in the united state");
        Console.WriteLine("Looking for the best match: ...");
        var result  = await Embedding.GetTheBestMatch(embeddedText);
        Console.WriteLine("best match is: "+result);
        
    }
}
