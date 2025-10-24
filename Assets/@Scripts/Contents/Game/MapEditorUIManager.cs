using System.Collections.Generic;
using System.Globalization;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapEditorUIManager : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource audioSource;
    public Slider timeSlider;
    public Button startButton;
    public Button saveButton;
    public TextMeshProUGUI timeText;

    [Header("References")]
    public Transform player;
    public Transform mapParent;

    [Header("Data Files")]
    public TextAsset midiCsvFile;
    public TextAsset mapCsvFile;

    public string musicMapName = "DefaultMap";

    private bool isPlaying = true;
    public float playerSpeed = 10.4f;

    [SerializeField] private PrefabManager prefabManager;
    [SerializeField] public GameObject notePrefab;

    private List<NoteMarker> noteMarkers = new();
    private List<MapObject> mapObjects = new();

    [HideInInspector] public float selectedNoteX = 0f;
    private void Awake()
    {
#if UNITY_EDITOR
        prefabManager = AssetDatabase.LoadAssetAtPath<PrefabManager>("Assets/Editor/PrefabManager.asset");
        if (prefabManager == null)
        {
            Debug.LogError("PrefabManager.asset�� ã�� �� �����ϴ�! ��θ� Ȯ���ϼ���.");
        }
        else
        {
            prefabManager.LoadAllPrefabs();
        }
#else
        Debug.LogWarning("PrefabManager�� ������ ȯ�濡���� �ε�˴ϴ�.");
#endif
    }

    void Start()
    {
        startButton.onClick.AddListener(TogglePlay);
        saveButton.onClick.AddListener(SaveMapCSV);
        timeSlider.onValueChanged.AddListener(OnSliderChanged);

        if (midiCsvFile != null)
            LoadMusicCSV();

        if (mapCsvFile != null)
            LoadMapCSV(mapCsvFile);
    }

    void Update()
    {
        // ���� �ð� ǥ��
        if (audioSource != null && audioSource.clip != null && timeText != null)
        {
            float currentTime = audioSource.time;
            float totalTime = audioSource.clip.length;
            timeText.text = $"{Extension.FormatTime(currentTime)} / {Extension.FormatTime(totalTime)}";
        }

        if (isPlaying)
        {
            timeSlider.value = audioSource.time / audioSource.clip.length;
            UpdateCamera();
        }

        // Ctrl + S ����
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
            SaveMapCSV();
    }

    void FixedUpdate()
    {
        if (isPlaying)
            player.position = Vector3.right * (audioSource.time * playerSpeed);
    }

    // ---------------------------
    // ��� / ����
    // ---------------------------
    void TogglePlay()
    {
        if (isPlaying)
            audioSource.Pause();
        else
        {
            audioSource.time = player.position.x / playerSpeed;
            audioSource.Play();
        }

        isPlaying = !isPlaying;
    }

    void OnSliderChanged(float value)
    {
        audioSource.time = value * audioSource.clip.length;
        player.position = new Vector3(audioSource.time * playerSpeed, 0, 0);
        UpdateCamera();
    }

    void UpdateCamera()
    {
        Camera.main.transform.position = new Vector3(player.position.x, 0, -10);
    }

    // ---------------------------
    // MIDI CSV �ε�
    // ---------------------------
    void LoadMusicCSV()
    {
        noteMarkers.Clear();
        string[] lines = midiCsvFile.text.Split('\n');

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i])) continue;
            string[] data = lines[i].Trim().Split(',');

            if (!float.TryParse(data[6], NumberStyles.Float, CultureInfo.InvariantCulture, out float timeVal))
                continue;

            NoteMarker note = new NoteMarker { time = timeVal };
            noteMarkers.Add(note);
            CreateNoteMarker(note);
        }

        Debug.Log($"Loaded {noteMarkers.Count} MIDI note markers.");
    }

    void CreateNoteMarker(NoteMarker note)
    {
        Transform notesParent = mapParent.Find("NoteMarkers");
        if (notesParent == null)
        {
            GameObject noteGroup = new GameObject("NoteMarkers");
            noteGroup.transform.SetParent(mapParent);
            notesParent = noteGroup.transform;
        }

        float xPos = note.time * playerSpeed;
        Vector3 spawnPos = new Vector3(xPos, 0f, 0f);
        GameObject marker = Instantiate(notePrefab, spawnPos, Quaternion.identity, notesParent);

        marker.transform.localScale = Vector3.one * 0.2f;
        marker.name = $"Note_{note.time:F2}";

        if (marker.GetComponent<Collider2D>() == null)
            marker.AddComponent<BoxCollider2D>();
        if (marker.GetComponent<NoteSelectable>() == null)
            marker.AddComponent<NoteSelectable>();
    }

    // ---------------------------
    // �� ���� (������ ��Ʈ ����)
    // ---------------------------
    public void SaveMapCSV()
    {
        string folderPath = Application.dataPath + "/Data/MapData";
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        string path = $"{folderPath}/{musicMapName}_Map.csv";
        using (StreamWriter writer = new StreamWriter(path))
        {
            writer.WriteLine("type,x,y,z,rotation");

            foreach (Transform obj in mapParent)
            {
                if (obj.name == "NoteMarkers")
                    continue;

                //  �θ� ������ ������ ���� (�ڽ� ����)
                string prefabName = obj.name.Replace("(Clone)", "").Trim();
                writer.WriteLine(
                    $"{prefabName}," +
                    $"{obj.position.x.ToString(CultureInfo.InvariantCulture)}," +
                    $"{obj.position.y.ToString(CultureInfo.InvariantCulture)}," +
                    $"{obj.position.z.ToString(CultureInfo.InvariantCulture)}," +
                    $"{obj.eulerAngles.z.ToString(CultureInfo.InvariantCulture)}"
                );
            }
        }

        Debug.Log($" �� ���� �Ϸ�: {path}");
    }

    // ---------------------------
    // �� �ε� (������ �״�� ����)
    // ---------------------------
    public void LoadMapCSV(TextAsset csv)
    {
        if (csv == null) return;
        mapObjects.Clear();

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

            GameObject prefab = prefabManager.GetPrefabByType(type);
            if (prefab == null)
            {
                Debug.LogWarning($"[Load] '{type}' �������� ã�� �� �����ϴ�.");
                continue;
            }

            GameObject obj = Instantiate(prefab, new Vector3(x, y, z), Quaternion.Euler(0, 0, rot), mapParent);
            obj.name = type;
        }

        Debug.Log(" �� �ҷ����� �Ϸ�");
    }
}

// ------------------------------------
// ������ ����
// ------------------------------------
[System.Serializable]
public class NoteMarker { public float time; }

[System.Serializable]
public class MapObject
{
    public string type;
    public Vector3 position;
    public float rotation;
}
