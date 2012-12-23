using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class AnghusScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_anghus");
		SetRace(10002);
		SetBody(height: 1.2f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 15, eye: 12, eyeColor: 134, lip: 2);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		

		EquipItem(Pocket.Face, 0x1324, 0x72664B, 0xDDAA57, 0xE5D952);
		EquipItem(Pocket.Hair, 0x100E, 0x616161, 0x616161, 0x616161);
		EquipItem(Pocket.Armor, 0x3C12, 0x3366, 0x3366, 0xFFFFFF);
		EquipItem(Pocket.Shoe, 0x4338, 0x22632, 0x1B2157, 0x4D3500);
		EquipItem(Pocket.RightHand2, 0x9C4A, 0xA7A7A7, 0x495458, 0xFFFFFF);

		SetLocation(region: 23, x: 29028, y: 35741);

		SetDirection(133);
		SetStand("");
	}
}
