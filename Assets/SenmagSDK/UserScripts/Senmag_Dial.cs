using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{
	public class Dial : MonoBehaviour
	{
		public float position;
		public bool show = true;
		int visible;
		public Quaternion rotation;
		// Start is called before the first frame update
		GameObject dial;
		GameObject pointer;
		void Start()
		{
			dial = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
			pointer = GameObject.CreatePrimitive(PrimitiveType.Cube);
			dial.transform.parent = transform;
			pointer.transform.parent = dial.transform;
			dial.name = "dial";
			pointer.name = "pointer";

			dial.transform.localScale = new Vector3(1, .5f, 1);
			dial.transform.localPosition = new Vector3(0, 0f, 0);
			pointer.transform.localScale = new Vector3(0.1f, 2, 0.5f);
			pointer.transform.localPosition = new Vector3(0, 0.001f, .5f);

			visible = 1;
			//dial.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

			dial.GetComponent<CapsuleCollider>().enabled = false;
			dial.AddComponent<MeshCollider>();
			dial.AddComponent<Rigidbody>();
			dial.GetComponent<MeshCollider>().convex = true;
			dial.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePosition;
			dial.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationX;
			dial.GetComponent<Rigidbody>().constraints |= RigidbodyConstraints.FreezeRotationZ;
			dial.GetComponent<Rigidbody>().angularDrag = 1f;
			dial.GetComponent<Renderer>().material.SetColor("_Color", new Color(.5f, .5f, .5f));
			pointer.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, (94f / 255f), (31f / 255f)));
		}

		// Update is called once per frame
		void Update()
		{
			position = dial.transform.rotation.y;
			rotation = dial.transform.rotation;
			if (show == true && visible == 0)
			{
				dial.GetComponent<MeshRenderer>().enabled = true;
				dial.GetComponent<MeshCollider>().enabled = true;
				pointer.GetComponent<MeshRenderer>().enabled = true;
				pointer.GetComponent<BoxCollider>().enabled = true;
				visible = 1;
			}
			else if (show == false && visible == 1)
			{
				dial.GetComponent<MeshRenderer>().enabled = false;
				dial.GetComponent<MeshCollider>().enabled = false;
				pointer.GetComponent<MeshRenderer>().enabled = false;
				pointer.GetComponent<BoxCollider>().enabled = false;

				visible = 0;
			}
		}
		private void FixedUpdate()
		{
			//gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(0, 5, 0);
		}
	}
}