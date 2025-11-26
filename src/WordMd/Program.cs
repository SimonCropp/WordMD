public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        Logging.Init();
        try
        {
            return await Inner(args);
        }
        catch (Exception exception)
        {
            Log.Logger.Fatal(exception, "Failed at startup");
            throw;
        }
    }

    static Task<int> Inner(string[] args)
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
        rootCommand.SetAction(result =>
        {
            var docxPath = result.GetValue(docxPathArgument);
            var editorName = result.GetValue(editorNameArgument);
            var editorOrder = result.GetValue(editorOrderOption);
            // Determine mode based on arguments
            if (docxPath is null && editorOrder?.Length == 0)
            {
                // Setup mode
                return HandleSetupAsync();
            }

            if (docxPath is not null)
            {
                // Edit mode
                return HandleEditAsync(docxPath, editorName);
            }

            if (editorOrder?.Length > 0)
            {
                // Update editor order
                return HandleEditorOrderAsync(editorOrder);
            }

            return Task.CompletedTask;
        });

        var parseResult = rootCommand.Parse(args);
        return parseResult.InvokeAsync();
    }

    static Task HandleSetupAsync()
    {
        Log.Information("Setting up WordMD...");

        // Detect installed editors
        var editors = EditorConfiguration.DetectInstalledEditors();
        EditorConfiguration.SetEditorOrder(editors);

        // Register context menu
        RegistryManager.RegisterContextMenu(editors);

        Log.Information("WordMD setup completed successfully");
        Log.Information("Detected editors: {Editors}", string.Join(", ", editors));

        return Task.CompletedTask;
    }

    static Task HandleEditorOrderAsync(string[] editorOrder)
    {
        Log.Information("Updating editor order...");

        var orderList = editorOrder.ToList();

        // Add any missing editors that were detected but not in the list
        var detected = EditorConfiguration.DetectInstalledEditors();
        foreach (var editor in detected)
        {
            if (!orderList.Contains(editor, StringComparer.OrdinalIgnoreCase))
            {
                orderList.Add(editor);
            }
        }

        EditorConfiguration.SetEditorOrder(orderList);
        RegistryManager.RegisterContextMenu(orderList);

        Log.Information("Editor order updated: {EditorOrder}", string.Join(", ", orderList));

        return Task.CompletedTask;
    }

    static Task HandleEditAsync(string docxPath, string? editorName)
    {
        if (!File.Exists(docxPath))
        {
            Log.Error("File not found: {DocxPath}", docxPath);
            Environment.ExitCode = 1;
            return Task.CompletedTask;
        }

        // Determine which editor to use
        var editor = string.IsNullOrEmpty(editorName)
            ? EditorConfiguration.GetEditor(EditorConfiguration.GetDefaultEditor())
            : EditorConfiguration.GetEditor(editorName);

        if (editor == null)
        {
            Log.Error("Editor not found: {EditorName}", editorName ?? "default");
            Environment.ExitCode = 1;
            return Task.CompletedTask;
        }

        Log.Information("Editing {DocxPath} with {EditorName}", docxPath, editor.DisplayName);

        // Create instances for this edit session
        var document = new WordMdDocument(docxPath);

        var converter = new MarkdownToWordConverter();

        var launcher = new EditorLauncher(document, editor, converter);

        return launcher.LaunchAsync(docxPath);
    }
}