namespace TreeMap;

public class StorageFactory<T> : IMapStorageFactory where T : IMapStorage
{
    private readonly object[] _args;
    private readonly string _name;

    public StorageFactory(string? name = null, params object[] args)
    {
        _args = args;
        _name = name ?? typeof(T).Name.Replace("MapStorage_", "");
    }

    public IMapStorage Create() => Activator.CreateInstance(typeof(T), _args) as IMapStorage
        ?? throw new InvalidOperationException($"Could not create instance of type {typeof(T).FullName}");

    public override string ToString() => _name;
}
