using System.Diagnostics.CodeAnalysis;
using NexusMods.Abstractions.Games.Trees;
using NexusMods.Paths;
using NexusMods.Paths.Trees;
using NexusMods.Paths.Trees.Traits;

namespace NexusMods.Abstractions.GameLocators.Trees;

/// <summary>
///     The base class for a tree which stores items using <see cref="GamePath"/>(s)
/// </summary>
public abstract class AGamePathNodeTree<TValue>
{
    private readonly Dictionary<LocationId, KeyedBox<RelativePath, GamePathNode<TValue>>> _trees;

    /// <summary>
    /// Base constructor for a delegating tree, this is used to wrap a tree and delegate all calls to it.
    /// </summary>
    /// <param name="items">Items from which to create the tree.</param>
    protected AGamePathNodeTree(IEnumerable<KeyValuePair<GamePath, TValue>> items)
    {
        _trees = items.GroupBy(i => i.Key.LocationId)
            .Select(g => (g.Key, GamePathNode<TValue>.Create(g)))
            .ToDictionary(d => d.Key, d => d.Item2);
    }

    /// <summary>
    /// Enumerates all the recursive files in this tree.
    /// </summary>
    public IEnumerable<KeyedBox<RelativePath, GamePathNode<TValue>>> GetAllDescendentFiles()
    {
        return _trees.Values.SelectMany(e => e.GetFiles());
    }

    /// <summary>
    /// Returns all the files in a specific location
    /// </summary>
    /// <param name="id"></param>
    public KeyedBox<RelativePath, GamePathNode<TValue>> this[LocationId id] => _trees[id];

    /// <summary>
    /// Returns all the files in a specific location
    /// </summary>
    /// <param name="path"></param>
    public KeyedBox<RelativePath, GamePathNode<TValue>> this[GamePath path] => _trees[path.LocationId].FindByPathFromChild(path.Path)!;

    /// <summary>
    /// Attempts to get a value from the tree, returns false if the value does not exist.
    /// </summary>
    public bool TryGetValue(LocationId id, [MaybeNullWhen(false)] out KeyedBox<RelativePath, GamePathNode<TValue>> value)
    {
        return _trees.TryGetValue(id, out value);
    }

    /// <summary>
    /// Attempts to get a value from the tree, returns false if the value does not exist.
    /// </summary>
    public bool TryGetValue(GamePath path, [MaybeNullWhen(false)] out KeyedBox<RelativePath, GamePathNode<TValue>> value)
    {
        if (_trees.TryGetValue(path.LocationId, out var tree))
        {
            var tmpValue = tree.FindByPathFromChild(path.Path);
            if (tmpValue != null)
            {
                value = tmpValue;
                return true;
            }
        }

        value = default;
        return false;
    }

    /// <summary>
    /// Returns true if the tree contains a value for the given location
    /// </summary>
    public bool ContainsKey(GamePath path)
    {
        return TryGetValue(path, out _);
    }
}
