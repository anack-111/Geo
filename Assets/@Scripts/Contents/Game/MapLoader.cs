using System.Globalization;
using UnityEngine;

public class MapLoader : MonoBehaviour
{
    [Header("Map Data")]
    public TextAsset mapCsvFile;

    [Header("References")]
    public Transform mapParent;
    public float playerSpeed = 10.4f;

    [Header("Managers")]
    public PrefabManager prefabManager; // ScriptableObject ����

    void Start()
    {
        if (mapCsvFile == null)
        {
            Debug.LogError(" Map CSV ������ �������� �ʾҽ��ϴ�!");
            return;
        }

        if (prefabManager == null)
        {
            Debug.LogError(" PrefabManager(ScriptableObject)�� �������� �ʾҽ��ϴ�!");
            return;
        }

        LoadMapCSV(mapCsvFile);
    }

    void LoadMapCSV(TextAsset csv)
    {
        string[] lines = csv.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] data = lines[i].Trim().Split(',');
            if (data.Length < 5) continue;

            string type = data[0];
            float x = float.Parse(data[1], CultureInfo.InvariantCulture);
            float y = float.Parse(data[2], CultureInfo.InvariantCulture);
            float z = float.Parse(data[3], CultureInfo.InvariantCulture);
            float rot = float.Parse(data[4], CultureInfo.InvariantCulture);

            Vector3 pos = new Vector3(x, y, z);
            Quaternion rotQ = Quaternion.Euler(0, 0, rot);

            // ScriptableObject PrefabManager���� prefab ã��
            GameObject prefab = prefabManager.GetPrefabByType(type);
            if (prefab == null)
            {
                Debug.LogWarning($" '{type}' �������� ã�� �� �����ϴ�. ({pos})");
                continue;
            }

            GameObject obj = Instantiate(prefab, pos, rotQ, mapParent);
            obj.name = type;
        }

        Debug.Log($" �� �ε� �Ϸ� ({lines.Length - 1}�� ������Ʈ)");
    }
}
