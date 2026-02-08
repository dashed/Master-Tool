using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features
{
    /// <summary>
    /// Tests for the vision state machine logic used by VisionFeature.
    /// Duplicates the pure state-tracking logic since Unity camera components
    /// cannot be referenced from net9.0 tests.
    /// </summary>
    [TestFixture]
    public class VisionStateTests
    {
        /// <summary>
        /// Simulates the vision component's On property (NightVision.On or ThermalVision.On).
        /// </summary>
        private bool _componentOn;

        /// <summary>
        /// Simulates the mod's _modForced flag (_modForcedNvOn or _modForcedThermalOn).
        /// </summary>
        private bool _modForced;

        /// <summary>
        /// Mirrors the state-tracking logic from VisionFeature.UpdateNightVision/UpdateThermalVision.
        /// </summary>
        private void UpdateVision(bool modToggleEnabled)
        {
            if (modToggleEnabled)
            {
                if (!_componentOn)
                    _componentOn = true;
                _modForced = true;
            }
            else if (_modForced)
            {
                _componentOn = false;
                _modForced = false;
            }
            // else: toggle OFF and mod didn't force it — don't touch state
        }

        [SetUp]
        public void SetUp()
        {
            _componentOn = false;
            _modForced = false;
        }

        // --- Core state machine tests ---

        [Test]
        public void ToggleOff_ModDidNotForce_DoesNotTouchState()
        {
            // Simulate vanilla NVGs being turned on by the game
            _componentOn = true;

            UpdateVision(modToggleEnabled: false);

            Assert.That(_componentOn, Is.True, "Should not interfere with vanilla NV state");
            Assert.That(_modForced, Is.False);
        }

        [Test]
        public void ToggleOn_ForcesVisionOn()
        {
            UpdateVision(modToggleEnabled: true);

            Assert.That(_componentOn, Is.True, "Mod should force vision ON");
            Assert.That(_modForced, Is.True, "Should track that mod forced it");
        }

        [Test]
        public void ToggleOn_AlreadyOn_KeepsOn()
        {
            _componentOn = true; // Already on (vanilla or previous mod frame)

            UpdateVision(modToggleEnabled: true);

            Assert.That(_componentOn, Is.True, "Should stay ON");
            Assert.That(_modForced, Is.True);
        }

        [Test]
        public void ToggleOnThenOff_CleansUpOnce()
        {
            // Turn on
            UpdateVision(modToggleEnabled: true);
            Assert.That(_componentOn, Is.True);
            Assert.That(_modForced, Is.True);

            // Turn off — should do one-time cleanup
            UpdateVision(modToggleEnabled: false);
            Assert.That(_componentOn, Is.False, "Should disable vision on cleanup");
            Assert.That(_modForced, Is.False, "Should clear forced flag");
        }

        [Test]
        public void ToggleOnThenOff_SubsequentFrames_DoNothing()
        {
            // Turn on then off
            UpdateVision(modToggleEnabled: true);
            UpdateVision(modToggleEnabled: false);

            // Simulate vanilla NVGs turning on after mod released control
            _componentOn = true;

            // More frames with mod toggle OFF
            UpdateVision(modToggleEnabled: false);
            UpdateVision(modToggleEnabled: false);

            Assert.That(_componentOn, Is.True, "Should not interfere with vanilla state after cleanup");
            Assert.That(_modForced, Is.False);
        }

        [Test]
        public void RepeatedToggleOff_NeverInterferes()
        {
            // Many frames with toggle OFF and vanilla NV ON
            _componentOn = true;

            for (int i = 0; i < 100; i++)
            {
                UpdateVision(modToggleEnabled: false);
            }

            Assert.That(_componentOn, Is.True, "Should never disable vanilla NV");
            Assert.That(_modForced, Is.False);
        }

        [Test]
        public void ToggleOn_MultipleFrames_KeepsVisionOn()
        {
            for (int i = 0; i < 10; i++)
            {
                UpdateVision(modToggleEnabled: true);
            }

            Assert.That(_componentOn, Is.True);
            Assert.That(_modForced, Is.True);
        }

        [Test]
        public void RapidToggle_OnOffOnOff_HandlesCorrectly()
        {
            // ON
            UpdateVision(modToggleEnabled: true);
            Assert.That(_componentOn, Is.True);

            // OFF
            UpdateVision(modToggleEnabled: false);
            Assert.That(_componentOn, Is.False);

            // ON again
            UpdateVision(modToggleEnabled: true);
            Assert.That(_componentOn, Is.True);

            // OFF again
            UpdateVision(modToggleEnabled: false);
            Assert.That(_componentOn, Is.False);
            Assert.That(_modForced, Is.False);
        }

        [Test]
        public void ToggleOff_VanillaAlreadyOff_StaysOff()
        {
            _componentOn = false;

            UpdateVision(modToggleEnabled: false);

            Assert.That(_componentOn, Is.False);
            Assert.That(_modForced, Is.False);
        }

        [Test]
        public void ToggleOn_ThenOff_VanillaReenables_ModDoesNotInterfere()
        {
            // Mod turns NV on
            UpdateVision(modToggleEnabled: true);
            Assert.That(_componentOn, Is.True);

            // Mod turns off — cleanup
            UpdateVision(modToggleEnabled: false);
            Assert.That(_componentOn, Is.False);

            // Game re-enables NV (player has real NVGs equipped)
            _componentOn = true;

            // Mod stays off — should not touch it
            UpdateVision(modToggleEnabled: false);
            Assert.That(_componentOn, Is.True, "Game re-enabled NV, mod should not interfere");
        }

        // --- Edge case: external state changes while mod is active ---

        [Test]
        public void ToggleOn_ExternalDisable_ModReenables()
        {
            // Mod forces NV on
            UpdateVision(modToggleEnabled: true);
            Assert.That(_componentOn, Is.True);

            // Something external disables NV (e.g., game unequips NVGs)
            _componentOn = false;

            // Next frame — mod should re-enable it
            UpdateVision(modToggleEnabled: true);
            Assert.That(_componentOn, Is.True, "Mod should re-force NV on");
        }
    }
}
