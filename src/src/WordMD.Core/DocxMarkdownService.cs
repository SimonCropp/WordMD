using System.IO.Compression;

namespace WordMD.Core;

public class DocxMarkdownService
{
    private const string MarkdownEntryName = "wordmd/document.md";
    private const string ImagesDirectoryName = "wordmd/images/";

    public void EmbedMarkdown(string docxPath, string markdownContent, Dictionary<string, byte[]> images)
    {
        using var fileStream = new FileStream(docxPath, FileMode.Open, FileAccess.ReadWrite);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Update);

        // Remove existing markdown and images
        RemoveWordMdEntries(archive);

        // Add markdown
        var markdownEntry = archive.CreateEntry(MarkdownEntryName);
        using (var entryStream = markdownEntry.Open())
        using (var writer = new StreamWriter(entryStream))
        {
            writer.Write(markdownContent);
        }

        // Add images
        foreach (var (imageName, imageData) in images)
        {
            var imageEntry = archive.CreateEntry($"{ImagesDirectoryName}{imageName}");
            using var entryStream = imageEntry.Open();
            entryStream.Write(imageData);
        }
    }

    public (string MarkdownContent, Dictionary<string, byte[]> Images) ExtractMarkdown(string docxPath)
    {
        using var fileStream = new FileStream(docxPath, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);

        var markdownContent = "";
        var markdownEntry = archive.GetEntry(MarkdownEntryName);
        if (markdownEntry is not null)
        {
            using var entryStream = markdownEntry.Open();
            using var reader = new StreamReader(entryStream);
            markdownContent = reader.ReadToEnd();
        }

        var images = new Dictionary<string, byte[]>();
        var imageEntries = archive.Entries
            .Where(e => e.FullName.StartsWith(ImagesDirectoryName))
            .ToList();

        foreach (var imageEntry in imageEntries)
        {
            var imageName = imageEntry.FullName.Substring(ImagesDirectoryName.Length);
            using var entryStream = imageEntry.Open();
            using var memoryStream = new MemoryStream();
            entryStream.CopyTo(memoryStream);
            images[imageName] = memoryStream.ToArray();
        }

        return (markdownContent, images);
    }

    public void ExtractToDirectory(string docxPath, string targetDirectory)
    {
        var (markdown, images) = ExtractMarkdown(docxPath);

        Directory.CreateDirectory(targetDirectory);

        var markdownPath = Path.Combine(targetDirectory, "document.md");
        File.WriteAllText(markdownPath, markdown);

        if (images.Count > 0)
        {
            var imagesDirectory = Path.Combine(targetDirectory, "images");
            Directory.CreateDirectory(imagesDirectory);

            foreach (var (imageName, imageData) in images)
            {
                var imagePath = Path.Combine(imagesDirectory, imageName);
                File.WriteAllBytes(imagePath, imageData);
            }
        }
    }

    public (string MarkdownContent, Dictionary<string, byte[]> Images) ReadFromDirectory(string directory)
    {
        var markdownPath = Path.Combine(directory, "document.md");
        var markdown = File.Exists(markdownPath) ? File.ReadAllText(markdownPath) : "";

        var images = new Dictionary<string, byte[]>();
        var imagesDirectory = Path.Combine(directory, "images");

        if (Directory.Exists(imagesDirectory))
        {
            foreach (var imageFile in Directory.GetFiles(imagesDirectory))
            {
                var imageName = Path.GetFileName(imageFile);
                images[imageName] = File.ReadAllBytes(imageFile);
            }
        }

        return (markdown, images);
    }

    private static void RemoveWordMdEntries(ZipArchive archive)
    {
        var entriesToRemove = archive.Entries
            .Where(e => e.FullName.StartsWith("wordmd/"))
            .ToList();

        foreach (var entry in entriesToRemove)
        {
            entry.Delete();
        }
    }
}
