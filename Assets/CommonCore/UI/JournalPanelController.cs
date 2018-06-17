﻿using CommonCore.Rpg;
using CommonCore.State;
using CommonCore.StringSub;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CommonCore.UI
{

    public class JournalPanelController : PanelController
    {
        public GameObject ItemTemplatePrefab;
        public RectTransform ScrollContent;

        public RawImage SelectedImage;
        public Text SelectedTitle;
        public Text SelectedDescription;
        public Text SelectedStageDescription;

        private string SelectedQuest;

        public override void SignalPaint()
        {
            SelectedQuest = null;
            PaintList();
            ClearDetailPane();
        }

        private void PaintList()
        {
            foreach (Transform t in ScrollContent)
            {
                Destroy(t.gameObject);
            }
            ScrollContent.DetachChildren();

            var quests = GameState.Instance.CampaignState.GetAllQuests();
            foreach(var quest in quests)
            {
                if (quest.Value == 0)
                    continue;

                string questName = quest.Key;
                var qd = QuestModel.GetDef(quest.Key);
                if (qd != null && !string.IsNullOrEmpty(qd.NiceName))
                    questName = qd.NiceName;
                GameObject itemGO = Instantiate<GameObject>(ItemTemplatePrefab, ScrollContent);
                itemGO.GetComponentInChildren<Text>().text = questName;
                if (quest.Value < 0)
                    itemGO.GetComponentInChildren<Text>().color = Color.red;
                Button b = itemGO.GetComponent<Button>();
                string lexS = quest.Key; //captured quest name
                b.onClick.AddListener(delegate { OnQuestSelected(lexS); }); //scoping is weird here
            }
        }

        

        public void OnQuestSelected(string quest)
        {
            SelectedQuest = quest;
            ClearDetailPane();
            PaintSelectedQuest();
        }

        private void PaintSelectedQuest()
        {
            int questStage = GameState.Instance.CampaignState.GetQuestStage(SelectedQuest);
            QuestDef qd = QuestModel.GetDef(SelectedQuest);
            if(qd == null)
            {
                SelectedTitle.text = SelectedQuest;
                SelectedDescription.text = "{missing def}";
            }
            else
            {
                SelectedTitle.text = qd.NiceName;
                SelectedDescription.text = Sub.Macro(qd.Description);
                Texture2D tex = Resources.Load<Texture2D>("UI/Icons/" + qd.Image);
                if (tex != null)
                    SelectedImage.texture = tex;
                string stageText = qd.GetStageText(questStage);
                if (stageText != null)
                    SelectedStageDescription.text = Sub.Macro(stageText);
            }
        }

        private void ClearDetailPane()
        {
            SelectedImage.texture = null;
            SelectedTitle.text = string.Empty;
            SelectedDescription.text = string.Empty;
        }
    }
}