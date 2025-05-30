using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class OtterHealth : MonoBehaviour
{
    [Header("Health Settings")]
    public float maxHealth = 100f;
    public float currentHealth;
    public float shieldMax = 100f;
    public float currentShield;
    
    [Header("UI Elements")]
    public Slider healthSlider;
    public Slider shieldSlider;
    public Image damageOverlay;
    
    [Header("Effects")]
    public ParticleSystem damageEffect;
    public AudioClip damageSound;
    public AudioClip healSound;
    public AudioClip shieldBreakSound;
    
    private AudioSource audioSource;
    private bool isDead = false;
    
    public UnityEvent onDeath;
    public UnityEvent onDamage;
    public UnityEvent onHeal;
    
    private void Start()
    {
        currentHealth = maxHealth;
        currentShield = shieldMax;
        audioSource = GetComponent<AudioSource>();
        
        UpdateUI();
    }
    
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        
        // Apply damage to shield first
        if (currentShield > 0)
        {
            float remainingDamage = Mathf.Max(0, damage - currentShield);
            currentShield = Mathf.Max(0, currentShield - damage);
            
            if (currentShield <= 0 && shieldBreakSound != null)
            {
                audioSource.PlayOneShot(shieldBreakSound);
            }
            
            damage = remainingDamage;
        }
        
        // Apply remaining damage to health
        if (damage > 0)
        {
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            if (damageEffect != null)
                damageEffect.Play();
                
            if (damageSound != null)
                audioSource.PlayOneShot(damageSound);
                
            onDamage.Invoke();
        }
        
        UpdateUI();
        
        if (currentHealth <= 0 && !isDead)
        {
            Die();
        }
    }
    
    public void Heal(float amount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        
        if (healSound != null)
            audioSource.PlayOneShot(healSound);
            
        onHeal.Invoke();
        UpdateUI();
    }
    
    public void AddShield(float amount)
    {
        if (isDead) return;
        
        currentShield = Mathf.Min(shieldMax, currentShield + amount);
        UpdateUI();
    }
    
    private void Die()
    {
        isDead = true;
        onDeath.Invoke();
        
        // Disable player controls
        OtterPlayerController playerController = GetComponent<OtterPlayerController>();
        if (playerController != null)
        {
            playerController.enabled = false;
        }
        
        // Disable weapon system
        WeaponSystem weaponSystem = GetComponent<WeaponSystem>();
        if (weaponSystem != null)
        {
            weaponSystem.enabled = false;
        }
        
        // TODO: Implement death animation and respawn system
    }
    
    private void UpdateUI()
    {
        if (healthSlider != null)
            healthSlider.value = currentHealth / maxHealth;
            
        if (shieldSlider != null)
            shieldSlider.value = currentShield / shieldMax;
            
        if (damageOverlay != null)
        {
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 1f - (currentHealth / maxHealth);
            damageOverlay.color = overlayColor;
        }
    }
} 