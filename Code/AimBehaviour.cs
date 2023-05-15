using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// エイムの挙動管理クラス
/// </summary>
public class AimBehaviour : PlayerBehaviour
{
	//他クラス共有変数
	public readonly float AIM_CROSS_MIN = 0.0f;		//クロスヘアの最小サイズ
	public readonly float AIM_CROSS_AMP = 20.0f;	//クロスヘアの振れ幅

	public Camera _camera;							//メインカメラ
	public Vector3 _camAngle;						//カメラ角度
	public Vector3 _aimAngle;						//エイム角度
	public float _aimLockLerp = 0.0f;               //クロスヘアの位置変更 0～1で変動。

	public Vector3 _crossCenter = Vector3.zero;		//クロスヘアの中心座標

	//メンバ変数
	public RectTransform[] _crosshair;

	private readonly float TURN_SPEED = 180.0f;		//振り返りのスピード
	private readonly float ANGLE_LIMIT = 45.0f;     //限界射角（上下）

	private readonly float AIM_FIX = 20.0f;			//エイム時のキャラクターの向き補正
	private readonly float BASE_CAM_LEN = 4.0f;		//通常時のカメラの距離
	private readonly float AIM_CAM_LEN = 0.5f;		//エイム時のカメラの距離
	private Vector3 BASE_CAM_FIX;					//通常時のカメラの位置補正
	private Vector3 AIM_CAM_FIX;					//エイム時のカメラの位置補正	

	private readonly float AIM_POS_SPEED = 5.0f;	//通常↔エイムのカメラの切り替え速度
	private readonly float AIM_LOCK_SPEED = 2.0f;   //クロスヘア収束速度

	private readonly float AIM_CROSS_FIX = 10.0f;   //クロスヘアの大きさ補正
	private readonly float AIM_SWING_MAX = 3.0f;	//クロスヘアのブレ幅の最大値
	private readonly float AIM_SWING_SPEED = 180.0f;//クロスヘアの揺れ速度

	private Transform _spine,_hips,_root;			//ボーン情報
	private Vector3 _initialSpineRotation, _initialHipsRotation, _initialRootRotation;

	private float _aimSwingRadius = 0.0f;           //クロスヘアの揺れ幅（動くと大きくなる）
	private float _aimSwingAngle = 0.0f;			//クロスヘアの揺れ幅計算に使用する角度
	private float _aimPosLerp = 0.0f;               //カメラの位置変更 0～1で変動。
	private float _aimLockMin = 0.0f;				//クロスヘアの枠の最小幅度　動くと最小まで窄まらなくなる。

	// Start is called before the first frame update
	void Start()
    {
		_camera = Camera.main;
		_camAngle = transform.eulerAngles;

		BASE_CAM_FIX = new Vector3(0.0f, 1.5f, 0.0f);
		AIM_CAM_FIX = new Vector3(0.5f, 1.5f, -AIM_CAM_LEN);

		_spine = Instance._animator.GetBoneTransform(HumanBodyBones.Spine);
		_hips = Instance._animator.GetBoneTransform(HumanBodyBones.Hips);
		_root = _hips.root;

		_initialSpineRotation = _spine.localEulerAngles;
		_initialHipsRotation = _hips.localEulerAngles;
		_initialRootRotation = _root.localEulerAngles;
	}

	public override void BaseUpdate()
	{
		var input = MouseInput();
		CamRotation(input);

		AimStandby();

		NodAimCamera();
	}

	private void MoveCrossPosition()
	{
		var targetNum = 0.0f;
		if (!Instance._isGround) targetNum = 0.5f;
		else if (Instance._vec.magnitude > 0.0f) targetNum = 0.3f;
		else targetNum = 0.0f;

		_aimLockMin = Mathf.MoveTowards(_aimLockMin, targetNum, AIM_LOCK_SPEED * Time.deltaTime);
		_aimLockLerp = Mathf.MoveTowards(_aimLockLerp, _aimLockMin, AIM_LOCK_SPEED * Time.deltaTime);

		for (int i = 0; i < _crosshair.Length; i++)
		{
			_crosshair[i].localPosition = new Vector3(
				(AIM_CROSS_FIX + AIM_CROSS_MIN + AIM_CROSS_AMP * _aimLockLerp) * Mathf.Cos(90.0f * i * Mathf.Deg2Rad),
				(AIM_CROSS_FIX + AIM_CROSS_MIN + AIM_CROSS_AMP * _aimLockLerp) * Mathf.Sin(90.0f * i * Mathf.Deg2Rad),
				0.0f) + _crossCenter;
		}
	}

	public override void ReloadUpdate()
	{
		var input = MouseInput();
		CamRotation(input);

		NodAimCamera();
	}



	public override void BaseLateUpdate()
	{
		MoveCameraZoomOut();
	}

	public override void AimLateUpdate()
	{
		MoveCameraZoomIn();
	}

	public override void ShootLateUpdate()
	{
		MoveCameraZoomIn();
	}

	public override void ReloadLateUpdate()
	{
		MoveCameraZoomOut();
	}

	/// <summary>
	/// マウス移動量を戻り値で返す関数
	/// </summary>
	/// <returns>入力量</returns>
	private Vector3 MouseInput()
	{
		var input = Vector3.zero;

		input.x = Input.GetAxisRaw("Mouse X");
		input.y = Input.GetAxisRaw("Mouse Y");

		return input;
	}

	/// <summary>
	/// マウス移動量によってカメラを回転させる関数
	/// </summary>
	/// <param name="input">入力量</param>
	private void CamRotation(Vector3 input){
		_camAngle.x -= input.y * Time.deltaTime * TURN_SPEED;
		if (_camAngle.x > ANGLE_LIMIT) _camAngle.x = ANGLE_LIMIT;
		else if (_camAngle.x < -ANGLE_LIMIT) _camAngle.x = -ANGLE_LIMIT;

		_camAngle.y = (_camAngle.y + input.x * Time.deltaTime * TURN_SPEED) % 360.0f;
	}

	/// <summary>
	/// 非エイムの挙動を管理する関数
	/// </summary>
	private void AimStandby(){
		if (Input.GetMouseButton(1))
		{
			AimStart();
		}
	}

	/// <summary>
	/// エイムを開始する関数
	/// </summary>
	private void AimStart()
	{
		Instance._state = Player.STATE.AIM;
		Instance._animator.SetBool("Aim", true);
		foreach (var img in _crosshair) img.gameObject.SetActive(true);
		_crossCenter = Vector3.zero;
		_aimSwingRadius = 0;
	}

	/// <summary>
	/// エイム時の照準の揺れを作る関数
	/// </summary>
	private void AimSwing()
	{
		//_aimSwingRadiusをLerpで1f秒毎にAIME_SWING_MAXに近づける
		_aimSwingRadius = Mathf.Lerp(_aimSwingRadius, AIM_SWING_MAX, Time.deltaTime);

		//_aimSwingAngleを円を描くような角度になるよう作成
		//_aimSwingAngleにAIM_SWING_SPEEDを足して毎フレーム秒かける。それを360で割った余りを取得(度数に変換)
		_aimSwingAngle = (_aimSwingAngle + AIM_SWING_SPEED * Time.deltaTime) % 360.0f;

		//yの増加分をMathf.sin*_aimSwingRadiusでxの増加分ををMathf.cos*_aimSwingRadiusで作成
		var ampY = Mathf.Sin(_aimSwingAngle * Mathf.Deg2Rad) * _aimSwingRadius;
		var ampX = Mathf.Cos(_aimSwingAngle * Mathf.Deg2Rad) * _aimSwingRadius;

		//_camAngleのx,yに上記の増加分を足したものを_aimAngleに設定
		_aimAngle.y = _camAngle.y + ampY;
		_aimAngle.x = _camAngle.x + ampX;

		//_crossCenterに増加分のXYを設定
		_crossCenter = new Vector2(ampX, ampY);
	}

	/// <summary>
	/// 射撃時の照準用の関数
	/// </summary>
	private void AimShoot()
	{
		//_aimAngleに_camAngleを設定
		_aimAngle.y = _camAngle.y;
		_aimAngle.x = _camAngle.x;

		//_aimSwingRadiusをLerpを使って徐々に0にしていく
		_aimSwingRadius = Mathf.Lerp(_aimSwingRadius, 0.0f, Time.deltaTime);
		//_crossCenterをLerpを使って徐々に0にしていく
		_crossCenter = Vector3.Lerp(_crossCenter, Vector3.zero, Time.deltaTime);
	}
	/// <summary>
	/// エイム中の挙動を管理する関数
	/// </summary>
	private void Aim()
	{
		if (!Input.GetMouseButton(1))
		{
			AimEnd();
		}
	}

	/// <summary>
	/// エイムを終了する関数
	/// </summary>
	public void AimEnd(){
		Instance._animator.SetBool("Aim", false);
		foreach (var img in _crosshair) img.gameObject.SetActive(false);
		Instance._state = Player.STATE.BASE;
	}

	/// <summary>
	/// 非エイム時の照準（カメラ）を管理する関数
	/// </summary>
	private void NodAimCamera()
	{
		_aimAngle.y = _camAngle.y;
		_aimAngle.x = 0.0f;
	}

	public override void AimUpdate()
	{
		var input = MouseInput();
		CamRotation(input);

		Aim();

		AimTurn();
	}

	public override void ShootUpdate()
	{
		var input = MouseInput();
		CamRotation(input);

		AimShoot();
		AimTurn();
	}

	/// <summary>
	/// カメラのy軸に合わせて、キャラクターの向きを変更する関数
	/// </summary>
	private void AimTurn(){
		var eulerAngles = transform.eulerAngles;
		eulerAngles.y = _aimAngle.y + AIM_FIX;
		transform.eulerAngles = eulerAngles;
	}

	/// <summary>
	/// ズームアウト時のカメラの座標を計算し、変更する関数
	/// </summary>
	private void MoveCameraZoomOut()
	{
		Instance._camera.transform.eulerAngles = _camAngle;
		var basePos = transform.position + BASE_CAM_FIX + Instance._camera.transform.rotation * Vector3.back * BASE_CAM_LEN;

		var q = Quaternion.Euler(0.0f, Instance._camera.transform.eulerAngles.y, 0.0f);
		var fix = Mathf.Tan(_camAngle.x * Mathf.Deg2Rad) * AIM_CAM_LEN;
		var aimPos = transform.position + q * AIM_CAM_FIX + Vector3.up * fix;

		_aimPosLerp = Mathf.MoveTowards(_aimPosLerp, 0.0f, AIM_POS_SPEED * Time.deltaTime);
		Instance._camera.transform.position = Vector3.Lerp(basePos, aimPos, _aimPosLerp);
	}

	/// <summary>
	/// ズームイン時のカメラの座標を計算し、変更する関数
	/// </summary>
	private void MoveCameraZoomIn()
	{
		Instance._camera.transform.eulerAngles = _camAngle;
		var basePos = transform.position + BASE_CAM_FIX + Instance._camera.transform.rotation * Vector3.back * BASE_CAM_LEN;

		var q = Quaternion.Euler(0.0f, Instance._camera.transform.eulerAngles.y, 0.0f);
		var fix = Mathf.Tan(_camAngle.x * Mathf.Deg2Rad) * AIM_CAM_LEN;
		var aimPos = transform.position + q * AIM_CAM_FIX + Vector3.up * fix;


		_aimPosLerp = Mathf.MoveTowards(_aimPosLerp, 1.0f, AIM_POS_SPEED * Time.deltaTime);
		Instance._camera.transform.position = Vector3.Lerp(basePos, aimPos, _aimPosLerp);
	}

	public void OnAnimatorIK(int layerIndex)
	{
		//上下への射撃に合わせて体の向きを変更
		Quaternion targetRot = Quaternion.Euler(_aimAngle.x, transform.eulerAngles.y, 0);
		targetRot *= Quaternion.Euler(_initialRootRotation);
		targetRot *= Quaternion.Euler(_initialHipsRotation);
		targetRot *= Quaternion.Euler(_initialSpineRotation);

		Instance._animator.SetBoneLocalRotation(HumanBodyBones.Spine, Quaternion.Inverse(_hips.rotation) * targetRot);
	}
}
