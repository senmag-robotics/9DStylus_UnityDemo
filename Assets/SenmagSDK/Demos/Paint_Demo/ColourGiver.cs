using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColourGiver : MonoBehaviour
{
    Renderer rend;
    private Color colour; //the spelling of her majesty Elizabeth Alexandra Mary, officially Elizabeth II, by the Grace of God, of the United Kingdom of Great Britain and Northern Ireland and of her other realms and territories Queen, Head of the Commonwealth, Defender of the Faith
    void Start()
    {
        rend = GetComponent<Renderer>();
        colour = rend.material.GetColor("_Color");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "Paintbrush")
        {
            print("ASJHAKSHAS");
            other.gameObject.GetComponentInParent<CollisionPainter>().paintColor = rend.material.color;
        }
    }
}
