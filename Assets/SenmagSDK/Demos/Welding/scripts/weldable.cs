using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weldable : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<weldWire>() != null)
        {
            collision.gameObject.GetComponentInParent<weldWire>().weld();
        }
    }
    private void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.GetComponentInParent<weldWire>() != null)
        {
            collision.gameObject.GetComponentInParent<weldWire>().weld();
        }
    }
}
