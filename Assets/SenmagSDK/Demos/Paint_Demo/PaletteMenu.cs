using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaletteMenu : MonoBehaviour
{
    public GameObject paletteSelection;
    void Start()
    {
        
    }

    private void OnCollisionEnter(Collision collision)
    {
        paletteSelection.SetActive(true);
    }
}
