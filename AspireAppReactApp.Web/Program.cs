using AspireAppReactApp.Web;
using AspireAppReactApp.Web.Components;
using Azure.Storage.Blobs;
using Azure.Storage.Queues;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();


builder.AddAzureBlobServiceClient("blobs");
builder.AddAzureQueueServiceClient("queues");

builder.Services.AddSingleton<QueueMessageHandler>();
builder.Services.AddHostedService<StorageWorker>();


builder.Services.AddSingleton(
    static provider => provider.GetRequiredService<QueueServiceClient>().GetQueueClient("thumbnail-queue"));
builder.Services.AddKeyedSingleton(
    "images", static (provider, _) => provider.GetRequiredService<BlobServiceClient>().GetBlobContainerClient("images"));
builder.Services.AddKeyedSingleton(
    "thumbnails", static (provider, _) => provider.GetRequiredService<BlobServiceClient>().GetBlobContainerClient("thumbnails"));


builder.Services.AddOutputCache();

builder.Services.AddHttpClient<WeatherApiClient>(client =>
    {
        // This URL uses "https+http://" to indicate HTTPS is preferred over HTTP.
        // Learn more about service discovery scheme resolution at https://aka.ms/dotnet/sdschemes.
        client.BaseAddress = new("https+http://apiservice");
    });

builder.Services.AddHttpClient<TodoApiClient>(client =>
{
    client.BaseAddress = new("https+http://apiservice");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseAntiforgery();

app.UseOutputCache();

app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.MapDefaultEndpoints();

app.MapGet(ImageUrl.RoutePattern, async (string slug, bool? thumbnail, IServiceProvider services, CancellationToken cancellationToken) =>
{
    var containerClient = services.GetRequiredKeyedService<BlobContainerClient>(thumbnail == true ? "thumbnails" : "images");
    var blobClient = containerClient.GetBlobClient(slug);

    if (!await blobClient.ExistsAsync(cancellationToken))
    {
        return Results.NotFound();
    }

    var properties = (await blobClient.GetPropertiesAsync(cancellationToken: cancellationToken)).Value;

    return Results.Stream(destination => blobClient.DownloadToAsync(destination, cancellationToken: cancellationToken),
        contentType: properties.ContentType,
        lastModified: properties.LastModified,
        entityTag: new(properties.ETag.ToString("H")));
});
app.Run();
