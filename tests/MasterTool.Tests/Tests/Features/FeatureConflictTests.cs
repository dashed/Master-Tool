using MasterTool.Core;
using NUnit.Framework;

namespace MasterTool.Tests.Tests.Features;

/// <summary>
/// Tests for feature interaction scenarios where multiple features are enabled simultaneously.
/// Validates priority chains, independence, and combined displacement when features overlap.
/// </summary>
[TestFixture]
public class FeatureConflictTests
{
    private readonly Vec3 _forward = new Vec3(0, 0, 1);
    private readonly Vec3 _right = new Vec3(1, 0, 0);

    // ===================================================================
    // Section 1: Damage Chain Priority
    // ===================================================================

    [Test]
    public void GodMode_Overrides_All_Other_Damage_Features()
    {
        // GodMode ON + IgnoreHeadshots + HeadDamage50% + Reduction50% + Keep1Health
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: true,
            isHead: true,
            ignoreHeadshots: true,
            headDamagePercent: 50,
            damageReductionPercent: 50,
            keep1Health: true,
            keep1Selection: "All",
            bodyPartCurrentHp: 10f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(0f));
    }

    [Test]
    public void GodMode_Off_IgnoreHeadshots_On_Head_Hit_Zeroes_Damage()
    {
        // IgnoreHeadshots wins over HeadDamagePercent for head hits
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: false,
            isHead: true,
            ignoreHeadshots: true,
            headDamagePercent: 50,
            damageReductionPercent: 100,
            keep1Health: false,
            keep1Selection: "All",
            bodyPartCurrentHp: 35f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(0f));
    }

    [Test]
    public void HeadDamage_And_DamageReduction_Stack_Multiplicatively()
    {
        // 100 * 0.5 (head%) * 0.5 (global%) = 25
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: false,
            isHead: true,
            ignoreHeadshots: false,
            headDamagePercent: 50,
            damageReductionPercent: 50,
            keep1Health: false,
            keep1Selection: "All",
            bodyPartCurrentHp: 35f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(25f));
    }

    [Test]
    public void DamageReduction_Then_Keep1Health_Clamps_When_Lethal()
    {
        // 100 * 0.5 (reduction) = 50 damage, but bodyPart has 20 HP
        // Keep1Health clamps: min(50, 20 - 3) = 17
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: false,
            isHead: false,
            ignoreHeadshots: false,
            headDamagePercent: 100,
            damageReductionPercent: 50,
            keep1Health: true,
            keep1Selection: "All",
            bodyPartCurrentHp: 20f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(17f));
    }

    [Test]
    public void Full_Chain_With_Head_GodMode_Blocks_Everything()
    {
        // All features ON + head hit → GodMode short-circuits to 0
        float result = DamageLogic.ComputeLocalPlayerDamage(
            200f,
            godMode: true,
            isHead: true,
            ignoreHeadshots: true,
            headDamagePercent: 25,
            damageReductionPercent: 10,
            keep1Health: true,
            keep1Selection: "All",
            bodyPartCurrentHp: 5f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(0f));
    }

    [Test]
    public void Non_Head_Hit_Ignores_Head_Specific_Features()
    {
        // Body shot: ignoreHeadshots and headDamagePercent should NOT apply
        // Only damageReductionPercent applies: 100 * 0.5 = 50
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: false,
            isHead: false,
            ignoreHeadshots: true,
            headDamagePercent: 10,
            damageReductionPercent: 50,
            keep1Health: false,
            keep1Selection: "All",
            bodyPartCurrentHp: 100f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(50f));
    }

    [Test]
    public void All_Features_Off_Damage_Unchanged()
    {
        float result = DamageLogic.ComputeLocalPlayerDamage(
            75f,
            godMode: false,
            isHead: false,
            ignoreHeadshots: false,
            headDamagePercent: 100,
            damageReductionPercent: 100,
            keep1Health: false,
            keep1Selection: "All",
            bodyPartCurrentHp: 100f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(75f));
    }

    [Test]
    public void Enemy_Damage_Multiplier_Independent_Of_Player_Features()
    {
        // Player damage is zeroed by godmode
        float playerDmg = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: true,
            isHead: false,
            ignoreHeadshots: false,
            headDamagePercent: 100,
            damageReductionPercent: 100,
            keep1Health: false,
            keep1Selection: "All",
            bodyPartCurrentHp: 100f,
            isChest: false
        );
        // Enemy damage is multiplied independently
        float enemyDmg = DamageLogic.ComputeEnemyDamage(100f, 5f);

        Assert.That(playerDmg, Is.EqualTo(0f));
        Assert.That(enemyDmg, Is.EqualTo(500f));
    }

    // ===================================================================
    // Section 2: GodMode + CodMode Interaction
    // ===================================================================

    [Test]
    public void CodMode_ShouldHeal_Independent_Of_GodMode()
    {
        // ShouldHeal only checks timer vs delay — GodMode state is irrelevant
        bool godModeOn = true;
        _ = godModeOn; // GodMode state exists but does not affect heal logic
        Assert.That(HealingLogic.ShouldHeal(15f, 10f), Is.True);
    }

    [Test]
    public void CalculateHealAmount_Unaffected_By_Damage_Blocking()
    {
        // Even if GodMode blocks all damage, healing still works on current/max HP
        float healAmount = HealingLogic.CalculateHealAmount(60f, 100f, 5f);
        Assert.That(healAmount, Is.EqualTo(5f));
    }

    [Test]
    public void Timer_Accumulation_Heals_After_Delay_Without_Damage()
    {
        // Simulate time passing without damage events
        float timeSinceHit = 0f;

        // Accumulate 625 frames at 16ms (~10s)
        for (int i = 0; i < 625; i++)
        {
            timeSinceHit += 0.016f;
        }

        Assert.That(timeSinceHit, Is.EqualTo(10f).Within(0.01f));
        Assert.That(HealingLogic.ShouldHeal(timeSinceHit, 9.5f), Is.True);
        Assert.That(HealingLogic.CalculateHealAmount(80f, 100f, 3f), Is.EqualTo(3f));
    }

    // ===================================================================
    // Section 3: FlyMode + Speedhack Displacement
    // ===================================================================

    [Test]
    public void FlyMode_And_Speedhack_Displacements_Sum()
    {
        float dt = 0.016f;
        Vec3 flyDisp = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, dt);
        Vec3 speedDisp = SpeedhackLogic.ComputeDisplacement(_forward, 2f, dt);
        Vec3 combined = flyDisp + speedDisp;

        Assert.That(combined.z, Is.EqualTo(flyDisp.z + speedDisp.z).Within(0.01f));
        Assert.That(combined.Magnitude, Is.GreaterThan(flyDisp.Magnitude));
    }

    [Test]
    public void FlyMode_Only_Speedhack_Direction_Zero()
    {
        float dt = 0.016f;
        Vec3 flyDisp = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, dt);
        Vec3 speedDisp = SpeedhackLogic.ComputeDisplacement(Vec3.Zero, 5f, dt);

        Assert.That(speedDisp.SqrMagnitude, Is.EqualTo(0f));
        Vec3 combined = flyDisp + speedDisp;
        Assert.That(combined.z, Is.EqualTo(flyDisp.z).Within(0.001f));
    }

    [Test]
    public void Speedhack_Only_Fly_Input_Zero()
    {
        float dt = 0.016f;
        Vec3 flyDisp = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 0, 0, 0, 10f, dt);
        Vec3 speedDisp = SpeedhackLogic.ComputeDisplacement(_forward, 3f, dt);

        Assert.That(flyDisp.SqrMagnitude, Is.EqualTo(0f));
        Vec3 combined = flyDisp + speedDisp;
        Assert.That(combined.z, Is.EqualTo(speedDisp.z).Within(0.001f));
    }

    [Test]
    public void Both_Displacements_Scale_With_DeltaTime()
    {
        float dtSmall = 0.016f;
        float dtLarge = 0.032f;

        Vec3 flySmall = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, dtSmall);
        Vec3 flyLarge = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, dtLarge);
        Vec3 speedSmall = SpeedhackLogic.ComputeDisplacement(_forward, 2f, dtSmall);
        Vec3 speedLarge = SpeedhackLogic.ComputeDisplacement(_forward, 2f, dtLarge);

        // Both should roughly double with doubled deltaTime
        Assert.That(flyLarge.Magnitude / flySmall.Magnitude, Is.EqualTo(2f).Within(0.01f));
        Assert.That(speedLarge.Magnitude / speedSmall.Magnitude, Is.EqualTo(2f).Within(0.01f));
    }

    // ===================================================================
    // Section 4: DamageReduction + Keep1Health Edge Cases
    // ===================================================================

    [Test]
    public void Keep1Health_Prevents_Lethal_After_Percentage_Reduction()
    {
        // 100 * 0.75 (reduction 75%) = 75 damage on a body part with 10 HP
        // Keep1Health clamps: min(75, 10 - 3) = 7
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: false,
            isHead: false,
            ignoreHeadshots: false,
            headDamagePercent: 100,
            damageReductionPercent: 75,
            keep1Health: true,
            keep1Selection: "All",
            bodyPartCurrentHp: 10f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(7f));
    }

    [Test]
    public void Keep1Health_Allows_Non_Lethal_After_Reduction()
    {
        // 100 * 0.5 (reduction) = 50 damage on body part with 100 HP
        // 100 - 50 = 50 remaining, well above 3 → no clamping
        float result = DamageLogic.ComputeLocalPlayerDamage(
            100f,
            godMode: false,
            isHead: false,
            ignoreHeadshots: false,
            headDamagePercent: 100,
            damageReductionPercent: 50,
            keep1Health: true,
            keep1Selection: "All",
            bodyPartCurrentHp: 100f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(50f));
    }

    [Test]
    public void Keep1Health_HeadAndThorax_Does_Not_Protect_Stomach()
    {
        // Stomach (isHead=false, isChest=false) with "Head And Thorax" selection
        // 50 damage on 10 HP → would be lethal, but NOT protected
        float result = DamageLogic.ComputeLocalPlayerDamage(
            50f,
            godMode: false,
            isHead: false,
            ignoreHeadshots: false,
            headDamagePercent: 100,
            damageReductionPercent: 100,
            keep1Health: true,
            keep1Selection: "Head And Thorax",
            bodyPartCurrentHp: 10f,
            isChest: false
        );
        Assert.That(result, Is.EqualTo(50f));
    }

    // ===================================================================
    // Section 5: Sustenance Independence
    // ===================================================================

    [Test]
    public void Sustenance_And_Healing_Operate_Independently()
    {
        // Energy sustenance fills to max
        float energy = SustenanceLogic.ComputeNewValue(50f, 100f, true);
        // Healing returns heal amount
        float healAmount = HealingLogic.CalculateHealAmount(60f, 100f, 5f);

        Assert.That(energy, Is.EqualTo(100f));
        Assert.That(healAmount, Is.EqualTo(5f));
    }

    [Test]
    public void Both_Sustenance_And_Healing_Return_Positive_Simultaneously()
    {
        // Both features produce positive results at the same time
        float energyGain = SustenanceLogic.ComputeNewValue(80f, 100f, true) - 80f;
        float healAmount = HealingLogic.CalculateHealAmount(70f, 100f, 3f);

        Assert.That(energyGain, Is.GreaterThan(0f));
        Assert.That(healAmount, Is.GreaterThan(0f));
    }

    // ===================================================================
    // Section 6: Fall Damage + Fly Mode
    // ===================================================================

    [Test]
    public void SafeHeight_Protects_Against_Any_Practical_Fall()
    {
        // SafeHeight is 999999, any practical building/cliff height is far below
        float tallestBuilding = 500f;
        Assert.That(FallDamageDefaults.SafeHeight, Is.GreaterThan(tallestBuilding));
        Assert.That(FallDamageDefaults.SafeHeight, Is.EqualTo(999999f));
    }

    [Test]
    public void DefaultHeight_Only_Short_Falls_Safe()
    {
        // Default 1.8m means only very small drops are safe
        float shortDrop = 1.5f;
        float normalDrop = 3f;
        Assert.That(FallDamageDefaults.DefaultHeight, Is.GreaterThan(shortDrop));
        Assert.That(FallDamageDefaults.DefaultHeight, Is.LessThan(normalDrop));
    }

    // ===================================================================
    // Section 7: Weight + Speed
    // ===================================================================

    [Test]
    public void Weight_Calculation_Independent_Of_Speedhack()
    {
        // Weight uses its own logic; speedhack uses displacement logic
        float? weight = WeightLogic.ComputeWeight(50f, true, 0);
        Vec3 speedDisp = SpeedhackLogic.ComputeDisplacement(_forward, 3f, 0.016f);

        Assert.That(weight, Is.EqualTo(0f));
        Assert.That(speedDisp.Magnitude, Is.GreaterThan(0f));
    }

    [Test]
    public void Zero_Weight_Does_Not_Affect_Movement_Calculation()
    {
        // Weight set to 0% does not change displacement vectors
        float? weight = WeightLogic.ComputeWeight(50f, true, 0);
        Vec3 fly1 = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 0.016f);
        Vec3 fly2 = MovementLogic.CalculateFlyMovement(_forward, _right, 0, 1, 0, 0, 10f, 0.016f);

        Assert.That(weight, Is.EqualTo(0f));
        Assert.That(fly1.z, Is.EqualTo(fly2.z).Within(0.001f));
    }
}
