using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 部位判定などのダメージ判定とコントロールを行うクラス
/// </summary>
public class DamageBehaviour : PlayerBehaviour
{
	//他クラス共有変数
	

	//メンバ変数
	private readonly int MAX_HP = 100;                //最大HP
	private readonly int HEAD_DAMAGE = 20;           //ヘッドショットされた時に受けるダメージ
	private readonly int BODY_DAMEGE = 10;           //ボディショットされた時に受けるダメージ

	private int _hp;                                //現在のHP

	// Start is called before the first frame update
	void Start()
    {
		_hp = MAX_HP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	/// <summary>
	/// 頭にダメージを受けた時の処理関数
	/// </summary>
	public void DamageHead()
	{
		_hp -= HEAD_DAMAGE;
		Instance._damegeAlpha = 1.0f;
		DeathCheck();
	}

	/// <summary>
	/// 体にダメージを受けた時の処理関数
	/// </summary>
	public void DamageBody()
	{
		_hp -= BODY_DAMEGE;
		Instance._damegeAlpha = 1.0f;
		DeathCheck();
	}

	/// <summary>
	/// 死亡判定をする関数
	/// </summary>
	private void DeathCheck()
	{
		//死亡判定・処理を書く
		if (_hp <= 0) Destroy(gameObject);
	}
}
