using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

namespace uselessBox
{
	public class grapple : MonoBehaviour
	{
		// Start is called before the first frame update
		LineRenderer line;
		public float maxDistance = 1f;
		public float grappleForce = 0.01f;
		public bool isGrappling;
		private int customForceIndex;

		private float grappleForceMod;
		private float grappleVibrationSpeed = .75f;
		private float grappleVibrationMag = 0.3f;
		private float grappleVibrationCounter = 0;

		private Senmag_HapticCursor activeCursor;

		void Start()
		{
			isGrappling = false;
			line = gameObject.AddComponent<LineRenderer>();

			line.SetColors(new Color(0.1f, 0.1f, 0.1f), new Color(0.1f, 0.1f, 0.1f));

			Material whiteDiffuseMat = new Material(Shader.Find("Unlit/Texture"));
			whiteDiffuseMat.color = Color.gray;
			line.material = whiteDiffuseMat;

			//line.material = new Material(Shader.Find("Unlit/Texture"));

			//line.startColor = new Color(0.5f, 0.5f, 0.5f);
			//line.endColor = new Color(0.5f, 0.5f, 0.5f);

			line.SetWidth(0.01f, 0.01f);
			line.positionCount = 2;
			line.numCornerVertices = 10;

			customForceIndex = -1;
		}

		// Update is called once per frame
		void Update()
		{
			if (isGrappling)
			{

				if((activeCursor.currentPosition - this.transform.position).magnitude > maxDistance)
				{
					isGrappling = false;
					UnityEngine.Debug.Log("grapple broke!");
				}

				if (customForceIndex == -1)
				{
					customForceIndex = activeCursor.requestCustomForce(gameObject);
				}
				else
				{

					grappleVibrationCounter += grappleVibrationSpeed;
					grappleForceMod = grappleForce + Mathf.Sin(grappleVibrationCounter) * grappleForce * grappleVibrationMag;

					Vector3 force = transform.position - activeCursor.currentPosition;
					force *= grappleForceMod / force.magnitude;




					activeCursor.modifyCustomForce(customForceIndex, force, gameObject);
					//UnityEngine.Debug.Log(force * 100);
				}
			}
			else if (customForceIndex != -1)
			{
				activeCursor.releaseCustomForce(customForceIndex, gameObject);
				customForceIndex = -1;
				line.enabled = false;
			}
		}

		private void LateUpdate()
		{
			if (isGrappling)
			{
				//isGrappling = false;
				line.SetPosition(0, activeCursor.currentPosition);
				line.SetPosition(1, this.transform.position);
			}
			else if (GetComponent<ParticleSystem>().particleCount != 0)
			{
				ParticleSystem m_currentParticleEffect = GetComponent<ParticleSystem>();
				ParticleSystem.Particle[] ParticleList = new ParticleSystem.Particle[m_currentParticleEffect.particleCount];
				m_currentParticleEffect.GetParticles(ParticleList);


				if ((ParticleList[0].position - this.transform.position).magnitude > maxDistance)
				{
					GetComponent<ParticleSystem>().Stop();
					m_currentParticleEffect.SetParticles(ParticleList, 0);
					line.enabled = false;
				}
				else
				{
					line.enabled = true;

					line.SetPosition(0, ParticleList[0].position);
					line.SetPosition(1, this.transform.position);
				}
			}

			else line.enabled = false;

		}

		void OnParticleCollision(GameObject other)
		{
			//UnityEngine.Debug.Log("Hit!");
			if (other.GetComponentInParent<Senmag_HapticCursor>() != null)
			{
				if (other.GetComponentInParent<Senmag_HapticCursor>() != activeCursor)
				{
					if (customForceIndex != -1) activeCursor.releaseCustomForce(customForceIndex, gameObject);
				}
			
				isGrappling = true;

				activeCursor = other.GetComponentInParent<Senmag_HapticCursor>();
				UnityEngine.Debug.Log("Hit!");
			}
		}
	}
}