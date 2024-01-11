using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{
	public class ModelViewer : MonoBehaviour
	{

		public GameObject Model;
		GameObject dial;
		GameObject slider;
		GameObject spaceBall;
		// Start is called before the first frame update
		void Start()
		{
			dial = GameObject.CreatePrimitive(PrimitiveType.Cube);
			slider = GameObject.CreatePrimitive(PrimitiveType.Cube);
			//spaceBall = GameObject.CreatePrimitive(PrimitiveType.Sphere);


			dial.GetComponent<MeshRenderer>().enabled = false;
			dial.GetComponent<BoxCollider>().enabled = false;
			slider.GetComponent<MeshRenderer>().enabled = false;
			slider.GetComponent<BoxCollider>().enabled = false;

			/*spaceBall.transform.parent = transform;
			spaceBall.transform.localPosition = new Vector3(3, 0.2f, -0.2f);
			spaceBall.transform.localScale = new Vector3(1, 1f, 1f);
			spaceBall.AddComponent<Rigidbody>();
			spaceBall.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
			spaceBall.GetComponent<Rigidbody>().angularDrag = 10;
			spaceBall.GetComponent<Rigidbody>().mass = .1f;
			spaceBall.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));
			*/


			dial.transform.parent = transform;
			dial.transform.localPosition = new Vector3(0, 0, 4f);
			dial.transform.localScale = new Vector3(5, .2f, 5);
			dial.AddComponent<Dial>();

			slider.transform.parent = transform;
			slider.transform.localPosition = new Vector3(0, 0.2f, -0.2f);
			slider.transform.localScale = new Vector3(3, .2f, 0.5f);
			slider.AddComponent<Slider>();


			Model = Instantiate(Model);
			Model.transform.localPosition = new Vector3(0, 0f, 1.5f);
		}

		// Update is called once per frame
		void Update()
		{
			float scale = (5 + slider.GetComponent<Slider>().position * 10) / 2000; ;
			Model.transform.localScale = new Vector3(scale, scale, scale);
			float distance = dial.GetComponent<Dial>().position;

			Model.transform.localPosition = new Vector3(0, .5f + slider.GetComponent<Slider>().position*.6f, .3f);
			//Quaternion rotations = spaceBall.transform.localRotation;
			Quaternion rotations = dial.GetComponent<Dial>().rotation;

			Model.transform.rotation = rotations;// new Quaternion(0, 1, 0, rotation);
		}
	}
}