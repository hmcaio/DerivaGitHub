/*
 	Deriva
	An experimental/contemplative game

	Copyright 2014 Caio Hideki Matsumoto (hmcaio@hotmail.com)

	This file is part of Deriva.

		Deriva is free software: you can redistribute it and/or modify
		it under the terms of the GNU General Public License as published by
		the Free Software Foundation, either version 3 of the License, or
		(at your option) any later version.

		Deriva is distributed in the hope that it will be useful,
		but WITHOUT ANY WARRANTY; without even the implied warranty of
		MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
		GNU General Public License for more details.

		You should have received a copy of the GNU General Public License
		along with Deriva.  If not, see <http://www.gnu.org/licenses/>.
 */


using System.Collections;
using UnityEngine;

public delegate void CallBackMethod();

/// <summary>
/// <para>[Persistent Singleton]</para>
/// <para>Class that manages all game related tasks and events, like loading scenes and fading the screen</para>
/// </summary>
public class GameMaster : MonoBehaviour
{
    #region VARIABLES AND FIELDS

    public GUIText debugText;
    private GameObject player;
    public GameObject Player
    {
        get
        {
            return player;
        }
        set
        {
            player = value;
        }
    }
    private GameObject _mainCamera;
    public GameObject MainCamera
    {
        set
        {
            _mainCamera = value;
            if (_mainCamera != null)
                cameraScript = _mainCamera.GetComponent<CameraCollider>();
        }
    }
    public Texture2D empty, full;
    public GameObject prefabTutorial;
    public GUITexture ambient;
    public delegate void ProgressUpdatedEvent(int p);
    public ProgressUpdatedEvent OnProgressUpdated;
    public delegate void SceneLoadedEvent();
    public SceneLoadedEvent OnSceneLoaded;
    public GUITexture screenFader;

    private CameraCollider cameraScript;
    public enum GameState { NONE, MAIN_MENU, STAGE_SELECTION, SPLASH_SCREEN, PLAYING };
    public GameState gameState;
    public enum GameEvent { LOAD_LEVEL1, FREE_PLAYER, ACTIVATOR2, LEVEL_END };

    private bool isFading = false, showProgress = false;
    private AsyncOperation async = null;
    private float progress;
    private bool isFirstTime = true;  //Bool to indicate if the tutorial is needed
    public bool IsFirstTime
    {
        get { return isFirstTime; }
    }
    private bool ok;
    private Color bgColor = new Color(130f/256f, 130f/256f, 216f/256f);


    #endregion


    #region PERSISTENT SINGLETON STUFF

    //Private reference only this class can access
    private static GameMaster instance;

    //Public reference the other classes will use
    public static GameMaster Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = GameObject.FindObjectOfType<GameMaster>();

            //Tells Unity not to destroy this object when loading a new scene
            DontDestroyOnLoad(instance.gameObject);

            return instance;
        }
    }


    void Awake()
    {
        Application.targetFrameRate = 45;

        if (instance == null)
        {
            //If I am the first instance, make me the singleton
            instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            //If a singleton already exists and you find another reference in scene, destroy it!
            if (this != instance)
                Destroy(this.gameObject);
        }

        //Adapting to different screen sizes
        GUITexture[] guiTxts = gameObject.GetComponentsInChildren<GUITexture>();
        foreach (GUITexture txt in guiTxts)
        {
            txt.pixelInset = new Rect(0, 0, Screen.width, Screen.height);
        }
    }

    #endregion


    void Start()
    {
        //Always initialize the fade screen texture enabled
        screenFader.enabled = true;

        //Start playing audio through the manager
        AudioManager.Instance.PlayAudio(AudioManager.AudioTracks.MENU, true, 0f, 4f, 1f);

        StartCoroutine(ScreenFadeToClear(Constants.FADE_NORMAL));

        //Register the handler to the hardware back button event
        TouchInputManager.OnBackPressed += BackButtonPressed;

        //Tell the manager to start touch input handling
        StartCoroutine(TouchInputManager.Update());

        //Instantiate and show the touch tutorial
        Instantiate(prefabTutorial, Vector3.zero, Quaternion.identity);
        TouchTut.Instance.Show();

        gameState = GameState.MAIN_MENU;

        //Tell the application the Unity side is ready
        StaticInterop.UnityLoaded();
    }

    void OnDisable()
    {
        //"De-register" the handler from the event
        TouchInputManager.OnBackPressed -= BackButtonPressed;
    }

    /// <summary>
    /// Manages any game event raised
    /// </summary>
    /// <param name="gameEvent">The type of game event raised</param>
    public void OnGameEvent(GameEvent gameEvent)
    {
        switch (gameEvent)
        {
            case GameEvent.LOAD_LEVEL1:
                StartCoroutine(LoadScene(Constants.LEVEL_1, true, CallBackScene1));

                break;
            case GameEvent.FREE_PLAYER:
                DestroyTrack();
                if (isFirstTime)
                {
                    cameraScript.StartWidenFOV(TouchTut.Instance.ChangeToStep2);
                    isFirstTime = false;
                }
                else
                    cameraScript.StartWidenFOV();

                break;
            case GameEvent.ACTIVATOR2:
                DestroyTrack2();

                break;
            case GameEvent.LEVEL_END:
                SSMenu.Instance.DisableMenu();
                StartCoroutine(LoadScene(Constants.LEVEL_1, /*false*/true, CallBackLevelEnd, Constants.FADE_VERY_SLOW, Constants.FADE_FAST));

                break;
            default:

                break;
        }
    }

    IEnumerator LoadScene(string level, bool showProgress, CallBackMethod method)
    {
        StartCoroutine(LoadScene(level, showProgress, method, Constants.FADE_FAST, Constants.FADE_FAST));
        yield return 0;
    }

    /// <summary>
    /// "Asynchronous" method to load scenes
    /// </summary>
    /// <param name="level">The scene to be loaded</param>
    /// <param name="showProgress">Wheter to show a progress bar</param>
    /// <param name="method">The method to execute after the scene is loaded</param>
    /// <param name="fadeOutSpeed">Speed of the screen fade out effect</param>
    /// <param name="fadeInSpeed">Speed of the screen fade in effect</param>
    /// <returns></returns>
    IEnumerator LoadScene(string level, bool showProgress, CallBackMethod method, float fadeOutSpeed, float fadeInSpeed)
    {
        //TODO: Doesn't work when showProgress equals false! See OnGUI method

        //Fade screen to black
        StartCoroutine(ScreenFadeToBlack(fadeOutSpeed));

        //Wait until the fade effect is over
        while (isFading)
            yield return 0;

        //Load the loading screen
        Application.LoadLevel(Constants.LEVEL_LOADING);

        //Fade screen to clear
        StartCoroutine(ScreenFadeToClear(Constants.FADE_FAST));

        //Wait until the fade effect is over
        while (isFading)
            yield return 0;

        //this.showProgress = showProgress;
        
        async = Application.LoadLevelAdditiveAsync(level);
        ok = false;
        while (!async.isDone)
        {
            if (OnProgressUpdated != null)
                OnProgressUpdated(Mathf.CeilToInt(async.progress * 10));
            yield return 0;
        }
        OnProgressUpdated(10);  //100% loaded

        _mainCamera.SetActive(false);
        player.SetActive(false);

        while (!ok)
            yield return 0;

        if (OnSceneLoaded != null)
            OnSceneLoaded();

        _mainCamera.SetActive(true);
        player.SetActive(true);

        //Scene loaded. Start the fade in
        StartCoroutine(ScreenFadeToClear(fadeInSpeed));

        ////Wait until the fade effect is over
        //while (isFading)
        //    yield return 0;

        //Clean (?)
        Resources.UnloadUnusedAssets();

        //If there is a callback method assigned, call it
        if (method != null)
            method();
    }

    public void SetAllowSceneActivation()
    {
        //Fade screen to black
        StartCoroutine(ScreenFadeToBlack(Constants.FADE_FAST, () => { ok = true; }));
    }

    void CallBackScene1()
    {
        //Set the small depth of field
        cameraScript.SetSmallFOV();
        //Play beginning of audio
        AudioManager.Instance.PlayAudio(AudioManager.AudioTracks.STAGE_1_BEGINNING, LoopMusic, 2f, 2f, 1f);
        //Enable the menu for screenshots
        SSMenu.Instance.EnableMenu();
    }

    void LoopMusic()
    {
        //Play looping part of audio
        AudioManager.Instance.PlayAudio(AudioManager.AudioTracks.STAGE_1_MIDDLE, true, 0f, 0f, 1f);
    }

    void CallBackLevelEnd()
    {
        cameraScript.SetSmallFOV();
        SSMenu.Instance.EnableMenu();
    }

    void SetUpStraight(PlayerControl player)
    {
        player.state = PlayerControl.PlayerState.STRAIGHT;
    }

    void DestroyTrack()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(Tags.TRACK);
        foreach (GameObject obj in objs)
            Destroy(obj);
    }

    void DestroyTrack2()
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(Tags.TRACK2);
        foreach (GameObject obj in objs)
            Destroy(obj);
    }

    /// <summary>
    /// Event handler for the Back soft button
    /// </summary>
    void BackButtonPressed()
    {
        ExitGame();
    }

    public void ExitGame()
    {
        StaticInterop.FireVibrate(200);
        StaticInterop.FireExit();
        Application.Quit();
    }

    /// <summary>
    /// "Asynchronous" screen fade in effect
    /// </summary>
    /// <param name="fadeSpeed">Fade speed</param>
    /// <returns></returns>
    IEnumerator ScreenFadeToClear(float fadeSpeed, CallBackMethod method = null)
    {
        //If it's fading, wait
        while (isFading)
            yield return 0;

        float start = Time.time;  //Save the starting time
        isFading = true;  //Change the flag
        while (screenFader.color.a > 0.001f)  //The fade effect
        {
            screenFader.color = new Color(bgColor.r, bgColor.g, bgColor.b, 1f - (Mathf.Pow((Time.time - start) / fadeSpeed, 2)));
            yield return 0;
        }
        screenFader.color = Color.clear;
        screenFader.enabled = false;  //Deactivates the component
        isFading = false;  //Change the flag

        if (method != null)
            method();
    }

    /// <summary>
    /// "Asynchronous" screen fade out effect
    /// </summary>
    /// <param name="fadeSpeed">Fade speed</param>
    /// <returns></returns>
    IEnumerator ScreenFadeToBlack(float fadeSpeed, CallBackMethod method = null)
    {
        //If it's fading, wait
        while (isFading)
            yield return 0;

        float start = Time.time;  //Save the starting time
        isFading = true;  //Change the flag
        screenFader.enabled = true;  //Activates the component
        while (screenFader.color.a < 0.999f)  //The fade effect
        {
            screenFader.color = new Color(bgColor.r, bgColor.g, bgColor.b, (Mathf.Pow((Time.time - start) / fadeSpeed, 2)));
            yield return 0;
        }
        isFading = false;  //Change the flag

        if (method != null)
            method();
    }
}
