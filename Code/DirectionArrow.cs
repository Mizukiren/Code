using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// ダメージを受けた方向を表示する、矢印を管理するクラス
/// </summary>
public class DirectionArrow : MonoBehaviour
{
	private readonly float POS_SCALE = 0.8f;    //矢印の座標調整用
	private readonly float FADE_SPEED = 0.2f;   //矢印のフェードアウトの速度
	private Vector2 UI_SIZE;					//UIのキャンバスサイズ	

	private Camera _camera;						//カメラの情報
	private Player _player;						//プレイヤー情報
	
	private Image _arrowImg;					//矢印イメージ
		
	public float _arrowAlpha { set; get; }      //矢印の透過度
	public GameObject _owner{ set; get; }       //攻撃をした敵の情報

	// Start is called before the first frame update
	void Start()
    {
		UI_SIZE = FindObjectOfType<CanvasScaler>().referenceResolution;

		_camera = Camera.main;
		_player = FindObjectOfType<Player>();

		transform.SetParent(GameObject.Find("DirectionArrows").transform);
		_arrowImg = GetComponent<Image>();
    }

    // Update is called once per frame1
    void Update()
    {
		if (_owner == null)
		{
			Destroy(gameObject);
		}
		else{
            Vector3 dir = _owner.transform.position - _player.transform.position;
			var normalizedDir = new Vector3(dir.x, 0.0f, dir.z).normalized;

			var q = Quaternion.Euler(0.0f, -_camera.transform.eulerAngles.y, 0.0f);
			var finalDir = q * normalizedDir;
			var x = finalDir.x * UI_SIZE.x / 2.0f * POS_SCALE;
			var y = finalDir.z * UI_SIZE.y / 2.0f * POS_SCALE;
			var pos = new Vector3(x, y, 0.0f);

			transform.localPosition = pos;

			var rotZ = Mathf.Atan2(pos.y, pos.x) * Mathf.Rad2Deg - 90.0f;
			transform.localRotation = Quaternion.Euler(0.0f, 0.0f, rotZ);
		}
    }

	void LateUpdate()
	{
		_arrowAlpha = Mathf.MoveTowards(_arrowAlpha, 0.0f, FADE_SPEED * Time.deltaTime);
		_arrowImg.color = new Color(1.0f, 1.0f, 1.0f, _arrowAlpha);
		if (_arrowAlpha == 0.0f) Destroy(gameObject);
	}

	/// <summary>
	/// 重複するマーカーオブジェクトを取得する関数
	/// </summary>
	/// <returns></returns>
	public DirectionArrow GetDuplicateObject(GameObject owner)
	{
		//同じオーナによるマーカーができないようにチェック
		var check = FindObjectsOfType<DirectionArrow>().Where(x => x._owner == owner);
		if (check.Any()) return check.First();
		else return null;
	}
}
