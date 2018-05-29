﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using CommonCore.State;


namespace CommonCore.World
{
    //TODO: probably 90% should be moved into WorldSceneController
    public abstract class BaseSceneController : MonoBehaviour
    {
        public Dictionary<string, System.Object> LocalStore { get; protected set; }

        public virtual void Awake()
        {
            Debug.Log("Base Scene Controller Awake");
        }

        public virtual void Start()
        {
            Debug.Log("Base Scene Controller Start");
        }

        public virtual void Update()
        {

        }

        public virtual void ExitScene()
        {
            //for now, assume something else set up metagamestate
            Debug.Log("Exiting scene: ");
            Save();
            SceneManager.LoadScene("LoadingScene");
        }

        public void Save()
        {
            GameState gs = GameState.Instance;

            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name;
            Debug.Log("Saving scene: " + name);

            //get restorable components
            List<RestorableComponent> rcs = new List<RestorableComponent>();
            WorldUtils.GetComponentsInDescendants(transform, rcs);

            //purge local object state
            Dictionary<string, RestorableData> localState;
            if (gs.LocalObjectState.ContainsKey(name))
            {
                localState = gs.LocalObjectState[name];
                localState.Clear();
            }
            else
            {
                localState = new Dictionary<string, RestorableData>();
                gs.LocalObjectState[name] = localState;
            }

            foreach (RestorableComponent rc in rcs)
            {
                RestorableData rd = rc.Save();
                if (rc is LocalRestorableComponent || rc is BlankRestorableComponent)
                {
                    localState[rc.gameObject.name] = rd;
                }
                else if (rc is MotileRestorableComponent)
                {
                    gs.MotileObjectState[rc.gameObject.name] = rd;
                }
                else if (rc is PlayerRestorableComponent)
                {
                    gs.PlayerWorldState = rd;
                }
                else
                {
                    Debug.LogWarning("Unknown restorable type in " + rc.gameObject.name);
                }
            }

            //purge and copy local data store
            if (gs.LocalDataState.ContainsKey(name))
            {
                gs.LocalDataState.Remove(name);
            }
            gs.LocalDataState.Add(name, LocalStore);

        }

        public void Restore()
        {
            GameState gs = GameState.Instance;

            Scene scene = SceneManager.GetActiveScene();
            string name = scene.name;

            Debug.Log("Restoring scene: " + name);

            //restore local store
            LocalStore = gs.LocalDataState.ContainsKey(name) ? gs.LocalDataState[name] : new Dictionary<string, System.Object>();

            //restore local object state
            RestoreLocalObjects(gs, name);

            //restore motile objects
            RestoreMotileObjects(gs, name);

            //restore player
            RestorePlayer(gs);

        }

        protected void RestoreLocalObjects(GameState gs, string name)
        {
            if (gs.LocalObjectState.ContainsKey(name))
            {
                Dictionary<string, RestorableData> localState = gs.LocalObjectState[name];

                foreach (KeyValuePair<string, RestorableData> kvp in localState)
                {
                    if (kvp.Value is DynamicRestorableData)
                        RestoreLocalObject(kvp);
                    else
                        RestoreBlankObject(kvp);
                }
            }
            else
            {
                //no data, skip local
                Debug.Log("No local object data for scene!");
            }
        }

        private void RestoreBlankObject(KeyValuePair<string, RestorableData> kvp)
        {
            Transform t = transform.FindDeepChild(kvp.Key);
            if (t != null)
            {
                GameObject go = t.gameObject;

                //if it exists, restore it
                BlankRestorableComponent rc = go.GetComponent<BlankRestorableComponent>();
                if (rc != null)
                {
                    rc.Restore(kvp.Value);
                }
                else
                {
                    Debug.LogWarning("Blank object " + go.name + " has no restorable component!");
                }
            }
            else
            {
                Debug.LogWarning("Attempted to restore " + kvp.Key + " but object doesn't exist!");
            }
        }

        private void RestoreLocalObject(KeyValuePair<string, RestorableData> kvp)
        {
            DynamicRestorableData rd = kvp.Value as DynamicRestorableData;

            Transform t = transform.FindDeepChild(kvp.Key);

            if (t != null)
            {
                GameObject go = t.gameObject;

                //if it exists, restore it
                LocalRestorableComponent rc = go.GetComponent<LocalRestorableComponent>();
                if (rc != null)
                {
                    rc.Restore(rd);
                }
                else
                {
                    Debug.LogWarning("Local object " + go.name + " has no restorable component!");
                }
            }
            else
            {
                //if it doesn't, create it
                try
                {
                    GameObject go = Instantiate(Resources.Load("entities/" + rd.FormID), transform) as GameObject;

                    if (go != null)
                    {
                        go.name = kvp.Key;

                        LocalRestorableComponent rc = go.GetComponent<LocalRestorableComponent>();
                        if (rc != null)
                        {
                            rc.Restore(rd);
                        }
                        else
                        {
                            Debug.LogWarning("Local object " + go.name + " has no restorable component!");
                        }
                    }
                }
                catch (ArgumentException)
                {
                    Debug.LogWarning("Tried to spawn " + rd.FormID + " but couldn't find prefab!");
                }
            }
        }

        protected void RestoreMotileObjects(GameState gs, string name)
        {
            foreach (KeyValuePair<string, RestorableData> kvp in gs.MotileObjectState)
            {
                DynamicRestorableData rd = kvp.Value as DynamicRestorableData;

                if (rd == null)
                {
                    Debug.LogError("Local object " + kvp.Key + " has invalid data!");
                }

                //is it in this scene
                string objectSceneName = rd.Scene;
                if (objectSceneName == name)
                {
                    //we have a match! since it's motile, we'll have to create a new object
                    try
                    {
                        GameObject go = Instantiate(Resources.Load("entities/" + rd.FormID), transform) as GameObject;
                        {
                            go.name = kvp.Key;

                            MotileRestorableComponent mrc = go.GetComponent<MotileRestorableComponent>();
                            if (mrc != null)
                            {
                                mrc.Restore(rd);
                            }
                            else
                            {
                                Debug.LogWarning("Motile object " + go.name + " has no restorable component!");
                            }
                        }
                    }
                    catch (ArgumentException)
                    {
                        Debug.LogWarning("Tried to spawn " + rd.FormID + " but couldn't find prefab!");
                    }

                }
            }
        }

        protected void RestorePlayer(GameState gs)
        {
            MetaState mgs = MetaState.Instance;
            GameObject player = WorldUtils.GetPlayerObject();
            RestorableData prd = gs.PlayerWorldState;
            if (prd != null)
            {
                if (player == null)
                {
                    //spawn the player object in
                    player = Instantiate(Resources.Load("entities/" + "spec_player"), transform) as GameObject;
                    if (mgs.TransitionType == SceneTransitionType.LoadGame)
                    {
                        player.GetComponent<PlayerRestorableComponent>().Restore(prd);
                    }
                    else
                    {
                        // get intent and move
                        RestorePlayerToIntent(mgs, player);
                    }

                }
                else
                {
                    //restore player if relevant, warn either way
                    if (mgs.TransitionType == SceneTransitionType.LoadGame)
                    {
                        player.GetComponent<PlayerRestorableComponent>().Restore(prd);
                        Debug.LogWarning("Player already exists, restoring anyway");
                    }
                    else
                    {
                        //if an intent exists, move
                        RestorePlayerToIntent(mgs, player);
                        Debug.LogWarning("Player already exists");
                    }


                }
            }
            else
            {
                //warn that no player data exists
                Debug.LogWarning("No player world data exists!");
            }
        }

        private void RestorePlayerToIntent(MetaState mgs, GameObject player)
        {
            if (mgs.PlayerIntent != null)
            {
                if (!string.IsNullOrEmpty(mgs.PlayerIntent.SpawnPoint))
                {
                    GameObject spawnPoint = WorldUtils.FindObjectByTID(mgs.PlayerIntent.SpawnPoint);
                    player.transform.position = spawnPoint.transform.position;
                    player.transform.rotation = spawnPoint.transform.rotation;
                }
                else
                {
                    player.transform.position = mgs.PlayerIntent.Position;
                    player.transform.rotation = mgs.PlayerIntent.Rotation;
                }
            }
            else
            {
                Debug.LogWarning("No player spawn intent exists!");
            }
        }
    }
}
