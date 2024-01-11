using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HSVPicker;
using UnityEngine.UI;
using SenmagHaptic;

public class Slider_Haptic : MonoBehaviour
{
	public GameObject targetSliderObj;
	
	public UnityEngine.UI.Slider slider;

	//public bool useX = true;
	//public bool useY = false;
	public bool forceX = true;
	public bool forceY = true;
	public bool forceZ = false;

	public float damping = 0.2f;
	private Vector3 forceLast;

	public float forceGain = 5f;
	public float maxForce = 1f;
	private bool cursorInteracting;
	private int cursorInteractingCounter;
	private Senmag_HapticCursor activeCursor;
	private int myCustomForceIndex;


	private bool updateRequired;
	private Vector3 localCursorPos;
	// Start is called before the first frame update
	void Start()
    {
		slider = targetSliderObj.GetComponent<UnityEngine.UI.Slider>();
		updateRequired = false;
	}

    // Update is called once per frame
    void Update()
    {
		if(updateRequired){
			updateRequired = false;
			
			slider.value = (localCursorPos.x + 0.5f);

			//targetSlider.value = (localCursorPos.x + 0.5f);
			//targetSlider. = (localCursorPos.x + 0.5f);

		}

		if(cursorInteractingCounter > 0)
		{
			cursorInteractingCounter -= 1;
			if(cursorInteractingCounter == 0)
			{
				cursorInteracting = false;
				if(activeCursor != null){
					if(myCustomForceIndex != -1) activeCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);
				}
			}
		}

		//targetSlider.Set(localCursorPos.x + 0.5f);

	}

	private void FixedUpdate()
	{
		if (cursorInteracting)
		{
			Vector3 localCursorPos = this.transform.InverseTransformPoint(activeCursor.currentPosition);
			Vector3 force = new Vector3(0, 0, 0);
			Vector3 velocity = force - forceLast;
			forceLast = force;

			//UnityEngine.Debug.Log(force);

			if (forceX) force.x -= (localCursorPos.x) * forceGain * GetComponent<BoxCollider>().size.x;
			if(forceY) force.y -= (localCursorPos.y) * forceGain * GetComponent<BoxCollider>().size.y;
			if(forceZ) force.z -= (localCursorPos.z) * forceGain * GetComponent<BoxCollider>().size.z;
			if (force.magnitude > maxForce) force *= (maxForce / force.magnitude);

			force -= velocity * damping;
			//UnityEngine.Debug.Log(force.y);
			force = (gameObject.transform.rotation) * force;

			activeCursor.modifyCustomForce(myCustomForceIndex, force, this.gameObject);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject.GetComponentInParent<Senmag_HapticCursor>() != null)
		{
			localCursorPos = this.transform.InverseTransformPoint(other.transform.position);
			updateRequired = true;

			if(myCustomForceIndex != -1 && activeCursor != null) activeCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);


			activeCursor = other.gameObject.GetComponentInParent<Senmag_HapticCursor>();
			myCustomForceIndex = activeCursor.requestCustomForce(this.gameObject);
			cursorInteracting = true;
			cursorInteractingCounter = 2;
		}
		else Physics.IgnoreCollision(GetComponent<Collider>(), other.gameObject.GetComponent<Collider>());

		/*GameObject obj = other.gameObject.transform.parent.gameObject;
		if (obj == null) return;
		bool exit = false;
		while (exit == false)
		{

			if (obj.name.Contains("Cursor"))
			{
				//			UnityEngine.Debug.Log("Found cursor!");
				exit = true;
				break;
			}
			else
			{
				if (obj.transform.parent == null) return;
				obj = obj.transform.parent.gameObject;

			}
		}

		UnityEngine.Debug.Log("Found cursor!");
		localCursorPos = this.transform.InverseTransformPoint(other.transform.position);
		updateRequired = true;*/
	}
}
