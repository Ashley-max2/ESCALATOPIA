using System;
using UnityEngine;

/// <summary>
/// Sistema de eventos global para comunicación desacoplada entre sistemas.
/// Sigue el principio SOLID de inversión de dependencias.
/// </summary>
public static class GameEvents
{
    // Player Events
    public static event Action<Vector3> OnPlayerDeath;
    public static event Action<Vector3> OnPlayerRespawn;
    public static event Action<string> OnPlayerStateChanged;
    
    // Climbing Events
    public static event Action OnClimbStart;
    public static event Action OnClimbEnd;
    
    // Hook Events
    public static event Action<Vector3> OnHookFired;
    public static event Action OnHookConnected;
    public static event Action OnHookReleased;
    
    // Fall Events
    public static event Action<float> OnPlayerFalling;
    public static event Action<float> OnPlayerLanded;
    
    public static void PlayerDeath(Vector3 position) => OnPlayerDeath?.Invoke(position);
    public static void PlayerRespawn(Vector3 position) => OnPlayerRespawn?.Invoke(position);
    public static void PlayerStateChanged(string stateName) => OnPlayerStateChanged?.Invoke(stateName);
    
    public static void ClimbStart() => OnClimbStart?.Invoke();
    public static void ClimbEnd() => OnClimbEnd?.Invoke();
    
    public static void HookFired(Vector3 target) => OnHookFired?.Invoke(target);
    public static void HookConnected() => OnHookConnected?.Invoke();
    public static void HookReleased() => OnHookReleased?.Invoke();
    
    public static void PlayerFalling(float height) => OnPlayerFalling?.Invoke(height);
    public static void PlayerLanded(float fallDistance) => OnPlayerLanded?.Invoke(fallDistance);
}
