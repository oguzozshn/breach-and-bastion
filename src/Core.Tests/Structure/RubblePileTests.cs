using BreachAndBastion.Core.Structure;

namespace BreachAndBastion.Core.Tests.Structure;

public class RubblePileTests
{
    [Fact]
    public void Deposit_keeps_bulked_volume_and_respects_angle_of_repose()
    {
        var pile = new RubblePile();

        pile.Deposit(4, 0);
        pile.Deposit(2, 1.2);

        Assert.Equal(6 * RubblePile.Bulking, pile.Area, 1e-9);
        var maxStep = Math.Tan(RubblePile.ReposeAngleDegrees * Math.PI / 180) * RubblePile.BinWidth;
        for (var k = 0; k < pile.BinCount - 1; k++)
            Assert.True(pile.Height(k) - pile.Height(k + 1) <= maxStep + 1e-6);
    }

    [Fact]
    public void Material_landing_at_the_face_makes_the_face_highest()
    {
        var pile = new RubblePile();

        pile.Deposit(3, 0);

        for (var k = 1; k < pile.BinCount; k++) Assert.True(pile.HeightAtFace >= pile.Height(k));
    }
}
