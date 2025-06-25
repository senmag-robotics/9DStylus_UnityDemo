using SenmagHaptic;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;

public class Senmag_gravityWell : MonoBehaviour
{
    public enum ForceType
    {
        linear,
        linearInverted,
        constant,
    }

    public float        radius;
    public ForceType    forceType = ForceType.linearInverted;
    public float        forceGain = 20f;
    public float        maxForce = 10f;
    public float        damping = 50f;
    public float        deadzone = 0.02f;
    public float        deadzoneOverlap = 0.1f;  //softens the inside edge

    private int myCustomForceIndex;
    private Senmag_HapticCursor activeCursor;
    public bool locked;

    private Vector3 posLast;
    private Vector3 dampingVector;
    public float dampingFilter;
    
    // Start is called before the first frame update
    void Start()
    {
        myCustomForceIndex = -1;
        locked = false;
        gameObject.AddComponent<SphereCollider>();
        gameObject.GetComponent<SphereCollider>().radius = radius;
        gameObject.GetComponent<SphereCollider>().isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {

        if (other.GetComponentInParent<Senmag_HapticCursor>() != null)
        {
           // UnityEngine.Debug.Log("Enter");
            //activeCursor = other.GetComponentInParent<Senmag_HapticCursor>();
            myCustomForceIndex = other.GetComponentInParent<Senmag_HapticCursor>().requestCustomForce(transform.gameObject);

            posLast = other.GetComponentInParent<Senmag_HapticCursor>().getPosition();
            dampingVector = new Vector3(0, 0, 0);
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.GetComponentInParent<Senmag_HapticCursor>() != null)
        {
            if(myCustomForceIndex == -1)
            {
                //activeCursor = other.GetComponentInParent<Senmag_HapticCursor>();
                myCustomForceIndex = other.GetComponentInParent<Senmag_HapticCursor>().requestCustomForce(transform.gameObject);
            }
            locked = true;
            Vector3 force = new Vector3(0, 0, 0);
            Vector3 distance = (transform.position - other.GetComponentInParent<Senmag_HapticCursor>().getPosition());
            
            if (distance.magnitude > deadzone)
            {

                if (forceType == ForceType.linear)
                {
                    force = distance * forceGain * radius;
                }
                if (forceType == ForceType.linearInverted)
                {
                    force = distance;

                    float forceMag = radius - distance.magnitude;
                    force = distance * (forceMag / distance.magnitude);
                }
                if (forceType == ForceType.constant)
                {
                    force = distance * (forceGain / distance.magnitude);
                }

                if (distance.magnitude < deadzone + deadzoneOverlap)
                {
                    force *= (distance.magnitude - deadzone) / deadzoneOverlap;

                }
            }
           // UnityEngine.Debug.Log(force);

            


                dampingVector = dampingVector * dampingFilter + (1.0f - dampingFilter) * (other.GetComponentInParent<Senmag_HapticCursor>().getPosition() - posLast);
            posLast = other.GetComponentInParent<Senmag_HapticCursor>().getPosition();

            force -= dampingVector * damping;

            if (force.magnitude > maxForce) force *= (maxForce / force.magnitude);
            other.GetComponentInParent<Senmag_HapticCursor>().modifyCustomForce(myCustomForceIndex, force, transform.gameObject);

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Senmag_HapticCursor>() != null)
        {
            locked = false;
            //UnityEngine.Debug.Log("Exit");
            other.GetComponentInParent<Senmag_HapticCursor>().modifyCustomForce(myCustomForceIndex, new Vector3(0, 0, 0), transform.gameObject); //cleanup
            other.GetComponentInParent<Senmag_HapticCursor>().releaseCustomForce(myCustomForceIndex, transform.gameObject);
            myCustomForceIndex = -1;
            activeCursor = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
