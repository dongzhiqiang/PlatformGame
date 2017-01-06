using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/**
 * A sample animation classes that workws with SpriteManager 1
 * which can be downloaded for free at: http://wiki.unity3d.com/index.php?title=SpriteManager
 * 
 * To use add sprite manager to your project and uncomment below.
 */ 
/*
public class SpriteAnimator : MonoBehaviour {
	
	public SpriteManager manager;
	public RaycastCharacterController controller;
	public AnimationData[] animationData;
	public Texture2D[] spriteSheets;
	public float spriteSizeX = 0.125f;
	public float spriteSizeY = 0.1f;
	
	private Sprite sprite;
	private Dictionary<string, AnimationData> animations;
	private bool jumping;
	
	// Use this for initialization
	void Start () {
		
		// Initialise sprite and animations
		sprite = manager.AddSprite(gameObject,2f,2f, Vector2.zero, new Vector2(spriteSizeX, spriteSizeY), false);
		animations = new Dictionary<string, AnimationData>();
		for (int i = 0; i < animationData.Length; i++) {
			UVAnimation current = new UVAnimation();
			current.BuildUVAnim(new Vector2(animationData[i].xPos * spriteSizeX, animationData[i].yPos * spriteSizeY), 
								new Vector2(spriteSizeX, spriteSizeY), animationData[i].columns, animationData[i].rows,
								animationData[i].frames, animationData[i].fps);
			current.name = animationData[i].name;
			current.loopCycles = animationData[i].loopCycles;
			animationData[i].animation = current;
			animations.Add(animationData[i].name, animationData[i]);
		}
		
		sprite.PlayAnim(animations["idle"].animation);
		sprite.SetAnimCompleteDelegate(AnimationComplete);
		
		RegisterListeners();
	}
	
	private void RegisterListeners(){
		// Register listener
		controller.Idle += new RaycastCharacterController.CharacterControllerEventDelegate (Idle);
		controller.Walk += new RaycastCharacterController.CharacterControllerEventDelegate (Walk);
		controller.Run += new RaycastCharacterController.CharacterControllerEventDelegate (Run);
		controller.Jump += new RaycastCharacterController.CharacterControllerEventDelegate (Jump);
		controller.Fall += new RaycastCharacterController.CharacterControllerEventDelegate (Fall);
	}

	
	public void PlayAnimation(AnimationData animationData){
		sprite.PlayAnim(animationData.animation);
	}
	
	public void Idle (CharacterState previousState) {
		if (!jumping) {
			if (previousState != CharacterState.IDLE ) {
				PlayAnimation(animations["idle"], controller.veloicty.x);
			}
		}
	}
	
	
	public void Walk (CharacterState previousState) {
		if (!jumping){
			if (previousState == CharacterState.RUNNING) {
				// Play from 'aligned' frame to ensure no jerks (walk animation must be aligned with run)
				// This method is in SM1, but ask and I can send you my update
				// sprite.PlayAnimFromCurrentFrame(runAnimation);
				PlayAnimation(animations["walk"], controller.veloicty.x);
			} else if (previousState != CharacterState.WALKING){
				PlayAnimation(animations["walk"], controller.veloicty.x);
			}
		}
	}
	
	public void Run (CharacterState previousState) {
		if (!jumping) {
			if (previousState == CharacterState.WALKING) {
				// Play from 'aligned' frame to ensure no jerks (walk animation must be aligned with run)
				// This method is in SM1, but ask and I can send you my update
				// sprite.PlayAnimFromCurrentFrame(runAnimation);
				PlayAnimation(animations["run"], controller.veloicty.x);
			} else if (previousState != CharacterState.RUNNING){
				PlayAnimation(animations["run"], controller.veloicty.x);
			}
		}
	}
	
	public void Jump (CharacterState previousState) {
		
		jumping = true;
		PlayAnimation(animations["jump"], controller.veloicty.x);
	}
	
	public void Fall (CharacterState previousState) {
		if (previousState != CharacterState.FALLING) {
			PlayAnimation(animations["fall"], controller.veloicty.x);
		}
	}
	
	public void AnimationComplete () {
		jumping = false;
	}
	
}


[System.Serializable]
public class AnimationData {
	public string name;
	public int xPos;
	public int yPos;
	public int rows;
	public int columns;
	public int frames;
	public float fps;
	public int spriteSheet;
	public int loopCycles;
	public UVAnimation animation;
}

*/