﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore
{
    /*
     * CommonCore Parameters class
     * Includes common parameters, version info, etc
     */
    public static class CCParams
    {
        //*****version info
        public static readonly SemanticVersion VersionCode = new SemanticVersion(1,0,0); //1.0.0
        public const string VersionName = "Arroyo"; //start with A, locations from RPGs
        public static string UnityVersion
        {
            get
            {
                return Application.unityVersion;
            }
        }

        //*****basic config settings
        public const bool AutoInit = true;
        public const bool AutoloadModules = true;
        private const DataLoadPolicy LoadData = DataLoadPolicy.OnStart;

        //*****additional config settings
        public const bool UseVerboseLogging = true;

        //*****game config settings
        public const string InitialScene = "World_Ext_TestIsland";
        public const bool UseCustomLeveling = false;
        public const PlayerViewType DefaultPlayerView = PlayerViewType.PreferFirst;

        //*****automatic environment params
        public static bool IsDebug
        {
            get
            {
                return Debug.isDebugBuild; //may change to PDC (#define DEVELOPMENT_BUILD)
            }
        }

        public static bool IsEditor
        {
            get
            {
                #if UNITY_EDITOR
                return true;
                #else
                return false;
                #endif
            }
        }
        public static string PersistentDataPath
        {
            get
            {
                return Application.persistentDataPath;
            }
        }

        public static string SavePath
        {
            get
            {
                return PersistentDataPath + "/saves";
            }
        }

        public static DataLoadPolicy LoadPolicy
        {
            get
            {
                if (LoadData == DataLoadPolicy.Auto)
                {
                    #if UNITY_EDITOR
                    return DataLoadPolicy.OnDemand;
                    #else
                    return DataLoadPolicy.OnStart;
                    #endif
                }
                else
                    return LoadData;
            }
        }
    }


}