using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SenmagHaptic
{
	public class TelerobotUI : MonoBehaviour
	{
		private bool menuOpen;

		public GameObject UIPanel;
		public GameObject Panel_RobotSettings;
		public GameObject Panel_HapticsSettings;
		public GameObject Panel_TaskSettings;

		public GameObject Button_Menu;
		public GameObject Button_RobotSettings;
		public GameObject Button_HapticsSettings;
		public GameObject Button_TaskSettings;

		private BannerState UIBannerStateLast;


		// Start is called before the first frame update
		void Start()
		{
			menuOpen = false;


		}

		// Update is called once per frame
		void Update()
		{
			if (Button_Menu.GetComponent<Senmag_button>().wasClicked())
			{
				Button_Menu.GetComponent<Senmag_button>().isFlashing = false;
				if (UIPanel.GetComponent<MessageBanner>().bannerState == BannerState.hidden)
				{
					UIPanel.GetComponent<MessageBanner>().showBanner();
					Button_Menu.GetComponent<Senmag_button>().isHighlighted = true;
				}
				else
				{
					if (Panel_RobotSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_RobotSettings.GetComponent<MessageBanner>().hideBanner();
					if (Panel_HapticsSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_HapticsSettings.GetComponent<MessageBanner>().hideBanner();
					if (Panel_TaskSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_TaskSettings.GetComponent<MessageBanner>().hideBanner();
					UIPanel.GetComponent<MessageBanner>().hideBanner();
					Button_Menu.GetComponent<Senmag_button>().isHighlighted = false;
					Button_RobotSettings.GetComponent<Senmag_button>().isHighlighted = false;
					Button_HapticsSettings.GetComponent<Senmag_button>().isHighlighted = false;
					Button_TaskSettings.GetComponent<Senmag_button>().isHighlighted = false;
				}
			}
			/*if (Button_Menu.GetComponent<Senmag_button>().wasClicked())
			{
				if (menuOpen == false)
				{
					UIPanel.GetComponent<MessageBanner>().bannerState = BannerState.appearing;
					Button_Menu.GetComponent<Senmag_button>().isHighlighted = true;
					menuOpen = true;
				}
				else
				{
					UIPanel.GetComponent<MessageBanner>().bannerState = BannerState.dissapearing;
					Button_Menu.GetComponent<Senmag_button>().isHighlighted = false;
					menuOpen = false;
				}
			}*/

			if (Button_RobotSettings.GetComponent<Senmag_button>().wasClicked())
			{
				if(Panel_RobotSettings.GetComponent<MessageBanner>().bannerState == BannerState.hidden) 
				{
					Panel_RobotSettings.GetComponent<MessageBanner>().showBanner();
					Button_RobotSettings.GetComponent<Senmag_button>().isHighlighted = true;
				}
				else
				{
					Panel_RobotSettings.GetComponent<MessageBanner>().hideBanner();
					Button_RobotSettings.GetComponent<Senmag_button>().isHighlighted = false;
				}

				Button_HapticsSettings.GetComponent<Senmag_button>().isHighlighted = false;
				Button_TaskSettings.GetComponent<Senmag_button>().isHighlighted = false;
				if (Panel_HapticsSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_HapticsSettings.GetComponent<MessageBanner>().hideBanner();
				if (Panel_TaskSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_TaskSettings.GetComponent<MessageBanner>().hideBanner();
			}
			if (Button_HapticsSettings.GetComponent<Senmag_button>().wasClicked())
			{
				if (Panel_HapticsSettings.GetComponent<MessageBanner>().bannerState == BannerState.hidden)
				{
					Panel_HapticsSettings.GetComponent<MessageBanner>().showBanner();
					Button_HapticsSettings.GetComponent<Senmag_button>().isHighlighted = true;
				}
				else
				{
					Panel_HapticsSettings.GetComponent<MessageBanner>().hideBanner();
					Button_HapticsSettings.GetComponent<Senmag_button>().isHighlighted = false;
				}
				Button_RobotSettings.GetComponent<Senmag_button>().isHighlighted = false;
				Button_TaskSettings.GetComponent<Senmag_button>().isHighlighted = false;
				if (Panel_RobotSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_RobotSettings.GetComponent<MessageBanner>().hideBanner();
				if (Panel_TaskSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_TaskSettings.GetComponent<MessageBanner>().hideBanner();
			}
			if (Button_TaskSettings.GetComponent<Senmag_button>().wasClicked())
			{
				if (Panel_TaskSettings.GetComponent<MessageBanner>().bannerState == BannerState.hidden)
				{
					Panel_TaskSettings.GetComponent<MessageBanner>().showBanner();
					Button_TaskSettings.GetComponent<Senmag_button>().isHighlighted = true;
				}
				else
				{
					Panel_TaskSettings.GetComponent<MessageBanner>().hideBanner();
					Button_TaskSettings.GetComponent<Senmag_button>().isHighlighted = false;
				}

				Button_HapticsSettings.GetComponent<Senmag_button>().isHighlighted = false;
				Button_RobotSettings.GetComponent<Senmag_button>().isHighlighted = false;
				if (Panel_HapticsSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_HapticsSettings.GetComponent<MessageBanner>().hideBanner();
				if (Panel_RobotSettings.GetComponent<MessageBanner>().bannerState != BannerState.hidden) Panel_RobotSettings.GetComponent<MessageBanner>().hideBanner();
			}





			/*if(UIPanel.GetComponent<MessageBanner>().bannerState != UIBannerStateLast)
			{
				Button_Menu.GetComponent<Senmag_button>().resetButton();
				Button_RobotSettings.GetComponent<Senmag_button>().resetButton();
				Button_HapticsSettings.GetComponent<Senmag_button>().resetButton();
				Button_TaskSettings.GetComponent<Senmag_button>().resetButton();
			}
			UIBannerStateLast = UIPanel.GetComponent<MessageBanner>().bannerState;*/
		}
		private void hideRobotSettings()
		{

		}
		private void hideHapticsSettings()
		{

		}
		private void hideTaskSettings()
		{

		}
	}
}