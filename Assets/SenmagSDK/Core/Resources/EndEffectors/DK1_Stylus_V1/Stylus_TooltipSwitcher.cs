using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class Stylus_TooltipSwitcher : MonoBehaviour
{

	public List<GameObject> ToolTips = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
		UnityEngine.Debug.Log("Swapping tool...");
		if (transform.parent.GetComponentInChildren<Senmag_radialMenu>() != null)
		{
			int selection = transform.parent.GetComponentInChildren<Senmag_radialMenu>().currentSelection;

			UnityEngine.Debug.Log("Selection was " + transform.parent.GetComponentInChildren<Senmag_radialMenu>().currentSelection);

			if (selection < 0 || selection >= ToolTips.Count)
			{
				UnityEngine.Debug.Log("Selection out of bounds");
				Destroy(this.gameObject);
			}

			transform.parent.GetComponentInChildren<Senmag_radialMenu>().activeCursor.GetComponent<Senmag_HapticCursor>().stylusControl.setTool_custom(ToolTips[selection], 1f);



		}
		else
		{
			UnityEngine.Debug.Log("No radial menu discovered - cannot determine object selection");
			
		}
		Destroy(this.gameObject);
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
