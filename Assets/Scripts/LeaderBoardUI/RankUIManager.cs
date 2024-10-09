using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RankUIManager : MonoBehaviour
{
    [SerializeField] public GameObject rankDataPrefabs;

    [SerializeField] public Transform rankPanel;

    [SerializeField] public List<PlayerData> playerData = new List<PlayerData>();

    [SerializeField] public List<GameObject> createdPlayerData = new List<GameObject>();

    public RankData yourRankData;
    // Start is called before the first frame update
    void Start()
    {
        CreatedRankData();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreatedRankData()
    {
        for (int i = 0; i < playerData.Count; i++)
        {
            GameObject rankObj = Instantiate(rankDataPrefabs, rankPanel);
            RankData rankData = rankObj.GetComponent<RankData>();

            rankData.playerData = new PlayerData();
            rankData.playerData.playerName = playerData[i].playerName;
            rankData.playerData.playerScore = playerData[i].playerScore;
            rankData.playerData.rankNumber = playerData[i].rankNumber;
            rankData.playerData.profileSprite = playerData[i].profileSprite;

            rankData.UpdateData();
            createdPlayerData.Add(rankObj);

        }
    }   

    private void ClearRankData()
    {
        foreach (GameObject createdData in createdPlayerData)
        {
            Destroy(createdData);
        }  
        createdPlayerData.Clear();
    }

    private void SortRankData()
    {
        List<PlayerData> sortRankPlayer = new List<PlayerData>();
        sortRankPlayer = playerData.OrderByDescending(data => data.playerScore).ToList();
        sortRankPlayer.ForEach(data =>  data.rankNumber = sortRankPlayer.IndexOf(data)+1);
        playerData = sortRankPlayer;
    }
    
    [ContextMenu("Reload")]
    public void ReloadRankData()
    {
        ClearRankData();
        SortRankData();
        CreatedRankData();
    }
}
