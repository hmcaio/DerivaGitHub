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

public class AreaTrigger : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.PLAYER))
        {
            other.GetComponent<PlayerControl>().Add(this);
        }
        else if (other.CompareTag(Tags.FRACTAL))
        {
            other.GetComponent<FractalBehaviour>().Add(this);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(Tags.PLAYER))
        {
            other.GetComponent<PlayerControl>().Remove(this);
        }
        else if (other.CompareTag(Tags.FRACTAL))
        {
            other.GetComponent<FractalBehaviour>().Remove(this);
        }
    }
}
