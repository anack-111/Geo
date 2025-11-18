using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_LobbyScene : UI_Scene
{
    #region Enum

    enum GameObjects
    {
       
    }
    enum Buttons
    {
        DuDuButton,
        RudyButton,
        TasteButton,
        WouldYouButton,
        TickButton
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


        GetButton((int)Buttons.DuDuButton).gameObject.BindEvent(() => OnClickStartButton("DuDuDu"));
        GetButton((int)Buttons.RudyButton).gameObject.BindEvent(() => OnClickStartButton("RudyPop"));
        GetButton((int)Buttons.TasteButton).gameObject.BindEvent(() => OnClickStartButton("마시따"));
        GetButton((int)Buttons.WouldYouButton).gameObject.BindEvent(() => OnClickStartButton("WouldYou"));
        GetButton((int)Buttons.TickButton).gameObject.BindEvent(() => OnClickStartButton("Tick"));
        return true;
    }

    //private void Awake()
    //{
    //    Init();
    //}

    void OnClickStartButton(string id)
    {
        Managers.Game._musicName = id;
        Managers.Scene.LoadScene(Define.EScene.GameScene);
      
    }


}
