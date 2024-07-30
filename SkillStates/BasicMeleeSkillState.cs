
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
    class BasicMeleeSkillState : BaseSkillState
    {
        private float duration = 0.2f;
        private Vector3 dir;
        private OverlapAttack attack;
        private float damageCoefficient = 4.2f;
        private GameObject hitEffectPrefab = null;
        private bool parried;
        public Animator animator;
        private uint ID;
        private string hitboxGroupName = "";

        public override void OnEnter()
        {
            base.OnEnter();
            animator = base.GetModelAnimator();
            if (!animator.GetBool("slide"))
            {
                base.StartAimMode(1, true);
            }

            base.PlayAnimation("LeftArm, Override", "MeleeAttack");

            AkSoundEngine.PostEvent(ID, base.gameObject);

            attack = base.InitMeleeOverlap(damageCoefficient, hitEffectPrefab, base.GetModelTransform(), hitboxGroupName);
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            //base.characterBody.isSprinting = false;
            if (base.isAuthority)
            {
                attack.Fire();
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
            return InterruptPriority.Skill;
        }
    }
}