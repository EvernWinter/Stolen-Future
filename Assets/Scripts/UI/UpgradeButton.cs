using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UpgradeType
{
    MaxHealth,
    Heal,
    ShootSpeed,
    ShootPoint,
    Damage,
    BulletType
}
public class UpgradeButton : MonoBehaviour
{
    [SerializeField] public UpgradeType upgradeType;
    [SerializeField] private PlayerController player;
    [SerializeField] private Image upgradeImage;
    [SerializeField] private Sprite[] upgradeSprite;
    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateButton()
    {
        switch (upgradeType)
        {
            case UpgradeType.MaxHealth:
                upgradeImage.sprite = upgradeSprite[0];
                break;
            case UpgradeType.Heal:
                upgradeImage.sprite = upgradeSprite[1];
                break;
            case UpgradeType.ShootSpeed:
                upgradeImage.sprite = upgradeSprite[2];
                break;
            case UpgradeType.ShootPoint:
                upgradeImage.sprite = upgradeSprite[3];
                break;
            case UpgradeType.Damage:
                upgradeImage.sprite = upgradeSprite[4];
                break;
        }
    }
    public void ChooseUpgrade()
    {
        switch (upgradeType)
        {
            case UpgradeType.MaxHealth:
                player.UpgradeMaxHealth();
                break;
            case UpgradeType.Heal:
                player.Heal();
                break;
            case UpgradeType.ShootSpeed:
                player.ShootingCooldown *= 0.8f;
                break;
            case UpgradeType.ShootPoint:
                player.UnlockShootPoint();
                break;
            case UpgradeType.Damage:
                player.Damage += 5 + ( 1 * Time.deltaTime);
                break;
        }
    }
}
