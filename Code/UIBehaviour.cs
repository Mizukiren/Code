using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI関連を管理するクラス
/// </summary>
public class UIBehaviour : PlayerBehaviour
{
	//他クラス共有変数
	public float _damegeAlpha;          //ダメージ演出イメージの透過度

	//メンバ変数
	public Image _damegeImg;                    //ダメージ演出イメージ
	public Text _loadNum;                       //装填残弾数テキスト
	public Text _CarryNum;                      //弾の所持数テキスト

	private readonly float FADE_SPEED = 0.2f;   //ダメージ演出のフェードアウトの速度

	// Start is called before the first frame update
	void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void LateUpdate()
	{
		_damegeAlpha = Mathf.MoveTowards(_damegeAlpha, 0.0f, FADE_SPEED * Time.deltaTime);
		_damegeImg.color = new Color(1.0f,1.0f,1.0f,_damegeAlpha);

		_loadNum.text = Instance._loadNum.ToString();
		_CarryNum.text = Instance._carryNum.ToString();
	}
}
