using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerStats : MonoBehaviour
{
    CharacterScriptableObject characterData;

    //Current stats
    float currentHealth;
    float currentRecovery;
    float currentMoveSpeed;
    float currentMight;
    float currentProjectileSpeed;
    float currentMagnet;

    #region Current Stats Properties
    public float CurrentHealth
    {
        get { return currentHealth; }
        set
        {
            if(currentHealth != value)
            {
                currentHealth = value;
                if(GameManager.instance != null)
                {
                    GameManager.instance.currentHealthDisplay.text = "Health: " + (int)currentHealth;
                }
            }
        }
    }

    public float CurrentRecovery
    {
        get { return currentRecovery; }
        set
        {
            if (currentRecovery != value)
            {
                currentRecovery = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + currentRecovery;
                }
            }
        }
    }

    public float CurrentMoveSpeed
    {
        get { return currentMoveSpeed; }
        set
        {
            if (currentMoveSpeed != value)
            {
                currentMoveSpeed = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + currentMoveSpeed;
                }
            }
        }
    }

    public float CurrentMight
    {
        get { return currentMight; }
        set
        {
            if (currentMight != value)
            {
                currentMight = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMightDisplay.text = "Might: " + currentMight;
                }
            }
        }
    }

    public float CurrentProjectileSpeed
    {
        get { return currentProjectileSpeed; }
        set
        {
            if (currentProjectileSpeed != value)
            {
                currentProjectileSpeed = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + currentProjectileSpeed;
                }
            }
        }
    }

    public float CurrentMagnet
    {
        get { return currentMagnet; }
        set
        {
            if (currentMagnet != value)
            {
                currentMagnet = value;
                if (GameManager.instance != null)
                {
                    GameManager.instance.currentMagnetDisplay.text = "Magnet: " + currentMagnet;
                }
            }
        }
    }
    #endregion

    public ParticleSystem damageEffect;

    //Experence and level of player
    [Header("Experience/Level")]
    public int experience = 0;
    public int level = 1;
    public int experienceCap = 100; //the max amount exp need to up to lv2 when start game
    public int experienceCapIncrease; //the max amount exp need for every level

    //i-frames
    [Header("I-Frames")]
    public float invincibilityDuration;
    float invincibilityTimer;
    bool isInvincible;

    InventoryManager inventory;
    public int weaponIndex;
    public int passiveItemIndex;

    [Header("UI")]
    public Image healthBar;
    public Image experienceBar;
    public TMP_Text levelText;

    //public GameObject firstPassiveItemTest, secondPassiveItemTest;
    //public GameObject weaponTest;

    void Awake()
    {
        characterData = CharacterSelector.GetData();
        CharacterSelector.instance.DestroySingleton();

        inventory = GetComponent<InventoryManager>();

        CurrentHealth = characterData.MaxHealth;
        CurrentRecovery = characterData.Recovery;
        CurrentMoveSpeed = characterData.MoveSpeed;
        CurrentMight = characterData.Might;
        CurrentProjectileSpeed = characterData.ProjectileSpeed;
        CurrentMagnet = characterData.Magnet;

        //spawn starting weapon
        SpawnWeapon(characterData.StartingWeapon);

        //SpawnPassiveItem(firstPassiveItemTest);
        //SpawnPassiveItem(secondPassiveItemTest);
        //SpawnWeapon(weaponTest);
    }

    void Start()
    {
        //set the current stats display
        GameManager.instance.currentHealthDisplay.text = "Health: " + (int)currentHealth;
        GameManager.instance.currentRecoveryDisplay.text = "Recovery: " + currentRecovery;
        GameManager.instance.currentMoveSpeedDisplay.text = "Move Speed: " + currentMoveSpeed;
        GameManager.instance.currentMightDisplay.text = "Might: " + currentMight;
        GameManager.instance.currentProjectileSpeedDisplay.text = "Projectile Speed: " + currentProjectileSpeed;
        GameManager.instance.currentMagnetDisplay.text = "Magnet: " + currentMagnet;

        GameManager.instance.AssignChosenCharacterUI(characterData);

        UpdateHealthBar();
        UpdateExperienceBar();
        UpdateLevelText();
    }

    void Update()
    {
        if(invincibilityTimer > 0)
        {
            invincibilityTimer -= Time.deltaTime;
        }
        else if(isInvincible)
        {
            isInvincible = false;
        }

        Recover();
    }

    public void IncreaseExperience(int amount)
    {
        experience += amount;

        LevelUpChecker();

        UpdateExperienceBar();
    }

    void LevelUpChecker()
    {
        if(experience >= experienceCap) //level up if the experience is meet required amount
        {
            //level up the player and deduct experience
            level++;
            experience -= experienceCap;
            experienceCap += experienceCapIncrease;

            UpdateLevelText();

            GameManager.instance.StartLevelUp();
        }
    }

    void UpdateExperienceBar()
    {
        experienceBar.fillAmount = (float)experience / experienceCap;
    }

    void UpdateLevelText()
    {
        levelText.text = "Level " + level.ToString();
    }

    public void TakeDamage(float dmg)
    {
        //if the player is not currently in invincible state, reduce health and start it
        if(!isInvincible)
        {
            CurrentHealth -= dmg;
            if(damageEffect)
            {
                Instantiate(damageEffect, transform.position, Quaternion.identity);
            }
            invincibilityTimer = invincibilityDuration;
            isInvincible = true;

            if (CurrentHealth <= 0)
            {
                Kill();
            }

            UpdateHealthBar();
        }
    }

    void UpdateHealthBar()
    {
        //update the health bar
        healthBar.fillAmount = currentHealth / characterData.MaxHealth; 
    }

    public void Kill()
    {
        if(!GameManager.instance.isGameOver)
        {
            GameManager.instance.AssignLevelReachedUI(level);
            GameManager.instance.AssignChosenWeaponsAndPassiveItemsUI(inventory.weaponUISlots, inventory.passiveItemUISlots);
            GameManager.instance.GameOver();
        }
    }

    public void RestoreHealth(float amount)
    {
        //just health the player when current health is less than maxium health
        if(CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += amount;

            if(CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }

    void Recover()
    {
        if(CurrentHealth < characterData.MaxHealth)
        {
            CurrentHealth += CurrentRecovery * Time.deltaTime;
            //make the player recovery doesn't exceed maxium health
            if(CurrentHealth > characterData.MaxHealth)
            {
                CurrentHealth = characterData.MaxHealth;
            }
        }
    }

    public void SpawnWeapon(GameObject weapon)
    {
        if(weaponIndex >= inventory.weaponSlots.Count -1)
        {
            Debug.LogError("Inventory slots already full");
            return;
        }

        //spawn starting weapon
        GameObject spawnedWeapon = Instantiate(weapon, transform.position, Quaternion.identity);
        spawnedWeapon.transform.SetParent(transform); //set the weapon to be a child of the player
        inventory.AddWeapon(weaponIndex, spawnedWeapon.GetComponent<WeaponController>()); //add the weapon to inventory slot
        weaponIndex++;
    }

    public void SpawnPassiveItem(GameObject passiveItem)
    {
        if (passiveItemIndex >= inventory.passiveItemSlots.Count - 1)
        {
            Debug.LogError("Inventory slots already full");
            return;
        }

        //spawn starting passive item
        GameObject spawnedPassiveItem = Instantiate(passiveItem, transform.position, Quaternion.identity);
        spawnedPassiveItem.transform.SetParent(transform); //set the passive item to be a child of the player
        inventory.AddPassiveItem(passiveItemIndex, spawnedPassiveItem.GetComponent<PassiveItem>()); //add the passive item to inventory slot
        passiveItemIndex++;
    }
}
