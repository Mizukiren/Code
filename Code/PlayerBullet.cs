using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// プレイヤー弾の挙動管理クラス
/// </summary>
public class PlayerBullet : MonoBehaviour
{
	private readonly float SPEED = 380.0f;  //弾速
	private bool _isHit = true;				//多重ヒット防止用

    // Start is called before the first frame update
    void Start()
    {
		//3秒後に削除
		Destroy(gameObject, 3.0f);
    }

    // Update is called once per frame
    void Update()
    {
		HitCheck();
		WallCheck();

		transform.position += transform.rotation * Vector3.forward * Time.deltaTime * SPEED;
	}


	/// <summary>
	/// 弾が当たり判定に当たったかどうかを確認する関数
	/// </summary>
	private void HitCheck()
	{
		var ray = new Ray();
		ray.origin = transform.position; //光線を発射する場所
		ray.direction = transform.rotation * Vector3.forward; //光線を発射する方向

		RaycastHit hit;
		var dis = SPEED * Time.deltaTime;

		if (Physics.Raycast(ray, out hit, dis))
		{
			HitEnemy(hit);
		}
	}

	/// <summary>
	/// 敵に当たったか確認し、ダメージを与える関数
	/// </summary>
	/// <param name="hit">当たったオブジェクトの情報</param>
	private void HitEnemy(RaycastHit hit)
	{
		if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Collision") && _isHit)
		{
			if (hit.collider.tag == "Enemy")
			{
				if (hit.collider.name == "Head") hit.transform.root.GetComponent<Enemy>().DamageHead();
				else if (hit.collider.name == "Body") hit.transform.root.GetComponent<Enemy>().DamageBody();

				_isHit = false;
			}
		}
	}

	/// <summary>
	/// 弾が壁などに当たったかどうかチェックをして、処理する関数
	/// </summary>
	private void WallCheck()
	{
		var ray = new Ray();
		ray.origin = transform.position; //光線を発射する場所
		ray.direction = transform.rotation * Vector3.forward; //光線を発射する方向

		RaycastHit hit;
		var dis = SPEED * Time.deltaTime;

		int layerMask = 1 << LayerMask.NameToLayer("Default");

		if (Physics.Raycast(ray, out hit, dis, layerMask))
		{
			var fx = FxManager.Instance.PlayFx("ImpactConcrete", hit.point, Quaternion.LookRotation(hit.normal));
			fx.transform.parent = hit.transform;
			Destroy(gameObject);
		}
	}

}
