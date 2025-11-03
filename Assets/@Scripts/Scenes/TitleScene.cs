using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static Define;

public class TitleScene : BaseScene
{
    protected override void Init()
    {
        base.Init();
        Application.targetFrameRate = 60;
        SceneType = Define.EScene.TitleScene;
    }

    public override void Clear() { }

}
