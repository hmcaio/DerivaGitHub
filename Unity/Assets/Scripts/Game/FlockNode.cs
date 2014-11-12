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

public class FlockNode : MonoBehaviour
{
    public FlockNode next;  //Reference to the next FlockNode


    void OnTriggerEnter(Collider other)
    {
        //Check if the GameObject is a Fractal
        if (other.CompareTag(Tags.FRACTAL))
        {
            if (next)
            {
                //Set next target position
                other.gameObject.GetComponent<FractalBehaviour>().DestPos = next.transform.position;
            }
            else
            {
                //Reached last node. Stop flocking
                other.gameObject.GetComponent<FractalBehaviour>().IsFlocking = false;
            }
        }
    }
}
