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

public class CameraCollider : MonoBehaviour
{
    #region FIELDS AND PROPERTIES

    public GameObject theCam;
    public Transform target;
    public Vector3 positionOffset = new Vector3(0, 1, -4.5f);
    public Vector3 nearPos = new Vector3(0, -0.3f, 3.75f);
    public Vector3 focusOffset = new Vector3(0, 1, 0);
    public float smoothing = 5f;

    const int closedAngle = 50;
    const int openAngle = 70;
    int count = 0;

    #endregion


    void Start()
    {
        count = 0;
    }

    void OnEnable()
    {
        GameMaster.Instance.MainCamera = gameObject;
    }

    void FixedUpdate()
    {
        transform.position = Vector3.Lerp(
                    transform.position,
                    target.transform.position
                    + positionOffset.x * target.transform.right
                    + positionOffset.y * target.transform.up
                    + positionOffset.z * target.transform.forward,
                    smoothing * Time.deltaTime);

        transform.LookAt(
            target.transform.position
            + focusOffset.x * target.transform.right
            + focusOffset.y * target.transform.up
            + focusOffset.z * target.transform.forward,
            target.transform.up);
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(Tags.PLAYER) && !other.CompareTag(Tags.SPHERE_TRIGGER))
        {
            count++;
            StopCoroutine("FarCamera");
            StartCoroutine("NearCamera");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(Tags.PLAYER) && !other.CompareTag(Tags.SPHERE_TRIGGER))
        {
            count--;
            if (count == 0)
            {
                StopCoroutine("NearCamera");
                StartCoroutine("FarCamera");
            }
        }
    }

    IEnumerator NearCamera()
    {
        while (Vector3.Distance(theCam.transform.localPosition, nearPos) > 0.01f)
        {
            theCam.transform.localPosition = Vector3.Lerp(
                theCam.transform.localPosition,
                nearPos,
                smoothing / 3 * Time.deltaTime);

            yield return 0;
        }

        theCam.transform.localPosition = nearPos;
    }

    IEnumerator FarCamera()
    {
        while (Vector3.Distance(theCam.transform.localPosition, Vector3.zero) > 0.01f)
        {
            theCam.transform.localPosition = Vector3.Lerp(
                theCam.transform.localPosition,
                Vector3.zero,
                smoothing / 3 * Time.deltaTime);

            yield return 0;
        }

        theCam.transform.localPosition = Vector3.zero;
    }

    public void SetSmallFOV()
    {
        theCam.camera.fieldOfView = closedAngle;
    }

    public void StartWidenFOV(CallBackMethod callback = null)
    {
        StartCoroutine(WidenFOV(callback));
    }

    IEnumerator WidenFOV(CallBackMethod callback = null)
    {
        float start = Time.time;
        while (theCam.camera.fieldOfView < openAngle - 0.25f)
        {
            //theCam.camera.fieldOfView = Mathf.LerpAngle(theCam.camera.fieldOfView, 60f, 0.75f * Time.deltaTime);
            theCam.camera.fieldOfView = closedAngle + (1f - Mathf.Pow(((Time.time - start) / 3f) - 1, 2)) * (openAngle - closedAngle);  //1 - (x - 1)^2
            yield return 0;
        }
        theCam.camera.fieldOfView = openAngle;

        if (callback != null)
            callback();
    }
}
