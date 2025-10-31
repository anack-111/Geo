using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    //일단 이렇게
    public string _musicName;

    public bool _isPlay = true;

    public Action<int> OnCoinChanged;

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
}
