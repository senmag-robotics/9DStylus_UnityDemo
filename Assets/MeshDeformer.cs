using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using SenmagHaptic;

[RequireComponent(typeof(MeshFilter))]

[System.Serializable]
public class StaticFrictionEffect
{
	public Vector3	frictionLockPoint;
	public float	staticFrictionGain = 0;
	public float	staticFrictionspacing;

}
public class MeshDeformer : MonoBehaviour {

	public float springForce = 20f;
	public float damping = 5f;
	public float deformForce = 5f;

	Mesh deformingMesh;
	Vector3[] originalVertices, displacedVertices;
	Vector3[] vertexVelocities;


	public float stiffness;
	public float dynamicFriction = 0;


	private Vector3 cursorPos;
	private Vector3 cursorPosLast;
	private Vector3 cursorVelocity;
	private Vector3 surfacePos;
	private Vector3 meshDeformPos;


	public List<StaticFrictionEffect> staticFrictionEffects = new List<StaticFrictionEffect>();

	LPFilter CursorVelocityFilterX = new LPFilter();
	LPFilter CursorVelocityFilterY = new LPFilter();
	LPFilter CursorVelocityFilterZ = new LPFilter();

	private Vector3 myPos;
	private bool isTouched;
	private int myForceIndex;
	private float meshForce;
	public float sphereRadius;
	float uniformScale = 1f;

	void Start () {


		sphereRadius = transform.localScale.x;

		deformingMesh = GetComponent<MeshFilter>().mesh;
		originalVertices = deformingMesh.vertices;
		displacedVertices = new Vector3[originalVertices.Length];
		for (int i = 0; i < originalVertices.Length; i++) {
			displacedVertices[i] = originalVertices[i];
		}
		vertexVelocities = new Vector3[originalVertices.Length];
	}

	void Update () {
		uniformScale = transform.localScale.x;
		for (int i = 0; i < displacedVertices.Length; i++) {
			UpdateVertex(i);
		}
		deformingMesh.vertices = displacedVertices;
		deformingMesh.RecalculateNormals();


		if (isTouched)
		{
			/*float force = 0;
			surfacePos = cursorPos - transform.position;
			myPos = transform.position;
			force = (sphereRadius - surfacePos.magnitude) * deformForce;
			surfacePos = transform.position + surfacePos * (sphereRadius / surfacePos.magnitude);*/
			MeshDeformer deformer = GetComponent<MeshDeformer>();
			//deformer.AddDeformingForce(surfacePos, meshForce);
			deformer.AddDeformingForce(meshDeformPos, meshForce);
			

		}


	}
	private void OnDrawGizmos()
	{
		if (isTouched)
		{
			//Gizmos.color = Color.red;
			//Gizmos.DrawLine(transform.position, meshDeformPos);
			//Gizmos.color = Color.green;
			//Gizmos.DrawLine(transform.position, surfacePos);
			//Gizmos.DrawSphere(surfacePos, .02f);
			//Gizmos.DrawSphere(cursorPos, .1f);
			//Gizmos.DrawSphere(transform.position, .1f);
		}
	}

	void UpdateVertex (int i) {
		Vector3 velocity = vertexVelocities[i];
		Vector3 displacement = displacedVertices[i] - originalVertices[i];
		displacement *= uniformScale;
		velocity -= displacement * springForce * Time.deltaTime;
		velocity *= 1f - damping * Time.deltaTime;
		vertexVelocities[i] = velocity;
		displacedVertices[i] += velocity * (Time.deltaTime / uniformScale);
	}

	public void AddDeformingForce (Vector3 point, float force) {
		point = transform.InverseTransformPoint(point);
		for (int i = 0; i < displacedVertices.Length; i++) {
			AddForceToVertex(i, point, force);
		}
	}

	void AddForceToVertex (int i, Vector3 point, float force) {
		Vector3 pointToVertex = displacedVertices[i] - point;
		pointToVertex *= uniformScale;
		float attenuatedForce = force / (1f + pointToVertex.sqrMagnitude);
		float velocity = attenuatedForce * Time.deltaTime;
		vertexVelocities[i] += pointToVertex.normalized * velocity;
	}


	private void OnTriggerEnter(Collider collider)
	{
		if (isCursor(collider))
		{
			isTouched = true;
			cursorPos = collider.gameObject.transform.position;
			cursorPosLast = cursorPos;
			CursorVelocityFilterX.init(0.1f);
			CursorVelocityFilterY.init(0.1f);
			CursorVelocityFilterZ.init(0.1f);

			foreach (StaticFrictionEffect effect in staticFrictionEffects) effect.frictionLockPoint = cursorPos;
			
			myForceIndex = collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().requestCustomForce(gameObject);
			if (myForceIndex == -1) myForceIndex = collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().requestCustomForce(gameObject);


			calculateForce(collider);
		}
	}
	private void OnTriggerStay(Collider collider)
	{
		if (isCursor(collider))
		{
			//MeshDeformer deformer = GetComponent<MeshDeformer>();
			//deformer.AddDeformingForce(collider.gameObject.transform.position, 1f);
			cursorPos = collider.gameObject.transform.position;
			isTouched = true;
			if (myForceIndex == -1) myForceIndex = collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().requestCustomForce(gameObject);

			calculateForce(collider);
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (isCursor(collider))
		{
			isTouched = false;
			collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().releaseCustomForce(myForceIndex, gameObject);
			myForceIndex = -1;
		}
	}


	private bool isCursor(Collider collider)
	{
		if (collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>())
		{
			//UnityEngine.Debug.Log("was cursor");
			return true;
		}
		//UnityEngine.Debug.Log("wasnt cursor");
		return false;
	}

	private void calculateForce(Collider collider)
	{
		surfacePos = cursorPos - transform.position;
		myPos = transform.position;
		meshForce = ((sphereRadius + collider.gameObject.transform.parent.transform.parent.GetComponent<Senmag_Workspace>().cursorScale) - surfacePos.magnitude) * deformForce;



		




		Vector3 surfacePosPlusCursor = transform.position + surfacePos * ((sphereRadius + collider.gameObject.transform.parent.transform.parent.GetComponent<Senmag_Workspace>().cursorScale) / surfacePos.magnitude);
		surfacePos = transform.position + surfacePos * (sphereRadius / surfacePos.magnitude);
		Vector3 cursorForce = (surfacePosPlusCursor - cursorPos) * stiffness;






		float forceOffset = 0.1f;
		float rayOffset = 1.1f;
		Vector3 rayOrigin = ((surfacePos - transform.position) * rayOffset) + transform.position;

		//Ray inputRay = new Ray(transform.position, (surfacePos - transform.position) * 2f);
		Ray inputRay = new Ray(rayOrigin, transform.position - rayOrigin);
		RaycastHit hit;
		//Debug.DrawRay(rayOrigin, transform.position - rayOrigin, Color.white);

		if (Physics.Raycast(inputRay, out hit))
		{
			//UnityEngine.Debug.Log("ray hit" + hit.transform.gameObject.name);
			MeshDeformer deformer = hit.collider.GetComponent<MeshDeformer>();
			if (deformer)
			{
				//UnityEngine.Debug.Log("ray hit ball");
				Vector3 point = hit.point;
				point += hit.normal * forceOffset;
				meshDeformPos = point;
				//deformer.AddDeformingForce(point, force);
			}
		}






		Quaternion rotation = Quaternion.LookRotation(transform.position, cursorPos);

		cursorVelocity = new Vector3(CursorVelocityFilterX.update(cursorPos.x - cursorPosLast.x), CursorVelocityFilterY.update(cursorPos.y - cursorPosLast.y), CursorVelocityFilterZ.update(cursorPos.z - cursorPosLast.z));
		cursorPosLast = cursorPos;
		float forceMagnitude = cursorForce.magnitude;
		if (forceMagnitude > 0.02f) forceMagnitude = 0.02f;

		Vector3 frictionForce = (cursorVelocity * dynamicFriction * forceMagnitude);
		frictionForce = rotation * frictionForce;
		if (frictionForce.y > 0) frictionForce.y = 0;
		frictionForce = Quaternion.Inverse(rotation) * frictionForce;
		cursorForce -= frictionForce;
		//cursorForce -= (cursorVelocity * dynamicFriction * forceMagnitude);



		foreach (StaticFrictionEffect effect in staticFrictionEffects)
		{
			if ((effect.frictionLockPoint - cursorPos).magnitude > effect.staticFrictionspacing)
			{
				effect.frictionLockPoint = cursorPos;
			}
			else
			{
				frictionForce = (cursorPos - effect.frictionLockPoint) * effect.staticFrictionGain * forceMagnitude;
				frictionForce = rotation * frictionForce;
				frictionForce.y = 0;
				frictionForce = Quaternion.Inverse(rotation) * frictionForce;
				cursorForce -= frictionForce;


				//cursorForce -= (cursorPos - effect.frictionLockPoint) * effect.staticFrictionGain * forceMagnitude;
			}

		}

		//UnityEngine.Debug.Log(meshForce);
		collider.gameObject.transform.parent.GetComponent<Senmag_HapticCursor>().modifyCustomForce(myForceIndex, cursorForce, gameObject);
	}
}