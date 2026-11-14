
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelSystem : MonoBehaviour
{
    public int level;
    public float maxLevel;
    public float currentXp;
    public int nextLevelXp = 20;
    [Header("Multipliers")]
    [Range(1f, 300f)]
    public float additionMultiplier;
    [Range(2f, 4f)]
    public float powerMultiplier = 20f;
    [Range(7f, 14f)]
    public float divisionMultiplier = 7f;
    public GameObject levelUpEffect;

    [Header("UI")]
    public Image frontXpBar;
    public Image backXpBar;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI XpText;
    
    public TextMeshProUGUI _fullTitleText;

    //Audio  
    [Header("Audio")]
    public AudioClip levelUpSound;
    private AudioSource source;
    //Timers
    private float lerpTimer;
    private float delayTimer;

    List<string> _firstTitlePart = new List<string>() {"Slide", "Skid", "Corner", "Curve", "Turn", "Spin", "Glide", "Swivel", "Sway", "Whirlwind", "Twist", "Flow", "Sweep", "Bend", "Arc", "Zigzag", "Loop", "Revolve", "Veer", "Wave"};
    List<string> _secondTitlePart = new List<string>() {"Slinger", "Seeker", "Crusader", "Conqueror","Tamer","Master","Guru","Shaman","Sorcerer","Titan","Phantom","Samurai","Baron","Ace","Zephyr","Legend", "Renegade", "Vanguard", "Wraith", };
    public List<GameObject> levelStars;
     
    void Start()
    {
        MigrateLegacyProgressIfNeeded();
        ApplyTotalExperience(BD_PlayerProgression.TotalExperience);
        BD_PlayerProgression.ExperienceChanged += ApplyTotalExperience;
        RefreshUI();
        source = GetComponent<AudioSource>();
    }

    private void OnDestroy()
    {
        BD_PlayerProgression.ExperienceChanged -= ApplyTotalExperience;
    }

    private void RefreshUI()
    {
        if (levelText)
            levelText.text = "Level " + level;
        if (XpText)
            XpText.text = Mathf.Round(currentXp) + "/" + nextLevelXp + " XP";
        if (frontXpBar)
            frontXpBar.fillAmount = currentXp / nextLevelXp;
        if (backXpBar)
            backXpBar.fillAmount = currentXp / nextLevelXp;
        PlayerTitleSetUp();
        ActivateStarForLevel(level);
    }

    void Update()
    {
        UpdateXpUI();
        if (level != maxLevel)
        {
            if (currentXp >= nextLevelXp)
            {
                LevelUp();
            }        
        }
        else
        {
            currentXp = nextLevelXp;
            if (XpText)
                XpText.text = "MAX";
            if (frontXpBar)
                frontXpBar.fillAmount = currentXp / nextLevelXp;
            if (backXpBar)
                backXpBar.fillAmount = currentXp / nextLevelXp;
        }
    }
    private void UpdateXpUI() 
    {
        if (!frontXpBar || !backXpBar || !XpText)
            return;

        float xpFraction = currentXp / nextLevelXp;
        float fXP = frontXpBar.fillAmount;

        if (fXP < xpFraction)
        {
            delayTimer += Time.deltaTime;
            backXpBar.fillAmount = xpFraction;
            if (delayTimer > 3)
            {
                lerpTimer += Time.deltaTime;
                float percentComplete = lerpTimer / 5;
                percentComplete = percentComplete * percentComplete;
                frontXpBar.fillAmount = Mathf.Lerp(fXP, backXpBar.fillAmount, percentComplete);
            }

        }
        XpText.text = currentXp + "/" + nextLevelXp + " XP";
    }

    public void GainExperienceFlatRate(float xpGained)
    {
        BD_PlayerProgression.AddExperience(Mathf.Max(0, Mathf.RoundToInt(xpGained)));
        lerpTimer = 0f;
        delayTimer = 0f;
    }
    public void GainExperienceScalable(float xpGained, int passedLevel)
    {
        if (passedLevel < level)
        {
            float multiplier = 1 + (level - passedLevel) * 0.1f;
            BD_PlayerProgression.AddExperience(Mathf.Max(0, Mathf.RoundToInt(xpGained * multiplier)));

        }
        else
        {
            BD_PlayerProgression.AddExperience(Mathf.Max(0, Mathf.RoundToInt(xpGained)));

        }

        lerpTimer = 0f;
        delayTimer = 0f;

    }
    
    public void PlayerTitleSetUp()
    {
        if (!_fullTitleText || _firstTitlePart.Count == 0 || _secondTitlePart.Count == 0)
            return;

        int titleCount = Mathf.Min(_firstTitlePart.Count, _secondTitlePart.Count);
        int titleIndex = Mathf.Clamp(level - 1, 0, titleCount - 1);
        _fullTitleText.text = _firstTitlePart[titleIndex] + " " + _secondTitlePart[titleIndex];
    }
    
    public void LevelUp() 
    {
        level += 1;
        if (backXpBar)
            backXpBar.fillAmount = 0f;
        if (frontXpBar)
            frontXpBar.fillAmount = 0f;
        currentXp = Mathf.Round(currentXp-nextLevelXp);

        nextLevelXp = CalculateNextLevelXp();
        int levelCap = maxLevel > 0f ? Mathf.FloorToInt(maxLevel) : 50;
        level = Mathf.Clamp(level, 1, Mathf.Max(1, levelCap));

        if (XpText)
            XpText.text = Mathf.Round(currentXp) + "/" + nextLevelXp+ " XP";
        if (levelText)
            levelText.text = "Level " + level;
        
        PlayerTitleSetUp();
        ActivateStarForLevel(level);

        if (levelUpEffect)
            levelUpEffect.SetActive(true);
        // GetComponent<PlayerHealth>().IncreaseHealth(level);
        if (source && levelUpSound)
            source.PlayOneShot(levelUpSound);
    }

    private void ApplyTotalExperience(int totalExperience)
    {
        level = 1;
        currentXp = Mathf.Max(0, totalExperience);
        int levelCap = maxLevel > 0f ? Mathf.Max(1, Mathf.FloorToInt(maxLevel)) : 50;
        nextLevelXp = CalculateNextLevelXp();

        while (level < levelCap && currentXp >= nextLevelXp)
        {
            currentXp -= nextLevelXp;
            level++;
            nextLevelXp = CalculateNextLevelXp();
        }

        if (level >= levelCap)
            currentXp = Mathf.Min(currentXp, nextLevelXp);

        RefreshUI();
    }

    private void MigrateLegacyProgressIfNeeded()
    {
        if (PlayerPrefs.HasKey(BD_PlayerProgression.TotalExperienceKey))
            return;

        int legacyLevel = Mathf.Max(1, level);
        int legacyCurrentXp = Mathf.Max(0, Mathf.RoundToInt(currentXp));
        int total = legacyCurrentXp;
        int originalLevel = level;

        for (level = 1; level < legacyLevel; level++)
            total = (int)Mathf.Min((long)total + CalculateNextLevelXp(), int.MaxValue);

        level = originalLevel;
        BD_PlayerProgression.SetExperience(total);
    }


    public void ActivateStarForLevel(int level)
    {
        if (level > 0 && levelStars != null && levelStars.Count > 0)
        { 
            if (level % 5 == 0)
            {
                foreach (var star in levelStars)
                {
                    if (star)
                        star.SetActive(false);
                }
                if (levelStars[0])
                    levelStars[0].SetActive(true);
            }
            else
            {
                int starIndex = (level - 1) % levelStars.Count;
                
                for (int i = 0; i < levelStars.Count; i++)
                {
                    if (levelStars[i])
                        levelStars[i].SetActive(i <= starIndex);
                }
            }
        }
    }

    private void DisplayAccrueAmount() 
    {
        
    }
    private int CalculateNextLevelXp() 
    {
        int solveForRequiredXp = 0;
        for (int levelCycle = 1; levelCycle <= level; levelCycle++)
        {
            solveForRequiredXp += (int)Mathf.Floor(levelCycle + additionMultiplier * Mathf.Pow(powerMultiplier, levelCycle / divisionMultiplier));
        }
        return Mathf.Max(1, solveForRequiredXp / 4);
    }
}
