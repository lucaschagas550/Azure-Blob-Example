using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;

//IMPORTANTE
//Na Azure em SETTINGS => Configuration "Allow Blob anonymous access" deixar com Enable, para ter acesso as imagens
// Ideia eh retornar a Url da imagem a ser exibida com => string imageUrl = $"{baseUrl}/{containerName}/{blob.Name}";

string storageAccountName = "machiningdev"; // Nome do servico
string storageAccountKey = "key"; //Esta em acess key

var storageSharedKeyCredential = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
var blobServiceClient = new BlobServiceClient(new Uri("https://machiningdev.blob.core.windows.net/"), storageSharedKeyCredential);

string containerName = "quickstartblobstest";
var containerClient = blobServiceClient.GetBlobContainerClient(containerName);

var blobClientt = containerClient.GetBlobClient("superman.jpg");

// Defina as permissões e o tempo de expiração para a SAS URL
var sasBuilder = new BlobSasBuilder()
{
    BlobContainerName = containerClient.Name,
    BlobName = blobClientt.Name,
    Resource = "b",
    StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5), // Início da validade
    ExpiresOn = DateTimeOffset.UtcNow.AddHours(1), // Tempo de expiração
};

sasBuilder.SetPermissions(BlobSasPermissions.Read); // Permissão de leitura

// Gere a SAS URL
string sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(storageAccountName, storageAccountKey)).ToString();
string blobSasUri = blobClientt.Uri + "?" + sasToken;

//Cria o container
if (!await containerClient.ExistsAsync())
{
    // Se o contêiner não existir, crie-o.
    var container = await containerClient.CreateAsync(PublicAccessType.BlobContainer);
    Console.WriteLine($"Contêiner '{containerName}' criado com sucesso.");
}
else
{
    Console.WriteLine($"O contêiner '{containerName}' já existe.");
}


string blobName = "123-Teste.png";
var blobClient = containerClient.GetBlobClient(blobName); // obtem uma imagem

// Verifique se o blob existe
if (!await blobClient.ExistsAsync())
{
    Console.WriteLine($"O blob '{blobName}' não existe no contêiner '{containerName}'.");
}

// Faça o download do conteúdo do blob (imagem PNG) para um fluxo de memória (stream)
//var  blobDownloadInfo = await blobClient.OpenReadAsync();
//var img = containerClient.GetBlobClient(blobName);
//var imageStream = img.Download();

//using (var memoryStream = new MemoryStream())
//{
//    await imageStream.Value.Content.CopyToAsync(memoryStream);

//    // Agora você tem a imagem PNG em um MemoryStream e pode exibi-la ou salvá-la como necessário.
//    // Para exibi-la, você pode usar uma biblioteca gráfica ou um controle de imagem em sua aplicação.
//    // Aqui, vamos apenas salvar a imagem em um arquivo.

//    string outputPath = "imagem.png";
//    File.WriteAllBytes(outputPath, memoryStream.ToArray());

//    Console.WriteLine($"Imagem '{blobName}' salva em '{outputPath}'.");
//}




// Liste os blobs no contêiner com base no termo de pesquisa
string searchTerm = "superman.jpg";
var blobList = new List<BlobItem>();
await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: searchTerm).ConfigureAwait(false))
{
    // Adicione o blob à lista
    blobList.Add(blobItem);
}

//Copia a imagem localmente
foreach (var blob in blobList)
{
    // string imageUrl = $"{baseUrl}/{containerName}/{blob.Name}";
    string localFilePath = Path.Combine(@$"D:\Projetos de Exemplo\Blob.Azure.Example\Images", blob.Name);
    var blobDownloadInfo2 = await containerClient.GetBlobClient(blob.Name).DownloadAsync();

    using (var fileStream = File.OpenWrite(localFilePath))
    {
        await blobDownloadInfo2.Value.Content.CopyToAsync(fileStream);
        fileStream.Close();
    }

    Console.WriteLine($"Blob '{blob.Name}' copiado para '{localFilePath}'.");
}





//Inserir uma lista de imagens
// Lista de caminhos para as imagens que você deseja fazer upload.
List<string> imagePaths = new List<string>
        {
            @$"D:\Projetos de Exemplo\Blob.Azure.Example\Enviar\enviou-erza.jpg",
            @$"D:\Projetos de Exemplo\Blob.Azure.Example\Enviar\enviou-Teste.png",
            // Adicione mais caminhos de imagens conforme necessário.
        };

foreach (string imagePath in imagePaths)
{
    var name = Path.GetFileName(imagePath); // Use o nome do arquivo como o nome do blob

    // Obtenha uma referência ao blob
    var client = containerClient.GetBlobClient(name);

    // Faça upload da imagem
    using (var fileStream = File.OpenRead(imagePath))
    {
        await client.UploadAsync(fileStream, true);
        Console.WriteLine($"Imagem '{name}' carregada com sucesso.");
    }
}


// Create the container and return a container client object
Console.WriteLine("Hello, World!");