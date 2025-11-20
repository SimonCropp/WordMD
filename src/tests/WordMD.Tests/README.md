# WordMD Tests

This project contains comprehensive tests for the WordMD solution using NUnit and Verify.

## Testing Framework

- **NUnit 4.x** - Test framework
- **Verify.NUnit** - Snapshot testing for assertions

## Test Structure

### EditorDefinitionsTests
Tests for the editor definitions and lookup functionality:
- Verifies all known editors are properly configured
- Tests case-insensitive lookup
- Validates editor properties

### ConfigurationServiceTests
Tests for configuration management:
- Configuration serialization/deserialization
- Editor order updates
- Default values

### DocxMarkdownServiceTests
Tests for DOCX file manipulation:
- Embedding markdown and images in DOCX files
- Extracting markdown and images
- Directory-based operations
- Multiple embed/extract cycles

### MarkdownFileWatcherTests
Tests for file system watching:
- File creation/modification detection
- Debouncing of rapid changes
- Filtering of hidden and temp files
- Subdirectory watching (images)

### RiderSettingsGeneratorTests
Tests for Rider configuration:
- DotSettings file generation
- XML validity
- Auto-save disabled setting

### EditorDetectionServiceTests
Tests for editor detection:
- Finding installed editors
- Path validation
- Consistency across multiple detections

## Running Tests

### Using dotnet CLI

Run all tests:
```bash
dotnet test
```

Run with detailed output:
```bash
dotnet test --verbosity normal
```

Run specific test class:
```bash
dotnet test --filter "FullyQualifiedName~EditorDefinitionsTests"
```

### Using Rider

1. Open WordMD.sln in Rider
2. Navigate to the test file
3. Click the run icon next to test methods
4. Or use Test Explorer (View → Tool Windows → Unit Tests)

### Using build script

```bash
build.cmd
```

This will restore, build, run tests, and pack the tool.

## Verify Snapshots

Verify creates snapshot files in the `Snapshots` folder for tests that use `await Verify()`. These snapshots are:

- **Committed to source control** - They represent expected outputs
- **Automatically compared** - Tests fail if actual output differs from snapshot
- **Easy to review** - Human-readable format for code review

### Updating Snapshots

If a test legitimately needs a new snapshot:

1. Run the test - it will fail
2. Review the `*.received.*` file in the Snapshots folder
3. If correct, delete the old `*.verified.*` file
4. Rename `*.received.*` to `*.verified.*`
5. Commit the new snapshot

Or use Verify's auto-accept features:
- Set `DiffEngine.UseAccept = true` in test
- Use Verify's DiffTool integration

## Test Categories

Tests are organized by the component they test:

- **Unit Tests** - Test individual classes in isolation
- **Integration Tests** - Test interactions between components (e.g., DocxMarkdownService with file system)

## Coverage Goals

Target code coverage:
- Core business logic: 80%+
- Service classes: 70%+
- Overall: 75%+

Run coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Writing New Tests

### Using NUnit

```csharp
[TestFixture]
public class MyServiceTests
{
    [SetUp]
    public void Setup()
    {
        // Initialize before each test
    }

    [Test]
    public void MyMethod_WithValidInput_ShouldSucceed()
    {
        // Arrange
        var service = new MyService();
        
        // Act
        var result = service.MyMethod("input");
        
        // Assert
        Assert.That(result, Is.EqualTo("expected"));
    }

    [TearDown]
    public void TearDown()
    {
        // Cleanup after each test
    }
}
```

### Using Verify

```csharp
[Test]
public async Task MyMethod_WithComplexOutput_ShouldMatchSnapshot()
{
    // Arrange
    var service = new MyService();
    
    // Act
    var result = service.MyMethod();
    
    // Assert with snapshot
    await Verify(result);
}
```

### Test Naming Convention

Format: `MethodName_Scenario_ExpectedBehavior`

Examples:
- `GetByName_WithValidName_ShouldReturnEditor`
- `DetectInstalledEditors_ShouldReturnList`
- `EmbedMarkdown_WithImages_ShouldEmbedAllImages`

## Best Practices

1. **Arrange-Act-Assert** - Structure tests clearly
2. **One assertion per test** - Or use `Assert.Multiple()`
3. **Descriptive names** - Test names should explain what's being tested
4. **Clean up resources** - Use `[TearDown]` for file system operations
5. **Isolated tests** - Tests should not depend on each other
6. **Verify for complex objects** - Use snapshots for objects, collections, or text
7. **Direct assertions for simple cases** - Use `Assert.That()` for primitives and booleans

## Troubleshooting

### Tests fail on first run
- Verify snapshots don't exist yet
- Review `*.received.*` files and accept if correct

### File system tests are flaky
- Increase delays in file watcher tests
- Ensure proper cleanup in `[TearDown]`

### Verify differences are hard to read
- Configure DiffTool in `ModuleInitializer.cs`
- Use `DiffEngine` for visual diffs

## CI/CD Integration

Tests are designed to run in CI/CD pipelines:
- No interactive prompts
- Deterministic output (with proper Verify configuration)
- Fast execution
- Clear failure messages

Add to your CI pipeline:
```yaml
- run: dotnet test --configuration Release --logger "trx" --results-directory TestResults
```
