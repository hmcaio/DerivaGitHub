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

public class LoadingCubes : MonoBehaviour
{
    #region FIELDS AND PROPERTIES

    public Color destColor;
    public float smoothing = 1f;
    public float rotationSpeed = 1f;
    public GameObject[] objsToDestroy;

    private List<Transform> cubes;
    private List<Renderer> renderers;
    private int n = 0;
    private Color c;
    //private AsyncOperation ao;

    #endregion


    void Start()
    {
        if (GameMaster.Instance.IsFirstTime)
            TouchTut.Instance.HideLoad();

        //Get all Renderer components from children
        renderers = new List<Renderer>(gameObject.GetComponentsInChildren<Renderer>());

        //Get all Transform components from children
        cubes = new List<Transform>(gameObject.GetComponentsInChildren<Transform>());
        //Remove this GameObject's transform component from the list
        cubes.Remove(gameObject.transform);

        //Set event handlers
        GameMaster.Instance.OnProgressUpdated += SetProgress;
        GameMaster.Instance.OnSceneLoaded += SceneLoaded;

        //Start changing colors according to load progress
        StartCoroutine(ChangeCubeColors());
    }

    void OnDisable()
    {
        //Detach event handlers
        GameMaster.Instance.OnProgressUpdated -= SetProgress;
        GameMaster.Instance.OnSceneLoaded -= SceneLoaded;

        if (GameMaster.Instance.IsFirstTime)
            TouchTut.Instance.ShowLoad();
    }

    IEnumerator ChangeCubeColors()
    {
        for (int i = 0; i < 9; i++)
        {
            while (n <= i)
                yield return 0;

            c = renderers[i].material.GetColor("_ObjectColor");

            while (Mathf.Abs(c.r - destColor.r) > 0.01f)
            {
                c = Color.Lerp(c, destColor, smoothing * Time.deltaTime);
                renderers[i].material.SetColor("_ObjectColor", c);
                yield return 0;
            }

            renderers[i].material.SetColor("_ObjectColor", destColor);

            yield return 0;
        }

        //Finished coloring the cubes, now do a rotation animation
        for (float t = 0f; t < 2f; t += Time.deltaTime)
        {
            for (int i = 0; i < cubes.Count; i++)
                cubes[i].Rotate(-Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

            yield return 0;
        }

        //After some time animating, start fading screen
        GameMaster.Instance.SetAllowSceneActivation();

        //Meanwhile, continue animating
        while (true)
        {
            for (int i = 0; i < cubes.Count; i++)
                cubes[i].Rotate(-Vector3.up, rotationSpeed * Time.deltaTime, Space.Self);

            yield return 0;
        }
    }

    void SetProgress(int p)
    {
        n = p;
    }

    void SceneLoaded()
    {
        foreach (GameObject t in objsToDestroy)
        {
            Destroy(t);
        }
        Destroy(gameObject);
    }
}
