
using System.Text;
using MongoDB.Bson;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using MongoDB.Driver;
using FaissMask;

public static class Embedding
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly string apikey = "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";
    private static readonly string url = "https://api.openai.com/v1/embeddings";
    private static readonly string similarityUrl = "https://api.openai.com/v1/engines/text-similarity-babbage-001/embeddings";
    public static async Task<List<float>> EmbedText(string text)
    {
        var payload = new
        {
            input = text,
            model = "text-embedding-3-large",
        };
        client.DefaultRequestHeaders.Clear();
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apikey}");
        var json = JsonConvert.SerializeObject(payload);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            var response = await client.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            var rootObject = JsonConvert.DeserializeObject<JObject>(responseString);
            var embeddingToken = rootObject?["data"]?[0]?["embedding"];

            if (embeddingToken != null)
            {
                var jsonArray = (JArray)embeddingToken;
                return jsonArray.Select(item => item.Value<float>()).ToList();
            }

            return null;
        }
        catch (Exception e)
        {
            Console.WriteLine("Error: " + e.Message);
            return null;
        }
    }
    

    private static double CosineSimilarity(List<float> vec1, List<float> vec2)
    {
        if (vec1 == null || vec2 == null || vec1.Count != vec2.Count)
            return 0.0;

        double dotProduct = 0.0;
        double magnitude1 = 0.0;
        double magnitude2 = 0.0;

        for (int i = 0; i < vec1.Count; i++)
        {
            dotProduct += vec1[i] * vec2[i];
            magnitude1 += Math.Pow(vec1[i], 2);
            magnitude2 += Math.Pow(vec2[i], 2);
        }

        magnitude1 = Math.Sqrt(magnitude1);
        magnitude2 = Math.Sqrt(magnitude2);

        if (magnitude1 == 0 || magnitude2 == 0)
            return 0.0;

        return dotProduct / (magnitude1 * magnitude2);
    }

    public static async Task<string?> FindBestMatchUsingFaiss(List<float> queryEmbedding)
    {
        var documents = await Database.getDocuments();
        if (documents == null || documents.Count == 0)
        {
            throw new InvalidOperationException("No documents found in the database.");
        }

        long dimension = documents.Count;
        var index = new IndexFlatL2(dimension);
        Console.WriteLine("Adding embeddings to the index...");
        foreach (var doc in documents)
        {
            if (doc.TryGetValue("embedding", out BsonValue embeddingValue))
            {
                var embeddingArray = JArray.Parse(embeddingValue.AsString);
                var embedding = embeddingArray.Select(item => item.Value<float>()).ToArray();
                index.Add(embedding);
            }
        }
        Console.WriteLine("Indexing complete.");
        Console.WriteLine("Searching for the best match...");

        int k = 1;
        var distances = new float[k];
        var indices = new long[k];
        index.Search(queryEmbedding.ToArray(), k);

        if (indices.Length > 0 && indices[0] >= 0 && indices[0] < documents.Count)
        {
            return documents[(int)indices[0]]["title"].AsString;
        }

        return null;
    }


    public static async Task<string?> GetTheBestMatch(List<float> arrayOrigin)
    {
        var documents = await Database.getDocuments();
        if (documents == null || documents.Count == 0)
        {
            throw new InvalidOperationException("No documents found in the database.");
        }

        double bestSimilarity = double.NegativeInfinity;
        string bestMatch = null;  
        foreach (var document in documents)
        {
            if (document.TryGetValue("embedding", out BsonValue embeddingValue))
            {
                var embeddingArray = JArray.Parse(embeddingValue.AsString);
                var embeddingList = embeddingArray.Select(item => item.Value<float>()).ToList();
                var similarity = CosineSimilarity(embeddingList, arrayOrigin);
                if (similarity > bestSimilarity)
                {
                    bestSimilarity = similarity;
                    bestMatch = document.GetValue("title")?.AsString;
                }
            }
        }

        return bestMatch;
    }
}
