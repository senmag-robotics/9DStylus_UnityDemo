using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    // this script controls the logic of the canvas
    public GameObject wheelUI;
    public GameObject colorPickerUI;
    public GameObject paletteMenu;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            wheelUI.SetActive(!wheelUI.activeSelf);
            if (paletteMenu.activeSelf)
            {
                paletteMenu.SetActive(false);
            }
        }
    }
}
