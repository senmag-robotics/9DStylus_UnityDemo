using UnityEngine;
using SenmagHaptic;

public class SnapToGrid : MonoBehaviour
{
	[SerializeField] private Vector3 gridSize = default; // distance between snap points for each axis
	public float explForce = default;
	public bool moveable = true;
	private bool movedOnce = false;

	bool pickedUpLast = false;

	public void Update()
	{
		if(GetComponent<Senmag_interactionTools>() != null)
		{
			if(GetComponent<Senmag_interactionTools>().pickedUp == false && pickedUpLast == true)
			{
				Snap();
			}

			pickedUpLast = GetComponent<Senmag_interactionTools>().pickedUp;
		}
	}
	public void Snap() 
	{
		// Rounds object position to the nearest multiple of the grid size
		var position = new Vector3(
			Mathf.Round(this.transform.position.x / this.gridSize.x) * this.gridSize.x,
			Mathf.Round(this.transform.position.y / this.gridSize.y) * this.gridSize.y,
			Mathf.Round(this.transform.position.z / this.gridSize.z) * this.gridSize.z
		);

		this.transform.position = position;
		this.transform.rotation = Quaternion.identity; // Object rotation is reset
	}

}
