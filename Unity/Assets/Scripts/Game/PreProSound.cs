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
using System.Collections.Generic;
using UnityEngine;
#if UNITY_WP8
using UnityEngine.Windows;
#endif

public class PreProSound : MonoBehaviour
{
    public GameObject cube;
    public GameObject lights;
    public float factor;

    string path = @"C:\Users\Caio\Desktop\t.bytes";

    public GUIText debugText;
    GameObject[] objs;
    public TextAsset t;
    public AudioSource audioSrc;
    public float min, sec;
    public int subband;
    public float scaleFactor;
    public float smoothing;

    enum State { RECORDING, WRITING, READING, PLAYING, NONE };
    State state;

    float time;
    float[] samples;
    float[] curValues;
    List<float> values;
    float average, minValue, maxValue, diff;
    int sampleCount;
    int count;

    float[] spectrum;
    int qSamples = 1024;
    float a, b;
    float sumX, sumY, sumX2, sumXY;


    void Start()
    {
        objs = GameObject.FindGameObjectsWithTag("Fractal");

        //samples = new float[512];
        spectrum = new float[qSamples];

        curValues = new float[8];
        values = new List<float>();

        minValue = float.PositiveInfinity;
        maxValue = float.NegativeInfinity;

        time = min * 60 + sec;
        state = State.READING;
        //state = State.NONE;

        //Debug.Log("Path: " + Application.persistentDataPath + "\n" + Application.dataPath);
    }

    void FixedUpdate()
    {
        //guiText.text = state.ToString() + 
        //    "\nTempo: " + ((audioSrc.audio.time - 30) / 60).ToString("00") + ":" + (audioSrc.audio.time % 60).ToString("00");

        switch (state)
        {
            case State.RECORDING:
                if (audioSrc.time < time)
                {
                    GetData();
                }
                else
                {
                    audioSrc.Stop();
                    state = State.WRITING;
                }
                break;

            case State.WRITING:
                WriteToFile();
                values.Clear();

                state = State.READING;
                break;

            case State.READING:
                ReadFromFile();

                count = 0;
                audioSrc.Play();
                state = State.PLAYING;
                break;

            case State.PLAYING:
                if (audioSrc.time < time)
                {
                    float f = 1f + ((values[count] * 10) + minValue) / diff - 0.25f;
                    //Debug.Log(f.ToString("F3"));
                    foreach (GameObject obj in objs)
                    {
                        /*obj.transform.localScale = Vector3.Lerp(
                            gameObject.transform.localScale,
                            new Vector3(f, f, f),
                            smoothing * Time.deltaTime);*/

                        obj.renderer.material.color = Color.Lerp(Color.black, Color.blue, f);
                    }

                    //lights.light.color = new Color(236/256f, 167/256f, 187/256f) * f * factor;
                    //lights.light.color = Color.Lerp(Color.black, new Color(236 / 256f, 167 / 256f, 187 / 256f), f*factor);
                    //cube.renderer.material.color = Color.Lerp(Color.white, Color.red, f);

                    count++;
                    if (count >= values.Count)
                        state = State.NONE;
                }
                break;

            case State.NONE:
                //Finished
                break;

            default:
                break;
        }
    }

    void GetData()
    {
        //audioSrc.audio.GetSpectrumData(samples, 0, FFTWindow.BlackmanHarris);

        //int count = 0;
        //for (int i = 0; i < 8; ++i)
        //{
        //    average = 0;
        //    sampleCount = (int)Mathf.Pow(2, i) * 2;

        //    for (int j = 0; j < sampleCount; ++j)
        //    {
        //        average += samples[count] * (count + 1);
        //        ++count;
        //    }

        //    average /= sampleCount;
        //    curValues[i] = average * 10;

        //    if (curValues[i] < minValue)
        //        minValue = curValues[i];
        //    else if (curValues[i] > maxValue)
        //        maxValue = curValues[i];
        //}

        //values.Add(scaleFactor * curValues[subband]);

        audioSrc.audio.GetSpectrumData(spectrum, 0, FFTWindow.BlackmanHarris);

        sumX = sumY = sumX2 = sumXY = 0;
        for (int i = 0; i < qSamples; i++)
        {
            if (i % 2 == 0)  //OBS.: Apparently, spectrum[1023] is an absurdly high value that makes the sum equals to infinite, which is not cool at all =P
            {
                sumX += i;
                sumY += (Mathf.Log(spectrum[i]) + 10) * 10;
                sumX2 += i * i;
                sumXY += i * (Mathf.Log(spectrum[i]) + 10) * 10;
            }
        }

        //Line adjustment
        a = ((qSamples * 0.5f) * sumXY - sumX * sumY) / ((qSamples * 0.5f) * sumX2 - sumX * sumX);
        b = (sumY - a * sumX) / (qSamples * 0.5f);

        float v = -b / (a * qSamples);
        values.Add(v);

        if (v < minValue)
            minValue = v;
        else if (v > maxValue)
            maxValue = v;
    }

    void WriteToFile()
    {
        values.Add(minValue);
        values.Add(maxValue);

        byte[] byteArray = new byte[values.Count * 4];
        Buffer.BlockCopy(values.ToArray(), 0, byteArray, 0, byteArray.Length);
#if UNITY_WP8
        File.WriteAllBytes(path, byteArray);
#elif UNITY_ANDROID
        //TODO
#endif
    }

    void ReadFromFile()
    {
        minValue = 0;
        maxValue = 0;

        //TextAsset t = Resources.Load<TextAsset>("t");
        //byte[] byteArray = File.ReadAllBytes(Application.dataPath + @"\t.txt");
        //if (t == null) Debug.Log("null");
        byte[] byteArray = t.bytes;
        float[] floatArray = new float[byteArray.Length / 4];
        Buffer.BlockCopy(byteArray, 0, floatArray, 0, byteArray.Length);
        values = new List<float>(floatArray);

        maxValue = values[values.Count - 1];
        minValue = values[values.Count - 2];
        values.RemoveAt(values.Count - 1);
        values.RemoveAt(values.Count - 2);

        diff = Mathf.Abs(maxValue - minValue);
        //Debug.Log("min: " + minValue.ToString("F3") + " max: " + maxValue.ToString("F3") + " diff: " + diff.ToString("F3"));
    }
}
