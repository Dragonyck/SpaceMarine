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

[module: UnverifiableCode]
[assembly: SecurityPermission(SecurityAction.RequestMinimum, SkipVerification = true)]
namespace SpaceMarine
{
    [BepInDependency(R2API.ContentManagement.R2APIContentManager.PluginGUID)]
    [BepInDependency(R2API.LanguageAPI.PluginGUID)]
    [BepInDependency(R2API.LoadoutAPI.PluginGUID)]
    [BepInDependency(R2API.Networking.NetworkingAPI.PluginGUID)]
    [BepInDependency(R2API.PrefabAPI.PluginGUID)]
    [BepInDependency(R2API.SoundAPI.PluginGUID)]
    [BepInDependency(R2API.RecalculateStatsAPI.PluginGUID)]
    [BepInPlugin(MODUID, MODNAME, VERSION)]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.EveryoneNeedSameModVersion)]
    public class MainPlugin : BaseUnityPlugin
    {
        public const string MODUID = "com.Dragonyck.SpaceMarine";
        public const string MODNAME = "SpaceMarine";
        public const string VERSION = "1.0.0";
        public const string SURVIVORNAME = "SpaceMarine";
        public const string SURVIVORNAMEKEY = "SPACEMARINE";
        public static GameObject characterPrefab;
        public static readonly Color characterColor = new Color32(11, 125, 172, 255);

        private void Awake()
        {
            //On.RoR2.Networking.NetworkManagerSystemSteam.OnClientConnect += (self, user, t) => { };

            Assets.PopulateAssets();
            Prefabs.CreatePrefabs();
            CreatePrefab();
            RegisterStates();
            RegisterCharacter();
            Hook.Hooks();
        }
        internal static void CreatePrefab()
        {
            var baseBody = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Commando/CommandoBody.prefab").WaitForCompletion();

            characterPrefab = PrefabAPI.InstantiateClone(baseBody, SURVIVORNAME + "Body", true);
            characterPrefab.GetComponent<NetworkIdentity>().localPlayerAuthority = true;
            Destroy(characterPrefab.transform.Find("ModelBase").gameObject);
            Destroy(characterPrefab.transform.Find("CameraPivot").gameObject);
            Destroy(characterPrefab.transform.Find("AimOrigin").gameObject);

            GameObject model = Assets.MainAssetBundle.LoadAsset<GameObject>("marineMdl");
            model.AddComponent<AnimationEvents>().soundCenter = model;

            GameObject ModelBase = new GameObject("ModelBase");
            ModelBase.transform.parent = characterPrefab.transform;
            ModelBase.transform.localPosition = new Vector3(0f, -0.94f, 0f);
            ModelBase.transform.localRotation = Quaternion.identity;
            ModelBase.transform.localScale = new Vector3(1f, 1f, 1f);

            GameObject gameObject3 = new GameObject("AimOrigin");
            gameObject3.transform.parent = ModelBase.transform;
            gameObject3.transform.localPosition = new Vector3(0f, 1.4f, 0f);
            gameObject3.transform.localRotation = Quaternion.identity;
            gameObject3.transform.localScale = Vector3.one;

            Transform transform = model.transform;
            transform.parent = ModelBase.transform;
            transform.localPosition = Vector3.zero;
            transform.localScale = Vector3.one;
            transform.localRotation = Quaternion.identity;

            CharacterDirection characterDirection = characterPrefab.GetComponent<CharacterDirection>();
            characterDirection.targetTransform = ModelBase.transform;
            characterDirection.modelAnimator = model.GetComponentInChildren<Animator>();
            characterDirection.turnSpeed = 720f;

            CharacterBody bodyComponent = characterPrefab.GetComponent<CharacterBody>();
            bodyComponent.name = SURVIVORNAME + "Body";
            bodyComponent.baseNameToken = SURVIVORNAMEKEY + "_NAME";
            bodyComponent.subtitleNameToken = SURVIVORNAMEKEY + "_SUBTITLE";
            bodyComponent.bodyFlags = CharacterBody.BodyFlags.ImmuneToExecutes;
            bodyComponent.rootMotionInMainState = false;
            bodyComponent.mainRootSpeed = 0;
            bodyComponent.baseMaxHealth = 110;
            bodyComponent.levelMaxHealth = 50;
            bodyComponent.baseRegen = 1.5f;
            bodyComponent.levelRegen = 0.2f;
            bodyComponent.baseMaxShield = 0;
            bodyComponent.levelMaxShield = 0;
            bodyComponent.baseMoveSpeed = 7;
            bodyComponent.levelMoveSpeed = 0;
            bodyComponent.baseAcceleration = 110;
            bodyComponent.baseJumpPower = 15;
            bodyComponent.levelJumpPower = 0;
            bodyComponent.baseDamage = 12;
            bodyComponent.levelDamage = 2.4f;
            bodyComponent.baseAttackSpeed = 1;
            bodyComponent.levelAttackSpeed = 0;
            bodyComponent.baseCrit = 1;
            bodyComponent.levelCrit = 0;
            bodyComponent.baseArmor = 0;
            bodyComponent.levelArmor = 1;
            bodyComponent.baseJumpCount = 1;
            bodyComponent.sprintingSpeedMultiplier = 1.45f;
            bodyComponent.wasLucky = false;
            bodyComponent.hideCrosshair = false;
            bodyComponent.aimOriginTransform = gameObject3.transform;
            bodyComponent.hullClassification = HullClassification.Human;
            bodyComponent.portraitIcon = Assets.MainAssetBundle.LoadAsset<Sprite>("portrait").texture;
            bodyComponent._defaultCrosshairPrefab = Prefabs.crosshair;
            bodyComponent.isChampion = false;
            bodyComponent.currentVehicle = null;
            bodyComponent.skinIndex = 0U;
            bodyComponent.bodyColor = characterColor;

            HealthComponent healthComponent = characterPrefab.GetComponent<HealthComponent>();
            healthComponent.health = bodyComponent.baseMaxHealth;
            healthComponent.shield = 0f;
            healthComponent.barrier = 0f;

            CharacterMotor characterMotor = characterPrefab.GetComponent<CharacterMotor>();
            characterMotor.walkSpeedPenaltyCoefficient = 1f;
            characterMotor.characterDirection = characterDirection;
            characterMotor.muteWalkMotion = false;
            characterMotor.mass = 160f;
            characterMotor.airControl = 0.25f;
            characterMotor.disableAirControlUntilCollision = false;
            characterMotor.generateParametersOnAwake = true;

            InputBankTest inputBankTest = characterPrefab.GetComponent<InputBankTest>();
            inputBankTest.moveVector = Vector3.zero;

            CameraTargetParams cameraTargetParams = characterPrefab.GetComponent<CameraTargetParams>();
            cameraTargetParams.cameraParams = baseBody.GetComponent<CameraTargetParams>().cameraParams;
            cameraTargetParams.cameraPivotTransform = null;
            cameraTargetParams.recoil = Vector2.zero;
            cameraTargetParams.dontRaycastToPivot = false;

            ModelLocator modelLocator = characterPrefab.GetComponent<ModelLocator>();
            modelLocator.modelTransform = transform;
            modelLocator.modelBaseTransform = ModelBase.transform;
            modelLocator.dontReleaseModelOnDeath = false;
            modelLocator.autoUpdateModelTransform = true;
            modelLocator.dontDetatchFromParent = false;
            modelLocator.noCorpse = false;
            modelLocator.normalizeToFloor = false;
            modelLocator.preserveModel = false;

            ChildLocator childLocator = model.GetComponent<ChildLocator>();

            CharacterModel characterModel = model.AddComponent<CharacterModel>();

            SkinnedMeshRenderer[] renderers = model.GetComponentsInChildren<SkinnedMeshRenderer>();
            List<CharacterModel.RendererInfo> rendererInfoList = new List<CharacterModel.RendererInfo>();
            for (int i = 0; i < renderers.Length; i++)
            {
                var r = renderers[i];
                r.material = Utils.InstantiateMaterial(r.material.mainTexture);
                rendererInfoList.Add(new CharacterModel.RendererInfo()
                {
                    renderer = r,
                    defaultMaterial = r.material,
                    defaultShadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On,
                    ignoreOverlays = false
                });
            }
            CharacterModel.RendererInfo[] rendererInfos = rendererInfoList.ToArray();
            characterModel.body = bodyComponent;
            characterModel.baseRendererInfos = rendererInfos;
            characterModel.autoPopulateLightInfos = true;
            characterModel.invisibilityCount = 0;
            characterModel.temporaryOverlays = new List<TemporaryOverlay>();
            characterModel.mainSkinnedMeshRenderer = renderers[0];

            LanguageAPI.Add(SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME", "Default");

            var modelSkinController = model.AddComponent<ModelSkinController>();
            modelSkinController.skins = new SkinDef[]
            {
                LoadoutAPI.CreateNewSkinDef(Utils.CreateNewSkinDefInfo(model, SURVIVORNAMEKEY + "BODY_DEFAULT_SKIN_NAME", rendererInfos))
            };

            Collider[] colliders = model.GetComponentsInChildren<Collider>();
            HurtBoxGroup hurtBoxGroup = model.AddComponent<HurtBoxGroup>();
            List<HurtBox> hurtboxes = new List<HurtBox>();
            foreach (Collider c in colliders)
            {
                HurtBox hurtbox = c.gameObject.AddComponent<HurtBox>();
                hurtbox.gameObject.layer = LayerIndex.entityPrecise.intVal;
                hurtbox.healthComponent = healthComponent;
                hurtbox.isBullseye = true;
                hurtbox.damageModifier = HurtBox.DamageModifier.Normal;
                hurtbox.hurtBoxGroup = hurtBoxGroup;
                hurtbox.indexInGroup = 0;

                hurtBoxGroup.mainHurtBox = hurtbox;
                hurtBoxGroup.bullseyeCount = 1;

                hurtboxes.Add(hurtbox);
            }
            hurtBoxGroup.hurtBoxes = hurtboxes.ToArray();

            KinematicCharacterMotor kinematicCharacterMotor = characterPrefab.GetComponent<KinematicCharacterMotor>();
            kinematicCharacterMotor.CharacterController = characterMotor;

            var ragdollMaterial = Addressables.LoadAssetAsync<PhysicMaterial>("RoR2/Base/Common/physmatRagdoll.physicMaterial").WaitForCompletion();
            List<Transform> transforms = new List<Transform>();
            List<string> boneNames = new List<string>()
            {
                "BASE",
                "DEF_hips",
                "DEF_hipLegstretch.L",
                "DEF_leg_upper.L",
                "DEF_leg_lower.L",
                "DEF_foot.L",
                "DEF_hipRegstretch.R",
                "DEF_leg_upper.R",
                "DEF_leg_lower.R",
                "DEF_foot.R",
                "MECH_spine",
                "MECH_spine_1",
                "rig_spine_2",
                "spine_top",
                "DEF_neck",
                "DEF_head",
                "TWEAK_spine_2",
                "DEF_spine_2",
                "DEF_collar_L",
                "DEF_arm_upper.L",
                "DEF_arm_lower.L",
                //"DEF_hand.L",
                "DEF_collar_R",
                "DEF_arm_upper.R",
                "DEF_arm_lower.R",
                //"DEF_hand.R",
                "DEF_wpn_bolter_grip"
            };
            foreach (Transform t in model.GetComponentsInChildren<Transform>(true))
            {
                string name = t.name;
                if (boneNames.Contains(name))
                {
                    transforms.Add(t);
                    GameObject g = t.gameObject;
                    g.layer = LayerIndex.ragdoll.intVal;
                    if (!g.GetComponent<Rigidbody>())
                    {
                        g.AddComponent<Rigidbody>();
                    }
                    var bonecollider = g.AddComponent<CapsuleCollider>();
                    bonecollider.radius = 0.4f;
                    bonecollider.height = 3.75f;
                    bonecollider.material = ragdollMaterial;
                    bonecollider.sharedMaterial = ragdollMaterial;
                    Rigidbody parentRigidBody = t.parent.GetComponent<Rigidbody>();
                    if (parentRigidBody)
                    {
                        var joint = g.AddComponent<CharacterJoint>();
                        joint.autoConfigureConnectedAnchor = true;
                        joint.enablePreprocessing = true;
                        joint.connectedBody = parentRigidBody;
                    }
                }
            }
            var ragdoll = model.AddComponent<RagdollController>();
            ragdoll.bones = transforms.ToArray();

            Utils.CreateHitbox("Dash", model.transform, Vector3.one * 8, new Vector3(0, 1.2f, 2));

            characterPrefab.GetComponent<Interactor>().maxInteractionDistance = 3f;
            characterPrefab.GetComponent<InteractionDriver>().highlightInteractor = true;

            SfxLocator sfxLocator = characterPrefab.GetComponent<SfxLocator>();
            sfxLocator.deathSound = "Play_ui_player_death";
            sfxLocator.barkSound = "";
            sfxLocator.openSound = "";
            sfxLocator.landingSound = "Play_SpaceMarine_Impact";
            sfxLocator.fallDamageSound = "Play_SpaceMarine_Impact_Heavy";
            sfxLocator.aliveLoopStart = "";
            sfxLocator.aliveLoopStop = "";

            characterPrefab.GetComponent<Rigidbody>().mass = characterMotor.mass;

            AimAnimator aimAnimator = model.AddComponent<AimAnimator>();
            aimAnimator.inputBank = inputBankTest;
            aimAnimator.directionComponent = characterDirection;
            aimAnimator.pitchRangeMin = -60f;
            aimAnimator.pitchRangeMax = 60;
            aimAnimator.yawRangeMin = -90;
            aimAnimator.yawRangeMax = 90;
            aimAnimator.pitchGiveupRange = 30f;
            aimAnimator.yawGiveupRange = 10f;
            aimAnimator.giveupDuration = 3;

            FootstepHandler footstepHandler = model.AddComponent<FootstepHandler>();
            footstepHandler.baseFootstepString = "Play_SpaceMarine_Footsteps";
            footstepHandler.sprintFootstepOverrideString = "";
            footstepHandler.enableFootstepDust = true;
            footstepHandler.footstepDustPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/GenericFootstepDust.prefab").WaitForCompletion();

            EntityStateMachine mainStateMachine = bodyComponent.GetComponent<EntityStateMachine>();
            mainStateMachine.mainStateType = new SerializableEntityStateType(typeof(CharacterMain));

            CharacterDeathBehavior characterDeathBehavior = characterPrefab.GetComponent<CharacterDeathBehavior>();
            characterDeathBehavior.deathStateMachine = characterPrefab.GetComponent<EntityStateMachine>();
            characterDeathBehavior.deathState = new SerializableEntityStateType(typeof(EntityStates.Commando.DeathState));

            Utils.NewStateMachine<PassiveBarrier>(characterPrefab, "Passive");

            NetworkStateMachine networkStateMachine = bodyComponent.GetComponent<NetworkStateMachine>();
            networkStateMachine.stateMachines = bodyComponent.GetComponents<EntityStateMachine>();

            ContentAddition.AddBody(characterPrefab);
        }
        private void RegisterCharacter()
        {
            string desc = " <style=cSub>\r\n\r\n" +
                "< ! > \r\n\r\n" +
                "< ! > \r\n\r\n" +
                "< ! > \r\n\r\n" +
                "< ! > \r\n\r\n";

            string outro = "..and so he left.";
            string fail = "..and so he vanished.";
            string lore = "";

            LanguageAPI.Add(SURVIVORNAMEKEY + "_NAME", SURVIVORNAME);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_DESCRIPTION", desc);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SUBTITLE", "");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_OUTRO", outro);
            LanguageAPI.Add(SURVIVORNAMEKEY + "_FAIL", fail);
            //LanguageAPI.Add(SURVIVORNAMEKEY + "_LORE", lore);

            var survivorDef = ScriptableObject.CreateInstance<SurvivorDef>();
            {
                survivorDef.cachedName = SURVIVORNAMEKEY + "_NAME";
                survivorDef.unlockableDef = null;
                survivorDef.descriptionToken = SURVIVORNAMEKEY + "_DESCRIPTION";
                survivorDef.primaryColor = characterColor;
                survivorDef.bodyPrefab = characterPrefab;
                survivorDef.displayPrefab = Utils.NewDisplayModel(characterPrefab.GetComponent<ModelLocator>().modelBaseTransform.gameObject, SURVIVORNAME + "Display");
                survivorDef.outroFlavorToken = SURVIVORNAMEKEY + "_OUTRO";
                survivorDef.desiredSortPosition = 22;
                survivorDef.mainEndingEscapeFailureFlavorToken = SURVIVORNAMEKEY + "_FAIL";
            };

            ContentAddition.AddSurvivorDef(survivorDef);

            SkillSetup();

            var characterMaster = PrefabAPI.InstantiateClone(Prefabs.Load<GameObject>("RoR2/Base/Commando/CommandoMonsterMaster.prefab"), SURVIVORNAME + "Master", true);

            ContentAddition.AddMaster(characterMaster);

            CharacterMaster component = characterMaster.GetComponent<CharacterMaster>();
            component.bodyPrefab = characterPrefab;
        }
        void RegisterStates()
        {
            bool hmm;
            ContentAddition.AddEntityState<Primary>(out hmm);
            ContentAddition.AddEntityState<Secondary>(out hmm);
            ContentAddition.AddEntityState<Utility>(out hmm);
            ContentAddition.AddEntityState<Special>(out hmm);
            ContentAddition.AddEntityState<CharacterMain>(out hmm);
            ContentAddition.AddEntityState<PassiveBarrier>(out hmm);
            ContentAddition.AddEntityState<MeleeSkillState>(out hmm);
            ContentAddition.AddEntityState<BasicMeleeSkillState>(out hmm);
        }
        void SkillSetup()
        {
            foreach (GenericSkill obj in characterPrefab.GetComponentsInChildren<GenericSkill>())
            {
                BaseUnityPlugin.DestroyImmediate(obj);
            }
            PassiveSetup();
            PrimarySetup();
            SecondarySetup();
            UtilitySetup();
            SpecialSetup();

        }
        void PassiveSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();

            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_NAME", "Iron Halo");
            LanguageAPI.Add(SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION", "Slowly generate <style=cIsHealing>barrier</style> passively. Amount generated is a percentage of your <style=cIsHealth>health</style> equal to <style=cIsDamage>double</style> your current level." +
                "Gain an additional style=cIsHealth>15 max health</style> and <style=cIsDamage>1 armor</style> per level.");

            component.passiveSkill.enabled = true;
            component.passiveSkill.skillNameToken = SURVIVORNAMEKEY + "_PASSIVE_NAME";
            component.passiveSkill.skillDescriptionToken = SURVIVORNAMEKEY + "_PASSIVE_DESCRIPTION";
            component.passiveSkill.icon = Assets.MainAssetBundle.LoadAsset<Sprite>("passive");
        }
        void PrimarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1", "Bolt Rifle");//<style=cIsDamage></style> <style=cIsUtility></style> <style=cIsHealth></style> <style=cIsHealing></style> 
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M1_DESCRIPTION", "Fire an explosive slug that hits for <style=cIsDamage>150% damage</style>, while the blast hits for <style=cIsDamage>50% damage</style> in a small AoE.");

            var SkillDef = Utils.NewSkillDef<SkillDef>(typeof(Primary), "Weapon", 0, 0f, true, false, true, InterruptPriority.Any, true, false, true, 0, 0, 0, 
                Assets.MainAssetBundle.LoadAsset<Sprite>("primary"), SURVIVORNAMEKEY + "_M1_DESCRIPTION", SURVIVORNAMEKEY + "_M1");
            component.primary = Utils.NewGenericSkill(characterPrefab, SkillDef);

        }
        void SecondarySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2", "Frag Grenades");//<style=cIsDamage></style> <style=cIsUtility></style> <style=cIsHealth></style> <style=cIsHealing></style> 
            LanguageAPI.Add(SURVIVORNAMEKEY + "_M2_DESCRIPTION", "Throw a grenade with a large blast radius dealing <style=cIsDamage></style>250% damage</style>.");

            var SkillDef = Utils.NewSkillDef<SkillDef>(typeof(Secondary), "Weapon", 1, 5f, true, false, false, InterruptPriority.Skill, true, true, false, 1, 1, 1,
                Assets.MainAssetBundle.LoadAsset<Sprite>("secondary"), SURVIVORNAMEKEY + "_M2_DESCRIPTION", SURVIVORNAMEKEY + "_M2", new string[] { "KEYWORD_HEAVY" });
            component.secondary = Utils.NewGenericSkill(characterPrefab, SkillDef);

        }
        void UtilitySetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL", "Charge");//<style=cIsDamage></style> <style=cIsUtility></style> <style=cIsHealth></style> <style=cIsHealing></style> 
            LanguageAPI.Add(SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", "<style=cIsUtility>Heavy</style>. Charge forward gaining <style=cIsUtility>200% movement speed</style>. Deals <style=cIsDamage>250% damage</style> to enemies. Hitting a large enemy stops the charge and deals <style=cIsDamage>1000% damage</style>.");

            var SkillDef = Utils.NewSkillDef<SkillDef>(typeof(Utility), "Body", 1, 5f, true, false, false, InterruptPriority.Any, true, true, false, 1, 1, 1,
                Assets.MainAssetBundle.LoadAsset<Sprite>("utility"), SURVIVORNAMEKEY + "_UTIL_DESCRIPTION", SURVIVORNAMEKEY + "_UTIL");
            component.utility = Utils.NewGenericSkill(characterPrefab, SkillDef);

        }
        void SpecialSetup()
        {
            SkillLocator component = characterPrefab.GetComponent<SkillLocator>();
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC", "Iron Resolve");//<style=cIsDamage></style> <style=cIsUtility></style> <style=cIsHealth></style> <style=cIsHealing></style> 
            LanguageAPI.Add(SURVIVORNAMEKEY + "_SPEC_DESCRIPTION", "Become <style=cIsDamage>immune</style> to knockback effects, and create a regeneration field around yourself that <style=cIsHealing>heals 10%</style> of your <style=cIsHealing>max health</style> per second for <style=cIsUtility>6s + 0.1s</style> per level.");

            var SkillDef = Utils.NewSkillDef<SkillDef>(typeof(Special), "Slide", 1, 30f, true, false, false, InterruptPriority.Any, true, true, false, 1, 1, 1,
                Assets.MainAssetBundle.LoadAsset<Sprite>("special"), SURVIVORNAMEKEY + "_SPEC_DESCRIPTION", SURVIVORNAMEKEY + "_SPEC");
            component.special = Utils.NewGenericSkill(characterPrefab, SkillDef);

        }
    }
}
