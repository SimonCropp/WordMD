public class WordMdDocument(string docxPath)
{
    public void ExtractToDirectory(string targetDirectory)
    {
        Log.Information("Extracting markdown and images from {DocxPath} to {TargetDirectory}", docxPath, targetDirectory);

        Directory.CreateDirectory(targetDirectory);

        using var document = WordprocessingDocument.Open(docxPath, false);
        var embeddedPackages = document.MainDocumentPart?.EmbeddedPackageParts ?? [];

        foreach (var package in embeddedPackages)
        {
            var relationshipId = document.MainDocumentPart!.GetIdOfPart(package);
            Log.Debug("Processing embedded package: {RelationshipId}", relationshipId);

            using var stream = package.GetStream();
            var fileName = GetFileName(package, relationshipId);
            var targetPath = Path.Combine(targetDirectory, fileName);

            using var fileStream = File.Create(targetPath);
            stream.CopyTo(fileStream);

            Log.Debug("Extracted {FileName} to {TargetPath}", fileName, targetPath);
        }
    }

    public void EmbedFromDirectory(string sourceDirectory)
    {
        Log.Information("Embedding markdown and images from {SourceDirectory} to {DocxPath}", sourceDirectory, docxPath);

        using var document = WordprocessingDocument.Open(docxPath, true);

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

            Log.Debug("Embedded {FileName}", Path.GetFileName(file));
        }
    }

    public void ApplyRestrictedEditing()
    {
        Log.Information("Applying restricted editing to {DocxPath}", docxPath);

        using var document = WordprocessingDocument.Open(docxPath, true);
        var settings = document.MainDocumentPart?.DocumentSettingsPart;

        if (settings == null)
        {
            document.MainDocumentPart!.AddNewPart<DocumentSettingsPart>();
        }

        // Apply read-only restriction with password "WordMD"
        // This is a simplified implementation - full implementation would use OpenXML SDK properly
        Log.Warning("Restricted editing implementation requires proper OpenXML document protection");
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
}