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
    class CharacterMain : GenericCharacterMain
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            modelAnimator.SetFloat("inCombat", base.characterBody.outOfCombat ? 0 : 1);
        }
    }
    class PassiveBarrier : Idle
    {
        public override void FixedUpdate()
        {
            base.FixedUpdate();
            if (NetworkServer.active && base.fixedAge >= 5)
            {
                base.fixedAge = 0;

                base.healthComponent.AddBarrier(base.healthComponent.fullCombinedHealth * ((base.characterBody.level * 2) / 100));
            }
        }
    }
}
