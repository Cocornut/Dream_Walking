using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HealthScript : MonoBehaviour
{
    public int maxHealth = 100;
    private int currentHealth;

    // Health Display
    private GameObject healthTextObject;
    private TextMeshProUGUI healthText;

    // Death display
    private GameObject loseTextObject;
    private TextMeshProUGUI loseText;

    private void Start()
    {
        currentHealth = maxHealth;

        healthTextObject = GameObject.Find("Health");
        healthText = healthTextObject.GetComponent<TextMeshProUGUI>();

        loseTextObject = GameObject.Find("Win/Lose");
        loseText = loseTextObject.GetComponent<TextMeshProUGUI>();

        UpdateHealthUI();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        // Later death animation and screen
        if (loseText != null)
        {
            // Change the text
            loseText.text = "You Died";
            // Set the text colour
            loseText.color = Color.red;
            // Show the text
            loseTextObject.SetActive(true);
            // Pause the game
            Time.timeScale = 0f;
        }
    }

    private void UpdateHealthUI()
    {
        if (healthText != null)
        {
            healthText.text = "Health: " + currentHealth.ToString();
        }
    }
}
