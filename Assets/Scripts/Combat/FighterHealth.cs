using UnityEngine;

/// <summary>
/// Simple HP for a basic fight. KO freezes further attack starts via
/// <see cref="FighterCombat"/> checking <see cref="IsKnockedOut"/>.
/// </summary>
public class FighterHealth : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int currentHealth = 100;

    public int MaxHealth => maxHealth;
    public int CurrentHealth => currentHealth;
    public bool IsKnockedOut => currentHealth <= 0;
    public float Normalized => maxHealth > 0 ? Mathf.Clamp01((float)currentHealth / maxHealth) : 0f;

    public void Configure(int startingHealth)
    {
        maxHealth = Mathf.Max(1, startingHealth);
        currentHealth = maxHealth;
    }

    public void ApplyDamage(int amount)
    {
        if (amount <= 0 || IsKnockedOut)
            return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log($"[Health] {name} took {amount} → {currentHealth}/{maxHealth}" +
                  (IsKnockedOut ? " KO" : ""));
    }
}
