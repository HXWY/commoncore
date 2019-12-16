﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{
    //DO NOT EDIT THIS FILE

    /// <summary>
    /// What the upcoming scene transition is intended to do
    /// </summary>
    public enum SceneTransitionType
    {
        NewGame, LoadGame, ChangeScene, EndGame
    }

    /// <summary>
    /// Represents the state of the current game session. Largely concerned with transitioning between scenes.
    /// </summary>
    public sealed partial class MetaState
    {
        
        private static MetaState instance;

        private MetaState()
        {
        }

        public static MetaState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new MetaState();
                }
                return instance;
            }
        }

        /// <summary>
        /// Soft-resets session state related to the game world
        /// </summary>
        public void Clear()
        {
            Intents.Clear();
            LoadSave = null;
            PreviousScene = null;
            NextScene = null;
            SkipLoadingScreen = false;
        }

        /// <summary>
        /// Purges the current sessions state
        /// </summary>
        public static void Reset()
        {
            instance = new MetaState();
        }

        //TODO refactor the way Intents work

        public void IntentsExecutePreload()
        {
            Debug.Log(string.Format("Executing intents preload ({0} total)", Intents.Count));
            foreach (Intent i in Intents)
            {
                i.PreloadExecute();
            }
        }

        public void IntentsExecutePostload()
        {
            Debug.Log(string.Format("Executing intents postload ({0} total)", Intents.Count));
            foreach (Intent i in Intents)
            {
                i.PostloadExecute();
            }
        }

        public void IntentsExecuteLoading()
        {
            Debug.Log(string.Format("Executing intents loading ({0} total)", Intents.Count));
            foreach (Intent i in Intents)
            {
                i.LoadingExecute();
            }
        }

        //Actual instance data (shared across game types)

        /// <summary>
        /// The type of scene transition we intend to execute
        /// </summary>
        public SceneTransitionType TransitionType { get; set; }

        /// <summary>
        /// The scene we are transitioning from, or null if none
        /// </summary>
        public string PreviousScene { get; set; }

        /// <summary>
        /// The scene to transition to, or the scene we are in
        /// </summary>
        public string NextScene { get; set; }

        /// <summary>
        /// The save file to load, if applicable
        /// </summary>
        public string LoadSave { get; set; }
        
        /// <summary>
        /// Whether to skip the loading screen for this transition
        /// </summary>
        public bool SkipLoadingScreen { get; set; }

        /// <summary>
        /// Flags set for this game session (cheats, special behaviour, etc)
        /// </summary>
        public HashSet<string> SessionFlags { get; private set; } = new HashSet<string>();

        /// <summary>
        /// Intents to be persisted/executed across the next transition
        /// </summary>
        public List<Intent> Intents { get; private set; } = new List<Intent>();
    }
}
