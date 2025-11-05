using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_God : UI_Popup
{


    #region Enum
    enum GameObjects
    {
        GodOffText,
        GodOnText
    }
    enum Buttons
    {
        GodModeButton,
        LobbyButton
    }
    enum Texts
    {

    }

    enum Images
    {

    }
    #endregion

    public override bool Init()
    {
        if (base.Init() == false)
            return false;

        #region Object Bind

        BindObject(typeof(GameObjects));
        BindButton(typeof(Buttons));
        BindText(typeof(Texts));
        BindImage(typeof(Images));

        #endregion
        GetButton((int)Buttons.GodModeButton).gameObject.BindEvent(ToggleText);
        GetButton((int)Buttons.LobbyButton).gameObject.BindEvent(OnClickStartButton);


        GetObject((int)GameObjects.GodOnText).SetActive(false);

        return true;
    }

    private void OnClickStartButton()
    {
        Managers.Scene.LoadScene(Define.EScene.LobbyScene);
    }

    private void Awake()
    {
        Init();
    }

    public void ToggleText()
    {
        Managers.Object.Player.GetComponent<PlayerController>()._isHack = !Managers.Object.Player.GetComponent<PlayerController>()._isHack;

        if (GetObject((int)GameObjects.GodOffText).activeSelf)
        {
            GetObject((int)GameObjects.GodOffText).SetActive(false);
            GetObject((int)GameObjects.GodOnText).SetActive(true);
        }
        else
        {
            GetObject((int)GameObjects.GodOffText).SetActive(true);
            GetObject((int)GameObjects.GodOnText).SetActive(false);
        }
    }
}
