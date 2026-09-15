using System.Collections.Generic;
using DevelopKit;
using UnityEngine;

/// <summary>
/// 게임에서 관리하는 리소스의 종류입니다.
/// </summary>
public enum eResourceType
{
    None = 0,
    Prefab_Unit_Costume,
    Prefab_Unit_Accessory,
    Prefab_Unit_Skin,
    Prefab_UI,
    Sprite,
    Texture,
    Max
}

/// <summary>
/// Resources 에셋 로드와 캐시를 관리합니다.
/// </summary>
public class ResourceManager : MonoSingleton<ResourceManager>
{
    private readonly Dictionary<string, Object> _cache = new();

    private readonly List<KeyValuePair<eResourceType, string>> _dicResourcePaths = new List<KeyValuePair<eResourceType, string>>
    {
        new KeyValuePair<eResourceType, string>(eResourceType.Prefab_Unit_Costume, "Prefabs/CharacterParts/Costume/"),
        new KeyValuePair<eResourceType, string>(eResourceType.Prefab_Unit_Accessory, "Prefabs/CharacterParts/Accessory/"),
        new KeyValuePair<eResourceType, string>(eResourceType.Prefab_Unit_Skin, "Prefabs/CharacterParts/Skin/"),
        new KeyValuePair<eResourceType, string>(eResourceType.Prefab_UI, "Prefabs/UI/")
    };

    public override bool Initialize()
    {
        return true;
    }

    public T Load<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        return Resources.Load<T>(NormalizePath(path));
    }

    public T LoadCached<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        string normalizedPath = NormalizePath(path);
        string key = typeof(T).FullName + ":" + normalizedPath;

        if (_cache.TryGetValue(key, out Object cached) && cached != null)
            return cached as T;

        T asset = Resources.Load<T>(normalizedPath);
        if (asset != null)
            _cache[key] = asset;

        return asset;
    }

    public T[] LoadAll<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(NormalizePath(path));
    }

    public bool TryLoad<T>(string path, out T asset) where T : Object
    {
        asset = Load<T>(path);
        return asset != null;
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    protected override void Destroy()
    {
        ClearCache();
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/').Trim('/');
    }
}
