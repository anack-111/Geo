using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TextCore.Text;
using static Define;

/*
 * 자주쓰이는 범용적인 함수들 
 */

public static class Util
{

    public static T GetOrAddComponent<T>(GameObject go) where T : UnityEngine.Component
    {
        T component = go.GetComponent<T>();
        if (component == null)
            component = go.AddComponent<T>();
        return component;
    }


    public static GameObject FindChild(GameObject go, string name = null, bool recursive = false)
    {
        Transform transform = FindChild<Transform>(go, name, recursive);
        if (transform == null)
            return null;

        return transform.gameObject;
    }

    public static T FindChild<T>(GameObject go, string name = null, bool recursive = false) where T : UnityEngine.Object
    {
        if (go == null)
            return null;

        if (recursive == false)
        {
            for (int i = 0; i < go.transform.childCount; i++)
            {
                Transform transform = go.transform.GetChild(i);
                if (string.IsNullOrEmpty(name) || transform.name == name)
                {
                    T component = transform.GetComponent<T>();
                    if (component != null)
                        return component;
                }
            }
        }
        else
        {
            foreach (T component in go.GetComponentsInChildren<T>())
            {
                if (string.IsNullOrEmpty(name) || component.name == name)
                    return component;
            }
        }

        return null;
    }


    public static Color HexToColor(string color)
    {
        Color parsedColor;
        ColorUtility.TryParseHtmlString("#" + color, out parsedColor);

        return parsedColor;
    }
    public static void LimitYVelocity(float limit, Rigidbody2D rb)
    {
        int gravityMultiplier = (int)(Mathf.Abs(rb.gravityScale) / rb.gravityScale);
        if (rb.velocity.y * -gravityMultiplier > limit)
            rb.velocity = Vector2.up * -limit * gravityMultiplier;
    }

    public static void CreateGamemode(Rigidbody2D rb, Movement host, bool onGroundRequired, float initalVelocity,float gravityScale, bool canHold = false,
        bool flipOnClick = false, float rotationMod = 0, float yVelocityLimit = Mathf.Infinity)
    {
        bool inputHeld = host._inputHeld;
        bool inputPressed = host._inputPressed;
        bool inputReleased = host._inputReleased;

        if (!inputHeld || (canHold && host.OnGround()))
            host._isClickProcessed = false;

        rb.gravityScale = gravityScale * host._gravity;
        LimitYVelocity(yVelocityLimit, rb);

        if (inputHeld)
        {
            if ((host.OnGround() && !host._isClickProcessed) || (!onGroundRequired && !host._isClickProcessed))
            {
                host._isClickProcessed = true;
                rb.velocity = Vector2.up * initalVelocity * host._gravity;
                host._gravity *= flipOnClick ? -1 : 1;

                // Cube 점프 이펙트
                if (host._currentGameMode == EGameMode.Cube && host._jumpEffect != null)
                    host._jumpEffect.Play();
            }
        }

        if (host.OnGround() || !onGroundRequired)
            
                 host._sprite.rotation = Quaternion.Euler(0, 0, 0);

    }

    public static bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Input.mousePresent && EventSystem.current.IsPointerOverGameObject())
            return true;

        if (Input.touchCount > 0)
            for (int i = 0; i < Input.touchCount; i++)
                if (EventSystem.current.IsPointerOverGameObject(Input.GetTouch(i).fingerId))
                    return true;

        return false;
    }





}
