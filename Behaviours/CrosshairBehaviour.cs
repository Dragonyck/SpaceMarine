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
    class CrosshairBehaviour : CrosshairController
    {
        private InputBankTest input;
        public GameObject off;
        public GameObject on;
        private void Start()
        {
            var hud = base.GetComponent<HudElement>();
            if (hud && hud.targetBodyObject)
            {
                input = hud.targetBodyObject.GetComponent<InputBankTest>();
            }
        }
        private void FixedUpdate()
        {
            if (input)
            {
                float num = 0f;
                RaycastHit raycastHit;
                bool hit = Physics.Raycast(CameraRigController.ModifyAimRayIfApplicable(input.GetAimRay(), base.gameObject, out num), out raycastHit, 250 + num, LayerIndex.entityPrecise.mask);
                off.SetActive(!hit);
                on.SetActive(hit);
            }
        }
    }
}
