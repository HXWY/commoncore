﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace CommonCore.State
{
    //DO NOT EDIT THIS FILE

    /// <summary>
    /// Represents the entire state of the game
    /// </summary>
    public partial class GameState
    {
        private static GameState instance;

        public static GameState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new GameState();
                }
                return instance;
            }
        }

        /// <summary>
        /// Purges the current game state and recreates it
        /// </summary>
        public static void Reset()
        {
            instance = new GameState();
        }

        /// <summary>
        /// Saves the current game state to file
        /// </summary>
        public static void SerializeToFile(string path)
        {
            string data = Serialize();
            File.WriteAllText(path, data);
        }

        /// <summary>
        /// Serializes the current game state to a string
        /// </summary>
        public static string Serialize()
        {
            return JsonConvert.SerializeObject(Instance,
                Formatting.Indented,
                new JsonSerializerSettings
                {
                    Converters = CCJsonConverters.Defaults.Converters,
                    TypeNameHandling = TypeNameHandling.Auto
                });
        }

        /// <summary>
        /// Loads a file into the current game state
        /// </summary>
        public static void DeserializeFromFile(string path)
        {
            Deserialize(File.ReadAllText(path));
        }

        /// <summary>
        /// Deserializes a string and replaces the current game state
        /// </summary>
        public static void Deserialize(string data)
        {
            instance = JsonConvert.DeserializeObject<GameState>(data,
            new JsonSerializerSettings
            {
                Converters = CCJsonConverters.Defaults.Converters,
                TypeNameHandling = TypeNameHandling.Auto
            });
        }

        /// <summary>
        /// Loads initial values into the current game state
        /// </summary>
        public static void LoadInitial()
        {
            instance.Init();
        }

        /// <summary>
        /// Loads initial values into the current game state
        /// </summary>
        private void Init()
        {
            //we actually use reflection to get all "decorated" methods and run them

            var initMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(InitAttribute), false).Length > 0)
                .ToList();

            if (initMethods.Count == 0)
                return; //abort if no init methods available
            else if (initMethods.Count == 1)
                initMethods[0].Invoke(this, null); //if we only have one, it's easy
            else
            {
                initMethods.Sort((m1, m2) => ((InitAttribute)m2.GetCustomAttributes(typeof(InitAttribute), false)[0]).Priority
                .CompareTo(((InitAttribute)m1.GetCustomAttributes(typeof(InitAttribute), false)[0]).Priority));
                foreach (var m in initMethods)
                    m.Invoke(this, null);
            }

            InitialLoaded = true;
        }

        /// <summary>
        /// Runs handling after GameState is deserialized 
        /// </summary>
        [OnDeserialized]
        private void HandleOnDeserialized(StreamingContext context)
        {
            var loadMethods = GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => m.GetCustomAttributes(typeof(AfterLoadAttribute), false).Length > 0)
                .ToList();

            if (loadMethods.Count == 0)
                return;
            else if (loadMethods.Count == 1)
                loadMethods[0].Invoke(this, null);
            else
            {
                loadMethods.Sort((m1, m2) => ((AfterLoadAttribute)m2.GetCustomAttributes(typeof(AfterLoadAttribute), false)[0]).Priority
                .CompareTo(((AfterLoadAttribute)m1.GetCustomAttributes(typeof(AfterLoadAttribute), false)[0]).Priority));
                foreach (var m in loadMethods)
                    m.Invoke(this, null);
            }
        }

        //basic game data to be shared across games

        /// <summary>
        /// Generic data store for global game state
        /// </summary>
        public Dictionary<string, object> GlobalDataState { get; private set; } = new Dictionary<string, object>();

        /// <summary>
        /// Generic data store for scene-local game state
        /// </summary>
        public Dictionary<string, Dictionary<string, object>> LocalDataState { get; private set; } = new Dictionary<string, Dictionary<string, object>>();

        /// <summary>
        /// The current scene we are in
        /// </summary>
        public string CurrentScene { get; set; }

        /// <summary>
        /// If we are allowed to save at this point
        /// </summary>
        public bool SaveLocked { get; set; }

        /// <summary>
        /// If we are allowed to open the menu at this point
        /// </summary>
        public bool MenuLocked { get; set; }

        /// <summary>
        /// Whether we have loaded initial data already
        /// </summary>
        [JsonProperty]
        public bool InitialLoaded { get; private set; }

        [JsonProperty]
        private long CurrentUID;

        /// <summary>
        /// The next available unique ID
        /// </summary>
        /// <remarks>Accessing this will increment the backing counter (<see cref="CurrentUID"/>)</remarks>
        [JsonIgnore]
        public long NextUID { get { return ++CurrentUID; } }

        /// <summary>
        /// Decorate methods with this atrribute to have them run on GameState initialization. Higher priority is sooner.
        /// </summary>
        public class InitAttribute : Attribute
        {
            /// <summary>
            /// Higher priority init methods are run first
            /// </summary>
            public int Priority { get; private set; } = 0;

            public InitAttribute()
            {

            }

            /// <param name="priority">When to run this init method; higher is sooner</param>
            public InitAttribute(int priority)
            {
                Priority = priority;
            }
        }

        /// <summary>
        /// Decorate methods with this attribute to have them run after deserializing GameState. Higher priority is sooner.
        /// </summary>
        public class AfterLoadAttribute : Attribute
        {
            /// <summary>
            /// Higher priority load methods are run first
            /// </summary>
            public int Priority { get; private set; } = 0;

            public AfterLoadAttribute()
            {

            }

            /// <param name="priority">When to run this load method; higher is sooner</param>
            public AfterLoadAttribute(int priority)
            {
                Priority = priority;
            }
        }
    }
}
