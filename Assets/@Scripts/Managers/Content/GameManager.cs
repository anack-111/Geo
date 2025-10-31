using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Define;

public class GameManager
{
    //�ϴ� �̷���
    public string _musicName;

    public bool _isPlay = true;

    public Action<int> OnCoinChanged;
    public Action<EGameMode> OnModeChanged;
    // ���� �ʵ� + ������Ƽ
    private int _coin = 0;
    public int Coin
    {
        get => _coin;
        private set
        {
            if (_coin == value)
                return;     // ���ϰ��̸� ����(�ɼ�)
            _coin = value;
            OnCoinChanged?.Invoke(_coin);   // �����ڵ鿡�� �˸�
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

}
