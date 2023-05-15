using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 射撃制御用クラス
/// </summary>
public class FireBehaviour : PlayerBehaviour
{
	//他クラス共有変数
	public readonly int BULLET_LOAD = 40;			//弾の最大装填数
	public readonly int BULLET_CARRY = 200;         //弾の最大所持数
	public int _loadNum;							//装填残弾数
	public int _carryNum;                           //弾の所持数

	//メンバ変数
	public Transform _firePoint;					//銃弾の発射位置
	public PlayerBullet _bullet;                    //発射するオブジェクト

	private float MAX_DISTANCE = 1000.0f;			//弾の最大飛距離

	private GameObject _muzzleFlash;                //マズルフラッシュオブジェクト

	private readonly float SHOOT_AMP_V_MIN = -1.0f;	//銃を撃った際の振れ幅最小値（垂直方向）
	private readonly float SHOOT_AMP_V_MAX = -2.0f; //銃を撃った際の振れ幅最大値（垂直方向）
	private readonly float SHOOT_AMP_H = 2.0f;      //銃を撃った際の振れ幅（水平方向）
	private readonly float FIRE_INTERVAL = 0.1f;    //発射間隔
	private readonly float AIM_SPREAD = 0.3f;		//銃を撃った際のクロスヘア拡大度合い
	private Vector2 UI_SIZE; 						//UIのキャンバスサイズ

	private float _shootTime;						//発射間隔管理用


	// Start is called before the first frame update
	void Start()
	{
		UI_SIZE = FindObjectOfType<CanvasScaler>().referenceResolution;
		_muzzleFlash = FxManager.Instance.PlayFx("MuzzleFlash", Vector3.zero, Quaternion.identity);
		_muzzleFlash.SetActive(false);

		_loadNum = BULLET_LOAD;
		_carryNum = BULLET_CARRY;
	}

	public override void BaseUpdate()
	{
		ReloadStandby();
	}


	public override void AimUpdate()
	{
		ShootStandby();
		ReloadStandby();
	}

	public override void ShootUpdate()
	{
		Shoot();
		ReloadStandby();
	}

	public override void ReloadUpdate()
	{
		
	}

	/// <summary>
	/// 射撃待機中の挙動を制御する関数
	/// </summary>
	private void ShootStandby(){
		if (Input.GetMouseButton(0) && _loadNum > 0)
		{
				_muzzleFlash.SetActive(true);
				_muzzleFlash.transform.position = _firePoint.position;
				_muzzleFlash.transform.rotation = _firePoint.rotation * Quaternion.Euler(Vector3.up * 180.0f);

				Instance._animator.SetBool("Shoot", true);
				Instance._state = Player.STATE.SHOOT;
		}
		else if(Input.GetMouseButtonDown(0) && _loadNum == 0)
		{
			SoundManager.Instance.PlaySe("Empty");
		}
	}

	/// <summary>
	/// 射撃中の挙動を制御する関数
	/// </summary>
	private void Shoot()
	{
		if (Input.GetMouseButton(0))
		{
			if (_loadNum > 0)
			{
				if (Time.time - _shootTime > FIRE_INTERVAL)
				{
					var ray = CreateShootRay();
					var dir = CalcFireDirection(ray);
					FireBullet(dir);
					CrossSpread();
					CameraShake();
				}

				_muzzleFlash.transform.position = _firePoint.position;
				_muzzleFlash.transform.rotation = _firePoint.rotation * Quaternion.Euler(Vector3.up * 180.0f);
			}
			else
			{ //弾切れ
				SoundManager.Instance.PlaySe("Empty");
				ShootEnd();
			}
		}
		else
		{
			ShootEnd();
		}
	}

	/// <summary>
	/// 射撃をした際、クロスヘアを拡大する関数
	/// </summary>
	private void CrossSpread()
	{
		Instance._aimLockLerp += AIM_SPREAD;
		if (Instance._aimLockLerp > 1.0f) Instance._aimLockLerp = 1.0f;
	}

	/// <summary>
	/// 射撃をした際、カメラを揺らす関数
	/// </summary>
	private void CameraShake()
	{
		//カメラのブレ
		var camAmpX = Random.Range(SHOOT_AMP_V_MIN, SHOOT_AMP_V_MAX);
		var camAmpY = Random.Range(-SHOOT_AMP_H, SHOOT_AMP_H);

		Instance._camAngle += new Vector3(camAmpX, camAmpY, 0.0f);
	}

	/// <summary>
	/// 射撃を終了する関数
	/// </summary>
	public void ShootEnd()
	{
		_muzzleFlash.SetActive(false);
		Instance._animator.SetBool("Shoot", false);
		Instance._state = Player.STATE.AIM;
	}

	/// <summary>
	/// クロスヘア内にからランダムに発射するレイを作成する関数
	/// </summary>
	/// <returns>カメラから飛ばすレイ</returns>
	private Ray CreateShootRay(){
		var amp = Instance.AIM_LOCK_MIN + Instance.AIM_LOCK_AMP * Instance._aimLockLerp;
		var ampX = Random.Range(-amp, amp); //クロスヘアの中心位置をプラス
		var ampY = Random.Range(-amp, amp);
		var viewX = 0.5f + ampX / UI_SIZE.x;
		var viewY = 0.5f + ampY / UI_SIZE.y;

		return Instance._camera.ViewportPointToRay(new Vector3(viewX, viewY, 0.0f));
	}

	/// <summary>
	/// 弾の発射方向を計算する関数
	/// </summary>
	/// <param name="ray">カメラから飛ばすレイ</param>
	/// <returns>弾の発射方向</returns>
	private Vector3 CalcFireDirection(Ray ray)
	{
		RaycastHit hit;
		//レイヤーマスクを作成する
		int layerMask = ~(1 << LayerMask.NameToLayer("Player"));
		if (Physics.Raycast(ray, out hit, MAX_DISTANCE, layerMask))
		{
			return hit.point - _firePoint.position;
		}
		else
		{
			//レイが当たらなかったら、最大飛距離向かってに弾を撃つ
			return ray.origin + ray.direction * MAX_DISTANCE - _firePoint.position;
		}
	}

	/// <summary>
	/// 弾を発射する関数
	/// </summary>
	/// <param name="ray">発射先を計算するレイ</param>
	private void FireBullet(Vector3 dir){
		var pos = _firePoint.position;
		var rot = Quaternion.LookRotation(dir);
		Instantiate(_bullet, pos, rot);

		_loadNum--;

		SoundManager.Instance.PlaySe("Fire");
		_shootTime = Time.time;
	}


	/// <summary>
	/// リロード待機中の挙動を制御する関数
	/// </summary>
	private void ReloadStandby(){
		if (Input.GetKeyDown(KeyCode.R) && _loadNum < BULLET_LOAD && _carryNum > 0)
		{
			Reload();
		}
	}


	/// <summary>
	/// リロード中の挙動を制御する関数
	/// </summary>
	private void Reload(){
		Instance.ShootEnd();
		Instance.AimEnd();
		SoundManager.Instance.PlaySe("Reload");
		Instance._animator.SetBool("Reload", true);
		Instance._state = Player.STATE.RELOAD;
	}


	/// <summary>
	/// リロードを終了する関数
	/// </summary>
	public void EndReload(){
		var replenishNum = BULLET_LOAD - _loadNum;

		//補充したい数が、残り弾数より多い時
		if (replenishNum > _carryNum){
			_loadNum += _carryNum;
			_carryNum = 0;
		}
		else{
			_loadNum += replenishNum;
			_carryNum -= replenishNum;
		}

		Instance._animator.SetBool("Reload", false);
		Instance._state = Player.STATE.BASE;
	}
}
