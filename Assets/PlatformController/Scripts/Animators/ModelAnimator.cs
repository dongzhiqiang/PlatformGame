using UnityEngine;
using System.Collections;

using System.Collections.Generic;

/**
 * A sample animation classes that works with 3d models.
 */

public class ModelAnimator : MonoBehaviour {
	
	public RaycastCharacterController controller;
	
	void Start(){
		// Set all animations to loop
   		GetComponent<Animation>().wrapMode = WrapMode.Loop;
   		// except jumping
   		GetComponent<Animation>()["jump"].wrapMode = WrapMode.Once;
   		GetComponent<Animation>()["jump"].layer = 1;
		
		// Register listeners
		controller.Idle += new RaycastCharacterController.CharacterControllerEventDelegate (Idle);
		controller.Walk += new RaycastCharacterController.CharacterControllerEventDelegate (Walk);
		controller.Run += new RaycastCharacterController.CharacterControllerEventDelegate (Run);
		controller.Jump += new RaycastCharacterController.CharacterControllerEventDelegate (Jump);
		controller.WallJump += new RaycastCharacterController.CharacterControllerEventDelegate (Jump);
		controller.Fall += new RaycastCharacterController.CharacterControllerEventDelegate (Fall);
		controller.Slide += new RaycastCharacterController.CharacterControllerEventDelegate (Idle);
		controller.Hold += new RaycastCharacterController.CharacterControllerEventDelegate (Hold);
		controller.Climb += new RaycastCharacterController.CharacterControllerEventDelegate (Climb);
	}
	
	public void Idle (CharacterState previousState) {
		GetComponent<Animation>().CrossFade("idle");
		CheckDirection();
	}
	
	public void Walk (CharacterState previousState) {
		GetComponent<Animation>().CrossFade("walk");
		CheckDirection();
	}

	public void Run (CharacterState previousState) {
		GetComponent<Animation>().CrossFade("run");
		CheckDirection();
	}

	public void Jump(CharacterState previousState) {
		GetComponent<Animation>().CrossFade("jump");
		CheckDirection();
	}
	
	public void Fall(CharacterState previousState) {
		GetComponent<Animation>().CrossFade("fall");
		CheckDirection();
	}
	
	private void CheckDirection(){
		if (controller.Velocity.x > 0 ) {
			transform.localRotation = Quaternion.Euler (0.0f, 270.0f, 0.0f);
		} else if (controller.Velocity.x < 0) {
			transform.localRotation = Quaternion.Euler (0.0f, 90.0f, 0.0f);
		}	
	}
	
	public void Hold(CharacterState previousState) {
		GetComponent<Animation>().CrossFade("hold");
		transform.localRotation = Quaternion.Euler (0.0f, 180.0f, 0.0f);
	}
	
	public void Climb(CharacterState previousState) {
		GetComponent<Animation>().CrossFade("walk");
		transform.localRotation = Quaternion.Euler (0.0f, 180.0f, 0.0f);
	}
}