using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SenmagHaptic
{

	public enum BannerState
	{
		hidden,
		appearing,
		shown,
		dissapearing,
	};

	public class MessageBanner : MonoBehaviour
	{
		
		public List<GameObject> associatedButtons;

		[HideInInspector]
		private float scaleState;
		public BannerState bannerState;

		private string newString;
		private int currentStringLen;
		private int totalStringLen;
		private int textAnimateCounter = 0;


		private GameObject Text;
		private Vector3 scale;
		[Header("Text Options")]
		[Space(20)]

		public TMP_FontAsset Font;
		public FontStyles Style;
		public float FontScale = 0.5f;
		[MultilineAttribute(2)]

		public string BannerText;
		public Color TextColor;
		public Color BorderColor;
		public float BorderThickness;
		public Color BackgroundColor;
		public int BackgroundResolution = 500;

		[Header("Animation options")]
		public bool animateX;
		public bool animateY;
		public bool animateZ;

		public float animateSpeed;

		public bool animateText;
		public float textAnimateSpeed;
		public bool animationFinished;
		private int currentLines = 0;


		// Start is called before the first frame update
		void Start()
		{
			Text = new GameObject();
			Text.name = "Text";
			Text.transform.parent = transform;
			Text.transform.localScale = new Vector3(1, 1, 1);
			Text.transform.localRotation = new Quaternion(0, 0, 0, 1);
			Text.transform.localPosition = new Vector3(0, 0, 0);

			Text.AddComponent<TextMeshProUGUI>();
			//Text.GetComponent<TextMeshProUGUI>().text = ButtonText;
			Text.GetComponent<TextMeshProUGUI>().text = "";
			Text.GetComponent<TextMeshProUGUI>().fontSize = FontScale;
			Text.GetComponent<TextMeshProUGUI>().color = TextColor;
			Text.GetComponent<TextMeshProUGUI>().font = Font;
			Text.GetComponent<TextMeshProUGUI>().fontStyle = Style;
			Text.GetComponent<TextMeshProUGUI>().outlineWidth = BorderThickness;
			Text.GetComponent<TextMeshProUGUI>().outlineColor = BorderColor;
			Text.GetComponent<TextMeshProUGUI>().horizontalAlignment = HorizontalAlignmentOptions.Center;
			Text.GetComponent<TextMeshProUGUI>().verticalAlignment = VerticalAlignmentOptions.Middle;

			scale = transform.localScale;
			transform.localScale = new Vector3(0, 0, 0);
			//bannerState = BannerState.hidden;
			scaleState = 0;
			totalStringLen = 0;
			currentStringLen = 1;
		}

		// Update is called once per frame
		void Update()
		{
			if (bannerState == BannerState.appearing)
			{
				scaleState += animateSpeed;
				if (scaleState >= Mathf.PI / 2f)
				{
					bannerState = BannerState.shown;
					transform.localScale = scale;
					setText(BannerText, true, textAnimateSpeed);

					foreach (GameObject x in associatedButtons)
					{
						x.GetComponent<Senmag_button>().enable();
						x.GetComponent<Senmag_button>().resetButton();
					}
				}
				else
				{
					float sf = Mathf.Sin(scaleState);
					Vector3 tmpscale = scale;

					if (animateX == true)
					{
						tmpscale.x *= sf;
					}
					if (animateY == true)
					{
						tmpscale.y *= sf;
					}
					if (animateZ == true)
					{
						tmpscale.z *= sf;
					}
					transform.localScale = tmpscale;
				}
			}
			if (bannerState == BannerState.dissapearing)
			{
				scaleState += animateSpeed;
				if (scaleState >= Mathf.PI)
				{
					bannerState = BannerState.hidden;
					transform.localScale = new Vector3(0, 0, 0);
					scaleState = 0;
				}
				else
				{
					float sf = Mathf.Sin(scaleState);
					Vector3 tmpscale = scale;

					if (animateX == true)
					{
						tmpscale.x *= sf;
					}
					if (animateY == true)
					{
						tmpscale.y *= sf;
					}
					if (animateZ == true)
					{
						tmpscale.z *= sf;
					}
					transform.localScale = tmpscale;
				}
			}

			if (currentStringLen <= totalStringLen)
			{
				textAnimateCounter++;
				//if (textAnimateCounter > (1/Time.deltaTime) * (1/textAnimateSpeed))
				if(true)
				{
					int totalLines = newString.Split('\n').Length;
					string newLine = "\n\r";

					


					textAnimateCounter = 0;
					Text.GetComponent<TextMeshProUGUI>().text = newString.Substring(0, currentStringLen);
					int appendCount = 0;
					if (Text.GetComponent<TextMeshProUGUI>().text.Split('\n').Length > currentLines)
					{
						appendCount = totalLines - currentLines;
						currentLines = Text.GetComponent<TextMeshProUGUI>().text.Split('\n').Length;
						newLine = "\n";
					}
					else
					{
						currentLines = Text.GetComponent<TextMeshProUGUI>().text.Split('\n').Length;
						appendCount = totalLines - currentLines;
					}




					for (int x = 0; x < appendCount; x++)
					{
						Text.GetComponent<TextMeshProUGUI>().text += newLine;
					}
					currentStringLen += 1;

				}
			}
			else animationFinished = true;

		}

		public void setText(string newText, bool animate, float animationSpeed)
		{
			if (animate == true)
			{
				animationFinished = false;
				
				newString = newText;
                newString = newString.Replace("\\n\\r", "\n\r");

                currentStringLen = 0;
                totalStringLen = newString.Length;

                Text.GetComponent<TextMeshProUGUI>().text = "";
				textAnimateSpeed = animationSpeed;
				currentLines = 0;
			}
			else Text.GetComponent<TextMeshProUGUI>().text = newText;
		}

		public void hideBanner()
		{
			bannerState = BannerState.dissapearing;
			foreach(GameObject x in associatedButtons)
			{
				x.GetComponent<Senmag_button>().disable();
			}
		}
		public void showBanner()
		{
			bannerState = BannerState.appearing;
			
		}

	}
}
