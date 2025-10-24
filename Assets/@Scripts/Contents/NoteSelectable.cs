using UnityEngine;

public class NoteSelectable : MonoBehaviour
{
    private void OnMouseDown()
    {
        if (Camera.main == null) return;

        // Ŭ�� ��ġ��
        float selectedX = transform.position.x;

        // ���� ������ ���� ã�Ƽ� ����
        var editor = FindObjectOfType<MapEditorUIManager>();
        if (editor != null)
        {
            editor.selectedNoteX = selectedX;
            Debug.Log($"?? Note selected at X = {selectedX:F2}");
        }
    }
}
