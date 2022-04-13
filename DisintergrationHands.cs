// Decompiled with JetBrains decompiler
// Type: XRL.World.Parts.Mutation.DisintergrationHands
// Assembly: Assembly-CSharp, Version=2.0.203.30, Culture=neutral, PublicKeyToken=null

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
  public class DisintergrationHands : BaseDefaultEquipmentMutation //BaseDefaultEquipmentMutation if an equiped physical
  {
    public string BodyPartType = "Hands";


    public bool CreateObject = true;

    public Guid ActivatedAbilityID = Guid.Empty;

    public DisintergrationHands()
    {
      this.DisplayName ="Disintergration Hands";
      this.Type = "Physical";
    }
    public override bool GeneratesEquipment() => true;

    public override bool WantEvent(int ID, int cascade) => base.WantEvent(ID, cascade) || ID == GetItemElementsEvent.ID;

    public override bool HandleEvent(GetItemElementsEvent E)
    {
      E.Add("chance", 1);
      return base.HandleEvent(E);
    }


    public override void Register(GameObject Object)
    {
      Object.RegisterPartEvent((IPart) this, "AIGetOffensiveMutationList");
      Object.RegisterPartEvent((IPart) this, "CommandDisintergrationHands");
      base.Register(Object);
    }

public override string GetDescription()
    {
      BodyPart registeredSlot = this.GetRegisteredSlot(this.BodyPartType, true);
      return registeredSlot != null ? "You emit a blast of  Disintergration"  + "." : "You emit a blast of Disintergration.";
    }
    public override string GetLevelText(int Level) => "" + " fixed line , maximum range determined by level\n" + "Damage to non-structural objects: {{rules|" + Level.ToString() + "2d30+"  + "}}\n" 
    + "Cooldown: " + (20 - Level).ToString() +"rounds";



   

public static void Disintegrate(
      Cell C,
      int Level,
      GameObject immunity,
      GameObject owner = null,
      bool noObjects = true,
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
      if (C!= null )
      {
    //WILL OFC ONLY TARGET OBJECTS
    
          foreach (GameObject gameObject1 in C.GetObjectsInCell())
          {
                 
                scrapBuffer.Goto(gameObject1);
                scrapBuffer.Write("&" + Phase.getRandomDisintegrationColor(phase).ToString() + ((char) Stat.Random(191, 198)).ToString());
                owner.pPhysics.PlayWorldSound("disintegration", combat: true);
                noObjects = false;

   
        if (!flag2 && owner != null)
      {
       
     
            flag2 = true;
            break;
          
        
      }
            
          }
          //added check to see if we have objects in target, if not we paint map anyway if we do its already been painted so we dont
          if (noObjects){
          
            scrapBuffer.Goto(C.X, C.Y);
            scrapBuffer.Write("&" + Phase.getRandomDisintegrationColor(phase).ToString() + ((char) Stat.Random(191, 198)).ToString());
             owner.pPhysics.PlayWorldSound("disintegration", combat: true);
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

    public bool CheckObjectProperlyEquipped()
    {
      if (!this.CreateObject)
        return true;
      return this.HasRegisteredSlot(this.BodyPartType) && this.GetRegisteredSlot(this.BodyPartType, false) != null;
    }
    public override bool FireEvent(Event E)
    {
      DisintergrationHands mutation = null;
      //ai use of mutation
      if (E.ID == "AIGetOffensiveMutationList")
      {
        if (E.GetIntParameter("Distance") <= 1 && this.IsMyActivatedAbilityAIUsable(this.ActivatedAbilityID)&& this.CheckObjectProperlyEquipped())
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
        if (!this.CheckObjectProperlyEquipped())
        {
          if (this.ParentObject.IsPlayer())
            Popup.ShowFail("Your " + this.BodyPartType + " is too damaged to do that!");
          return false;
        }
        else{

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



 ScreenBuffer scrapBuffer1 = ScreenBuffer.GetScrapBuffer1(true);
      XRLCore.Core.RenderMapToBuffer(scrapBuffer1);

        if (currentCell == null)
          return false;
             int index1 = 0;
            for (int index2 = Math.Min(cellList.Count, 10); index1 < index2; ++index1)
      {

        if (cellList.Count == 1 || cellList[index1] != this.ParentObject.CurrentCell )
         DisintergrationHands.Disintegrate(cellList[index1],  this.Level, this.ParentObject);
        if (index1 < index2 - 1 && cellList[index1].IsSolidFor((GameObject) null, this.ParentObject) ||index1>rangecap)
          break;
      }


       
       
        
        this.CooldownMyActivatedAbility(this.ActivatedAbilityID, (20 - this.Level));
        this.UseEnergy(100);
      }}
      
      return base.FireEvent(E);
    }




    public override bool ChangeLevel(int NewLevel) => base.ChangeLevel(NewLevel);

    
    public override void OnRegenerateDefaultEquipment(Body body) => base.OnRegenerateDefaultEquipment(body);



    public void MakeFlickering(BodyPart part)
    {
      if (part == null)
        return;
      if (part.DefaultBehavior != null && part.DefaultBehavior.Blueprint != "Disintergration flickers" && !part.DefaultBehavior.pRender.DisplayName.Contains("{{Flickering}"))
        part.DefaultBehavior.pRender.DisplayName = "{{Flickering}} " + part.DefaultBehavior.pRender.DisplayName;
      if (part.Parts == null)
        return;
      for (int index = 0; index < part.Parts.Count; ++index)
        this.MakeFlickering(part.Parts[index]);
    }


  public override void OnDecorateDefaultEquipment(Body body)
    {
      if (this.CreateObject)
      {
        BodyPart part1;
        if (!this.HasRegisteredSlot(this.BodyPartType))
        {
          part1 = body.GetFirstPart(this.BodyPartType);
          if (part1 != null)
            this.RegisterSlot(this.BodyPartType, part1);
        }
        else
          part1 = this.GetRegisteredSlot(this.BodyPartType, false);
        if (part1 != null && part1.DefaultBehavior == null)
        {
          GameObject gameObject = GameObject.create("Disintergration Flickers");
          gameObject.GetPart<Armor>().WornOn = this.BodyPartType;
          part1.DefaultBehavior = gameObject;
        }
        this.MakeFlickering(part1);
        if (this.BodyPartType == "Hands")
        {
          foreach (BodyPart part2 in body.GetParts())
          {
            if (part2.Type == "Hand")
              this.MakeFlickering(part2);
          }
        }
      }
      base.OnDecorateDefaultEquipment(body);
    }




    public override bool Mutate(GameObject GO, int Level)
    {
      this.ActivatedAbilityID = this.AddMyActivatedAbility(nameof (DisintergrationHands), "CommandDisintergrationHands", "Physical Mutation", Icon: "ê");
      return base.Mutate(GO, Level);
    }

    public override bool Unmutate(GameObject GO)
    {
      this.RemoveMyActivatedAbility(ref this.ActivatedAbilityID);
      return base.Unmutate(GO);
    }
  }
  }

