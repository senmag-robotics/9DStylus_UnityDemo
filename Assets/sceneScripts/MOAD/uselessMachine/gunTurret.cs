using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace uselessBox
{
	public class gunTurret : MonoBehaviour
	{
		
		public bool isActive;

		private float hatchTargetAngle;
		// Start is called before the first frame update
		void Start()
		{

			isActive = false;

		}

		// Update is called once per frame
		void Update()
		{
			if (isActive == true)
			{
				transform.LookAt(transform.position - (GameObject.Find("cursor1").transform.GetChild(0).position - transform.position));
			}
			else
			{

			}

		}
	}
}
