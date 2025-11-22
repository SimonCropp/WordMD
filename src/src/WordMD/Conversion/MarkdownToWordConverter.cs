using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Markdig;
using Markdig.Syntax;
using Markdig.Syntax.Inlines;
using Microsoft.Extensions.Logging;

namespace WordMD.Conversion;

public class MarkdownToWordConverter
{
    private readonly ILogger<MarkdownToWordConverter> _logger;
    private readonly MarkdownPipeline _pipeline;

    public MarkdownToWordConverter(ILogger<MarkdownToWordConverter> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _pipeline = new MarkdownPipelineBuilder()
            .UseAdvancedExtensions()
            .Build();
    }

    public void ConvertToWord(string markdownPath, string docxPath)
    {
        _logger.LogInformation("Converting {MarkdownPath} to {DocxPath}", markdownPath, docxPath);

        var markdown = File.ReadAllText(markdownPath);
        var document = Markdown.Parse(markdown, _pipeline);

        using var wordDocument = WordprocessingDocument.Open(docxPath, true);
        var body = wordDocument.MainDocumentPart!.Document.Body!;

        // Clear existing content (except sections with embedded packages)
        var elementsToRemove = body.Elements<Paragraph>().ToList();
        foreach (var element in elementsToRemove)
        {
            element.Remove();
        }

        // Convert markdown to Word elements
        foreach (var block in document)
        {
            ConvertBlock(block, body);
        }

        wordDocument.MainDocumentPart.Document.Save();
        _logger.LogInformation("Conversion completed successfully");
    }

    private void ConvertBlock(Block block, Body body)
    {
        switch (block)
        {
            case HeadingBlock heading:
                body.AppendChild(CreateHeading(heading));
                break;
            case ParagraphBlock paragraph:
                body.AppendChild(CreateParagraph(paragraph));
                break;
            case ListBlock list:
                ConvertList(list, body);
                break;
            case CodeBlock code:
                body.AppendChild(CreateCodeBlock(code));
                break;
            case ThematicBreakBlock:
                body.AppendChild(CreateHorizontalRule());
                break;
            default:
                _logger.LogWarning("Unsupported block type: {BlockType}", block.GetType().Name);
                break;
        }
    }

    private Paragraph CreateHeading(HeadingBlock heading)
    {
        var paragraph = new Paragraph();
        var style = heading.Level switch
        {
            1 => "Heading1",
            2 => "Heading2",
            3 => "Heading3",
            4 => "Heading4",
            5 => "Heading5",
            _ => "Heading6"
        };

        var props = new ParagraphProperties(new ParagraphStyleId {Val = style});
        paragraph.AppendChild(props);

        var run = new Run();
        ConvertInlines(heading.Inline!, run);
        paragraph.AppendChild(run);

        return paragraph;
    }

    private Paragraph CreateParagraph(ParagraphBlock paragraphBlock)
    {
        var paragraph = new Paragraph();
        var run = new Run();

        if (paragraphBlock.Inline != null)
        {
            ConvertInlines(paragraphBlock.Inline, run);
        }

        paragraph.AppendChild(run);
        return paragraph;
    }

    private void ConvertInlines(Markdig.Syntax.Inlines.ContainerInline inline, Run run)
    {
        foreach (var child in inline)
        {
            switch (child)
            {
                case LiteralInline literal:
                    run.AppendChild(new Text(literal.Content.ToString()) {Space = SpaceProcessingModeValues.Preserve});
                    break;
                case EmphasisInline emphasis:
                    var emphRun = new Run();
                    var emphProps = new RunProperties();

                    if (emphasis.DelimiterCount == 2)
                    {
                        emphProps.AppendChild(new Bold());
                    }
                    else
                    {
                        emphProps.AppendChild(new Italic());
                    }

                    emphRun.AppendChild(emphProps);
                    ConvertInlines(emphasis, emphRun);
                    run.Parent?.AppendChild(emphRun);
                    break;
                case CodeInline code:
                    var codeRun = new Run();
                    var codeProps = new RunProperties(new RunFonts {Ascii = "Courier New"});
                    codeRun.AppendChild(codeProps);
                    codeRun.AppendChild(new Text(code.Content) {Space = SpaceProcessingModeValues.Preserve});
                    run.Parent?.AppendChild(codeRun);
                    break;
                case LineBreakInline:
                    run.AppendChild(new Break());
                    break;
                default:
                    _logger.LogWarning("Unsupported inline type: {InlineType}", child.GetType().Name);
                    break;
            }
        }
    }

    private void ConvertList(ListBlock list, Body body)
    {
        foreach (var item in list.Cast<ListItemBlock>())
        {
            foreach (var block in item)
            {
                if (block is ParagraphBlock paragraph)
                {
                    var listParagraph = CreateParagraph(paragraph);
                    var props = listParagraph.GetFirstChild<ParagraphProperties>() ?? new ParagraphProperties();

                    var numProps = new NumberingProperties(
                        new NumberingLevelReference {Val = 0},
                        new NumberingId {Val = 1});

                    props.AppendChild(numProps);
                    listParagraph.PrependChild(props);
                    body.AppendChild(listParagraph);
                }
            }
        }
    }

    private Paragraph CreateCodeBlock(CodeBlock code)
    {
        var paragraph = new Paragraph();
        var props = new ParagraphProperties(new ParagraphStyleId {Val = "Code"});
        paragraph.AppendChild(props);

        var run = new Run();
        var runProps = new RunProperties(new RunFonts {Ascii = "Courier New"});
        run.AppendChild(runProps);

        var text = code is FencedCodeBlock fenced ? fenced.Lines.ToString() : code.Lines.ToString();
        run.AppendChild(new Text(text) {Space = SpaceProcessingModeValues.Preserve});
        paragraph.AppendChild(run);

        return paragraph;
    }

    private static Paragraph CreateHorizontalRule()
    {
        var paragraph = new Paragraph();
        var props = new ParagraphProperties(
            new ParagraphBorders(
                new BottomBorder
                {
                    Val = BorderValues.Single,
                    Size = 6,
                    Space = 1,
                    Color = "auto"
                }));
        paragraph.AppendChild(props);
        return paragraph;
    }
}