using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WordMD.Conversion;
using WordMD.Core;
using WordMD.Editors;

namespace WordMD;

class Program
{ static Task<int> Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();
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
                return HandleSetupAsync(serviceProvider);
            }

            if (docxPath is not null)
            {
                // Edit mode
                return HandleEditAsync(serviceProvider, docxPath, editorName);
            }

            if (editorOrder?.Length > 0)
            {
                // Update editor order
                return HandleEditorOrderAsync(serviceProvider, editorOrder);
            }

            return Task.CompletedTask;
        });

        var parseResult = rootCommand.Parse(args);
        return parseResult.InvokeAsync();
    }

    private static Task HandleSetupAsync(ServiceProvider serviceProvider)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var editorConfig = serviceProvider.GetRequiredService<EditorConfiguration>();
        var registryManager = serviceProvider.GetRequiredService<RegistryManager>();

        logger.LogInformation("Setting up WordMD...");

        // Detect installed editors
        var editors = editorConfig.DetectInstalledEditors();
        editorConfig.SetEditorOrder(editors);

        // Register context menu
        registryManager.RegisterContextMenu(editors);

        logger.LogInformation("WordMD setup completed successfully");
        logger.LogInformation("Detected editors: {Editors}", string.Join(", ", editors));

        return Task.CompletedTask;
    }

    private static Task HandleEditorOrderAsync(ServiceProvider serviceProvider, string[] editorOrder)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var editorConfig = serviceProvider.GetRequiredService<EditorConfiguration>();
        var registryManager = serviceProvider.GetRequiredService<RegistryManager>();

        logger.LogInformation("Updating editor order...");

        var orderList = editorOrder.ToList();

        // Add any missing editors that were detected but not in the list
        var detected = editorConfig.DetectInstalledEditors();
        foreach (var editor in detected)
        {
            if (!orderList.Contains(editor, StringComparer.OrdinalIgnoreCase))
            {
                orderList.Add(editor);
            }
        }

        editorConfig.SetEditorOrder(orderList);
        registryManager.RegisterContextMenu(orderList);

        logger.LogInformation("Editor order updated: {EditorOrder}", string.Join(", ", orderList));

        return Task.CompletedTask;
    }

    private static Task HandleEditAsync(ServiceProvider serviceProvider, string docxPath, string? editorName)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var editorConfig = serviceProvider.GetRequiredService<EditorConfiguration>();

        if (!File.Exists(docxPath))
        {
            logger.LogError("File not found: {DocxPath}", docxPath);
            Environment.ExitCode = 1;
            return Task.CompletedTask;
        }

        // Determine which editor to use
        var editor = string.IsNullOrEmpty(editorName)
            ? EditorConfiguration.GetEditor(editorConfig.GetDefaultEditor())
            : EditorConfiguration.GetEditor(editorName);

        if (editor == null)
        {
            logger.LogError("Editor not found: {EditorName}", editorName ?? "default");
            Environment.ExitCode = 1;
            return Task.CompletedTask;
        }

        logger.LogInformation("Editing {DocxPath} with {EditorName}", docxPath, editor.DisplayName);

        // Create instances for this edit session
        var documentLogger = serviceProvider.GetRequiredService<ILogger<WordMDDocument>>();
        var document = new WordMDDocument(docxPath, documentLogger);

        var converter = serviceProvider.GetRequiredService<MarkdownToWordConverter>();
        var riderSettings = serviceProvider.GetRequiredService<RiderSettingsGenerator>();
        var launcherLogger = serviceProvider.GetRequiredService<ILogger<EditorLauncher>>();

        var launcher = new EditorLauncher(document, editor, converter, riderSettings, launcherLogger);

        return launcher.LaunchAsync(docxPath);
    }

    private static ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        // Register services
        services.AddSingleton<EditorConfiguration>();
        services.AddSingleton<RegistryManager>();
        services.AddSingleton<MarkdownToWordConverter>();
        services.AddSingleton<RiderSettingsGenerator>();

        return services;
    }
}