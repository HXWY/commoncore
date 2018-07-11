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

    /*
     * Just your basic Semantic Version struct
     */
    public struct SemanticVersion
    {
        public readonly int Major;
        public readonly int Minor;
        public readonly int Patch;

        public SemanticVersion(int major, int minor, int patch)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
        }

        public override string ToString()
        {
            return string.Format("{0}.{1}.{2}", Major, Minor, Patch);
        }
    }

    public enum DataLoadPolicy
    {
        Auto, OnDemand, OnStart, Cached
    }
}