﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore;
using CommonCore.State;
using CommonCore.World;
using CommonCore.ObjectActions;

namespace World.Ext.TestIsland
{

    public class TestIslandWellController : MonoBehaviour
    {
        const string MonstersKilledKey = "TestIslandWellMonsters";

        public int MonstersToKill;


        public ActionSpecialEvent SpawnerSpecial;

        private int MonstersKilled;

        private void Start()
        {
            object monstersPrevious;
            if(WorldUtils.GetSceneController().LocalStore.TryGetValue(MonstersKilledKey, out monstersPrevious))
            {
                MonstersKilled = (int)monstersPrevious;
            }
        }

        public void OnEnterWellZone(ActionInvokerData data)
        {
            //we're done if...
            if (GameState.Instance.CampaignState.HasFlag("DemoWellMonstersKilled") || GameState.Instance.CampaignState.HasFlag("DemoWellMonstersSpawned"))
                return;


            //otherwise, activate spawners!
            GameState.Instance.CampaignState.AddFlag("DemoWellMonstersSpawned");
            SpawnerSpecial.Invoke(new ActionInvokerData() { Activator = WorldUtils.GetPlayerController() });

            //also set quest stage
            if (GameState.Instance.CampaignState.GetQuestStage("DemoQuest") < 50)
                GameState.Instance.CampaignState.SetQuestStage("DemoQuest", 50);
            
        }

        public void OnMonsterKilled()
        {
            MonstersKilled++;

            if (MonstersKilled >= MonstersToKill)
            {
                GameState.Instance.CampaignState.AddFlag("DemoWellMonstersKilled");
                if (GameState.Instance.CampaignState.GetQuestStage("DemoQuest") < 60)
                    GameState.Instance.CampaignState.SetQuestStage("DemoQuest", 60);
            }                

            WorldUtils.GetSceneController().LocalStore[MonstersKilledKey] = MonstersKilled;
        }

    }
}