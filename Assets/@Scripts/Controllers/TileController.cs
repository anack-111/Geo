using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class TileController : BaseController
{
    public ETileType TileType;
    public SpriteRenderer _beatSprite;

    [Range(30, 300)]  float bpm = 120f;   // 분당 박자
    [Range(0, 255)]  int minAlpha = 100;  //  0.392
    [Range(0, 255)]  int maxAlpha = 120;  //  0.784

    [Min(1)] public int beatsPerCycle = 1;

    Tweener _tw;
    [SerializeField, Min(0f)] float rise = 0.03f;      // 올라가는 시간(초)
    [SerializeField, Min(0f)] float fall = 0.07f;      // 내려가는 시간(초)
    [SerializeField] bool onlyWhenVisible = true;       // 화면에 보일 때만 깜빡


    void OnEnable()
    {
        Managers.Game.OnCoinChanged += BlinkOnce;
    }
    void OnDisable() => _tw?.Kill();
    bool IsVisible()
    {
        // SpriteRenderer가 카메라 프러스텀 안에 한 번이라도 렌더링되면 true
        return _beatSprite && _beatSprite.isVisible;
    }
    public void BlinkOnce(int _)
    {
        if (_beatSprite == null)
            return;

        if (onlyWhenVisible && !IsVisible())
            return;

        // 시작/끝 알파 = 0
        var c = _beatSprite.color;
        c.a = 0f;
        _beatSprite.color = c;

        float aPeak = Mathf.Clamp01(maxAlpha / 255f);

        // 이전 트윈 중복 방지
        _beatSprite.DOKill();

        // 0 -> aPeak -> 0, 빠르게 한 번
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
            .SetUpdate(true); // 타임스케일 무시하려면 true 유지
    }


}
