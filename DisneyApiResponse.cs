using System.Text.Json.Serialization;

namespace ConsumerDisneyIdApi;

// /character retorna uma lista em "data" e a paginação em "info".
internal class DisneyApiResponse
{
    [JsonPropertyName("info")]
    public DisneyPageInfo? Info { get; set; }

    [JsonPropertyName("data")]
    public List<DisneyCharacter?>? Data { get; set; }
}

internal class DisneyPageInfo
{
    [JsonPropertyName("totalPages")]
    public int TotalPages { get; set; }
}

internal class DisneyCharacter
{
    [JsonPropertyName("_id")]
    public int Id { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }
}
