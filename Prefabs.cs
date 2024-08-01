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
    class Prefabs
    {
        //internal static GameObject prefab;
        internal static GameObject crosshair;
        internal static GameObject grenade;
        internal static GameObject areaIndicator;
        internal static BuffDef knockback;
        internal static BuffDef speed;
        internal static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        internal static void CreatePrefabs()
        {
            //prefab = PrefabAPI.InstantiateClone(Load<GameObject>("path"), "prefabname", false);

            knockback = Utils.NewBuffDef("Iron Resolve", false, false, Assets.Load<Sprite>("buffIcon"), new Color32(241, 177, 0, 255));
            speed = Utils.NewBuffDef("Charge", false, false, Load<Sprite>("RoR2/Base/Achievements/texMoveSpeedIcon.png"), MainPlugin.characterColor);

            areaIndicator = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab"), "GrenadeProjectile", false);
            var areaIndicatorMesh = areaIndicator.GetComponentInChildren<MeshRenderer>(false);
            areaIndicatorMesh.material = new Material(areaIndicatorMesh.material);
            areaIndicatorMesh.material.SetColor("_TintColor", new Color32(255, 170, 40, 255));
            areaIndicatorMesh.material.SetTexture("_RampTex", Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampConstructLaserTypeB.png"));
            var fx = UnityEngine.Object.Instantiate(Load<GameObject>("RoR2/Base/Mushroom/MushroomWard.prefab").GetComponentsInChildren<ParticleSystem>()[1].gameObject, areaIndicator.transform.position, Quaternion.identity, areaIndicator.transform);

            grenade = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab"), "GrenadeProjectile", false);
            var grenadeImpact = grenade.GetComponent<ProjectileImpactExplosion>();
            grenadeImpact.timerAfterImpact = false;
            grenadeImpact.destroyOnEnemy = true;
            grenadeImpact.destroyOnWorld = true;
            ContentAddition.AddProjectile(grenade);

            crosshair = PrefabAPI.InstantiateClone(Assets.Load<GameObject>("crosshair"), "Crosshair", false);
            var child = crosshair.GetComponent<ChildLocator>();
            var c = crosshair.AddComponent<CrosshairBehaviour>();
            c.off = child.FindChild("bgOff").gameObject;
            c.on = child.FindChild("bgOn").gameObject;
            c.maxSpreadAngle = 18;
            List<CrosshairController.SpritePosition> spritePosList = new List<CrosshairController.SpritePosition>();
            foreach (Image i in child.FindChild("nibs").GetComponentsInChildren<Image>())
            {
                spritePosList.Add(new CrosshairController.SpritePosition()
                {
                    zeroPosition = i.transform.localPosition * 0.28571f,
                    onePosition = i.transform.localPosition,
                    target = (RectTransform)i.transform
                });
            }
            c.spriteSpreadPositions = spritePosList.ToArray();
        }
    }
}
