using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameManager
{
    //일단 이렇게
    public string _musicName;
    

    public bool _isPlay = true;

    public Action<int> OnCoinChanged;
    public Action<EGameMode> OnModeChanged;
    // 내부 필드 + 프로퍼티
    private int _coin = 0;
    public int Coin
    {
        get => _coin;
        private set
        {
            if (_coin == value)
                return;     // 동일값이면 무시(옵션)
            _coin = value;
            OnCoinChanged?.Invoke(_coin);   // 구독자들에게 알림
        }
    }

    public SpriteRenderer _globalSprite;
    public void GameOver()
    {
        _isPlay = false;
    }

    public void CoinAdd(int value)
    {
        Coin += value;
    }

    public void Init()
    {
        _mainCam = Camera.main;
        OnModeChanged += HandleCameraZoom;
        _isPlay = true;
        Coin = 0;
    }

    Sequence seq;
    const float A100 = 100f / 255f;
    void SetA(float a)
    {
        var c = _globalSprite.color; c.a = a; _globalSprite.color = c;
    }
    public void Flash(float rise = 0.1f, float fall = 0.1f)
    {
        if (!_globalSprite)
            return;

        seq?.Kill();
        seq = DOTween.Sequence()
            .Append(DOTween.To(() => _globalSprite.color.a, a => SetA(a), A100, rise))
            .Append(DOTween.To(() => _globalSprite.color.a, a => SetA(a), 0f, fall))
            .SetLink(_globalSprite.gameObject, LinkBehaviour.KillOnDisable);
    }



    Camera _mainCam;
    Sequence _camSeq;

    void HandleCameraZoom(EGameMode mode)
    {
        if (_mainCam == null)
            return;

        float targetSize = Define.SCREEN_HEIGHT_VALUES[(int)mode] > 10 ? 5f : 6.5f;

        if (Mathf.Abs(_mainCam.orthographicSize - targetSize) < 0.01f)
            return;

        _mainCam.GetComponent<FollowPlayer>().CallShockWave();

        _camSeq?.Kill();
        _camSeq = DOTween.Sequence()
            .Append(_mainCam.DOOrthoSize(targetSize, 0.4f).SetEase(Ease.OutQuad));
    }

    
   


}
