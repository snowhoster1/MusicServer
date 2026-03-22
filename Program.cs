using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Ensure music directory exists
var musicPath = Path.Combine(AppContext.BaseDirectory, "music");
if (!Directory.Exists(musicPath))
{
    Directory.CreateDirectory(musicPath);
}

// Also check in ContentRootPath
var contentRootMusic = Path.Combine(builder.Environment.ContentRootPath, "music");
if (!Directory.Exists(contentRootMusic))
{
    Directory.CreateDirectory(contentRootMusic);
}

Console.WriteLine($"Music path: {musicPath}");
Console.WriteLine($"Content root: {builder.Environment.ContentRootPath}");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
}

app.UseRouting();

app.UseAuthorization();

// Serve music files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(musicPath),
    RequestPath = "/music"
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
