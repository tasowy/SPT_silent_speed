using BepInEx;
using BepInEx.Configuration;
using Comfort.Common;
using EFT;
using UnityEngine;

namespace SPT_silent_speed
{
    [BepInPlugin("com.tasowy.SPT_silent_speed", "SPT_silent_speed", "1.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            Settings.Init(Config);
            Logger.LogInfo("SPT_silent_speed loaded!");
        }

        private void Update()
        {
            if (!Settings.Enabled.Value || !Singleton<IBotGame>.Instantiated)
                return;

            if (IsKeyPressed(Settings.MaxSilentKey.Value))
                SetMaxSilentSpeed();
        }

        private void SetMaxSilentSpeed()
        {
            var player = Singleton<GameWorld>.Instance?.MainPlayer;
            var ctx = player?.MovementContext;
            if (player == null || ctx == null)
                return;

            // Silent (CovertNoiseLevel 0) is unreachable here -> do nothing.
            if (ctx.IsInPronePose || ctx.IsSprintEnabled)
                return;
            if (ctx.PhysicalConditionContainsAny(EPhysicalCondition.LeftLegDamaged | EPhysicalCondition.RightLegDamaged))
                return;

            float overweight = player.Physical.WalkOverweight;
            if (overweight >= 0.1f)
                return;

            float max = ctx.MaxSpeed;
            if (max <= 0f)
                return;

            // From UpdateCovertEfficiency: Eff = InverseLerp(num, num-0.2, r) * (1-W), num = 0.4 * CovertMovementSpeed.
            // Eff > 0.9  =>  r < num - 0.18 / (1-W). CovertMovementSpeed buff already includes skill level.
            float covertBuff = player.Skills.CovertMovementSpeed;
            float target = max * (0.4f * covertBuff - 0.18f / (1f - overweight)) - Settings.SafetyMargin.Value;
            target = Mathf.Clamp(target, 0.05f * max, Mathf.Min(max, ctx.StateSpeedLimit));

            ctx.SetCharacterMovementSpeed(target, false);
            ctx.UpdateCovertEfficiency(ctx.ClampedSpeed, true);

            // Single nudge in case of float deadband; otherwise leave speed as computed.
            if (ctx.CovertNoiseLevel != 0)
            {
                target *= 0.95f;
                ctx.SetCharacterMovementSpeed(target, false);
                ctx.UpdateCovertEfficiency(ctx.ClampedSpeed, true);
                if (ctx.CovertNoiseLevel != 0)
                    return;
            }

            player.RaiseChangeSpeedEvent();
        }

        private static bool IsKeyPressed(KeyboardShortcut key)
        {
            if (!Input.GetKeyDown(key.MainKey))
                return false;

            foreach (var modifier in key.Modifiers)
            {
                if (!Input.GetKey(modifier))
                    return false;
            }

            return true;
        }
    }
}
