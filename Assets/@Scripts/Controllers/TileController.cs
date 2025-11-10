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
    [Range(0, 255)]  int maxAlpha = 255;  //  0.784

    [Min(1)] public int beatsPerCycle = 1;

    Tweener _tw;

    [SerializeField] bool onlyWhenVisible = true;       // 화면에 보일 때만 깜빡


    void OnEnable()
    {
        //Managers.Game.OnCoinChanged += BlinkOnce;
    }
    void OnDisable() => _tw?.Kill();
    bool IsVisible()
    {
        // SpriteRenderer가 카메라 프러스텀 안에 한 번이라도 렌더링되면 true
        return _beatSprite && _beatSprite.isVisible;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            BlinkOnce(0);
        }
    }

    public float rise = 0.04f;   // 아주 빠르게 켜짐
    public float hold = 0.02f;   // 짧게 유지
    public float fall = 0.5f;   // 서서히 사라짐

    public void BlinkOnce(int _)
    {
        if (_beatSprite == null) return;
        if (onlyWhenVisible && !IsVisible()) return;

        // 원래 색상(알파 포함)
        var baseCol = _beatSprite.color;

        // 시작 시 완전 투명
        var startCol = new Color(baseCol.r, baseCol.g, baseCol.b, 0f);
        _beatSprite.color = startCol;

        // 목표 알파값 (maxAlpha는 0~255)
        float aPeak = Mathf.Clamp01(maxAlpha / 255f);

        // 이전 트윈 중복 방지
        _beatSprite.DOKill();

        // 0 → aPeak → 0 (알파만 조절)
        var seq = DOTween.Sequence().SetUpdate(true);

        // 등장 (페이드 인)
        seq.Append(_beatSprite
            .DOFade(aPeak, Mathf.Max(0.001f, rise))
            .SetEase(Ease.OutExpo));

        // 잠시 유지
        seq.AppendInterval(Mathf.Max(0f, hold));

        // 사라짐 (페이드 아웃)
        seq.Append(_beatSprite
            .DOFade(0f, Mathf.Max(0.001f, fall))
            .SetEase(Ease.InExpo));
    }



}
