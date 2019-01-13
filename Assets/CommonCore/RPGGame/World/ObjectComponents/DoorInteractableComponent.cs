﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CommonCore.World;

namespace CommonCore.RpgGame.World
{

    public class DoorInteractableComponent : InteractableComponent
    {
        public string Scene;
        public string SpawnPoint;
        public bool UsePosition = false;
        public Vector3 Position;
        public Vector3 Rotation;

        public override void OnActivate(GameObject activator)
        {
            if (CheckEligibility(activator))
            {
                string spawnPointHack = SpawnPoint;
                if (UsePosition)
                    spawnPointHack = null;
                else if (string.IsNullOrEmpty(SpawnPoint))
                    spawnPointHack = string.Empty;

                GameWorldUtils.ChangeScene(Scene, spawnPointHack, Position, Rotation);
            }
        }
    }
}