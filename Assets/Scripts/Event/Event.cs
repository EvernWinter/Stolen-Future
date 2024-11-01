using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

public class Event : MonoBehaviour
{
    public GameObject[] map;

    public GameObject enemy;
    public GameObject player;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetEvent(bool isActive)
    {
        if (isActive)
        {
            foreach (var mapColor in map)
            {
                mapColor.GetComponent<SpriteRenderer>().color = new Color(121f / 255f, 9f / 255f, 255f / 255f);
            }

            enemy.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 145f / 255f, 0f / 255f);
            player.GetComponent<SpriteRenderer>().color = new Color(255f / 255f, 70f / 255f, 195f / 255f);
        }
        /*else
        {
            foreach (var mapColor in map)
            {
                mapColor.GetComponent<SpriteRenderer>().color = Color.white;
            }

            enemy.GetComponent<SpriteRenderer>().color = Color.white;
            player.GetComponent<SpriteRenderer>().color = Color.white;
        }*/
        
    }
}
