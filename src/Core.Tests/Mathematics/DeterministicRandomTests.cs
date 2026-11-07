using BreachAndBastion.Core.Mathematics;

namespace BreachAndBastion.Core.Tests.Mathematics;

public class DeterministicRandomTests
{
    [Fact]
    public void Same_seed_gives_same_sequence()
    {
        var a = new DeterministicRandom(42);
        var b = new DeterministicRandom(42);

        for (var i = 0; i < 100; i++) Assert.Equal(a.NextUInt64(), b.NextUInt64());
    }

    [Fact]
    public void Known_seed_gives_known_first_value()
    {
        // Pins the algorithm: changing it would silently break saved replays.
        Assert.Equal(0xBDD732262FEB6E95UL, new DeterministicRandom(42).NextUInt64());
    }

    [Fact]
    public void Doubles_stay_in_unit_interval()
    {
        var random = new DeterministicRandom(7);
        for (var i = 0; i < 10_000; i++) Assert.InRange(random.NextDouble(), 0.0, 0.9999999999999999);
    }
}
