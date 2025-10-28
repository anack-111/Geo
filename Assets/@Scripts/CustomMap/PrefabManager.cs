using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor; // ������ ����
#endif

// Addressables�� �� ResourceManager�� ���� ����Ѵٰ� ����(Managers.Resource.*)
[CreateAssetMenu(fileName = "PrefabManager", menuName = "MapEditor/PrefabManager")]
public class PrefabManager : ScriptableObject
{
    [Header("Addressables")]
    [SerializeField] string prefabLabel = "MapPrefabs"; // �װ� ���� �󺧸����� ����

#if UNITY_EDITOR
    [Header("Editor Scan Folder (for MapEditor)")]
    [SerializeField] string editorPrefabFolder = "Assets/@Resources/Prefabs/Object";
    // �����Ϳ��� ��� ������ ������
    private readonly Dictionary<string, GameObject> _editorPrefabs = new();
#endif

    // ��Ÿ��(Addressables) Ű ����
    private readonly Dictionary<string, string> _addrKeyByName = new();
    public bool IsReady { get; private set; }

    // ============== ������ ���� ���� �ε� ==============
#if UNITY_EDITOR
    // MapEditorUIManager�� ȣ���ϴ� ���� API ����
    public void LoadAllPrefabs()
    {
        _editorPrefabs.Clear();

        // ���� �������� ������ ��ĵ (����, ���)
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { editorPrefabFolder });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (prefab != null)
                _editorPrefabs[prefab.name] = prefab; // �̸����� Ű��
        }
    }
#endif

    // ============== ��Ÿ��(Addressables) �ʱ�ȭ ==============
    public IEnumerator Initialize(System.Action<float> onProgress = null)
    {
        if (IsReady) yield break;

        int total = 0;
        int loaded = 0;
        bool done = false;

        Managers.Resource.LoadAllAsync<GameObject>(prefabLabel, (primaryKey, cur, cnt) =>
        {
            // Addressables�� ĳ�ÿ� �ø� �������� �ٽ� ���� �̸� ����
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

    // ============== ���� ��ȸ ==============
    public GameObject GetPrefabByType(string name)
    {
#if UNITY_EDITOR
        // ������(�� ������ UI)������ ������ �ٷ� ��ȯ
        if (_editorPrefabs.TryGetValue(name, out var go))
            return go;
#endif
        // ��Ÿ��(����� ��)������ Addressables ĳ�ÿ��� ��ȯ
        if (_addrKeyByName.TryGetValue(name, out var key))
            return Managers.Resource.Load<GameObject>(key);

        return null;
    }


    public string[] GetPrefabNames()
    {
#if UNITY_EDITOR
        // �����Ϳ��� LoadAllPrefabs()�� ä�� ��ųʸ� �켱
        if (_editorPrefabs != null && _editorPrefabs.Count > 0)
            return new List<string>(_editorPrefabs.Keys).ToArray();
#endif
        // ��Ÿ��(Addressables �ʱ�ȭ ��) ��ųʸ�
        if (_addrKeyByName != null && _addrKeyByName.Count > 0)
            return new List<string>(_addrKeyByName.Keys).ToArray();

        return System.Array.Empty<string>();
    }
}
