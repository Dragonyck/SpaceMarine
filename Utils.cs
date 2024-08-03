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
using UnityEngine.AddressableAssets;
using Rewired.ComponentControls.Effects;

namespace SpaceMarine
{
    static class Extensions
    {
        public static GameObject NewSyringe(this GameObject g, params Material[] m)
        {
            g.GetComponentsInChildren<MeshRenderer>(false)[0].material = m[0];
            g.GetComponent<TrailRenderer>().material = m[1];
            g.GetComponent<ParticleSystemRenderer>().material = m[2];
            return g;
        }
        public static Material SwapTexture(this Material m, Texture2D t)
        {
            m.mainTexture = t;
            return m;
        }
        public static GameObject SwapMaterials(this GameObject g, Material[] m)
        {
            var renderers = g.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                if (m[i])
                {
                    renderers[i].material = m[i];
                }
            }
            return g;
        }
        public static float Mult(this float f, float mult = 100)
        {
            return f * mult;
        }
        public static Color ToRGB255(this Color c, string title, string body)
        {
            return new Color(c.r / 255, c.g / 255, c.b / 255);
        }
        public static void SetStrings(this TooltipProvider t, string title, string body)
        {
            t.overrideTitleText = title;
            t.overrideBodyText = body;
        }
    }
    class Utils
    {
        public static SkillDef NewSkillDef<T>(Type activationState, string activationStateMachineName, int baseMaxStock, float baseRechargeInterval, bool beginSkillCooldownOnSkillEnd, bool canceledFromSprinting, bool fullRestockOnAssign, InterruptPriority interruptPriority,
            bool isCombatSkill, bool mustKeyPress, bool cancelSprintingOnActivation, int rechargeStock, int requiredStock, int stockToConsume, Sprite icon, string skillDescriptionToken, string skillName, params string[] keywordTokens) where T : SkillDef
        {
            var SkillDef = ScriptableObject.CreateInstance<T>();
            SkillDef.activationState = new SerializableEntityStateType(activationState);
            SkillDef.activationStateMachineName = activationStateMachineName;
            SkillDef.baseMaxStock = baseMaxStock;
            SkillDef.baseRechargeInterval = baseRechargeInterval;
            SkillDef.beginSkillCooldownOnSkillEnd = beginSkillCooldownOnSkillEnd;
            SkillDef.canceledFromSprinting = canceledFromSprinting;
            SkillDef.fullRestockOnAssign = fullRestockOnAssign;
            SkillDef.interruptPriority = interruptPriority;
            SkillDef.isCombatSkill = isCombatSkill;
            SkillDef.mustKeyPress = mustKeyPress;
            SkillDef.cancelSprintingOnActivation = cancelSprintingOnActivation;
            SkillDef.rechargeStock = rechargeStock;
            SkillDef.requiredStock = requiredStock;
            SkillDef.stockToConsume = stockToConsume;
            SkillDef.icon = icon;
            SkillDef.skillDescriptionToken = skillDescriptionToken;
            SkillDef.skillName = skillName;
            SkillDef.skillNameToken = SkillDef.skillName;
            SkillDef.keywordTokens = keywordTokens;
            ContentAddition.AddSkillDef(SkillDef);
            return SkillDef;
        }
        public static Color HexTo10(string hexColor)
        {
            var characters = hexColor.ToCharArray();
            Color color = new Color(characters[0] + characters[1], characters[2] + characters[3], characters[4] + characters[5], 0xFF) / 255;
            return color;
        }
        public static EntityStateMachine NewStateMachine<T>(GameObject obj, string customName) where T : EntityState
        {
            SerializableEntityStateType s = new SerializableEntityStateType(typeof(T));
            var newStateMachine = obj.AddComponent<EntityStateMachine>();
            newStateMachine.customName = customName;
            newStateMachine.initialStateType = s;
            newStateMachine.mainStateType = s;
            return newStateMachine;
        }
        public static GenericSkill NewGenericSkill(GameObject obj, SkillDef skill)
        {
            GenericSkill generic = obj.AddComponent<GenericSkill>();
            SkillFamily newFamily = ScriptableObject.CreateInstance<SkillFamily>();
            newFamily.variants = new SkillFamily.Variant[1];
            generic._skillFamily = newFamily;
            SkillFamily skillFamily = generic.skillFamily;
            skillFamily.variants[0] = new SkillFamily.Variant
            {
                skillDef = skill,
                viewableNode = new ViewablesCatalog.Node(skill.skillNameToken, false, null)
            };
            ContentAddition.AddSkillFamily(skillFamily);
            return generic;
        }
        public static void AddAlt(SkillFamily skillFamily, SkillDef SkillDef)
        {
            Array.Resize<SkillFamily.Variant>(ref skillFamily.variants, skillFamily.variants.Length + 1);
            skillFamily.variants[skillFamily.variants.Length - 1] = new SkillFamily.Variant
            {
                skillDef = SkillDef,
                viewableNode = new ViewablesCatalog.Node(SkillDef.skillNameToken, false, null)
            };
        }
        public static BuffDef NewBuffDef(string name, bool stack, bool hidden, Sprite sprite, Color color)
        {
            BuffDef buff = ScriptableObject.CreateInstance<BuffDef>();
            buff.name = name;
            buff.canStack = stack;
            buff.isHidden = hidden;
            buff.iconSprite = sprite;
            buff.buffColor = color;
            ContentAddition.AddBuffDef(buff);
            return buff;
        }
        public static ObjectScaleCurve AddScaleComponent(GameObject target, float timeMax)
        {
            ObjectScaleCurve scale = target.AddComponent<ObjectScaleCurve>();
            scale.useOverallCurveOnly = true;
            scale.timeMax = timeMax;
            scale.overallCurve = AnimationCurve.Linear(0, 0, 1, 1);
            return scale;
        }
        public static RotateAroundAxis AddRotationComponent(GameObject target, float speed, RotateAroundAxis.RotationAxis axis)
        {
            RotateAroundAxis rot = target.AddComponent<RotateAroundAxis>();
            rot.speed = RotateAroundAxis.Speed.Fast;
            rot.fastRotationSpeed = speed;
            rot.rotateAroundAxis = axis;
            return rot;
        }
        public static GameObject NewDisplayModel(GameObject model, string name)
        {
            GameObject characterDisplay = PrefabAPI.InstantiateClone(model, name, false);
            characterDisplay.GetComponentInChildren<Animator>().runtimeAnimatorController = Assets.Load<RuntimeAnimatorController>("displayAnimator");
            characterDisplay.GetComponentInChildren<CharacterModel>().enabled = false;
            foreach (SkinnedMeshRenderer r in characterDisplay.GetComponentsInChildren<SkinnedMeshRenderer>(true))
            {
                r.material = new Material(r.material);
                r.material.DisableKeyword("DITHER");
            }
            return characterDisplay;
        }
        public static AnimateShaderAlpha AddShaderAlphaComponent(GameObject target, float timeMax, AnimationCurve curve, bool destroyOnEnd = true, bool disableOnEnd = false)
        {
            AnimateShaderAlpha c = target.AddComponent<AnimateShaderAlpha>();
            c.timeMax = timeMax;
            c.alphaCurve = curve;
            c.destroyOnEnd = destroyOnEnd;
            c.disableOnEnd = disableOnEnd;
            return c;
        }
        internal static Sprite CreateSprite(Texture2D tex)
        {
            return Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 128);
        }
        internal static LoadoutAPI.SkinDefInfo CreateNewSkinDefInfo(GameObject rootObject, string skinName, CharacterModel.RendererInfo[] rendererInfos, UnlockableDef unlockableDef = null)
        {
            LoadoutAPI.SkinDefInfo skinDefInfo = default(LoadoutAPI.SkinDefInfo);
            skinDefInfo.BaseSkins = Array.Empty<SkinDef>();
            skinDefInfo.MinionSkinReplacements = new SkinDef.MinionSkinReplacement[0];
            skinDefInfo.ProjectileGhostReplacements = new SkinDef.ProjectileGhostReplacement[0];
            skinDefInfo.GameObjectActivations = new SkinDef.GameObjectActivation[0];
            skinDefInfo.Icon = Assets.MainAssetBundle.LoadAsset<Sprite>("skinIcon");
            skinDefInfo.MeshReplacements = new SkinDef.MeshReplacement[0];
            skinDefInfo.Name = skinName;
            skinDefInfo.NameToken = skinName;
            skinDefInfo.RendererInfos = rendererInfos;
            skinDefInfo.RootObject = rootObject;
            skinDefInfo.UnlockableDef = unlockableDef;
            return skinDefInfo;
        }
        internal static T CopyComponent<T>(T original, GameObject destination) where T : Component
        {
            System.Type type = original.GetType();
            Component copy = destination.AddComponent(type);
            System.Reflection.FieldInfo[] fields = type.GetFields();
            foreach (System.Reflection.FieldInfo field in fields)
            {
                field.SetValue(copy, field.GetValue(original));
            }
            return copy as T;
        }
        public static Sprite CreateSpriteFromTexture(Texture2D texture)
        {
            if (texture)
            {
                var sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                return sprite;
            }
            return null;
        }
        public static GameObject FindInActiveObjectByName(string name)
        {
            Transform[] objs = Resources.FindObjectsOfTypeAll<Transform>() as Transform[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i].gameObject;
                    }
                }
            }
            return null;
        }
        public static void CreateHitboxes(string name, Transform parent, Vector3[] sizes, Vector3[] positions)
        {
            var hitBoxGroup1 = parent.gameObject.AddComponent<HitBoxGroup>();
            hitBoxGroup1.groupName = name;
            List<HitBox> hitboxes = new List<HitBox>();
            for (int i = 0; i < sizes.Length; i++)
            {
                var hitboxTransform1 = new GameObject(name + (i + 1));
                hitboxTransform1.transform.SetParent(parent);
                hitboxTransform1.transform.localPosition = positions[i];
                hitboxTransform1.transform.localRotation = Quaternion.identity;
                hitboxTransform1.transform.localScale = sizes[i];
                HitBox hitBox = hitboxTransform1.AddComponent<HitBox>();
                hitboxTransform1.layer = LayerIndex.projectile.intVal;
                hitboxes.Add(hitBox);
            }
            hitBoxGroup1.hitBoxes = hitboxes.ToArray();
        }
        public static GameObject CreateHitbox(string name, Transform parent, Vector3 scale, Vector3 localPosition)
        {
            var hitboxTransform1 = new GameObject(name);
            hitboxTransform1.transform.SetParent(parent);
            hitboxTransform1.transform.localPosition = localPosition;
            hitboxTransform1.transform.localRotation = Quaternion.identity;
            hitboxTransform1.transform.localScale = scale;
            var hitBoxGroup1 = parent.gameObject.AddComponent<HitBoxGroup>();
            HitBox hitBox = hitboxTransform1.AddComponent<HitBox>();
            hitboxTransform1.layer = LayerIndex.projectile.intVal;
            hitBoxGroup1.hitBoxes = new HitBox[] { hitBox };
            hitBoxGroup1.groupName = name;
            return hitboxTransform1;
        }
        internal static EffectComponent RegisterEffect(GameObject effect, float duration, string soundName = "", bool applyScale = false, bool parentToReferencedTransform = true, bool positionAtReferencedTransform = true)
        {
            var effectcomponent = effect.GetComponent<EffectComponent>();
            if (!effectcomponent)
            {
                effectcomponent = effect.AddComponent<EffectComponent>();
            }
            if (duration != -1)
            {
                var destroyOnTimer = effect.GetComponent<DestroyOnTimer>();
                if (!destroyOnTimer)
                {
                    effect.AddComponent<DestroyOnTimer>().duration = duration;
                }
                else
                {
                    destroyOnTimer.duration = duration;
                }
            }
            if (!effect.GetComponent<NetworkIdentity>())
            {
                effect.AddComponent<NetworkIdentity>();
            }
            if (!effect.GetComponent<VFXAttributes>())
            {
                effect.AddComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            }
            effectcomponent.applyScale = applyScale;
            effectcomponent.effectIndex = EffectIndex.Invalid;
            effectcomponent.parentToReferencedTransform = parentToReferencedTransform;
            effectcomponent.positionAtReferencedTransform = positionAtReferencedTransform;
            effectcomponent.soundName = soundName;
            ContentAddition.AddEffect(effect);
            return effectcomponent;
        }
        public static Material InstantiateMaterial(Texture tex)
        {
            Material mat = UnityEngine.Object.Instantiate<Material>(Prefabs.Load<Material>("RoR2/Base/Commando/matCommandoDualies.mat"));
            if (mat)
            {
                mat.SetColor("_Color", Color.white);
                mat.SetTexture("_MainTex", tex);
                mat.SetColor("_EmColor", Color.black);
                mat.SetFloat("_EmPower", 0);
                mat.SetTexture("_EmTex", null);
                mat.SetFloat("_NormalStrength", 1f);
                mat.SetTexture("_NormalTex", null);
                return mat;
            }
            return mat;
        }
        public static Material InstantiateMaterial(Color color, Texture tex, Color emColor, float emPower, Texture emTex, float normStr, Texture normTex)
        {
            Material mat = UnityEngine.Object.Instantiate<Material>(LegacyResourcesAPI.Load<GameObject>("Prefabs/CharacterBodies/CommandoBody").GetComponentInChildren<CharacterModel>().baseRendererInfos[0].defaultMaterial);
            if (mat)
            {
                mat.SetColor("_Color", color);
                mat.SetTexture("_MainTex", tex);
                mat.SetColor("_EmColor", emColor);
                mat.SetFloat("_EmPower", emPower);
                mat.SetTexture("_EmTex", emTex);
                mat.SetFloat("_NormalStrength", 1f);
                mat.SetTexture("_NormalTex", normTex);
                return mat;
            }
            return mat;
        }
        public static Material FindMaterial(string name)
        {
            Material[] objs = Resources.FindObjectsOfTypeAll<Material>() as Material[];
            for (int i = 0; i < objs.Length; i++)
            {
                if (objs[i].hideFlags == HideFlags.None)
                {
                    if (objs[i].name == name)
                    {
                        return objs[i];
                    }
                }
            }
            return null;
        }
    }
}
