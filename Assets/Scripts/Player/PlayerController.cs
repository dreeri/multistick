using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float jumpForce = 11.0f;
	public float moveSpeed = 3.0f;
	public float bulletSpeed = 0.1f;
	public Transform bulletPrefab;
	public Transform[] playerArray = new Transform[numOfPlayers];

	static int numOfPlayers;
	Vector3 torsoHeight = new Vector3(0.0f, 0.4f, 0.0f);
	float[] horizontalAxis;
	float[] verticalAxis;
	Vector2[] aimDirection;
	bool[] jumpCheck;
	bool[] grounded;
	bool[] secondJump;
	bool[] shootCheck;
	Transform[] groundcheckleft; 
	Transform[] groundcheckright;
	Transform[] armObject;
	SkeletonAnimation[] spineAnimation; 
	string[] currentAnimation;

	void Start()
	{
		numOfPlayers = playerArray.Length;

		horizontalAxis = new float[numOfPlayers];
		verticalAxis = new float[numOfPlayers];
		aimDirection = new Vector2[numOfPlayers];
		jumpCheck = new bool[numOfPlayers];
		grounded = new bool[numOfPlayers];
		secondJump = new bool[numOfPlayers];
		shootCheck = new bool[numOfPlayers];
		groundcheckleft = new Transform[numOfPlayers]; 
		groundcheckright = new Transform[numOfPlayers];
		armObject = new Transform[numOfPlayers];
		spineAnimation = new SkeletonAnimation[numOfPlayers]; 
		currentAnimation = new string[numOfPlayers];

		for (int i = 0; i < playerArray.Length; i++) {
			armObject[i] = playerArray[i].transform.Find("character_/SkeletonUtility-Root/root/fews");
			groundcheckleft[i] = playerArray[i].transform.Find("GroundCheck_left");
			groundcheckright[i] = playerArray[i].transform.Find("GroundCheck_right");
			spineAnimation[i] = playerArray[i].transform.Find("character_").GetComponent<SkeletonAnimation>();
		}
	}

	void Update()
	{
		InputController ();
	}

	void FixedUpdate()
	{
		PlayerMovement();
		Shooting ();
		GroundCheck();
		UpdateAnimations();
	}

	void PlayerMovement() 
	{
		for (int i = 0; i < playerArray.Length; i++) {
			if(horizontalAxis[i] > 0)
			{
				playerArray[i].transform.Translate( Vector2.right * Time.deltaTime * moveSpeed);
				playerArray[i].transform.Find("character_").localRotation = Quaternion.Euler(0, 0, 0);
			}
			if(horizontalAxis[i] < 0)
			{
				playerArray[i].transform.Translate(Vector2.left * Time.deltaTime * moveSpeed);
				playerArray[i].transform.Find("character_").localRotation = Quaternion.Euler(0, 180, 0);
			}

			if(jumpCheck[i] && (grounded[i] || secondJump[i]))
			{
				jumpCheck[i] = false;
				playerArray[i].GetComponent<Rigidbody2D>().velocity = new Vector2(playerArray[i].GetComponent<Rigidbody2D>().velocity.x , jumpForce);
				if(!grounded[i])
				{
					secondJump[i] = false;
				}
			}
		}
	}

	void InputController ()
	{
		for (int i = 0; i < playerArray.Length; i++) {
			horizontalAxis[i] = Input.GetAxis("Player"+i+"Horizontal");
			verticalAxis[i] = Input.GetAxis("Player"+i+"Vertical");
			aimDirection[i] = new Vector2(horizontalAxis[i], verticalAxis[i]);

			if(Input.GetButtonDown("Player"+i+"Jump") && jumpCheck[i] == false)
			{
				jumpCheck[i] = true;
			}
			if(Input.GetButtonUp("Player"+i+"Jump") && jumpCheck[i] == true)
			{
				jumpCheck[i] = false;
			}

			if(Input.GetButtonDown("Player"+i+"Shoot") && shootCheck[i] == false)
			{
				shootCheck[i] = true;
			}
			if(Input.GetButtonUp("Player"+i+"Shoot") && shootCheck[i] == true)
			{
				shootCheck[i] = false;
			}

			if(Input.GetButtonDown("Player"+i+"Reset"))
			{
				playerArray[i].transform.position = new Vector3( 0f, -0.5f, 0f);
			}
		}
	}

	void Shooting()
	{
		for (int i = 0; i < playerArray.Length; i++) {
			RaycastHit2D hit = Physics2D.Raycast (playerArray [i].transform.position + torsoHeight, aimDirection [i]);
			if (hit.collider != null) {
				Debug.DrawLine (playerArray [i].transform.position + torsoHeight, hit.point, Color.blue);
				armObject [i].position = new Vector3 (hit.point.x, hit.point.y, 0);
			}
			
			if (shootCheck [i]) {
				shootCheck [i] = false;
				Quaternion bulletRotation = Quaternion.LookRotation (Vector3.forward, new Vector3 (hit.point.x, hit.point.y, 0) - playerArray [i].transform.position);
				Transform bullet = (Transform)Instantiate (bulletPrefab, playerArray [i].transform.position + torsoHeight, bulletRotation);
				if (hit.transform.tag != playerArray [i].transform.tag && hit.transform.tag.Contains ("Player")) {
					hit.transform.position = new Vector3 (9f, -0.5f, 0f);
				}
			}
		}
	}

	void GroundCheck()
	{	
		for (int i = 0; i < playerArray.Length; i++) {
			if(Physics2D.Linecast (playerArray[i].transform.position, groundcheckleft[i].position, 1 << LayerMask.NameToLayer ("Ground")) 
					|| Physics2D.Linecast (playerArray[i].transform.position, groundcheckright[i].position, 1 << LayerMask.NameToLayer ("Ground"))) 
			{
				grounded[i] = true;
			}
			else
			{
				grounded[i] = false;
			}
			
			if(grounded[i] && !secondJump[i])
			{
				secondJump[i] = true;
			}
		}
	}

	void UpdateAnimations()
	{
		for (int i = 0; i < playerArray.Length; i++) {
			if(horizontalAxis[i] != 0 && grounded[i]) 											
			{
				SetAnimation(spineAnimation[i], "run", true, i);
			}
			else if(grounded[i] && horizontalAxis[i] == 0)
			{
				SetAnimation(spineAnimation[i], "idle", true, i);
			}
			else if(!grounded[i]) 												
			{
				SetAnimation(spineAnimation[i], "jump", false, i); 
			}
		}
	}

	void SetAnimation(SkeletonAnimation skeleton, string name, bool loop, int i) 
	{
		if (name == currentAnimation[i]) {
			return;
		}
		skeleton.state.SetAnimation (0, name, loop);
		currentAnimation[i] = name;
	}
}