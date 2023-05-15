using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
	// ジャンプする力
	private float jumpForce = 20.0f;

	private void OnTriggerEnter(Collider other)
	{
		// 当たった相手のタグがPlayerの時
		if (other.gameObject.CompareTag("Player"))
		{
			// 当たった相手のRigidbodyを取得して、上向きの力を加える
			other.gameObject.GetComponent<Rigidbody>().AddForce(0, jumpForce, 0, ForceMode.Impulse);
		}
	}
}