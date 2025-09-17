using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;


public class hapticScaleArrow : MonoBehaviour
{

	public float forceGain = 5f;
	public float maxForce = 1f;

	public bool globalMod;

	private bool cursorInteracting;

	private Senmag_HapticCursor activeCursor;
	private int myCustomForceIndex;
	public bool grabbed;
	private Vector3 grabPos_cursor;
	private Vector3 grabPos_arrow;
	private float myZPos;

	public float scaleStepSize = 0;

	// Start is called before the first frame update
	void Start()
    {
		cursorInteracting = false;

	}

	private void OnTriggerEnter(Collider other)
	{
		if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
		{
			cursorInteracting = true;
			activeCursor = other.gameObject.GetComponentInParent<Senmag_HapticCursor>();
			myCustomForceIndex = activeCursor.requestCustomForce(this.gameObject);
			//UnityEngine.Debug.Log("cursor enter with forceIndex: " + myCustomForceIndex);
		}
	}
	private void OnTriggerExit(Collider other)
	{
		if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
		{
			activeCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);
			cursorInteracting = false;
			//activeCursor = null;
			//UnityEngine.Debug.Log("cursor exit");
		}
	}

	private void startGrab()
	{

		grabPos_arrow = transform.localPosition;
		myZPos = transform.localPosition.x;
		grabPos_cursor = activeCursor.currentLocalPosition;
		//grabPos_cursor = this.transform.InverseTransformPoint(activeCursor.currentPosition);
		grabbed = true;
	}

	private void stopGrab()
	{
		grabbed = false;
	}

	public float getPos()
	{
		return transform.localPosition.z;
	}

	public void setPos(float newpos)
	{
		Vector3 pos = transform.localPosition;
		pos.z = newpos;
		transform.localPosition = pos;
	}

	// Update is called once per frame
	bool keyLatch;
	void Update()
    {
		/*if(Input.GetKey(KeyCode.C)){
			if (keyLatch == false && cursorInteracting == true)
			{
				keyLatch = true;
				startGrab();
				UnityEngine.Debug.Log("grabbing");
			}
		}
		else if(keyLatch == true)
		{
			keyLatch = false;
			stopGrab();
			UnityEngine.Debug.Log("dropping");
		}*/

		if (cursorInteracting == true)
		{
			if(activeCursor.GetComponentInChildren<Senmag_stylusControl>() != null)
			{
				if (activeCursor.GetComponentInChildren<Senmag_stylusControl>().Input_isHeld(Stylus_Action.leftClick))
				{
					if(keyLatch == false){
						keyLatch = true;
						startGrab();
					}
				}
				else if(keyLatch == true)
				{
					keyLatch = false;
					stopGrab();
				}
			}
		}

		if (grabbed)
		{
			if (activeCursor.GetComponentInChildren<Senmag_stylusControl>() != null){
				globalMod = activeCursor.GetComponentInChildren<Senmag_stylusControl>().Input_isHeld(Stylus_Action.auxClick);
				if (keyLatch == true && activeCursor.GetComponentInChildren<Senmag_stylusControl>().Input_isHeld(Stylus_Action.leftClick)== false)
				{
					keyLatch = false;
					stopGrab();
				}
			}

			Vector3 newPos = this.transform.InverseTransformPoint(activeCursor.currentLocalPosition);

			float cursorOffset = this.transform.InverseTransformPoint(activeCursor.currentLocalPosition).z - this.transform.InverseTransformPoint(grabPos_cursor).z;

			Vector3 mypos = transform.localPosition;
			mypos.z = grabPos_arrow.z + cursorOffset * transform.localScale.z;
			transform.localPosition = mypos;
		}

		if (cursorInteracting)
		{
			Vector3 localCursorPos = this.transform.InverseTransformPoint(activeCursor.currentLocalPosition);
			Vector3 force = new Vector3(0,0,0);
			//UnityEngine.Debug.Log(localCursorPos);

			force.x -= (localCursorPos.x) * forceGain * GetComponent<BoxCollider>().size.x;
			force.y -= (localCursorPos.y) * forceGain * GetComponent<BoxCollider>().size.y;
			if(force.magnitude > maxForce) force *= (maxForce / force.magnitude);

			force = (gameObject.transform.rotation) * force;
			activeCursor.modifyCustomForce(myCustomForceIndex, force, this.gameObject);
		}
	}

	private void OnDestroy()
	{
		if (cursorInteracting)
		{
			activeCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);
			cursorInteracting = false;
			activeCursor = null;
		}
	}
}
