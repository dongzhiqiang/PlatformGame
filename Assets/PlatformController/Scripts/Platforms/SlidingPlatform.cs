using UnityEngine;
using System.Collections;

public class SlidingPlatform : Platform {
	
	public Vector2 speed;
	
	private bool alreadyAdded;
	
	override protected void DoUpdate(){
		alreadyAdded = false;	
	}
	
	override public void DoAction(RaycastCollider collider, RaycastCharacterController character) {
		// If we are standing on this
		if (!alreadyAdded && collider.direction == RC_Direction.DOWN) {
			character.Velocity += speed;
			alreadyAdded = true;
		}
	}
}
