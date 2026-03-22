using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using MusicServer.Models;
using System.Text.Json;

namespace MusicServer.Pages;

public class IndexModel : PageModel
{
    private readonly IWebHostEnvironment _env;
    
    public IndexModel(IWebHostEnvironment env)
    {
        _env = env;
    }

    public List<MusicFile> MusicFiles { get; set; } = new();
    public List<string> Categories { get; set; } = new();
    public string? SelectedCategory { get; set; }
    public DateTime? SelectedDate { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public int PageSize { get; set; } = 10;

    public void OnGet(string? category, string? date, int page = 1)
    {
        var allFiles = MusicStore.LoadAll();
        
        // Get unique categories
        Categories = allFiles.Select(f => f.Category).Distinct().OrderBy(c => c).ToList();
        if (!Categories.Contains("General"))
            Categories.Insert(0, "General");

        // Filter by category
        SelectedCategory = category;
        if (!string.IsNullOrEmpty(category) && category != "All")
        {
            allFiles = allFiles.Where(f => f.Category == category).ToList();
        }

        // Filter by date
        if (!string.IsNullOrEmpty(date) && DateTime.TryParse(date, out var selectedDate))
        {
            SelectedDate = selectedDate;
            allFiles = allFiles.Where(f => f.CreatedAt.Date == selectedDate.Date).ToList();
        }

        // Sort by date descending
        allFiles = allFiles.OrderByDescending(f => f.CreatedAt).ToList();

        // Pagination
        CurrentPage = page;
        var totalItems = allFiles.Count;
        TotalPages = (int)Math.Ceiling(totalItems / (double)PageSize);
        if (TotalPages == 0) TotalPages = 1;
        if (CurrentPage < 1) CurrentPage = 1;
        if (CurrentPage > TotalPages) CurrentPage = TotalPages;

        MusicFiles = allFiles.Skip((CurrentPage - 1) * PageSize).Take(PageSize).ToList();
    }

    public IActionResult OnGetDownload(int id)
    {
        var files = MusicStore.LoadAll();
        var file = files.FirstOrDefault(f => f.Id == id);
        if (file == null)
            return NotFound();

        var filePath = Path.Combine(MusicStore.GetMusicFolder(), file.FileName);
        if (!System.IO.File.Exists(filePath))
            return NotFound();

        var stream = System.IO.File.OpenRead(filePath);
        return File(stream, "audio/mpeg", file.OriginalFileName);
    }

    public IActionResult OnGetDelete(int id)
    {
        var files = MusicStore.LoadAll();
        var file = files.FirstOrDefault(f => f.Id == id);
        if (file != null)
        {
            var filePath = Path.Combine(MusicStore.GetMusicFolder(), file.FileName);
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);
            
            files.Remove(file);
            MusicStore.SaveAll(files);
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUploadAsync(IFormFile file, string category, string description)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "請選擇檔案";
            return RedirectToPage();
        }

        MusicStore.EnsureDirectories();
        
        var files = MusicStore.LoadAll();
        var newId = files.Any() ? files.Max(f => f.Id) + 1 : 1;
        
        var extension = Path.GetExtension(file.FileName);
        var newFileName = $"{newId}{extension}";
        var filePath = Path.Combine(MusicStore.GetMusicFolder(), newFileName);
        
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }
        
        var musicFile = new MusicFile
        {
            Id = newId,
            FileName = newFileName,
            OriginalFileName = file.FileName,
            Category = string.IsNullOrEmpty(category) ? "General" : category,
            Description = description ?? "",
            FileSize = file.Length,
            CreatedAt = DateTime.Now
        };
        
        files.Add(musicFile);
        MusicStore.SaveAll(files);
        
        TempData["Success"] = "檔案上傳成功！";
        return RedirectToPage();
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}
