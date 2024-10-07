using SenmagHaptic;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class WeighingScale : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject frontFace;
    public GameObject desiredObject;

    public GameObject otherScale1;
    public GameObject otherScale2;

    public int state = 0;
    bool solved = false;
    DateTime solvedTimer;
    void Start()
    {
        frontFace.GetComponent<Renderer>().material.color = Color.yellow;
    }

    // Update is called once per frame
    void Update()
    {
        
        if(state == 1) { 
            if(otherScale1 != null)
            {
                if(otherScale1.GetComponentInChildren<WeighingScale>().state == 1)
                {
                    if (otherScale2 != null)
                    {
                        if (otherScale2.GetComponentInChildren<WeighingScale>().state == 1)
                        {
                            
                            if (solved == false) solvedTimer = System.DateTime.Now;
                            solved = true;
                        }
                        else solved = false;
                    }
                }
                else solved = false;
            }
        }
        else solved = false;

        if(solved == true && (System.DateTime.Now - solvedTimer).Seconds > 2)
        {
            GameObject.Find("SenmagWorkspace").GetComponentInChildren<AWE_Demo>().advanceState();
        }
    }

    private void OnCollisionStay(Collision other)
    {
        if (other.gameObject.GetComponentInChildren<Senmag_HapticCursor>() == null) {
            if (other.gameObject == desiredObject)
            {
                state = 1;
                frontFace.GetComponent<Renderer>().material.color = Color.green;
            }
            else
            {
                frontFace.GetComponent<Renderer>().material.color = Color.red;
                state = 2;
            }
        }
    }
    private void OnCollisionExit(Collision other)
    {
        if (other.gameObject.GetComponentInChildren<Senmag_HapticCursor>() == null)
        {
            state = 0;
            frontFace.GetComponent<Renderer>().material.color = Color.yellow;
        }
    }
}
