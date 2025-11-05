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
        
        Init();
    }

    private IEnumerator Start()
    {
        // 맵 로더가 준비될 때까지 대기
        while (!MapLoader.MapReady)
            yield return null;

        // 이제 플레이 요소와 동시에 시작
        Managers.Sound.Play(Define.ESound.Bgm, Managers.Game._musicName);
        PlayerController player = Managers.Object.Spawn<PlayerController>(new Vector3(0, -2.3f, 0));

        _playerCamera.Init();
        Managers.Game.Init();
    }

    protected override void Init()
    {
        base.Init();
        SceneType = Define.EScene.GameScene;
        UI_GameScene gameScene = Managers.UI.ShowSceneUI<UI_GameScene>();
        gameScene.UpdateCoinText(Managers.Game.Coin);
        Managers.Game._globalSprite = _playerCamera._globalSprite;
        Managers.UI.ShowPopupUI<UI_God>();


    }


    private void Update()
    {

    }

    public override void Clear()
    {
    }
}
