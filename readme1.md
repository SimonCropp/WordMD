# WordMD

A tool for editing markdown documents embedded in Word (.docx) files.

## Overview

WordMD allows you to maintain markdown content within Word documents. The markdown and associated images are stored as embedded packages within the .docx file, and can be edited using your favorite markdown editor.

## Features

- **Embedded Markdown**: Store markdown documents and images inside Word files as EmbeddedPackageParts
- **Right-click Context Menu**: Edit .docx files directly from Windows Explorer
- **Multiple Editor Support**: Works with VSCode, Rider, Notepad++, Typora, Markdown Monster, and Obsidian
- **Background Processing**: Automatically converts markdown to Word format on save
- **File Watching**: Detects changes to markdown and images in real-time
- **Restricted Editing**: Prevents direct editing of Word content (password: "WordMD")
- **Rider Integration**: Disables auto-save for seamless editing experience

## Installation

Install as a .NET global tool:

```bash
dotnet tool install --global WordMD
```

## Setup

Run the setup command to detect installed editors and register context menu:

```bash
wordmd
```

This will:
- Detect installed markdown editors
- Register Windows Explorer context menu items
- Create configuration file in `%APPDATA%\WordMD`

## Usage

### From Windows Explorer

Right-click on a .docx file and select:
- **WordMD Edit** - Opens in your default markdown editor
- **WordMD** → [Editor Name] - Opens in a specific editor

### From Command Line

Edit with default editor:
```bash
wordmd "C:\path\to\document.docx"
```

Edit with specific editor:
```bash
wordmd "C:\path\to\document.docx" vscode
```

Configure editor order:
```bash
wordmd --editor-order vscode,rider,notepad
```

## Supported Editors

- **Visual Studio Code** (`vscode`)
- **JetBrains Rider** (`rider`)
- **Notepad++** (`notepad`)
- **Typora** (`typora`)
- **Markdown Monster** (`markdownmonster`)
- **Obsidian** (`obsidian`)

## How It Works

1. **Extraction**: When you open a .docx file for editing, WordMD extracts the embedded markdown and images to a temporary directory
2. **Editing**: Your chosen markdown editor opens the extracted markdown file
3. **Watching**: A background process monitors the temporary directory for changes
4. **Conversion**: On save, the markdown is converted to Word format using Markdig
5. **Embedding**: The updated markdown and images are re-embedded in the .docx file
6. **Cleanup**: When you close the editor, temporary files are cleaned up

## Architecture

The solution consists of two projects:

- **WordMD**: Main application containing all functionality (dotnet tool)
  - Core: Document handling and file watching
  - Editors: Editor detection, configuration, and registry management
  - Conversion: Markdown to Word conversion and Rider settings generation
- **WordMD.Tests**: NUnit tests with Verify snapshot testing

**Solution Format:** Uses modern .slnx format (7 lines of clean XML). See [SLNX_FORMAT.md](SLNX_FORMAT.md) for details.

## Technologies

- **.NET 10** with C# 14
- **DocumentFormat.OpenXml**: For Word document manipulation
- **Markdig**: Markdown parsing and processing
- **System.CommandLine**: Command-line argument parsing (stable 2.0)
- **NUnit** with **Verify**: Testing framework with snapshot testing

## Modern Solution Format

This solution uses the new **.slnx format** (XML-based solution file) introduced in Visual Studio 2022 17.10. This provides:

- Clean, human-readable XML structure
- Better merge conflict resolution
- Simpler version control diffs
- No GUID management required

Compatible with Visual Studio 2022 17.10+, JetBrains Rider 2023.3+, and all `dotnet` CLI commands.

## Configuration

Configuration is stored in `%APPDATA%\WordMD\wordmd-config.json`:

```json
{
  "EditorOrder": [
    "vscode",
    "rider",
    "notepad"
  ]
}
```

## Development

### Requirements

- .NET 10 SDK
- Windows OS (for registry integration)
- JetBrains Rider or Visual Studio 2022 17.10+ (for .slnx support)

### Building

```bash
dotnet build
```

### Testing

```bash
dotnet test
```

### Packaging

```bash
dotnet pack
```

## License

MIT License

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
