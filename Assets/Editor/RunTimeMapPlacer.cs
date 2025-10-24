using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Define;

public class RunTimeMapPlacer : EditorWindow
{
    private PrefabManager prefabManagerAsset;   // Prefab �ڵ� �ε� �Ŵ���
    private string[] prefabNames;          // ��Ӵٿ�� �̸� ���
    private int selectedPrefabIndex = 0;   // ���� ���õ� �ε���

    private int rows = 5;
    private int cols = 5;
    private float spacing = 1f;

    [MenuItem("Tools/RunTimeMapPlacer")]
    static void Init()
    {
        GetWindow<RunTimeMapPlacer>("RunTimeMapPlacer");
    }

    void OnEnable()
    {
        if (prefabManagerAsset == null)
            prefabManagerAsset = AssetDatabase.LoadAssetAtPath<PrefabManager>("Assets/Editor/PrefabManager.asset");

        prefabManagerAsset.LoadAllPrefabs();
        prefabNames = prefabManagerAsset.GetPrefabNames();
    }

    void OnGUI()
    {
        if (prefabManagerAsset == null)
        {
            EditorGUILayout.HelpBox("PrefabManager�� ���� �����ؾ� �մϴ�!", MessageType.Warning);
            return;
        }

        EditorGUILayout.LabelField("�ڵ� ������ ��ġ��", EditorStyles.boldLabel);

        //  �������� �ڵ����� �ε�� ������ ��Ӵٿ�
        selectedPrefabIndex = EditorGUILayout.Popup("Prefab Type", selectedPrefabIndex, prefabNames);

        rows = EditorGUILayout.IntField("Rows", rows);
        cols = EditorGUILayout.IntField("Cols", cols);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);

        GUILayout.Space(10);
        if (GUILayout.Button("Generate Grid (Selected Note X)"))
            GenerateGrid();

        if (GUILayout.Button("Clear Map Objects"))
            ClearGenerated();
    }

    void GenerateGrid()
    {
        var editor = FindObjectOfType<MapEditorUIManager>();
        if (editor == null || editor.mapParent == null)
        {
            Debug.LogError(" MapEditorUIManager �Ǵ� mapParent�� �����ϴ�!");
            return;
        }

        string prefabKey = prefabNames[selectedPrefabIndex];
        GameObject prefab = prefabManagerAsset.GetPrefabByType(prefabKey);
        if (prefab == null)
        {
            Debug.LogError($" '{prefabKey}' �������� ã�� �� �����ϴ�!");
            return;
        }

        // �θ� �׷� ����
        float baseX = editor.selectedNoteX;
        GameObject gridParent = new GameObject($"{prefabKey}");
        gridParent.transform.SetParent(editor.mapParent);
        gridParent.transform.position = new Vector3(baseX, 0, 0);

        // ���� ����
        for (int y = 0; y < rows; y++)
        {
            for (int x = 0; x < cols; x++)
            {
                Vector3 pos = new Vector3(x * spacing, y * spacing, 0);
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, SceneManager.GetActiveScene());
                instance.transform.SetParent(gridParent.transform);
                instance.transform.localPosition = pos;
                instance.tag = "Editable";

                // TileController �ڵ� �߰�
                TileController tile = instance.GetComponent<TileController>();
                if (tile == null) tile = instance.AddComponent<TileController>();
                tile.TileType = (ETileType)System.Enum.Parse(typeof(ETileType), prefabKey);
            }
        }

        Debug.Log($" {prefabKey} {rows * cols}�� ���� �Ϸ� (���� X={baseX:F2})");
    }

    void ClearGenerated()
    {
        var editor = FindObjectOfType<MapEditorUIManager>();
        if (editor == null || editor.mapParent == null)
        {
            Debug.LogWarning(" mapParent�� ã�� �� �����ϴ�.");
            return;
        }

        for (int i = editor.mapParent.childCount - 1; i >= 0; i--)
        {
            Transform child = editor.mapParent.GetChild(i);
            if (child.CompareTag("Editable"))
                DestroyImmediate(child.gameObject);
        }

        Debug.Log(" ���� Ÿ�� ���� �Ϸ�.");
    }
}
