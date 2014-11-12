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

public class StartPlayerControl : MonoBehaviour
{
    #region FIELDS AND PROPERTIES

    public Vector3 force = new Vector3(0, 0, 10);

    #endregion


    void FixedUpdate()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        gameObject.rigidbody.AddRelativeForce(force * Input.GetAxis("Vertical"), ForceMode.Acceleration);
#elif UNITY_WP8 || UNITY_ANDROID
        if (TouchTut.Instance.IsTouching)
            FixedMoveForward();
#endif
    }

    public void FixedMoveForward()
    {
        gameObject.rigidbody.AddRelativeForce(force, ForceMode.Acceleration);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(Tags.START_GAME))
        {
            GameMaster.Instance.OnGameEvent(GameMaster.GameEvent.LOAD_LEVEL1);
        }
    }
}
