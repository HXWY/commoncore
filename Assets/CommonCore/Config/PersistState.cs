﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace CommonCore.Config
{

    public class PersistState
    {
        private static readonly string Path = CCParams.PersistentDataPath + "/persist.json";

        public static PersistState Instance { get; private set; }

        internal static void Load()
        {
            Instance = CCBaseUtil.LoadExternalJson<PersistState>(Path);
            if (Instance == null)
                Instance = new PersistState();
        }

        internal static void Save()
        {
            CCBaseUtil.SaveExternalJson(Path, Instance);
        }

        //set defaults in constructor
        [JsonConstructor]
        private PersistState()
        {
            ExtraStore = new Dictionary<string, object>();
        }

        //actual persist data here (TODO)
        public Dictionary<string, System.Object> ExtraStore { get; private set; }
    }
}