using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SenmagHaptic;

public class meshFade : MonoBehaviour
{
	// Start is called before the first frame update
	public float fadeRate = 0.02f;

	private float fadeProgress = 0;
	public int fadeState = 0;
	private Color startColor;
	private Color fadedColor;

	public Material defaultMaterial;
	public Material defaultMaterialtransparent;
	void Start()
    {
	
		startColor = GetComponent<Renderer>().material.color;
		fadedColor = GetComponent<Renderer>().material.color;
		fadedColor.a = 1;
	}

    // Update is called once per frame
    void Update()
    {
		if(transform.GetChild(0).GetComponent<Senmag_interactionTools>().touched == true)
		{
			transform.GetChild(0).GetComponent<Senmag_interactionTools>().touched = false;
			startFade();
		}
		if (fadeState == 1)
		{
			if (fadeProgress <= 1) { 
				fadeProgress += fadeRate;
				UnityEngine.Debug.Log(fadeProgress);
				GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(startColor, fadedColor, fadeProgress));
			}
		}

		if (fadeState == 0)
		{
			if (fadeProgress <= 1)
			{
				fadeProgress += fadeRate;
				UnityEngine.Debug.Log(fadeProgress);
				GetComponent<Renderer>().material.SetColor("_Color", Color.Lerp(fadedColor, startColor, fadeProgress));
			}
		}
	}

	public void startFade()
	{
		if(fadeState != 1)
		{
			fadeState = 1;
			fadeProgress = 0;
		}
	}

	public void reverseFade()
	{
		if (fadeState != 0)
		{
			fadeState = 0;
			fadeProgress = 0;
		}
	}

}
