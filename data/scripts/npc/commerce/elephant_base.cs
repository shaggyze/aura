// Aura Script
// --------------------------------------------------------------------------
// Commerce Elephant Base
// --------------------------------------------------------------------------

using System;
using System.Collections;
using Common.Constants;
using Common.World;
using World.Network;
using World.Scripting;
using World.World;

public class CommerceElephantScript : NPCScript
{
	public override void OnLoad()
	{
		SetRace(377);
		SetBody(height: 2f, fat: 1f, upper: 1f, lower: 1f);

		Phrases.Add("Bhoo hoo...");
		Phrases.Add("Bhoo!");
		Phrases.Add("Bhoo! Bhoo!");
		Phrases.Add("Boo!");
		Phrases.Add("Boooo!");
	}

	public override IEnumerable OnTalk(WorldClient c)
	{
		Msg(c, Options.FaceAndName,
			"It has been trumpeting and shuffling non-stop.",
			"As it stomps the ground with its giant feel, it bellows again.",
			"Now, it stares softly at you with its two innocent eyes."
		);
		Msg(c, "Boooo?", "Bhoo!<p/>Bhooo!", "Bhoo, bhoooo!");
		End();
	}
}
