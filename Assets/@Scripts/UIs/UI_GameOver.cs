using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_GameOver : UI_Popup
{
    #region Enum

    enum GameObjects
    {

    }
    enum Buttons
    {
        LobbyButton
    }

    enum Texts
    {

    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;
        // 오브젝트 바인딩

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));


        GetButton((int)Buttons.LobbyButton).gameObject.BindEvent(OnClickLobbyButton);


        return true;
    }
    void OnClickLobbyButton()
    {
        Managers.Scene.LoadScene(Define.EScene.LobbyScene);
    }
}
