using System.CommandLine;
using System.CommandLine.Invocation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WordMD.Conversion;
using WordMD.Core;
using WordMD.Editors;

namespace WordMD;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var services = ConfigureServices();
        var serviceProvider = services.BuildServiceProvider();

        var rootCommand = new RootCommand("WordMD - Edit markdown documents embedded in Word files");

        // Add --editor-order option
        var editorOrderOption = new Option<string[]>(
            "--editor-order",
            "Specify the order of markdown editors (comma-separated)")
        {
            AllowMultipleArgumentsPerToken = true,
            Arity = ArgumentArity.ZeroOrMore
        };
        rootCommand.AddOption(editorOrderOption);

        // Add docx path argument (optional)
        var docxPathArgument = new Argument<string?>("docx-path")
        {
            Description = "Path to the .docx file to edit",
            Arity = ArgumentArity.ZeroOrOne
        };
        rootCommand.AddArgument(docxPathArgument);

        // Add editor name argument (optional)
        var editorNameArgument = new Argument<string?>("editor-name")
        {
            Description = "Name of the editor to use",
            Arity = ArgumentArity.ZeroOrOne
        };
        rootCommand.AddArgument(editorNameArgument);

        // Use the stable 2.0 handler pattern (SetHandler extension)
        rootCommand.SetHandler(async (string? docxPath, string? editorName, string[]? editorOrder) =>
        {
            var logger = serviceProvider.GetRequiredService<ILogger<Program>>();

            try
            {
                // Handle --editor-order
                if (editorOrder != null && editorOrder.Length > 0)
                {
                    await HandleEditorOrderAsync(serviceProvider, editorOrder);
                    return;
                }

                // Handle setup (no arguments)
                if (string.IsNullOrEmpty(docxPath))
                {
                    await HandleSetupAsync(serviceProvider);
                    return;
                }

                // Handle edit
                await HandleEditAsync(serviceProvider, docxPath, editorName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception");
                Environment.ExitCode = 1;
            }
        }, docxPathArgument, editorNameArgument, editorOrderOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task HandleSetupAsync(ServiceProvider serviceProvider)
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

        await Task.CompletedTask;
    }

    private static async Task HandleEditorOrderAsync(ServiceProvider serviceProvider, string[] editorOrder)
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

        await Task.CompletedTask;
    }

    private static async Task HandleEditAsync(ServiceProvider serviceProvider, string docxPath, string? editorName)
    {
        var logger = serviceProvider.GetRequiredService<ILogger<Program>>();
        var editorConfig = serviceProvider.GetRequiredService<EditorConfiguration>();

        if (!File.Exists(docxPath))
        {
            logger.LogError("File not found: {DocxPath}", docxPath);
            Environment.ExitCode = 1;
            return;
        }

        // Determine which editor to use
        var editor = string.IsNullOrEmpty(editorName)
            ? editorConfig.GetEditor(editorConfig.GetDefaultEditor())
            : editorConfig.GetEditor(editorName);

        if (editor == null)
        {
            logger.LogError("Editor not found: {EditorName}", editorName ?? "default");
            Environment.ExitCode = 1;
            return;
        }

        logger.LogInformation("Editing {DocxPath} with {EditorName}", docxPath, editor.DisplayName);

        // Create instances for this edit session
        var documentLogger = serviceProvider.GetRequiredService<ILogger<WordMDDocument>>();
        var document = new WordMDDocument(docxPath, documentLogger);

        var converter = serviceProvider.GetRequiredService<MarkdownToWordConverter>();
        var riderSettings = serviceProvider.GetRequiredService<RiderSettingsGenerator>();
        var launcherLogger = serviceProvider.GetRequiredService<ILogger<EditorLauncher>>();

        var launcher = new EditorLauncher(document, editor, converter, riderSettings, launcherLogger);

        await launcher.LaunchAsync(docxPath);
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