using UnityEngine;
using System.Collections;

public class UpAndDownPlatform : Platform {
	
	public float maxHeight;
	public float minHeight;
	public float speed;
	
	override protected void DoStart() {
		if (speed < 0.0f) {
			velocity = new Vector3(0.0f, speed, 0.0f);
			speed *= -1;
		} else {
			velocity = new Vector3(0.0f, speed, 0.0f);
		}
	}
	
	override protected void DoUpdate () {
		if (myTransform.position.y >= maxHeight) {
			myTransform.position = new Vector3(myTransform.position.x, maxHeight, myTransform.position.z);
			velocity = new Vector3(0.0f, -1 * speed, 0.0f);
		} else if (myTransform.position.y <= minHeight) {
			myTransform.position = new Vector3(myTransform.position.x, minHeight, myTransform.position.z);
			velocity = new Vector3(0.0f, speed, 0.0f);
		}
	}
	
	override public bool ParentOnStand() {
		return true;	
	}
}
