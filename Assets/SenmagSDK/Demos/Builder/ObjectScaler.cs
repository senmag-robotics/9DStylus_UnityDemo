using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class ObjectScaler : MonoBehaviour
{

	public GameObject scaleArrowXP;
	public GameObject scaleArrowXN;
	public GameObject scaleArrowYP;
	public GameObject scaleArrowYN;
	public GameObject scaleArrowZP;
	public GameObject scaleArrowZN;

	public GameObject targetObject;


	private hapticScaleArrow arrowxp;
	private hapticScaleArrow arrowxn;
	private hapticScaleArrow arrowyp;
	private hapticScaleArrow arrowyn;
	private hapticScaleArrow arrowzp;
	private hapticScaleArrow arrowzn;

	private Vector3 objectBaseScale;
	private Vector3 startScale;
	private Vector3 objectScaleLast;

	private float objectBaseMass;
	private bool forceGlobal;

	// Start is called before the first frame update
	void Start()
    {
		arrowxp = scaleArrowXP.GetComponentInChildren<hapticScaleArrow>();
		arrowxn = scaleArrowXN.GetComponentInChildren<hapticScaleArrow>();
		arrowyp = scaleArrowYP.GetComponentInChildren<hapticScaleArrow>();
		arrowyn = scaleArrowYN.GetComponentInChildren<hapticScaleArrow>();
		arrowzp = scaleArrowZP.GetComponentInChildren<hapticScaleArrow>();
		arrowzn = scaleArrowZN.GetComponentInChildren<hapticScaleArrow>();

		if (targetObject != null) setTargetObject(targetObject);


    }

	public void setTargetObject(GameObject target)
	{
		targetObject = target;
		this.transform.position = target.transform.position;
		objectBaseScale = target.transform.localScale;
		forceGlobal = false;
		Vector3 objectScale = new Vector3(0,0,0);
		if(target.GetComponent<BoxCollider>() != null){
			objectScale = target.GetComponent<BoxCollider>().bounds.size;
			this.transform.position = target.GetComponent<BoxCollider>().bounds.center;
		}
		if (target.GetComponentInChildren<BoxCollider>() != null) objectScale = target.GetComponentInChildren<BoxCollider>().bounds.size;

		if (target.GetComponent<SphereCollider>() != null){
			objectScale = target.GetComponent<SphereCollider>().bounds.size;
			forceGlobal = true;
		}
		if (target.GetComponentInChildren<SphereCollider>() != null){						//sphere colliders don't scale irregularly
			objectScale = target.GetComponentInChildren<SphereCollider>().bounds.size;
			forceGlobal = true;
		}
		if (target.GetComponent<CapsuleCollider>() != null){
			objectScale = target.GetComponent<CapsuleCollider>().bounds.size;
			forceGlobal = true;
		}
		if (target.GetComponentInChildren<CapsuleCollider>() != null){						//capsule colliders don't scale irregularly
			objectScale = target.GetComponentInChildren<CapsuleCollider>().bounds.size;
			forceGlobal = true;
		}
		if (target.GetComponent<MeshCollider>() != null){
			objectScale = target.GetComponent<MeshCollider>().bounds.size;
			this.transform.position = target.GetComponent<MeshCollider>().bounds.center;
		}
		if (target.GetComponentInChildren<MeshCollider>() != null) objectScale = target.GetComponentInChildren<MeshCollider>().bounds.size;
		//else return;

		objectBaseMass = target.GetComponentInChildren<Rigidbody>().mass;


		startScale = transform.localScale;
		//rescaleSelf();
		startScale.x *= objectScale.x;
		startScale.y *= objectScale.y;
		startScale.z *= objectScale.z;
		transform.localScale = startScale;

		if (target.GetComponent<Senmag_interactionTools>() != null)
		{
			if (target.GetComponent<Senmag_interactionTools>().scaleSteps < 0)
			{
				transform.localScale = new Vector3(0, 0, 0);
			}
			arrowxp.scaleStepSize = target.GetComponent<Senmag_interactionTools>().scaleSteps;
			arrowxn.scaleStepSize = target.GetComponent<Senmag_interactionTools>().scaleSteps;
			arrowyp.scaleStepSize = target.GetComponent<Senmag_interactionTools>().scaleSteps;
			arrowyn.scaleStepSize = target.GetComponent<Senmag_interactionTools>().scaleSteps;
			arrowzp.scaleStepSize = target.GetComponent<Senmag_interactionTools>().scaleSteps;
			arrowzn.scaleStepSize = target.GetComponent<Senmag_interactionTools>().scaleSteps;
		}

		
	}

	// Update is called once per frame
	void Update()
    {
		if (targetObject.GetComponentInChildren<Senmag_interactionTools>() != null)
		{
			if(targetObject.GetComponentInChildren<Senmag_interactionTools>().pickedUp == true)
			{
				targetObject.transform.parent = null;
				Destroy(this.gameObject);
			}
		}

		//UnityEngine.Debug.Log("scaler update...");
		if (targetObject.GetComponent<Senmag_interactionTools>() != null)
		{
			//UnityEngine.Debug.Log("checking int tools main...");
			if (targetObject.GetComponent<Senmag_interactionTools>().pickedUp == true)
			{
				targetObject.transform.parent = null;
				Destroy(this.gameObject);
			}
			
		}
		else if (targetObject.GetComponentInChildren<Senmag_interactionTools>() != null)
		{
			//UnityEngine.Debug.Log("interaction tools found in children...");

			int count = 0;
			foreach(Senmag_interactionTools interactionTools in targetObject.GetComponentsInChildren<Senmag_interactionTools>())
			{
				//UnityEngine.Debug.Log("checking int tools " + count + ", result: " + interactionTools.pickedUp);
				count += 1;
				if (interactionTools.pickedUp == true)
				{
					targetObject.transform.parent = null;
					Destroy(this.gameObject);
				}
			}
			if(count == 0) UnityEngine.Debug.Log("no interaction tools found in children...");

		}
		else
		{
			//UnityEngine.Debug.Log("object doesn't have an interaction tools...");
		}


		if (arrowxp.grabbed){
			arrowxn.setPos(arrowxp.getPos());
			if(arrowxp.globalMod || forceGlobal) globalScale(arrowxp.getPos());
			rescaleObject();
		}
		if (arrowxn.grabbed)
		{
			arrowxp.setPos(arrowxn.getPos());
			if (arrowxn.globalMod || forceGlobal) globalScale(arrowxn.getPos());
			rescaleObject();
		}
		if (arrowyp.grabbed)
		{
			arrowyn.setPos(arrowyp.getPos());
			if (arrowyp.globalMod || forceGlobal) globalScale(arrowyp.getPos());
			rescaleObject();
		}
		if (arrowyn.grabbed)
		{
			arrowyp.setPos(arrowyn.getPos());
			if (arrowyn.globalMod || forceGlobal) globalScale(arrowyn.getPos());
			rescaleObject();
		}
		if (arrowzp.grabbed)
		{
			arrowzn.setPos(arrowzp.getPos());
			if (arrowzp.globalMod || forceGlobal) globalScale(arrowzp.getPos());
			rescaleObject();
		}
		if (arrowzn.grabbed)
		{
			arrowzp.setPos(arrowzn.getPos());
			if (arrowzn.globalMod || forceGlobal) globalScale(arrowzn.getPos());
			rescaleObject();
		}

		if(targetObject != null){
			if(objectScaleLast != targetObject.transform.localScale)
			{
				objectScaleLast = targetObject.transform.localScale;
				//rescaleSelf();
			}
			if (targetObject.GetComponent<MeshCollider>() != null)
			{
				this.transform.position = targetObject.GetComponent<MeshCollider>().bounds.center;
			}
		}
	}
	void globalScale(float scale){
		arrowxp.setPos(scale);
		arrowxn.setPos(scale);
		arrowyp.setPos(scale);
		arrowyn.setPos(scale);
		arrowzp.setPos(scale);
		arrowzn.setPos(scale);
	}

	void rescaleSelf()
	{
		//UnityEngine.Debug.Log
		Vector3 myscale = new Vector3(0,0,0);
		myscale.x = startScale.x * targetObject.transform.localScale.x;
		myscale.y = startScale.y * targetObject.transform.localScale.y;
		myscale.z = startScale.z * targetObject.transform.localScale.z;
		transform.localScale = myscale;
	}
	void rescaleObject()
	{
		float scalex = 1 + arrowxp.getPos();
		float scaley = 1 + arrowyp.getPos();
		float scalez = 1 + arrowzp.getPos();
		Vector3 newScale = new Vector3(scalex * objectBaseScale.x, scaley * objectBaseScale.y, scalez * objectBaseScale.z);
		targetObject.transform.localScale = newScale;
		targetObject.GetComponent<Rigidbody>().mass = objectBaseMass * (newScale.magnitude / objectBaseScale.magnitude);
		//UnityEngine.Debug.Log("base: " + objectBaseMass + ", scale: " + newScale.magnitude / objectBaseScale.magnitude + ", new mass: " + objectBaseMass * (newScale.magnitude / objectBaseScale.magnitude));

	}
}
