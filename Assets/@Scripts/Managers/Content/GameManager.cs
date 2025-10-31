using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    //�ϴ� �̷���
    public string _musicName;

    public bool _isPlay = true;

    public Action<int> OnCoinChanged;

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
