using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Jump : MonoBehaviour
{
	// �W�����v�����
	private float jumpForce = 20.0f;

	private void OnTriggerEnter(Collider other)
	{
		// ������������̃^�O��Player�̎�
		if (other.gameObject.CompareTag("Player"))
		{
			// �������������Rigidbody���擾���āA������̗͂�������
			other.gameObject.GetComponent<Rigidbody>().AddForce(0, jumpForce, 0, ForceMode.Impulse);
		}
	}
}