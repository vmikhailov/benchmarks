namespace TreeMap;

public class StorageFactory<T> : IMapStorageFactory where T : IMapStorage, new()
{
    private readonly string _name;

    public StorageFactory(string? name = null)
    {
        _name = name ?? typeof(T).Name.Replace("MapStorage_", "");
    }

    public IMapStorage Create() => new T();

    public override string ToString() => _name;
}