using Common.Constants;
using Common.World;
using System;
using World.Network;
using World.Scripting;
using World.World;

public class Courcleinhabitant3Script : NPCScript
{
	public override void OnLoad()
	{
		SetName("_courcleinhabitant3");
		SetRace(10002);
		SetBody(height: 0.9999999f, fat: 1f, upper: 1f, lower: 1f);
		SetFace(skin: 27, eye: 8, eyeColor: 82, lip: 20);

		NPC.ColorA = 0x808080;
		NPC.ColorB = 0x808080;
		NPC.ColorC = 0x808080;		

		EquipItem(Pocket.Face, 0x1324, 0x85880D, 0xD59A5C, 0x87A7D8);
		EquipItem(Pocket.Hair, 0xFAE, 0x211407, 0x211407, 0x211407);
		EquipItem(Pocket.Armor, 0x3B8F, 0xCED3BA, 0x162561, 0x7982A6);

		SetLocation(region: 3300, x: 254379, y: 186844);

		SetDirection(103);
		SetStand("");
	}
}
