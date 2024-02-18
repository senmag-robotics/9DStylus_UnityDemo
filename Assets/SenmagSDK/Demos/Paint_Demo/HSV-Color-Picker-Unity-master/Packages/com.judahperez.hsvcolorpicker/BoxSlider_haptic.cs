using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HSVPicker;


public class BoxSlider_haptic : MonoBehaviour
{

	public GameObject targetBoxSliderObj;
	private BoxSlider targetBoxSlider;

	//public bool useX = true;
	//public bool useY = false;

	private bool updateRequired;
	private Vector3 localCursorPos;

    // Start is called before the first frame update
    void Start()
    {
		targetBoxSlider = targetBoxSliderObj.GetComponent<BoxSlider>();
		updateRequired = false;
	}

    // Update is called once per frame
    void Update()
    {
        if(updateRequired == true)
		{
			updateRequired = false;
			targetBoxSlider.Set(localCursorPos.x + 0.5f);
			targetBoxSlider.SetY(localCursorPos.y + 0.5f);
		}
		

    }

	private void OnTriggerStay(Collider other)
	{

		GameObject obj = other.gameObject.transform.parent.gameObject;
		if(obj == null) return;
		bool exit = false;
		while (exit == false)
		{
			
			if (obj.name.Contains("Cursor"))
			{
	//			UnityEngine.Debug.Log("Found cursor!");
				exit = true;
				break;
			}
			else
			{
				if (obj.transform.parent == null) return;
				obj = obj.transform.parent.gameObject;
				
			}
		}

		UnityEngine.Debug.Log("Found cursor!");
		localCursorPos = this.transform.InverseTransformPoint(other.transform.position);
		updateRequired = true;
	}
}
