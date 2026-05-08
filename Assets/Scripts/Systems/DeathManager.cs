public enum DeathCause
{
    None,
    Stamina,
    Fall,
    Water
}

public static class DeathManager
{
    public static DeathCause LastDeathCause = DeathCause.None;
}