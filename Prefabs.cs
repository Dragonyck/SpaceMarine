﻿using System;
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
        internal static T Load<T>(string path)
        {
            return Addressables.LoadAssetAsync<T>(path).WaitForCompletion();
        }
        internal static void CreatePrefabs()
        {
            //prefab = PrefabAPI.InstantiateClone(Load<GameObject>("path"), "prefabname", false);
        }
    }
}
