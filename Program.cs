
class Program
{
    public static async Task Main(String[] args)
    {
        Console.WriteLine("starting Program:");
        string connectionString = "mongodb://localhost:27017/";
        Database db = new Database(connectionString);
        await Database.connectionToDB();
        var embeddedText = await Embedding.EmbedText("Ukrain war in the united state");
        var result  = await Embedding.getTheBestMatch(embeddedText);
        Console.WriteLine("best match is: "+result);
        
    }
}
