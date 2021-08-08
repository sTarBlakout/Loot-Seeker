namespace Gameplay.Core
{
    public enum Order
    {
        None,
        Move,
        Attack
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
