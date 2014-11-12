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


using System.Collections.Generic;
using UnityEngine;

namespace Behaviours
{
    public delegate void OnValueUpdatedEvent(float value);

    ///// <summary>
    ///// Struct for the resultant values of color and float
    ///// </summary>
    //public struct ColVal
    //{
    //    /// <summary>
    //    /// Initializes a new ColVal struct with the given values
    //    /// </summary>
    //    /// <param name="col"></param>
    //    /// <param name="val"></param>
    //    public ColVal(Color col, float val)
    //    {
    //        color = col;
    //        value = val;
    //    }

    //    public Color color;
    //    public float value;
    //}


    /// <summary>
    /// Base class for audio based color change algorithms
    /// </summary>
    public abstract class BaseBehaviour
    {
        protected AudioSource audioSrc;

        public OnValueUpdatedEvent OnValueUpdated;


        public BaseBehaviour(AudioSource audio)
        {
            audioSrc = audio;
            OnValueUpdated = null;
        }

        /// <summary>
        /// The audio based color change algorithm
        /// </summary>
        /// <returns></returns>
        public abstract void CalcValue();
    }

    /// <summary>
    /// Adham's
    /// </summary>
    public class Behaviour1 : BaseBehaviour
    {
        #region FIELDS AND PROPERTIES

        const int qSamples = 512;//64;

        private float[] samples;
        private float average;
        private int band = 6;
        private int n;
        private int start;
        private float value;

        #endregion


        public Behaviour1(AudioSource audio)
            : base(audio)
        {
            samples = new float[qSamples];
            n = (int)Mathf.Pow(2, band) * 2;
            start = n - 2;
        }

        public override void CalcValue()
        {
            audioSrc.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

            average = 0;
            for (int i = 0; i < n; i++)
                average += samples[start + i] * (i + 1);
            average /= n;
            value = average * 10;

            if (OnValueUpdated != null)
                OnValueUpdated(value);
        }
    }

    /// <summary>
    /// Interpolation and adjust
    /// </summary>
    public class Behaviour2 : BaseBehaviour
    {
        #region FIELDS AND PROPERTIES

        const int qSamples = 512;

        private float[] samples;
        //private Color color;
        private float a, b;
        private float sumX, sumY, sumX2, sumXY;

        #endregion


        public Behaviour2(AudioSource audio, float[] xArray, Color[] pointsList)
            : base(audio)
        {
            samples = new float[qSamples];
            NIP.GeneratePolynomial(xArray, pointsList);

            //Pre-calculate the constant values
            sumX = sumX2 = 0;
            for (int i = 0; i < qSamples; i += 2)
            {
                sumX += i;// / 2;
                sumX2 += (i /*/ 2*/) * (i /*/ 2*/);
            }
        }

        public override void CalcValue()
        {
            audioSrc.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

            sumY = sumXY = 0;
            for (int i = 0; i < qSamples; i += 2)
            {
                sumY += ((Mathf.Log(samples[i]) + 10) * 10);
                sumXY += (i /*/ 2*/) * ((Mathf.Log(samples[i]) + 10) * 10);

                //FileMan.Write(samples[i]);

                //sumY += ((Mathf.Log(samples[i]) + 10) * 10);
                //sumXY += (i /*/ 2*/) * ((Mathf.Log(samples[i]) + 10) * 10);
            }

            //Line adjustment
            a = ((qSamples * 0.5f) * sumXY - sumX * sumY) / ((qSamples * 0.5f) * sumX2 - sumX * sumX);
            b = (sumY - a * sumX) / (qSamples * 0.5f);

            //Base
            //color = (-b / (a * qSamples));
            //color = ((-b / (a * qSamples)) - min) / (|min| + |max|);

            //tjeee: Min= -3.8; Max= 0.85
            //float f = ((-b / (a * qSamples)) + 1f) / 2.5f;
            float f = ((-b / (a * qSamples)) + 1f) / 2f;
            ////f -= Mathf.Abs(Mathf.Sin(Time.time)) / 2;
            //Vector3 v = NIP.Calculate(f);
            //color = new Color(v.x, v.y, v.z);
            ////color = Color.Lerp(Color.white, Color.red, f);

            //return new ColVal(color, f);

            if (OnValueUpdated != null)
                OnValueUpdated(f);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Behaviour3 : BaseBehaviour
    {
        #region FIELDS AND PROPERTIES

        const int qSamples = 64;
        const int qSubband = 4;
        const int qBuffer = 5;
        const float c = 1.25f;

        private float[] samples;
        private Queue<float>[] queues;
        //private Color color;
        private float f;

        #endregion


        public Behaviour3(AudioSource audio)
            : base(audio)
        {
            samples = new float[qSamples];

            queues = new Queue<float>[qSubband];
            for (int a = 0; a < qSubband; a++)
            {
                //Initialize buffer with zeros
                queues[a] = new Queue<float>(qBuffer);
                for (int b = 0; b < qBuffer; b++)
                    queues[a].Enqueue(0f);
            }
        }

        public override void CalcValue()
        {
            audioSrc.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

            //Calculates the instant sum of each subband
            for (int a = 0; a < qSubband; a++)
            {
                float instantSum = 0;
                for (int b = 0; b < qSamples / qSubband; b++)
                    instantSum += samples[a * (qSamples / qSubband) + b];

                //Updates the average
                float[] array;
                array = queues[a].ToArray();
                float average = 0;
                for (int d = 0; d < qBuffer; d++)
                    average += array[d];
                average /= qBuffer;

                queues[a].Dequeue();  //Discard the oldest value
                queues[a].Enqueue(instantSum);  //Inserts the newest value

                if (a == 0)
                {
                    if (instantSum > c * average)
                        f = 1;
                    else
                        f = 0f;

                    //color = new Color(f, 0, 0);
                }
            }

            //return new ColVal(color, f);

            if (OnValueUpdated != null)
                OnValueUpdated(f);
        }
    }
}
