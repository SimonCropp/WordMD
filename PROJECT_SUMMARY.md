# WordMD Solution - Complete Package

## Overview

Complete .NET 10 solution for embedding and editing markdown documents within Word files (.docx), with comprehensive test coverage using NUnit and VerifyTests.

## What's Included

### Source Projects
- **WordMD.Core** - Core business logic library
- **WordMD.Cli** - Command-line tool (packaged as dotnet global tool)

### Test Project
- **WordMD.Tests** - Comprehensive test suite with 60+ tests
  - NUnit 4.2.2 for test framework
  - Verify.NUnit 26.6.0 for snapshot assertions
  - 7 test classes covering all major components
  - Integration tests for complete workflows

### Documentation
- `README.md` - Main project documentation
- `USAGE.md` - Usage examples and workflows
- `CONTRIBUTING.md` - Contribution guidelines
- `QUICKREF.md` - Quick reference card
- `TEST_COVERAGE.md` - Test coverage summary
- `RIDER_TESTING.md` - Rider-specific testing guide
- `tests/WordMD.Tests/README.md` - Test project details

### Build & Test Scripts
- `build.cmd` - Build, test, and pack the solution
- `test.cmd` - Run tests only

### Configuration
- `global.json` - .NET SDK version specification
- `Directory.Build.props` - Common project properties
- `.gitignore` - Git ignore patterns

## Solution Structure

```
WordMD/
├── src/
│   ├── WordMD.Core/              Core library
│   │   ├── EditorDefinitions.cs
│   │   ├── EditorDetectionService.cs
│   │   ├── ConfigurationService.cs
│   │   ├── RegistryService.cs
│   │   ├── DocxMarkdownService.cs
│   │   ├── MarkdownToWordService.cs
│   │   ├── MarkdownFileWatcher.cs
│   │   ├── RiderSettingsGenerator.cs
│   │   └── EditorService.cs
│   └── WordMD.Cli/               CLI tool
│       └── Program.cs
├── tests/
│   └── WordMD.Tests/             Test project
│       ├── EditorDefinitionsTests.cs
│       ├── ConfigurationServiceTests.cs
│       ├── DocxMarkdownServiceTests.cs
│       ├── MarkdownFileWatcherTests.cs
│       ├── RiderSettingsGeneratorTests.cs
│       ├── EditorDetectionServiceTests.cs
│       ├── IntegrationTests.cs
│       └── ModuleInitializer.cs
├── WordMD.sln                    Solution file
├── global.json
├── Directory.Build.props
├── build.cmd
├── test.cmd
└── [Documentation files]
```

## Quick Start

### 1. Build
```bash
dotnet restore
dotnet build
```

### 2. Run Tests
```bash
dotnet test
```

### 3. Install as Tool
```bash
dotnet pack -c Release
dotnet tool install -g --add-source src/WordMD.Cli/bin/Release WordMD
```

### 4. Setup Context Menu
```bash
wordmd
```

### 5. Edit a Document
Right-click any `.docx` file → WordMD Edit

## Key Features

### Core Functionality
- ✅ Embed markdown in Word documents
- ✅ Extract markdown for editing
- ✅ Auto-convert on save
- ✅ Image embedding and extraction
- ✅ File watching with debouncing
- ✅ Document protection

### Editor Support
- ✅ VS Code
- ✅ JetBrains Rider (with auto-save disabled)
- ✅ Typora
- ✅ Markdown Monster
- ✅ Obsidian
- ✅ Notepad

### Windows Integration
- ✅ Explorer context menu
- ✅ Registry configuration
- ✅ Configurable editor order
- ✅ Default editor selection

## Test Coverage

### Unit Tests (42 tests)
- EditorDefinitions (13 tests)
- ConfigurationService (8 tests)
- DocxMarkdownService (15 tests)
- RiderSettingsGenerator (6 tests)

### Integration Tests (18 tests)
- MarkdownFileWatcher (8 tests)
- EditorDetectionService (7 tests)
- Full workflow tests (7 tests)

### Snapshot Tests (15 tests)
Using Verify for complex object verification

## Technology Stack

- **.NET 10.0** - Latest .NET version
- **C# 14** - Latest language features
- **System.CommandLine** - CLI argument parsing
- **DocumentFormat.OpenXml** - DOCX manipulation
- **NUnit 4.2.2** - Test framework
- **Verify.NUnit 26.6.0** - Snapshot testing

## Requirements

- .NET 10.0 SDK or later
- Windows (for registry-based context menu)
- JetBrains Rider or Visual Studio 2025+ (recommended)
- At least one supported markdown editor

## Development Workflow

1. Clone/extract the solution
2. Open `WordMD.sln` in Rider
3. Build solution (`Ctrl+Shift+B`)
4. Run tests (`Ctrl+U, L`)
5. Make changes
6. Run affected tests
7. Create PR

## Testing in Rider

- Open Unit Tests window (`Ctrl+Alt+U`)
- Run all tests or specific test classes
- Debug with breakpoints
- View coverage
- Accept/reject Verify snapshots with diff tool
- Use continuous testing for immediate feedback

See `RIDER_TESTING.md` for detailed Rider workflows.

## CI/CD Ready

Tests are designed for continuous integration:
- No interactive prompts
- Deterministic output
- Fast execution
- Proper cleanup
- TRX logger support

## License

MIT License

## Support

- Open issues for bugs
- Submit PRs for features
- See CONTRIBUTING.md for guidelines

## Package Distribution

Ready for NuGet distribution:
```bash
dotnet pack -c Release
dotnet nuget push src/WordMD.Cli/bin/Release/WordMD.*.nupkg --source https://api.nuget.org/v3/index.json
```

## Notes

- Command line arguments use kebab-case (e.g., `--editor-order`)
- Verify snapshots are committed to source control
- Test coverage target: 75%+ overall
- All tests use proper cleanup in `[TearDown]`
- File system tests include delays for async operations
