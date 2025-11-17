using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClearController : BaseController
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Managers.Object.Player._isHack = true;
            Managers.UI.ShowPopupUI<UI_GameOver>();
        }
    }
}
