using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEditor.PlayerSettings;
using UnityEngine.SceneManagement;
public class TileGenerator2D : EditorWindow
{
    GameObject tilePrefab;  // ������ Ÿ�� ������
    int rows = 5;           // �� ��
    int cols = 5;           // �� ��
    float spacing = 1f;     // Ÿ�� ����
    [MenuItem("Tools/2D Tile Grid Generator")]
    static void Init()
    {
        GetWindow<TileGenerator2D>("2D Tile Grid Generator");
    }
    private void OnGUI()
    {
        // Ÿ�� ������ ����
        tilePrefab = (GameObject)EditorGUILayout.ObjectField("Tile Prefab", tilePrefab, typeof(GameObject), false);
        // ��, ��, ���� ����
        rows = EditorGUILayout.IntField("Rows", rows);
        cols = EditorGUILayout.IntField("Cols", cols);
        spacing = EditorGUILayout.FloatField("Spacing", spacing);
        // �� ���� ��ư
        if (GUILayout.Button("Generate"))
        {
            GenerateGrid();
        }
    }
    private void GenerateGrid()
    {
        // Ÿ�� �������� �Ҵ���� ���� ��� ��� �޽��� ���
        if (tilePrefab == null)
        {
            Debug.LogWarning("Ÿ�� �������� �Ҵ����ּ���.");
            return;
        }
        // �θ� GameObject ����
        GameObject parent = new GameObject("TileGrid");
        // 2D ���� ����
        for (int y = 0; y < rows; y++)  // ��
        {
            for (int x = 0; x < cols; x++)  // ��
            {
                // ��ġ ��� (2D ��鿡���� X, Y ��ǥ ���)
                Vector3 pos = new Vector3(x * spacing, y * spacing, 0);
                // Ÿ�� �������� ���� �����ϰ� ��ġ ����
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(tilePrefab, SceneManager.GetActiveScene());
                instance.transform.position = pos;
                instance.transform.SetParent(parent.transform);  // �θ� ������Ʈ �Ʒ��� ��ġ
            }
        }
    }
}