public interface IPenalty
{
    PenaltyType PenaltyType { get; }

    /// <summary>Activate this penalty effect on the player.</summary>
    void Activate();

    /// <summary>Deactivate and clean up this penalty effect.</summary>
    void Deactivate();
}
