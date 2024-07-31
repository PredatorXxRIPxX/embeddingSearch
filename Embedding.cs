
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


public static class Embedding
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string apikey = "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";
    private static readonly string url = "https://api.openai.com/v1/embeddings";
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

    public static async Task<double> GetTextSimilarity(string text1, string text2)
    {
        var payload = new
        {
            input = new[] { text1, text2 }
        };
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");
            var response = await client.PostAsync("https://api.openai.com/v1/engines/text-similarity-davinci-001/embeddings", content);
            var responseString = await response.Content.ReadAsStringAsync();
            var responseObject = JsonConvert.DeserializeObject<JObject>(responseString);

            double similarity = responseObject["data"]?[0]?["similarity"]?.Value<double>() ?? 0.0;
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
        Console.WriteLine("going in the loop");
        foreach (var document in documents)
        {
            var result = await GetTextSimilarity(array_origin.toString(), document.GetElement("embedding").ToString());
            Console.WriteLine(result);
            if (best_similarity < result)
            {
                best_similarity = result;
                bestMatch = document.GetElement("title").Value.ToString();
            }
            
        }
        return bestMatch;
    }
}
