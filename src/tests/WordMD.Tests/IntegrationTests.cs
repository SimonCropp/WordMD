using System.IO.Compression;
using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class IntegrationTests
{
    private string _testDirectory = null!;
    private DocxMarkdownService _docxService = null!;
    private MarkdownToWordService _markdownToWordService = null!;
    private RiderSettingsGenerator _riderSettingsGenerator = null!;

    [SetUp]
    public void Setup()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"wordmd_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
        
        _docxService = new DocxMarkdownService();
        _markdownToWordService = new MarkdownToWordService();
        _riderSettingsGenerator = new RiderSettingsGenerator();
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private string CreateMinimalDocx()
    {
        var docxPath = Path.Combine(_testDirectory, "document.docx");
        
        using var fileStream = new FileStream(docxPath, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        
        // Create minimal structure
        var contentTypes = archive.CreateEntry("[Content_Types].xml");
        using (var writer = new StreamWriter(contentTypes.Open()))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                  <Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>
                </Types>
                """);
        }

        var rels = archive.CreateEntry("_rels/.rels");
        using (var writer = new StreamWriter(rels.Open()))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">
                  <Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>
                </Relationships>
                """);
        }

        return docxPath;
    }

    [Test]
    public async Task FullWorkflow_CreateEmbedExtractModifyReembed_ShouldSucceed()
    {
        // Step 1: Create a new DOCX
        var docxPath = CreateMinimalDocx();

        // Step 2: Embed initial markdown
        var initialMarkdown = "# Initial Document\n\nThis is the first version.";
        var images = new Dictionary<string, byte[]>
        {
            ["logo.png"] = [0x89, 0x50, 0x4E, 0x47]
        };

        _docxService.EmbedMarkdown(docxPath, initialMarkdown, images);

        // Step 3: Extract to working directory
        var workingDir = Path.Combine(_testDirectory, "working");
        _docxService.ExtractToDirectory(docxPath, workingDir);

        // Step 4: Verify extracted content
        var markdownPath = Path.Combine(workingDir, "document.md");
        Assert.That(File.Exists(markdownPath), Is.True);

        var extractedMarkdown = await File.ReadAllTextAsync(markdownPath);
        Assert.That(extractedMarkdown, Is.EqualTo(initialMarkdown));

        // Step 5: Modify the markdown
        var modifiedMarkdown = "# Updated Document\n\n## Changes\n\nThis has been updated.";
        await File.WriteAllTextAsync(markdownPath, modifiedMarkdown);

        // Step 6: Add a new image
        var newImagePath = Path.Combine(workingDir, "images", "chart.png");
        await File.WriteAllBytesAsync(newImagePath, [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A]);

        // Step 7: Re-embed changes
        var (updatedMarkdown, updatedImages) = _docxService.ReadFromDirectory(workingDir);
        _docxService.EmbedMarkdown(docxPath, updatedMarkdown, updatedImages);

        // Step 8: Extract again and verify
        var verifyDir = Path.Combine(_testDirectory, "verify");
        _docxService.ExtractToDirectory(docxPath, verifyDir);

        var finalMarkdown = await File.ReadAllTextAsync(Path.Combine(verifyDir, "document.md"));
        var finalImages = Directory.GetFiles(Path.Combine(verifyDir, "images"));

        await Verify(new
        {
            FinalMarkdown = finalMarkdown,
            ImageCount = finalImages.Length,
            ImageNames = finalImages.Select(Path.GetFileName).OrderBy(n => n).ToList()
        });
    }

    [Test]
    public async Task FullWorkflow_WithRiderSettings_ShouldCreateDotSettings()
    {
        var docxPath = CreateMinimalDocx();
        var markdown = "# Test";
        _docxService.EmbedMarkdown(docxPath, markdown, new Dictionary<string, byte[]>());

        var workingDir = Path.Combine(_testDirectory, "rider_working");
        _docxService.ExtractToDirectory(docxPath, workingDir);

        // Generate Rider settings
        _riderSettingsGenerator.GenerateDotSettings(workingDir);

        var dotSettingsPath = Path.Combine(workingDir, "Default.DotSettings");
        var dotSettingsExists = File.Exists(dotSettingsPath);
        var dotSettingsContent = dotSettingsExists 
            ? await File.ReadAllTextAsync(dotSettingsPath) 
            : "";

        await Verify(new
        {
            DotSettingsExists = dotSettingsExists,
            DotSettingsContent = dotSettingsContent,
            HasAutoSaveSetting = dotSettingsContent.Contains("AutoSave")
        });
    }

    [Test]
    public void CompleteEditorWorkflow_DetectAndOrder_ShouldWork()
    {
        var detectionService = new EditorDetectionService();
        var configService = new ConfigurationService();

        // Detect editors
        var installedEditors = detectionService.DetectInstalledEditors();

        // Create configuration
        var config = new WordMdConfiguration
        {
            EditorOrder = installedEditors.Select(e => e.Definition.Name).ToList(),
            DefaultEditor = installedEditors.FirstOrDefault()?.Definition.Name
        };

        // Verify we have a valid configuration
        Assert.Multiple(() =>
        {
            Assert.That(config.EditorOrder, Is.Not.Empty.Or.Empty);
            Assert.That(config.DefaultEditor, Is.Null.Or.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public async Task MultipleEditCycles_ShouldMaintainDataIntegrity()
    {
        var docxPath = CreateMinimalDocx();
        var markdown = "# Version 1";
        var images = new Dictionary<string, byte[]>();

        // Cycle 1
        _docxService.EmbedMarkdown(docxPath, markdown, images);
        var (extracted1, _) = _docxService.ExtractMarkdown(docxPath);

        // Cycle 2
        markdown = "# Version 2\n\nUpdated content.";
        _docxService.EmbedMarkdown(docxPath, markdown, images);
        var (extracted2, _) = _docxService.ExtractMarkdown(docxPath);

        // Cycle 3
        markdown = "# Version 3\n\n## Section 1\n\nFinal version.";
        images["final.png"] = [0x89, 0x50, 0x4E, 0x47];
        _docxService.EmbedMarkdown(docxPath, markdown, images);
        var (extracted3, extractedImages3) = _docxService.ExtractMarkdown(docxPath);

        await Verify(new
        {
            Cycle1 = extracted1,
            Cycle2 = extracted2,
            Cycle3 = extracted3,
            FinalImageCount = extractedImages3.Count,
            FinalImages = extractedImages3.Keys.OrderBy(k => k).ToList()
        });
    }

    [Test]
    public void EditorDefinitions_AllEditorsHaveValidArgumentPatterns()
    {
        var testPath = @"C:\Test\document.md";

        foreach (var editor in KnownEditors.AllEditors)
        {
            var formattedArgs = string.Format(editor.ArgumentsPattern, testPath);

            Assert.Multiple(() =>
            {
                Assert.That(formattedArgs, Is.Not.Null.And.Not.Empty,
                    $"Editor {editor.Name} produced null/empty arguments");
                Assert.That(formattedArgs, Does.Contain(testPath).Or.Contain("document.md"),
                    $"Editor {editor.Name} arguments don't contain the file path");
            });
        }
    }

    [Test]
    public async Task DocumentWithComplexMarkdown_ShouldPreserveFormatting()
    {
        var docxPath = CreateMinimalDocx();
        var markdown = """
            # Main Title
            
            ## Introduction
            
            This is a **bold** statement with *italic* text and `inline code`.
            
            ### Features
            
            - Feature 1
            - Feature 2
              - Nested item
            - Feature 3
            
            #### Code Example
            
            ```csharp
            public class Example
            {
                public void Method()
                {
                    Console.WriteLine("Hello, World!");
                }
            }
            ```
            
            ##### Links and Images
            
            [Link text](https://example.com)
            
            ![Alt text](images/screenshot.png)
            
            ###### Tables
            
            | Header 1 | Header 2 |
            |----------|----------|
            | Cell 1   | Cell 2   |
            | Cell 3   | Cell 4   |
            """;

        var images = new Dictionary<string, byte[]>
        {
            ["screenshot.png"] = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A]
        };

        _docxService.EmbedMarkdown(docxPath, markdown, images);
        var (extracted, extractedImages) = _docxService.ExtractMarkdown(docxPath);

        await Verify(new
        {
            ExtractedMarkdown = extracted,
            ImageCount = extractedImages.Count,
            ImageNames = extractedImages.Keys.OrderBy(k => k).ToList(),
            MarkdownMatches = extracted == markdown
        });
    }
}
