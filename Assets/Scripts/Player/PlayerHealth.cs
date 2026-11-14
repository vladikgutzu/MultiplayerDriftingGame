using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class PlayerHealth : MonoBehaviour
{
    private float health;
    private float lerpTimer;
    public int maxHealth = 100;
    public float chipSpeed = 2f;
    public Image frontHealthBar;
    public Image backHealthBar;
    public TextMeshProUGUI healthText;
    // Start is called before the first frame update
    void Start()
    {
        maxHealth = Mathf.Max(1, maxHealth);
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        health = Mathf.Clamp(health, 0, maxHealth);
        UpdateHealthUI();
       
    }
    public void UpdateHealthUI() 
    {
        if (!frontHealthBar || !backHealthBar)
            return;

      //  Debug.Log(health);
        float fillF = frontHealthBar.fillAmount;
        float fillB = backHealthBar.fillAmount;
        float hFraction = health / maxHealth;
        if (fillB > hFraction) 
        {
            frontHealthBar.fillAmount = hFraction;
            backHealthBar.color = Color.red;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / Mathf.Max(.01f, chipSpeed);
            percentComplete = percentComplete * percentComplete;
            backHealthBar.fillAmount = Mathf.Lerp(fillB, hFraction, percentComplete);
        }
        if (fillF < hFraction) 
        {
            backHealthBar.color = Color.green;
            backHealthBar.fillAmount = hFraction;
            lerpTimer += Time.deltaTime;
            float percentComplete = lerpTimer / Mathf.Max(.01f, chipSpeed);
            percentComplete = percentComplete * percentComplete;
            frontHealthBar.fillAmount = Mathf.Lerp(fillF, backHealthBar.fillAmount, percentComplete);
        }
        if (healthText)
            healthText.text = Mathf.CeilToInt(health) + "/" + maxHealth;
    }

    public void TakeDamage(float damage) 
    {
        health -= Mathf.Max(0f, damage);
        lerpTimer = 0f;
    }
    public void RestoreHealth(float healAmount) 
    {
        health += Mathf.Max(0f, healAmount);
        lerpTimer = 0f;
    }
    public void IncreaseHealth(int level)
    {
        maxHealth += Mathf.RoundToInt((health * 0.01f) * ((100 - level) * 0.1f));
        maxHealth = Mathf.Max(1, maxHealth);
        health = maxHealth;
    }
    public void GameOver() { }

}
