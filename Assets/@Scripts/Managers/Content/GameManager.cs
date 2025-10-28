using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager 
{
    public bool _isPlay = true;


    public void GameOver()
    {
        _isPlay = false;
    }
}
