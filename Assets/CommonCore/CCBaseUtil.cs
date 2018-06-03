﻿using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using WanzyeeStudio;

namespace CommonCore
{
    /*
     * CommonCore Base Utilities class
     * Includes common/general utility functions that don't fit within a module
     */
    public static class CCBaseUtil
    {
        //this seems absolutely pointless but will make sense when eXPostFacto (mod support) is added
        public static T LoadResource<T>(string path) where T: UnityEngine.Object
        {
            return Resources.Load<T>(path);
        }

        
        public static T LoadExternalJson<T>(string path)
        {
            if (!File.Exists(path))
            {
                return default(T);
            }
            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path), new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        public static void SaveExternalJson(string path, System.Object obj)
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented, new JsonSerializerSettings
            {
                Converters = JsonNetUtility.defaultSettings.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
            File.WriteAllText(path, json);
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

            //else return null
            return null;
        }
    }
}