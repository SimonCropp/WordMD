using System.IO.Compression;
using WordMD.Core;

namespace WordMD.Tests;

[TestFixture]
public class DocxMarkdownServiceTests
{
    private DocxMarkdownService _service = null!;
    private string _testDirectory = null!;

    [SetUp]
    public void Setup()
    {
        _service = new DocxMarkdownService();
        _testDirectory = Path.Combine(Path.GetTempPath(), $"wordmd_test_{Guid.NewGuid()}");
        Directory.CreateDirectory(_testDirectory);
    }

    [TearDown]
    public void TearDown()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }
    }

    private string CreateEmptyDocx()
    {
        var docxPath = Path.Combine(_testDirectory, "test.docx");
        
        using var fileStream = new FileStream(docxPath, FileMode.Create);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Create);
        
        // Create minimal docx structure
        var contentTypes = archive.CreateEntry("[Content_Types].xml");
        using (var writer = new StreamWriter(contentTypes.Open()))
        {
            writer.Write("""
                <?xml version="1.0" encoding="UTF-8" standalone="yes"?>
                <Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">
                  <Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>
                  <Default Extension="xml" ContentType="application/xml"/>
                </Types>
                """);
        }

        return docxPath;
    }

    [Test]
    public void EmbedMarkdown_WithSimpleContent_ShouldCreateEntry()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = "# Hello World\n\nThis is a test.";
        var images = new Dictionary<string, byte[]>();

        _service.EmbedMarkdown(docxPath, markdown, images);

        using var fileStream = new FileStream(docxPath, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        
        var markdownEntry = archive.GetEntry("wordmd/document.md");
        
        Assert.That(markdownEntry, Is.Not.Null);
    }

    [Test]
    public async Task EmbedMarkdown_WithContent_ShouldPreserveMarkdown()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = "# Hello World\n\nThis is a test.";
        var images = new Dictionary<string, byte[]>();

        _service.EmbedMarkdown(docxPath, markdown, images);

        var (extractedMarkdown, _) = _service.ExtractMarkdown(docxPath);

        await Verify(extractedMarkdown);
    }

    [Test]
    public void EmbedMarkdown_WithImages_ShouldEmbedAllImages()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = "# Test\n\n![Image](images/test.png)";
        var images = new Dictionary<string, byte[]>
        {
            ["test.png"] = [0x89, 0x50, 0x4E, 0x47], // PNG header
            ["another.jpg"] = [0xFF, 0xD8, 0xFF, 0xE0] // JPEG header
        };

        _service.EmbedMarkdown(docxPath, markdown, images);

        using var fileStream = new FileStream(docxPath, FileMode.Open, FileAccess.Read);
        using var archive = new ZipArchive(fileStream, ZipArchiveMode.Read);
        
        var imageEntries = archive.Entries
            .Where(e => e.FullName.StartsWith("wordmd/images/"))
            .ToList();

        Assert.That(imageEntries, Has.Count.EqualTo(2));
    }

    [Test]
    public async Task ExtractMarkdown_WithImages_ShouldReturnAllImages()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = "# Test";
        var images = new Dictionary<string, byte[]>
        {
            ["test.png"] = [0x89, 0x50, 0x4E, 0x47],
            ["another.jpg"] = [0xFF, 0xD8, 0xFF, 0xE0]
        };

        _service.EmbedMarkdown(docxPath, markdown, images);
        var (_, extractedImages) = _service.ExtractMarkdown(docxPath);

        await Verify(new
        {
            ImageNames = extractedImages.Keys.OrderBy(k => k).ToList(),
            ImageCount = extractedImages.Count
        });
    }

    [Test]
    public void ExtractMarkdown_FromEmptyDocx_ShouldReturnEmptyContent()
    {
        var docxPath = CreateEmptyDocx();

        var (markdown, images) = _service.ExtractMarkdown(docxPath);

        Assert.Multiple(() =>
        {
            Assert.That(markdown, Is.Empty);
            Assert.That(images, Is.Empty);
        });
    }

    [Test]
    public void ExtractToDirectory_ShouldCreateMarkdownFile()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = "# Test Document\n\nContent here.";
        _service.EmbedMarkdown(docxPath, markdown, new Dictionary<string, byte[]>());

        var targetDir = Path.Combine(_testDirectory, "extracted");
        _service.ExtractToDirectory(docxPath, targetDir);

        var markdownPath = Path.Combine(targetDir, "document.md");
        
        Assert.That(File.Exists(markdownPath), Is.True);
    }

    [Test]
    public async Task ExtractToDirectory_WithImages_ShouldCreateImagesFolder()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = "# Test";
        var images = new Dictionary<string, byte[]>
        {
            ["test.png"] = [0x89, 0x50, 0x4E, 0x47]
        };
        _service.EmbedMarkdown(docxPath, markdown, images);

        var targetDir = Path.Combine(_testDirectory, "extracted");
        _service.ExtractToDirectory(docxPath, targetDir);

        var imagesDir = Path.Combine(targetDir, "images");
        var imageFiles = Directory.GetFiles(imagesDir);
        
        await Verify(new
        {
            ImagesDirectoryExists = Directory.Exists(imagesDir),
            ImageFiles = imageFiles.Select(Path.GetFileName).OrderBy(f => f).ToList()
        });
    }

    [Test]
    public void ReadFromDirectory_ShouldReadMarkdownAndImages()
    {
        var testDir = Path.Combine(_testDirectory, "content");
        Directory.CreateDirectory(testDir);
        
        var markdown = "# Test Content";
        File.WriteAllText(Path.Combine(testDir, "document.md"), markdown);
        
        var imagesDir = Path.Combine(testDir, "images");
        Directory.CreateDirectory(imagesDir);
        File.WriteAllBytes(Path.Combine(imagesDir, "test.png"), [0x89, 0x50, 0x4E, 0x47]);

        var (readMarkdown, readImages) = _service.ReadFromDirectory(testDir);

        Assert.Multiple(() =>
        {
            Assert.That(readMarkdown, Is.EqualTo(markdown));
            Assert.That(readImages, Has.Count.EqualTo(1));
            Assert.That(readImages.ContainsKey("test.png"), Is.True);
        });
    }

    [Test]
    public void EmbedMarkdown_MultipleTimes_ShouldReplaceContent()
    {
        var docxPath = CreateEmptyDocx();
        
        _service.EmbedMarkdown(docxPath, "# First", new Dictionary<string, byte[]>());
        _service.EmbedMarkdown(docxPath, "# Second", new Dictionary<string, byte[]>());

        var (markdown, _) = _service.ExtractMarkdown(docxPath);

        Assert.That(markdown, Is.EqualTo("# Second"));
    }

    [Test]
    public async Task EmbedMarkdown_WithMultilineMarkdown_ShouldPreserveFormatting()
    {
        var docxPath = CreateEmptyDocx();
        var markdown = """
            # Main Title
            
            ## Subsection
            
            Some **bold** and *italic* text.
            
            - List item 1
            - List item 2
            - List item 3
            
            ```csharp
            var x = 42;
            Console.WriteLine(x);
            ```
            """;

        _service.EmbedMarkdown(docxPath, markdown, new Dictionary<string, byte[]>());
        var (extracted, _) = _service.ExtractMarkdown(docxPath);

        await Verify(extracted);
    }
}
