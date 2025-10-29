#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class RunTimeMapPlacer : EditorWindow
{
    private PrefabManager prefabManagerAsset;   // Prefab �ڵ� �ε� �Ŵ���
    private string[] prefabNames = System.Array.Empty<string>(); // ��Ӵٿ�� �̸� ���
    private int selectedPrefabIndex = 0;        // ���� ���õ� �ε���

    private int rows = 5;
    private int cols = 5;
    private float spacing = 1f;

    [MenuItem("Tools/RunTimeMapPlacer")]
    static void Init() => GetWindow<RunTimeMapPlacer>("RunTimeMapPlacer");

    void OnEnable()
    {
        SceneView.duringSceneGui += OnSceneGUIShortcut;

        // 1) PrefabManager.asset�� Ÿ������ �˻�(��� �־ OK)
        if (prefabManagerAsset == null)
        {
            string[] guids = AssetDatabase.FindAssets("t:PrefabManager");
            if (guids != null && guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                prefabManagerAsset = AssetDatabase.LoadAssetAtPath<PrefabManager>(path);
            }
        }

        // 2) �����Ϳ� ������ ��ĵ (PrefabManager.LoadAllPrefabs�� ������ ����)
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
            EditorGUILayout.HelpBox("PrefabManager.asset�� ã�� �� �����ϴ�. (������Ʈ ���� ����/�����ϼ���)", MessageType.Error);
            if (GUILayout.Button("��˻�")) OnEnable();
            return;
        }

        EditorGUILayout.LabelField("�ڵ� ������ ��ġ��", EditorStyles.boldLabel);

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
            Debug.LogError(" MapEditorUIManager �Ǵ� mapParent�� �����ϴ�!");
            return;
        }
        if (prefabNames == null || prefabNames.Length == 0) return;

        string prefabKey = prefabNames[selectedPrefabIndex];
        GameObject prefab = prefabManagerAsset.GetPrefabByType(prefabKey); // ������ ��ųʸ����� �ٷ� ������
        if (prefab == null)
        {
            Debug.LogError($" '{prefabKey}' �������� ã�� �� �����ϴ�! (PrefabManager.LoadAllPrefabs�� ��ĵ ���� Ȯ��)");
            return;
        }

        // �θ� �׷� ����
        float baseX = editor.selectedNoteX;
        GameObject gridParent = new GameObject($"{prefabKey}");
        Undo.RegisterCreatedObjectUndo(gridParent, "Create Grid Parent");
        gridParent.transform.SetParent(editor.mapParent);
        gridParent.transform.position = new Vector3(baseX, -2.3f, 0);
        // (���ϸ� �׷쿡�� �±� �ο�) gridParent.tag = "Editable";

        // ���� ����
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * spacing, y * spacing, 0);
                var instObj = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
                Undo.RegisterCreatedObjectUndo(instObj, "Create Grid Item");

                instObj.transform.SetParent(gridParent.transform);
                instObj.transform.localPosition = pos;
                // �±װ� ������ ���� ����, �̸� ������Ʈ�� "Editable" �±׸� ����� �μ���.
                instObj.tag = "Editable";

                // TileController �ڵ� �߰�
                var tile = instObj.GetComponent<TileController>();
                if (tile == null) tile = instObj.AddComponent<TileController>();
                // Enum �̸��� ������ �̸��� ��Ȯ�� ���� ������ ���� �� ��� �ڵ�
                try { tile.TileType = (ETileType)System.Enum.Parse(typeof(ETileType), prefabKey); }
                catch { /* ���� �Ǵ� ���� ���̺� ��� */ }
            }
        }

        Debug.Log($" {prefabKey} {rows * cols}�� ���� �Ϸ� (���� X={baseX:F2})");
    }

    void ClearGenerated()
    {
        var editor = Object.FindObjectOfType<MapEditorUIManager>();
        if (editor == null || editor.mapParent == null)
        {
            Debug.LogWarning(" mapParent�� ã�� �� �����ϴ�.");
            return;
        }

        // �׷���� ���� ����� ������ CompareTag ��� �̸�/�θ� �������� ����ų�,
        // gridParent���� "Editable" �±׸� �޾� �μ���.
        for (int i = editor.mapParent.childCount - 1; i >= 0; i--)
        {
            Transform child = editor.mapParent.GetChild(i);
            if (child.CompareTag("Editable"))
                Object.DestroyImmediate(child.gameObject);
        }

        Debug.Log(" ���� Ÿ�� ���� �Ϸ�.");
    }
}
#endif
