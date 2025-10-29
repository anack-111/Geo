using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager 
{
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
}
