using System.Collections;
using System.Collections.Generic;
using Microlight.MicroBar;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    [Header("Prefabs")]
    [SerializeField] MicroBar healthBar;
    [SerializeField] MicroBar levelBar;
    
    
    
    
    

    public void Damage(float health) 
    {
        if (healthBar != null)
        {
            Debug.Log($"Health!!!{health}");
            healthBar.UpdateBar(health, UpdateAnim.Damage);
        }
    }
    public void Heal(float health) 
    {
        // Update HealthBar
        //if(healthBar != null) healthBar.UpdateBar(health, false, UpdateAnim.Heal);
        
    }
    public void LevelUp(float level) 
    {
        if(levelBar != null) levelBar.UpdateBar(level, false, UpdateAnim.Damage);
        
    }

    public void SetHealth(float maxHealth)
    {
        Debug.Log($"Max : {maxHealth}");
        healthBar.Initialize(maxHealth);
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
