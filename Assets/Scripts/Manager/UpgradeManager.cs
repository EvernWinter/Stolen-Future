using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UpgradeManager : MonoBehaviour
{
    [Header("Upgrade")]
    [SerializeField] private Button[] buttons;
    [SerializeField] private PlayerController player;
    [SerializeField] private GameObject upgradePanel;
    [SerializeField] private GameManager gameManager;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChooseUpgrade()
    {
        gameManager.PauseGame();
        upgradePanel.SetActive(true);

        // Create a list of all possible upgrade types
        List<UpgradeType> availableUpgrades = new List<UpgradeType>
        {
            UpgradeType.MaxHealth,
            UpgradeType.Heal,
            UpgradeType.ShootSpeed,
            UpgradeType.ShootPoint,
            UpgradeType.Damage
        };
        Shuffle(availableUpgrades);
        foreach (var button in buttons)
        {
            UpgradeType selectedUpgrade;
            
            while (true)
            {
                // Randomly select an upgrade type from the list
                int randIndex = Random.Range(0, availableUpgrades.Count);
                selectedUpgrade = availableUpgrades[randIndex];

                // Check conditions before assigning the upgrade type
                if (selectedUpgrade == UpgradeType.ShootPoint &&
                    player.GetComponent<PlayerController>().reservePositions.Count > 0)
                {
                    button.GetComponent<UpgradeButton>().upgradeType = selectedUpgrade;
                    button.GetComponent<UpgradeButton>().UpdateButton();
                    availableUpgrades.RemoveAt(randIndex);
                    break; // Exit the while loop once an upgrade is assigned
                }
                else if (selectedUpgrade == UpgradeType.ShootSpeed &&
                         player.GetComponent<PlayerController>().ShootingCooldown >= 0.5f)
                {
                    button.GetComponent<UpgradeButton>().upgradeType = selectedUpgrade;
                    button.GetComponent<UpgradeButton>().UpdateButton();
                    availableUpgrades.RemoveAt(randIndex);
                    break; // Exit the while loop once an upgrade is assigned
                }
                else if (selectedUpgrade == UpgradeType.MaxHealth || selectedUpgrade == UpgradeType.Heal || selectedUpgrade == UpgradeType.Damage)
                {
                    button.GetComponent<UpgradeButton>().upgradeType = selectedUpgrade;
                    button.GetComponent<UpgradeButton>().UpdateButton();
                    availableUpgrades.RemoveAt(randIndex);
                    break; // Exit the while loop once an upgrade is assigned
                }
                // If conditions are met or no special condition is required, assign and remove the upgrade type
            }
            Debug.Log($"Button {button.name} selected upgrade {selectedUpgrade}");
        }
    }
    
    private void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
