# WordMD Quick Reference

## Installation

```bash
# Install
dotnet tool install --global WordMD

# Update
dotnet tool update --global WordMD

# Uninstall
dotnet tool uninstall --global WordMD
```

## Setup

```bash
# Initial setup (detect editors, register context menu)
wordmd
```

## Basic Usage

```bash
# Edit with default editor
wordmd document.docx

# Edit with specific editor
wordmd document.docx vscode
wordmd document.docx rider
wordmd document.docx notepad
wordmd document.docx typora
```

## Configuration

```bash
# Set editor order (first = default)
wordmd --editor-order vscode,rider,notepad,typora

# Example: Make Rider the default
wordmd --editor-order rider,vscode
```

## Supported Editors

| Editor | Command Name | Executable |
|--------|-------------|------------|
| Visual Studio Code | `vscode` | `code` |
| JetBrains Rider | `rider` | `rider64.exe` |
| Notepad++ | `notepad` | `notepad++.exe` |
| Typora | `typora` | `typora.exe` |
| Markdown Monster | `markdownmonster` | `MarkdownMonster.exe` |
| Obsidian | `obsidian` | `obsidian.exe` |

## Context Menu

After running `wordmd` setup:

**Right-click any .docx file:**
- **WordMD Edit** → Opens in default editor
- **WordMD** → Submenu with all configured editors

## File Locations

```
# Configuration
%APPDATA%\WordMD\wordmd-config.json

# Temp directory during editing
%TEMP%\WordMD\<guid>\

# Registry entries
HKCU\SOFTWARE\Classes\.docx\shell\WordMD Edit
HKCU\SOFTWARE\Classes\.docx\shell\WordMD
```

## Common Tasks

### Change Default Editor

```bash
# Make Rider the default
wordmd --editor-order rider,vscode,notepad
```

### Edit with Images

1. Right-click .docx → "WordMD Edit"
2. In markdown, reference images:
   ```markdown
   ![My Image](image.png)
   ```
3. Copy `image.png` to temp directory (check console output for path)
4. Save markdown → image embeds in Word

### Batch Setup for Team

```bash
# Create setup script: setup.bat
@echo off
dotnet tool install --global WordMD
wordmd
pause
```

## Troubleshooting

### Editor not detected?

```bash
# Re-run setup
wordmd

# Check if editor is in PATH
where code      # VSCode
where rider64   # Rider
```

### Context menu not showing?

```bash
# Re-run setup (may need admin rights)
wordmd
```

### Changes not saving?

Check:
1. Editor is still open?
2. Saving the .md file (not just Word)?
3. Write permissions on .docx file?

## Build & Test

```bash
# Restore
dotnet restore

# Build
dotnet build

# Test
dotnet test

# Pack
dotnet pack --configuration Release

# Install locally
dotnet tool install -g --add-source ./artifacts WordMD
```

## Markdown Syntax Supported

```markdown
# Headings (H1-H6)

**bold** and *italic*

- Unordered lists
  - Nested items

1. Ordered lists
2. Second item

`inline code`

```csharp
// Code blocks
public void Method() { }
```

[Links](https://example.com)

![Images](image.png)

---
Horizontal rules
```

## Tips

✓ Save markdown frequently to see updates in Word  
✓ Use relative paths for images  
✓ Close editor when done to clean up temp files  
✓ Don't edit Word directly (it's protected)  
✓ Rider users: auto-save is disabled automatically  

## Help & Support

```bash
# Show help
wordmd --help

# Show version
dotnet tool list --global | findstr WordMD
```

## Resources

- README.md - Overview
- USAGE.md - Detailed examples
- CONTRIBUTING.md - Development guide
- CHANGELOG.md - Version history

---

**Quick Start:**
1. `dotnet tool install --global WordMD`
2. `wordmd`
3. Right-click .docx → "WordMD Edit"
