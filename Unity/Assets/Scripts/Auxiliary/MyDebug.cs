﻿/*
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

public class MyDebug : MonoBehaviour
{
    #region STANDARD SINGLETON STUFF

    //Private reference only this class can access
    private static MyDebug instance;

    //Public reference the other classes will use
    public static MyDebug Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = GameObject.FindObjectOfType<MyDebug>();

            return instance;
        }
    }

    #endregion
}
