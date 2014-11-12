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


using UnityEngine;
using System.Collections;

public enum TouchType { NONE, TAP, DOUBLE_TAP, SWIPE, HOLD, DRAG };
public enum Swipe { NONE, RIGHT, LEFT, UP, DOWN, UP_RIGHT, UP_LEFT, DOWN_RIGHT, DOWN_LEFT };


/// <summary>
/// Class containing methods to manage touch input, such as tap and swipe detection
/// </summary>
public static class TouchInputManager// : MonoBehaviour
{
    public delegate void InputAction(Touch touch);
    public static event InputAction
        OnTap,
        OnDoubleTap,
        OnHold,
        OnDrag,
        OnTouchUp;

    public delegate void InputSwipeAction(Touch startTouch, Touch endTouch, Swipe direction = Swipe.NONE);
    public static event InputSwipeAction OnSwipe;

    public delegate void HardwareButtonPressed();
    public static event HardwareButtonPressed OnBackPressed;


#if UNITY_EDITOR || UNITY_STANDALONE

    static float maxDeltaTime = 0.25f;  //Maximum delta time in seconds for the movement to be considered a swipe
    static float minDeltaDistance = 0.5f;  //Minimum distance of the touch for the movement to be considered a swipe
    static Vector3 touchStartPos;
    static float touchStartTime;  //Time in seconds


    public static IEnumerator Update()
    {
        while (true)
        {
            yield return 0;
        }
    }

    /// <summary>
    /// Method to detect a swipe
    /// </summary>
    /// <returns>A value of the enum Swipe. It can be Swipe.Right or Swipe.Left in case it's a horizontal swipe, Swipe.Up or Swipe.Down in case it's a vecrtical swipe, or Swipe.None, if no swipe movement was detected</returns>
    public static Swipe SwipeDetect()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStartPos = Input.mousePosition;
            touchStartTime = Time.time;
        }
        else if (Input.GetMouseButtonUp(0))
        {
            Vector3 delta = Input.mousePosition - touchStartPos;
            float deltaTime = Time.time - touchStartTime;
            float angle = Vector3.Angle(Vector3.right, delta);

            if (deltaTime < maxDeltaTime && delta.magnitude > minDeltaDistance)
            {
                if (angle < 45)  //Swipe to the right /o/
                {
                    return Swipe.RIGHT;
                }
                else if (angle > 135)  //Swipe to the left \o\
                {
                    return Swipe.LEFT;
                }
                else
                {
                    if (delta.y > 0)  //Swipe upwards |o|
                    {
                        return Swipe.UP;
                    }
                    else  //Swipe downwards |o|
                    {
                        return Swipe.DOWN;
                    }
                }
            }
        }

        return Swipe.NONE;
    }

    //public void DetectTap()
    //{
    //    //Input.
    //}

#elif UNITY_WP8 || UNITY_ANDROID

    #region FIELDS AND PROERTIES

    static TouchType touchType, prevTouchType;
    static float maxDeltaTime = 0.25f;
    static float minSwipeDistance = Screen.width / 10;
    static float startTime, deltaTime;
    static Vector2 startPos, delta;
    static bool moved, doubleTap;
    static Touch startingTouch;

    static Touch touch;
    public static Touch CurTouch
    {
        get { return touch; }
    }

    static Touch prevTouch;
    public static Touch PrevTouch
    {
        get { return prevTouch; }
    }

    public static TouchType CurTouchType
    {
        get { return touchType; }
    }

    public static TouchType PrevTouchType
    {
        get { return prevTouchType; }
    }

    static bool isTouching;
    public static bool IsTouching
    {
        get { return isTouching; }
    }

    #endregion


    /// <summary>
    /// In every Update step the Input is checked, verifying any touch and its type
    /// </summary>
    public static IEnumerator Update()
    {
        while (true)
        {
            if (Input.GetKey(KeyCode.Escape))
            {
                if (OnBackPressed != null)  //If there is anyone listening to this event, fire the event
                    OnBackPressed();
            }

            prevTouch = touch;
            prevTouchType = touchType;
            isTouching = Input.touchCount > 0;

            switch (Input.touchCount)
            {
                case -1:  //Don't know if it ever gets here...
                    touchType = TouchType.NONE;

                    break;
                case 0:
                    touchType = TouchType.NONE;

                    break;
                case 1:  //If there's a single finger touching the screen
                    touch = Input.GetTouch(0);
                    switch (touch.phase)
                    {
                        case TouchPhase.Began:
                            startingTouch = touch;
                            startPos = touch.position;  //Save the starting position of the touch
                            startTime = Time.time;  //Save the starting time of the touch
                            if (touch.tapCount == 2)
                                doubleTap = true;

                            break;
                        case TouchPhase.Ended:
                            if (doubleTap)
                            {
                                touchType = TouchType.DOUBLE_TAP;
                                doubleTap = false;
                                if (OnDoubleTap != null)  //If there is anyone listening to this event, fire the event
                                    OnDoubleTap(touch);
                            }
                            else
                            {
                                delta = touch.position - startPos;
                                deltaTime = Time.time - startTime;

                                if (deltaTime < maxDeltaTime)  //Short touches: tap, swipe...
                                {
                                    if (moved)  //It could be a tap or a swipe
                                    {
                                        if (delta.magnitude >= minSwipeDistance)  //Swipe
                                        {
                                            touchType = TouchType.SWIPE;
                                            if (OnSwipe != null)  //If there is anyone listening to this event, fire the event
                                                OnSwipe(startingTouch, touch, SwipeDetect(true));
                                        }
                                        else  //Tap
                                        {
                                            touchType = TouchType.TAP;
                                            if (OnTap != null)  //If there is anyone listening to this event, fire the event
                                                OnTap(touch);
                                        }

                                        moved = false;  //Clear the flag
                                    }
                                    else  //It is a tap
                                    {
                                        touchType = TouchType.TAP;
                                        if (OnTap != null)  //If there is anyone listening to this event, fire the event
                                            OnTap(touch);
                                    }
                                }
                                else  //Long touches: hold, drag...
                                {
                                    //TODO: Currently not handling drag...
                                    touchType = TouchType.HOLD;
                                    if (OnHold != null)  //If there is anyone listening to this event, fire the event
                                        OnHold(touch);
                                }
                            }
                            
                            if (OnTouchUp != null)  //If there is anyone listening to this event, fire the event
                                OnTouchUp(touch);
                            
                            startingTouch = new Touch();

                            break;
                        case TouchPhase.Moved:
                            moved = true;
                            touchType = TouchType.DRAG;
                                if (OnDrag != null)  //If there is anyone listening to this event, fire the event
                                    OnDrag(touch);

                            break;
                        case TouchPhase.Stationary:
                            if (Time.time - startTime > maxDeltaTime)
                            {
                                touchType = TouchType.HOLD;
                                if (OnHold != null)  //If there is anyone listening to this event, fire the event
                                    OnHold(touch);
                            }

                            break;
                        default:
                            break;
                    }

                    break;
                default:

                    break;
            }

            yield return 0;  //Wait for next frame
        }
    }

    /// <summary>
    /// Method that returns the type of touch detected
    /// </summary>
    /// <returns>A <code>TouchType</code> value representing the type of touch detected</returns>
    public static TouchType TouchDetection()
    {
        return touchType;
    }

    ///// <summary>
    ///// Method that returns a Touch instance with information of the tap detected
    ///// </summary>
    ///// <returns>An instance of <code>Touch</code></returns>
    //public static Touch TapDetect()
    //{
    //    if (touchType == TouchType.TAP)
    //        return touch;

    //    //throw new TouchException();
    //    return touch;
    //}

    /// <summary>
    /// Method to detect the direction of a swipe movement
    /// </summary>
    /// <param name="diagonals">Set this bool to true to also detect the diagonal directions. If set to false, only the 4 main directions will be considered</param>
    /// <returns></returns>
    public static Swipe SwipeDetect(bool diagonals = false)
    {
        float angle = Vector2.Angle(Vector2.right, delta);

        if (deltaTime < maxDeltaTime && delta.magnitude > minSwipeDistance)
        {
            if (angle < 45)  //Swipe to the right /o/
            {
                if (diagonals)
                {
                    if (angle < 22.5f)
                        return Swipe.RIGHT;
                    else
                    {
                        if (delta.y > 0)
                            return Swipe.UP_RIGHT;
                        else
                            return Swipe.DOWN_RIGHT;
                    }    
                }
                else
                    return Swipe.RIGHT;
            }
            else if (angle > 135)  //Swipe to the left \o\
            {
                if (diagonals)
                {
                    if (angle > 157.5f)
                        return Swipe.LEFT;
                    else
                    {
                        if (delta.y > 0)
                            return Swipe.UP_LEFT;
                        else
                            return Swipe.DOWN_LEFT;
                    }
                }
                else
                    return Swipe.LEFT;
            }
            else
            {
                if (delta.y > 0)  //Swipe upwards |o|
                {
                    if (diagonals)
                    {
                        if (angle < 67.5f)
                            return Swipe.UP_RIGHT;
                        else if (angle > 112.5f)
                            return Swipe.UP_LEFT;
                        else
                            return Swipe.UP;
                    }
                    else
                        return Swipe.UP;
                }
                else  //Swipe downwards |o|
                {
                    if (diagonals)
                    {
                        if (angle < 67.5f)
                            return Swipe.DOWN_RIGHT;
                        else if (angle > 112.5f)
                            return Swipe.DOWN_LEFT;
                        else
                            return Swipe.DOWN;
                    }
                    else
                        return Swipe.DOWN;
                }
            }
        }

        return Swipe.NONE;
    }

    //public static Touch HoldDetect()
    //{
    //    if (touchType == TouchType.HOLD)
    //        return touch;

    //    return touch;
    //}

    //public static Touch DragDetect()
    //{
    //    return touch;
    //}

#endif
}

/*public class TouchException : UnityException
{
    public TouchException()
    {

    }
}*/
