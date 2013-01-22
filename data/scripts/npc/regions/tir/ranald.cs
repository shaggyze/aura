// Aura Script
// --------------------------------------------------------------------------
// Ranald - Combat Instructor
// --------------------------------------------------------------------------

using System;
using System.Collections;
using Common.Constants;
using Common.World;
using World.Network;
using World.Scripting;
using World.World;

public class RanaldScript : NPCScript
{
	public override void OnLoad()
	{
		SetName("_ranald");
		SetRace(10002);
		SetBody(height: 1f, fat: 1f, upper: 1.1f, lower: 1f);
		SetFace(skin: 20, eye: 0, eyeColor: 0, lip: 0);
		SetStand("human/male/anim/male_natural_stand_npc_ranald02", "human/male/anim/male_natural_stand_npc_ranald_talk");
		SetLocation("tir", 4651, 32166, 195);

		EquipItem(Pocket.Face, 4900, 0xF88B4A);
		EquipItem(Pocket.Hair, 4154, 0x4D4B53);
		EquipItem(Pocket.Armor, 15652, 0xAC9271, 0x4D4F48, 0x7C6144);
		EquipItem(Pocket.Shoe, 17012, 0x9C7D6C, 0xFFC9A3, 0xF7941D);
		EquipItem(Pocket.LeftHand1, 40012, 0xDCDCDC, 0xC08B48, 0x808080);

		Phrases.Add("I need a drink...");
		Phrases.Add("I guess I drank too much last night...");
		Phrases.Add("I need a nap...");
		Phrases.Add("I should drink in moderation...");
		Phrases.Add("I should sharpen my blade later.");
		Phrases.Add("It's really dusty here.");
		Phrases.Add("What's with the hair styles of today's kids?");
	}

	public override IEnumerable OnTalk(WorldClient c)
	{
		Msg(c, Options.FaceAndName,
			"From his appearance and posture, there is no doubt that he is well into middle age, but he is surprisingly well-built and in good shape.",
			"Long fringes of hair cover half of his forehead and right cheek. A strong nose bridge stands high between his shining hawkish eyes.",
			"His deep, low voice has the power to command other people's attention."
		);
		MsgSelect(c, "How can I help you?", "Start Conversation", "@talk", "Shop", "@shop", "Modify Item", "@modify", "Get Ciar Beginner Dungeon Pass", "@pass");
		
		var r = Wait();
		switch (r)
		{
			case "@talk":
			{
				Msg(c, "Hmm...<br/>Nice to meet you.");
				
			L_Keywords:
				Msg(c, Options.Name, "(Ranald is paying attention to me.)");
				ShowKeywords(c);
				
				var keyword = Wait();
				
				Msg(c, "Well, I don't really know...");
				goto L_Keywords;
			}

			case "@shop":
			{
				Msg(c, "Tell me if you need a Quest Scroll.<br/>Working on these quests can also be a good way to train yourself.");
				OpenShop(c);
				End();
			}
			
			case "@modify":
			{
				Msg(c,
					"Hmm... You want me to modify your item? You got some nerve!",
					"Ha ha. Just joking. Do you need to modify an item? Count on Ranald.",
					"Pick an item to modify.",
					"Oh, before that. Types or numbers of modifications are different depending on what item you want to modify. Always remember that."
				);
				End();
			}

			case "@pass":
			{
				GiveItem(c, 63139);
				Notice(c, "Recieved Ciar Beginner Dungeon Pass from Ranald.");
				Msg(c, "Ok, here's the pass.<br/>You can ask for it again if you need it.<br/>That doesn't mean you can fill up the iventory with a pile of passes.");
				End();
			}
		}
	}
}
