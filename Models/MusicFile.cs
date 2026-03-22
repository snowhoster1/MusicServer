using System;
using System.Collections.Generic;

namespace MusicServer.Models
{
    public class MusicFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string OriginalFileName { get; set; } = "";
        public string Category { get; set; } = "General";
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public long FileSize { get; set; }
        public string Description { get; set; } = "";
    }

    public class MusicStore
    {
        private static readonly string DataFile = Path.Combine(AppContext.BaseDirectory, "music_data.json");
        private static readonly string MusicFolder = Path.Combine(AppContext.BaseDirectory, "music");

        public static void EnsureDirectories()
        {
            if (!Directory.Exists(MusicFolder))
                Directory.CreateDirectory(MusicFolder);
        }

        public static List<MusicFile> LoadAll()
        {
            EnsureDirectories();
            if (!File.Exists(DataFile))
            {
                // Try to scan music folder for existing files
                var files = new List<MusicFile>();
                if (Directory.Exists(MusicFolder))
                {
                    var mp3Files = Directory.GetFiles(MusicFolder, "*.mp3");
                    int id = 1;
                    foreach (var f in mp3Files)
                    {
                        var info = new FileInfo(f);
                        files.Add(new MusicFile
                        {
                            Id = id++,
                            FileName = info.Name,
                            OriginalFileName = info.Name,
                            Category = "General",
                            CreatedAt = info.CreationTime,
                            FileSize = info.Length
                        });
                    }
                    if (files.Any())
                        SaveAll(files);
                }
                return files;
            }
            
            var json = File.ReadAllText(DataFile);
            return System.Text.Json.JsonSerializer.Deserialize<List<MusicFile>>(json) ?? new List<MusicFile>();
        }

        public static void SaveAll(List<MusicFile> files)
        {
            var json = System.Text.Json.JsonSerializer.Serialize(files, new System.Text.Json.JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(DataFile, json);
        }

        public static string GetMusicFolder() => MusicFolder;
    }
}
