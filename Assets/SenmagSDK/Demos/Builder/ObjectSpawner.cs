using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectSpawner : MonoBehaviour
{
	public List<GameObject> spawnableObjects = new List<GameObject>();
	public ObjectScaler objectScaler;
	public float animationSpeed = 0.2f;

	private GameObject createdObject;

	private bool objectDefaultIsKinematic;
	public float currentScale;
	public bool opening;
	private Vector3 fullScale;
    // Start is called before the first frame update
    void Start()
    {
		if (transform.parent.GetComponentInChildren<Senmag_radialMenu>() != null)
		{
			int selection = transform.parent.GetComponentInChildren<Senmag_radialMenu>().currentSelection;

			UnityEngine.Debug.Log("Selection was " + transform.parent.GetComponentInChildren<Senmag_radialMenu>().currentSelection);

			if(selection < 0 || selection >= spawnableObjects.Count)
			{
				UnityEngine.Debug.Log("Selection out of bounds");
				Destroy(this.gameObject);
			}

			UnityEngine.Debug.Log("Spawner start...");

			createdObject = Instantiate(spawnableObjects[selection]);
			if(createdObject.GetComponentInChildren<Rigidbody>()== null && createdObject.GetComponent<Rigidbody>() == null)
			{
				UnityEngine.Debug.Log("Spawner adding rigidbody...");
				createdObject.AddComponent<Rigidbody>();
			}
			if (createdObject.GetComponent<Rigidbody>() != null){
				createdObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

				//objectDefaultIsKinematic = createdObject.GetComponent<Rigidbody>().isKinematic;
				//createdObject.GetComponent<Rigidbody>().isKinematic = true;
			}

			if (createdObject.GetComponentInChildren<Rigidbody>() != null){
				createdObject.GetComponentInChildren<Rigidbody>().constraints = RigidbodyConstraints.FreezeAll;

				//objectDefaultIsKinematic = createdObject.GetComponentInChildren<Rigidbody>().isKinematic;
				//createdObject.GetComponentInChildren<Rigidbody>().isKinematic = true;
			}

			createdObject.transform.parent = this.gameObject.transform;
			createdObject.transform.localPosition = new Vector3(0,0,0);
			fullScale = createdObject.transform.localScale;
			objectScaler.setTargetObject(createdObject.gameObject);
			createdObject.transform.localScale = new Vector3(0, 0, 0);


			currentScale = 0;
			opening = true;

		}
		else
		{
			UnityEngine.Debug.Log("No radial menu discovered - cannot determine object selection");
			Destroy(this.gameObject);
		}
	}

    // Update is called once per frame
    void Update()
    {
		if(objectScaler == null)
		{
			Destroy(this.gameObject);
		}
		if (opening == true)
		{
			currentScale += (1 - currentScale) * animationSpeed;
			if (currentScale >= 0.98f)
			{
				currentScale = 1;
				opening = false;
			}
			createdObject.transform.localScale = fullScale * currentScale;
		}
	}
}
