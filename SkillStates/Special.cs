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
    class Special : BaseSkillState
    {
        private float duration;
        private SphereSearch search = new SphereSearch();
        private Vector3 pos;
        private GameObject areaIndicator;
        private TeamMask mask;
        private float stopwatch;
        private float radius = 9;

        public override void OnEnter()
        {
            base.OnEnter();

            duration = 10 + base.characterBody.level;

            areaIndicator = UnityEngine.Object.Instantiate(Prefabs.areaIndicator, base.transform.position, Quaternion.identity);
            areaIndicator.transform.localScale = Vector3.one * radius;

            if (NetworkServer.active)
            {
                base.characterBody.AddTimedBuff(Prefabs.knockback, duration);

                search.origin = pos;
                search.mask = LayerIndex.entityPrecise.mask;
                search.radius = radius;
                mask = default(TeamMask);
                mask.AddTeam(TeamIndex.Player);
                search.mask = LayerIndex.entityPrecise.mask;
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            pos = base.transform.position;

            if (areaIndicator)
            {
                areaIndicator.transform.position = pos;
            }

            if (NetworkServer.active)
            {
                stopwatch += Time.fixedDeltaTime;
                if (stopwatch >= 1)
                {
                    stopwatch = 0;

                    search.origin = pos;
                    foreach (HurtBox h in search.RefreshCandidates().FilterCandidatesByHurtBoxTeam(mask).OrderCandidatesByDistance().FilterCandidatesByDistinctHurtBoxEntities().GetHurtBoxes())
                    {
                        h.healthComponent.Heal(base.healthComponent.fullCombinedHealth * 0.025f, default);
                        h.healthComponent.body.AddTimedBuff(Prefabs.knockback, 1.5f);
                    }
                }
            }
            if (base.fixedAge >= this.duration && base.isAuthority)
            {
                this.outer.SetNextStateToMain();
            }
        }
        public override void OnExit()
        {
            if (areaIndicator)
            {
                Destroy(areaIndicator);
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Death;
        }
    }
}
