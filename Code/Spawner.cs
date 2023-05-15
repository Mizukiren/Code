using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject Prefab;

    public float MinTime = 20f;

    public float MaxTime = 30f;

    float Timer;

    public float LifeTime = 80f;

    // Start is called before the first frame update
    void Start()
    {
        Timer = Random.Range(MinTime, MaxTime);
    }

    // Update is called once per frame
    void Update()
    {
        Timer -= Time.deltaTime;
        if (Timer < 0)
        {
            Timer = Random.Range(MinTime, MaxTime);
            GameObject obj = Instantiate(Prefab, transform.position,
            Quaternion.identity);
            Destroy(obj, LifeTime);
        }

    }
}
