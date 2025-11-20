using System.CommandLine;
using WordMD.Core;

namespace WordMD.Cli;

class Program
{
    static async Task<int> Main(string[] args)
    {
        // Configure setup command (no arguments = setup)
        var editorOrderOption = new Option<string[]>("--editor-order")
        {
            Description = "Comma-separated list of editor names to specify order",
            AllowMultipleArgumentsPerToken = true
        };

        // Add positional arguments for edit mode
        var docxPathArgument = new Argument<string?>("docx-path")
        {
            Description = "Path to the .docx file to edit"
        };

        var editorNameArgument = new Argument<string?>("editor-name")
        {
            Description = "Name of the editor to use"
        };

        var rootCommand = new RootCommand("WordMD - Edit markdown documents within Word files")
        {
            editorOrderOption,
            docxPathArgument,
            editorNameArgument
        };
        rootCommand.SetAction(async result =>
        {
            var docxPath = result.GetValue(docxPathArgument);
            var editorName = result.GetValue(editorNameArgument);
            var editorOrder = result.GetValue(editorOrderOption);
            // Determine mode based on arguments
            if (docxPath is null && editorOrder?.Length == 0)
            {
                // Setup mode
                await SetupMode();
            }
            else if (docxPath is not null)
            {
                // Edit mode
                await EditMode(docxPath, editorName);
            }
            else if (editorOrder?.Length > 0)
            {
                // Update editor order
                await UpdateEditorOrder(editorOrder);
            }
        });

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }

    private static async Task SetupMode()
    {
        Console.WriteLine("WordMD Setup");
        Console.WriteLine("============");
        Console.WriteLine();

        var detectionService = new EditorDetectionService();
        var configService = new ConfigurationService();
        var registryService = new RegistryService();

        Console.WriteLine("Detecting installed markdown editors...");
        var installedEditors = detectionService.DetectInstalledEditors();

        if (installedEditors.Count == 0)
        {
            Console.WriteLine("No supported markdown editors found.");
            Console.WriteLine("Supported editors: VSCode, Rider, Typora, Markdown Monster, Obsidian, Notepad");
            return;
        }

        Console.WriteLine($"Found {installedEditors.Count} editor(s):");
        foreach (var editor in installedEditors)
        {
            Console.WriteLine($"  - {editor.Definition.DisplayName}");
        }
        Console.WriteLine();

        // Load or create configuration
        var config = configService.Load();

        // If no editor order is set, use the detected order
        if (config.EditorOrder.Count == 0)
        {
            config.EditorOrder = installedEditors.Select(e => e.Definition.Name).ToList();
            configService.Save(config);
        }

        // Order editors according to configuration
        var orderedEditors = OrderEditors(installedEditors, config.EditorOrder);

        // Set default editor if not set
        if (config.DefaultEditor is null && orderedEditors.Count > 0)
        {
            config.DefaultEditor = orderedEditors[0].Definition.Name;
            configService.Save(config);
        }

        Console.WriteLine("Registering context menu entries...");
        registryService.RegisterContextMenu(orderedEditors, config.DefaultEditor);

        Console.WriteLine();
        Console.WriteLine("Setup complete!");
        Console.WriteLine();
        Console.WriteLine("Right-click any .docx file to see the WordMD options:");
        Console.WriteLine("  - 'WordMD Edit' - Opens with default editor");
        Console.WriteLine("  - 'WordMD' submenu - Choose from available editors");
        Console.WriteLine();
        Console.WriteLine($"Default editor: {config.DefaultEditor}");
        Console.WriteLine();
        Console.WriteLine("To change editor order, use:");
        Console.WriteLine("  wordmd --editor-order vscode,rider,typora");

        await Task.CompletedTask;
    }

    private static async Task EditMode(string docxPath, string? editorName)
    {
        if (!File.Exists(docxPath))
        {
            Console.WriteLine($"Error: File not found: {docxPath}");
            return;
        }

        var detectionService = new EditorDetectionService();
        var configService = new ConfigurationService();
        var config = configService.Load();

        var installedEditors = detectionService.DetectInstalledEditors();
        var orderedEditors = OrderEditors(installedEditors, config.EditorOrder);

        // Determine which editor to use
        InstalledEditor? selectedEditor = null;

        if (editorName is not null)
        {
            selectedEditor = orderedEditors.FirstOrDefault(e =>
                e.Definition.Name.Equals(editorName, StringComparison.OrdinalIgnoreCase));

            if (selectedEditor is null)
            {
                Console.WriteLine($"Error: Editor '{editorName}' not found or not installed");
                return;
            }
        }
        else if (config.DefaultEditor is not null)
        {
            selectedEditor = orderedEditors.FirstOrDefault(e =>
                e.Definition.Name.Equals(config.DefaultEditor, StringComparison.OrdinalIgnoreCase));
        }

        if (selectedEditor is null && orderedEditors.Count > 0)
        {
            selectedEditor = orderedEditors[0];
        }

        if (selectedEditor is null)
        {
            Console.WriteLine("Error: No editor available");
            return;
        }

        Console.WriteLine($"Opening {Path.GetFileName(docxPath)} with {selectedEditor.Definition.DisplayName}...");

        var docxService = new DocxMarkdownService();
        var markdownToWordService = new MarkdownToWordService();
        var riderSettingsGenerator = new RiderSettingsGenerator();
        var editorService = new EditorService(docxService, markdownToWordService, riderSettingsGenerator);

        await editorService.EditDocumentAsync(docxPath, selectedEditor);

        Console.WriteLine("Edit session complete.");
    }

    private static async Task UpdateEditorOrder(string[] editorNames)
    {
        var configService = new ConfigurationService();
        var detectionService = new EditorDetectionService();
        var registryService = new RegistryService();

        var editorList = editorNames.SelectMany(n => n.Split(','))
            .Select(n => n.Trim())
            .Where(n => !string.IsNullOrEmpty(n))
            .ToList();

        configService.UpdateEditorOrder(editorList);

        Console.WriteLine("Editor order updated:");
        foreach (var editor in editorList)
        {
            Console.WriteLine($"  - {editor}");
        }

        // Re-register context menu with new order
        var config = configService.Load();
        var installedEditors = detectionService.DetectInstalledEditors();
        var orderedEditors = OrderEditors(installedEditors, config.EditorOrder);

        registryService.RegisterContextMenu(orderedEditors, config.DefaultEditor);

        Console.WriteLine();
        Console.WriteLine("Context menu updated.");

        await Task.CompletedTask;
    }

    private static List<InstalledEditor> OrderEditors(
        List<InstalledEditor> editors,
        List<string> order)
    {
        if (order.Count == 0)
        {
            return editors;
        }

        var ordered = new List<InstalledEditor>();

        // Add editors in specified order
        foreach (var editorName in order)
        {
            var editor = editors.FirstOrDefault(e =>
                e.Definition.Name.Equals(editorName, StringComparison.OrdinalIgnoreCase));

            if (editor is not null)
            {
                ordered.Add(editor);
            }
        }

        // Add remaining editors
        foreach (var editor in editors)
        {
            if (!ordered.Contains(editor))
            {
                ordered.Add(editor);
            }
        }

        return ordered;
    }
}
