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

public class TouchTut : MonoBehaviour
{
    #region PERSISTENT SINGLETON STUFF

    //Private reference only this class can access
    private static TouchTut instance;

    //Public reference the other classes will use
    public static TouchTut Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = GameObject.FindObjectOfType<TouchTut>();

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
        width = Screen.width / 5;
        centerY = 5 + (width / 2);
        centerX = Screen.width - centerY;
        guiTexture.pixelInset = new Rect(centerX - width / 2, centerY - width / 2, width, width);
    }

    #endregion


    #region FIELDS AND PROPERTIES

    public Color originalColor;
    public GameObject trailPrefab;

    private const float tutTime = 5f;
    private int
        width,
        centerX,
        centerY;
    private delegate void VisualBehaviour();
    private VisualBehaviour visBehaviour;
    private float smoothing = 0.75f;
    private Color destColor;
    private int destWidth;
    private PlayerControl playerControl;
    private bool isFading = true;
    private bool isTouching = false;
    public bool IsTouching
    {
        get { return isTouching; }
    }
    private GameObject trail;

    #endregion


    void Start()
    {
        guiTexture.color = Color.clear;
        width = Screen.width / 10;
        guiTexture.pixelInset = new Rect(centerX - width / 2, centerY - width / 2, width, width);
        destWidth = width;
    }

    public void Show()
    {
        gameObject.SetActive(true);  //Ensure the GameObject is enabled

        //visBehaviour = Pulsate;  //The initial visual behaviour is to pulsate

        //TouchInputManager.OnHold += WaitForFirstTouch;
        //TouchInputManager.OnDrag += WaitForFirstTouch;
        //TouchInputManager.OnTouchUp += OnTouchUpHandler;

        StartCoroutine(FadeIn());  //Fade in effect
    }

    public void ShowLoad()
    {
        gameObject.SetActive(true);

        TouchInputManager.OnHold += OnTouchingHandler;
        TouchInputManager.OnDrag += OnTouchingHandler;
        TouchInputManager.OnTouchUp += OnTouchUpHandler;
    }

    public void HideLoad()
    {
        TouchInputManager.OnHold -= OnTouchingHandler;
        TouchInputManager.OnDrag -= OnTouchingHandler;
        TouchInputManager.OnTouchUp -= OnTouchUpHandler;

        gameObject.SetActive(false);
    }

    void OnDisable()
    {
        TouchInputManager.OnHold -= OnTouchingHandler;
        TouchInputManager.OnDrag -= OnTouchingHandler;
        TouchInputManager.OnTouchUp -= OnTouchUpHandler;
    }

    IEnumerator FadeIn()
    {
        isFading = true;
        destColor = originalColor;
        destColor.a = 0.25f;
        while (Mathf.Abs(guiTexture.color.a - destColor.a) > 0.01f)  //The fade effect
        {
            guiTexture.color = Color.Lerp(guiTexture.color, destColor, smoothing * Time.deltaTime);
            yield return 0;
        }
        //destColor = guiTexture.color;
        guiTexture.color = destColor;
        destColor = originalColor;
        destWidth = Screen.width / 5;
        isFading = false;

        visBehaviour = Pulsate;  //The initial visual behaviour is to pulsate

        TouchInputManager.OnHold += WaitForFirstTouch;
        TouchInputManager.OnDrag += WaitForFirstTouch;
    }

    void Update()
    {
        if (isFading)
            return;

        if (visBehaviour != null)
            visBehaviour();
    }

    void Pulsate()
    {
        //Make a pulsing animation to catch the player's attention and induce him/her to touch the tutorial
        width = (int)Mathf.Lerp(Screen.width / 10, Screen.width / 6, Mathf.PingPong(Time.time, 1f) / 1f);
        guiTexture.pixelInset = new Rect(centerX - width / 2, centerY - width / 2, width, width);
    }

    void WaitForFirstTouch(Touch touch)
    {
        if (guiTexture.HitTest(touch.position))  //Check if the touch was inside the tutorial
        {
            visBehaviour = Smooth;  //Change to smoothly expand and shrink with touch behaviour

            //Change handlers
            TouchInputManager.OnHold -= WaitForFirstTouch;
            TouchInputManager.OnDrag -= WaitForFirstTouch;

            TouchInputManager.OnHold += OnTouchingHandler;
            TouchInputManager.OnDrag += OnTouchingHandler;
            TouchInputManager.OnTouchUp += OnTouchUpHandler;
        }
    }

    void Smooth()
    {
        if (Mathf.Abs(guiTexture.color.a - destColor.a) > 0.01f)
        {
            guiTexture.color = Color.Lerp(guiTexture.color, destColor, 4f * smoothing * Time.deltaTime);
            width = (int)Mathf.Lerp(width, destWidth, 4f * smoothing * Time.deltaTime);
            guiTexture.pixelInset = new Rect(centerX - width / 2, centerY - width / 2, width, width);
        }
    }

    void OnTouchingHandler(Touch touch)
    {
        if (guiTexture.HitTest(touch.position))  //Touching inside the tutorial
        {
            isTouching = true;
            destColor = originalColor;
            destWidth = Screen.width / 5;
        }
        else  //Touching outside the tutorial
        {
            //Change flag
            isTouching = false;
            destColor = originalColor;
            destColor.a = 0.25f;
            destWidth = Screen.width / 10;
        }
    }

    void OnTouchUpHandler(Touch touch)
    {
        //Change flag
        isTouching = false;
        destColor = originalColor;
        destColor.a = 0.25f;
        destWidth = Screen.width / 10;
    }

    public void ChangeToStep2()
    {
        TouchInputManager.OnHold -= OnTouchingHandler;
        TouchInputManager.OnDrag -= OnTouchingHandler;
        TouchInputManager.OnTouchUp -= OnTouchUpHandler;

        visBehaviour = null;

        StartCoroutine(FadeOut(() => { Destroy(gameObject); }));
        //StartCoroutine(ShootTrail());
    }

    IEnumerator ShootTrail()
    {
        float h, v;
        trail = Instantiate(trailPrefab, Vector3.zero, Quaternion.identity) as GameObject;
        trail.transform.parent = Camera.main.transform;

        for (int i = 0; i < 20; i++)
        {
            //Reset to starting position
            trail.transform.position = Camera.main.ScreenToWorldPoint(new Vector3(centerX, centerY, 10));

            //Reactivate trail
            trail.SetActive(true);

            //Randomize horizontal and vertical final coefficients
            h = Random.Range(0.5f, 5);
            v = Random.Range(0.5f, 5);

            //Translate
            for (float t = 0; t < 1.25f; t += Time.deltaTime)
            {
                trail.transform.position +=
                    (-trail.transform.right * Mathf.Lerp(1, h, t)
                    + trail.transform.up * Mathf.Lerp(1, v, t)) * Time.deltaTime;

                yield return 0;
            }

            //Deactivate trail to make it invisible until it is repositioned
            trail.SetActive(false);

            //Wait some time before shooting another trail. This wait time has to be longer than the trail lifespan
            yield return new WaitForSeconds(2f);
        }

        Destroy(trail);
        Destroy(gameObject);
    }

    IEnumerator FadeOut(CallBackMethod callback)
    {
        isFading = true;
        destColor = Color.clear;
        while (Mathf.Abs(guiTexture.color.a - destColor.a) > 0.01f)  //The fade effect
        {
            guiTexture.color = Color.Lerp(guiTexture.color, destColor, smoothing * Time.deltaTime);
            yield return 0;
        }
        guiTexture.color = destColor;
        isFading = false;

        if (callback != null)
            callback();
    }
}
