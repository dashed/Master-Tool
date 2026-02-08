using NUnit.Framework;

namespace MasterTool.Tests.Tests.Utils
{
    [TestFixture]
    public class PlayerTagTests
    {
        // Mirror of EPlayerSide enum values used in the game
        private enum Side { Savage, Bear, Usec }

        // Mirror of WildSpawnType roles relevant to tagging
        private enum Role { assault, marksman, boss, sectant, follower }

        /// <summary>
        /// Standalone copy of PlayerUtils.GetPlayerTag logic.
        /// Maps a player's side and role to a display tag string.
        /// </summary>
        private static string GetPlayerTag(Side side, Role role)
        {
            if (side == Side.Savage)
            {
                if (role != Role.assault && role != Role.marksman)
                {
                    return "BOSS";
                }
                return "SCAV";
            }
            return side.ToString().ToUpper();
        }

        [Test]
        public void GetPlayerTag_SavageAssault_ReturnsScav()
        {
            Assert.That(GetPlayerTag(Side.Savage, Role.assault), Is.EqualTo("SCAV"));
        }

        [Test]
        public void GetPlayerTag_SavageMarksman_ReturnsScav()
        {
            Assert.That(GetPlayerTag(Side.Savage, Role.marksman), Is.EqualTo("SCAV"));
        }

        [Test]
        public void GetPlayerTag_SavageBoss_ReturnsBoss()
        {
            Assert.That(GetPlayerTag(Side.Savage, Role.boss), Is.EqualTo("BOSS"));
        }

        [Test]
        public void GetPlayerTag_SavageSectant_ReturnsBoss()
        {
            Assert.That(GetPlayerTag(Side.Savage, Role.sectant), Is.EqualTo("BOSS"));
        }

        [Test]
        public void GetPlayerTag_SavageFollower_ReturnsBoss()
        {
            Assert.That(GetPlayerTag(Side.Savage, Role.follower), Is.EqualTo("BOSS"));
        }

        [Test]
        public void GetPlayerTag_BearSide_ReturnsBear()
        {
            Assert.That(GetPlayerTag(Side.Bear, Role.assault), Is.EqualTo("BEAR"));
        }

        [Test]
        public void GetPlayerTag_UsecSide_ReturnsUsec()
        {
            Assert.That(GetPlayerTag(Side.Usec, Role.assault), Is.EqualTo("USEC"));
        }
    }
}
