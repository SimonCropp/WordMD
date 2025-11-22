# WordMD Usage Examples

This guide provides detailed examples of how to use WordMD in different scenarios.

## Installation

### Install as Global Tool

```bash
dotnet tool install --global WordMD
```

### Update to Latest Version

```bash
dotnet tool update --global WordMD
```

### Uninstall

```bash
dotnet tool uninstall --global WordMD
```

## Initial Setup

After installation, run setup to detect editors and register context menu:

```bash
wordmd
```

**Output:**
```
[INFO] Setting up WordMD...
[INFO] Detected installed editor: Visual Studio Code
[INFO] Detected installed editor: JetBrains Rider
[INFO] WordMD setup completed successfully
[INFO] Detected editors: vscode, rider
```

## Editing Documents

### Example 1: Edit with Default Editor

```bash
wordmd "C:\Documents\MyDocument.docx"
```

This will:
1. Extract markdown and images to temp directory
2. Open the markdown file in your default editor (first in editor order)
3. Watch for changes in the temp directory
4. Convert markdown to Word and update the .docx on save
5. Clean up temp files when editor closes

### Example 2: Edit with Specific Editor

```bash
wordmd "C:\Documents\MyDocument.docx" rider
```

For Rider, this will also:
- Create a `Default.DotSettings` file to disable auto-save
- Prevent unwanted automatic saves

### Example 3: Edit from Explorer

Right-click on a .docx file:
- Select **"WordMD Edit"** for default editor
- Select **"WordMD"** → **"Visual Studio Code"** for specific editor

## Configuration

### Example 4: Set Editor Preference Order

```bash
wordmd --editor-order vscode,rider,typora,notepad
```

This sets your preferred editor order. The first editor in the list becomes the default.

### Example 5: Prioritize Rider Over VSCode

```bash
wordmd --editor-order rider,vscode
```

Now Rider is the default, and "WordMD Edit" will use Rider.

## Workflow Scenarios

### Scenario 1: Documentation with Images

1. Create a Word document
2. Right-click → "WordMD Edit"
3. Write markdown with image references:
   ```markdown
   # My Document
   
   ![Architecture Diagram](diagram.png)
   ```
4. Add `diagram.png` to the temp directory (shown in logs)
5. Save the markdown file
6. Images are automatically embedded in the Word document

### Scenario 2: Team Collaboration

1. Share a .docx file with embedded markdown
2. Team members can edit with their preferred editor
3. Markdown stays consistent across different editors
4. Word formatting is regenerated from markdown

### Scenario 3: Version Control Friendly

```bash
# Edit the document
wordmd "Project.docx" vscode

# Document is updated with markdown source
# The embedded markdown can be extracted and versioned separately
```

## Advanced Usage

### Custom Editor Detection

If your editor isn't detected automatically, ensure it's in your PATH:

```bash
# Check if editor is in PATH
where code      # Visual Studio Code
where rider64   # JetBrains Rider
```

### Temp Directory Location

WordMD uses `%TEMP%\WordMD\<guid>` for temporary files. Check logs for exact location:

```
[INFO] Using temp directory: C:\Users\Username\AppData\Local\Temp\WordMD\abc123...
```

### Configuration File Location

Configuration is stored at:
```
%APPDATA%\WordMD\wordmd-config.json
```

Example content:
```json
{
  "EditorOrder": [
    "vscode",
    "rider",
    "notepad"
  ]
}
```

## Troubleshooting

### Issue: Editor Doesn't Open

**Solution:** Ensure the editor is installed and in PATH:
```bash
wordmd  # Re-run setup to detect editors
```

### Issue: Changes Not Saving to Word

**Check:**
1. Is the file watcher running? (check console logs)
2. Is the temp directory still accessible?
3. Do you have write permissions to the .docx file?

### Issue: Context Menu Not Showing

**Solution:** Re-run setup with administrator privileges:
```bash
# Run as administrator
wordmd
```

### Issue: Rider Auto-Save Still Enabled

The `.DotSettings` file should be in the temp directory. Check:
```
C:\Users\Username\AppData\Local\Temp\WordMD\<guid>\Default.DotSettings
```

## Tips and Best Practices

### 1. Use Relative Image Paths

```markdown
![Logo](images/logo.png)
```

Place images in a subfolder for organization.

### 2. Prefer Markdown Features

Use markdown syntax over HTML when possible for better portability.

### 3. Save Frequently

Changes are detected on save, so save your markdown file to see updates in Word.

### 4. Close Editor When Done

Temp files are only cleaned up when the editor closes.

### 5. Don't Edit Word Directly

The document has restricted editing (password: "WordMD") to prevent conflicts.

## Integration with Other Tools

### VS Code Extensions

Recommended extensions:
- Markdown All in One
- Markdown Preview Enhanced
- markdownlint

### Rider Plugins

Recommended plugins:
- Markdown Navigator
- Markdown Editor

### Git Integration

```bash
# Extract markdown for versioning
wordmd "document.docx" vscode
# Markdown is in temp directory - copy it to version control
```

## Markdown Features Supported

WordMD supports standard markdown plus:

- **Headings** (H1-H6)
- **Bold** and *italic*
- Lists (ordered and unordered)
- Code blocks with syntax highlighting
- Inline code
- Links
- Images
- Horizontal rules
- Tables (via Markdig extensions)
- Task lists
- Strikethrough

## Example Markdown

```markdown
# Document Title

## Introduction

This is a **bold** statement and this is *italic*.

### Features

- Feature 1
- Feature 2
  - Sub-feature 2.1
  - Sub-feature 2.2

### Code Example

Here's some `inline code`.

```csharp
public class Example
{
    public void Method()
    {
        Console.WriteLine("Hello, WordMD!");
    }
}
```

### Image

![Diagram](architecture.png)

---

## Conclusion

For more information, visit [the docs](https://example.com).
```

## Support

For issues, questions, or feature requests:
- GitHub Issues: https://github.com/wordmd/wordmd/issues
- Discussions: https://github.com/wordmd/wordmd/discussions
