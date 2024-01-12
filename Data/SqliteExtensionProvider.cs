namespace QueryTerminal.Data;

public class SqliteExtensionProvider
{
    private IEnumerable<string> _extensions = Enumerable.Empty<string>();

    public void RegisterExtension(string extensionFile)
    {
        _extensions = _extensions.Append(extensionFile);
    }

    public IEnumerable<string> GetExtensions()
    {
        return _extensions;
    }
}
