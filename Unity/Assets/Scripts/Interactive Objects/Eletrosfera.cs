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

public class Eletrosfera : MonoBehaviour
{
    public float force1, force2, force3;
    Transform[] children;


    void Start()
    {
        children = gameObject.GetComponentsInChildren<Transform>();
    }

    void /*Fixed*/Update()
    {
        children[1].Rotate(Vector3.right * force1 * Time.deltaTime, Space.Self);
        children[2].Rotate(Vector3.up * force2 * Time.deltaTime, Space.Self);
        children[3].Rotate(Vector3.right * force3 * Time.deltaTime, Space.Self);
    }
}
