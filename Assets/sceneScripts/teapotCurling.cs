using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SenmagHaptic
{
	public class teapotCurling : MonoBehaviour
	{
		// Start is called before the first frame update

		GameObject alley;
		GameObject target1;
		GameObject target2;
		GameObject target3;
		GameObject target4;
		public GameObject teapot;
		GameObject resetButton;
		int buttonLatch;
		void Start()
		{
			alley = GameObject.CreatePrimitive(PrimitiveType.Cube);
			target1 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			target2 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			target3 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			target4 = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			resetButton = GameObject.CreatePrimitive(PrimitiveType.Cube);

			alley.name = "alley";
			target1.name = "target";
			teapot.name = "teapot";
			resetButton.name = "TeapotButton";

			alley.transform.parent = transform;
			target1.transform.parent = alley.transform;
			target2.transform.parent = target1.transform;
			target3.transform.parent = target2.transform;
			target4.transform.parent = target3.transform;
			resetButton.transform.parent = alley.transform;

			target1.GetComponent<CapsuleCollider>().enabled = false;
			target2.GetComponent<CapsuleCollider>().enabled = false;
			target3.GetComponent<CapsuleCollider>().enabled = false;
			target4.GetComponent<CapsuleCollider>().enabled = false;

			alley.transform.localScale = new Vector3(4f, 1f, 4f);
			target1.transform.localScale = new Vector3(0.5f, 1f, 0.5f);
			target2.transform.localScale = new Vector3(0.7f, 1f, 0.7f);
			target3.transform.localScale = new Vector3(0.6f, 1f, 0.6f);
			target4.transform.localScale = new Vector3(0.4f, 1f, 0.4f);

			alley.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));
			alley.AddComponent<Rigidbody>();
			alley.GetComponent<Rigidbody>().mass = 20;
			alley.GetComponent<Rigidbody>().isKinematic = true;
			target1.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 0, 0));
			target2.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));
			target3.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 0, 0));
			target4.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1));

			alley.transform.localPosition = new Vector3(0, 0, 1.2f);
			target1.transform.localPosition = new Vector3(0, -0.499f, .25f);
			target2.transform.localPosition = new Vector3(0, 0.001f, 0f);
			target3.transform.localPosition = new Vector3(0, 0.001f, 0f);
			target4.transform.localPosition = new Vector3(0, 0.001f, 0f);

			resetButton.GetComponent<BoxCollider>().enabled = false;
			resetButton.GetComponent<MeshRenderer>().enabled = false;
			resetButton.transform.localScale = new Vector3(0.08f, 0.5f, 0.02f);
			resetButton.transform.localPosition = new Vector3(-.05f, 0.47f, -.45f);

			resetButton.AddComponent<button>();
			resetButton.GetComponent<button>().ButtonText = "spawn teapot!";
			resetButton.GetComponent<button>().scaleX = 1f;
			resetButton.GetComponent<button>().scaleY = 1;
			resetButton.GetComponent<button>().scaleZ = 1f;
			resetButton.GetComponent<button>().textScale = new Vector3(0.1f, 1, 0.2f);

			//	teapot.transform.localScale = new Vector3(0.002f, 0.002f, 0.002f);


		}

		// Update is called once per frame
		void Update()
		{
			if (resetButton.GetComponent<button>().checkClicked() == 1)
			{
				Debug.Log("spawning teapot... ");
				
					Vector3 tmp = transform.position;
					tmp.y += 0.2f;
					tmp.z -= 0.8f;
					Instantiate(teapot, tmp, transform.rotation);
					buttonLatch = 1;
				
			}
			
		}
	}
}
