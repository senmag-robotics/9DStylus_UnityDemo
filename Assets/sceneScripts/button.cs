using System.Collections;
using System.Collections.Generic;

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine;
namespace SenmagHaptic
{
	public class button : MonoBehaviour
	{
		public float strength = 60;
		public float scaleX;
		public float scaleY;
		public float scaleZ;
		public string ButtonText;
		public Vector3 textScale = new Vector3(.8f, 1, .8f);
		public bool show = true;
		public float textSize = 0.7f;
		
		private bool latched;

		int visible;

		ConfigurableJoint spring;
		GameObject buttonBase;
		GameObject buttonFace;
		GameObject buttonText;
		// Start is called before the first frame update
		void Start()
		{
			buttonBase = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonBase.name = "button";
			buttonBase.transform.parent = transform;
			buttonBase.transform.localPosition = new Vector3(0, 0, 0);
			buttonBase.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
			buttonBase.AddComponent<Rigidbody>();
			buttonBase.GetComponent<Rigidbody>().isKinematic = true;
			buttonBase.GetComponent<Renderer>().material.SetColor("_Color",new Color(.5f, .5f, .5f));


			buttonFace = GameObject.CreatePrimitive(PrimitiveType.Cube);
			buttonFace.name = "buttonFace";
			buttonFace.transform.parent = buttonBase.transform;
			buttonFace.transform.localPosition = new Vector3(0, 2f, 0);
			buttonFace.transform.localScale = new Vector3(0.8f, .1f, 0.8f);
			buttonFace.AddComponent<Rigidbody>();
			buttonFace.GetComponent<Rigidbody>().useGravity = false;
			buttonFace.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;

			//buttonBase.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 0, 0));
			buttonFace.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, (94f / 255f), (31f/255f)));
			buttonFace.AddComponent<buttonFaceScript>();
			//Destroy(buttonFace.GetComponent<MeshFilter>());
			buttonText = new GameObject();
			
			//buttonText.AddComponent<Transform>();
			buttonText.AddComponent<MeshRenderer>();
			buttonText.AddComponent<TextMesh>();
			buttonText.GetComponent<TextMesh>().text = ButtonText;
			buttonText.GetComponent<TextMesh>().characterSize = textSize;
			buttonText.GetComponent<TextMesh>().anchor = TextAnchor.MiddleCenter;
			buttonText.transform.parent = buttonFace.transform;
			
			//buttonText.transform.localScale = new Vector3(.8f, textSize, .8f);
			buttonText.transform.localScale = textScale;
			buttonText.transform.localPosition = new Vector3(0, 0, 0);
			buttonText.transform.Rotate(new Vector3(90, 0, 0));

			buttonText.GetComponent<Renderer>().material.SetColor("_Color", new Color(0, 0, 0));
			
			


			spring = buttonBase.AddComponent<ConfigurableJoint>();
			spring.connectedBody = buttonFace.GetComponent<Rigidbody>();
			spring.xMotion = ConfigurableJointMotion.Locked;
			spring.zMotion = ConfigurableJointMotion.Locked;
			spring.yMotion = ConfigurableJointMotion.Free;
			spring.anchor = new Vector3(0, 2*buttonBase.transform.localScale.y, 0);
			spring.autoConfigureConnectedAnchor = false;
			spring.connectedAnchor = new Vector3(0, 0, 0);
			spring.enableCollision = true;
			

		

			JointDrive joint = new JointDrive();
			joint.positionSpring = 100000f;
			joint.maximumForce = strength;
			joint.positionDamper = 150;
			spring.yDrive = joint;

			visible = 1;
			//spring.xMotion = ConfigurableJointMotion.Locked;
			//	spring = new GameObject
		}

		// Update is called once per frame
		void Update()
		{
			if(show == true && visible == 0)
			{
				visible = 1;
				buttonBase.GetComponent<MeshRenderer>().enabled = true;
				buttonBase.GetComponent<BoxCollider>().enabled = true;
				buttonFace.GetComponent<MeshRenderer>().enabled = true;
				buttonFace.GetComponent<BoxCollider>().enabled = true;
				buttonText.GetComponent<MeshRenderer>().enabled = true;
			}
			if (show == false && visible == 1)
			{
				visible = 0;
				buttonBase.GetComponent<MeshRenderer>().enabled = false;
				buttonBase.GetComponent<BoxCollider>().enabled = false;
				buttonFace.GetComponent<MeshRenderer>().enabled = false;
				buttonFace.GetComponent<BoxCollider>().enabled = false;
				buttonText.GetComponent<MeshRenderer>().enabled = false;
			}

			//Debug.Log(name + buttonFace.transform.localPosition.y);
			//if(latched == true && )
		}

		public void setColour(int colour)
		{
			if (colour == 0) buttonFace.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, (94f / 255f), (31f / 255f)));
			if (colour == 1) buttonFace.GetComponent<Renderer>().material.SetColor("_Color", new Color((94f / 255f), 1, (31f / 255f)));
			if (colour == 2) buttonFace.GetComponent<Renderer>().material.SetColor("_Color", new Color((94f / 255f), (94f / 255f), (94f / 255f)));
		}
		public int checkPressed()
		{
			return buttonFace.GetComponent<buttonFaceScript>().isPressed;
		}

		public int checkClicked()
		{
			int tmp = buttonFace.GetComponent<buttonFaceScript>().isClicked;

			
			if(tmp == 1) buttonFace.GetComponent<buttonFaceScript>().isClicked = 0;

			//if(tmp == 1 && latched == true) tmp = 0;


			return tmp;
		}
	}
}
