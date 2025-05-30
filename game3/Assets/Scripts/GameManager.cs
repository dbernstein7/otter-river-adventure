using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int maxPlayers = 100;
    public float gameStartDelay = 5f;
    public float stormShrinkInterval = 60f;
    public float initialStormRadius = 1000f;
    public float finalStormRadius = 50f;
    public float stormDamagePerSecond = 1f;
    
    [Header("UI Elements")]
    public TextMeshProUGUI playerCountText;
    public TextMeshProUGUI stormTimerText;
    public TextMeshProUGUI gameStateText;
    public Image stormCircleImage;
    
    [Header("References")]
    public Transform stormCenter;
    public GameObject stormVisual;
    
    private List<OtterPlayerController> activePlayers = new List<OtterPlayerController>();
    private float currentStormRadius;
    private float nextStormShrinkTime;
    private bool gameStarted = false;
    private bool gameEnded = false;
    
    private void Start()
    {
        currentStormRadius = initialStormRadius;
        nextStormShrinkTime = Time.time + gameStartDelay + stormShrinkInterval;
        UpdateStormVisual();
    }
    
    private void Update()
    {
        if (!gameStarted)
        {
            if (Time.time >= gameStartDelay)
            {
                StartGame();
            }
            return;
        }
        
        if (gameEnded) return;
        
        // Update storm
        if (Time.time >= nextStormShrinkTime)
        {
            ShrinkStorm();
        }
        
        // Apply storm damage to players outside the circle
        foreach (OtterPlayerController player in activePlayers)
        {
            if (player != null)
            {
                float distanceToCenter = Vector3.Distance(player.transform.position, stormCenter.position);
                if (distanceToCenter > currentStormRadius)
                {
                    OtterHealth health = player.GetComponent<OtterHealth>();
                    if (health != null)
                    {
                        health.TakeDamage(stormDamagePerSecond * Time.deltaTime);
                    }
                }
            }
        }
        
        // Update UI
        UpdateUI();
        
        // Check for game end
        CheckGameEnd();
    }
    
    private void StartGame()
    {
        gameStarted = true;
        gameStateText.text = "Game Started!";
        // TODO: Enable player movement and controls
    }
    
    private void ShrinkStorm()
    {
        float shrinkAmount = (initialStormRadius - finalStormRadius) / 
            (stormShrinkInterval * (maxPlayers / 10)); // Shrink faster with more players
        
        currentStormRadius = Mathf.Max(finalStormRadius, currentStormRadius - shrinkAmount);
        nextStormShrinkTime = Time.time + stormShrinkInterval;
        
        UpdateStormVisual();
    }
    
    private void UpdateStormVisual()
    {
        if (stormVisual != null)
        {
            stormVisual.transform.localScale = new Vector3(currentStormRadius * 2, 1, currentStormRadius * 2);
        }
        
        if (stormCircleImage != null)
        {
            stormCircleImage.rectTransform.sizeDelta = new Vector2(currentStormRadius * 2, currentStormRadius * 2);
        }
    }
    
    private void UpdateUI()
    {
        if (playerCountText != null)
        {
            int alivePlayers = activePlayers.Count;
            playerCountText.text = $"Players Alive: {alivePlayers}";
        }
        
        if (stormTimerText != null)
        {
            float timeUntilNextShrink = nextStormShrinkTime - Time.time;
            stormTimerText.text = $"Next Storm Shrink: {timeUntilNextShrink:F0}s";
        }
    }
    
    private void CheckGameEnd()
    {
        int alivePlayers = 0;
        OtterPlayerController lastPlayer = null;
        
        foreach (OtterPlayerController player in activePlayers)
        {
            if (player != null)
            {
                OtterHealth health = player.GetComponent<OtterHealth>();
                if (health != null && health.currentHealth > 0)
                {
                    alivePlayers++;
                    lastPlayer = player;
                }
            }
        }
        
        if (alivePlayers <= 1)
        {
            EndGame(lastPlayer);
        }
    }
    
    private void EndGame(OtterPlayerController winner)
    {
        gameEnded = true;
        gameStateText.text = winner != null ? "Game Over - Winner!" : "Game Over - Draw!";
        // TODO: Show end game UI and stats
    }
    
    public void RegisterPlayer(OtterPlayerController player)
    {
        if (!activePlayers.Contains(player))
        {
            activePlayers.Add(player);
        }
    }
    
    public void UnregisterPlayer(OtterPlayerController player)
    {
        activePlayers.Remove(player);
    }
} 