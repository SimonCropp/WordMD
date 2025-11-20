using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace WordMD.Core;

public class MarkdownToWordService
{
    private const string RestrictedEditingPassword = "WordMD";

    public void ConvertMarkdownToWord(string docxPath, string markdownContent)
    {
        using var document = WordprocessingDocument.Open(docxPath, true);
        var mainPart = document.MainDocumentPart;

        if (mainPart is null)
        {
            throw new InvalidOperationException("Document has no main part");
        }

        // Clear existing body content
        var body = mainPart.Document.Body;
        body?.RemoveAllChildren();

        // Convert markdown to Word using DocSharp.Markdown
        // Note: DocSharp.Markdown integration would go here
        // For now, we'll create a basic structure
        ConvertMarkdownBasic(body!, markdownContent);

        // Apply document protection
        ApplyDocumentProtection(document);

        mainPart.Document.Save();
    }

    private void ConvertMarkdownBasic(Body body, string markdownContent)
    {
        // Split by lines and convert
        var lines = markdownContent.Split('\n');

        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                body.AppendChild(new Paragraph());
                continue;
            }

            var trimmedLine = line.TrimStart();

            // Headers
            if (trimmedLine.StartsWith("# "))
            {
                body.AppendChild(CreateHeading(trimmedLine.Substring(2), "Heading1"));
            }
            else if (trimmedLine.StartsWith("## "))
            {
                body.AppendChild(CreateHeading(trimmedLine.Substring(3), "Heading2"));
            }
            else if (trimmedLine.StartsWith("### "))
            {
                body.AppendChild(CreateHeading(trimmedLine.Substring(4), "Heading3"));
            }
            else
            {
                body.AppendChild(CreateParagraph(trimmedLine));
            }
        }
    }

    private Paragraph CreateHeading(string text, string style)
    {
        var paragraph = new Paragraph();
        var paragraphProperties = new ParagraphProperties();
        paragraphProperties.Append(new ParagraphStyleId { Val = style });
        paragraph.Append(paragraphProperties);
        paragraph.Append(new Run(new Text(text)));
        return paragraph;
    }

    private Paragraph CreateParagraph(string text)
    {
        var paragraph = new Paragraph();
        paragraph.Append(new Run(new Text(text)));
        return paragraph;
    }

    private void ApplyDocumentProtection(WordprocessingDocument document)
    {
        var mainPart = document.MainDocumentPart;
        if (mainPart is null) return;

        var settings = mainPart.DocumentSettingsPart ?? mainPart.AddNewPart<DocumentSettingsPart>();
        
        if (settings.Settings is null)
        {
            settings.Settings = new Settings();
        }

        // Remove existing protection
        var existingProtection = settings.Settings.Elements<DocumentProtection>().FirstOrDefault();
        existingProtection?.Remove();

        // Add new protection with password
        var protection = new DocumentProtection
        {
            Edit = DocumentProtectionValues.ReadOnly,
            Enforcement = OnOffValue.FromBoolean(true)
        };

        // Set password hash (simplified - in production, use proper password hashing)
        var passwordHash = GeneratePasswordHash(RestrictedEditingPassword);
        protection.CryptographicProviderType = CryptProviderValues.RsaFull;
        protection.Hash = Convert.ToBase64String(passwordHash);

        settings.Settings.Append(protection);
        settings.Settings.Save();
    }

    private byte[] GeneratePasswordHash(string password)
    {
        // Simplified password hash generation
        // In production, use proper Word password hashing algorithm
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        return sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
    }

    public void RemoveDocumentProtection(string docxPath)
    {
        using var document = WordprocessingDocument.Open(docxPath, true);
        var settings = document.MainDocumentPart?.DocumentSettingsPart?.Settings;

        if (settings is null) return;

        var protection = settings.Elements<DocumentProtection>().FirstOrDefault();
        protection?.Remove();

        settings.Save();
    }
}
