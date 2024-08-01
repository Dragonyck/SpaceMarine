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
    class Secondary : BaseSkillState
    {
        private float duration;
        private float baseDuration = 0.4f;
        private float damageCoefficient = 2.5f;
        private Animator animator;
        private bool hasFired;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = baseDuration / base.attackSpeedStat;

            base.PlayAnimation("Gesture, Override", "Throw", "M2", duration);

            animator = base.GetModelAnimator();
            animator.SetFloat("Curve", 0);

            duration = baseDuration / base.attackSpeedStat;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority && animator.GetFloat("Curve") >= 0.1f && !hasFired)
            {
                hasFired = true;

                Ray aimRay = base.GetAimRay();
                ProjectileManager.instance.FireProjectile(Prefabs.grenade, aimRay.origin, Util.QuaternionSafeLookRotation(aimRay.direction), base.gameObject, base.damageStat * damageCoefficient, 680, base.RollCrit(), default);

                EffectManager.SimpleMuzzleFlash(Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab"), base.gameObject, "hand.l", true);
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.PrioritySkill;
        }
    }
}
