using UnityEngine;
using System.Collections;

public class RaycastCharacterController : MonoBehaviour {
	
	public RaycastCollider[] feetColliders;
	public RaycastCollider[] headColliders;
	public RaycastCollider[] sides;
	public MovementDetails movement;
	public JumpDetails jump;
	public SlopeDetails slopes;
	public ClimbDetails climbing;
	public int backgroundLayer;
	public int passThroughLayer;
	public int climableLayer;
	public Platform myParent;
	public bool controllerActive = true;
	public RaycastCharacterInput characterInput;
	public bool sendAnimationEventsEachFrame = false;
	
	public static float maxFrameTime = 0.05f;
	public static float groundedLookAhead = 0.25f;
	public static float maxSpeedForIdle = 0.1f;

	
	private Transform myTransform;
	private float frameTime;
	private Vector3 velocity;
	private int jumpCount = 0;
	private float jumpHeldTimer = 0.0f;
	private float jumpButtonTimer = 0.0f;
	private float fallingTime = 0.0f;
	private int wallJumpDirection = 0;
	private float wallJumpTimer = 0.0f;
	private float currentDrag = 0.0f;
	private bool startedClimbing = false;
	
	#region events
	
	public delegate void CharacterControllerEventDelegate(CharacterState previousState);
	public event CharacterControllerEventDelegate Run;
	public event CharacterControllerEventDelegate Walk;
	public event CharacterControllerEventDelegate Slide;
	public event CharacterControllerEventDelegate Idle;
	public event CharacterControllerEventDelegate Jump;
	public event CharacterControllerEventDelegate Fall;
	public event CharacterControllerEventDelegate Hold;
	public event CharacterControllerEventDelegate Climb;
	public event CharacterControllerEventDelegate WallJump;
	
	#endregion
	
	#region public methods
	
	public void SetDrag(float drag) {
		currentDrag = drag;
	}
	
	public Vector2 Velocity {
		get { return velocity; }	
		set { velocity = value; }
	}
		
	private CharacterState state;
	public CharacterState State{
		get { return state; }
		set {
			if (value != state || sendAnimationEventsEachFrame || value == CharacterState.WALL_JUMPING || value == CharacterState.JUMPING) {
				switch(value) {
					case CharacterState.IDLE :  	if (Idle != null) Idle(state);
													break;
					case CharacterState.WALKING :  	if (Walk != null) Walk(state);
													break;					
					case CharacterState.RUNNING :  	if (Run != null) Run(state);
													break;
					case CharacterState.SLIDING :  	if (Slide != null) Slide(state);
													break;	
					case CharacterState.JUMPING :  	if (Jump != null) Jump(state);
													break;						
					case CharacterState.FALLING :  	if (Fall != null) Fall(state);
													break;
					case CharacterState.WALL_JUMPING :  if (WallJump != null) WallJump(state);
													break;					
					case CharacterState.HOLDING :  	if (Hold != null) Hold(state);
													break;
					case CharacterState.CLIMBING :  if (Climb != null) Climb(state);
													break;
				}
			}
			state = value;
		}
	}
	
	#endregion
	
	#region Unity hooks
	
	void Awake () {
		myTransform = transform;	
		velocity = Vector3.zero;
		currentDrag = movement.drag;
		
		// Assign default transforms
		if (feetColliders != null) {
			foreach (RaycastCollider c in feetColliders) {
				if (c.transform == null) c.transform = transform;
			}
		}
		if (headColliders != null) {
			foreach (RaycastCollider c in headColliders) {
				if (c.transform == null) c.transform = transform;	
			}
		}
		if (sides != null) {
			foreach (RaycastCollider c in sides) {
				if (c.transform == null) c.transform = transform;	
			}
		}	
	}
	
	
	void LateUpdate() {
		frameTime = Time.deltaTime;
		if (frameTime > maxFrameTime) frameTime = maxFrameTime;
		
		
		if (controllerActive) {
			bool grounded = IsGrounded(slopes.groundError);
			if (grounded) {
				fallingTime = 0.0f;
			} else {
				fallingTime += frameTime;
			}
			MoveInXDirection(grounded);
			MoveInYDirection(grounded);
		}
	}
	
#endregion
	
	private void MoveInXDirection(bool grounded) {
		// Calculate Velocity
		if (characterInput.x != 0) {
			
			bool walking = false;
			if (jumpCount > 0) currentDrag = jump.drag;
			if (jumpCount > 0 || (characterInput.x > 0 && characterInput.x < 1.0f ) || (characterInput.x < 0 && characterInput.x > -1.0f )) walking = true;
			float newVelocity = velocity.x + (frameTime * movement.acceleration * characterInput.x);
			newVelocity = newVelocity / (currentDrag);
			if (walking) {
				// If going too fast just apply drag (dont limit to walk speed else we will get odd jerks)
				if (velocity.x > movement.walkSpeed || velocity.x < movement.walkSpeed * -1) {
					velocity.x = velocity.x / (currentDrag);
					Debug.Log ("Limiting only");
				} else {
					velocity.x = newVelocity;
					// Limit to walk speed;
					if (velocity.x > movement.walkSpeed) velocity.x = movement.walkSpeed;
					if (velocity.x < -1 * movement.walkSpeed) velocity.x = -1 * movement.walkSpeed;
				}
			} else {
				velocity.x = newVelocity;
				// Limit to run speed;
				if (velocity.x > movement.runSpeed) velocity.x = movement.runSpeed;
				if (velocity.x < -1 * movement.runSpeed) velocity.x = -1 * movement.runSpeed;
			}
		} else {
			velocity.x = velocity.x / (currentDrag);
		}
		
		// Apply velocity
		if (velocity.x > movement.skinSize || velocity.x * -1 > movement.skinSize)
			myTransform.Translate(velocity.x * frameTime, 0.0f, 0.0f);
		
		float forceSide = 0.0f;
		for (int i = 0; i < sides.Length; i++) {
			RaycastHit hitSides = sides[i].GetCollision(1 << backgroundLayer );
			
			// Pushing on something that has an action when you push it
			if (hitSides.collider != null) {
				Platform platform = hitSides.collider.gameObject.GetComponent<Platform>();
				if (platform != null)  {
					platform.DoAction(sides[i], this);
				}
			} 
			// Stop movement
			float tmpForceSide = (hitSides.normal * (sides[i].distance - hitSides.distance)).x;
			if (tmpForceSide > Mathf.Abs(forceSide) || tmpForceSide * -1 > Mathf.Abs(forceSide)){
				forceSide = tmpForceSide;
				break;
			}
			
		}
		if (forceSide > movement.skinSize) {			
			myTransform.Translate(Mathf.Max(velocity.x * frameTime * -1, forceSide), 0.0f, 0.0f);		
			wallJumpDirection = -1;
			wallJumpTimer = jump.wallJumpTime;
		} else if (-1 * forceSide > movement.skinSize) {		
			myTransform.Translate(Mathf.Min(velocity.x * frameTime * -1, forceSide), 0.0f, 0.0f);	
			wallJumpDirection = 1;
			wallJumpTimer = jump.wallJumpTime;
		}
		if ((forceSide > 0 && velocity.x < 0) || (forceSide < 0 && velocity.x > 0)) {
			velocity.x = 0.0f;
		}
		
		// Animation 
		if (IsGrounded (groundedLookAhead) && !startedClimbing) {
			if  (		(velocity.x > movement.walkSpeed && characterInput.x > 0.1f) || 
				 		(velocity.x < movement.walkSpeed * -1 && characterInput.x < -0.1f)) {
				State = CharacterState.RUNNING;
			} else if (	(velocity.x > maxSpeedForIdle && characterInput.x > 0.1f) || 
				 		(velocity.x < -1 * maxSpeedForIdle  && characterInput.x < -0.1f)){
				State = CharacterState.WALKING;
			} else if (	velocity.x > maxSpeedForIdle || velocity.x < -1 * maxSpeedForIdle ){
				State = CharacterState.SLIDING;
			} else {
				State = CharacterState.IDLE;
			}
		} else if (startedClimbing) {
			if (velocity.x != 0) {
				State = CharacterState.CLIMBING;	
			} else {
				State = CharacterState.HOLDING;	
			}
		}
		
		// Reset Drag
		currentDrag = movement.drag;
	}
	
	private void MoveInYDirection(bool grounded){
		float slope = 0.0f;
		int slopeCount = -1; 
		bool isClimbing = false;
		bool cantClimbDown = false;
		bool hasHitHead = false;
		int climbCount = 0;
		bool isClimbingUpOrDown = false;
		
		// Apply parent velocity
		if (myParent != null) myTransform.Translate(0.0f, myParent.velocity.y * frameTime, 0.0f);
		
		// Limit Velocity
		if (velocity.y < movement.terminalVelocity) velocity.y = movement.terminalVelocity;
		
		// Apply velocity
		if (velocity.y > movement.skinSize || velocity.y * -1 > movement.skinSize){
			myTransform.Translate(0.0f, velocity.y * frameTime, 0.0f, Space.World);
		}
		
		// Fall/Stop
		if (velocity.y <= 0.0f || startedClimbing) {
			float maxForce = 0.0f;
			bool hasHitFeet = false;
			GameObject hitGameObject = null;
			float lastHitDistance = -1;
			float lastHitX = 0.0f;
			
			foreach (RaycastCollider feetCollider in feetColliders) {
				RaycastHit hitFeet = feetCollider.GetCollision(1 << backgroundLayer | 1 << passThroughLayer | (climbing.allowClimbing ? 1 << climableLayer : 0), slopes.slopeLookAhead);
				float force = (hitFeet.normal * (feetCollider.distance - hitFeet.distance)).y;			
				// Standing on a something that has an action when you stand on it
				if (hitFeet.collider != null) {
					Platform platform = hitFeet.collider.gameObject.GetComponent<Platform>();
					if (platform != null && feetCollider.distance >= hitFeet.distance) {
						platform.DoAction(feetCollider, this);
						if (platform.ParentOnStand()) {
							// Special case for climables (top step)
							if (climbing.allowClimbing && hitFeet.collider.gameObject.layer == climableLayer) {
								if (startedClimbing || characterInput.y < 0.0f || jumpButtonTimer > 0.0f) {
									myParent = null;		
								} else {
									hasHitFeet = true;
									maxForce = force;
									myParent = platform;
									hitGameObject = hitFeet.collider.gameObject;
								}
							} else {
								// Normal parenting (moving platforms etc)s
								myParent = platform;
								hitGameObject = hitFeet.collider.gameObject;
							}
						}
					}
					// Climbing 
					if (climbing.allowClimbing && hitFeet.collider.gameObject.layer == climableLayer &&
						hitFeet.distance <= feetCollider.distance && jumpButtonTimer <= 0.0f) {
						if (startedClimbing || climbing.autoStick || characterInput.y != 0) {
							climbCount++;
							if (climbCount >= climbing.collidersRequired) {
								startedClimbing = true;
								isClimbing = true;
								hasHitFeet = true;
								
								if (characterInput.y > 0.0f) {
									maxForce = climbing.speed * frameTime;
									if (maxForce > force) maxForce = force;
									isClimbingUpOrDown = true;
								} else if (!cantClimbDown && characterInput.y < 0.0f) {	
									maxForce = climbing.speed * frameTime * -1;
									if (maxForce < -1 * force) maxForce = -1 * force;
									isClimbingUpOrDown = true;
								}
							}
						}
					} else {
						if (startedClimbing) {
							startedClimbing = false;
						}
						// Calculate slope
						if (slopes.allowSlopes && hitFeet.collider.gameObject.layer != climableLayer) {
							if (lastHitDistance < 0.0f) {
								lastHitDistance = hitFeet.distance;
								lastHitX = feetCollider.offset.x;
								if (slopeCount == -1) slopeCount = 0;
							} else {
								slope += Mathf.Atan ((lastHitDistance - hitFeet.distance) / (feetCollider.offset.x - lastHitX)) * Mathf.Rad2Deg;
								slopeCount++;
								lastHitDistance = hitFeet.distance;
								lastHitX = feetCollider.offset.x;
							}
						}
					}
					// Get force to apply
					if (force > maxForce && hitFeet.collider.gameObject.layer != climableLayer) {
						// We hit a blocker stop all climbing
						cantClimbDown = true;
						isClimbingUpOrDown = false;
						isClimbing = false;
						startedClimbing = false;
						hasHitFeet = true;
						maxForce = force;
						hitGameObject = hitFeet.collider.gameObject;
					}
				}
			}
			if (hasHitFeet) {
				myTransform.Translate(0.0f, maxForce, 0.0f, Space.World);	
				velocity.y = 0.0f;
				if (myParent != null && hitGameObject != myParent.gameObject) myParent = null;
				grounded = true;
				fallingTime = 0.0f;	
			} else {
				ApplyGravity();
				// Only reset the parent if we are far enough away
				if (myParent != null && !IsGrounded (groundedLookAhead)) {
					myParent = null;
				}	
			}
		} else {
			ApplyGravity();	
		}
		if (slopes.allowSlopes){
			if (slopeCount > 0 && !isClimbing) {
				myTransform.Rotate(0.0f, 0.0f, slopes.rotationSpeed * (slope / (float) slopeCount));
			} else if (slopeCount == -1 || isClimbing) {
				myTransform.localRotation = Quaternion.RotateTowards(myTransform.localRotation, Quaternion.identity, slopes.rotationSpeed * 10.0f);
			}
		}
		
		// Hitting Head
		if (velocity.y > 0.0f || isClimbing || (myParent != null && myParent.velocity.y > 0.0f)) {
			float maxForce = 0.0f;
			foreach (RaycastCollider headCollider in headColliders) {
				RaycastHit hitHead = headCollider.GetCollision(1 << backgroundLayer);
				float force = (hitHead.normal * (headCollider.distance - hitHead.distance)).y;
				if (hitHead.collider != null) {
					if (force < -1 * movement.skinSize && force < maxForce) {
						hasHitHead = true;
						maxForce = force;
					}
				}
			}
			
			if (hasHitHead ) {
				myTransform.Translate(0.0f, maxForce, 0.0f, Space.World);		
				if (velocity.y > 0.0f) velocity.y = 0.0f;
			}
			if (!isClimbing) {
				ApplyGravity();
			} 
		}
		
		// Jump
		if (!hasHitHead || isClimbing) {
			if (characterInput.jumpButtonDown ) {
				if ((grounded || myParent != null) && jumpCount == 0 && jumpButtonTimer <= 0.0f) {
					myParent = null;
					velocity.y = jump.jumpVelocity;
					jumpCount = 1;
					jumpButtonTimer = jump.jumpTimer;
					jumpHeldTimer = 0.0f;
					State = CharacterState.JUMPING;
				} else if (jumpCount == 1 && jump.canDoubleJump){
					myParent = null;
					jumpCount++;
					velocity.y = jump.doubleJumpVelocity;
					State = CharacterState.JUMPING;
				}
			} else if (characterInput.jumpButtonHeld && jumpHeldTimer < jump.jumpHeldTime && jumpCount == 1) {
				velocity.y += jump.jumpFrameVelocity * Time.deltaTime * (jump.jumpHeldTime - jumpHeldTimer);
				jumpHeldTimer += Time.deltaTime;
			}
		}
		if (jumpButtonTimer > 0.0f) jumpButtonTimer -= frameTime;
		if (jumpButtonTimer	<= 0.0f && grounded) {
			jumpCount = 0;	
		}
		
		// Wall jump
		if (wallJumpDirection != 0) {
			wallJumpTimer -= frameTime;	
			if (wallJumpTimer < 0.0f) wallJumpDirection = 0;
		}

		if (jump.canWallJump && characterInput.jumpButtonDown && wallJumpDirection != 0 && 
			((wallJumpDirection == -1 && characterInput.x > 0) ||
			 (wallJumpDirection == 1 && characterInput.x < 0))) {
			myParent = null;
			velocity.y = jump.jumpVelocity;
			jumpCount = 2;
			jumpButtonTimer = jump.jumpTimer;
			jumpHeldTimer = 0.0f;
			State = CharacterState.WALL_JUMPING;
		}
		
		// Animations
		if (velocity.y < -1.0f) {
			State = CharacterState.FALLING;
		} else if (startedClimbing){
			if (isClimbingUpOrDown) {
				State = CharacterState.CLIMBING;	
			} else {
				State = CharacterState.HOLDING;	
			}
		}
	
	}
	
	private void ApplyGravity() {
		velocity.y += (frameTime * Physics.gravity.y);
	}
	
	public bool IsGrounded(float offset){
		foreach (RaycastCollider foot in feetColliders) {
			if (foot.IsColliding(1 << backgroundLayer | 1 << passThroughLayer | 1 << climableLayer, offset)) return true;
		}
		return false;
	}
	
	void OnDrawGizmos(){
		if (feetColliders != null) {
			foreach (RaycastCollider c in feetColliders) {
				if (c.transform == null) c.transform =  transform ;
				c.DrawRayCast();	
			}
		}
		if (headColliders != null) {
			foreach (RaycastCollider c in headColliders) {
				if (c.transform == null) c.transform = transform;
				c.DrawRayCast();	
			}
		}
		if (sides != null) {
			foreach (RaycastCollider c in sides) {
				if (c.transform == null) c.transform = transform;
				c.DrawRayCast();	
			}
		}	
	}
}

public enum RC_Direction {UP, DOWN, LEFT, RIGHT};

[System.Serializable]
public class MovementDetails {

	public float walkSpeed = 3.0f;
	public float runSpeed = 5.0f;
	public float acceleration = 75.0f;	
	public float drag = 1.15f;	
	public float terminalVelocity = -20.0f;
	public float skinSize = 0.001f;
}

[System.Serializable]
public class SlopeDetails {
	public bool allowSlopes = false;
	public float slopeLookAhead = 0.5f;
	public float groundError = 0.05f;
	public float rotationSpeed = 0.25f;
}

[System.Serializable]
public class JumpDetails {
	public bool canDoubleJump = false;
	public bool canWallJump = false;
	public float jumpVelocity = 10.0f;	
	public float doubleJumpVelocity = 8.0f;	
	public float jumpTimer = 0.2f;
	public float jumpHeldTime = 0.25f;
	public float jumpFrameVelocity = 25.0f;
	public float wallJumpTime = 0.33f;
	public float drag = 1.01f;
}

[System.Serializable]
public class ClimbDetails {
	public bool autoStick = false;
	public bool allowClimbing = true;
	public float speed = 2.5f;
	public int collidersRequired = 5;
}

[System.Serializable]
public class RaycastCollider {
	public Transform transform;
	public Vector3 offset;
	public float distance = 1;	
	public RC_Direction direction;
	
	private Vector3 staticOffset;	
	
	public bool IsColliding() {
		return Physics.Raycast (transform.position + transform.localRotation * offset, transform.localRotation * GetVectorForDirection(), distance);
	}
	
	public bool IsColliding(int layerMask) {
		return Physics.Raycast (transform.position + transform.localRotation * offset, transform.localRotation * GetVectorForDirection(), distance, layerMask);
	}
	
	public bool IsColliding(int layerMask, float skinSize) {
		return Physics.Raycast (transform.position + transform.localRotation * offset, transform.localRotation * GetVectorForDirection(), distance + skinSize, layerMask);
	}
	
	public RaycastHit GetCollision() {
		RaycastHit hit;
		Physics.Raycast (transform.position + transform.localRotation * offset, transform.localRotation * GetVectorForDirection(), out hit, distance);
		return hit;	
	}
	
	public RaycastHit GetCollision(int layerMask) {
		RaycastHit hit;
		Physics.Raycast (transform.position + transform.localRotation * offset, transform.localRotation * GetVectorForDirection(), out hit, distance, layerMask);
		return hit;	
	}
	
	public RaycastHit GetCollision(int layerMask, float extraDistance) {
		RaycastHit hit;
		Physics.Raycast (transform.position + transform.localRotation * offset, transform.localRotation * GetVectorForDirection(), out hit, distance + extraDistance, layerMask);
		return hit;	
	}
	
	public void DrawRayCast ()
	{
		if (transform != null) {
			switch (direction) {
				case RC_Direction.DOWN: Gizmos.color = Color.green; break;
				case RC_Direction.RIGHT: Gizmos.color = Color.red;  break;
				case RC_Direction.LEFT: Gizmos.color = Color.yellow;break;
				case RC_Direction.UP: Gizmos.color = Color.magenta; break;
			}
			Vector3 position = transform.position + transform.localRotation * offset;
			
			Gizmos.DrawLine (position, position + ((transform.localRotation * GetVectorForDirection()) * distance));
		}
	}
	
	public Vector3 GetVectorForDirection(){
		switch (direction) {
			case RC_Direction.DOWN: return Vector3.up * -1;
			case RC_Direction.RIGHT: return Vector3.right;
			case RC_Direction.LEFT: return Vector3.right * -1;
			case RC_Direction.UP: return Vector3.up;
		}
		return Vector3.zero;
	}
	
}

[System.FlagsAttribute]
public enum CharacterState {IDLE, WALKING, RUNNING, JUMPING, FALLING, SLIDING, HOLDING, CLIMBING, WALL_JUMPING};

public class CharacterControllerEventDelegate {
	
}