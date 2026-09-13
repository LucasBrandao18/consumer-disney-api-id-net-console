using System.Net.Http.Json;
using System.Text.Json;

namespace ConsumerDisneyIdApi;

internal class DisneyApiClient(HttpClient client, Action<string>? reportProgress = null)
{
    public async Task<DisneyCharacter> GetRandomCharacterAsync(
        CancellationToken cancellationToken = default)
    {
        DisneyApiResponse firstPage = await GetPageAsync(1, cancellationToken);
        int totalPages = firstPage.Info?.TotalPages
            ?? throw new JsonException("A API não informou a paginação.");

        if (totalPages < 0 || (totalPages == 0 && firstPage.Data!.Count > 0))
        {
            throw new JsonException("A API retornou uma paginação inválida.");
        }

        List<DisneyCharacter?> characters = new(firstPage.Data!);
        reportProgress?.Invoke($"Lendo {totalPages} página(s) da API...");

        // Inclui todas as páginas no sorteio e mantém as requisições em HTTPS.
        for (int page = 2; page <= totalPages; page += 3)
        {
            // Até três consultas simultâneas reduzem a espera pela rede.
            DisneyApiResponse[] responses = await Task.WhenAll(
                Enumerable.Range(page, Math.Min(3, totalPages - page + 1))
                    .Select(number => GetPageAsync(number, cancellationToken)));

            foreach (DisneyApiResponse response in responses)
            {
                characters.AddRange(response.Data!);
            }

            reportProgress?.Invoke($"Páginas lidas: {Math.Min(page + 2, totalPages)}/{totalPages}.");
        }

        if (!characters.Any(character => character is not null))
        {
            throw new InvalidOperationException("A API não retornou nenhum personagem.");
        }

        DisneyCharacter[] candidates = characters
            .OfType<DisneyCharacter>()
            .Where(character => !string.IsNullOrWhiteSpace(character.Name)
                && IsWebUrl(character.ImageUrl))
            .DistinctBy(character => character.Id)
            .ToArray();

        if (candidates.Length == 0)
        {
            throw new InvalidOperationException(
                "Nenhum personagem possui nome e URL de imagem válidos.");
        }

        // Não usa semente fixa. Cada processo faz um novo sorteio independente.
        reportProgress?.Invoke($"Sorteando entre {candidates.Length} personagens com nome e imagem...");
        cancellationToken.ThrowIfCancellationRequested();
        return candidates[Random.Shared.Next(candidates.Length)];
    }

    private async Task<DisneyApiResponse> GetPageAsync(
        int page, CancellationToken cancellationToken)
    {
        DisneyApiResponse? response = await client.GetFromJsonAsync<DisneyApiResponse>(
            $"https://api.disneyapi.dev/character?page={page}&pageSize=1000",
            cancellationToken);

        if (response?.Data is null)
        {
            throw new JsonException("A API não retornou uma lista de personagens em data.");
        }

        return response;
    }

    private static bool IsWebUrl(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out Uri? uri)
            && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp);
    }

}
