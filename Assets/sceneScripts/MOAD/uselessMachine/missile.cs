using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

namespace uselessBox 
{

	public class missile : MonoBehaviour
	{
		public enum MissileState
		{
			ready,
			launching,
			launched,
			exploding,
			destroyed,
		}

		public MissileState missileState;
		public float missilePower;
		public float turnSpeed;
		public float launchDistance = 1;
		public float turningSlowdown = 2;

		public ParticleSystem missileSmoke;
		public ParticleSystem explosionSmoke;

		public AudioClip soundClipLaunch;
		public AudioClip soundClipExplosion;


		

		private Vector3 startPosition;


		private Quaternion startRotation;


		

		// Start is called before the first frame update
		void Start()
		{

			startPosition = transform.localPosition;
			startRotation = transform.localRotation;
			missileSmoke.enableEmission = false;
			//explosionSmoke.enableEmission = false;
			transform.GetChild(0).GetComponent<MeshCollider>().enabled = false;
			missileSmoke.Play();

		}

		// Update is called once per frame


		void Update()
		{
			if (missileState == MissileState.launching)
			{
				
				missileSmoke.enableEmission = true;
				
				gameObject.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, missilePower), ForceMode.Impulse);
				if ((transform.localPosition - startPosition).magnitude > launchDistance) missileState = MissileState.launched;
			}
			if (missileState == MissileState.launched)
			{
				transform.GetChild(0).GetComponent<MeshCollider>().enabled = true;
				missileSmoke.enableEmission = true;

				Vector3 cursorPos = GameObject.Find("SenmagWorkspace").GetComponent<Senmag_Workspace>().osenmagServer.deviceList[0].cursor.GetComponent<Senmag_HapticCursor>().currentPosition;
				//gameObject.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, missilePower), ForceMode.Impulse);
				Quaternion lookOnLook = Quaternion.LookRotation(cursorPos - transform.position);
				transform.rotation = Quaternion.Slerp(transform.rotation, lookOnLook, Time.deltaTime * turnSpeed);

				float angleError = Quaternion.Angle(lookOnLook, transform.rotation);
				//float angleError = (lookOnLook.eulerAngles - transform.rotation.eulerAngles).magnitude;
				//if (angleError > 360) angleError = 360;
				//if (angleError > 180) angleError = 360-angleError;

				float speedMod = (angleError / (180) * turningSlowdown);
				if (speedMod > 0.9f) speedMod = 0.9f;
				speedMod = 1 - speedMod;
				gameObject.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, missilePower * speedMod), ForceMode.Impulse);
				//UnityEngine.Debug.Log(angleError + "   " + speedMod);
			}
			if (missileState == MissileState.exploding)
			{
				if (explosionSmoke.isPlaying == false)
				{
					respawn();
					missileState = MissileState.ready;
				}
			}
		}

		public void launch()
		{
			if (missileState == MissileState.ready) {
				gameObject.GetComponent<Rigidbody>().AddRelativeForce(new Vector3(0, 0, missilePower), ForceMode.Impulse);
				gameObject.GetComponent<Rigidbody>().isKinematic = false;
				missileState = MissileState.launching;
				transform.GetComponent<AudioSource>().clip = soundClipLaunch;
				transform.GetComponent<AudioSource>().Play();


				

				//missileSmoke.enableEmission = true;
				//transform.GetChild(0).GetComponent<ParticleSystem>().enableEmission = true;
			}
		}

		private void OnCollisionEnter(Collision collision)
		{
			explode();

		}

		public void explode()
		{
			Component[] hapticExplosions;

			hapticExplosions = GetComponents(typeof(Senmag_ThingyGoBoom));
			foreach(Senmag_ThingyGoBoom explosion in hapticExplosions) explosion.HapticBoom();

			transform.GetComponent<AudioSource>().clip = soundClipExplosion;
			transform.GetComponent<AudioSource>().Play();


			UnityEngine.Debug.Log("boom!");
			transform.GetChild(0).GetComponent<MeshCollider>().enabled = false;
			List<GameObject> children = new List<GameObject>(0);
			GetDescendants(transform, children);
			missileSmoke.enableEmission = false;
			foreach (GameObject obj in children) {
				if (obj.GetComponent<MeshRenderer>()) obj.GetComponent<MeshRenderer>().enabled = false;
			}
			explosionSmoke.Play();
			missileState = MissileState.exploding;


		}

		private void GetDescendants(Transform parent, List<GameObject> list)
		{
			foreach (Transform child in parent)
			{
				list.Add(child.gameObject);
				GetDescendants(child, list);
			}
		}

		public void respawn()
		{
			transform.localPosition = startPosition;
			transform.localRotation = startRotation;
			gameObject.GetComponent<Rigidbody>().isKinematic = true;
			missileState = MissileState.ready;

			List<GameObject> children = new List<GameObject>(0);
			GetDescendants(transform, children);
			missileSmoke.enableEmission = false;
			foreach (GameObject obj in children)
			{
				if (obj.GetComponent<MeshRenderer>()) obj.GetComponent<MeshRenderer>().enabled = true;
			}
		}
	}
}
