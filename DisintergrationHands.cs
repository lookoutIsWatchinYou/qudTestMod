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
  public class DisintergrationHands : BaseMutation //BaseDefaultEquipmentMutation if an equiped physical
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

    public override string GetLevelText(int Level) => "" + " random area around self, maximum range determined by level\n" + "Damage to non-structural objects: {{rules|" + Level.ToString() + "2d30+"  + "}}\n" + "Damage to structural objects: {{rules|" +  Level.ToString() + "2d30}}\n" 
    + "Cooldown: " + (20 - Level).ToString() +"rounds";



   

public static void Disintegrate(
      Cell C,
      int Level,
      GameObject immunity,
      GameObject owner = null,
      GameObject source = null,
      bool lowPrecision = false,
      bool indirect = false)
    {
         string str1 = Level.ToString() + "2d30" ;
      string str2 = Level.ToString() + "2d30" ;
      bool flag2 = lowPrecision;
      TextConsole textConsole = Look._TextConsole;
      ScreenBuffer scrapBuffer = TextConsole.ScrapBuffer;
      XRLCore.Core.RenderMapToBuffer(scrapBuffer);
      if (owner == null)
        owner = immunity;
      int phase = Phase.getPhase(source ?? owner);
      bool flag1 = false;
      if (C!= null)
      {
    
          foreach (GameObject gameObject in C.GetObjectsInCell())
          {
          
                scrapBuffer.Goto(gameObject);
                scrapBuffer.Write("&" + Phase.getRandomDisintegrationColor(phase).ToString() + ((char) Stat.Random(191, 198)).ToString());
                    if (gameObject.Count > 0 && owner != null && owner.pPhysics != null)
        owner.pPhysics.PlayWorldSound("disintegration", combat: true);
   
        if (!flag2 && owner != null)
      {
       
     
            flag2 = true;
            break;
          
        
      }
            
          }
         
            textConsole.DrawBuffer(scrapBuffer);
            Thread.Sleep(75);
          
        
      }
  
    
        foreach (GameObject gameObject1 in C.GetObjectsInCell())
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
            gameObject2.TakeDamage(Amount, "from %t disintegration!", nameof (Disintegration), Owner: Owner, Source: Source, Accidental: (num1 != 0), Indirect: (num2 != 0));
          }
        }
      
    }

    public override bool FireEvent(Event E)
    {
      DisintergrationHands mutation = null;
      //ai use of mutation
      if (E.ID == "AIGetOffensiveMutationList")
      {
        if (E.GetIntParameter("Distance") <= 3+ this.Level && this.IsMyActivatedAbilityAIUsable(this.ActivatedAbilityID))
        {
          Cell currentCell = this.ParentObject.CurrentCell;
          if (currentCell != null)
          {
            bool flag1 = false;
            bool flag2 = true;
            if (this.ParentObject.pBrain != null)
            {
              foreach (GameObject GO in currentCell.ParentZone.FastSquareVisibility(currentCell.X, currentCell.Y, 4+this.Level, "Combat", this.ParentObject))
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
      
      //player call
      else if (E.ID == "CommandDisintergrationHands")
      {     
        int rangecap;
        if (this.Level>10)
        {
           rangecap = 10;


        }
        else{
          rangecap = this.Level;
        }  
        


     
 

       // List<Cell> cellList = this.PickBurst(3, 8, false, AllowVis.OnlyVisible);

      List<Cell> cellList = this.PickLine(rangecap, AllowVis.Any, IgnoreLOS: true, Snap: true); // cell list is just what the user picks here





        if (currentCell == null)
          return false;
             int index1 = 0;
            for (int index2 = Math.Min(cellList.Count, 10); index1 < index2; ++index1)
      {

        if (cellList.Count == 1 || cellList[index1] != this.ParentObject.CurrentCell)
         DisintergrationHands.Disintegrate(cellList[index1],  this.Level, this.ParentObject);
        if (index1 < index2 - 1 && cellList[index1].IsSolidFor((GameObject) null, this.ParentObject))
          break;
      }


       
       
        
        this.CooldownMyActivatedAbility(this.ActivatedAbilityID, (20 - this.Level));
        this.UseEnergy(100);
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

