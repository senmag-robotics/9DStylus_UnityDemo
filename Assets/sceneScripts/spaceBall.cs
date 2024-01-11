using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace SenmagHaptic
{
	public class spaceBall : MonoBehaviour
	{
		GameObject Ball;
		GameObject anchor;
		ConfigurableJoint spring;
		public bool show = true;
		int visible = 1;
		public Vector3 position;
		// Start is called before the first frame update
		void Start()
		{
			Ball = GameObject.CreatePrimitive(PrimitiveType.Sphere);
			Ball.transform.parent = transform;
			Ball.transform.localScale = new Vector3(1, 1, 1);
			Ball.transform.localPosition = new Vector3(0, 0, 0);
			anchor = GameObject.CreatePrimitive(PrimitiveType.Cube);
			anchor.transform.parent = transform;
			anchor.transform.localPosition = new Vector3(0, 0, 0);
			anchor.transform.localScale = new Vector3(1, 1, 1);
			anchor.AddComponent<Rigidbody>();
			anchor.GetComponent<MeshRenderer>().enabled = false;
			anchor.GetComponent<BoxCollider>().enabled = false;
			anchor.GetComponent<Rigidbody>().isKinematic = true;

			Ball.AddComponent<Rigidbody>();
			Ball.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeRotation;
			Ball.GetComponent<Rigidbody>().useGravity = false;

			spring = Ball.AddComponent<ConfigurableJoint>();
			spring.connectedBody = anchor.GetComponent<Rigidbody>();
			spring.xMotion = ConfigurableJointMotion.Free;
			spring.zMotion = ConfigurableJointMotion.Free;
			spring.yMotion = ConfigurableJointMotion.Free;
			spring.anchor = new Vector3(0, 0, 0);
			spring.autoConfigureConnectedAnchor = false;
			spring.connectedAnchor = new Vector3(0, 0, 0);

			JointDrive joint = new JointDrive();
			joint.positionSpring = 50f;
			joint.maximumForce = 40;
			joint.positionDamper = 5;
			spring.xDrive = joint;
			spring.yDrive = joint;
			spring.zDrive = joint;
		}

		// Update is called once per frame
		void Update()
		{
			if(show == true && visible == 0)
			{
				Ball.GetComponent < MeshRenderer>().enabled = true;
				Ball.GetComponent <SphereCollider>().enabled = true;
				visible = 1;
			}
			if (show == false && visible == 1)
			{
				Ball.GetComponent<MeshRenderer>().enabled = false;
				Ball.GetComponent<SphereCollider>().enabled = false;
				visible = 0;
			}

			position.x = Ball.transform.localPosition.x;
			position.y = Ball.transform.localPosition.y;
			position.z = Ball.transform.localPosition.z;
			if (Ball.transform.localPosition.x < 0.01 && Ball.transform.localPosition.x > -0.01) position.x = 0;
			if (Ball.transform.localPosition.y < 0.01 && Ball.transform.localPosition.y > -0.01) position.y = 0;
			if (Ball.transform.localPosition.z < 0.01 && Ball.transform.localPosition.z > -0.01) position.z = 0;
		}
	}
}