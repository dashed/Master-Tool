using NUnit.Framework;

namespace MasterTool.Tests.Tests.ESP
{
    /// <summary>
    /// Tests for the LOS layer mask initialization logic.
    /// Duplicates the pure bitmask computation from PlayerEsp.InitLayerMask()
    /// since Unity's LayerMask/Physics APIs cannot be referenced from net9.0 tests.
    /// </summary>
    [TestFixture]
    public class LineOfSightTests
    {
        private const int FallbackMask = 0x02251800;

        /// <summary>
        /// Mirrors the layer mask computation from PlayerEsp.InitLayerMask().
        /// Takes resolved layer indices (-1 if not found) and returns the combined mask.
        /// </summary>
        private static int ComputeLayerMask(int highPoly, int lowPoly, int terrain)
        {
            int mask = 0;
            if (highPoly >= 0)
                mask |= 1 << highPoly;
            if (lowPoly >= 0)
                mask |= 1 << lowPoly;
            if (terrain >= 0)
                mask |= 1 << terrain;

            return mask != 0 ? mask : FallbackMask;
        }

        [Test]
        public void ComputeLayerMask_AllLayersFound_CombinesBits()
        {
            // Typical EFT layers: HighPoly=12, LowPoly=11, Terrain=8
            int result = ComputeLayerMask(12, 11, 8);

            Assert.That(result & (1 << 12), Is.Not.Zero, "HighPolyCollider bit should be set");
            Assert.That(result & (1 << 11), Is.Not.Zero, "LowPolyCollider bit should be set");
            Assert.That(result & (1 << 8), Is.Not.Zero, "Terrain bit should be set");
            Assert.That(result, Is.EqualTo((1 << 12) | (1 << 11) | (1 << 8)));
        }

        [Test]
        public void ComputeLayerMask_NoLayersFound_ReturnsFallback()
        {
            int result = ComputeLayerMask(-1, -1, -1);

            Assert.That(result, Is.EqualTo(FallbackMask));
        }

        [Test]
        public void ComputeLayerMask_PartialLayersFound_OnlySetsFoundBits()
        {
            // Only terrain found at layer 8
            int result = ComputeLayerMask(-1, -1, 8);

            Assert.That(result, Is.EqualTo(1 << 8));
            Assert.That(result, Is.Not.EqualTo(FallbackMask));
        }

        [Test]
        public void ComputeLayerMask_SingleLayerFound_DoesNotUseFallback()
        {
            int result = ComputeLayerMask(12, -1, -1);

            Assert.That(result, Is.EqualTo(1 << 12));
        }

        [Test]
        public void FallbackMask_HasExpectedBitsSet()
        {
            // The fallback mask 0x02251800 should have specific bits set
            // corresponding to known EFT collision layers
            Assert.That(FallbackMask, Is.GreaterThan(0));
            Assert.That(FallbackMask & (1 << 11), Is.Not.Zero, "Bit 11 should be set in fallback");
            Assert.That(FallbackMask & (1 << 12), Is.Not.Zero, "Bit 12 should be set in fallback");
        }

        [Test]
        public void ComputeLayerMask_Layer0_SetsCorrectBit()
        {
            // Edge case: layer 0 should still work (1 << 0 = 1)
            int result = ComputeLayerMask(0, -1, -1);

            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void ComputeLayerMask_DuplicateLayers_NoDuplication()
        {
            // If two layer names resolve to the same index, OR is idempotent
            int result = ComputeLayerMask(12, 12, 12);

            Assert.That(result, Is.EqualTo(1 << 12));
        }

        [Test]
        public void ComputeLayerMask_MaxLayer31_SetsHighBit()
        {
            // Unity supports layers 0-31
            int result = ComputeLayerMask(31, -1, -1);

            Assert.That(result, Is.EqualTo(1 << 31));
            Assert.That(result, Is.LessThan(0), "Bit 31 makes int negative (sign bit)");
        }
    }
}
