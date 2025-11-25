using UnityEngine;

public static class PlayerControllerExtensions
{
    public static bool EstaEnEstado<T>(this PlayerController player) where T : IState
    {
        return player.GetCurrentState() is T;
    }

    public static IState GetCurrentState(this PlayerController player)
    {
        // Necesitarás hacer público el campo estadoActual o agregar un getter
        // Por ahora, asumamos que agregamos este método en PlayerController:
        return player.GetEstadoActual();
    }
}