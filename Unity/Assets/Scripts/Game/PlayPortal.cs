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

public class PlayPortal : MonoBehaviour
{
    #region FIELDS AND PROPERTIES

    public Light pointLight, spotLight;
    public Color colorA, colorB;
    public float spotMinIntensity;
    public float t = 1f;

    private float p;

    #endregion


    void Update()
    {
        p = Mathf.PingPong(Time.time, t);

        pointLight.color = Color.Lerp(colorA, colorB, p / t);

        spotLight.color = Color.Lerp(colorA, colorB, p / t);
        spotLight.intensity = spotMinIntensity + (p / t) * (8 - spotMinIntensity);
    }
}
