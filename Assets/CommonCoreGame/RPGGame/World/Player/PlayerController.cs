﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using CommonCore.Input;
using CommonCore.UI;
using CommonCore.LockPause;
using CommonCore.DebugLog;
using CommonCore.State;
using CommonCore.Messaging;
using CommonCore.Audio;
using CommonCore.Config;
using CommonCore.World;
using CommonCore.RpgGame.Rpg;
using CommonCore.RpgGame.UI;

namespace CommonCore.RpgGame.World
{
    public class PlayerController : BaseController, ITakeDamage
    {
        public bool AutoinitHud = true;

        [Header("Interactivity")]
        public bool PlayerInControl;

        public float MaxProbeDist;
        public float MaxUseDist;

        [Header("Components")]
        public PlayerMovementComponent MovementComponent;
        public PlayerWeaponComponent WeaponComponent;
        public RpgHUDController HUDScript;
        public Transform CameraRoot;
        public GameObject ModelRoot;        
        public Transform TargetPoint;
        private QdmsMessageInterface MessageInterface;

        [Header("Sounds")]
        public AudioSource WalkSound;
        public AudioSource RunSound;
        public AudioSource FallSound;
        public AudioSource PainSound;
        public AudioSource JumpSound;
        public AudioSource DeathSound;

        [Header("Shooting")]
        public bool AttackEnabled = true;
        public bool AttemptToUseStats = true;

        private Renderer[] ModelRendererCache;


        // Use this for initialization
        public override void Start()
        {
            base.Start();

            Debug.Log("Player controller start");

            if(!CameraRoot)
            {
                CameraRoot = transform.Find("CameraRoot");
            }

            if(!ModelRoot)
            {
                ModelRoot = transform.GetChild(0).gameObject;
            }

            if(!MovementComponent)
            {
                MovementComponent = GetComponent<PlayerMovementComponent>();
            }

            if(!WeaponComponent)
            {
                WeaponComponent = GetComponentInChildren<PlayerWeaponComponent>();
            }

            if(!HUDScript)
            {
                HUDScript = (RpgHUDController)BaseHUDController.Current; //I would not recommend this cast
            }
            
            if(!HUDScript && AutoinitHud)
            {
                Instantiate<GameObject>(CoreUtils.LoadResource<GameObject>("UI/DefaultWorldHUD"));
                if (EventSystem.current == null)
                    Instantiate(CoreUtils.LoadResource<GameObject>("UI/DefaultEventSystem"));

                HUDScript = (RpgHUDController)BaseHUDController.Current;
            }

            MessageInterface = new QdmsMessageInterface(gameObject);

            LockPauseModule.CaptureMouse = true;

            SetDefaultPlayerView();
            SetInitialViewModels();            
        }

        //TODO: still unsure about the state system, but I'll likely rewrite this whole class
        //should be fixedupdate
        public override void Update()
        {
            HandleMessages();

            if (Time.timeScale == 0 || LockPauseModule.IsPaused())
                return;

            if (PlayerInControl && !LockPauseModule.IsInputLocked())
            {
                HandleView();
                HandleInteraction();
                //HandleWeapons();
            }
        }

        private void SetDefaultPlayerView()
        {
            //TODO make this not stupid
            GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
            GameObject fpCamera = CameraRoot.Find("ViewBobNode").Find("FP Camera").gameObject;

            switch (GameParams.DefaultPlayerView)
            {
                case PlayerViewType.PreferFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    SetModelVisibility(false);
                    break;
                case PlayerViewType.PreferThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    SetModelVisibility(true);
                    break;
                case PlayerViewType.ForceFirst:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(true);
                    SetModelVisibility(false);
                    break;
                case PlayerViewType.ForceThird:
                    tpCamera.SetActive(true);
                    fpCamera.SetActive(false);
                    SetModelVisibility(true);
                    break;
                case PlayerViewType.ExplicitOther:
                    tpCamera.SetActive(false);
                    fpCamera.SetActive(false);
                    break;
            }

            PushViewChangeMessage(GameParams.DefaultPlayerView);
        }

        private void SetInitialViewModels()
        {
            WeaponComponent.HandleWeaponChange(EquipSlot.LeftWeapon);
            WeaponComponent.HandleWeaponChange(EquipSlot.RightWeapon);
        }

        private void HandleMessages()
        {
            while (MessageInterface.HasMessageInQueue)
            {
                HandleMessage(MessageInterface.PopFromQueue());
            }
        }

        private void HandleMessage(QdmsMessage message)
        {
            if (message is QdmsFlagMessage)
            {
                string flag = ((QdmsFlagMessage)message).Flag;
                switch (flag)
                {
                    case "RpgChangeWeapon":
                        {
                            var kvm = message as QdmsKeyValueMessage;
                                
                            if(kvm != null && kvm.HasValue<EquipSlot>("Slot"))
                            {
                                WeaponComponent.HandleWeaponChange(kvm.GetValue<EquipSlot>("Slot"));
                            }
                            else
                            {
                                WeaponComponent.HandleWeaponChange(EquipSlot.None);
                            }
                                
                        }                        
                        break;
                }
            }
        }

        private void HandleView()
        {
            if (!(GameParams.DefaultPlayerView == PlayerViewType.PreferFirst || GameParams.DefaultPlayerView == PlayerViewType.PreferThird))
                return;

            if(MappedInput.GetButtonDown(DefaultControls.ChangeView)) 
            {
                //slow and stupid but it'll work for now

                GameObject tpCamera = CameraRoot.Find("Main Camera").gameObject;
                GameObject fpCamera = CameraRoot.Find("ViewBobNode").Find("FP Camera").gameObject;

                if (tpCamera.activeSelf)
                {
                    fpCamera.SetActive(true);
                    tpCamera.SetActive(false);
                    SetModelVisibility(false);
                    PushViewChangeMessage(PlayerViewType.ForceFirst);
                }
                else
                {
                    fpCamera.SetActive(false);
                    tpCamera.SetActive(true);
                    SetModelVisibility(true);
                    PushViewChangeMessage(PlayerViewType.ForceThird);
                }
            }
        }

        private void HandleInteraction()
        {
            //get thing, probe and display tooltip, check use
            //TODO handle tooltips

            HUDScript.ClearTarget();

            int layerMask = LayerMask.GetMask("Default","ActorHitbox");

            Debug.DrawRay(CameraRoot.position, CameraRoot.transform.forward * MaxProbeDist);

            //we should actually do RaycastAll, cull based on 2D distance and separate 3D distance, and possibly handle occlusion

            //raycast all, go through the hits ignoring hits to self
            RaycastHit[] hits = Physics.RaycastAll(CameraRoot.transform.position, CameraRoot.transform.forward, MaxProbeDist * 2, layerMask, QueryTriggerInteraction.Collide);            
            if(hits != null && hits.Length > 0)
            {
                //GameObject nearestObject = null;
                InteractableComponent nearestInteractable = null;
                float nearestDist = float.MaxValue;
                foreach(RaycastHit hit in hits)
                {
                    //skip if it's further than nearestDist (occluded) or flatdist is further than MaxProbeDist (too far away)
                    if (hit.distance > nearestDist)
                        continue;

                    float fDist = CoreUtils.GetFlatVectorToTarget(transform.position, hit.point).magnitude;
                    if (fDist > MaxProbeDist)
                        continue;

                    //nearestObject = hit.collider.gameObject;

                    //if there's a PlayerController attached, we've hit ourselves
                    if (hit.collider.GetComponent<PlayerController>() != null)
                        continue;

                    //TODO pull a similar trick to see if we're pointing at an Actor?

                    //get the interactable component and hitbox component; if it doesn't have either then it's an obstacle
                    InteractableComponent ic = hit.collider.GetComponent<InteractableComponent>();
                    ActorHitboxComponent ahc = hit.collider.GetComponent<ActorHitboxComponent>();
                    if (ic == null && ahc == null)
                    {
                        //we null out our hit first since it's occluded by this one                        
                        nearestInteractable = null;
                        nearestDist = hit.distance;
                        continue;
                    }

                    //it's just us lol
                    if (ahc != null && ahc.ParentController is PlayerController)
                        continue;                    
                    
                    //we have an interactablecomponent and we're not occluded
                    if(ic != null)
                    {
                        nearestInteractable = ic;
                        nearestDist = hit.distance;
                        continue;
                    }

                    //if it doesn't meet any of those criteria then it's an occluder
                    nearestInteractable = null;
                    nearestDist = hit.distance;

                }

                //if(nearestObject != null)
                //    Debug.Log("Nearest: " + nearestObject.name);

                if (nearestInteractable != null && nearestInteractable.enabled)
                {
                    //Debug.Log("Detected: " + ic.Tooltip);
                    
                    HUDScript.SetTargetMessage(nearestInteractable.Tooltip);

                    //actual use
                    if (MappedInput.GetButtonDown(DefaultControls.Use))
                    {
                        nearestInteractable.OnActivate(this.gameObject);
                    }
                }
            }

        }

        private void SetModelVisibility(bool visible) //sets the visibility of the _third-person_ model, I think
        {
            //fill renderer cache if empty
            if(ModelRendererCache == null || ModelRendererCache.Length == 0)
            {
                ModelRendererCache = ModelRoot.GetComponentsInChildren<Renderer>(true);
            }

            foreach(var r in ModelRendererCache)
            {
                if (visible)
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
                else
                    r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
            }

            //TODO delegate to PlayerWeaponComponent

            WeaponComponent.SetVisibility(!visible); //invert because that sets the visibility of the first-person models
        }

        private void PushViewChangeMessage(PlayerViewType newView)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            dict["ViewType"] = newView;
            QdmsKeyValueMessage msg = new QdmsKeyValueMessage(dict, "PlayerChangeView");
            QdmsMessageBus.Instance.PushBroadcast(msg);
        }
        

        public void TakeDamage(ActorHitInfo data)
        {
            if (MetaState.Instance.SessionFlags.Contains("GodMode"))
                return;

            CharacterModel playerModel = GameState.Instance.PlayerRpgState;

            //damage model is very stupid right now, we will make it better later
            float dt = playerModel.DerivedStats.DamageThreshold[(int)data.DType];
            float dr = playerModel.DerivedStats.DamageResistance[(int)data.DType];
            float damageTaken = RpgWorldUtils.CalculateDamage(data.Damage, data.DamagePierce, dt, dr);

            if (data.HitLocation == ActorBodyPart.Head)
                damageTaken *= 2.0f;
            else if (data.HitLocation == ActorBodyPart.LeftArm || data.HitLocation == ActorBodyPart.LeftLeg || data.HitLocation == ActorBodyPart.RightArm || data.HitLocation == ActorBodyPart.RightLeg)
                damageTaken *= 0.75f;

            if(damageTaken > 1)
            {
                if (PainSound != null && !PainSound.isPlaying)
                    PainSound.Play();
            }

            playerModel.Health -= damageTaken;

        }

    }
}
