using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class weldWire : MonoBehaviour
{
    private MIGTorchControl torchControl;
    public GameObject explosionEffect;

    private bool destroying;
    // Start is called before the first frame update
    void Start()
    {
        torchControl = GetComponentInParent<MIGTorchControl>();
    }
    private void OnTriggerEnter(Collider other)
    {
        if (torchControl.feeding == true)
        {
            if (other.gameObject.GetComponentInParent<weldable>() != null)
            {
                if (torchControl.weldReady <= 0)
                {
                    weld();
                }
            }
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (torchControl.feeding == true)
        {
            if (other.gameObject.GetComponentInParent<weldable>() != null)
            {
                if (torchControl.weldReady <= 0)
                {
                    weld();
                }
            }
        }
    }
    // Update is called once per frame
    void Update()
    {
        if (destroying == true)
        {
            if (GetComponent<ParticleSystem>().isPlaying == false)
            {
                Destroy(gameObject.transform.parent.gameObject);
                Destroy(gameObject);
            }
        }
    }
    public void weld()
    {
        torchControl.weldReady = 5;
        //GetComponent<ParticleSystem>().Play();
        //destroying = true;
        Instantiate(explosionEffect, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject.transform.parent.gameObject);
        Destroy(gameObject);
    }
}
