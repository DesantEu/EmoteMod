using Celeste.Mod.CelesteNet.Client;
using Microsoft.Xna.Framework;
using Monocle;
using System;
using System.Collections.Generic;
using System.Linq;

// stolen from celestenet by Jade "0x0ade" BiggestNutsackInTheWorld

namespace Celeste.Mod.EmoteMod
{
	// TODO: This is taken mostly as is from GhostNet and can be improved.
	public class EmoteWheel : Entity
	{

		public Entity Tracking;

		public float Alpha = 1f;

		protected float time = 0f;

		public bool Shown = false;
		protected bool popupShown = false;
		protected float popupTime = 100f;
		protected bool timeRateSet = false;

		public HashSet<string> TimeRateSkip = new HashSet<string>();
		public float timeSkipForcedDelay = -1f;
		public bool ForceSetTimeRate;

		public float Angle = 0f;

		public int Selected = -1;
		protected int PrevSelected;
		protected float selectedTime = 0f;

		public MTexture Petal = GFX.Gui["emotemod/emotewheel/petalv2_short"];
		public MTexture PetalBottom = GFX.Gui["emotemod/emotewheel/petalv2_short_bottom"];

		public EmoteWheel(Entity tracking)
			: base(Vector2.Zero)
		{
			Tracking = tracking;

			Tag = TagsExt.SubHUD;
			Depth = -1;
		}

		public override void Update()
		{
			if (TimeRateSkip.Contains("EmptySpaceHeart") &&
				timeSkipForcedDelay <= 0f &&
				Engine.Scene is Level l && !l.InCutscene)
			{
				TimeRateSkip.Remove("EmptySpaceHeart");
			}

			if (timeSkipForcedDelay >= 0f)
			{
				timeSkipForcedDelay -= Engine.RawDeltaTime;
			}

			// TimeRate check is for Prologue Dash prompt freeze
			if (Engine.TimeRate > 0.05f && (TimeRateSkip.Count == 0 || ForceSetTimeRate))
			{
				if (Shown && !timeRateSet)
				{
					Engine.TimeRate = 0.25f;
					timeRateSet = true;

				}
				else if (!Shown && timeRateSet)
				{
					Engine.TimeRate = 1f;
					timeRateSet = false;
				}
			}

			base.Update();

			if (Tracking == null || Tracking?.Scene != Scene)
				RemoveSelf();
		}

		public override void Render()
		{
			base.Render();

			string[] emotes = {
				EmoteModMain.Settings.emote0,
				EmoteModMain.Settings.emote1,
				EmoteModMain.Settings.emote2,
				EmoteModMain.Settings.emote3,
				EmoteModMain.Settings.emote4,
				EmoteModMain.Settings.emote5,
				EmoteModMain.Settings.emote6,
				EmoteModMain.Settings.emote7,
				EmoteModMain.Settings.emote8,
				EmoteModMain.Settings.emote9
			};


			if (Shown)
			{
				// get selected
				Angle = EmoteWheelModule.JoystickEmoteWheel.Value.Angle();
				float angle = (float)((Angle + Math.PI * 2f) % (Math.PI * 2f));
				float start = (-0.5f / emotes.Length) * 2f * (float)Math.PI;
				if (2f * (float)Math.PI + start < angle)
				{
					angle -= 2f * (float)Math.PI;
				}
				for (int i = 0; i < emotes.Length; i++)
				{
					float min = ((i - 0.5f) / emotes.Length) * 2f * (float)Math.PI;
					float max = ((i + 0.5f) / emotes.Length) * 2f * (float)Math.PI;
					if (min <= angle && angle <= max)
					{
						Selected = i;
						break;
					}
				}
			}

			time += Engine.RawDeltaTime;

			if (!Shown)
			{
				Selected = -1;
			}
			selectedTime += Engine.RawDeltaTime;
			// detect when new emote is selected
			if (PrevSelected != Selected)
			{
				selectedTime = 0f;
				PrevSelected = Selected;
			}

			// cool fade in and out animation
			float popupAlpha;
			float popupScale;

			popupTime += Engine.RawDeltaTime;
			if (Shown && !popupShown)
			{
				popupTime = 0f;
			}
			else if ((Shown && popupTime > 1f) ||
				(!Shown && popupTime < 1f))
			{
				popupTime = 1f;
			}
			popupShown = Shown;

			if (popupTime < 0.1f)
			{
				float t = popupTime / 0.1f;
				// Pop in.
				popupAlpha = Ease.CubeOut(t);
				popupScale = Ease.ElasticOut(t);

			}
			else if (popupTime < 1f)
			{
				// Stay.
				popupAlpha = 1f;
				popupScale = 1f;

			}
			else
			{
				float t = (popupTime - 1f) / 0.2f;
				// Fade out.
				popupAlpha = 1f - Ease.CubeIn(t);
				popupScale = 1f - 0.2f * Ease.CubeIn(t);
			}

			float alpha = Alpha * popupAlpha;

			if (alpha <= 0f)
				return;

			if (Tracking == null)
				return;

			Level level = SceneAs<Level>();
			if (level == null)
				return;

			popupScale *= level.GetScreenScale();

			// get pos
			Vector2 pos = Tracking.Position;
			pos.Y -= 8f;

			pos = level.WorldToScreen(pos);

			float radius = Petal.Width * 0.5f * 0.75f * popupScale;

			pos = pos.Clamp(
				0f + radius, 0f + radius,
				1920f - radius, 1080f - radius
			);

			// scale of selected stuff is 2f * this
			float selScaleScale = 0.67f;

			#region draw petals
			// animation for selected petal
			float selPetalEase = (selScaleScale - 0.5f) * (1f - Calc.Clamp(Ease.CubeOut(selectedTime / 0.1f), 0f, 1f));

			// draw petals
			for (int i = 0; i < emotes.Length; i++)
			{
				// no idea
				float selScale = Selected == i ? 128f * selScaleScale - selPetalEase : 64f;
				float rot = i / 10f * 2f * (float)Math.PI;

				// draw them around the player
				Petal.DrawCentered(
					// position
					pos // center
					+ new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot))  // position around player
					* selScale // move further out if selected
					* popupScale // scale in and out when shown/hidden
					,
					Color.White * alpha * alpha * alpha, // no idea
					// scale
					Vector2.One // yep
					* popupScale // scale in and out when shown/hidden
					* (Selected == i ? (selScaleScale // if selected then big
					- selPetalEase) // animation on select
					: 0.5f), // not selected = smol
					// rorate
					rot
				);

				// since the selected petal is gonna be bigger and further out we will draw the bottom part over it to make it seem normal
				if (Selected == i)
				{
					PetalBottom.DrawCentered(
						// position
						pos // center
						+ new Vector2((float)Math.Cos(rot), (float)Math.Sin(rot)) // around the player
						* 64f, // always smol
						Color.White * alpha * alpha * alpha,
						// scale
						Vector2.One
						* popupScale // animation on hide/show
						* 0.5f, // always smol
								// rotate
						rot
					);
				}
			}
			#endregion

			#region draw emotes
			// uhhhhh
			float selectedScale = 1f + 0.05f * 2f * selScaleScale;
			float selEmoteEase = (2f * selScaleScale - 1f) * (1f - Calc.Clamp(Ease.CubeOut(selectedTime / 0.1f), 0f, 1f));
			float selMultiplier = ((2f * selScaleScale) - selEmoteEase) * 0.95f;

			// draw emotes
			for (int i = 0; i < emotes.Length; i++)
			{

				string emote = emotes[i];
				if (string.IsNullOrEmpty(emote))
					continue;

				// get pos
				float a = (i / (float)emotes.Length) * 2f * (float)Math.PI;
				Vector2 emotePos = pos + new Vector2(
					(float)Math.Cos(a),
					(float)Math.Sin(a)
				) * radius * (Selected == i ? selMultiplier : 1f); // we want to draw the selected emote further

				Sprite.Animation sanim = getAnimationByName(emote);
				if (sanim == null) continue;

				MTexture[] tanim = sanim.Frames;
				MTexture icon = tanim[Selected == i ?
					(int)Math.Floor(selectedTime / sanim.Delay) % sanim.Frames.Length // animate selected emote
					: tanim.Length > 2 ? tanim.Length / 2 : 0]; // display the middle frame of the rest

				// get size and all
				Vector2 iconSize = new Vector2(icon.Width, icon.Height);
				float iconScale = (Math.Max(icon.Width, icon.Height) / Math.Max(iconSize.X, iconSize.Y)) * 2.5f * popupScale;
				// no idea
				emotePos.Y -= (iconScale * iconSize.Y) / 3f;

				icon.DrawCentered(
					emotePos,
					//                              // blinking when selected                              // default opacity
					Color.White * (Selected == i ? (Calc.BetweenInterval(selectedTime, 0.1f) ? 0.9f : 1f) : 1f) * alpha,
					// scale the selected one up a bit
					Vector2.One * (Selected == i ? selectedScale * selMultiplier : 1f) * iconScale
				);
			}
			#endregion
		}

		private Sprite.Animation getAnimationByName(string animation)
		{
			Dictionary<string, Sprite.Animation> madeline_bp = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
			Dictionary<string, Sprite.Animation> madeline_no_bp = GFX.SpriteBank.SpriteData["player_no_backpack"].Sprite.Animations;
			Dictionary<string, Sprite.Animation> madeline_badeline = GFX.SpriteBank.SpriteData["player_badeline"].Sprite.Animations;
			Dictionary<string, Sprite.Animation> badeline = GFX.SpriteBank.SpriteData["badeline"].Sprite.Animations;

			// b
			if (animation == "b")
			{
				return getAnimationByName("spin");
			}

			// this sucks but idk
			if (madeline_no_bp.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
			{
				return madeline_no_bp[animation];
			}
			else if (madeline_bp.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
			{
				return madeline_bp[animation];
			}
			else if (badeline.Keys.Contains(animation, StringComparer.OrdinalIgnoreCase))
			{
				return badeline[animation];
			}

			else if (findCustomEmote(animation) != null)
			{
				return findCustomEmote(animation);
			}

			else
			{
				EmoteModMain.echo($"EMOTEMOD ERROR: Could not find '{animation}'");

				return getAnimationByName("player_idle");
			}
		}

		private Sprite.Animation findCustomEmote(string animation)
		{
			foreach (KeyValuePair<string, SpriteData> sdata in GFX.SpriteBank.SpriteData)
			{
				if (animation.ToLower().Contains(sdata.Key.ToLower()))
				{
					try
					{
						Dictionary<string, Sprite.Animation> player = GFX.SpriteBank.SpriteData["player"].Sprite.Animations;
						Dictionary<string, Sprite.Animation> anims = sdata.Value.Sprite.Animations;
						string animName = animation.Remove(0, sdata.Key.Length + 1); // strip sprite name
						KeyValuePair<string, Sprite.Animation> newAnim = new KeyValuePair<string, Sprite.Animation>(animName, anims[animName]);

						return newAnim.Value;
					}
					catch
					{
						return null;
					}
				}
			}
			return null;
		}
	}
}