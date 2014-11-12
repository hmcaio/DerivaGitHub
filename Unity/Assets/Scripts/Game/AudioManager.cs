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


using Behaviours;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para>[Persistent Singleton]</para>
/// <para>Class to manage audio related tasks, like loading, playing and stopping audio tracks.</para>
/// <para>Also responsible for executing audio based color change algorithms and notifying any listeners.</para>
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region PERSISTENT SINGLETON STUFF

    //Private reference only this class can access
    private static AudioManager instance;

    //Public reference the other classes will use
    public static AudioManager Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = GameObject.FindObjectOfType<AudioManager>();

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

        audioTracks = new List<string>();
        foreach (string path in Constants.paths)
            audioTracks.Add(path);
    }

    #endregion


    #region FIELDS AND PROPERTIES

    private List<string> audioTracks;
    public enum AudioTracks { MENU, STAGE_1_BEGINNING, STAGE_1_MIDDLE };
    enum State { NONE, PLAYING, STOPPED, FADING_IN, FADING_OUT };
    State state;

    public float smoothing;
    //public float[] xArray;
    //public Color[] pointsList;
    public Color
        fractalLowColor = new Color(0f, 253f/256f, 221f/256f), 
        fractalHighColor = Color.white;
    public Color 
        bigFractalLowColor = new Color(82f / 256f, 109f / 256f, 200f / 256f),
        bigFractalHighColor = new Color(33f / 256f, 38f / 256f, 100f / 256f);

    private BaseBehaviour behaviour;
    private ColorBlender colorBlender1, colorBlender2;
    public delegate void OnTrackEndedEvent();
    private OnTrackEndedEvent OnTrackEnded = null;

    #endregion


    void Start()
    {
        behaviour = new Behaviour1(audio);
        colorBlender1 = new ColorBlender(behaviour, fractalLowColor, fractalHighColor, smoothing);
        colorBlender2 = new ColorBlender(behaviour, bigFractalLowColor, bigFractalHighColor, smoothing);

        //FileMan.Open(@"C:\Users\Caio\Desktop\Test.txt");
    }

    void OnDisable()
    {
        //FileMan.Close();
    }

    /// <summary>
    /// Loads the AudioCllip at runtime
    /// </summary>
    /// <param name="track">The audio asset to be loaded</param>
    /// <returns>The AudioClip loaded</returns>
    private AudioClip LoadAudioClip(AudioTracks track)
    {
        return Resources.Load<AudioClip>(Constants.AUDIO_FOLDER + audioTracks[(int)track]);
    }

    /// <summary>
    /// Unloads unused audio assets
    /// </summary>
    private void UnloadAudioClip()
    {
        //?
        Resources.UnloadUnusedAssets();
    }

    /// <summary>
    /// Starts playing an audio track. If there is a previous music playing, executes the fade out first
    /// </summary>
    /// <param name="track">The music to be played</param>
    /// <param name="loop">Wheter to loop the music</param>
    /// <param name="fadeOut">Duration of the fade out effect</param>
    /// <param name="fadeIn">Duration of the fade in effect</param>
    /// <param name="volume">Target volume</param>
    public void PlayAudio(AudioTracks track, bool loop = false, float fadeOut = 0f, float fadeIn = 0f, float volume = 1f)
    {
        StartCoroutine(StartFadeIn(track, loop, fadeOut, fadeIn, volume));
    }

    public void PlayAudio(AudioTracks track, OnTrackEndedEvent handler, float fadeOut = 0f, float fadeIn = 0f, float volume = 1f)
    {
        OnTrackEnded = handler;
        StartCoroutine(StartFadeIn(track, false, fadeOut, fadeIn, volume));
    }

    /// <summary>
    /// Stops playing audio
    /// </summary>
    /// <param name="fadeOut">Duration of the fade out effect</param>
    public void StopAudio(float fadeOut = 0f)
    {
        StartCoroutine(StartFadeOut(fadeOut));
    }

    /// <summary>
    /// "Asynchronous" audio fade in effect. If there is a previous music playing, executes the fade out first
    /// </summary>
    /// <param name="track">The music to be played</param>
    /// <param name="loop">Whether to loop the music</param>
    /// <param name="fadeOut">Duration of previous music fade out effect</param>
    /// <param name="time">Duration of the fade in effect</param>
    /// <param name="finalVolume">Target volume</param>
    /// <returns></returns>
    public IEnumerator StartFadeIn(AudioTracks track, bool loop, float fadeOut, float time, float finalVolume)
    {
        //Wait for the fade out if there is any music playing
        yield return StartCoroutine(StartFadeOut(fadeOut));

        //Load at runtime the AudioClip, adjust audio settings and play audio
        audio.clip = LoadAudioClip(track);
        audio.loop = loop;
        audio.volume = 0f;
        finalVolume = Mathf.Clamp(finalVolume, 0f, 1f);
        audio.Play();
        UnloadAudioClip();

        state = State.FADING_IN;  //Change the state of the manager
        if (time > 0)  //The fade effect
        {
            float rate = finalVolume / time;
            for (float timer = 0f; timer < time; timer += Time.deltaTime)
            {
                audio.volume += rate * Time.deltaTime;
                yield return 0;
            }
        }
        audio.volume = finalVolume;
        state = State.PLAYING;  //Change the state of the manager
    }

    /// <summary>
    /// "Asynchronous" audio fade out effect
    /// </summary>
    /// <param name="time">Duration of the fade out effect</param>
    /// <returns></returns>
    public IEnumerator StartFadeOut(float time)
    {
        state = State.FADING_OUT;  //Change the state of the manager
        if (time > 0)  //The fade effect
        {
            float rate = audio.volume / time;
            for (float timer = 0f; timer < time; timer += Time.deltaTime)
            {
                audio.volume -= rate * Time.deltaTime;
                yield return 0;
            }
        }
        audio.volume = 0f;
        audio.Stop();
        state = State.STOPPED;  //Change the state of the manager
    }

    void Update()
    {
        switch (state)
        {
            case State.NONE:
                break;
            case State.PLAYING:
                //Get information from the audio, do some calculation and launch events to update all listeners' colors
                behaviour.CalcValue();

                //For one shots, check if music is over to launch event
                if (OnTrackEnded != null)
                {
                    if (!audio.isPlaying)
                    {
                        OnTrackEnded();
                        OnTrackEnded = null;  //Disconnect listener
                    }
                }

                break;
            case State.STOPPED:
                break;
            case State.FADING_IN:
                break;
            case State.FADING_OUT:
                break;
            default:
                break;
        }
    }

    public void AddListener(IColorListener listener, int n)
    {
        if (n == 0)
        {
            colorBlender1.OnColorUpdated += listener.OnColorUpdatedHandler;
        }
        else if (n == 1)
        {
            colorBlender2.OnColorUpdated += listener.OnColorUpdatedHandler;
        }
    }

    public void RemoveListener(IColorListener listener, int n)
    {
        if (n == 0)
        {
            colorBlender1.OnColorUpdated -= listener.OnColorUpdatedHandler;
        }
        else if (n == 1)
        {
            colorBlender2.OnColorUpdated -= listener.OnColorUpdatedHandler;
        }
    }
}
