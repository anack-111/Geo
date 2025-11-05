
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
//using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static Define;

public class ObjectManager
{
    public PlayerController Player { get; private set; }


    public T Spawn<T>(Vector3 position, int templateID = 0, string prefabName = "") where T : BaseController
    {
        System.Type type = typeof(T);

        if (type == typeof(PlayerController))
        {
            GameObject go = Managers.Resource.Instantiate("Player");
            go.transform.position = new Vector3(0, -2.3f, 0);
            PlayerController pc = go.GetOrAddComponent<PlayerController>();

            Player = pc; 

            return pc as T;
        }


        return null;
    }
    //public T Spawn<T>(Vector3 position, string prefabName = "") where T : BaseController
    //{
    //    System.Type type = typeof(T);

    //    if (type == typeof(PlayerController))
    //    {
    //        GameObject go = Managers.Resource.Instantiate("Player");
    //        go.transform.position = new Vector3(0, -2.3f, 0);
    //        PlayerController pc = go.GetOrAddComponent<PlayerController>();


    //        return pc as T;
    //    }


    //    return null;
    //}


    public void Despawn<T>(T obj) where T : BaseController
    {
        System.Type type = typeof(T);

        if (type == typeof(PlayerController))
        {
            // ?
        }

    }


    public void ShowJudgeFont(Vector3 pos, string text)
    {
        string prefabName = "UI_Judge";
        GameObject go = Managers.Resource.Instantiate(prefabName, pooling: true);

        UI_Judge judge = go.GetOrAddComponent<UI_Judge>();
        judge.Show(text, pos);
    }

    public void Clear()
    {

    }

}
