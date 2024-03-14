using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class avr_hoopControl : MonoBehaviour
{
    public GameObject hoop;
    private GameObject targetObject;
    public bool moveHoop;
    public List<Vector3> hoopPositions = new List<Vector3>();
    private Vector3 lastPosition;
    private Vector3 nextPosition;
    private int positionIndex = 0;
    private float position = 1;
    public float hoopMotionTime;
    private float hoopMotionStartTime;

    public bool teleportBall;
    public Vector3 ballTeleportLocation;
    public int teleportDelay;
    public bool freezeOnTeleport;

    private int teleportCounter;
    // Start is called before the first frame update
    void Start()
    {
        if (hoop == null) moveHoop = false;
        if(hoopPositions.Count >= 1) nextPosition = hoopPositions[0];
    }

    // Update is called once per frame
    void Update()
    {
        if (position < 1)
        {
            
            position = (Time.time - hoopMotionStartTime) / hoopMotionTime;

            
            if (position > 1) position = 1;

            hoop.transform.position = Vector3.Slerp(lastPosition, nextPosition, position);

        }

        if (targetObject != null) {
            if (teleportCounter > 0)
            {
                teleportCounter--;
                if (teleportCounter == 0)
                {

                    if (moveHoop)
                    {
                        position = 0;
                        hoopMotionStartTime = Time.time;
                        lastPosition = nextPosition;
                        positionIndex += 1;
                        if (positionIndex >= hoopPositions.Count) positionIndex = 0;
                        nextPosition = hoopPositions[positionIndex];
                    }


                    if (freezeOnTeleport == true)
                    {
                        targetObject.gameObject.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
                    }
                    targetObject.gameObject.transform.position = ballTeleportLocation;
                    targetObject = null;
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)                                                         //entered when this object collides with another
    {
        if (other.gameObject.GetComponentInChildren<Senmag_interactionTools>() == true)                 //if the other object contains an interaction tools script
        {
            if (other.gameObject.GetComponentInChildren<Senmag_interactionTools>().pickedUp == true)    //if the other object is held by a cursor
            {

                other.gameObject.GetComponentInChildren<Senmag_interactionTools>().handleInteraction(Senmag_InteractionActionType.release, Senmag_StylusActionType.none);   //send the signal for the cursor to drop the other object
                if(teleportBall == true)
                {
                    targetObject = other.gameObject;
                    teleportCounter = teleportDelay;
                }
            }
        }
    }
}
