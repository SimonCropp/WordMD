using ProjectFilesGenerator;

[TestFixture]
public class Launch
{
    [Test]
    [Explicit]
    public Task Run() =>
        Program.Main([ProjectFiles.empty_docx]);
}