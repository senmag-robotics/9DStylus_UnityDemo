using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tip_Painter : MonoBehaviour
{
	GameObject ParentWithRigidBody;

	private Senmag_stylusControl stylusControl;
	private CollisionPainter collisionPainter;
	private int scaleUpdate;
	public int scaleUpdateRate = 10;
	public float scaleIncrement = 0.01f;
	public float scaleMin = 0.01f;
	public float scaleMax = 0.5f;

	public float pressureModRadius = 0;
	public float pressureModHardness = 0;
	public float pressureModStrength = 0;

	public float strength = 1;
	public float hardness = 1;

	public float baseScale = 0.02f;
	public bool allowScaling;

	public bool allowColourChange = true;

	public List<GameObject> colourIndicators = new List<GameObject>();
	public List<Material> materialIndicators = new List<Material>();

	public Color defaultColour = Color.red;
	// Start is called before the first frame update
	void Start()
    {
		if(allowColourChange) PaintManager.instance.enablePaint();
		//UnityEngine.Debug.Log("Tip painter Start");
        if(GetComponentInParent<Rigidbody>() != null)
		{
			ParentWithRigidBody = GetComponentInParent<Rigidbody>().transform.gameObject;
			//if(ParentWithRigidBody.AddComponent<CollisionPainter>() != null)
			//{
			//	Destroy(ParentWithRigidBody.GetComponent<CollisionPainter>());
			//}
			UnityEngine.Debug.Log("create collisionPainter");
			collisionPainter = ParentWithRigidBody.AddComponent<CollisionPainter>();
			collisionPainter.paintColor = defaultColour;
		}
		else
		{
			UnityEngine.Debug.Log("couldn't find parent rigidbody");
		}

		if (gameObject.GetComponentInParent<Senmag_stylusControl>() != null) stylusControl = gameObject.GetComponentInParent<Senmag_stylusControl>();
		else
		{
			UnityEngine.Debug.Log("Delete tool, unable to find stylus control object");
		}

		collisionPainter.pressureModRadius = pressureModRadius;
		collisionPainter.pressureModHardness = pressureModHardness;
		collisionPainter.pressureModStrength = pressureModStrength;
		collisionPainter.strengthConst = strength;
		collisionPainter.hardnessConst = hardness;

		if(allowScaling == true) collisionPainter.setRadius(this.transform.localScale.x / 4f);
		else collisionPainter.setRadius(baseScale);

		foreach(GameObject indicator in colourIndicators) indicator.GetComponent<MeshRenderer>().material.color = defaultColour;

		foreach (Material indicator in materialIndicators) indicator.color = defaultColour;

		if (allowColourChange) SetColour(PaintManager.instance.selectedColour);
		//UnityEngine.Debug.Log("Tip painter Start end");
	}

	void SetColour(Color newColour)
	{
		if(ParentWithRigidBody != null)
		{
			foreach (GameObject indicator in colourIndicators) indicator.GetComponent<MeshRenderer>().material.color = newColour;
			foreach (Material indicator in materialIndicators) indicator.color = newColour;
			collisionPainter.paintColor = newColour;
		}
	}

    // Update is called once per frame
    void Update()
    {
		if (allowColourChange){
			if(PaintManager.instance.isEnabled() == false) PaintManager.instance.enablePaint();
			if(PaintManager.instance.newColour() == true)
			{
				SetColour(PaintManager.instance.selectedColour);
			}
		}
		if (scaleUpdate > 0) scaleUpdate -= 1;

		if (scaleUpdate == 0 && allowScaling == true)
		{
			if (stylusControl.Input_isHeld(Stylus_Action.scrollUpShort))
			{
				scaleUpdate = scaleUpdateRate;
				Vector3 scale = this.transform.localScale;
				scale += new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
				if (scale.x > scaleMax) scale = new Vector3(scaleMax, scaleMax, scaleMax);
				this.transform.localScale = scale;
				collisionPainter.setRadius(scale.x / 4f);
			}
			if (stylusControl.Input_isHeld(Stylus_Action.scrollUpLong))
			{
				scaleUpdate = scaleUpdateRate/2;
				Vector3 scale = this.transform.localScale;
				scale += new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
				if (scale.x > scaleMax) scale = new Vector3(scaleMax, scaleMax, scaleMax);
				this.transform.localScale = scale;
				collisionPainter.setRadius(scale.x / 4f);
			}
			if (stylusControl.Input_isHeld(Stylus_Action.scrollDownShort))
			{
				scaleUpdate = scaleUpdateRate;
				Vector3 scale = this.transform.localScale;
				scale -= new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
				if (scale.x < scaleMin) scale = new Vector3(scaleMin, scaleMin, scaleMin);
				this.transform.localScale = scale;
				collisionPainter.setRadius(scale.x / 4f);
			}
			if (stylusControl.Input_isHeld(Stylus_Action.scrollDownLong))
			{
				scaleUpdate = scaleUpdateRate/2;
				Vector3 scale = this.transform.localScale;
				scale -= new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);
				if (scale.x < scaleMin) scale = new Vector3(scaleMin, scaleMin, scaleMin);
				this.transform.localScale = scale;
				collisionPainter.setRadius(scale.x / 4f);
			}
		}
    }

	private void OnDestroy()
	{

		if(ParentWithRigidBody != null)
		{
			UnityEngine.Debug.Log("destroy collisionPainter");
			Destroy(collisionPainter);
			PaintManager.instance.disablePaint();
		}
	}
}
