#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class RunTimeMapPlacer : EditorWindow
{
    private PrefabManager prefabManagerAsset;   // Prefab 자동 로드 매니저
    private string[] prefabNames = System.Array.Empty<string>(); // 드롭다운용 이름 목록
    private int selectedPrefabIndex = 0;        // 현재 선택된 인덱스

    [MenuItem("Tools/RunTimeMapPlacer")]
    static void Init() => GetWindow<RunTimeMapPlacer>("RunTimeMapPlacer");

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUIShortcut;

        // 1) PrefabManager.asset을 타입으로 검색(어디 있어도 OK)
        if (prefabManagerAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:PrefabManager");
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                prefabManagerAsset = AssetDatabase.LoadAssetAtPath<PrefabManager>(path);
            }
        }

        // 2) 에디터용 프리팹 스캔 (PrefabManager.LoadAllPrefabs는 에디터 전용)
        if (prefabManagerAsset != null)
        {
            prefabManagerAsset.LoadAllPrefabs();
            prefabNames = prefabManagerAsset.GetPrefabNames();
            if (prefabNames.Length == 0) selectedPrefabIndex = 0;
            else selectedPrefabIndex = Mathf.Clamp(selectedPrefabIndex, 0, prefabNames.Length - 1);
        }
    }

    void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUIShortcut;
    }

    void OnSceneGUIShortcut(SceneView sceneView)
    {
        Event e = Event.current;
        if (e.type == EventType.KeyDown && e.keyCode == KeyCode.A)
        {
            GenerateGrid();
            e.Use();
        }
    }

    void OnGUI()
    {
        if (prefabManagerAsset == null)
        {
            EditorGUILayout.HelpBox("PrefabManager.asset을 찾을 수 없습니다. (프로젝트 내에 생성/유지하세요)", MessageType.Error);
            if (GUILayout.Button("재검색")) OnEnable();
            return;
        }

        EditorGUILayout.LabelField("자동 프리팹 배치기", EditorStyles.boldLabel);

        using (new EditorGUI.DisabledScope(prefabNames == null || prefabNames.Length == 0))
        {
            selectedPrefabIndex = EditorGUILayout.Popup("Prefab Type", selectedPrefabIndex, prefabNames ?? System.Array.Empty<string>());
        }

        GUILayout.Space(10);
        using (new EditorGUI.DisabledScope(prefabNames == null || prefabNames.Length == 0))
        {
            if (GUILayout.Button("Generate Grid (Selected Note X)"))
                GenerateGrid();
        }
    }

    void GenerateGrid()
    {
        var editor = Object.FindObjectOfType<MapEditorUIManager>();
        if (editor == null || editor.mapParent == null)
        {
            Debug.LogError(" MapEditorUIManager 또는 mapParent가 없습니다!");
            return;
        }
        if (prefabNames == null || prefabNames.Length == 0) return;

        string prefabKey = prefabNames[selectedPrefabIndex];
        GameObject prefab = prefabManagerAsset.GetPrefabByType(prefabKey); // 에디터 딕셔너리에서 바로 가져옴
        if (prefab == null)
        {
            Debug.LogError($" '{prefabKey}' 프리팹을 찾을 수 없습니다! (PrefabManager.LoadAllPrefabs의 스캔 폴더 확인)");
            return;
        }

        // pos는 editor.selectedNoteX와 -2.3f로 설정
        Vector3 pos = new Vector3(editor.selectedNoteX, -2.65f, 0); // 원하는 위치에 프리팹 생성
        var instObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
        Undo.RegisterCreatedObjectUndo(instObj, "Create Grid Item");

        instObj.transform.SetParent(editor.mapParent);
        instObj.transform.localPosition = pos;

        // TileController 자동 추가
        var tile = instObj.GetComponent<TileController>();
        if (tile == null) tile = instObj.AddComponent<TileController>();
        // Enum 이름이 프리팹 이름과 정확히 같지 않으면 예외 → 방어 코드
        try { tile.TileType = (ETileType)System.Enum.Parse(typeof(ETileType), prefabKey); }
        catch { /* 무시 또는 매핑 테이블 사용 */ }

        Debug.Log($" {prefabKey} 하나 생성 완료");
    }
}
#endif
