using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Point : MonoBehaviour
{
    int point = 0;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(point == 10)
        {
            SceneManager.LoadScene("GameClear");
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
       
        //è’ìÀÇµÇΩëŒè€ÇÃÉ^ÉOÇîªï 
        if (collision.gameObject.tag == "Player")
        {
            point++;
            Destroy(gameObject);
        }
    }
}