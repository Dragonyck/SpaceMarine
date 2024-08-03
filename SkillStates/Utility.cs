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
        public override string animParameter => "Utility";
        public override float forceMagnitude => 2600;
        public override float damageCoefficient => 2.5f;
        public override GameObject hitEffectPrefab => Prefabs.impact;
        public override GameObject swingEffectPrefab => null;
        public override DamageType damageType => DamageType.Generic;
        public override string hitBoxGroupName => "Dash";
        public override float maxAttackSpeedScaling => 1;
        public override bool forceFire => false;
        public override void OnEnter()
        {
            base.OnEnter();

            animator.SetBool("charging", true);
            base.PlayAnimation("Gesture, Override", "Charge", "Utility", baseAttackDuration);

            if (base.isAuthority)
            {
                direction = base.inputBank.aimDirection;
                direction.y = 0f;
                UpdateDirection();
            }
            base.characterDirection.forward = direction;
            base.modelLocator.normalizeToFloor = true;

            if (NetworkServer.active)
            {
                base.characterBody.AddBuff(Prefabs.speed);
            }
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            direction = base.inputBank.aimDirection;
            UpdateDirection();
            if (base.isAuthority)
            {
                if (base.fixedAge >= 0.25f && base.inputBank.skill3.down)
                {
                    outer.SetNextState(new UtilitySwipe());
                    return;
                }

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
                            if (num >= 250)
                            {
                                this.outer.SetNextState(new UtilityImpact
                                {
                                    victim = hurtBox.healthComponent
                                });
                                return;
                            }
                        }
                    }
                }
            }
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
            return base.characterDirection.forward * base.characterBody.moveSpeed;
        }
        public override void OnExit()
        {
            if (NetworkServer.active)
            {
                base.characterBody.RemoveBuff(Prefabs.speed);
            }
            base.modelLocator.normalizeToFloor = false;
            if (!base.characterMotor.disableAirControlUntilCollision)
            {
                base.characterMotor.velocity += GetIdealVelocity();
            }
            animator.SetFloat("Utility", 1);
            animator.SetBool("charging", false);
            base.PlayAnimation("Gesture, Override", "ChargeMiss");
            base.OnExit();
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
    class UtilityImpact : BaseSkillState
    {
        public HealthComponent victim;
        public override void OnSerialize(NetworkWriter writer)
        {
            base.OnSerialize(writer);
            writer.Write(victim ? victim.gameObject : null);
        }
        public override void OnDeserialize(NetworkReader reader)
        {
            base.OnDeserialize(reader);
            GameObject gameObject = reader.ReadGameObject();
            victim = (gameObject ? gameObject.GetComponent<HealthComponent>() : null);
        } 
        public override void OnEnter()
        {
            base.OnEnter();

            var animator = base.GetModelAnimator();
            animator.SetFloat("Utility", 1);
            animator.SetBool("charging", false);
            base.PlayAnimation("Gesture, Override", "ChargeMiss");

            if (NetworkServer.active)
            {
                if (victim)
                {
                    DamageInfo damageInfo = new DamageInfo
                    {
                        attacker = base.gameObject,
                        damage = this.damageStat * 10,
                        crit = base.RollCrit(),
                        procCoefficient = 1f,
                        damageColorIndex = DamageColorIndex.Item,
                        damageType = DamageType.Stun1s,
                        position = base.characterBody.corePosition
                    };
                    victim.TakeDamage(damageInfo);
                    GlobalEventManager.instance.OnHitEnemy(damageInfo, victim.gameObject);
                    GlobalEventManager.instance.OnHitAll(damageInfo, victim.gameObject);
                }
                base.healthComponent.TakeDamageForce(base.characterDirection.forward * -EntityStates.Toolbot.ToolbotDash.knockbackForce, true, false);
            }
            if (base.isAuthority)
            {
                base.AddRecoil(-0.5f * 1 * 3f, -0.5f * 1 * 3f, -0.5f * 1 * 8f, 0.5f * 1 * 3f);
                EffectManager.SpawnEffect(Prefabs.impactHeavy, new EffectData() 
                {
                    origin = base.characterBody.corePosition, 
                    rotation = Util.QuaternionSafeLookRotation(base.characterDirection.forward),
                    scale = 4
                }, true);
                this.outer.SetNextStateToMain();
            }
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Frozen;
        }
    }
    class UtilitySwipe : MeleeSkillState
    {
        public override float baseAttackDuration => EntityStates.Merc.GroundLight.baseComboAttackDuration;
        public override string layerName => "FullBody, Override";
        public override string animationStateName => "Swipe";
        public override string animParameter => "Utility";
        public override string hitBoxGroupName => "Swing";
        public override float forceMagnitude => 4800;
        public override string swingMuzzle => "";
        public override float damageCoefficient => 2.5f;
        public override uint swingSound => Sounds.Play_SpaceMarine_Swing;
        public override float baseHopVelocity => 6;
        public override bool rootMotion => true;
        public override float rootMotionSpeed => 42;
        public override GameObject hitEffectPrefab => Prefabs.impact;
        public override GameObject swingEffectPrefab => null;
        public override DamageType damageType => DamageType.Generic;
        public override void OnEnter()
        {
            base.OnEnter();
        }
    }
}
