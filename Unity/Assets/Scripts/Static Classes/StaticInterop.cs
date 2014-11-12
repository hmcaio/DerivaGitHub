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


using System;
#if UNITY_WP8
using UnityEngine.Windows;
#endif

public static class StaticInterop
{
    public static event EventHandler SaveSSToLibrary;
    public delegate void PostEvent(byte[] bytes);
    public static PostEvent OnPost;
    public static event EventHandler OnExit;
    public delegate void VibrateEvent(int t);
    public static event VibrateEvent OnVibrate;

    public static string imageTitle;
    public static byte[] bytes;

    //This bool is needed so that the event handlers don't get assigned more than once in the WP8 app side!
    private static bool isFirstTime = true;
    public static bool IsFirstTime
    {
        get { return isFirstTime; }
        set { isFirstTime = value; }
    }

    /// <summary>
    /// Called from Unity when the app is responsive and ready for play, picked up by the app
    /// </summary>
    public static Action UnityLoaded;


    static StaticInterop()
    {
        //Create blank implementations to avoid errors within editor (?)
        UnityLoaded = delegate { };
    }

    public static void FireSaveSSToLibrary(string title)
    {
#if !UNITY_EDITOR
#if UNITY_WP8

        if (SaveSSToLibrary != null)
        {
            imageTitle = title;
            //bytes = bytesParam;

            if (bytes != null)
                SaveSSToLibrary(null, null);
        }

#elif UNITY_ANDROID

        //TODO

#endif
#endif
    }

    public static void FirePostScreenshot()
    {

#if !UNITY_EDITOR
    #if UNITY_WP8

        if (OnPost != null)
        {
            OnPost(bytes);
        }

    #elif UNITY_ANDROID

        //TODO

    #endif
#endif
    }

    public static void FireExit()
    {
#if !UNITY_EDITOR
    #if UNITY_WP8

            if (OnExit != null)
            {
                OnExit(null, null);
            }

    #elif UNITY_ANDROID

        //TODO

    #endif
#endif
    }

    public static void FireVibrate(int milliseconds)
    {
#if !UNITY_EDITOR
    #if UNITY_WP8

        if (OnVibrate != null)
            OnVibrate(milliseconds);

    #elif UNITY_ANDROID
        
        //TODO

    #endif
#endif
    }
}
