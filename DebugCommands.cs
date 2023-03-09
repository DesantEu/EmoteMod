using Monocle;
using System.Collections.Generic;
using System;

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
					Emote.DoEmote(emote, true, PlayerHelper.GetPlayer());
				}
				else if (custom == "toggle" || custom == "t") // toggles
				{
					// toggle gravity
					if (emote == "gravity" || emote == "g")
					{
						EmoteModModule.Settings.CancelGravity = !EmoteModModule.Settings.CancelGravity;
						EmoteModModule.echo($"toggled gravity");
						EmoteModModule.Instance.SaveSettings();
						EmoteModModule.Instance.LoadSettings();
					}
					// print current animation
					else if (emote == "i")
					{
						Engine.Commands.Log($"current animation: {PlayerHelper.GetPlayer().Sprite.CurrentAnimationID}");
					}
					// print current state
					else if (emote == "s")
					{
						Engine.Commands.Log(PlayerHelper.GetPlayer().StateMachine.State);
					}
					// dump animations of current sprite mode to log file
					else if (emote == "dump")
					{
						string temp = "animations: ";
						foreach (KeyValuePair<string, Sprite.Animation> animation in PlayerHelper.GetPlayer().Sprite.Animations)
						{
							temp += animation.Key + ", ";
						}
						EmoteModModule.echo(temp);
					}
					// tobble backpack
					else if (emote == "bp")
					{
						BackpackChanger.ScrollBackpack();
					}

					// haha funny
					else if (emote == "funnycommand" || emote == "fc")
					{
						BackpackChanger.EnterSickoMode();
					}
				}
				// binding emotes with console
				else if (int.TryParse(custom, out customInt) && customInt >= 0 && customInt <= 9)
				{
					switch (customInt)
					{
						case 0:
							EmoteModModule.Settings.emote0 = emote;
							break;
						case 1:
							EmoteModModule.Settings.emote1 = emote;
							break;
						case 2:
							EmoteModModule.Settings.emote2 = emote;
							break;
						case 3:
							EmoteModModule.Settings.emote3 = emote;
							break;
						case 4:
							EmoteModModule.Settings.emote4 = emote;
							break;
						case 5:
							EmoteModModule.Settings.emote5 = emote;
							break;
						case 6:
							EmoteModModule.Settings.emote6 = emote;
							break;
						case 7:
							EmoteModModule.Settings.emote7 = emote;
							break;
						case 8:
							EmoteModModule.Settings.emote8 = emote;
							break;
						case 9:
							EmoteModModule.Settings.emote9 = emote;
							break;
					}
					EmoteModModule.echo($"assigned {emote} to numpad {customInt}");
					EmoteModModule.Instance.SaveSettings();
					EmoteModModule.Instance.LoadSettings();
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
						else if (emote == "avatars")
						{

							if (Engine.Scene is Level)
							{
								Level level = (Level)Engine.Scene;

								foreach (Entity e in level.Entities)
								{
									if (e is CelesteNet.Client.Entities.Ghost) // this gets all ghists in the level
									{
										CelesteNet.Client.Entities.Ghost ghost = (CelesteNet.Client.Entities.Ghost)e;

										EmoteModModule.echo($"{ghost.NameTag.Name}, ");
									}
								}
							}

						}
						else
							foreach (KeyValuePair<string, Sprite.Animation> anim in spr[emote].Sprite.Animations)
							{
								anims += anim.Key + " ";
							}

						EmoteModModule.echo(anims);

					}
					catch
					{
						EmoteModModule.echo("something went wrong");
					}
				}

				// the stretches

				else if (custom == "x")
				{
					Stretcher.stretch_x(emoteFloat);
				}
				else if (custom == "y")
				{
					Stretcher.stretch_y(emoteFloat);
				}
				else if (custom == "xy")
				{
					if (emote == "lock" || emote == "l")
						Stretcher.lock_stretch();
					else
					{
						Stretcher.stretch_x(emoteFloat);
						Stretcher.stretch_y(emoteFloat);
					}
				}
				else if (custom == "test")
				{
					
				}

				else
				{
					EmoteModModule.echo($"failed to execute e {custom} {emote}. check your spelling");
				}
			}
		}


	}
}
