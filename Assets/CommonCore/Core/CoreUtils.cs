﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace CommonCore
{
    /*
     * CommonCore Core Utilities class
     * Includes common/general utility functions that don't fit within a module
     */
    public static class CoreUtils
    {
        internal static CCResourceManager ResourceManager {get; set;}

        /// <summary>
        /// Load a resource, respecting virtual/redirected paths
        /// </summary>
        public static T LoadResource<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.GetResource<T>(path);
        }

        /// <summary>
        /// Load resources from a folder, respecting virtual/redirected paths
        /// </summary>
        public static T[] LoadResources<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.GetResources<T>(path);
        }

        /// <summary>
        /// Check if a resource exists, respecting virtual/redirected paths 
        /// </summary>
        public static bool CheckResource<T>(string path) where T: UnityEngine.Object
        {
            return ResourceManager.ContainsResource<T>(path);
        }
        
        public static T LoadExternalJson<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            string text = File.ReadAllText(path);
            return LoadJson<T>(text);
        }

        public static T LoadJson<T>(string text)
        {
            return JsonConvert.DeserializeObject<T>(text, new JsonSerializerSettings
            {
                Converters = CCJsonConverters.Defaults.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void SaveExternalJson(string path, System.Object obj)
        {
            string json = SaveJson(obj);
            File.WriteAllText(path, json);
        }

        public static string SaveJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = CCJsonConverters.Defaults.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        /// <summary>
        /// Gets a list of scenes (by name) in the game
        /// </summary>
        /// <returns>A list of scenes in the game</returns>
        public static string[] GetSceneList() //TODO we'll probably move this into some kind of CommonCore.SceneManagement
        {
            int sceneCount = SceneManager.sceneCountInBuildSettings;
            var scenes = new List<string>(sceneCount);
            for (int i = 0; i < sceneCount; i++)
            {
                try
                {
                    scenes.Add(SceneUtility.GetScenePathByBuildIndex(i));
                }
                catch (Exception e)
                {
                    //ignore it, we've gone over or some stupid bullshit
                }

            }

            return scenes.ToArray();

        }

        /*
         * Converts a string to an int or a float with correct type
         * (limitation: literally int or float, no long or double etc)
         */
        public static object StringToNumericAuto(string input)
        {
            //check if it is integer first
            int iResult;
            bool isInteger = int.TryParse(input, out iResult);
            if (isInteger)
                return iResult;

            //then check if it could be decimal
            float fResult;
            bool isFloat = float.TryParse(input, out fResult);
            if (isFloat)
                return fResult;

            //else return what we started with
            return input;
        }

        /*
         * Converts a string to an int or a float with correct type
         * (double precision version: long or double)
         */
        public static object StringToNumericAutoDouble(string input)
        {
            //check if it is integer first
            long iResult;
            bool isInteger = long.TryParse(input, out iResult);
            if (isInteger)
                return iResult;

            //then check if it could be decimal
            double fResult;
            bool isFloat = double.TryParse(input, out fResult);
            if (isFloat)
                return fResult;

            //else return what we started with
            return input;
        }

        public static bool IsNumericType(Type type)
        {
            switch (Type.GetTypeCode(type))
            {
                case TypeCode.Byte:
                case TypeCode.SByte:
                case TypeCode.UInt16:
                case TypeCode.UInt32:
                case TypeCode.UInt64:
                case TypeCode.Int16:
                case TypeCode.Int32:
                case TypeCode.Int64:
                case TypeCode.Decimal:
                case TypeCode.Double:
                case TypeCode.Single:
                    return true;
                default:
                    return false;
            }
        }

        public static T Clamp<T>(this T val, T min, T max) where T : IComparable<T>
        {
            if (val.CompareTo(min) < 0) return min;
            else if (val.CompareTo(max) > 0) return max;
            else return val;
        }

        private static Transform WorldRoot;
        public static Transform GetWorldRoot() //TODO really ought to move this
        {
            if (WorldRoot == null)
            {
                GameObject rootGo = GameObject.Find("WorldRoot");
                if (rootGo == null)
                    return null;
                WorldRoot = rootGo.transform;
            }
            return WorldRoot;
        }

        private static Transform UIRoot;
        public static Transform GetUIRoot()
        {
            if(UIRoot == null)
            {
                GameObject rootGo = GameObject.Find("UIRoot");
                if (rootGo == null)
                    rootGo = new GameObject("UIRoot");
                UIRoot = rootGo.transform;
            }
            return UIRoot;
        }

        public static void DestroyAllChildren(Transform root)
        {
            foreach(Transform t in root)
            {
                GameObject.Destroy(t.gameObject);
            }
        }

        public static Vector2 ToFlatVec(this Vector3 vec3)
        {
            return new Vector2(vec3.x, vec3.z);
        }

        public static Vector3 ToSpaceVec(this Vector2 vec2)
        {
            return new Vector3(vec2.x, 0, vec2.y);
        }

        public static Vector3 GetFlatVectorToTarget(Vector3 pos, Vector3 target)
        {
            Vector3 dir = target - pos;
            return new Vector3(dir.x, 0, dir.z);
        }

        public static Vector2 GetRandomVector(Vector2 center, Vector2 extents)
        {
            return new Vector2(
                UnityEngine.Random.Range(-extents.x, extents.x) + center.x,
                UnityEngine.Random.Range(-extents.y, extents.y) + center.y
                );
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
        {
            return GetOrDefault(dictionary, key, default(TValue));
        }

        public static TValue GetOrDefault<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key, TValue def)
        {
            TValue result;
            if (dictionary.TryGetValue(key, out result))
                return result;

            return def;
        }

        public static object Ref(this object obj)
        {
            if (obj is UnityEngine.Object)
                return (UnityEngine.Object)obj == null ? null : obj;
            else
                return obj;
        }

        public static T Ref<T>(this T obj) where T : UnityEngine.Object
        {
            return obj == null ? null : obj;
        }

        public static string ToNiceString(this IEnumerable collection)
        {
            StringBuilder sb = new StringBuilder(256);
            sb.Append("[");

            IEnumerator enumerator = collection.GetEnumerator();
            bool eHasNext = enumerator.MoveNext();
            while(eHasNext)
            {
                sb.Append(enumerator.Current.ToString());

                eHasNext = enumerator.MoveNext();
                if (eHasNext)
                    sb.Append(", ");
            }
            sb.Append("]");

            return sb.ToString();
        }

    }
}