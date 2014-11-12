using UnityEngine;
using System.Collections;

// Attach this to a GUIText to make a frames/second indicator.
//
// It calculates frames/second over each updateInterval,
// so the display does not keep changing wildly.
//
// It is also fairly accurate at very low FPS counts (<10).
// We do this not by simply counting frames per interval, but
// by accumulating FPS for each frame. This way we end up with
// correct overall FPS even if the interval renders something like
// 5.5 frames.
public class FPSViewer : MonoBehaviour
{
    public float updateInterval = 0.5F;

    private float accum = 0; // FPS accumulated over the interval
    private int frames = 0; // Frames drawn over the interval
    private float timeleft; // Left time for current interval

    #region PERSISTENT SINGLETON STUFF

    //Private reference only this class can access
    private static FPSViewer instance;

    //Public reference the other classes will use
    public static FPSViewer Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = GameObject.FindObjectOfType<FPSViewer>();

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
    }

    #endregion


    void Start()
    {
        if (!guiText)
        {
            Debug.Log("UtilityFramesPerSecond needs a GUIText component!");
            enabled = false;
            return;
        }
        timeleft = updateInterval;
    }

    void Update()
    {
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        // Interval ended - update GUI text and start new interval
        if (timeleft <= 0.0)
        {
            // display two fractional digits (f2 format)
            float fps = accum / frames;
            string format = System.String.Format("{0:F2} FPS", fps);
            guiText.text = format;

            if (fps < 30)
                guiText.material.color = Color.yellow;
            else
                if (fps < 10)
                    guiText.material.color = Color.red;
                else
                    guiText.material.color = Color.green;
            //	DebugConsole.Log(format,level);
            timeleft = updateInterval;
            accum = 0.0F;
            frames = 0;
        }
    }
}