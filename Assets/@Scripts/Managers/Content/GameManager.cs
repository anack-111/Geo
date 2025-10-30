using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager
{
    //일단 이렇게
    public string _musicName;

    public bool _isPlay = true;
     int Coin { get; set; } = 0;    

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
