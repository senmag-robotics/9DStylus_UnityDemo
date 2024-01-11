using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PalettePicker : MonoBehaviour
{
    private GameObject[] palettes;
    public GameObject chosenPalette;
    // Start is called before the first frame update
    void Start()
    {
        palettes = GameObject.FindGameObjectsWithTag("Palette");
    }

    // Update is called once per frame
    private void OnCollisionEnter(Collision collision)
    {
        foreach (GameObject palette in palettes)
        {
            palette.SetActive(false);
        }
        chosenPalette.SetActive(true);
    }
}