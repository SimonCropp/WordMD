# WordMD Usage Examples

## First Time Setup

1. Install WordMD globally:
   ```bash
   dotnet tool install -g WordMD
   ```

2. Run initial setup:
   ```bash
   wordmd
   ```

   Output:
   ```
   WordMD Setup
   ============

   Detecting installed markdown editors...
   Found 3 editor(s):
     - Visual Studio Code
     - JetBrains Rider
     - Notepad

   Registering context menu entries...

   Setup complete!

   Right-click any .docx file to see the WordMD options:
     - 'WordMD Edit' - Opens with default editor
     - 'WordMD' submenu - Choose from available editors

   Default editor: vscode
   ```

## Editing Documents

### Via Context Menu

1. Navigate to a `.docx` file in Windows Explorer
2. Right-click the file
3. Select either:
   - **WordMD Edit** - Opens immediately with default editor
   - **WordMD > [Editor Name]** - Choose specific editor

### Via Command Line

Open with default editor:
```bash
wordmd "C:\Documents\MyDocument.docx"
```

Open with specific editor:
```bash
wordmd "C:\Documents\MyDocument.docx" vscode
wordmd "C:\Documents\MyDocument.docx" rider
wordmd "C:\Documents\MyDocument.docx" typora
```

## Customizing Editor Order

Change which editors appear first in the context menu:

```bash
wordmd --editor-order rider,vscode,typora
```

This makes Rider the default and orders the submenu accordingly.

## Workflow Example

1. **Start Editing**
   - Right-click `report.docx` → WordMD Edit
   - VS Code opens with `document.md` from temp directory

2. **Edit Content**
   ```markdown
   # Project Report
   
   ## Executive Summary
   
   This project achieved...
   
   ![Results Chart](images/chart.png)
   ```

3. **Auto-Save**
   - Save in VS Code (Ctrl+S)
   - WordMD detects change
   - Converts markdown to Word format
   - Updates `report.docx` automatically

4. **Add Images**
   - Copy image to `images/` folder in temp directory
   - Reference in markdown: `![Alt](images/newimage.png)`
   - Save - WordMD embeds the image in the docx

5. **Finish**
   - Close VS Code
   - Temp files cleaned up automatically
   - `report.docx` contains updated content + embedded markdown

## Rider-Specific Features

When using Rider, WordMD creates a `Default.DotSettings` file:

```xml
<wpf:ResourceDictionary ...>
  <s:Boolean x:Key="/Default/Environment/AutoSave/@EntryValue">False</s:Boolean>
</wpf:ResourceDictionary>
```

This prevents Rider's auto-save from triggering unnecessary conversions.

## Configuration File

Location: `%USERPROFILE%\.wordmd\config.json`

Example:
```json
{
  "EditorOrder": [
    "rider",
    "vscode",
    "typora"
  ],
  "DefaultEditor": "rider"
}
```

Edit manually or use `--editor-order` to update.

## Troubleshooting

### Editor Not Detected

Run setup again to re-scan for editors:
```bash
wordmd
```

### Context Menu Not Appearing

1. Ensure you have administrator privileges
2. Re-run setup:
   ```bash
   wordmd
   ```

### Markdown Not Converting

- Check that the document isn't open in Word
- Ensure you have write permissions
- Check console output for errors

## Advanced Scenarios

### Using Custom Markdown Syntax

WordMD passes your markdown through DocSharp.Markdown, which supports:
- Standard CommonMark
- Tables
- Task lists
- Strikethrough
- Embedded HTML (via Word Interop)

### Multiple Image References

```markdown
# Document

![Logo](images/logo.png)

Content here...

![Screenshot](images/screen1.png)
![Screenshot](images/screen2.png)
```

All images in the `images/` folder are embedded in the docx.

### Working with Existing Documents

If you have an existing `.docx` without embedded markdown:
1. Open with WordMD - it extracts empty markdown
2. Write your markdown
3. Save - WordMD replaces Word content with markdown-generated content
4. Future edits use the embedded markdown as source

## Best Practices

1. **Use meaningful filenames** for images
2. **Don't edit the Word document directly** - use markdown instead
3. **Commit both .docx and .md** if using version control
4. **Use Rider** for complex markdown with multiple files
5. **Test with simple documents** first
