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
    class Utility : MeleeSkillState
    {
        private Vector3 direction;
        public override float baseAttackDuration => 3;
        public override string animParameter => "walkSpeed";
        public override float forceMagnitude => 600;
        public override float damageCoefficient => 2.5f;
        public override GameObject hitEffectPrefab => Prefabs.Load<GameObject>("RoR2/Base/Common/VFX/OmniImpactVFXSlash.prefab");
        public override GameObject swingEffectPrefab => null;
        public override DamageType damageType => DamageType.Generic;
        public override string hitBoxGroupName => "Dash";
        public override void OnEnter()
        {
            base.OnEnter();
            animator.SetFloat("Curve", 1);
            if (base.isAuthority)
            {
                direction = base.inputBank.aimDirection;
                direction.y = 0f;
                UpdateDirection();
            }
            base.characterDirection.forward = direction;
            base.modelLocator.normalizeToFloor = true;
        }
        public override void FixedUpdate()
        {
            if (base.isAuthority)
            {
                overlapAttack.damage = base.damageStat * (damageCoefficient * GetDamageBoostFromSpeed());
            }
            base.FixedUpdate();
            animator.SetFloat("Curve", 1);
            direction = base.inputBank.aimDirection;
            UpdateDirection();
            if (base.isAuthority)
            {
                base.characterBody.isSprinting = true;

                if (!isInHitPause)
                {
                    base.characterDirection.moveVector = direction;
                    if (!base.characterMotor.disableAirControlUntilCollision)
                    {
                        base.characterMotor.rootMotion += GetIdealVelocity() * Time.fixedDeltaTime;
                    }
                }

                if (base.skillLocator.special && base.inputBank.skill4.down)
                {
                    base.skillLocator.special.ExecuteIfReady();
                }

                if (hasHit)
                {
                    for (int i = 0; i < hitList.Count; i++)
                    {
                        float num = 0f;
                        HurtBox hurtBox = hitList[i];
                        if (hurtBox.healthComponent)
                        {
                            CharacterMotor component = hurtBox.healthComponent.GetComponent<CharacterMotor>();
                            if (component)
                            {
                                num = component.mass;
                            }
                            else
                            {
                                Rigidbody component2 = hurtBox.healthComponent.GetComponent<Rigidbody>();
                                if (component2)
                                {
                                    num = component2.mass;
                                }
                            }
                            if (num >= 300)
                            {
                                outer.SetNextStateToMain();
                                return;
                                /*this.outer.SetNextState(new ToolbotDashImpact
                                {
                                    victimHealthComponent = hurtBox.healthComponent,
                                    idealDirection = this.idealDirection,
                                    damageBoostFromSpeed = this.GetDamageBoostFromSpeed(),
                                    isCrit = this.attack.isCrit
                                });
                                return;*/
                            }
                        }
                    }
                }
            }
        }
        private float GetDamageBoostFromSpeed()
        {
            return Mathf.Max(1f, base.characterBody.moveSpeed / base.characterBody.baseMoveSpeed);
        }
        private void UpdateDirection()
        {
            if (base.inputBank)
            {
                Vector2 vector = Util.Vector3XZToVector2XY(base.inputBank.moveVector);
                if (vector != Vector2.zero)
                {
                    vector.Normalize();
                    direction = new Vector3(vector.x, 0f, vector.y).normalized;
                }
            }
        }
        private Vector3 GetIdealVelocity()
        {
            return base.characterDirection.forward * base.characterBody.moveSpeed * 1.2f;
        }
        public override void OnExit()
        {
            base.modelLocator.normalizeToFloor = false;
            if (!base.characterMotor.disableAirControlUntilCollision)
            {
                base.characterMotor.velocity += this.GetIdealVelocity();
            }
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
}
