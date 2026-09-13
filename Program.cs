using System.ComponentModel;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Text.Json;

namespace ConsumerDisneyIdApi;

internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;

        using HttpClient client = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };
        using CancellationTokenSource timeout = new(TimeSpan.FromMinutes(2));

        try
        {
            Console.WriteLine("Consultando os personagens da Disney...");
            DisneyCharacter character = await new DisneyApiClient(client, Console.WriteLine)
                .GetRandomCharacterAsync(timeout.Token);

            Console.WriteLine("Nome:");
            Console.WriteLine(character.Name);
            Console.WriteLine("Imagem:");
            Console.WriteLine(character.ImageUrl);

            // O console exibe texto; a página abre a imagem remota no navegador.
            string previewPath = await SavePreviewAsync(character);
            Console.WriteLine($"Visualização: {previewPath}");

            // Permite testes e execução em ambientes sem navegador.
            if (!args.Contains("--no-browser", StringComparer.OrdinalIgnoreCase))
            {
                Process.Start(new ProcessStartInfo(previewPath)
                {
                    UseShellExecute = true
                });
            }

            return 0;
        }
        catch (HttpRequestException)
        {
            Console.Error.WriteLine(
                "Não foi possível consultar a Disney API. Verifique sua conexão e tente novamente.");
        }
        catch (OperationCanceledException)
        {
            Console.Error.WriteLine("A consulta demorou demais. Tente novamente.");
        }
        catch (JsonException)
        {
            Console.Error.WriteLine("A Disney API retornou dados em um formato inválido.");
        }
        catch (InvalidOperationException exception)
        {
            Console.Error.WriteLine(exception.Message);
        }
        catch (Exception exception) when (
            exception is IOException or UnauthorizedAccessException or Win32Exception)
        {
            Console.Error.WriteLine(
                "Não foi possível abrir a visualização. Abra a URL da imagem exibida no console.");
        }

        return 1;
    }

    internal static async Task<string> SavePreviewAsync(DisneyCharacter character)
    {
        string name = WebUtility.HtmlEncode(character.Name ?? string.Empty);
        string imageUrl = WebUtility.HtmlEncode(character.ImageUrl ?? string.Empty);
        string html = $$"""
            <!doctype html>
            <html lang="pt-BR">
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1">
                <title>{{name}} | Personagem Disney</title>
                <style>
                    body { margin: 0; padding: 32px 20px; background: #f1f5f9;
                           color: #172554; font-family: system-ui, sans-serif; text-align: center; }
                    main { max-width: 680px; margin: auto; padding: 24px;
                           background: white; border-radius: 16px; }
                    h1 { overflow-wrap: anywhere; }
                    img { display: block; max-width: 100%; max-height: 65vh;
                          width: auto; height: auto; margin: 24px auto; object-fit: contain; }
                    a { color: #1d4ed8; }
                    #image-error { color: #991b1b; }
                </style>
            </head>
            <body>
                <main>
                    <p>Personagem sorteado da Disney API</p>
                    <h1>{{name}}</h1>
                    <img src="{{imageUrl}}" alt="Imagem de {{name}}" referrerpolicy="no-referrer"
                         onerror="this.hidden = true; this.style.display = 'none'; document.getElementById('image-error').hidden = false;">
                    <p id="image-error" hidden>A imagem não pôde ser carregada. Verifique sua conexão ou execute o programa novamente.</p>
                    <p><a href="{{imageUrl}}" target="_blank" rel="noopener noreferrer">Abrir imagem original</a></p>
                    <p>Execute o projeto novamente para realizar outro sorteio.</p>
                </main>
            </body>
            </html>
            """;

        string directory = Path.Combine(Path.GetTempPath(), "ConsumerDisneyIdApi");
        Directory.CreateDirectory(directory);
        string path = Path.Combine(directory, "personagem.html");
        await File.WriteAllTextAsync(path, html, Encoding.UTF8);
        return path;
    }
}
