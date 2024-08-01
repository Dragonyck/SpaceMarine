using System;
using System.Reflection;
using System.Collections.Generic;
using BepInEx;
using R2API;
using R2API.Utils;
using EntityStates;
using RoR2;
using RoR2.Projectile;
using RoR2.Skills;
using UnityEngine;
using UnityEngine.Networking;
using KinematicCharacterController;
using BepInEx.Configuration;
using RoR2.UI;
using UnityEngine.UI;
using System.Security;
using System.Security.Permissions;
using System.Linq;
using R2API.ContentManagement;
using UnityEngine.AddressableAssets;

namespace SpaceMarine
{
    class Hook
    {
        internal static void Hooks()
        {
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.EntityStates.Toolbot.ToolbotDash.OnEnter += ToolbotDash_OnEnter;
            On.RoR2.HealthComponent.TakeDamageForce_DamageInfo_bool_bool += HealthComponent_TakeDamageForce_DamageInfo_bool_bool;
            On.RoR2.HealthComponent.TakeDamageForce_Vector3_bool_bool += HealthComponent_TakeDamageForce_Vector3_bool_bool;
        }
        private static void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender.HasBuff(Prefabs.speed))
            {
                args.moveSpeedMultAdd += 1;
            }
        }
        private static void ToolbotDash_OnEnter(On.EntityStates.Toolbot.ToolbotDash.orig_OnEnter orig, EntityStates.Toolbot.ToolbotDash self)
        {
            Debug.LogWarning("massThresholdForKnockback: " + EntityStates.Toolbot.ToolbotDash.massThresholdForKnockback);
            Debug.LogWarning("recoilAmplitude: " + EntityStates.Toolbot.ToolbotDash.recoilAmplitude);
            Debug.LogWarning("speedMultiplier: " + self.speedMultiplier);
            orig(self);
        }

        private static void HealthComponent_TakeDamageForce_Vector3_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_Vector3_bool_bool orig, HealthComponent self, Vector3 force, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(Prefabs.knockback))
            {
                force = Vector3.zero;
                disableAirControlUntilCollision = false;
            }
            orig(self, force, alwaysApply, disableAirControlUntilCollision);
        }
        private static void HealthComponent_TakeDamageForce_DamageInfo_bool_bool(On.RoR2.HealthComponent.orig_TakeDamageForce_DamageInfo_bool_bool orig, HealthComponent self, DamageInfo damageInfo, bool alwaysApply, bool disableAirControlUntilCollision)
        {
            if (self.body && self.body.HasBuff(Prefabs.knockback))
            {
                damageInfo.force = Vector3.zero;
                disableAirControlUntilCollision = false;
            }
            orig(self, damageInfo, alwaysApply, disableAirControlUntilCollision);
        }
    }
}
