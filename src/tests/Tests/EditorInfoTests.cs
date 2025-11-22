[TestFixture]
public class EditorInfoTests
{
    [Test]
    public void AllEditors_ShouldContainExpectedEditors()
    {
        var editors = EditorInfo.AllEditors;

        IsNotEmpty(editors);
    }
}