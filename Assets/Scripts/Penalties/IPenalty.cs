public interface IPenalty
{
    PenaltyType PenaltyType { get; }

    void Activate();

    void Deactivate();
}
