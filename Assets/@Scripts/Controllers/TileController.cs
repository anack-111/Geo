using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class TileController : BaseController
{
    public ETileType TileType;
    public SpriteRenderer _beatSprite;

    [Range(30, 300)]  float bpm = 120f;   // �д� ����
    [Range(0, 255)]  int minAlpha = 100;  //  0.392
    [Range(0, 255)]  int maxAlpha = 120;  //  0.784

    [Min(1)] public int beatsPerCycle = 1;

    Tweener _tw;
    [SerializeField, Min(0f)] float rise = 0.03f;      // �ö󰡴� �ð�(��)
    [SerializeField, Min(0f)] float fall = 0.07f;      // �������� �ð�(��)
    [SerializeField] bool onlyWhenVisible = true;       // ȭ�鿡 ���� ���� ����


    void OnEnable()
    {
        Managers.Game.OnCoinChanged += BlinkOnce;
    }
    void OnDisable() => _tw?.Kill();
    bool IsVisible()
    {
        // SpriteRenderer�� ī�޶� �������� �ȿ� �� ���̶� �������Ǹ� true
        return _beatSprite && _beatSprite.isVisible;
    }
    public void BlinkOnce(int _)
    {
        if (_beatSprite == null)
            return;

        if (onlyWhenVisible && !IsVisible())
            return;

        // ����/�� ���� = 0
        var c = _beatSprite.color;
        c.a = 0f;
        _beatSprite.color = c;

        float aPeak = Mathf.Clamp01(maxAlpha / 255f);

        // ���� Ʈ�� �ߺ� ����
        _beatSprite.DOKill();

        // 0 -> aPeak -> 0, ������ �� ��
        _beatSprite
            .DOFade(aPeak, Mathf.Max(0.001f, rise))
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
            {
                _beatSprite
                    .DOFade(0f, Mathf.Max(0.001f, fall))
                    .SetEase(Ease.InQuad)
                    .SetUpdate(true);
            })
            .SetUpdate(true); // Ÿ�ӽ����� �����Ϸ��� true ����
    }


}
