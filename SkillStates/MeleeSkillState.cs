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
using HG;
using System.Runtime.InteropServices;
using UnityEngine.Events;

namespace SpaceMarine
{
    class MeleeSkillState : BaseSkillState
    {
        public float attackDuration;
        public float earlyExitDuration;
        public virtual float baseAttackDuration => 0;
        public virtual float baseEarlyExitDuration => 0;
        public virtual float damageCoefficient => 0;
        public virtual float forceMagnitude => 440;
        public virtual float rootMotionSpeed => 25;
        public virtual float baseHopVelocity => 4;
        public virtual string layerName => "FullBody, Override";
        public virtual string animationStateName => "";
        public virtual string animParameter => "M1";
        public virtual string hitBoxGroupName => animationStateName;
        public virtual string hitBoxActiveParameter => "Curve";
        public virtual string swingMuzzle => animationStateName + "Muzzle";
        public virtual GameObject swingEffectPrefab => null;
        public virtual bool hopOnHit => true;
        public virtual bool rootMotion => true;
        public virtual bool rootMotionWhileHitting => false;
        public virtual uint swingSound => 0;
        public virtual DamageType damageType => DamageType.Generic;
        public virtual DamageColorIndex damageColor => DamageColorIndex.Default;
        public virtual Vector3 bonusForce => Vector3.zero;
        public virtual GameObject hitEffectPrefab => null;
        private bool hopped;
        private bool hasSwung;
        public bool isInHitPause;
        public float hitPauseDuration;
        public float hopVelocity;
        public float hitPauseTimer;
        public float stopwatch;
        public BaseState.HitStopCachedState hitStopCachedState;
        public OverlapAttack overlapAttack;
        public bool hasHit;
        private bool hasAnimParameter;
        private float attackSpeedScaling;
        public Animator animator;

        public override void OnEnter()
        {
            base.OnEnter();
            attackSpeedScaling = Math.Min(base.attackSpeedStat, 6);
            attackDuration = baseAttackDuration / attackSpeedScaling;
            earlyExitDuration = baseEarlyExitDuration / attackSpeedScaling;
            hitPauseDuration = EntityStates.Merc.GroundLight.hitPauseDuration / attackSpeedScaling;
            hopVelocity = baseHopVelocity / attackSpeedScaling;

            base.PlayAnimation(layerName, "BufferEmpty");

            animator = base.GetModelAnimator();
            animator.SetFloat(hitBoxActiveParameter, 0);

            base.StartAimMode(attackDuration + 1f);

            overlapAttack = base.InitMeleeOverlap(damageCoefficient, hitEffectPrefab, base.GetModelTransform(), hitBoxGroupName);
            overlapAttack.pushAwayForce = 1;

            hasAnimParameter = !animParameter.IsNullOrWhiteSpace();
            if (hasAnimParameter && !animationStateName.IsNullOrWhiteSpace())
            {
                base.PlayCrossfade(layerName, animationStateName, animParameter, attackDuration, 0.1f);
            }
        }
        public virtual Vector3 rootMotionDirection()
        {
            return base.characterDirection.forward;
        }
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (base.isAuthority)
            {
                bool hit = FireMeleeAttack(overlapAttack, animator, hitBoxActiveParameter, forceMagnitude, bonusForce);
                hasHit = hit;
                if (hasHit)
                {
                    if (hopOnHit && !base.characterMotor.isGrounded && !hopped)
                    {
                        base.SmallHop(base.characterMotor, hopVelocity);
                        hopped = true;
                    }
                    if (!rootMotionWhileHitting && !isInHitPause && hasAnimParameter)
                    {
                        hitStopCachedState = base.CreateHitStopCachedState(base.characterMotor, animator, animParameter);
                        isInHitPause = true;
                    }
                }
                if (animator.GetFloat(hitBoxActiveParameter) > 0.1f && rootMotion && !isInHitPause && !hasHit)
                {
                    Vector3 dir = rootMotionDirection();
                    var direction = new Vector3(dir.x, 0f, dir.z);
                    base.characterMotor.rootMotion += direction * rootMotionSpeed * Time.fixedDeltaTime;
                }
                if (hitPauseTimer >= hitPauseDuration && isInHitPause)
                {
                    base.ConsumeHitStopCachedState(hitStopCachedState, base.characterMotor, animator);
                    isInHitPause = false;
                    animator.speed = 1;
                }
                if (!isInHitPause)
                {
                    stopwatch += Time.fixedDeltaTime;
                }
                else
                {
                    hitPauseTimer += Time.fixedDeltaTime;
                    base.characterMotor.velocity = Vector3.zero;
                    animator.speed = 0;
                }
                if (stopwatch >= attackDuration - earlyExitDuration)
                {
                    if (base.inputBank.skill1.down)
                    {
                        SetState();
                    }
                    if (stopwatch >= attackDuration)
                    {
                        var state = StateOverride();
                        if (state != null)
                        {
                            outer.SetNextState(state);
                            return;
                        }
                        outer.SetNextStateToMain();
                        return;
                    }
                }
            }
            if (animator.GetFloat(hitBoxActiveParameter) >= 0.11f && !hasSwung)
            {
                hasSwung = true;
                AkSoundEngine.PostEvent(swingSound, base.gameObject);
                if (swingEffectPrefab && !swingMuzzle.IsNullOrWhiteSpace())
                {
                    EffectManager.SimpleMuzzleFlash(swingEffectPrefab, base.gameObject, swingMuzzle, false);
                }
            }
        }
        public bool FireMeleeAttack(OverlapAttack attack, Animator animator, string mecanimHitboxActiveParameter, float forceMagnitude, Vector3 bonusForce)
        {
            bool result = false;
            if (animator && animator.GetFloat(mecanimHitboxActiveParameter) > 0.1f)
            {
                attack.forceVector = base.characterDirection ? base.characterDirection.forward : base.transform.forward * forceMagnitude + bonusForce;
                result = attack.Fire(null);
            }
            return result;
        }
        public virtual void SetState()
        {
        }
        public virtual BaseSkillState StateOverride()
        {
            return null;
        }
        public override InterruptPriority GetMinimumInterruptPriority()
        {
            return InterruptPriority.Skill;
        }
    }
}