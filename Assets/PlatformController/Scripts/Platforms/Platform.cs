using UnityEngine;
using System.Collections;

public class Platform : MonoBehaviour {
	
	public Vector3 velocity {get; protected set;}
	public Transform myTransform {get; protected set;}
	protected float frameTime;
	
	void Start () {
		myTransform = transform;
		DoStart();
	}
	
	void Update(){
		frameTime = Time.deltaTime;
		if (frameTime > RaycastCharacterController.maxFrameTime) frameTime = RaycastCharacterController.maxFrameTime;
		Move();
		DoUpdate();
	}
	
	virtual protected void DoStart() {
	}
	
	virtual protected void Move() {
		myTransform.Translate(velocity * frameTime);
	}
	
	virtual protected void DoUpdate() {
	}
	
	virtual public void DoAction(RaycastCollider collider, RaycastCharacterController character) {
		// Do nothing
	}
	
	virtual public bool ParentOnStand() {
		return false;	
	}
}
