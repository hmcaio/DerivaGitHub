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


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostBehaviour : MonoBehaviour
{
    public float power = 50f;  //Power of the explosion
    public float smooth = 1.5f;  //Smoothness of the shrink effect

    List<Transform> fragments;  //References to the boost fragments


    void Start()
    {
        fragments = new List<Transform>(gameObject.transform.GetComponentsInChildren<Transform>());
        fragments.Remove(gameObject.transform);  //The first element is the group GameObject. Remove it so that we don't change its scale
    }

    void OnTriggerEnter(Collider other)
    {
        Explode();  //When the player get the boost, start the visual effects
    }

    void Explode()
    {
        audio.Play();  //Play the boost sound effect

        foreach (Transform frag in fragments)
        {
            //Add an explosion force with some little random values to each fragment
            frag.rigidbody.AddExplosionForce(
                power * (1.5f + Random.Range(-0.25f, 0.25f)),
                transform.position - new Vector3(0, Random.Range(-0.1f, 0.1f), Random.Range(0f, 0.25f)),
                1f,
                Random.Range(-0.25f, 0.25f));
        }

        StartCoroutine(Disappear());  //Start the shrink effect
    }

    IEnumerator Disappear()
    {
        Vector3 scale = fragments[0].localScale;  //Get the initial scale value of the fragments

        while (scale.sqrMagnitude > 0.1f)
        {
            scale = Vector3.Lerp(scale, Vector3.zero, smooth * Time.deltaTime);  //Scale down
            foreach (Transform frag in fragments)
                frag.localScale = scale;

            yield return 0;
        }

        Destroy(gameObject); //Destroy all the group after the end of the animation
    }
}
