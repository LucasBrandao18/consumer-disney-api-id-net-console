# consumer-disney-api-id-net-console


Aplicação de console em C# e .NET que consulta a Disney API e **sorteia um
personagem a cada execução**. O nome e a URL da imagem aparecem no terminal;
uma página com o nome e a imagem abre no navegador padrão.

## Atividade original e atualização

Solictado:

- a) Criar uma aplicação de console em C# que consulte a Disney API.
  O exemplo original usa `/character/423`, correspondente a Big Bad Wolf.
- b) Imprimir o nome e a URL da imagem retornados pela API.

A lista permite equipes de até cinco alunos e determina entregas individuais
no prazo do professor. A publicação e a entrega serão feitas manualmente.

Conforme a atualização solicitada, o projeto agora usa a listagem
`https://api.disneyapi.dev/character`, sem um ID fixo, e sorteia entre os
personagens disponíveis com nome e imagem válidos.

## Como executar

Requisitos: **SDK do .NET 10**, acesso à internet e navegador padrão
configurado para abrir arquivos HTML. Não há pacotes NuGet adicionais nem
necessidade de chave de API.

No VS Code, abra esta pasta e use **Run** no projeto C# (com a extensão C#
instalada), ou execute no terminal:

```powershell
dotnet run --project ConsumerDisneyIdApi.csproj
```

Também é possível entrar na pasta pelo PowerShell:

```powershell
Set-Location 'C:\Users\lucas\OneDrive\Documentos\vs code uni\homework-disneyapi'
dotnet run
```

Cada novo Run consulta a API novamente e realiza um novo sorteio. Como em
qualquer sorteio independente, um personagem pode se repetir por acaso;
não há personagem fixo nem semente constante no código.

Para compilar sem executar:

```powershell
dotnet build
```

Para executar sem abrir automaticamente o navegador:

```powershell
dotnet run -- --no-browser
```

Essa opção ainda imprime os dados e o caminho da página HTML gerada, que
pode ser aberta manualmente. Não altera o sorteio.

## Funcionamento

1. `Program.Main` aguarda as operações com `async/await`.
2. `DisneyApiClient` consulta `/character?page=1&pageSize=1000` e lê
   `info.totalPages`. Em seguida, consulta as demais páginas por HTTPS,
   em grupos de até três requisições simultâneas, exibindo o progresso.
   Assim, o sorteio abrange o catálogo retornado, e não apenas a primeira página.
3. `DisneyApiResponse.Data` representa uma lista de personagens. Os atributos
   `JsonPropertyName` mapeiam `_id`, `name` e `imageUrl` para o modelo C#.
4. Registros sem nome ou sem URL HTTP/HTTPS de imagem são descartados, e IDs
   duplicados são removidos.
5. `Random.Shared.Next(candidates.Length)` sorteia um índice válido da lista,
   dando a cada candidato a mesma chance, sem semente constante.
6. O console mostra `Nome:` e `Imagem:` com os valores da própria API.
7. Uma página `ConsumerDisneyIdApi/personagem.html` na pasta temporária do
   sistema exibe o nome e carrega a imagem diretamente da URL retornada.
   Essa página é atualizada a cada execução e aberta no navegador padrão.

O console de texto não renderiza imagens convencionais. Por isso a imagem
é exibida no navegador, junto com o nome. Nenhuma imagem local fixa é usada.
O HTML escapa os valores recebidos da API e informa caso a imagem não carregue
no navegador. O carregamento é feito pelo próprio navegador: o servidor das
imagens pode recusar requisições de clientes HTTP de console, mesmo quando
a mesma URL funciona no navegador.

## Tratamento de falhas

- API indisponível ou HTTP de erro: informa a falha de consulta.
- Tempo limite: até 30 segundos por consulta HTTP e dois minutos para a
  consulta completa ao catálogo.
- JSON inválido, `data` ausente ou paginação inválida: informa formato inválido.
- Lista vazia: informa que nenhum personagem foi retornado.
- Nome ou imagem ausentes: descarta o registro e procura outro candidato.
- Nenhum registro com nome e URL de imagem válidos: informa a ausência de candidatos.
- Imagem indisponível no navegador: mostra uma mensagem e mantém o link
  original disponível, sem substituir por uma imagem local. Um novo Run
  realiza outro sorteio.
- Falha ao gravar ou abrir a página: mantém o nome e a URL no console para
  que a imagem possa ser aberta manualmente.

O programa encerra com código `0` quando a consulta e a abertura da visualização
são concluídas, e `1` em caso de erro nessas etapas. O carregamento da imagem
acontece depois, no navegador; uma falha ali é informada na página, sem alterar
o código de saída do console. A imagem depende do servidor externo.

## Arquivos

| Arquivo | Responsabilidade |
| --- | --- |
| `ConsumerDisneyIdApi.csproj` | Configuração do projeto de console .NET 10. |
| `Program.cs` | Execução, mensagens, geração da página e abertura da imagem. |
| `DisneyApiClient.cs` | Consulta paginada, filtro de URLs válidas e sorteio. |
| `DisneyApiResponse.cs` | Modelos da lista de personagens e da paginação. |
| `.gitignore` | Ignora arquivos de compilação e configurações locais. |
| `README.md` | Atividade, funcionamento e comandos de execução. |

