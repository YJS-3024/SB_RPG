using System.Collections.Generic;
using DevelopKit;
using UnityEngine;
using yjs.DevKit;

/// <summary>
/// 게임에서 관리하는 리소스의 종류입니다.
/// </summary>
public enum eResourceType
{
    None = 0,
    Prefab_Unit_Rig,
    Prefab_Unit_Costume,
    Prefab_Unit_Accessory,
    Prefab_Unit_Skin,
    Prefab_UI,
    Model,
    Sprite,
    Texture,
    Material,
    Shader,
    Audio,
    Video,
    AnimationClip,
    AnimatorController,
    ScriptableObject,
    Font,
    Text,
    Other,
    Prefab_Unit_Weapon,
    Max
}

/// <summary>
/// Resources 에셋 로드와 캐시를 관리합니다.
/// </summary>
public class ResourceManager : MonoSingleton<ResourceManager>
{
    private readonly Dictionary<string, Object> _cache = new();
    private readonly Dictionary<eResourceType, string> _resourcePaths = new()
    {
        { eResourceType.Prefab_Unit_Rig, "Prefabs/CharacterParts" },
        { eResourceType.Prefab_Unit_Costume, "Prefabs/CharacterParts/Costume" },
        { eResourceType.Prefab_Unit_Accessory, "Prefabs/CharacterParts/Accessory" },
        { eResourceType.Prefab_Unit_Skin, "Prefabs/CharacterParts/Skin" },
        { eResourceType.Prefab_Unit_Weapon, "Prefabs/CharacterParts/Weapon" },
        { eResourceType.Prefab_UI, "Prefabs/UI" }
    };

    private bool _isInitialized;

    public override bool Initialize()
    {
        if (_isInitialized)
            return true;

        _isInitialized = true;
        return true;
    }

    public T Load<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        string normalizedPath = NormalizePath(path);
        T asset = Resources.Load<T>(normalizedPath);
        if (asset == null)
            RuntimeLog.Warning($"Resources load failed: {normalizedPath}");

        return asset;
    }

    public T Load<T>(eResourceType type, string assetName) where T : Object
    {
        return TryBuildPath(type, assetName, out string path) ? Load<T>(path) : null;
    }

    public T LoadCached<T>(string path) where T : Object
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        string normalizedPath = NormalizePath(path);
        string key = CreateCacheKey<T>(normalizedPath);

        if (_cache.TryGetValue(key, out Object cached) && cached != null)
            return cached as T;

        T asset = Resources.Load<T>(normalizedPath);
        if (asset != null)
            _cache[key] = asset;
        else
            RuntimeLog.Warning($"Resources load failed: {normalizedPath}");

        return asset;
    }

    public T LoadCached<T>(eResourceType type, string assetName) where T : Object
    {
        return TryBuildPath(type, assetName, out string path) ? LoadCached<T>(path) : null;
    }

    public T[] LoadAll<T>(string path) where T : Object
    {
        return Resources.LoadAll<T>(NormalizePath(path));
    }

    public T[] LoadAll<T>(eResourceType type) where T : Object
    {
        return TryGetRootPath(type, out string path) ? LoadAll<T>(path) : System.Array.Empty<T>();
    }

    public bool TryLoad<T>(string path, out T asset) where T : Object
    {
        asset = Load<T>(path);
        return asset != null;
    }

    public bool TryLoad<T>(eResourceType type, string assetName, out T asset) where T : Object
    {
        asset = Load<T>(type, assetName);
        return asset != null;
    }

    public bool UnloadCached<T>(string path) where T : Object
    {
        string normalizedPath = NormalizePath(path);
        string key = CreateCacheKey<T>(normalizedPath);

        if (!_cache.TryGetValue(key, out Object asset))
            return false;

        _cache.Remove(key);

        if (asset != null && !(asset is GameObject))
            Resources.UnloadAsset(asset);

        return true;
    }

    public void ClearCache()
    {
        _cache.Clear();
    }

    protected override void Destroy()
    {
        ClearCache();
        _isInitialized = false;
    }

    private bool TryBuildPath(eResourceType type, string assetName, out string path)
    {
        path = string.Empty;
        if (string.IsNullOrWhiteSpace(assetName) || !TryGetRootPath(type, out string rootPath))
            return false;

        path = NormalizePath(rootPath + "/" + assetName);
        return true;
    }

    private bool TryGetRootPath(eResourceType type, out string path)
    {
        if (_resourcePaths.TryGetValue(type, out path))
            return true;

        RuntimeLog.Warning($"Resource path is not registered: {type}");
        path = string.Empty;
        return false;
    }

    private static string CreateCacheKey<T>(string path) where T : Object
    {
        return typeof(T).FullName + ":" + path;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Replace('\\', '/').Trim('/');
    }
}