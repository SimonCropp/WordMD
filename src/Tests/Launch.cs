using ProjectFilesGenerator;

[TestFixture]
public class Launch
{
    [Test]
    [Explicit]
    public Task Empty() =>
        Program.Main([ProjectFiles.empty_docx]);
    [Test]
    [Explicit]
    public Task Run() =>
        Program.Main([ProjectFiles.sample_docx]);
}