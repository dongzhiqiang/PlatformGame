using UnityEngine;
using System.Collections;

public abstract class RaycastCharacterInput : MonoBehaviour {
	
	virtual public float x{get; protected set;}
	virtual public float y{get; protected set;}
	
	virtual public bool jumpButtonHeld{get; protected set;}
	virtual public bool jumpButtonDown{get; protected set;}
}