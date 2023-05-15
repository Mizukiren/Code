using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
	private readonly int MAX_HP = 100;				//最大HP
	private readonly int HEAD_DAMAGE = 20;			//ヘッドショットされた時に受けるダメージ
	private readonly int BODY_DAMEGE = 10;			//ボディショットされた時に受けるダメージ
	private readonly float MOVE_SPEED = 2.0f;		//移動速度
	private readonly float SHOOT_INTERVAL = 1.0f;	//射撃間隔

	public EnemyBullet _bullet;						//発射するオブジェクト

	private Player _player;							//プレイヤー情報
	private int _hp;								//現在のHP
	private float _shootTime;                       //発射間隔管理用

	// Start is called before the first frame update
	void Start()
    {
		_player = FindObjectOfType<Player>();
		_hp = MAX_HP;
	}

    // Update is called once per frame
    void Update()
    {
		Move();
		Shoot();
    }

	/// <summary>
	/// 敵の動きを制御する関数
	/// </summary>
	private void Move()
	{
		//プレイヤー目指してまっすぐ進んでくる
		var dir = _player.transform.position - transform.position;
		var vec = new Vector3(dir.x, 0.0f, dir.z).normalized * MOVE_SPEED;
		transform.position += vec * Time.deltaTime;
	}

	private void Shoot()
	{
		//常にプレイヤーの頭目掛けて100発100中の制度で射撃してくる
		//弾も無限
		if (Time.time - _shootTime > SHOOT_INTERVAL)
		{
			var head = _player.transform.Find("Head");
			var dir = head.position - transform.position;

			var pos = transform.position;
			var rot = Quaternion.LookRotation(dir);
			var obj = Instantiate(_bullet, pos, rot);
			obj._owner = gameObject;

			SoundManager.Instance.PlaySe("Fire");
			_shootTime = Time.time;
		}
	}

	/// <summary>
	/// 頭にダメージを受けた時の処理関数
	/// </summary>
	public void DamageHead(){
		_hp -= HEAD_DAMAGE;
		DeathCheck();
	}

	/// <summary>
	/// 体にダメージを受けた時の処理関数
	/// </summary>
	public void DamageBody()
	{
		_hp -= BODY_DAMEGE;
		DeathCheck();
	}

	/// <summary>
	/// 死亡判定をする関数
	/// </summary>
	private void DeathCheck()
	{
		if (_hp <= 0) Destroy(gameObject);	
	}
}
