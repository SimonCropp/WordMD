public class WordMDDocument : IDisposable
{
    string docPath;
    ILogger<WordMDDocument> _logger;
    WordprocessingDocument? document;

    public WordMDDocument(string docxPath, ILogger<WordMDDocument> logger)
    {
        docPath = docxPath ?? throw new ArgumentNullException(nameof(docxPath));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void ExtractToDirectory(string targetDirectory)
    {
        _logger.LogInformation("Extracting markdown and images from {DocxPath} to {TargetDirectory}", docPath, targetDirectory);

        Directory.CreateDirectory(targetDirectory);

        using var document = WordprocessingDocument.Open(docPath, false);
        var embeddedPackages = document.MainDocumentPart?.EmbeddedPackageParts ?? [];

        foreach (var package in embeddedPackages)
        {
            var relationshipId = document.MainDocumentPart!.GetIdOfPart(package);
            _logger.LogDebug("Processing embedded package: {RelationshipId}", relationshipId);

            using var stream = package.GetStream();
            var fileName = GetFileName(package, relationshipId);
            var targetPath = Path.Combine(targetDirectory, fileName);

            using var fileStream = File.Create(targetPath);
            stream.CopyTo(fileStream);

            _logger.LogDebug("Extracted {FileName} to {TargetPath}", fileName, targetPath);
        }
    }

    public void EmbedFromDirectory(string sourceDirectory)
    {
        _logger.LogInformation("Embedding markdown and images from {SourceDirectory} to {DocxPath}", sourceDirectory, docPath);

        using var document = WordprocessingDocument.Open(docPath, true);

        // Remove existing embedded packages
        var existingPackages = document.MainDocumentPart?.EmbeddedPackageParts.ToList() ?? [];
        foreach (var package in existingPackages)
        {
            document.MainDocumentPart!.DeletePart(package);
        }

        // Embed all files from source directory
        var files = Directory.GetFiles(sourceDirectory);
        foreach (var file in files)
        {
            var contentType = GetContentType(file);
            using var fileStream = File.OpenRead(file);
            var embeddedPackage = document.MainDocumentPart!.AddEmbeddedPackagePart(contentType);
            using var packageStream = embeddedPackage.GetStream();
            fileStream.CopyTo(packageStream);

            _logger.LogDebug("Embedded {FileName}", Path.GetFileName(file));
        }
    }

    public void ApplyRestrictedEditing()
    {
        _logger.LogInformation("Applying restricted editing to {DocxPath}", docPath);

        using var document = WordprocessingDocument.Open(docPath, true);
        var settings = document.MainDocumentPart?.DocumentSettingsPart;

        if (settings == null)
        {
            settings = document.MainDocumentPart!.AddNewPart<DocumentSettingsPart>();
        }

        // Apply read-only restriction with password "WordMD"
        // This is a simplified implementation - full implementation would use OpenXML SDK properly
        _logger.LogWarning("Restricted editing implementation requires proper OpenXML document protection");
    }

    static string GetFileName(EmbeddedPackagePart package, string relationshipId)
    {
        // Try to get filename from content type or use relationship ID
        var contentType = package.ContentType;
        var extension = contentType switch
        {
            "text/markdown" or "text/plain" => "md",
            "image/png" => "png",
            "image/jpeg" => "jpg",
            "image/gif" => "gif",
            _ => "bin"
        };

        return $"{relationshipId}.{extension}";
    }

    static string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        return extension switch
        {
            ".md" => "text/markdown",
            ".txt" => "text/plain",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            _ => "application/octet-stream"
        };
    }

    public void Dispose() =>
        document?.Dispose();
}