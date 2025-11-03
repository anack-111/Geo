using DG.Tweening.Core.Easing;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GameScene : BaseScene
{
    public FollowPlayer _playerCamera;


    private void Awake()
    {
        Managers.Sound.Play(Define.ESound.Bgm, Managers.Game._musicName);
        Init();
    }

    private void Start()
    {
        //PlayerController player = Managers.Object.Spawn<PlayerController>(new Vector3(0,-2.3f,0)); 
        //_playerCamera.Init();
        Managers.Game.Init();

    }
    protected override void Init()
    {
        base.Init();
        SceneType = Define.EScene.GameScene;
        UI_GameScene gameScene = Managers.UI.ShowSceneUI<UI_GameScene>();
        gameScene.UpdateCoinText(Managers.Game.Coin);
        Managers.Game._globalSprite = _playerCamera._globalSprite;
    }


    private void Update()
    {

    }

    public override void Clear()
    {

    }
}
