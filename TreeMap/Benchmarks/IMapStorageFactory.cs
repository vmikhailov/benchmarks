namespace TreeMap;

public interface IMapStorageFactory
{
    IMapStorage Create();

    string Name { get; }
}