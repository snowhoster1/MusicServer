var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();

// Add static file serving for music
builder.Services.AddDirectoryBrowser();

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
    FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(
        Path.Combine(AppContext.BaseDirectory, "music")),
    RequestPath = "/music"
});

app.MapStaticAssets();
app.MapRazorPages()
   .WithStaticAssets();

app.Run();
