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

/// <summary>
/// <para>[Standard Singleton]</para>
/// <para>Class to manage "Fractal" spawning</para>
/// </summary>
public class FractalMan : MonoBehaviour
{
    #region STANDARD SINGLETON STUFF
    //Private reference only this class can access
    private static FractalMan instance;

    //Public reference the other classes will use
    public static FractalMan Instance
    {
        get
        {
            //If instance hasn't been set yet, grab it from the scene
            //This will only happen the first time this reference is used
            if (instance == null)
                instance = GameObject.FindObjectOfType<FractalMan>();

            return instance;
        }
    }
    #endregion


    #region FIELDS AND PROPERTIES

    public GameObject fractalPrefab;
    public GameObject flockPath;
    public int maxFractals = 20;
    public float movImpulse = 1f;
    public float rotImpulse = 0.1f;
    public float spawnInterval = 4f;
    public float flockTimer = 20f;

    private List<Transform> spawnPoints;
    private List<GameObject> fractals;
    private List<FlockNode> flockNodes;

    #endregion


    void Start()
    {
        spawnPoints = new List<Transform>(gameObject.transform.GetComponentsInChildren<Transform>());
        spawnPoints.Remove(transform);  //Remove this GameObject's Transform component from the list

        //Pre instantiate all the fractals the scene will have
        fractals = new List<GameObject>();
        for (int i = 0; i < maxFractals; i++)
        {
            fractals.Add(Instantiate(fractalPrefab) as GameObject);
            fractals[i].SetActive(false);
        }

        flockNodes = new List<FlockNode>(flockPath.GetComponentsInChildren<FlockNode>());
        flockPath.SetActive(false);

        //Start spawning the fractals from the spawn points
        StartCoroutine(InitialSpawn());

        //Start the flock timer
        StartCoroutine(FlockTimer());
    }

    /// <summary>
    /// Starts spawning "Fractal"s with the interval set
    /// </summary>
    /// <returns></returns>
    IEnumerator InitialSpawn()
    {
        for (int i = 0; i < fractals.Count; i++)
        {
            fractals[i].SetActive(true);
            Spawn(fractals[i]);

            yield return new WaitForSeconds(spawnInterval);
        }
    }

    /// <summary>
    /// Set the position and rotation according to a random spawn point and adds rotation and force impulses
    /// </summary>
    /// <param name="obj">The "Fractal" GameObject</param>
    private void Spawn(GameObject obj)
    {
        //Randomly select a spawn point
        Transform origin = spawnPoints[Random.Range(0, spawnPoints.Count)];

        //Set initial position and rotation
        obj.tag = Tags.FRACTAL;
        obj.transform.position = origin.position;
        obj.transform.rotation = origin.rotation;

        //Add impulse
        obj.rigidbody.AddRelativeForce(
            new Vector3(Random.Range(-movImpulse, movImpulse), Random.Range(-movImpulse, movImpulse), Random.Range(2 * movImpulse, 4 * movImpulse)),
            ForceMode.Impulse);

        //Add rotation
        obj.rigidbody.AddRelativeTorque(
            new Vector3(Random.Range(-rotImpulse, rotImpulse), Random.Range(-rotImpulse, rotImpulse), Random.Range(-rotImpulse, rotImpulse)),
            ForceMode.Impulse);
    }

    /// <summary>
    /// Respawn the "Fractal"
    /// </summary>
    /// <param name="obj"></param>
    public void Respawn(GameObject obj)
    {
        obj.rigidbody.velocity = Vector3.zero;

        Spawn(obj);

        if (obj.GetComponent<FractalBehaviour>().IsFlocking)
            SearchNearestNode(obj);
    }

    IEnumerator FlockTimer()
    {
        //Start a timer after which the Fractals are going to flock 
        yield return new WaitForSeconds(flockTimer);

        //Set all Fractals to start flocking and go to the ending portal
        for (int i = 0; i < fractals.Count; i++)
        {
            SearchNearestNode(fractals[i]);

            yield return 0;
        }

        flockPath.SetActive(true);
    }

    void SearchNearestNode(GameObject fractal)
    {
        float dist = 0f, minDist = 0f;
        int nearestNodeIndex = 0;

        //Search for the nearest FlockNode from this Fractal
        for (int j = 0; j < flockNodes.Count; j++)
        {
            dist = Vector3.Distance(fractal.transform.position, flockNodes[j].transform.position);
            if (dist < minDist)
            {
                nearestNodeIndex = j;
                minDist = dist;
            }
        }

        fractal.GetComponent<FractalBehaviour>().DestPos = flockNodes[nearestNodeIndex].transform.position;
        fractal.GetComponent<FractalBehaviour>().IsFlocking = true;
    }
}
