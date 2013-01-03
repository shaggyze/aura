using Common.Constants;
using Common.Events;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class Skatha_peopleScript : NPCScript
{
	public override void OnLoad()
	{
		base.OnLoad();
		SetName("_skatha_people");
		SetRace(10001);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 17, eye: 162, eyeColor: 114, lip: 2);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		

		EquipItem(Pocket.Face, 0xF52, 0x116A94, 0x6C75, 0xE5B354);
		EquipItem(Pocket.Hair, 0xC3F, 0x8B6559, 0x8B6559, 0x8B6559);
		EquipItem(Pocket.Armor, 0x3E2D, 0x808080, 0x808080, 0x808080);
		EquipItem(Pocket.Head, 0x48F2, 0x808080, 0x808080, 0x808080);

		SetLocation(region: 4015, x: 32951, y: 40325);

		SetDirection(194);
        
        ServerEvents.Instance.ErinnDaytimeTick += On12HrTick;
        
		SetStand("chapter4/human/female/anim/female_c4_npc_skatha_human_stand");
        
        Phrases.Add("I'll miss the sounds of the ocean...");
        Phrases.Add("Owen, I miss you.");
        Phrases.Add("You don't have to fear me. Really!");

	}
	public override void Dispose()
	{
		ServerEvents.Instance.ErinnDaytimeTick -= On12HrTick;
		base.Dispose();
	}
    
    private void On12HrTick(object sender, TimeEventArgs e)
	{
		if (e.Hour >= 6 && e.Hour < 18) //Daytime
			Warp(region: 15, x: 100, y: 0);
		else
            Warp(region: 4015, x: 32951, y: 40325);
	}
}