
class Program
{
    public static async Task Main(String[] args)
    {
        Console.WriteLine("starting Program:");
        string connectionString = "mongodb+srv://wassim:wassimoux30@cluster0.3pj8tye.mongodb.net/?retryWrites=true&w=majority&appName=Cluster0";
        Database db = new Database(connectionString);
        await Database.connectionToDB();
        var result = await Embedding.EmbedText("Ukrain war in the united state");
        Console.WriteLine("embedded text; "+result);
        Console.WriteLine(await Embedding.getTheBestMatch(result));
    }
}
