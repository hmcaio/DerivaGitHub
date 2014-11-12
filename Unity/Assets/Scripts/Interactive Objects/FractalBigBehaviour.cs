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

public class FractalBigBehaviour : MonoBehaviour, IColorListener
{
    void OnEnable()
    {
        AudioManager.Instance.AddListener(this, 1);
    }

    void OnDisable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.RemoveListener(this, 1);
    }

    public void OnColorUpdatedHandler(Color color, float value)
    {
        gameObject.renderer.material.SetColor("_FractalColor", color);
    }
}
