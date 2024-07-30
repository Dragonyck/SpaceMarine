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

namespace SpaceMarine
{
    class Assets
    {
        public static AssetBundle MainAssetBundle = null;
        public static T Load<T>(string name) where T : UnityEngine.Object
        {
            return MainAssetBundle.LoadAsset<T>(name);
        }
        public static void PopulateAssets()
        {
            var assembly = Assembly.GetExecutingAssembly();
            if (MainAssetBundle == null)
            {
                using (var assetStream = assembly.GetManifestResourceStream(MainPlugin.MODNAME + ".AssetBundle." + MainPlugin.MODNAME.ToLower() + "assets"))
                {
                    MainAssetBundle = AssetBundle.LoadFromStream(assetStream);
                }
            }
            using (var bankStream = assembly.GetManifestResourceStream(MainPlugin.MODNAME + "." + MainPlugin.MODNAME + ".bnk"))
            {
                var bytes = new byte[bankStream.Length];
                bankStream.Read(bytes, 0, bytes.Length);
                SoundAPI.SoundBanks.Add(bytes);
            }
        }
    }
}
