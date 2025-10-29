using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI_God : MonoBehaviour
{
    public GameObject GodOffText;
    public GameObject GodOnText;
    // Start is called before the first frame update

    public void ToggleText()
    {
        if (GodOffText.activeSelf)
        {
            GodOffText.SetActive(false);
            GodOnText.SetActive(true);
        }
        else
        {
            GodOffText.SetActive(true);
            GodOnText.SetActive(false);
        }
    }
}
