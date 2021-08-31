namespace Gameplay.Core
{
    public enum TileParticleType
    {
        Reachable
    }
    
    public enum OrderResult
    {
        NotDetermined,
        Succes,
        HalfSucces,
        Fail
    }

    public enum OrderFailReason
    {
        None,
        TooFar,
        BlockedArea,
        NotAnEnemy,
        NotInteractable
    }

    public enum AnimMovement
    {
        RotateLeft,
        RotateRight,
        Walk
    }
}
