using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // 에디터 전용
#endif

// Addressables는 네 ResourceManager를 통해 사용한다고 가정(Managers.Resource.*)
[CreateAssetMenu(fileName = "PrefabManager", menuName = "MapEditor/PrefabManager")]
public class PrefabManager : ScriptableObject
{
    [Header("Addressables")]
    [SerializeField] string prefabLabel = "MapPrefabs"; // 네가 붙인 라벨명으로 변경

#if UNITY_EDITOR
    [Header("Editor Scan Folder (for MapEditor)")]
    [SerializeField] string editorPrefabFolder = "Assets/@Resources/Prefabs/Object";
    // 에디터에서 즉시 프리팹 참조용
    private readonly Dictionary<string, GameObject> _editorPrefabs = new();
#endif

    // 런타임(Addressables) 키 매핑
    private readonly Dictionary<string, string> _addrKeyByName = new();
    public bool IsReady { get; private set; }

    // ============== 에디터 전용 빠른 로딩 ==============
#if UNITY_EDITOR
    // MapEditorUIManager가 호출하는 기존 API 복구
    public void LoadAllPrefabs()
    {
        _editorPrefabs.Clear();

        // 지정 폴더에서 프리팹 스캔 (동기, 즉시)
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { editorPrefabFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                _editorPrefabs[prefab.name] = prefab; // 이름으로 키잉
        }
    }
#endif

    // ============== 런타임(Addressables) 초기화 ==============
    public IEnumerator Initialize(System.Action<float> onProgress = null)
    {
        if (IsReady) yield break;

        int total = 0;
        int loaded = 0;
        bool done = false;

        Managers.Resource.LoadAllAsync<GameObject>(prefabLabel, (primaryKey, cur, cnt) =>
        {
            // Addressables가 캐시에 올린 프리팹을 다시 꺼내 이름 매핑
            total = cnt;
            loaded = cur;

            var prefab = Managers.Resource.Load<GameObject>(primaryKey);
            if (prefab != null && !_addrKeyByName.ContainsKey(prefab.name))
                _addrKeyByName[prefab.name] = primaryKey;

            onProgress?.Invoke(total > 0 ? (float)loaded / total : 1f);
            if (loaded == total) { IsReady = true; done = true; }
        });

        while (!done) yield return null;
    }

    // ============== 공통 조회 ==============
    public GameObject GetPrefabByType(string name)
    {
#if UNITY_EDITOR
        // 에디터(맵 에디터 UI)에서는 에셋을 바로 반환
        if (_editorPrefabs.TryGetValue(name, out var go))
            return go;
#endif
        // 런타임(모바일 등)에서는 Addressables 캐시에서 반환
        if (_addrKeyByName.TryGetValue(name, out var key))
            return Managers.Resource.Load<GameObject>(key);

        return null;
    }


    public string[] GetPrefabNames()
    {
#if UNITY_EDITOR
        // 에디터에서 LoadAllPrefabs()로 채운 딕셔너리 우선
        if (_editorPrefabs != null && _editorPrefabs.Count > 0)
            return new List<string>(_editorPrefabs.Keys).ToArray();
#endif
        // 런타임(Addressables 초기화 후) 딕셔너리
        if (_addrKeyByName != null && _addrKeyByName.Count > 0)
            return new List<string>(_addrKeyByName.Keys).ToArray();

        return System.Array.Empty<string>();
    }
}
