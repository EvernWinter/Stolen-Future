using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Proyecto26;
using SimpleJSON;
using System.Linq;

[System.Serializable]
public struct Ranking
{
    public List<PlayerData> playerData;
}
public class FireBaseRankingManager : MonoBehaviour
{
    public const string url = "https://stolen-future-default-rtdb.asia-southeast1.firebasedatabase.app/";
    public const string secret = "QHHoH1UaoJkHvgXHvJYZiN6kVxwdUxSNXrz9tCqb";

    [Header("Main")] 
    public RankUIManager rankUIManager;

    public Ranking ranking;

    [Header("New Data")] 
    public PlayerData currentPlayerData;
    private List<PlayerData> sortPlayerData = new List<PlayerData>();


    /*#region MyRegion
    [Header("Test")] 
    public int testNum;
    [System.Serializable]
    public struct TestData
    {
        public int num;
        public string name;
    }
    [System.Serializable]
    public struct TestObjectData
    {
        public string name;
        public TestData testData;
    }
    public TestData testData = new TestData();

    public void TestSetData()
    {
        string urlData = $"{url}TestData.json?auth={secret}";

        testData.name = "AAA";
        testData.num = 1;

        RestClient.Put<TestData>(urlData, testData).Then(response =>
        {
            Debug.Log($"Upload Data Completed");
        }).Catch(error =>
        {
            Debug.Log($"Error on set to server");
            Debug.Log($"{error.Message}");
        });
    }

    public void TestGetData()
    {
        string urlData = $"{url}TestData.json?auth={secret}";

        RestClient.Get(urlData).Then(response =>
        {
            Debug.Log(response.Text);
            JSONNode jsonNode = JSONNode.Parse(response.Text);
            testNum = jsonNode["num"];
        }).Catch(error =>
        {
            Debug.Log($"Error");
        });
    }
    

    #endregion*/
    
    // Start is called before the first frame update
    void Start()
    {
        // TestSetData();
        // TestGetData(); 
    }

    [ContextMenu("Set Local To DataBase")]
    public void SetLocalDataToDatabase()
    {
        string urlData = $"{url}Ranking.json?auth={secret}";
        RestClient.Put<Ranking>(urlData, ranking).Then(response =>
        {
            Debug.Log($"Upload Data Completed");
        }).Catch(error =>
        {
            Debug.Log("Error to set rankdata");
        }); 
    }
    
    private void SortRankData()
    {
        List<PlayerData> sortRankPlayer = new List<PlayerData>();
        sortRankPlayer = ranking.playerData.OrderByDescending(data => data.playerScore).ToList();
        for (int i = 0; i < sortRankPlayer.Count; i++)
        {
            PlayerData changeRankNum = sortRankPlayer[i];
            changeRankNum.rankNumber = i + 1;

            sortRankPlayer[i] = changeRankNum;
        }
        ranking.playerData = sortRankPlayer;
    }

    public void FindYourDataInRanking()
    {
        currentPlayerData = rankUIManager.yourRankData.playerData = ranking.playerData
            .Where(data => data.playerName == currentPlayerData.playerName).FirstOrDefault();
        rankUIManager.yourRankData.playerData = currentPlayerData;
        
        
        rankUIManager.yourRankData.UpdateData();
    }

    public void ReloadSortingData()
    {
        string urlData = $"{url}Ranking/playerData.json?auth={secret}";

        RestClient.Get(urlData).Then(response =>
        {
            Debug.Log(response.Text);
            JSONNode jsonNode = JSONNode.Parse(response.Text);

            ranking = new Ranking();
            ranking.playerData = new List<PlayerData>();

            for (int i = 0; i < jsonNode.Count; i++)
            {
                ranking.playerData.Add(new PlayerData(jsonNode[i]["playerName"], null, jsonNode[i]["rankNumber"],
                    jsonNode[i]["playerScore"]));
            }

            SortRankData();

            string urlPlayerData = $"{url}Ranking/.json?auth={secret}";

            RestClient.Put<Ranking>(urlPlayerData, ranking).Then(response =>
            {
                Debug.Log($"Upload Data Complete");
                rankUIManager.playerData = ranking.playerData;
                rankUIManager.ReloadRankData();
                FindYourDataInRanking();
            }).Catch(error =>
            {
                Debug.Log($"Error to get data from server");
            });
        }).Catch(error =>
        {
            Debug.Log("Error to get data from server");
        });
    }
    
    public void AddDataWithSorting()
    {
        string urlData = $"{url}Ranking/playerData.json?auth={secret}";

        RestClient.Get(urlData).Then(response =>
        {
            Debug.Log(response.Text);
            JSONNode jsonNode = JSONNode.Parse(response.Text);

            ranking = new Ranking();
            ranking.playerData = new List<PlayerData>();

            for (int i = 0; i < jsonNode.Count; i++)
            {
                ranking.playerData.Add(new PlayerData(jsonNode[i]["playerName"],null,jsonNode[i]["rankNumber"],jsonNode[i]["playerScore"]));
            }

            PlayerData checkPlayerData =
                ranking.playerData.FirstOrDefault(data => data.playerName == currentPlayerData.playerName);
            int indexOfPlayer = ranking.playerData.IndexOf(checkPlayerData);

            if (checkPlayerData.playerName != null)
            {
                checkPlayerData.playerScore = currentPlayerData.playerScore;
                ranking.playerData[indexOfPlayer] = checkPlayerData;
            }
            else
            {
                ranking.playerData.Add(currentPlayerData);
            }

            SortRankData();

            string urlPlayerData = $"{url}Ranking/.json?auth={secret}";

            RestClient.Put<Ranking>(urlPlayerData, ranking).Then(response =>
            {
                Debug.Log($"Upload Data Complete");
                rankUIManager.playerData = ranking.playerData;
                rankUIManager.ReloadRankData();
                FindYourDataInRanking();
            }).Catch(error =>
            {
                Debug.Log($"Error to get data from server");
            });
        });
    }
    // Update is called once per frame
    void Update()
    {
        
    }
    
    
}
