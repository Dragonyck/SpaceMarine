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
    class Primary : BaseSkillState
    {
        private float duration;
        private float baseDuration = 0.25f;
        private float damageCoefficient = 1.5f;
        private float explosionDamageCoefficient = 0.5f;
        private bool bulletHit;
        private Vector3 hitPoint;

        public override void OnEnter()
        {
            base.OnEnter();
            base.StartAimMode(2, true);

            duration = baseDuration / base.attackSpeedStat;

            base.PlayAnimation("Gesture, Override", "Fire", "M1", duration);

            EffectManager.SimpleMuzzleFlash(Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/Muzzleflash1.prefab"), base.gameObject, "gunMuzzle", false);

            float recoilAmplitude = 1.2f;
            base.AddRecoil(-1f * recoilAmplitude, -1.5f * recoilAmplitude, -0.25f * recoilAmplitude, 0.25f * recoilAmplitude);
            base.characterBody.AddSpreadBloom(2);

            AkSoundEngine.PostEvent(Sounds.Play_SpaceMarine_Bolt_Fire, base.gameObject);
            //AkSoundEngine.PostEvent(EntityStates.ClayBruiser.Weapon.MinigunFire.fireSound, base.gameObject);
            if (base.isAuthority)
            {
                Ray aimRay = base.GetAimRay();
                float spread = 0.0425f;
                new BulletAttack
                {
                    bulletCount = 1,
                    aimVector = aimRay.direction,
                    origin = aimRay.origin,
                    damage = base.damageStat * damageCoefficient,
                    damageColorIndex = DamageColorIndex.Default,
                    damageType = DamageType.Generic,
                    falloffModel = BulletAttack.FalloffModel.DefaultBullet,
                    maxDistance = 250,
                    force = 20,
                    hitMask = LayerIndex.CommonMasks.bullet,
                    minSpread = -spread,
                    maxSpread = spread,
                    isCrit = base.RollCrit(),
                    owner = base.gameObject,
                    muzzleName = "smgMuzzle",
                    smartCollision = false,
                    procChainMask = default(ProcChainMask),
                    procCoefficient = 1,
                    radius = 0.08f,
                    sniper = false,
                    stopperMask = LayerIndex.CommonMasks.bullet,
                    weapon = null,
                    tracerEffectPrefab = Prefabs.Load<GameObject>("RoR2/Junk/Commando/TracerBarrage.prefab"),
                    spreadPitchScale = 1f,
                    spreadYawScale = 1f,
                    hitEffectPrefab = Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFX.prefab"),
                    hitCallback = delegate (BulletAttack _bulletAttack, ref BulletAttack.BulletHit info)
                    {
                        if (info.point != Vector3.zero)
                        {
                            hitPoint = info.point;
                        }
                        return BulletAttack.defaultHitCallback(_bulletAttack, ref info);
                    }
                }.Fire();

                if (hitPoint != Vector3.zero)
                {
                    new BlastAttack()
                    {
                        attacker = base.gameObject,
                        attackerFiltering = AttackerFiltering.NeverHitSelf,
                        baseDamage = base.damageStat * explosionDamageCoefficient,
                        crit = base.RollCrit(),
                        damageType = DamageType.Generic,
                        falloffModel = BlastAttack.FalloffModel.None,
                        position = hitPoint,
                        procCoefficient = 0.25f,
                        radius = 4,
                        teamIndex = base.teamComponent.teamIndex
                    }.Fire();

                    EffectManager.SpawnEffect(Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab"), new EffectData() 
                    { 
                        origin = hitPoint,
                        scale = 2
                    }, true);
                }
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.characterDirection)
            {
                Vector2 vector = Util.Vector3XZToVector2XY(base.GetAimRay().direction);
                if (vector != Vector2.zero)
                {
                    vector.Normalize();
                    var direction = new Vector3(vector.x, 0f, vector.y).normalized;
                    base.characterDirection.moveVector = direction;
                }
            }
            base.characterBody.isSprinting = false;
            base.skillLocator.primary.rechargeStopwatch = 0;
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