
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Define;
using static System.Net.Mime.MediaTypeNames;
using Image = UnityEngine.UI.Image;

public class UI_GameScene : UI_Scene
{

    UI_GameOver _gameOverPopupUI;

    #region Enum
    enum GameObjects
    {

    }
    enum Buttons
    {
        BlueButton,
        RedButton   
    }
    enum Texts
    {
        CoinText
    }

    enum Images
    {

    }
    #endregion


    void OnEnable()
    {
        Managers.Game.OnCoinChanged += UpdateCoinText;
    }

    public void UpdateCoinText(int coin)
    {
        GetText((int)Texts.CoinText).text = coin.ToString();
    }

    void OnDisable()
    {
        Managers.Game.OnCoinChanged -= UpdateCoinText;
    }
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



        GetButton((int)Buttons.BlueButton).onClick.AddListener(() =>
        {
            FindObjectOfType<Movement>().OnPressColor(EZoneColor.Blue);
        });

        GetButton((int)Buttons.RedButton).onClick.AddListener(() =>
        {
            FindObjectOfType<Movement>().OnPressColor(EZoneColor.Red);
        });


        return true;
    }

    private void Awake()
    {
        Init();
    }



}
