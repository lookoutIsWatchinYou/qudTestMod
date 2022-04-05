// Decompiled with JetBrains decompiler
// Type: XRL.World.Parts.Mutation.DisintergrationHands
// Assembly: Assembly-CSharp, Version=2.0.203.30, Culture=neutral, PublicKeyToken=null
// MVID: 4F307A10-530B-4D54-95B1-BBE58653F7D8
// Assembly location: F:\SteamLibrary\steamapps\common\Caves of Qud\CoQ_Data\Managed\Assembly-CSharp.dll

using ConsoleLib.Console;
using System;
using System.Collections.Generic;
using System.Threading;
using XRL.Core;
using XRL.Rules;
using XRL.UI;
using XRL.World.Capabilities;
using XRL.World.Effects;

namespace XRL.World.Parts.Mutation
{
  [Serializable]
  public class DisintergrationHands : BaseMutation
  {
    public Guid ActivatedAbilityID = Guid.Empty;

    public DisintergrationHands()
    {
      this.DisplayName = nameof (DisintergrationHands);
      this.Type = "Mental";
    }

    public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) || ID == GetItemElementsEvent.ID;

    public override bool HandleEvent(GetItemElementsEvent E)
    {
      E.Add("chance", 1);
      return base.HandleEvent(E);
    }

    public override bool AllowStaticRegistration() => true;

    public override void Register(GameObject Object)
    {
      Object.RegisterPartEvent((IPart) this, "AIGetOffensiveMutationList");
      Object.RegisterPartEvent((IPart) this, "CommandDisintergrationHands");
      base.Register(Object);
    }

    public override string GetDescription() => "You disintegrate nearby matter.";

    public override string GetLevelText(int Level) => "" + "Area: 3x3 around self\n" + "Damage to non-structural objects: {{rules|" + Level.ToString() + "d100+" + (2 * Level).ToString() + "}}\n" + "Damage to structural objects: {{rules|" + Level.ToString() + "d100+20}}\n" 
    + "Cooldown: 10 rounds";

    public static void Disintegrate(
      Cell C,
      int Radius,
      int Level,
      GameObject immunity,
      GameObject owner = null,
      GameObject source = null,
      bool lowPrecision = false,
      bool indirect = false)
    {
      TextConsole textConsole = Look._TextConsole;
      ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
      XRLCore.Core.RenderMapToBuffer(scrapBuffer);
      if (owner == null)
        owner = immunity;
      int phase = Phase.getPhase(source ?? owner);
      List<Cell> Return = new List<Cell>();
      C.GetAdjacentCells(Radius, Return, false);
      bool flag1 = false;
      if (C.ParentZone != null && C.ParentZone.IsActive())
      {
        for (int index = 0; index < Radius; ++index)
        {
          foreach (Cell cell in Return)
          {
            if (cell.ParentZone == C.ParentZone && cell.IsVisible())
            {
              flag1 = true;
              if (Radius < 3 || cell.PathDistanceTo(C) <= index - Stat.Random(0, 1) && cell.PathDistanceTo(C) > index - Stat.Random(2, 3))
              {
                scrapBuffer.Goto(cell.X, cell.Y);
                scrapBuffer.Write("&" + Phase.getRandomDisintegrationColor(phase).ToString() + ((char) Stat.Random(191, 198)).ToString());
              }
            }
          }
          if (flag1)
          {
            textConsole.DrawBuffer(scrapBuffer);
            Thread.Sleep(10);
          }
        }
      }
      if (Return.Count > 0 && owner != null && owner.pPhysics != null)
        owner.pPhysics.PlayWorldSound("disintegration", combat: true);
      string str1 = Level.ToString() + "d100+" + (2 * Level).ToString();
      string str2 = Level.ToString() + "d100";
      bool flag2 = lowPrecision;
      if (!flag2 && owner != null)
      {
        foreach (Cell cell in Return)
        {
          if (cell.HasObject(new Predicate<GameObject>(owner.IsRegardedWithHostilityBy)))
          {
            flag2 = true;
            break;
          }
        }
      }
      foreach (Cell cell in Return)
      {
        foreach (GameObject gameObject1 in cell.GetObjectsInCell())
        {
          if (gameObject1 != immunity && gameObject1.PhaseMatches(phase) && gameObject1.GetMatterPhase() <= 3 && gameObject1.GetIntProperty("Electromagnetic") <= 0)
          {
            string Dice = gameObject1.HasPart("Inorganic") ? str2 : str1;
            GameObject gameObject2 = gameObject1;
            int Amount = Dice.RollCached();
            bool flag3 = flag2 && owner != null && !gameObject1.IsHostileTowards(owner);
            GameObject Owner = owner;
            GameObject Source = source;
            int num1 = flag3 ? 1 : 0;
            int num2 = indirect ? 1 : 0;
            gameObject2.TakeDamage(Amount, "from %t DisintergrationHands!", nameof (DisintergrationHands), Owner: Owner, Source: Source, Accidental: (num1 != 0), Indirect: (num2 != 0));
          }
        }
      }
    }

    public override bool FireEvent(Event E)
    {
      if (E.ID == "AIGetOffensiveMutationList")
      {
        if (E.GetIntParameter("Distance") <= 3 && this.IsMyActivatedAbilityAIUsable(this.ActivatedAbilityID))
        {
          Cell currentCell = this.ParentObject.CurrentCell;
          if (currentCell != null)
          {
            bool flag1 = false;
            bool flag2 = true;
            if (this.ParentObject.pBrain != null)
            {
              foreach (GameObject GO in currentCell.ParentZone.FastSquareVisibility(currentCell.X, currentCell.Y, 4, "Combat", this.ParentObject))
              {
                if (GO != this.ParentObject)
                {
                  if (this.ParentObject.pBrain.GetFeeling(GO) >= 0)
                  {
                    flag2 = false;
                    break;
                  }
                  flag1 = true;
                }
              }
            }
            if (flag1 & flag2)
              E.AddAICommand("CommandDisintergrationHands");
          }
        }
      }
      else if (E.ID == "CommandDisintergrationHands")
      {
        Cell currentCell = this.ParentObject.GetCurrentCell();
        if (currentCell == null)
          return false;
        DisintergrationHands.Disintegrate(currentCell, 3, this.Level, this.ParentObject);
        this.CooldownMyActivatedAbility(this.ActivatedAbilityID, 3);
        this.UseEnergy(1000);
      }
      return base.FireEvent(E);
    }

    public override bool ChangeLevel(int NewLevel) => base.ChangeLevel(NewLevel);

    public override bool Mutate(GameObject GO, int Level)
    {
      this.ActivatedAbilityID = this.AddMyActivatedAbility(nameof (DisintergrationHands), "CommandDisintergrationHands", "Mental Mutation", Icon: "ê");
      return base.Mutate(GO, Level);
    }

    public override bool Unmutate(GameObject GO)
    {
      this.RemoveMyActivatedAbility(ref this.ActivatedAbilityID);
      return base.Unmutate(GO);
    }
  }
}
