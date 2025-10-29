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

    private int rows = 5;
    private int cols = 5;
    private float spacing = 1f;

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

        rows = EditorGUILayout.IntField("Rows", rows);
        cols = EditorGUILayout.IntField("Cols", cols);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);

        GUILayout.Space(10);
        using (new EditorGUI.DisabledScope(prefabNames == null || prefabNames.Length == 0))
        {
            if (GUILayout.Button("Generate Grid (Selected Note X)"))
                GenerateGrid();
        }

        if (GUILayout.Button("Clear Map Objects"))
            ClearGenerated();
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

        // 부모 그룹 생성
        float baseX = editor.selectedNoteX;
        GameObject gridParent = new GameObject($"{prefabKey}");
        Undo.RegisterCreatedObjectUndo(gridParent, "Create Grid Parent");
        gridParent.transform.SetParent(editor.mapParent);
        gridParent.transform.position = new Vector3(baseX, -2.3f, 0);
        // (원하면 그룹에도 태그 부여) gridParent.tag = "Editable";

        // 격자 생성
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * spacing, y * spacing, 0);
                var instObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
                Undo.RegisterCreatedObjectUndo(instObj, "Create Grid Item");

                instObj.transform.SetParent(gridParent.transform);
                instObj.transform.localPosition = pos;
                // 태그가 없으면 에러 나니, 미리 프로젝트에 "Editable" 태그를 만들어 두세요.
                instObj.tag = "Editable";

                // TileController 자동 추가
                var tile = instObj.GetComponent<TileController>();
                if (tile == null) tile = instObj.AddComponent<TileController>();
                // Enum 이름이 프리팹 이름과 정확히 같지 않으면 예외 → 방어 코드
                try { tile.TileType = (ETileType)System.Enum.Parse(typeof(ETileType), prefabKey); }
                catch { /* 무시 또는 매핑 테이블 사용 */ }
            }
        }

        Debug.Log($" {prefabKey} {rows * cols}개 생성 완료 (기준 X={baseX:F2})");
    }

    void ClearGenerated()
    {
        var editor = Object.FindObjectOfType<MapEditorUIManager>();
        if (editor == null || editor.mapParent == null)
        {
            Debug.LogWarning(" mapParent를 찾을 수 없습니다.");
            return;
        }

        // 그룹까지 같이 지우고 싶으면 CompareTag 대신 이름/부모 기준으로 지우거나,
        // gridParent에도 "Editable" 태그를 달아 두세요.
        for (int i = editor.mapParent.childCount - 1; i >= 0; i--)
        {
            Transform child = editor.mapParent.GetChild(i);
            if (child.CompareTag("Editable"))
                Object.DestroyImmediate(child.gameObject);
        }

        Debug.Log(" 기존 타일 제거 완료.");
    }
}
#endif
