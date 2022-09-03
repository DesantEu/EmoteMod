using Monocle;
using System.Collections.Generic;

namespace Celeste.Mod.EmoteMod
{
	public static class DebugCommands
	{
		[Command("e", "[subcommand] [arg] (might crash the game i warned you)")]
		public static void E(string custom, string emote)
		{
			if (!string.IsNullOrWhiteSpace(emote))
			{
				int customInt;
				int.TryParse(custom, out customInt);
				float emoteFloat; bool isFloat;
				isFloat = float.TryParse(emote, out emoteFloat);
				int emoteInt;
				int.TryParse(emote, out emoteInt);

				// this is where emotes should be
				if (custom == "c" || custom == "custom") // custom emotes
				{
					EmoteModule.Emote(emote, true, PlayerModule.GetPlayer());
				}
				else if (custom == "toggle" || custom == "t") // toggles
				{
					// toggle gravity
					if (emote == "gravity" || emote == "g")
					{
						EmoteModMain.Settings.CancelGravity = !EmoteModMain.Settings.CancelGravity;
						EmoteModMain.echo($"toggled gravity");
						EmoteModMain.Instance.SaveSettings();
						EmoteModMain.Instance.LoadSettings();
					}
					// print current animation
					else if (emote == "i")
					{
						Engine.Commands.Log($"current animation: {PlayerModule.GetPlayer().Sprite.CurrentAnimationID}");
					}
					// print current state
					else if (emote == "s")
					{
						Engine.Commands.Log(PlayerModule.GetPlayer().StateMachine.State);
					}
					// dump animations of current sprite mode to log file
					else if (emote == "dump")
					{
						string temp = "animations: ";
						foreach (KeyValuePair<string, Sprite.Animation> animation in PlayerModule.GetPlayer().Sprite.Animations)
						{
							temp += animation.Key + ", ";
						}
						EmoteModMain.echo(temp);
					}
					// tobble backpack
					else if (emote == "bp")
					{
						BackpackModule.ScrollBackpack();
					}

					// haha funny
					else if (emote == "funnycommand" || emote == "fc")
					{
						BackpackModule.EnterSickoMode();
					}

					else if (emote == "asd")
					{
						EmoteModule.dump_test();
					}
				}
				// binding emotes with console
				else if (int.TryParse(custom, out customInt) && customInt >= 0 && customInt <= 9)
				{
					switch (customInt)
					{
						case 0:
							EmoteModMain.Settings.emote0 = emote;
							break;
						case 1:
							EmoteModMain.Settings.emote1 = emote;
							break;
						case 2:
							EmoteModMain.Settings.emote2 = emote;
							break;
						case 3:
							EmoteModMain.Settings.emote3 = emote;
							break;
						case 4:
							EmoteModMain.Settings.emote4 = emote;
							break;
						case 5:
							EmoteModMain.Settings.emote5 = emote;
							break;
						case 6:
							EmoteModMain.Settings.emote6 = emote;
							break;
						case 7:
							EmoteModMain.Settings.emote7 = emote;
							break;
						case 8:
							EmoteModMain.Settings.emote8 = emote;
							break;
						case 9:
							EmoteModMain.Settings.emote9 = emote;
							break;
					}
					EmoteModMain.echo($"assigned {emote} to numpad {customInt}");
					EmoteModMain.Instance.SaveSettings();
					EmoteModMain.Instance.LoadSettings();
				}
				else if (custom == "d")
				{
					try
					{
						Dictionary<string, SpriteData> spr = GFX.SpriteBank.SpriteData;
						string anims = "";

						if (emote == "modes")
							foreach (KeyValuePair<string, SpriteData> anim in spr)
							{
								anims += anim.Key + " ";
							}
						else
							foreach (KeyValuePair<string, Sprite.Animation> anim in spr[emote].Sprite.Animations)
							{
								anims += anim.Key + " ";
							}

						EmoteModMain.echo(anims);

					}
					catch
					{
						EmoteModMain.echo("something went wrong");
					}
				}

				// the stretches

				else if (custom == "x" && int.TryParse(emote, out emoteInt))
				{
					EmoteStretcher.stretch_x(emoteFloat);
				}
				else if (custom == "y" && int.TryParse(emote, out emoteInt))
				{
					EmoteStretcher.stretch_y(emoteFloat);
				}
				else if (custom == "xy")
				{
					if (emote == "lock" || emote == "l")
						EmoteStretcher.lock_stretch();
					else
					{
						EmoteStretcher.stretch_x(emoteFloat);
						EmoteStretcher.stretch_y(emoteFloat);
					}
				}

				else
				{
					EmoteModMain.echo($"failed to execute e {custom} {emote}. check your spelling");
				}
			}
		}


	}
}
