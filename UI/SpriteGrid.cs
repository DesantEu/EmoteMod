using Monocle;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections;
using System;
using System.Reflection;
using System.Linq;

namespace Celeste.Mod.EmoteMod
{
    public enum EmotePickerStage
    {
        SpriteBank,
        Emote,
        Done
    }

    public class SpriteGrid : Entity
    {
        IEnumerator coro;
        EmotePickerStage stage;
        EmoteCard parent;

        List<string> customsForRemoval;
        List<SpriteGridCell> cells;
        List<SpriteGridTitle> titles;

        float circle_radius;
        float bg_rotation;
        float bg_alpha = 0f;
        Vector2 bg_origin;
        static float bg_thickness = Celeste.TargetWidth;

        public static float spacing = 70;
        public float scroll_offset;
        float left_start = Celeste.TargetWidth / 2 - 2.5f * SpriteGridCell.default_size * SpriteGridCell.upscale - spacing * 2;
        int cursor_at;

        bool Focused;

        float top_free_space = 90;

        Vector2 new_cell_pos(float vert_offset = 0, int reset_at = 0)
        {
            float x = left_start + (cells.Count - reset_at) % 5 * (SpriteGridCell.upscale * SpriteGridCell.default_size + spacing);

            // float y = vert_offset + (cells.Count - reset_at) / 5
            //         * SpriteGridCell.upscale * SpriteGridCell.default_size
            //         * 1.3f;
            int ycc = reset_at % 5 == 0 ? cells.Count() : cells.Count - reset_at + 5 - reset_at % 5;
            float y = SpriteGridCell.upscale * SpriteGridCell.default_size * 1.3f * (ycc / 5) + vert_offset;
            // float y = SpriteGridCell.upscale * SpriteGridCell.default_size * 1.3f * (ycc / 5) + vert_offset;
            // float y = top_free_space + SpriteGridCell.upscale * SpriteGridCell.default_size * 1.3f * (cells.Count() / 5);

            return new(x, y);
        }

        public override void Update()
        {
            base.Update();
            // EmoteModModule.echo("gallery update");

            if (coro != null)
                coro.MoveNext();

            if (Focused)
            {
                #region navigation
                if (Input.MenuRight.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at++;
                    if (cursor_at >= cells.Count)
                        cursor_at = 0;

                    cells[cursor_at].Select();
                }
                else if (Input.MenuLeft.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at--;
                    if (cursor_at < 0)
                        cursor_at = cells.Count - 1;

                    cells[cursor_at].Select();
                }
                else if (Input.MenuUp.Pressed)
                {
                    cells[cursor_at].Deselect();

                    if (cursor_at - 5 < 0)
                    {
                        int temp_pos = ((int)Math.Ceiling(cells.Count / 5f)) * 5 + cursor_at - 5;
                        if (temp_pos >= cells.Count)
                            cursor_at = temp_pos - 5;
                        else
                            cursor_at = temp_pos;
                    }
                    else
                    {
                        cursor_at -= 5;
                    }

                    cells[cursor_at].Select();
                }
                else if (Input.MenuDown.Pressed)
                {
                    cells[cursor_at].Deselect();

                    if (cursor_at + 5 >= cells.Count)
                        cursor_at %= 5;
                    else
                        cursor_at += 5;

                    cells[cursor_at].Select();
                }
                #endregion

                else if (Input.MenuCancel.Pressed)
                {
                    coro = SpriteBankExit();
                }
            }
        }

        public override void Render()
        {
            base.Render();
            // Draw.Rect(-10, -10, Celeste.TargetWidth + 20, Celeste.TargetHeight + 20, Color.Black * bg_alpha);
            Draw.LineAngle(bg_origin,
                    bg_rotation, Celeste.TargetWidth * 2,
                    Color.Black * bg_alpha, bg_thickness);
        }

        private IEnumerator SpriteBankEnter()
        {
            Visible = true;
            parent.stopVisibilityChecks = true;

            // add not-so-customs
            titles.Add(new("Visible to everyone", new(left_start, top_free_space), this, 0.125f + cells.Count * 0.02f));

            float defaults_y_start = top_free_space + titles.Last().total_height;

            foreach (string m in AnimationHelper.global_emotes.Keys.ToList())
            {
                cells.Add(new(AnimationHelper.GetInfo($"{m}:idle"), m, new_cell_pos(defaults_y_start), this, 0.125f + cells.Count * 0.02f)); // TODO: add fade in
            }

            // collect customs
            // why tf is it called modes here its not modes and its not even spritebanks
            // ok
            List<string> modes = GFX.SpriteBank.SpriteData.Keys
                .Where(i => !AnimationHelper.global_emotes.ContainsKey(i))
                .ToList();

            // float side_size = SpriteGridCell.default_size * SpriteGridCell.upscale;

            // add cells
            int cells_reset = cells.Count;
            Vector2 customs_start_pos = new_cell_pos(defaults_y_start, cells_reset);

            titles.Add(new("Visible to EmoteMod users", new(left_start, customs_start_pos.Y), this, 0.125f + cells.Count * 0.02f)); // TODO: add fade in

            float customs_y_start = defaults_y_start + titles.Last().total_height;
            // float customs_y_start = customs_start_pos.Y + titles.Last().total_height;

            EmoteModModule.echo($"start pos: {customs_start_pos.Y}, y start: {customs_y_start}");

            foreach (string m in modes)
            {
                // skip the ones with no animations if those even exist
                if (GFX.SpriteBank.SpriteData[m].Sprite.Animations.Count() == 0)
                    continue;

                List<string> anims = GFX.SpriteBank.SpriteData[m].Sprite.Animations.Keys.ToList();
                string anim_name = anims.Contains("idle") ? "idle" : anims.First();
                string full_name = $"{m}:{anim_name}";
                EmoteInfo info = AnimationHelper.GetInfo(full_name);

                if (info == null)
                {
                    Logger.Log(LogLevel.Info, "EmoteMod", $"could not get info for '{full_name}'");
                    continue;
                }
                else if (info.isCustom)
                {
                    Emote.addCustomEmote(full_name);
                    customsForRemoval.Add($"{full_name}");
                }

                cells.Add(new(info, m, new_cell_pos(customs_y_start, cells_reset), this, 0.125f + cells.Count * 0.02f));
                // Scene.Add(cells.Last());
            }


            // lmaoo no rotating rectangles?
            // too bad
            // metallica - fade to black

            // reset bg pos
            bg_origin = new Vector2(-bg_thickness / 2, Celeste.TargetHeight);
            bg_alpha = 0xff;

            // rotate the bg in
            for (float d = 0; d < 1; d += Engine.DeltaTime * 4)
            {
                bg_rotation = ((float)Math.PI) / 2 * (d - 1);
                bg_origin = new Vector2(
                        ((float)Math.Sin(bg_rotation)) * bg_thickness / 2,
                        Celeste.TargetHeight - bg_thickness / 2 * ((float)Math.Cos(bg_rotation))
                        );

                // EmoteModModule.echo($"bg:({bg_origin.X}, {bg_origin.Y}) at {bg_rotation}");
                yield return null;
            }

            bg_origin = new Vector2(0, 5 + Celeste.TargetHeight - bg_thickness / 2);
            bg_rotation = 0;

            parent.Visible = false;



            // obligatory wait
            // for (float d = 0; d < 1; d += Engine.DeltaTime * 8)
            //     yield return null;

            cursor_at = 0;
            cells[cursor_at].Select();


            Focused = true;

            yield return null;

        }

        private IEnumerator SpriteBankExit()
        {
            Focused = false;
            // metallica - fade to black
            for (float d = 1; d > 0; d -= Engine.DeltaTime * 12)
            {
                bg_alpha = 0xff * d;
                yield return null;

            }
            bg_alpha = 0;

            yield return null;
            parent.Focused = true;
            parent.atButton = Butt.Spritebank;
            parent.stopVisibilityChecks = false;
            parent.Visible = true;
            Clear();
            RemoveSelf();

        }

        void Init()
        {
            Tag = Tags.HUD;
            customsForRemoval = new();
            cells = new();
            titles = new();
        }

        void Clear()
        {
            foreach (string e in customsForRemoval)
            {
                try
                {
                    Emote.madeline_bp.Remove(e);
                }
                catch { }
            }
            foreach (SpriteGridCell c in cells)
            {
                c.RemoveSelf();
            }
            foreach (SpriteGridTitle t in titles)
            {
                t.RemoveSelf();
            }
        }


        /// <summary>
        /// <paramref name="spritebank"/> - the spritebank currently targeted
        /// <paramref name="emote"/> - the emote currently targeted
        /// this is for emote only
        /// </summary>
        public SpriteGrid(string spritebank, string emote, EmoteCard parent)
        {
        }
        /// <summary>
        /// <paramref name="spritebank"/> - the spritebank currently targeted
        /// this is for spritebank + emote
        /// </summary>
        public SpriteGrid(string spritebank, EmoteCard parent)
        {
            Init();
            stage = EmotePickerStage.SpriteBank;
            coro = SpriteBankEnter();
            Visible = true;
            this.parent = parent;
            parent.parent.Scene.Add(this);

            // parent.Scene.Add(this);
        }
    }
}
