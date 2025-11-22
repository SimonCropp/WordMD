# WordMD Solution Overview

Complete .NET 10 solution for editing markdown embedded in Word documents.

## Solution Structure

```
WordMD/
├── .github/
│   └── workflows/
│       └── ci.yml                      # GitHub Actions CI/CD pipeline
├── src/
│   └── WordMD/                         # Main application (dotnet tool)
│       ├── WordMD.csproj              # Project file with all dependencies
│       ├── Program.cs                 # Entry point with System.CommandLine
│       ├── Core/                      # Core functionality
│       │   ├── WordMDDocument.cs      # Document manipulation (extract/embed)
│       │   └── FileChangeWatcher.cs   # File system monitoring
│       ├── Editors/                   # Editor management
│       │   ├── EditorInfo.cs          # Editor definitions
│       │   ├── EditorConfiguration.cs # Config persistence
│       │   └── RegistryManager.cs     # Windows context menu
│       └── Conversion/                # Markdown conversion
│           ├── MarkdownToWordConverter.cs # MD to Word using Markdig
│           ├── RiderSettingsGenerator.cs  # .DotSettings creation
│           └── EditorLauncher.cs          # Edit session orchestration
├── tests/
│   └── WordMD.Tests/                   # NUnit test project
│       ├── WordMD.Tests.csproj        # Test project
│       ├── ModuleInitializer.cs       # Verify setup
│       ├── EditorConfigurationTests.cs
│       ├── EditorInfoTests.cs
│       └── FileChangeWatcherTests.cs
├── Directory.Build.props               # Common MSBuild properties
├── Directory.Packages.props            # Central package management
├── .editorconfig                       # Code style configuration
├── .gitignore                         # Git ignore patterns
├── global.json                        # .NET SDK version lock
├── WordMD.slnx                        # Visual Studio solution (new XML format)
├── README.md                          # Main documentation
├── USAGE.md                           # Detailed usage guide
├── CONTRIBUTING.md                    # Contribution guidelines
├── CHANGELOG.md                       # Version history
└── LICENSE                            # MIT License

```

## Key Features Implemented

### 1. Document Manipulation (Core namespace)
- **WordMDDocument**: Handles extraction and embedding of markdown/images as EmbeddedPackageParts
- **FileChangeWatcher**: Monitors temp directory for changes with debouncing

### 2. Editor Integration (Editors namespace)
- **EditorInfo**: Defines 6 markdown editors (VSCode, Rider, Notepad++, Typora, Markdown Monster, Obsidian)
- **EditorConfiguration**: Manages editor detection and ordering, persists to JSON config
- **RegistryManager**: Creates Windows Explorer context menu entries

### 3. Conversion (Conversion namespace)
- **MarkdownToWordConverter**: Converts markdown to Word using Markdig
  - Supports headings, paragraphs, lists, code blocks, emphasis, images
- **RiderSettingsGenerator**: Creates .DotSettings to disable auto-save
- **EditorLauncher**: Orchestrates the entire edit session

### 4. CLI Tool (Program.cs)
- **Program.cs**: Entry point using System.CommandLine
  - `wordmd` → Setup (detect editors, register context menu)
  - `wordmd <path>` → Edit with default editor
  - `wordmd <path> <editor>` → Edit with specific editor
  - `wordmd --editor-order <list>` → Configure editor order

### 5. Testing (WordMD.Tests)
- NUnit test framework
- Verify for snapshot testing
- FluentAssertions for readable assertions
- Tests for configuration, editors, and file watching
- **Code style**: Test classes have no namespaces (global namespace)

## Technology Stack

- **.NET 10** with **C# 14**
- **DocumentFormat.OpenXml 3.2.0** - Word document manipulation
- **Markdig 0.39.0** - Markdown parsing and processing
- **System.CommandLine 2.0.0** - CLI argument parsing (stable release)
- **Microsoft.Extensions.*** - Dependency injection, logging, configuration
- **NUnit 4.2.2** - Testing framework
- **Verify.NUnit 28.4.1** - Snapshot testing

## Build Configuration

### Central Package Management
All package versions are managed centrally in `Directory.Packages.props`:
- Ensures consistent versions across projects
- Simplifies updates
- Uses latest stable versions

### Common Properties
`Directory.Build.props` sets:
- Target Framework: net10.0-windows
- Language Version: C# 14
- Nullable reference types enabled
- Implicit usings enabled
- Warnings as errors
- Code analyzers enabled

### Code Style
`.editorconfig` enforces:
- Consistent formatting
- Naming conventions
- Code style rules
- C# preferences

## Deployment

### As .NET Global Tool
Package as NuGet tool:
```bash
dotnet pack --configuration Release
dotnet tool install --global WordMD --add-source ./artifacts
```

### From NuGet.org
```bash
dotnet tool install --global WordMD
```

## CI/CD Pipeline

GitHub Actions workflow (`.github/workflows/ci.yml`):
- Builds on windows-latest
- Runs all tests
- Creates NuGet packages
- Publishes to NuGet.org on main branch push

## Usage Flow

1. **Setup**
   ```bash
   wordmd
   ```
   - Detects installed editors
   - Saves to `%APPDATA%\WordMD\wordmd-config.json`
   - Registers context menu in Windows Registry

2. **Edit from Explorer**
   - Right-click .docx → "WordMD Edit"
   - Launches default editor

3. **Edit Process**
   - Extracts markdown + images to temp dir
   - Creates Rider .DotSettings if using Rider
   - Launches editor
   - Watches for file changes
   - On save: converts MD → Word, updates .docx, re-embeds
   - On editor close: cleans up temp files

## Configuration

### Config File Location
`%APPDATA%\WordMD\wordmd-config.json`

### Example Config
```json
{
  "EditorOrder": [
    "vscode",
    "rider",
    "notepad",
    "typora"
  ]
}
```

### Registry Entries
Context menu entries at:
```
HKEY_CURRENT_USER\SOFTWARE\Classes\.docx\shell\WordMD Edit
HKEY_CURRENT_USER\SOFTWARE\Classes\.docx\shell\WordMD
```

## Extension Points

### Adding New Editors
1. Add editor definition in `EditorInfo.cs`
2. Add to `AllEditors` array
3. Implement path detection
4. Add tests

### Custom Markdown Extensions
Modify `MarkdownToWordConverter.cs`:
- Add new Markdig extensions
- Handle custom block types
- Implement custom inline rendering

### Additional Word Manipulation
Extend `WordMDDocument.cs`:
- Custom document properties
- Advanced formatting
- Style management
- Template support

## Code Style

### General
- C# 14 features throughout
- Nullable reference types enabled
- Implicit usings enabled
- Warnings treated as errors

### Test Projects
Test classes use a minimal style with no namespaces:

```csharp
using FluentAssertions;
using WordMD.Editors;

[TestFixture]
public class EditorConfigurationTests
{
    [Test]
    public void TestName()
    {
        // Test implementation
    }
}
```

This approach:
- Reduces boilerplate
- Keeps tests focused
- Leverages global namespace
- Aligns with modern C# test practices

## Testing Strategy

### Unit Tests
- Editor configuration
- Editor detection
- File watching
- Configuration persistence

### Integration Tests
- Full edit workflow
- Document extraction/embedding
- Markdown conversion
- Temp file cleanup

### Snapshot Tests (Verify)
- Configuration file format
- Editor properties
- Conversion output

## Dependencies

All dependencies use latest stable versions:
- All packages are stable releases (no pre-release/beta versions)
- Regular security updates recommended
- Central management simplifies updates

## Platform Requirements

- Windows 10/11 (for registry integration)
- .NET 10 Runtime
- Visual Studio 2022 17.10+ or JetBrains Rider 2023.3+ (for .slnx support)
- Markdown editor(s) installed

## Solution Format

This solution uses the new .slnx format (XML-based solution format) introduced in Visual Studio 2022 17.10. Benefits include:

- **Human-readable XML** - Easy to read and modify
- **Better merge conflicts** - Simpler structure reduces conflicts
- **Version control friendly** - Clean diffs in source control
- **No GUIDs** - Projects referenced by path only
- **Solution folders** - Native support for organizing projects

To convert back to legacy .sln format if needed:
```bash
dotnet sln WordMD.slnx list
# Then create new .sln with the listed projects
```

## Security Considerations

- Document password: "WordMD" (configurable)
- Registry writes: HKEY_CURRENT_USER (no admin required)
- Temp files: User's temp directory
- Config files: User's AppData

## Performance Characteristics

- File watching: Debounced (500ms)
- Conversion: On-demand (only on save)
- Temp cleanup: On editor exit
- Memory: Minimal (background process)

## Known Limitations

1. Windows-only (registry integration)
2. Restricted editing implementation simplified
3. HTML in markdown requires Word Interop (not implemented)
4. Limited to text-based markdown features

## Future Enhancements

Potential features for future versions:
- Cross-platform support (Linux/Mac)
- HTML-in-markdown via Word Interop
- Custom document templates
- Batch processing
- Git integration for embedded markdown
- Web-based editor option
- Document history/versioning

## Documentation

- **README.md** - Overview and quick start
- **USAGE.md** - Detailed examples and scenarios
- **CONTRIBUTING.md** - Development guidelines
- **CHANGELOG.md** - Version history
- **LICENSE** - MIT License

## Getting Started

1. Open solution in Rider or Visual Studio 2022 17.10+
2. Restore packages: `dotnet restore`
3. Build: `dotnet build`
4. Run tests: `dotnet test`
5. Pack: `dotnet pack`
6. Install locally: `dotnet tool install -g --add-source ./artifacts WordMD`
7. Setup: `wordmd`
8. Use: Right-click .docx → "WordMD Edit"

**Note:** The solution uses the new .slnx format. If your IDE doesn't support it yet, you can still build using the dotnet CLI.

## Support

For questions, issues, or contributions:
- Review CONTRIBUTING.md
- Check existing issues
- Submit detailed bug reports
- Propose features with use cases

---

**Version:** 1.0.0  
**Framework:** .NET 10  
**Language:** C# 14  
**License:** MIT  
**Platform:** Windows
