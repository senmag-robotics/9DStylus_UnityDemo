using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{
	public class buttonFaceScript : MonoBehaviour
	{
		public int isPressed;
		public int isClicked;

		// Start is called before the first frame update
		void Start()
		{

		}

		// Update is called once per frame
		void Update()
		{

		}
		private void OnCollisionEnter(Collision collision)
		{
			//Debug.Log("button touched " + collision.gameObject.name);
			if (collision.gameObject.name == "button")
			{
				//Debug.Log("button pressed " + collision.gameObject.name);
				isPressed = 1;
				isClicked = 1;
				
			}
		}
		private void OnCollisionExit(Collision collision)
		{
			if(collision.gameObject.name == "button")
				{
				isPressed = 0;
				Debug.Log("button released!");
			}
		}
	}
}