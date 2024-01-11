using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
	public float position;
	public int orientation = 0;
	GameObject sliderBase;
	GameObject sliderEnd1;
	GameObject sliderEnd2;
	GameObject slider;
	public bool show = true;
	public Vector3 sliderRotation;
	int visible;

    // Start is called before the first frame update
    void Start()
    {
        sliderBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
		sliderEnd1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		sliderEnd2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
		slider = GameObject.CreatePrimitive(PrimitiveType.Cube);

		//slider.GetComponent<Transform>().rotation = Quaternion.Euler(sliderRotation);
		//sliderBase.GetComponent<Transform>().rotation = Quaternion.Euler(sliderRotation);
		//sliderEnd1.GetComponent<Transform>().rotation = Quaternion.Euler(sliderRotation);
		//sliderEnd2.GetComponent<Transform>().rotation = Quaternion.Euler(sliderRotation);

		sliderBase.transform.parent = transform;
		sliderEnd1.transform.parent = sliderBase.transform;
		sliderEnd2.transform.parent = sliderBase.transform;
		slider.transform.parent = sliderBase.transform;

		sliderBase.name = "sliderBase";
		sliderEnd1.name = "sliderEnd1";
		sliderEnd2.name = "sliderEnd2";
		slider.name = "slider";

		//slider.transform.localRotation = Quaternion.Euler(sliderRotation);
		//sliderBase.transform.localRotation = Quaternion.Euler(sliderRotation);
		//sliderEnd1.transform.localRotation = Quaternion.Euler(sliderRotation);
		//sliderEnd2.transform.localRotation = Quaternion.Euler(sliderRotation);

		sliderBase.AddComponent<Rigidbody>();
		sliderEnd1.AddComponent<Rigidbody>();
		sliderEnd2.AddComponent<Rigidbody>();
		slider.AddComponent<Rigidbody>();
		sliderBase.GetComponent<BoxCollider>().enabled = false;

		sliderBase.transform.localScale = new Vector3(1, 1, 1);
		sliderBase.transform.localPosition = new Vector3(0, 0, 0);
		sliderBase.transform.localRotation = Quaternion.identity;

		sliderEnd1.transform.localScale = new Vector3(0.1f, 1, 1);
		sliderEnd1.transform.localPosition = new Vector3(-0.45f, 0, 0);
		sliderEnd1.transform.localRotation = Quaternion.identity;
		sliderEnd2.transform.localScale = new Vector3(0.1f, 1, 1);
		sliderEnd2.transform.localPosition = new Vector3(0.45f, 0, 0);
		sliderEnd2.transform.localRotation = Quaternion.identity;

		slider.transform.localScale = new Vector3(0.1f, 0.8f, 0.5f);
		slider.transform.localPosition = new Vector3(0.0f, 0.5f, 0f);
		slider.transform.localRotation = Quaternion.identity;


		slider.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, (94f / 255f), (31f / 255f)));
		sliderBase.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));
		sliderEnd1.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));
		sliderEnd2.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));


		sliderBase.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		sliderEnd1.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		
		sliderEnd2.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;
		sliderEnd1.GetComponent<Rigidbody>().isKinematic = true;
		sliderEnd2.GetComponent<Rigidbody>().isKinematic = true;
		slider.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
		slider.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionY;
		slider.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezePositionZ;
		slider.GetComponent<Rigidbody>().drag = 200 ;
		slider.GetComponent<Rigidbody>().mass = .2f ;
		visible = 1;

		

	}

    // Update is called once per frame
    void Update()
    {
		position = slider.transform.localPosition.x;
		if (show == true && visible == 0)
		{
			sliderBase.GetComponent<MeshRenderer>().enabled = true;
		//	sliderBase.GetComponent<BoxCollider>().enabled = true;
			sliderEnd1.GetComponent<MeshRenderer>().enabled = true;
			sliderEnd1.GetComponent<BoxCollider>().enabled = true;
			sliderEnd2.GetComponent<MeshRenderer>().enabled = true;
			sliderEnd2.GetComponent<BoxCollider>().enabled = true;
			slider.GetComponent<MeshRenderer>().enabled = true;
			slider.GetComponent<BoxCollider>().enabled = true;
			visible = 1;
		}
		else if (show == false && visible == 1)
		{
			sliderBase.GetComponent<MeshRenderer>().enabled = false;
			sliderBase.GetComponent<BoxCollider>().enabled = false;
			sliderEnd1.GetComponent<MeshRenderer>().enabled = false;
			sliderEnd1.GetComponent<BoxCollider>().enabled = false;
			sliderEnd2.GetComponent<MeshRenderer>().enabled = false;
			sliderEnd2.GetComponent<BoxCollider>().enabled = false;
			slider.GetComponent<MeshRenderer>().enabled = false;
			slider.GetComponent<BoxCollider>().enabled = false;
			
			visible = 0;
		}
		//Debug.Log(position);
	}

	public void setPosition(float pos)
	{
		if (pos > 0.66f) pos = 0.66f;
		if (pos < -0.66f) pos = -0.66f;
		Vector3 tmp = slider.transform.localPosition;
		tmp.x = pos;
		slider.transform.localPosition = tmp;

	}
}
