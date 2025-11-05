using DG.Tweening;
using UnityEngine;

public class BeatController : MonoBehaviour
{
    [Header("Settings")]
    public Transform target;           // 안 넣으면 자기 자신
    public float scaleUp = 1.12f;      // 얼마나 커질지
    public float upTime = 0.06f;       // 커지는 시간
    public float downTime = 0.12f;     // 작아지는 시간

    Vector3 _base;
    Tween _t;

    void Awake()
    {
        if (target == null) target = transform;
        _base = target.localScale;

      
    }

    private void OnEnable()
    {
        Managers.Game.OnCoinChanged += Pulse;
    }


    private void OnDisable()
    {
        Managers.Game.OnCoinChanged -= Pulse;
    }

    // 외부에서 이벤트 오면 이 함수만 호출하면 됨
    public void Pulse(int _)
    {
        _t?.Kill();                     // 겹치는 트윈 정리
        target.localScale = _base;      // 스케일 리셋

        _t = target
            .DOScale(_base * scaleUp, upTime)
            .SetEase(Ease.OutQuad)
            .OnComplete(() =>
                target.DOScale(_base, downTime).SetEase(Ease.InOutQuad)
            );
    }
}
