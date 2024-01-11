using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using uselessBox;



namespace SenmagHaptic
{

	public enum MOADStates
	{
		moadState_first = 0,
		moadState_welcome,
		moadState_9D_intro,
		moadState_9D_intro2,
		moadState_9D_intro3,
		moadState_9D_intro4,
		moadState_tracking,
		moadState_trackingXYZ,
		moadState_trackingRPY,
		moadState_force,
		moadState_forceSolid,
		moadState_forceHelensSponge,
		moadState_textures,
		moadState_weight,
		moadState_weightLight,
		moadState_weightHeavy,
		//moadState_invisibleIntro,
		//moadState_invisibleTask,
		moadState_applicationsSpiel,
		moadState_uselessMachineIntro,
		moadState_uselessMachine,
		moadState_cleanup,
		moadState_conclusion,
		moadState_last,

	}


	public class MOAD_StateMachine : MonoBehaviour
	{
		public KeyCode key_sceneNext;
		public KeyCode key_scenePrev;
		public KeyCode key_sceneReload;

		public GameObject camera_main;
		public GameObject camera_aux;
		public bool hideSurprises = false;

		public GameObject prefab_cursorXYZ;
		public GameObject prefab_cursorRPY;

		public GameObject gameObject_messageBanner;
		public GameObject gameObject_buttonNext;

		public GameObject gameObject_cursorLabel;

		public GameObject gameObject_hardBall;
		public GameObject gameObject_softBall;
		public GameObject gameObject_heavySwingball;
		public GameObject gameObject_lightSwingball;
		public GameObject gameObject_invisible;
		public GameObject gameObject_uselessMachine;

		public float textSpeed = 200;
		public MOADStates initialState;
		public MOADStates appState;


		private Vector3 startPosition_lightSwingball;
		private Vector3 startPosition_heavySwingball;
		private Vector3 startPosition_hardBall;
		private Vector3 startPosition_softBall;
		private Vector3 startPosition_invisible;
		private Vector3 startPosition_uselessMachine;


		private Vector3 startPosition_cursor;

		public int appTimer = -1;

		private bool initialLoad = true;
		private bool stateTracker = false;

		void Start()
		{
			startPosition_hardBall = gameObject_hardBall.transform.position;
			startPosition_softBall = gameObject_softBall.transform.position;

			startPosition_lightSwingball = gameObject_lightSwingball.transform.position;
			startPosition_heavySwingball = gameObject_heavySwingball.transform.position;

			startPosition_uselessMachine = gameObject_uselessMachine.transform.position;
			startPosition_invisible = gameObject_invisible.transform.position;

			appState = MOADStates.moadState_first;
			unloadAll();

			appState = initialState;
			loadState(initialState);
			initialLoad = true;
		}

		

		// Update is called once per frame
		void Update()
		{
			if (initialLoad == true)
			{
				camera_main.SetActive(true);
				camera_aux.SetActive(false);

				UnityEngine.Debug.Log("loading 0 state...");
				initialLoad = false;
				unloadAll();
				loadState(appState);
			}

			if (Input.GetKeyDown(key_sceneNext))
			{
				appState += 1;
				if (appState > MOADStates.moadState_last) appState = MOADStates.moadState_last;
				unloadAll();
				loadState(appState);
			}
			if (Input.GetKeyDown(key_scenePrev))
			{
				appState -= 1;
				if (appState < MOADStates.moadState_first) appState = MOADStates.moadState_first;
				unloadAll();
				loadState(appState);
			}

			if (Input.GetKeyDown(key_sceneReload))
			{
				Application.LoadLevel(Application.loadedLevel);
			}


			if (processState(appState))
			{
				appState += 1;
				if (appState > MOADStates.moadState_last) appState = MOADStates.moadState_last;
				unloadAll();
				loadState(appState);
			}
		}

		bool processState(MOADStates currentState)
		{
			if(appTimer >= 0) appTimer -= 1;

			switch (currentState)
			{
				case (MOADStates.moadState_first):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().bannerState == BannerState.shown && stateTracker == false)
					{
						stateTracker = true;
						gameObject_messageBanner.GetComponent<MessageBanner>().setText("\"Touch is the first human sense\"", true, textSpeed);
					}
					break;
				case (MOADStates.moadState_welcome):
					if (appTimer == 0) return true;

					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && appTimer == -1)
					{
						appTimer = (int)(6 * (1 / Time.deltaTime));
					}
					break;
				case (MOADStates.moadState_9D_intro):
					if (appTimer == 0) return true;
					
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && appTimer == -1)
					{
						appTimer = (int)(6 * (1 / Time.deltaTime));
					}
					break;
				case (MOADStates.moadState_9D_intro2):
					if (appTimer == 0) return true;

					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && appTimer == -1)
					{
						appTimer = (int)(6 * (1 / Time.deltaTime));
					}

					break;
				case (MOADStates.moadState_9D_intro3):
					if (appTimer == 0) return true;

					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && appTimer == -1)
					{
						appTimer = (int)(6 * (1 / Time.deltaTime));
					}

					break;
				case (MOADStates.moadState_9D_intro4):
					if (appTimer == 0) return true;

					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && appTimer == -1)
					{
						appTimer = (int)(6 * (1 / Time.deltaTime));
					}

					break;
				case (MOADStates.moadState_tracking):
					if (appTimer == 0)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
					}
					//if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();

					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && appTimer == -1)
					{
						appTimer = (int)(2 * (1 / Time.deltaTime));
					}
					

					Vector3 position = GameObject.Find("cursor1").transform.GetChild(0).position;
					position.x += 0.9f;
					position.y += 0.15f;
					gameObject_cursorLabel.transform.position = position;

					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_trackingXYZ):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_trackingRPY):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_force):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_forceSolid):
					if(appTimer == 0)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
					}
					if(gameObject_hardBall.GetComponent<Senmag_interactionTools>().touched == true && appTimer == -1 && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false)
					{
						appTimer = (int)(3 * (1 / Time.deltaTime));
					}
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					

					break;

				case (MOADStates.moadState_forceHelensSponge):
					if (appTimer == 0)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
					}
					if (gameObject_softBall.GetComponent<Senmag_interactionTools>().touched == true && appTimer == -1 && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false)
					{
						appTimer = (int)(3 * (1 / Time.deltaTime));
					}
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					
					break;

				case (MOADStates.moadState_textures):
					if (appTimer == 0)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
					}
					if (gameObject_softBall.GetComponent<Senmag_interactionTools>().touched == true && gameObject_hardBall.GetComponent<Senmag_interactionTools>().touched && appTimer == -1 && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false)
					{
						appTimer = (int)(3 * (1 / Time.deltaTime));
					}
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					
					break;

				case (MOADStates.moadState_weight):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_weightLight):
					if (appTimer == 0)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
					}
					if (gameObject_lightSwingball.transform.GetChild(0).GetComponent<Senmag_interactionTools>().touched == true && appTimer == -1 && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false)
					{
						appTimer = (int)(3 * (1 / Time.deltaTime));
					}
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					
					break;

				case (MOADStates.moadState_weightHeavy):
					if (appTimer == 0)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
					}
					if (gameObject_heavySwingball.transform.GetChild(0).GetComponent<Senmag_interactionTools>().touched == true && appTimer == -1 && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false)
					{
						appTimer = (int)(3 * (1 / Time.deltaTime));
					}
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					
					break;

				/*case (MOADStates.moadState_invisibleIntro):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_invisibleTask):
					if(gameObject_invisible.transform.GetChild(0).transform.GetChild(0).GetComponent<interactionTools>().touched == true)
					{
						gameObject_buttonNext.GetComponent<Senmag_button>().show();
						gameObject_invisible.transform.GetChild(0).transform.GetChild(0).GetComponent<interactionTools>().touched = false;
					}
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;*/
				case (MOADStates.moadState_applicationsSpiel):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;
				case (MOADStates.moadState_uselessMachineIntro):
					if (gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;
				case (MOADStates.moadState_uselessMachine):
					if (gameObject_uselessMachine.transform.GetChild(0).GetComponent<uselessMachine>().boxDestroyed == true) return true;
					break;

				case (MOADStates.moadState_cleanup):
					if(gameObject_messageBanner.GetComponent<MessageBanner>().animationFinished == true && gameObject_buttonNext.GetComponent<Senmag_button>().isEnabled == false) gameObject_buttonNext.GetComponent<Senmag_button>().show();
					if (gameObject_buttonNext.GetComponent<Senmag_button>().wasClicked() == true) return true;
					break;

				case (MOADStates.moadState_conclusion):
					break;

				case (MOADStates.moadState_last):
					break;
			}
			return false;
		}

		void unloadAll()
		{
			stateTracker = false;
			gameObject_buttonNext.GetComponent<Senmag_button>().hide();

			if(gameObject_cursorLabel.GetComponent<MessageBanner>().bannerState != BannerState.hidden) gameObject_cursorLabel.GetComponent<MessageBanner>().hideBanner();
			Vector3 tmp = gameObject_uselessMachine.transform.position;
			tmp.x += 10;
			gameObject_uselessMachine.transform.position = tmp;

			tmp = gameObject_hardBall.transform.position;
			tmp.x += 15;
			gameObject_hardBall.transform.position = tmp;

			tmp = gameObject_softBall.transform.position;
			tmp.x += 20;
			gameObject_softBall.transform.position = tmp;
			gameObject_softBall.SetActive(false);

			tmp = gameObject_lightSwingball.transform.position;
			tmp.x += 25;
			gameObject_lightSwingball.transform.position = tmp;

			tmp = gameObject_heavySwingball.transform.position;
			tmp.x += 30;
			gameObject_heavySwingball.transform.position = tmp;

			tmp = gameObject_invisible.transform.position;
			tmp.x += 35;
			gameObject_invisible.transform.position = tmp;
		}

		void loadState(MOADStates newState)
		{
			appTimer = -1;
			camera_main.SetActive(true);
			camera_aux.SetActive(false);
			switch (newState)
			{
				case (MOADStates.moadState_first):
					
					break;

				case (MOADStates.moadState_welcome):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Welcome to our 9D interaction Demo!\n\r", true, textSpeed);
					break;
				case (MOADStates.moadState_9D_intro):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Many companies are developing VR and AR headsets\n\rthat can make the metaverse look realistic,\n\r                                                                                                                                                                                                                                                                                                                                     \n\rBut the metaverse is vapour\n\rthere are no suitable solutions to interact with it", true, textSpeed);
					break;

				case (MOADStates.moadState_9D_intro2):
					//gameObject_messageBanner.GetComponent<MessageBanner>().setText("", true, textSpeed);
					//gameObject_messageBanner.GetComponent<MessageBanner>().setText("Touch is often described as the first human sense.\n\r\n\rWe believe touch will be crucial for widespread \n\radoption of Mixed reality platforms ", true, textSpeed);
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Without natrual interaction, without touch, \n\r                                                                                                                                                                                                                                                                                                             \n\rthe metaverse will continue to struggle to attract \n\rwidespread adoption", true, textSpeed);
					break;

				case (MOADStates.moadState_9D_intro3):
					//gameObject_messageBanner.GetComponent<MessageBanner>().setText("", true, textSpeed);
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("The robot you are using is our prototype to support \n\rubiquitous interaction for 3D computer programs. \n\r                                                                                                                                                                                                                                       \n\rit is a \"Keyboard and mouse\" for the metaverse.", true, textSpeed);
					break;

				case (MOADStates.moadState_9D_intro4):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("You may have tried VR before, and have an idea\n\r what it can look like,\n\r                                                                                                                                                                                                                                                   \n\rToday, we will show you what it can feel like.", true, textSpeed);
					break;

				case (MOADStates.moadState_tracking):                                                                                                       
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Try moving your hand a bit.\n\r                                                                                                                                                                                                                              \n\rThe small ball is your virtual finger!", true, textSpeed);
					gameObject_cursorLabel.GetComponent<MessageBanner>().showBanner();
					GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().destroyCustomCursor();
					break;

				case (MOADStates.moadState_trackingXYZ):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Our robot tracks your position in six dimensions,\n\r                                                                                                                                                                                                                        \n\rTranslational (X, Y, Z)", true, textSpeed);

					GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().createCustomCursor(prefab_cursorXYZ);
					break;

				case (MOADStates.moadState_trackingRPY):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("And rotational (R P Y)", true, textSpeed);
					GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().createCustomCursor(prefab_cursorXYZ);
					
					break;

				case (MOADStates.moadState_force):
					GameObject.Find("cursor1").GetComponent<Senmag_HapticCursor>().destroyCustomCursor();
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Our robot also pushes back in 3D (X Y Z)\n\r                                                                                                                                                                                                                        \n\rWith some clever programming,\n\rwe can make the metaverse tangible...", true, textSpeed);
					break;

				case (MOADStates.moadState_forceSolid):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("We can program how things feel\n\r                                                                                                                                                                                                                       \n\rWe can make this sphere feel hard...", true, textSpeed);
					gameObject_softBall.GetComponent<Senmag_interactionTools>().touched = false;
					gameObject_hardBall.GetComponent<Senmag_interactionTools>().touched = false;
					gameObject_hardBall.transform.position = startPosition_hardBall;
					break;

				case (MOADStates.moadState_forceHelensSponge):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("and this sphere feel soft...", true, textSpeed);
					gameObject_softBall.GetComponent<Senmag_interactionTools>().touched = false;
					gameObject_hardBall.GetComponent<Senmag_interactionTools>().touched = false;
					gameObject_hardBall.transform.position = startPosition_hardBall;
					gameObject_softBall.transform.position = startPosition_softBall;
					gameObject_softBall.SetActive(true);
					break;

				case (MOADStates.moadState_textures):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("We can also simulate textures, \n\r                                                                                                                                                                                                                     \n\rCan you feel the difference between the two spheres?", true, textSpeed);
					gameObject_softBall.GetComponent<Senmag_interactionTools>().touched = false;
					gameObject_hardBall.GetComponent<Senmag_interactionTools>().touched = false;
					gameObject_hardBall.transform.position = startPosition_hardBall;
					gameObject_softBall.transform.position = startPosition_softBall;
					gameObject_softBall.SetActive(true);
					break;

				case (MOADStates.moadState_weight):
					//gameObject_buttonNext.GetComponent<Senmag_button>().show();
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Our desk-mounted design may look a bit funny, but it\n\rlets us simulate a greater of physical properties\n\r                                                                                                                                                                                                                  \n\rone is weight...", true, textSpeed);
					break;

				case (MOADStates.moadState_weightLight):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("So we can make things feel light...", true, textSpeed);
					gameObject_lightSwingball.transform.position = startPosition_lightSwingball;
					break;

				case (MOADStates.moadState_weightHeavy):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("or heavy...", true, textSpeed);
					gameObject_lightSwingball.transform.position = startPosition_lightSwingball;
					gameObject_heavySwingball.transform.position = startPosition_heavySwingball;
					break;

				/*case (MOADStates.moadState_invisibleIntro):
					//gameObject_buttonNext.GetComponent<Senmag_button>().show();
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Haptic feedback can let you interact with an object \n\r without needing to see it!\n\r", true, textSpeed);
					break;

				case (MOADStates.moadState_invisibleTask):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Try tracing the invisible path inside the box!", true, textSpeed);
					gameObject_invisible.transform.position = startPosition_invisible;
					gameObject_invisible.transform.GetChild(0).transform.GetChild(0).GetComponent<interactionTools>().touched = false;
					break;*/
				case (MOADStates.moadState_applicationsSpiel):
					//gameObject_messageBanner.GetComponent<MessageBanner>().setText("We believe 9D interaction will empower a range\n\rof applications, Including: 3D design, Tele-presence,\n\r Entertainment, Training, and\n\rSocial / collaborative platforms!", true, textSpeed);
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("We believe 9D interaction will empower a range\n\rof applications, Including: 3D design, Tele-presence,\n\r Entertainment, Training, and\n\rSocial / collaborative platforms!", true, textSpeed);
					break;
				case (MOADStates.moadState_uselessMachineIntro):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("But for techxpo, we've made the following fun\n\r demo to show the unique capabilities of 9D interaction!\n\r                                                                                                                                                                                                                        \n\r enjoy!", true, textSpeed);
					break;
				case (MOADStates.moadState_uselessMachine):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Legends speak of an ancient guardian who \n\rprotects the switch...\n\r\n\rThose who seek to disturb the switch, beware...", true, textSpeed) ;
					gameObject_uselessMachine.transform.position = startPosition_uselessMachine;
					if (hideSurprises == true)
					{
						camera_main.SetActive(false);
						camera_aux.SetActive(true);
					}
					break;

				case (MOADStates.moadState_cleanup):
					if (gameObject_uselessMachine.transform.GetChild(0).GetComponent<uselessMachine>().boxDestroyed == false) gameObject_uselessMachine.transform.GetChild(0).GetComponent<uselessMachine>().destroyBox();
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Congratulations, you have slain the ancient guardian!\n\r                       \n\r(and also ensured none will disturb its switch again...)", true, textSpeed);
					gameObject_uselessMachine.transform.position = startPosition_uselessMachine;
					
					break;

				case (MOADStates.moadState_conclusion):
					gameObject_messageBanner.GetComponent<MessageBanner>().setText("Thanks for trying our demo!\n\r\n\rWe are currently seeking early adopters who want to \n\rbe among the first to work with 9D interaction!", true, textSpeed);
					break;

				case (MOADStates.moadState_last):
					gameObject_messageBanner.GetComponent<MessageBanner>().hideBanner();
					
					break;

			}
		}

	}
}
