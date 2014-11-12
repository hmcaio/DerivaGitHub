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

namespace Behaviours
{
    public delegate void OnColorUpdatedEvent(Color color, float value);


    public class ColorBlender
    {
        public OnColorUpdatedEvent OnColorUpdated;

        private Color colorA, colorB, destColor, colorBlend;
        private float smoothing = 1f;
        private float targetValue;


        public ColorBlender(BaseBehaviour behaviour, Color colorA, Color colorB, float smoothing)
        {
            //Initializing
            OnColorUpdated = null;
            destColor = Color.white;
            colorBlend = Color.white;

            //Register this instance to the behaviour's event
            behaviour.OnValueUpdated += OnValueUpdatedHandler;

            //Set the colors that will be used to create the blended color
            this.colorA = colorA;
            this.colorB = colorB;

            //Set the smmothing value
            this.smoothing = smoothing;
        }

        public void OnValueUpdatedHandler(float value)
        {
            //Check if the value is usable
            if (float.IsNaN(value))
                return;

            //Blend color with the value calculated by the behaviour class
            destColor = Color.Lerp(colorA, colorB, value);
            targetValue = Mathf.Lerp(targetValue, value, 1.5f * smoothing * Time.deltaTime);

            //Another interpolation to make a smooth color change
            colorBlend = Color.Lerp(colorBlend, destColor, smoothing * Time.deltaTime);

            //If there is any listeners to this event, launch it
            if (OnColorUpdated != null)
                OnColorUpdated(colorBlend, targetValue);
        }
    }
}
