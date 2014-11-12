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

//#define FACEBOOK_INTEGRATION

using System.Collections;
using UnityEngine;

#if UNITY_WP8

using UnityEngine.Windows;

#elif UNITY_ANDROID

//TODO: Android reference

#endif

public class SSMenu : MonoBehaviour
{
    #region FIELDS AND PROPERTIES

    public Texture txtBG, txtBtn, txtView;
    public float showTime = 5f;

    private const int superSizeFactor = 1;

    private bool showing;
    private Rect menuBg;
    private int menuWidth;
    private int menuHeight;
    private int parcialHeight;
    private int btnWidth;
    private bool acceptInput;
    private string lastScreenshot;
    private bool isProcessing;

    #endregion


    #region PERSISTENT SINGLETON STUFF

    //Private reference only this class can access
    private static SSMenu instance;

    //Public reference the other classes will use
    public static SSMenu Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = SSMenu.FindObjectOfType<SSMenu>();

            //Tells Unity not to destroy this object when loading a new scene
            DontDestroyOnLoad(instance.gameObject);

            return instance;
        }
    }


    void Awake()
    {
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
        menuHeight = Screen.height * 9 / 40;
        menuWidth = (int)(txtBG.width * ((float)menuHeight / txtBG.height));  //Make witdh proportional to height
        parcialHeight = menuHeight * 2 / 3;
        btnWidth = parcialHeight - 15;

        DisableMenu();
    }

    #endregion


    public void EnableMenu()
    {
        menuBg = new Rect((Screen.width - menuWidth) / 2, -parcialHeight, menuWidth, menuHeight);
        showing = true;
        acceptInput = true;

        gameObject.SetActive(true);
        TouchInputManager.OnSwipe += ShowSSMenu;
    }

    public void DisableMenu()
    {
        TouchInputManager.OnSwipe -= ShowSSMenu;
        gameObject.SetActive(false);
    }

    void OnGUI()
    {
        if (!showing)
            return;

        //Draw menu background
        GUI.DrawTexture(menuBg, txtBG);

        //Screenshot button
        if (!isProcessing)
        {
            if (GUI.Button(new Rect(15, menuBg.y + 5, btnWidth, btnWidth), txtBtn))
            {
                StartCoroutine(TakeScreenshot());
            }
        }

#if FACEBOOK_INTEGRATION

        if (!string.IsNullOrEmpty(lastScreenshot))
        {
            //Facebook button
            if (GUI.Button(new Rect(Screen.width - btnWidth - 15, menuBg.y + 5, btnWidth, btnWidth), txtView))
            {
                StartCoroutine(PostInFacebook());
            }
        }

#endif
    }

    void ShowSSMenu(Touch startTouch, Touch endTouch, Swipe direction = Swipe.NONE)
    {
        //Only a swipe down from the top of the screen calls the menu
        if ((!direction.Equals(Swipe.DOWN)) || (startTouch.position.y < Screen.height * 0.75f))
            return;

        //If the menu is already visible ignore input
        if (!acceptInput)
            return;

        StartCoroutine(Show());  //Slide menu down
        StartCoroutine(Timer());  //Start the timer
    }

    IEnumerator Show()
    {
        //showing = true;
        acceptInput = false;

        //Slide the menu down
        while (menuBg.y < -1)
        {
            menuBg.y = Mathf.Lerp(menuBg.y, 0, 5 * Time.deltaTime);
            yield return 0;
        }
        menuBg.y = 0;
    }

    IEnumerator Hide()
    {
        //Slide the menu up
        while (menuBg.y > -parcialHeight + 1)
        {
            menuBg.y = Mathf.Lerp(menuBg.y, -parcialHeight, 6 * Time.deltaTime);
            yield return 0;
        }
        menuBg.y = -parcialHeight;

        //showing = false;
        acceptInput = true;
    }

    IEnumerator Timer()
    {
        for (float t = 0; t < showTime; t += Time.deltaTime)
            yield return 0;

        StartCoroutine(Hide());
    }

    IEnumerator TakeScreenshot()
    {
#if !UNITY_EDITOR
#if UNITY_WP8

        //Set flag so the screenshot button remains disabled until this process is over
        isProcessing = true;

        //Hide menu temporarily
        showing = false;
        yield return 0;

        //Set name of the screenshot
        lastScreenshot = string.Format("Deriva{0}_{1}_{2}_{3}_{4}_{5}", new object[] { System.DateTime.Now.Year, System.DateTime.Now.Month, System.DateTime.Now.Day, System.DateTime.Now.Hour, System.DateTime.Now.Minute, System.DateTime.Now.Second});
        //Take screenshot
        Application.CaptureScreenshot(lastScreenshot + ".png", superSizeFactor);

        yield return 0;  //Distribute process in parts

        showing = true;  //Show menu again

        yield return new WaitForSeconds(1f);  //Wait to make sure screenshot was captured

        //Rotate screenshot 90 degrees anti-clockwise (portrait to landscape)

        //Create an auxiliary texture
        Texture2D txt = new Texture2D(Screen.height, Screen.width, TextureFormat.ARGB32, false);
        //Load screenshot from memory
        txt.LoadImage(File.ReadAllBytes(Application.dataPath + "/" + lastScreenshot + ".png"));
        //This array represents the pixels of the portrait image
        Color[] src = txt.GetPixels();

        yield return 0;  //Distribute process in parts

        //This array will represent the pixels of the resulting landscape image
        Color[] dest = new Color[src.Length];

        //Fill the destiny array with the swapped values
        int n = txt.width, m = txt.height;
        int i, j;
        for (int k = 0; k < src.Length; k++)
        {
            i = n - 1 - (k / m);
            j = k % m;
            dest[k] = src[n * (m - 1 - j) + n - i - 1];
        }

        yield return 0;  //Distribute process in parts

        //Apply pixel color values to texture
        if (!txt.Resize(Screen.width, Screen.height))
            StaticInterop.FireVibrate(200);
        txt.SetPixels(dest);
        txt.Apply();

        yield return 0;  //Distribute process in parts

        StaticInterop.bytes = txt.EncodeToPNG();
        StaticInterop.FireSaveSSToLibrary(lastScreenshot);  //Finally, save screenshot to device library

        //Free memory from auxiliary texture
        Resources.UnloadUnusedAssets();

        //Set flag to enable screenshot button again
        isProcessing = false;

#elif UNITY_ANDROID

        //TODO

#endif
#endif

        yield return 0;
    }

#if FACEBOOK_INTEGRATION

    private IEnumerator PostInFacebook()
    {
#if !UNITY_EDITOR
#if UNITY_WP8

        while (isProcessing)
            yield return 0;
	    
        StaticInterop.FirePostScreenshot(/*File.ReadAllBytes(Application.dataPath + "/" + lastScreenshot + ".png")*/);

#elif UNITY_ANDROID

        //TODO

#endif
#endif

        yield return 0;
    }

#endif
}