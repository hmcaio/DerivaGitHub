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

public class FractalBehaviour : MonoBehaviour, IColorListener
{
    private List<AreaTrigger> triggers;
    public bool isFlocking = false;
    public bool IsFlocking
    {
        get { return isFlocking; }
        set
        {
            isFlocking = value;
        }
    }
    private Vector3 dest;
    public Vector3 DestPos
    {
        set { dest = value; }
    }
    public float
        intensityBase = 1f,
        intensityVar = 2f,
        rangeBase = 20f,
        rangeVar = 100f;


    void Start()
    {
        triggers = new List<AreaTrigger>();
    }

    void OnEnable()
    {
        AudioManager.Instance.AddListener(this, 0);
    }

    void OnDisable()
    {
        if (AudioManager.Instance != null)
            AudioManager.Instance.RemoveListener(this, 0);
    }

    void Update()
    {
        if (!isFlocking)
            return;

        //Move towards the destination
        rigidbody.AddForce((dest - transform.position).normalized * 0.2f, ForceMode.Force);
    }

    public void OnColorUpdatedHandler(Color color, float value)
    {
        gameObject.renderer.material.SetColor("_FractalColor", color);
        if (gameObject.light != null)
        {
            color.a = light.color.a;
            light.color = color;
            light.intensity = intensityBase + intensityVar * value;
            light.range = rangeBase + rangeVar * value;
        }
    }

    public void Add(AreaTrigger trigger)
    {
        triggers.Add(trigger);
    }

    public void Remove(AreaTrigger trigger)
    {
        triggers.Remove(trigger);
        if (triggers.Count == 0)
            FractalMan.Instance.Respawn(gameObject);
    }
}
