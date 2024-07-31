
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static class Embedding
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string apikey = "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";
    private static readonly string url = "https://api.openai.com/v1/embeddings";
    private static readonly string similarityUrl = "https://api.openai.com/v1/engines/text-similarity-babbage-001/embeddings";
    public static async Task<JToken> EmbedText(string text)
    {
        var payload = new
        {
            input = text,
            model = "text-embedding-3-large",
        };
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {Embedding.apikey}");
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, content);
            var responseString  = await response.Content.ReadAsStringAsync();
            var rootObject = JsonConvert.DeserializeObject<JObject>(responseString);
            var embedding = rootObject?["data"]?[0]?["embedding"];
            
            return embedding;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            return $"Exception: {e.Message}";
        }
    }

    public static async Task<double> GetTextSimilarity(dynamic text1, dynamic text2)
    {
        List<dynamic> comparision = new List<dynamic> { text1, text2 };
        var payload = new
        {
            input = comparision,
            model = "text-similarity-babbage-001" 
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");
            var response = await client.PostAsync(similarityUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<JObject>(responseString);
            var similarity = responseObject["data"]?[0]?["similarity"]?.Value<double>() ?? 0.0;
            return similarity;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");
            return 0.0;
        }
    }

    public static async Task<string?> getTheBestMatch(dynamic array_origin)
    {
        Console.WriteLine("start getting documents");
        var documents = await Database.getDocuments();
        Console.WriteLine("getting documents done");
        if (documents == null)
        {
            throw new InvalidOperationException("No documents found in the database.");
        }

        double best_similarity = Double.NegativeInfinity;
        string bestMatch = null;
        foreach (var document in documents)
        {
            var embeddingElement = document.GetElement("embedding");
            if (embeddingElement == null)
            {
                continue;
            }
            var embedding = embeddingElement.Value.ToString();
            var similarity = await Embedding.GetTextSimilarity(array_origin, embedding);
            if (similarity > best_similarity)
            {
                best_similarity = similarity;
                bestMatch = document.GetElement("title").Value?.ToString();
                Console.WriteLine("best match is: " + bestMatch);
                Console.WriteLine("still searching");
            }
        }
        return bestMatch;
    }
}
