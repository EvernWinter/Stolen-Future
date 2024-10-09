using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public struct PlayerData
{
    public string playerName;
    public Sprite profileSprite;
    public int rankNumber;
    public int playerScore;

    public PlayerData(string playerName, Sprite profileSprite, int rankNumber, int playerScore)
    {
        this.playerName = playerName;
        this.profileSprite = profileSprite;
        this.rankNumber = rankNumber;
        this.playerScore = playerScore;
    }
}

public class RankData : MonoBehaviour
{
    public PlayerData playerData;
    
    [SerializeField] private Image profileImg;

    [SerializeField] private TMP_Text rankText;

    [SerializeField] private TMP_Text playerNameText;

    [SerializeField] private TMP_Text scoreText;


    public void UpdateData()
    {
        profileImg.sprite = playerData.profileSprite;
        rankText.text = playerData.rankNumber.ToString();
        playerNameText.text = playerData.playerName;
        scoreText.text = playerData.playerScore.ToString("0");
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
