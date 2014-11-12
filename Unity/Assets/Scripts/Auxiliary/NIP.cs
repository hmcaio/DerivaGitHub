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

/// <summary>
/// Auxiliary class that generates and calculates a Newton Interpolation Polynomial (NIP) for colors
/// </summary>
public static class NIP
{
    #region FIELDS AND PROPERTIES

    private static Color[] pointsList;
    private static float[] xArray;  //Array of x values
    private static Vector3[] divDifTable;  //Divided Differences Table
    private static Vector3[] colorArray;  //Array of color RGB values


    #endregion


    /// <summary>
    /// Generates a Polynomial with the given x values and corresponding colors
    /// </summary>
    /// <param name="xArray">The x values</param>
    /// <param name="pointsList">The colors to interpolate</param>
    public static void GeneratePolynomial(float[] xArray, Color[] pointsList)
    {
        NIP.xArray = xArray;
        NIP.pointsList = pointsList;

        if (pointsList.Length == 0)
        {
            Debug.Log("Points list empty. There is nothing to interpolate.");
            return;
        }

        //Acquiring the x and f(x) values in the form of arrays
        int n = pointsList.Length;
        colorArray = new Vector3[n];
        int a1 = 0;
        for (int i = 0; i < n; i++)
        {
            colorArray[i] = new Vector3(pointsList[i].r, pointsList[i].g, pointsList[i].b);
        }

        //Bubble Sort (x ascending order)
        for (int i = 0; i < n - 1; i++)
        {
            bool changed = false;
            for (int j = 0; j < n - i - 1; j++)
            {
                if (xArray[j] > xArray[j + 1])
                {
                    float aux = xArray[j];
                    xArray[j] = xArray[j + 1];
                    xArray[j + 1] = aux;
                    Vector3 auxV = colorArray[j];
                    colorArray[j] = colorArray[j + 1];
                    colorArray[j + 1] = auxV;
                    changed = true;
                }
            }
            if (!changed)
                break;
        }

        //Calculating the length of the table
        divDifTable = new Vector3[n % 2 == 0 ?
            (n / 2) * (n + 1) :  //Even
            ((n / 2) + 1) * n];  //Odd

        //The first n elements of the table are the given f(x) values
        for (int i = 0; i < n; i++)
            divDifTable[i] = colorArray[i];

        //Filling the table
        int index = n;
        a1 = n;
        for (int i = 1; i < n; i++)
        {
            for (int j = 1; j < n - i + 1; j++)
            {
                divDifTable[index] = (divDifTable[index - a1 + 1] - divDifTable[index - a1])
                                    / (xArray[j - 1 + i] - xArray[j - 1]);
                index += 1;
            }
            a1--;
        }

        //for (int f = 0; f < divDifTable.Length; f++)
        //    Debug.Log(divDifTable[f].ToString());
    }

    /// <summary>
    /// Calculates the value for the Newton Interpolating Polynomial with the given x value
    /// </summary>
    /// <param name="x">Abscissa</param>
    /// <returns>Ordinate of the x value from the given Newton Interpolating Polynomial</returns>
    public static Vector3 Calculate(float x)
    {
        Vector3 result = Vector3.zero;
        float product = 1;
        int index = 0, a = pointsList.Length;

        for (int i = 0; i < pointsList.Length; i++)
        {
            result += (product * divDifTable[index]);
            index += a;
            a--;
            product *= (x - xArray[i]);
        }

        return new Vector3(Mathf.Clamp(result.x, 0f, 1f), Mathf.Clamp(result.y, 0f, 1f), Mathf.Clamp(result.z, 0f, 1f));
        //return new Color(Mathf.Clamp(result.x, 0f, 1f), Mathf.Clamp(result.y, 0f, 1f), Mathf.Clamp(result.z, 0f, 1f));
    }
}
