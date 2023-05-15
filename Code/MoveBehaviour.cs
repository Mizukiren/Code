using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// キャラクターの移動制御クラス
/// </summary>
public class MoveBehaviour : PlayerBehaviour
{
	//他クラス共有変数
	public Vector3 _vec;
	public bool _isGround;              //地面に着地しているかどうか

	//メンバ変数
	private readonly float WALK_SPEED = 10.0f;	//歩き速度
	private readonly float RUN_SPEED = 20.0f;	//走り速度
	private readonly float AIM_SPEED = 5.0f;    //エイム時の移動速度
	private readonly float AIR_SPEED = 5.0f;    //空中での移動速度
	private readonly float JUMP_POWER = 15.0f;   //ジャンプ力

	private float HEIGHT;

	// Start is called before the first frame update
	void Start()
	{
		HEIGHT = GetComponent<CapsuleCollider>().height;
	}

	void FixedUpdate()
	{
		var vel = _vec;
		vel.y = Instance._rigid.velocity.y;
		Instance._rigid.velocity = vel;
	}

	public override void BaseUpdate()
	{
		var input = KeyInput();

		BaseMove(input);
		IsGround();
		Jump();
		MoveTurn(_vec);
		MoveAnimation(input);
	}

	public override void AimUpdate()
	{
		var input = KeyInput();

		AimMove(input);
		IsGround();
		Jump();
		MoveAnimation(input);
	}

	public override void ShootUpdate()
	{
		var input = KeyInput();

		AimMove(input);
		IsGround();
		Jump();
		MoveAnimation(input);
	}

	public override void ReloadUpdate()
	{
		var input = KeyInput();

		BaseMove(input);
		IsGround();
		Jump();
		MoveTurn(_vec);
		MoveAnimation(input);
	}

	/// <summary>
	/// キー入力量を戻り値で返す関数
	/// </summary>
	/// <returns>入力量</returns>
	private Vector3 KeyInput(){
		var input = Vector3.zero;

		if (Input.GetKey(KeyCode.W)) input.z += 1.0f;
		if (Input.GetKey(KeyCode.S)) input.z -= 1.0f;
		if (Input.GetKey(KeyCode.A)) input.x -= 1.0f;
		if (Input.GetKey(KeyCode.D)) input.x += 1.0f;

		return input;
	}

	/// <summary>
	/// 基本時の移動量を計算する関数
	/// </summary>
	/// <param name="input">入力量</param>
	private void BaseMove(Vector3 input)
	{
		var moveSpeed = 0.0f;
		var lerpSpeed = 0.0f;

		if (_isGround)
		{
			moveSpeed = WALK_SPEED;
			if (Input.GetKey(KeyCode.LeftShift)) moveSpeed = RUN_SPEED;

			lerpSpeed = 5.0f;
		}
		else
		{
			moveSpeed = AIR_SPEED;

			lerpSpeed = 2.0f;
		}

		var q = Quaternion.Euler(0.0f, Instance._camera.transform.eulerAngles.y, 0.0f);
		var vec = q * input.normalized * moveSpeed;

		_vec = Vector3.Lerp(_vec, vec, lerpSpeed * Time.deltaTime);
		if (_vec.magnitude < 0.001f) _vec = Vector3.zero;
	}

	/// <summary>
	/// エイム時の移動量を計算する関数
	/// </summary>
	/// <param name="input">入力量</param>
	private void AimMove(Vector3 input)
	{
		var moveSpeed = AIM_SPEED;

		var q = Quaternion.Euler(0.0f, Instance._camera.transform.eulerAngles.y, 0.0f);
		_vec = q * input.normalized * moveSpeed;
	}

	/// <summary>
	/// 着地しているかどうかの判定を行う関数
	/// </summary>
	private void IsGround()
	{
		RaycastHit hit;
		//レイヤーマスクを作成する
		int layerMask = 1 << LayerMask.NameToLayer("Default");

		if(Physics.BoxCast(transform.position + Vector3.up * HEIGHT / 2.0f, Vector3.one*0.2f,Vector3.down,out hit,Quaternion.identity, HEIGHT / 2.0f + 0.1f,layerMask) 
			&& Instance._rigid.velocity.y <= 0.0f){

			Instance._animator.SetBool("Jump", false);
			_isGround = true;
		}
		else
		{
			_isGround = false;
		}
	}

	/// <summary>
	/// ジャンプを実行する関数
	/// </summary>
	private void Jump(){
		if (Input.GetKeyDown(KeyCode.Space) && _isGround)
		{
			Instance._rigid.velocity += Vector3.up * JUMP_POWER;
			Instance._animator.SetBool("Jump", true);
			_isGround = false;
		}
	}

	/// <summary>
	/// 移動量によってキャラクターの向きを変更する関数
	/// </summary>
	/// <param name="vec">移動量</param>
	private void MoveTurn(Vector3 vec){
		if (_isGround)
		{
			var dir = new Vector3(vec.x, 0.0f, vec.z);
			if (dir.magnitude > 0.0f) transform.rotation = Quaternion.LookRotation(dir);
		}
	}

	/// <summary>
	/// 基礎移動に関するアニメーションを実行する関数
	/// </summary>
	/// <param name="input">入力量</param>
	private void MoveAnimation(Vector3 input)
	{
		Instance._animator.SetFloat("Speed", Mathf.InverseLerp(0.0f, RUN_SPEED, _vec.magnitude));
	}

}
