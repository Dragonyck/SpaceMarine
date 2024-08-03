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
        internal static GameObject grenadeGhost;
        internal static GameObject areaIndicator;
        internal static GameObject tracer;
        internal static GameObject grenadeExplosion;
        internal static GameObject impact;
        internal static GameObject impactHeavy;
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
            speed = Utils.NewBuffDef("Charge", false, false, Load<Sprite>("RoR2/Base/Common/texMovespeedBuffIcon.tif"), MainPlugin.characterColor);

            impact = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab"), "ImpactEffect", false);
            Utils.RegisterEffect(impact, -1, "Play_SpaceMarine_Impact");

            impactHeavy = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Loader/OmniImpactVFXLoader.prefab"), "HeavyImpactEffect", false);
            Utils.RegisterEffect(impactHeavy, -1, "Play_SpaceMarine_Impact_Heavy", true);

            grenadeExplosion = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/LemurianBruiser/OmniExplosionVFXLemurianBruiserFireballImpact.prefab"), "GrenadeExplosion", false);
            Utils.RegisterEffect(grenadeExplosion, -1, "Play_SpaceMarine_Grenade_Explosion", true);

            tracer = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Junk/Commando/TracerBarrage.prefab"), "Tracer", false);
            var trail = tracer.GetComponent<LineRenderer>();
            trail.startColor = new Color32(255, 83, 0, 255);
            trail.endColor = trail.startColor;
            var tracerP = tracer.GetComponentInChildren<ParticleSystemRenderer>();
            tracerP.material = new Material(tracerP.material);
            tracerP.material.DisableKeyword("VERTEXCOLOR");
            tracerP.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentEyes.png"));
            var main = tracerP.GetComponent<ParticleSystem>().main;
            main.simulationSpeed = 2;
            ContentAddition.AddEffect(tracer);

            var texRampConstructLaserTypeB = Load<Texture2D>("RoR2/DLC1/Common/ColorRamps/texRampConstructLaserTypeB.png");
            areaIndicator = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Huntress/HuntressArrowRainIndicator.prefab"), "AreaIndicator", false);
            var areaIndicatorMesh = areaIndicator.GetComponentInChildren<MeshRenderer>(false);
            areaIndicatorMesh.material = new Material(Load<Material>("RoR2/Base/Common/VFX/matAreaIndicatorIntersectionOnly.mat"));
            areaIndicatorMesh.material.SetColor("_TintColor", new Color32(255, 170, 40, 255));
            areaIndicatorMesh.material.SetTexture("_RemapTex", texRampConstructLaserTypeB);
            var fx = UnityEngine.Object.Instantiate(Load<GameObject>("RoR2/Base/Mushroom/MushroomWard.prefab").GetComponentsInChildren<ParticleSystem>()[1].gameObject, areaIndicator.transform.position, Quaternion.identity, areaIndicator.transform);
            var fxP = fx.GetComponent<ParticleSystemRenderer>();
            fxP.material = new Material(fxP.material);
            fxP.material.SetTexture("_RemapTex", texRampConstructLaserTypeB);

            grenadeGhost = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Commando/CommandoGrenadeGhost.prefab"), "GrenadeProjectileGhost", false);
            var grenadeP = grenadeGhost.GetComponentInChildren<ParticleSystemRenderer>();
            grenadeP.material = new Material(grenadeP.material);
            grenadeP.material.DisableKeyword("VERTEXCOLOR");
            grenadeP.material.SetTexture("_MainTex", Assets.Load<Texture2D>("pulseMask"));
            grenadeP.material.SetTexture("_RemapTex", Load<Texture2D>("RoR2/Base/Common/ColorRamps/texRampParentEyes.png"));
            grenadeGhost.GetComponentInChildren<TrailRenderer>().material = tracerP.material;
            var grenadeMdl = Assets.Load<GameObject>("grenade");
            var mesh = grenadeGhost.GetComponentInChildren<MeshFilter>();
            mesh.mesh = grenadeMdl.GetComponentInChildren<MeshFilter>().mesh;
            var baseMat = new Material(Assets.Load<Material>("diffuse"));
            baseMat.shader = Load<Shader>("RoR2/Base/Shaders/HGStandard.shader");
            mesh.GetComponent<MeshRenderer>().material = baseMat;

            grenade = PrefabAPI.InstantiateClone(Load<GameObject>("RoR2/Base/Commando/CommandoGrenadeProjectile.prefab"), "GrenadeProjectile", true);
            grenade.GetComponent<ProjectileController>().ghostPrefab = grenadeGhost;
            var grenadeImpact = grenade.GetComponent<ProjectileImpactExplosion>();
            grenadeImpact.timerAfterImpact = false;
            grenadeImpact.destroyOnEnemy = true;
            grenadeImpact.destroyOnWorld = true;
            grenadeImpact.impactEffect = grenadeExplosion;
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
