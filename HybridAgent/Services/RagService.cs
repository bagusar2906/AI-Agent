using System.Net.Http.Json;
using System.Text.Json;
using HybridAgent.Domain;
using HybridAgent.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace HybridAgent.Services;

public class RagService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _http;

    public RagService(AppDbContext db, IHttpClientFactory factory)
    {
        _db = db;
        _http = factory.CreateClient();
        _http.BaseAddress = new Uri("http://ollama:11434");
    }

    // =============================
    // 1️⃣ Add Document with Embedding
    // =============================
    public async Task AddDocumentAsync(string content)
    {
        var embedding = await GenerateEmbeddingAsync(content);

        var doc = new DocumentChunk
        {
            Content = content,
            EmbeddingJson = JsonSerializer.Serialize(embedding)
        };

        _db.DocumentChunks.Add(doc);
        await _db.SaveChangesAsync();
    }

    // =============================
    // 2️⃣ Search Top-K Similar
    // =============================
    public async Task<List<string>> SearchAsync(string query, int topK = 3)
    {
        var queryEmbedding = await GenerateEmbeddingAsync(query);

        var documents = await _db.DocumentChunks
            .Where(d => d.EmbeddingJson != null)
            .ToListAsync();

        var scored = documents
            .Select(d => new
            {
                d.Content,
                Score = CosineSimilarity(
                    queryEmbedding,
                    JsonSerializer.Deserialize<float[]>(d.EmbeddingJson!)!
                )
            })
            .OrderByDescending(x => x.Score)
            .Take(topK)
            .Select(x => x.Content)
            .ToList();

        return scored;
    }

    // =============================
    // 3️⃣ Generate Embedding via Ollama
    // =============================
    private async Task<float[]> GenerateEmbeddingAsync(string text)
    {
        var response = await _http.PostAsJsonAsync("/api/embeddings", new
        {
            model = "nomic-embed-text",
            prompt = text
        });

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();

        return json.GetProperty("embedding")
            .EnumerateArray()
            .Select(x => x.GetSingle())
            .ToArray();
    }

    // =============================
    // 4️⃣ Cosine Similarity
    // =============================
    private static double CosineSimilarity(float[] a, float[] b)
    {
        double dot = 0.0;
        double normA = 0.0;
        double normB = 0.0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            normA += a[i] * a[i];
            normB += b[i] * b[i];
        }

        return dot / (Math.Sqrt(normA) * Math.Sqrt(normB) + 1e-10);
    }
}