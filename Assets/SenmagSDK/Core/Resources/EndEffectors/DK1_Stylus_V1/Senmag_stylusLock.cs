using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class Senmag_stylusLock : MonoBehaviour
{
	// Start is called before the first frame update
	Senmag_HapticCursor attachedCursor;
	private int myCustomForceIndex;

	public float lockStrength = 1.5f;
	public float lockScale = 0.2f;
	public float damping = 0.2f;
	private int lockMature;
	private Vector3 displacementLast;

	private List<LPFilter> positionFilter = new List<LPFilter>();

	void Start()
    {
		if(transform.parent.GetComponentInChildren<Senmag_radialMenu>() != null) attachedCursor = transform.parent.GetComponentInChildren<Senmag_radialMenu>().activeCursor;
		else{
			UnityEngine.Debug.Log("Stylus lock is intended to be called from a radial menu...");
			Destroy(this.gameObject);
			return;
		}
		this.transform.parent = null;
		this.transform.position = attachedCursor.currentPosition;
		
		myCustomForceIndex = attachedCursor.requestCustomForce(this.gameObject);
		transform.localScale = new Vector3(lockScale, lockScale, lockScale);

		this.transform.LookAt(Camera.main.transform.position);
		transform.RotateAround(transform.position, transform.up, 180);

		lockMature = (int)(1f / Time.fixedDeltaTime);

		positionFilter.Add(new LPFilter());
		positionFilter.Add(new LPFilter());
		positionFilter.Add(new LPFilter());

		positionFilter[0].init(0.05f);
		positionFilter[1].init(0.05f);
		positionFilter[2].init(0.05f);

		displacementLast = this.transform.InverseTransformPoint(attachedCursor.currentPosition);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
	private void FixedUpdate()
	{
		if(lockMature > 0) lockMature -= 1;
		Vector3 displacement = this.transform.InverseTransformPoint(attachedCursor.currentPosition);
		Vector3 velocity = displacement - displacementLast;
		displacementLast = displacement;

		velocity.x = positionFilter[0].update(velocity.x);
		velocity.y = positionFilter[1].update(velocity.y);
		velocity.z = positionFilter[2].update(velocity.z);




		if (displacement.magnitude > .75f && lockMature == 0)
		{
			UnityEngine.Debug.Log("Lock broken");
			attachedCursor.releaseCustomForce(myCustomForceIndex, this.gameObject);
			Destroy(this.gameObject);
		}

		if (displacement.magnitude < .15f) displacement = new Vector3(0,0,0);
		else displacement *= (displacement.magnitude - 0.2f);
		displacement *= -1f * lockStrength;

		displacement -= velocity * damping;
		displacement = (gameObject.transform.rotation) * displacement;		//transform force vector into global frame

		//UnityEngine.Debug.Log(velocity);
		attachedCursor.modifyCustomForce(myCustomForceIndex, displacement, this.gameObject);

	}
}
