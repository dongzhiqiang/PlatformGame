using UnityEngine;
using System.Collections;

public class IPhoneTouchInput : RaycastCharacterInput
{

	void Update ()
	{
		jumpButtonHeld = false;
		jumpButtonDown = false;
		x = 0;
		y = 0;
		for (int i = 0; i < Input.touchCount; i++) {
			if (Input.touches[i].phase == TouchPhase.Began || Input.touches[i].phase == TouchPhase.Stationary){
				if (Input.touches[i].position.x < Screen.width * 0.33) x = -1;
				if (Input.touches[i].position.x > Screen.width * 0.66) x = 1;
				if (Input.touches[i].position.y > Screen.height * 0.33) {jumpButtonDown = true; jumpButtonHeld = true;}
			}
			
		}
	}
}

