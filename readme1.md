# WordMD

Edit markdown documents embedded within Word files (.docx) with your favorite markdown editor.

## Overview

WordMD allows you to:
- Embed markdown content and images within Word documents as a hidden ZIP archive
- Edit the embedded markdown using your preferred editor (VS Code, Rider, Typora, etc.)
- Automatically convert markdown back to Word format on save
- Protect Word documents with restricted editing to prevent accidental modifications

## How It Works

A `.docx` file is actually a ZIP archive. WordMD leverages this by:
1. Storing the markdown source and images in a `wordmd/` directory within the ZIP
2. Extracting these files to a temporary directory when editing
3. Watching for changes and automatically converting markdown to Word
4. Re-embedding the updated markdown and images back into the `.docx`

## Installation

Install WordMD as a global .NET tool:

```bash
dotnet tool install -g WordMD
```

## Initial Setup

Run WordMD without arguments to set up Windows Explorer context menu integration:

```bash
wordmd
```

This will:
- Detect installed markdown editors on your system
- Register context menu entries for `.docx` files
- Create a configuration file in `~/.wordmd/config.json`

### Supported Editors

- Visual Studio Code
- JetBrains Rider
- Typora
- Markdown Monster
- Obsidian
- Notepad (fallback)

## Usage

### Context Menu

After setup, right-click any `.docx` file to see:

- **WordMD Edit** - Opens with your default editor
- **WordMD** - Submenu with all available editors

### Command Line

Edit a document with the default editor:

```bash
wordmd "path/to/document.docx"
```

Edit with a specific editor:

```bash
wordmd "path/to/document.docx" vscode
wordmd "path/to/document.docx" rider
```

### Configure Editor Order

Customize the order of editors in the context menu:

```bash
wordmd --editor-order vscode,rider,typora
```

Editors not in the list will appear after the specified ones in their default order.

## Features

### Rider Integration

When editing with Rider, WordMD creates a `Default.DotSettings` file that disables auto-save for the temporary directory, preventing unnecessary file change events.

### Document Protection

WordMD applies restricted editing to the Word document with the password `WordMD` to prevent accidental modifications to the generated content.

### File Watching

Changes to the markdown file and images are automatically detected and converted back to Word format, keeping the document in sync with your edits.

### HTML Embedding

For HTML content within markdown, WordMD uses Word Interop to properly embed the HTML into the Word document.

## Building from Source

```bash
# Restore and build
dotnet restore
dotnet build

# Run tests
dotnet test

# Pack as a tool
dotnet pack -c Release

# Install locally
dotnet tool install -g --add-source ./src/WordMD.Cli/bin/Release WordMD
```

## Testing

The solution includes comprehensive tests using NUnit and Verify:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --verbosity normal

# Run specific test class
dotnet test --filter "FullyQualifiedName~EditorDefinitionsTests"
```

See [tests/WordMD.Tests/README.md](tests/WordMD.Tests/README.md) for more details on the testing approach.

## Requirements

- .NET 10.0 or later
- Windows (for registry-based context menu integration)
- At least one supported markdown editor installed

## Configuration

Configuration is stored in `~/.wordmd/config.json`:

```json
{
  "EditorOrder": ["vscode", "rider", "typora"],
  "DefaultEditor": "vscode"
}
```

## Technical Details

### Archive Structure

Within the `.docx` ZIP archive:

```
document.docx
├── word/
│   └── ... (standard Word XML files)
└── wordmd/
    ├── document.md
    └── images/
        ├── image1.png
        └── image2.jpg
```

### Conversion

Markdown is converted to Word using:
- **DocSharp.Markdown** for standard markdown elements
- **Word Interop** for embedded HTML content

## License

MIT
