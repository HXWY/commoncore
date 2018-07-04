﻿using CommonCore.Rpg;
using CommonCore.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class StatusPanelController : PanelController
    {
        public bool CheckLevelUp = true;

        public RawImage CharacterImage;
        public Text HealthText;
        public Text ArmorText;
        public Text AmmoText;

        public override void SignalPaint()
        {
            CharacterModel pModel = GameState.Instance.PlayerRpgState;
            //PlayerControl pControl = PlayerControl.Instance;

            //repaint 
            HealthText.text = string.Format("Health: {0}/{1}", (int) pModel.Health, (int) pModel.DerivedStats.MaxHealth);
            ArmorText.text = string.Format("Level: {0} ({1}/{2} XP)\n", pModel.Level, pModel.Experience, RpgValues.XPToNext(pModel.Level));

            //this is now somewhat broken because there are more choices in the struct
            string rid = pModel.Gender == Sex.Female ? "portrait_f" : "portrait_m";
            CharacterImage.texture = Resources.Load<Texture2D>("UI/Portraits/" + rid);
        }

        void OnEnable()
        {
            if(CheckLevelUp && GameState.Instance.PlayerRpgState.Experience >= RpgValues.XPToNext(GameState.Instance.PlayerRpgState.Level))
            {
                LevelUpModal.PushModal(OnLevelUpDone);
            }
        }

        private void OnLevelUpDone()
        {
            SignalPaint();
        }

        public void OnClickOpenLevelDialog()
        {
            LevelUpModal.PushModal(SignalPaint);
        }

    }
}