/*
因为学校project的代码不能公开，所以选用这段在暑假练习使用Unity3D时写的代码。
这是一个Unity3d 2D project的一个角色控制的C# script. Standard asset 里面有
类似的 script 但是由于是练习，我从头开始写了一个新的Character的script
以实现standard asset里类似的功能及一些我自定义的功能
 */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class myCharacter : MonoBehaviour {
	
	public int Health = 0;
	public int currentMaxHealth = 5;
	public int movePower = 1;
	public int jumpPower = 1;
	public bool playerMode = true;
	public MyCameraShake2D cameraShake;

	private int faceRight = 1;  //-1 face left
	private const int MAXHEALTH = 30;
	private float rollStartTime = 0.0f;
	private bool onGround = false;
	private bool rolling = false;
	private bool Jumping = false;
	private bool doubleJumped = false;
	private bool Death = false;
	private bool inDeathAnimation = false;
	private bool controllable = true; 
	private float maxSpeed = 3.0f;
	private float maxWalkSpeed = 1.0f;
	private float maxJumpSpeed = 10.0f;
	private float currentHorizonSpeed = 0f;
	private string name = "";
	private Rigidbody2D myRigidbody;
	private Animator myAnimator;
	private myGroundCheck[] childGroundChecks;
	[SerializeField] private GameObject healthUI;

	// Use this for initialization
	private void Awake () {
		myRigidbody = gameObject.GetComponent<Rigidbody2D> ();
		myAnimator = gameObject.GetComponent<Animator> ();
		childGroundChecks = gameObject.GetComponentsInChildren<myGroundCheck> ();

		currentHorizonSpeed = 0.0f;
		Debug.Log ("myCharacter wake");
	}

	private void Start () {
		if (playerMode) {
			healthUI.GetComponent<healthBarManager> ().setHealth (Health);
		}
		Debug.Log ("myCharacter Start");
	}
	
	// Update is called once per frame
	private void Update () {
		myAnimator.SetBool ("Grounded", onGround);
	}

	private void FixedUpdate () {

		onGround = childGroundChecks [0].groundCheck ();
		myAnimator.SetBool ("Grounded", onGround);

		if (rolling && Time.time - rollStartTime >= 0.5f) {
			rolling = false;
			myAnimator.SetBool ("Rolling", rolling);
			gameObject.GetComponent<BoxCollider2D> ().enabled = true;
			movePower -= 2;
		}

		if (rolling) {
			gameObject.GetComponent<BoxCollider2D> ().enabled = false;
		}


		if (Death && !inDeathAnimation) {
			myAnimator.SetInteger ("Death", 0);
			inDeathAnimation = true;
		}

		if (Health <= 0 && !Death) {
			myAnimator.SetInteger ("Death", -1);
			gameObject.GetComponent<myController> ().enabled = false;
			controllable = false;
			Death = true;
		}

		if (!controllable && Health > 0 && !Death) {
			if (!myAnimator.GetCurrentAnimatorStateInfo (0).IsName ("Base Layer.backToLiveAnimation")) {
				if (playerMode) { 
					gameObject.GetComponent<myController> ().enabled = true;
					controllable = true;
				}
			}
		}

	}


	//////////////////////////////////Public API////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////

	public int changeHealth(int increment){
		Debug.Log ("Setting Health");
		Health += increment;

		if (playerMode) {
			healthUI.GetComponent<healthBarManager> ().setHealth (Health);
		}

		if (increment < 0 && Health > 0 && playerMode) {
			cameraShake.ShakeCamera (0.1f, 1.0f);
		}
		return Health;
	}
		
	public float getCurrentSpeedDirection () {
		return faceRight;
	}
		
	public void move(float momentum) {	
		while (Mathf.Abs (momentum) > 1.0f) {
			momentum /= 5.0f;
		}
		if (momentum * faceRight < 0) {
			turnAround ();
		}	
		myRigidbody.velocity = new Vector2(momentum * maxSpeed *  movePower,
											myRigidbody.velocity.y);
		currentHorizonSpeed = myRigidbody.velocity.x;
		//update the animator condition value
		myAnimator.SetFloat("Speed", Mathf.Abs(currentHorizonSpeed));
	}
		
	public void roll () {
		if (!rolling && Mathf.Abs(myRigidbody.velocity.x) > 1 && onGround) {
			rolling = true;
			myAnimator.SetBool ("Rolling", rolling);
			rollStartTime = Time.time;
			movePower += 2;	//move faster when rolling 
		}
	}
		
	public void jump() {
		if (onGround) {
			doubleJumped = false;
			myRigidbody.AddForce (jumpPower * new Vector2(0.0f, 150.0f));
		} else if (!doubleJumped) {
			doubleJumped = true;
			myRigidbody.velocity = new Vector2 (myRigidbody.velocity.x, 0);
			myRigidbody.AddForce (jumpPower * new Vector2(0.0f, 150.0f));
		}

		onGround = false;
	}   
		
	public void turnAround () {
		Transform myTransform = gameObject.GetComponent<Transform> ();
		Vector3 localScale =  myTransform.localScale;
		localScale.x *= -1;
		myTransform.localScale = localScale;
		faceRight *= -1;
	}

	public bool backToLive(int newHealth) {
		Debug.Log ("back to live is called");

		if (newHealth > 0 && Death && inDeathAnimation && myAnimator.GetInteger("Death") == 0) {
			Debug.Log ("back to liveing");
			changeHealth (newHealth - Health);
			myAnimator.SetInteger ("Death", 1); //-3 is the transition, go from death to idle
			Death = false;
			inDeathAnimation = false;
			return true;
		}
		return false;
	}

	public string getName(){
		return name;
	}

	public void setName(string newName) {
		name = newName;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////
	////////////////////////////////////////////////////////////////////////////////////////////////////

	//detect if touch groud
	void OnCollisionEnter2D(Collision2D collision){
		if(collision.collider.tag == "Ground"){
			onGround = true;
		}
	}

}
