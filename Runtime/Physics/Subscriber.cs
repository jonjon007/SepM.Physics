using Unity.Mathematics.FixedPoint;

/// <summary>
/// Inteferace <c>Subscriber</c> Used to call Step methods; tie this to game loop logic
/// </summary>
public interface Subscriber {
    void Subscribe();
    void Unsubscribe();
    void Step(fp timestep, long inputs);
}
