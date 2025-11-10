using DG.Tweening;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightBeam : MonoBehaviour
{
   

    void Awake()
    {
 
        if (_spotLight2D != null)
            _spotLight2D.intensity = baseIntensity;
    }


    public Light2D _spotLight2D;

    [Header("Flash")]
    private float baseIntensity = 0f;   // 평소 밝기
    private float peakIntensity = 3.0f;   // 번쩍 피크
    private float rise = 0.3f;           // 켜지는 시간
    private float fall = 0.6f;           // 꺼지는 시간
    private bool ignoreTimeScale = true;  // 일시정지에도 번쩍

    Tween _tw;


    // 이벤트에서 이 함수만 호출하면 끝
    public void FlashOnce()
    {
        if (_spotLight2D == null)
            return;

        _tw?.Kill(); // 중복 방지
        _spotLight2D.intensity = baseIntensity;

        var seq = DOTween.Sequence();
        if (ignoreTimeScale) seq.SetUpdate(true);

        seq.Append(
                   DOTween.To(
                       () => _spotLight2D.intensity,
                       v => _spotLight2D.intensity = v,
                       peakIntensity,
                       Mathf.Max(0.001f, rise)
                   ).SetEase(Ease.OutExpo)
               );

        // 피크 → 기본
        seq.Append(
            DOTween.To(
                () => _spotLight2D.intensity,
                v => _spotLight2D.intensity = v,
                baseIntensity,
                Mathf.Max(0.001f, fall)
            ).SetEase(Ease.InExpo)
        );


        _tw = seq;
    }
}
