using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WordMD.Core;

public class MarkdownToWordService
{
    private const string MarkdownContentType = "text/markdown";

    public void EmbedMarkdown(string docxPath, string markdownContent)
    {
        using var document = WordprocessingDocument.Open(docxPath, true);
        var mainPart = document.MainDocumentPart;

        if (mainPart is null)
        {
            throw new InvalidOperationException("Document has no main part");
        }

        // Remove existing embedded markdown if present
        var existingPart = GetEmbeddedMarkdownPart(mainPart);
        if (existingPart is not null)
        {
            mainPart.DeletePart(existingPart);
        }

        // Add markdown as embedded package part
        var embeddedPart = mainPart.AddEmbeddedPackagePart(MarkdownContentType);

        using var stream = embeddedPart.GetStream();
        using var writer = new StreamWriter(stream);
        writer.Write(markdownContent);
    }

    public string? ExtractMarkdown(string docxPath)
    {
        using var document = WordprocessingDocument.Open(docxPath, false);
        var mainPart = document.MainDocumentPart;

        if (mainPart is null)
        {
            throw new InvalidOperationException("Document has no main part");
        }

        var embeddedPart = GetEmbeddedMarkdownPart(mainPart);

        if (embeddedPart is null)
        {
            return null;
        }

        using var stream = embeddedPart.GetStream();
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }

    private EmbeddedPackagePart? GetEmbeddedMarkdownPart(MainDocumentPart mainPart)
    {
        return mainPart.EmbeddedPackageParts
            .FirstOrDefault(p => p.ContentType == MarkdownContentType);
    }

    public bool HasEmbeddedMarkdown(string docxPath)
    {
        using var document = WordprocessingDocument.Open(docxPath, false);
        var mainPart = document.MainDocumentPart;

        if (mainPart is null)
        {
            return false;
        }

        return GetEmbeddedMarkdownPart(mainPart) is not null;
    }
}