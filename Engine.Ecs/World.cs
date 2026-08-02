using System.Collections;
using System.Runtime.InteropServices;

namespace Engine.Ecs;

// ─── Entity ───────────────────────────────────────────────────────────

public readonly struct Entity : IEquatable<Entity>
{
    public readonly int Id;
    public readonly int Generation;

    public Entity(int id, int generation)
    {
        Id = id;
        Generation = generation;
    }

    public bool Equals(Entity other) =>
        Id == other.Id && Generation == other.Generation;

    public override bool Equals(object? obj) =>
        obj is Entity e && Equals(e);

    public override int GetHashCode() =>
        HashCode.Combine(Id, Generation);

    public static bool operator ==(Entity a, Entity b) => a.Equals(b);
    public static bool operator !=(Entity a, Entity b) => !a.Equals(b);

    public override string ToString() => $"Entity({Id}, gen{Generation})";
}

// ─── ComponentType<T> — each struct gets a unique int ID ──────────────

internal static class ComponentTypeCounter
{
    internal static int Next;
}

public static class ComponentType<T> where T : struct
{
    public static readonly int Id = ComponentTypeCounter.Next++;
}

// ─── Archetype — stores entities that share the same component set ────

public class Archetype
{
    public BitArray Mask { get; }
    public int Count => _entities.Count;

    private readonly List<Entity> _entities = new();
    private readonly Array?[] _columns;

    public Archetype(BitArray mask, int totalComponentTypes)
    {
        Mask = mask;
        _columns = new Array?[totalComponentTypes];
    }

    public Entity GetEntity(int index) => _entities[index];

    public ReadOnlySpan<Entity> GetEntities() => CollectionsMarshal.AsSpan(_entities);

    public void AddEntity(Entity e)
    {
        _entities.Add(e);
    }

    public void RemoveAt(int row)
    {
        int last = _entities.Count - 1;
        if (row < last)
        {
            _entities[row] = _entities[last];
            for (int i = 0; i < _columns.Length; i++)
            {
                if (_columns[i] is Array col)
                {
                    var tmp = col.GetValue(last)!;
                    col.SetValue(tmp, row);
                }
            }
        }
        _entities.RemoveAt(last);

        for (int i = 0; i < _columns.Length; i++)
        {
            if (_columns[i] is Array col)
            {
                var resized = Array.CreateInstance(col.GetType().GetElementType()!, _entities.Count);
                Array.Copy(col, resized, _entities.Count);
                _columns[i] = resized;
            }
        }
    }

    public void SetColumn(int typeId, Array column)
    {
        _columns[typeId] = column;
    }

    public Array? GetRawColumn(int typeId) => _columns[typeId];

    public Span<T> GetColumn<T>() where T : struct
    {
        var arr = (T[])_columns[ComponentType<T>.Id]!;
        return arr.AsSpan(0, _entities.Count);
    }

    public int GrowColumn<T>() where T : struct
    {
        int id = ComponentType<T>.Id;
        int count = _entities.Count;

        if (_columns[id] is T[] existing)
        {
            if (existing.Length < count)
            {
                var bigger = new T[Math.Max(existing.Length * 2, 4)];
                existing.AsSpan().CopyTo(bigger);
                _columns[id] = bigger;
            }
            return count - 1;
        }
        else
        {
            var arr = new T[Math.Max(count, 4)];
            _columns[id] = arr;
            return count - 1;
        }
    }
}

// ─── World — the ECS manager ──────────────────────────────────────────

public class World
{
    private readonly List<Archetype> _archetypes = new();
    private int _nextEntityId;

    private readonly Dictionary<int, (int archIdx, int row)> _entityLocations = new();
    private readonly List<int> _entityGenerations = new();

    private const int MaxComponents = 32;

    public Entity Create()
    {
        int id = _nextEntityId++;
        int gen = 0;

        if (id < _entityGenerations.Count)
        {
            gen = _entityGenerations[id];
        }
        else
        {
            _entityGenerations.Add(0);
        }

        var entity = new Entity(id, gen);
        var mask = new BitArray(MaxComponents);
        var arch = GetOrCreateArchetype(mask);
        int row = arch.Count;
        arch.AddEntity(entity);

        int archIdx = _archetypes.IndexOf(arch);
        _entityLocations[id] = (archIdx, row);

        return entity;
    }

    public void AddComponent<T>(Entity entity, T component) where T : struct
    {
        if (!_entityLocations.TryGetValue(entity.Id, out var loc))
            return;

        var oldArch = _archetypes[loc.archIdx];

        var newMask = (BitArray)oldArch.Mask.Clone();
        newMask[ComponentType<T>.Id] = true;

        var newArch = GetOrCreateArchetype(newMask);

        int newRow = newArch.Count;
        newArch.AddEntity(entity);

        for (int i = 0; i < MaxComponents; i++)
        {
            if (oldArch.Mask[i])
            {
                CopyColumn(oldArch, newArch, i, loc.row, newRow);
            }
        }

        int newSlot = newArch.GrowColumn<T>();
        var newCol = (T[])newArch.GetRawColumn(ComponentType<T>.Id)!;
        newCol[newRow] = component;

        oldArch.RemoveAt(loc.row);
        ReindexArchetype(loc.archIdx);

        int newArchIdx = _archetypes.IndexOf(newArch);
        _entityLocations[entity.Id] = (newArchIdx, newRow);
    }

    public void RemoveComponent<T>(Entity entity) where T : struct
    {
        if (!_entityLocations.TryGetValue(entity.Id, out var loc))
            return;

        var oldArch = _archetypes[loc.archIdx];

        var newMask = (BitArray)oldArch.Mask.Clone();
        newMask[ComponentType<T>.Id] = false;

        var newArch = GetOrCreateArchetype(newMask);

        int newRow = newArch.Count;
        newArch.AddEntity(entity);

        for (int i = 0; i < MaxComponents; i++)
        {
            if (i == ComponentType<T>.Id) continue;
            if (oldArch.Mask[i])
            {
                CopyColumn(oldArch, newArch, i, loc.row, newRow);
            }
        }

        oldArch.RemoveAt(loc.row);
        ReindexArchetype(loc.archIdx);

        int newArchIdx = _archetypes.IndexOf(newArch);
        _entityLocations[entity.Id] = (newArchIdx, newRow);
    }

    public void DestroyEntity(Entity entity)
    {
        if (!_entityLocations.TryGetValue(entity.Id, out var loc))
            return;

        var arch = _archetypes[loc.archIdx];
        arch.RemoveAt(loc.row);
        ReindexArchetype(loc.archIdx);
        _entityLocations.Remove(entity.Id);
    }

    public bool TryGetComponent<T>(Entity entity, out T component) where T : struct
    {
        if (_entityLocations.TryGetValue(entity.Id, out var loc))
        {
            var arch = _archetypes[loc.archIdx];
            if (arch.Mask[ComponentType<T>.Id])
            {
                component = arch.GetColumn<T>()[loc.row];
                return true;
            }
        }

        component = default;
        return false;
    }

    public T GetComponent<T>(Entity entity) where T : struct
        => TryGetComponent(entity, out T component) ? component : default;

    public void SetComponent<T>(Entity entity, T component) where T : struct
    {
        if (!_entityLocations.TryGetValue(entity.Id, out var loc))
            return;

        var arch = _archetypes[loc.archIdx];
        if (arch.Mask[ComponentType<T>.Id])
            arch.GetColumn<T>()[loc.row] = component;
    }

    public Query1<T> Query<T>() where T : struct
    {
        return new Query1<T>(_archetypes);
    }

    public Query2<T1, T2> Query<T1, T2>()
        where T1 : struct
        where T2 : struct
    {
        return new Query2<T1, T2>(_archetypes);
    }

    public Query3<T1, T2, T3> Query<T1, T2, T3>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
    {
        return new Query3<T1, T2, T3>(_archetypes);
    }

    public Query4<T1, T2, T3, T4> Query<T1, T2, T3, T4>()
        where T1 : struct
        where T2 : struct
        where T3 : struct
        where T4 : struct
    {
        return new Query4<T1, T2, T3, T4>(_archetypes);
    }

    private Archetype GetOrCreateArchetype(BitArray mask)
    {
        foreach (var arch in _archetypes)
        {
            if (arch.Mask.Length == mask.Length && MaskEquals(arch.Mask, mask))
                return arch;
        }

        var newArch = new Archetype(mask, MaxComponents);
        _archetypes.Add(newArch);
        return newArch;
    }

    private void CopyColumn(Archetype from, Archetype to, int typeId, int fromRow, int toRow)
    {
        var src = from.GetRawColumn(typeId);
        if (src == null) return;

        var dst = to.GetRawColumn(typeId);
        int required = toRow + 1;

        if (dst == null)
        {
            dst = Array.CreateInstance(src.GetType().GetElementType()!, Math.Max(required, 4));
            to.SetColumn(typeId, dst);
        }
        else if (dst.Length < required)
        {
            var grown = Array.CreateInstance(dst.GetType().GetElementType()!, Math.Max(required, dst.Length * 2));
            Array.Copy(dst, grown, dst.Length);
            to.SetColumn(typeId, grown);
            dst = grown;
        }

        dst.SetValue(src.GetValue(fromRow), toRow);
    }

    private void ReindexArchetype(int archIdx)
    {
        var arch = _archetypes[archIdx];
        for (int i = 0; i < arch.Count; i++)
        {
            var e = arch.GetEntity(i);
            _entityLocations[e.Id] = (archIdx, i);
        }
    }

    private static bool MaskEquals(BitArray a, BitArray b)
    {
        if (a.Length != b.Length) return false;
        var aWords = new int[(a.Length + 31) / 32];
        var bWords = new int[(b.Length + 31) / 32];
        a.CopyTo(aWords, 0);
        b.CopyTo(bWords, 0);
        for (int i = 0; i < aWords.Length; i++)
            if (aWords[i] != bWords[i]) return false;
        return true;
    }
}

// ─── Queries ──────────────────────────────────────────────────────────

public struct Query1<T1> where T1 : struct
{
    private readonly List<Archetype> _archetypes;

    public Query1(List<Archetype> archetypes) => _archetypes = archetypes;

    public void ForEach(Action<Span<T1>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || arch.Count == 0) continue;
            func(arch.GetColumn<T1>());
        }
    }

    public void ForEachEntity(Action<ReadOnlySpan<Entity>, Span<T1>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || arch.Count == 0) continue;
            func(arch.GetEntities(), arch.GetColumn<T1>());
        }
    }
}

public struct Query2<T1, T2> where T1 : struct where T2 : struct
{
    private readonly List<Archetype> _archetypes;

    public Query2(List<Archetype> archetypes) => _archetypes = archetypes;

    public void ForEach(Action<Span<T1>, Span<T2>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        int t2Id = ComponentType<T2>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || !arch.Mask[t2Id] || arch.Count == 0) continue;
            func(arch.GetColumn<T1>(), arch.GetColumn<T2>());
        }
    }

    public void ForEachEntity(Action<ReadOnlySpan<Entity>, Span<T1>, Span<T2>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        int t2Id = ComponentType<T2>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || !arch.Mask[t2Id] || arch.Count == 0) continue;
            func(arch.GetEntities(), arch.GetColumn<T1>(), arch.GetColumn<T2>());
        }
    }
}

public struct Query3<T1, T2, T3> where T1 : struct where T2 : struct where T3 : struct
{
    private readonly List<Archetype> _archetypes;

    public Query3(List<Archetype> archetypes) => _archetypes = archetypes;

    public void ForEach(Action<Span<T1>, Span<T2>, Span<T3>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        int t2Id = ComponentType<T2>.Id;
        int t3Id = ComponentType<T3>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || !arch.Mask[t2Id] || !arch.Mask[t3Id] || arch.Count == 0) continue;
            func(arch.GetColumn<T1>(), arch.GetColumn<T2>(), arch.GetColumn<T3>());
        }
    }

    public void ForEachEntity(Action<ReadOnlySpan<Entity>, Span<T1>, Span<T2>, Span<T3>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        int t2Id = ComponentType<T2>.Id;
        int t3Id = ComponentType<T3>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || !arch.Mask[t2Id] || !arch.Mask[t3Id] || arch.Count == 0) continue;
            func(arch.GetEntities(), arch.GetColumn<T1>(), arch.GetColumn<T2>(), arch.GetColumn<T3>());
        }
    }
}

public struct Query4<T1, T2, T3, T4> where T1 : struct where T2 : struct where T3 : struct where T4 : struct
{
    private readonly List<Archetype> _archetypes;

    public Query4(List<Archetype> archetypes) => _archetypes = archetypes;

    public void ForEach(Action<Span<T1>, Span<T2>, Span<T3>, Span<T4>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        int t2Id = ComponentType<T2>.Id;
        int t3Id = ComponentType<T3>.Id;
        int t4Id = ComponentType<T4>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || !arch.Mask[t2Id] || !arch.Mask[t3Id] || !arch.Mask[t4Id] || arch.Count == 0) continue;
            func(arch.GetColumn<T1>(), arch.GetColumn<T2>(), arch.GetColumn<T3>(), arch.GetColumn<T4>());
        }
    }

    public void ForEachEntity(Action<ReadOnlySpan<Entity>, Span<T1>, Span<T2>, Span<T3>, Span<T4>> func)
    {
        int t1Id = ComponentType<T1>.Id;
        int t2Id = ComponentType<T2>.Id;
        int t3Id = ComponentType<T3>.Id;
        int t4Id = ComponentType<T4>.Id;
        foreach (var arch in _archetypes)
        {
            if (!arch.Mask[t1Id] || !arch.Mask[t2Id] || !arch.Mask[t3Id] || !arch.Mask[t4Id] || arch.Count == 0) continue;
            func(arch.GetEntities(), arch.GetColumn<T1>(), arch.GetColumn<T2>(), arch.GetColumn<T3>(), arch.GetColumn<T4>());
        }
    }
}
