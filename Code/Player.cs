using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤーの基礎クラス。他クラスで使用する変数、コンポーネントを管理する。
/// </summary>
public class Player : MonoBehaviour
{
	public enum STATE {
		BASE,
		AIM,
		SHOOT,
		RELOAD,
	}
	public STATE _state;						//プレイヤーの状態管理

	public Animator _animator;					//アニメーション用
	public Rigidbody _rigid;					//リジッドボディ

	private MoveBehaviour _moveBehaviour;
	private AimBehaviour _aimBehaviour;
	private FireBehaviour _fireBehaviour;
	private UIBehaviour _uiBehaviour;
	private DamageBehaviour _damageBehaviour;

	private List<PlayerBehaviour> _playerBehaviours = new List<PlayerBehaviour>();

	//MoveBehaviour関連
	public Vector3 _vec 
	{
		get { return _moveBehaviour._vec; }
	}

	public bool _isGround
	{
		get { return _moveBehaviour._isGround; }
		set { _moveBehaviour._isGround = value; }
	}

	//AimBehaviour関連
	public float AIM_LOCK_MIN {
		get { return _aimBehaviour.AIM_CROSS_MIN; }
	}

	public float AIM_LOCK_AMP
	{
		get { return _aimBehaviour.AIM_CROSS_AMP; }
	}

	public Camera _camera
	{
		get { return _aimBehaviour._camera; }
		set { _aimBehaviour._camera = value; }
	}

	public float _aimLockLerp {
		get { return _aimBehaviour._aimLockLerp; }
		set { _aimBehaviour._aimLockLerp = value; }
	}

	public Vector3 _camAngle
	{
		get { return _aimBehaviour._camAngle; }
		set { _aimBehaviour._camAngle = value; }
	}

	public void AimEnd(){
		_aimBehaviour.AimEnd();
	}

	//FireBehaviour関連
	public int BULLET_LOAD
	{
		get { return _fireBehaviour.BULLET_LOAD; }
	}

	public int BULLET_CARRY
	{
		get { return _fireBehaviour.BULLET_CARRY; }
	}

	public int _loadNum
	{
		get { return _fireBehaviour._loadNum; }
		set { _fireBehaviour._loadNum = value; }
	}

	public int _carryNum
	{
		get { return _fireBehaviour._carryNum; }
		set { _fireBehaviour._carryNum = value; }
	}

	public void ShootEnd()
	{
		_fireBehaviour.ShootEnd();
	}

	//UIBehaviour関連
	public float _damegeAlpha
	{
		get { return _uiBehaviour._damegeAlpha; }
		set { _uiBehaviour._damegeAlpha = value; }
	}

	//DamageBehaviour関連
	public void DamageHead()
	{
		_damageBehaviour.DamageHead();
	}

	/// <summary>
	/// 体にダメージを受けた時の処理関数
	/// </summary>
	public void DamageBody()
	{
		_damageBehaviour.DamageBody();
	}

	void Awake()
	{
		_animator = GetComponent<Animator>();
		_rigid = transform.root.GetComponent<Rigidbody>();

		_moveBehaviour = GetComponent<MoveBehaviour>();
		_aimBehaviour = GetComponent<AimBehaviour>();
		_fireBehaviour = GetComponent<FireBehaviour>();
		_uiBehaviour = GetComponent<UIBehaviour>();
		_damageBehaviour = GetComponent<DamageBehaviour>();

		_playerBehaviours.Add(_moveBehaviour);
		_playerBehaviours.Add(_aimBehaviour);
		_playerBehaviours.Add(_fireBehaviour);
		_playerBehaviours.Add(_uiBehaviour);
		_playerBehaviours.Add(_damageBehaviour);

		Cursor.visible = false;
		Cursor.lockState = CursorLockMode.Locked;
	}

	// Start is called before the first frame update
	void Start()
    {
	}

	void Update()
	{
		if (_state == STATE.BASE)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.BaseUpdate();
		}
		else if (_state == STATE.AIM) {
			foreach (var behaviour in _playerBehaviours) behaviour.AimUpdate();
		}
		else if (_state == STATE.SHOOT)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.ShootUpdate();
		}
		else if (_state == STATE.RELOAD)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.ReloadUpdate();
		}
	}

	void LateUpdate()
	{
		if (_state == STATE.BASE)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.BaseLateUpdate();
		}
		else if (_state == STATE.AIM)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.AimLateUpdate();
		}
		else if (_state == STATE.SHOOT)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.ShootLateUpdate();
		}
		else if (_state == STATE.RELOAD)
		{
			foreach (var behaviour in _playerBehaviours) behaviour.ReloadLateUpdate();
		}
	}
}

public class PlayerBehaviour : MonoBehaviour
{
	protected Player Instance;

	private void Awake()
	{
		Instance = FindObjectOfType<Player>();
	}

	public virtual void BaseUpdate() { }
	public virtual void AimUpdate() { }
	public virtual void ShootUpdate() { }
	public virtual void ReloadUpdate() { }

	public virtual void BaseLateUpdate() { }
	public virtual void AimLateUpdate() { }
	public virtual void ShootLateUpdate() { }
	public virtual void ReloadLateUpdate() { }
}
