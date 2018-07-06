﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CommonCore.State
{
    public enum ConditionType
    {
        Flag, NoFlag, Item, Variable, Affinity, Quest, ActorValue //Eval is obviously not supported, might be able to provide Emit instead
    }

    public enum ConditionOption
    {
        Consume, Greater, Less, Equal, GreaterEqual, LessEqual, Started, Finished
    }

    [Serializable]
    public struct EditorConditional
    {
        public ConditionType Type;
        public string Target;
        public ConditionOption Option;
        public string OptionValue;

        public Conditional Parse()
        {
            ConditionOption? opt = null;
            if (Type == ConditionType.Item || Type == ConditionType.Quest || Type == ConditionType.ActorValue)
                opt = Option;

            IComparable val = (IComparable)CCBaseUtil.StringToNumericAuto(OptionValue);

            return new Conditional(Type, Target, opt, val);
        }
    }

    public class Conditional
    {
        public readonly ConditionType Type;
        public readonly string Target;
        public readonly ConditionOption? Option;
        public readonly IComparable OptionValue;

        public Conditional(ConditionType type, string target, ConditionOption? option, IComparable optionValue)
        {
            Type = type;
            Target = target;
            Option = option;
            OptionValue = optionValue;
        }

        public bool Evaluate()
        {
            switch (Type)
            {
                case ConditionType.Flag:
                    return GameState.Instance.CampaignState.HasFlag(Target);
                case ConditionType.NoFlag:
                    return !GameState.Instance.CampaignState.HasFlag(Target);
                case ConditionType.Item:
                    int qty = GameState.Instance.PlayerRpgState.Inventory.CountItem(Target);
                    if (qty < 1)
                        return false;
                    else return true;
                case ConditionType.Variable:
                    if (GameState.Instance.CampaignState.HasVar(Target))
                        return EvaluateValueWithOption(GameState.Instance.CampaignState.GetVar<int>(Target));
                    else return false;
                case ConditionType.Affinity:
                    throw new NotImplementedException(); //could be supported, but isn't yet
                case ConditionType.Quest:
                    if (GameState.Instance.CampaignState.HasQuest(Target))
                        return EvaluateValueWithOption(GameState.Instance.CampaignState.GetQuestStage(Target));
                    else return false;
                case ConditionType.ActorValue:
                    try
                    {
                        int av = GameState.Instance.PlayerRpgState.GetAV<int>(Target);
                        return EvaluateValueWithOption(av);
                    }
                    catch (KeyNotFoundException)
                    {
                        return false;
                    }
                default:
                    throw new NotSupportedException();
            }
        }

        private bool EvaluateValueWithOption(IComparable value)
        {
            //technically out of spec but should be fine
            //probably the only instance that will work here but not with Katana
            switch (Option.Value)
            {
                case ConditionOption.Greater:
                    return value.CompareTo(OptionValue) > 0;
                case ConditionOption.Less:
                    return value.CompareTo(OptionValue) < 0;
                case ConditionOption.Equal:
                    return value.CompareTo(OptionValue) == 0;
                case ConditionOption.GreaterEqual:
                    return value.CompareTo(OptionValue) >= 0;
                case ConditionOption.LessEqual:
                    return value.CompareTo(OptionValue) <= 0;
                case ConditionOption.Started:
                    return value.CompareTo(0) > 0;
                case ConditionOption.Finished:
                    return value.CompareTo(0) < 0;
                default:
                    throw new NotSupportedException();
            }
        }
    }

    internal enum MicroscriptType
    {
        Flag, Item, Variable, Affinity, Quest, ActorValue //eval is again not supported
    }

    internal enum MicroscriptAction
    {
        Set, Toggle, Add, Give, Take, Start, Finish
    }

    internal class MicroscriptNode //"directive" in Katana parlance
    {
        public readonly MicroscriptType Type;
        public readonly string Target;
        public readonly MicroscriptAction Action;
        public readonly object Value;

        public MicroscriptNode(MicroscriptType type, string target, MicroscriptAction action, object value)
        {
            Type = type;
            Target = target;
            Action = action;
            Value = value;
        }

        public void Execute()
        {
            switch (Type)
            {
                case MicroscriptType.Flag:
                    if (Action == MicroscriptAction.Toggle)
                    {
                        GameState.Instance.CampaignState.ToggleFlag(Target);
                    }
                    else if (Action == MicroscriptAction.Set)
                    {
                        bool sv = Convert.ToBoolean(Value);
                        GameState.Instance.CampaignState.SetFlag(Target, sv);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Item:
                    if (Action == MicroscriptAction.Give)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.AddItem(Target, Convert.ToInt32(Value));
                    }
                    else if (Action == MicroscriptAction.Take)
                    {
                        GameState.Instance.PlayerRpgState.Inventory.UseItem(Target, Convert.ToInt32(Value));
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Variable:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.CampaignState.SetVar(Target, Value.ToString());
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        decimal oldVal = Convert.ToDecimal(GameState.Instance.CampaignState.GetVar(Target));
                        GameState.Instance.CampaignState.SetVar(Target, (oldVal + Convert.ToDecimal(Value)).ToString()); //this is probably unsafe
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                case MicroscriptType.Affinity:
                    throw new NotImplementedException();
                case MicroscriptType.Quest:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.CampaignState.SetQuestStage(Target, Convert.ToInt32(Value));
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.CampaignState.SetQuestStage(Target, GameState.Instance.CampaignState.GetQuestStage(Target) + Convert.ToInt32(Value));
                    }
                    else if (Action == MicroscriptAction.Start)
                    {
                        GameState.Instance.CampaignState.StartQuest(Target);
                    }
                    else if (Action == MicroscriptAction.Finish)
                    {
                        if (GameState.Instance.CampaignState.IsQuestStarted(Target))
                            GameState.Instance.CampaignState.SetQuestStage(Target, Convert.ToInt32(Value));
                    }
                    break;
                case MicroscriptType.ActorValue:
                    if (Action == MicroscriptAction.Set)
                    {
                        GameState.Instance.PlayerRpgState.SetAV(Target, Value);
                    }
                    else if (Action == MicroscriptAction.Add)
                    {
                        GameState.Instance.PlayerRpgState.ModAV(Target, Value);
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }
                    break;
                default:
                    throw new NotSupportedException();
            }

        }
    }
}