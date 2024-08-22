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
    public enum Stages
    {
        SpriteBank,
        Emote,
        Done
    }

    public class SpriteGrid : Entity
    {
        IEnumerator coro;
        Stages stage;
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
        public float scroll_offset = 0;
        float left_start = Celeste.TargetWidth / 2 - 2.5f * SpriteGridCell.default_size * SpriteGridCell.upscale - spacing * 2;
        int cursor_at;

        float first_cell_y;

        bool Focused;

        float top_free_space = 90;
        float fadeIn => Math.Min(0.125f + cells.Count * 0.02f, 0.375f);

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
                // scroll
                // scroll_offset = cells[cursor_at].Position.Y - first_cell_y;

                #region navigation
                if (Input.MenuRight.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at++;
                    if (cursor_at >= cells.Count)
                        cursor_at = 0;

                    cells[cursor_at].Select();

                    if (cursor_at % 5 == 0)
                        coro = Refocus();
                }
                else if (Input.MenuLeft.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at--;
                    if (cursor_at < 0)
                        cursor_at = cells.Count - 1;

                    cells[cursor_at].Select();

                    if (cursor_at % 5 == 4)
                        coro = Refocus();
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
                    coro = Refocus();
                }
                else if (Input.MenuDown.Pressed)
                {
                    cells[cursor_at].Deselect();

                    if (cursor_at + 5 >= cells.Count)
                        cursor_at %= 5;
                    else
                        cursor_at += 5;

                    cells[cursor_at].Select();
                    coro = Refocus();
                }
                #endregion

                else if (Input.MenuConfirm.Pressed)
                {
                    if (stage == Stages.SpriteBank)
                    {
                        cells[cursor_at].Deselect();
                        coro = SpriteBankToEmote();
                    }
                    else if (stage == Stages.Emote)
                    {
                        SpriteGridCell selected = cells[cursor_at];

                        string animation = selected.info.animation;
                        string sb = selected.info.spritebank;

                        parent.emote = new(sb == "Default" ? animation : $"{sb}:{animation}", parent.emote.bind);
                        parent.emote.RefreshInfo();
                        parent.RefreshSprite();

                        parent.changesMade = true;
                        Focused = false;
                        coro = EmoteExit();
                    }
                }
                else if (Input.MenuCancel.Pressed)
                {
                    if (stage == Stages.SpriteBank)
                        coro = SpriteBankExit();
                    else if (stage == Stages.Emote)
                        coro = EmoteToSpritebank();
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

        void fillWithSpriteBanks(bool gradualFadeIn = false)
        {
            // add not-so-customs
            titles.Add(new("Visible to everyone", new(left_start, top_free_space), this,
                        // gradualFadeIn ? 0.125f + cells.Count * 0.02f : 0.125f));
                        gradualFadeIn ? fadeIn : 0.125f));

            float defaults_y_start = top_free_space + titles.Last().total_height;

            foreach (string m in AnimationHelper.global_emotes.Keys.ToList())
            {
                cells.Add(new(AnimationHelper.GetInfo($"{m}:idle"), m, new_cell_pos(defaults_y_start), this,
                        // 0.125f + cells.Count * 0.02f));
                        gradualFadeIn ? fadeIn : 0.125f));
            }

            first_cell_y = cells.First().Position.Y;

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

            titles.Add(new("Visible to EmoteMod users", new(left_start, customs_start_pos.Y), this,
                        // fadeIn));
                        gradualFadeIn ? fadeIn : 0.125f));

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

                cells.Add(new(info, m, new_cell_pos(customs_y_start, cells_reset), this,
                        // fadeIn));
                        gradualFadeIn ? fadeIn : 0.125f));
            }
        }

        private IEnumerator SpriteBankEnter()
        {
            Visible = true;
            parent.stopVisibilityChecks = true;

            fillWithSpriteBanks(gradualFadeIn: true);

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
            for (float d = 0; d < 1; d += Engine.DeltaTime * 8)
                yield return null;

            cursor_at = cells.FindIndex(c => c.info.spritebank == parent.info.spritebank);
            if (cursor_at < 0)
                cursor_at = 0;

            cells[cursor_at].Select();


            Focused = true;

            yield return null;

            if (cursor_at != 0)
                coro = Refocus();

        }

        private IEnumerator SpriteBankToEmote()
        {
            // get info
            string spritebank = cells[cursor_at].text;
            List<string> anims = GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations.Keys.ToList();

            Focused = false;

            // fade out
            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            Clear();
            cursor_at = 0;
            scroll_offset = 0;

            // display text and emotes
            titles.Add(new(spritebank, new(left_start, top_free_space), this));
            float cells_offset = top_free_space + titles.First().total_height;

            foreach (string a in anims)
            {
                EmoteInfo info = AnimationHelper.GetInfo($"{spritebank}:{a}");

                if (info == null)
                {
                    Logger.Log(LogLevel.Info, "EmoteMod", $"could not get info for '{a}'");
                    continue;
                }
                else if (info.isCustom)
                {
                    Emote.addCustomEmote($"{spritebank}:{a}");
                    customsForRemoval.Add($"{spritebank}:{a}");
                }

                cells.Add(new(info, a, new_cell_pos(cells_offset), this, cells.Count * 0.02f));
            }

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            stage = Stages.Emote;

            cells[cursor_at].Select();
            Focused = true;

            yield return null;

        }

        private IEnumerator EmoteToSpritebank()
        {
            string spritebank = titles.First().text;
            Focused = false;

            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            Clear();

            fillWithSpriteBanks();

            cursor_at = cells.FindIndex(c => c.info.spritebank == spritebank);
            if (cursor_at < 0)
                cursor_at = 0;

            scroll_offset = get_new_shift();

            // obligatory wait
            for (float d = 0; d < 1; d += Engine.DeltaTime * 8)
                yield return null;

            stage = Stages.SpriteBank;
            cells[cursor_at].Select();
            Focused = true;
            yield return null;
        }

        private IEnumerator EmoteExit()
        {
            Focused = false;

            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 16)
                yield return null;

            parent.Visible = true;

            // remove bg
            bg_origin = new Vector2(Celeste.TargetWidth, Celeste.TargetHeight - bg_thickness / 2);
            bg_alpha = 0xff;

            // rotate the bg in
            for (float d = 0; d < 1; d += Engine.DeltaTime * 4)
            {
                bg_rotation = ((float)Math.PI) * (1.5f - 0.5f * (1 - d));
                bg_origin = new Vector2(
                        Celeste.TargetWidth - ((float)Math.Sin(bg_rotation)) * bg_thickness / 2,
                        Celeste.TargetHeight + bg_thickness / 2 * ((float)Math.Cos(bg_rotation))
                        );

                EmoteModModule.echo(Math.Cos(bg_rotation).ToString());
                yield return null;
            }

            bg_origin = new Vector2(Celeste.TargetWidth + bg_thickness / 2, Celeste.TargetHeight);
            bg_rotation = ((float)Math.PI) * 2;
            bg_alpha = 0f;


            Clear();

            parent.Focused = true;
            parent.atButton = Butt.Animation;
            parent.stopVisibilityChecks = false;
            RemoveSelf();
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

        float get_new_shift()
        {
            int row = cursor_at / 5;

            float target;
            float new_shift;
            if (row == 0)
            {
                target = 0;
                new_shift = 0;
            }
            else
            {
                target = cells[cursor_at].Position.Y - Celeste.TargetHeight / 2;
                new_shift = target + SpriteGridCell.default_size / 2 * SpriteGridCell.upscale;
            }
            return new_shift;
        }

        IEnumerator Refocus()
        {
            float old_shift = scroll_offset;

            float new_shift = get_new_shift();

            for (float d = 1f; d > 0f; d -= Engine.DeltaTime * 4)
            {
                scroll_offset = new_shift - (new_shift - old_shift) * Ease.CubeIn(d);
                // for (int i = 0; i < cards.Count; i++)
                // {
                //     cards[i].Position = new Vector2(Celeste.TargetWidth / 2,
                //             cards_shift + i * 310f);
                // }

                yield return null;
            }
            scroll_offset = new_shift;
        }

        void Init()
        {
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
            customsForRemoval.Clear();

            foreach (SpriteGridCell c in cells)
            {
                c.RemoveSelf();
            }
            cells.Clear();

            foreach (SpriteGridTitle t in titles)
            {
                t.RemoveSelf();
            }
            titles.Clear();
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
            stage = Stages.SpriteBank;
            coro = SpriteBankEnter();
            Visible = true;
            this.parent = parent;
            Tag = parent.Tag;
            parent.parent.Scene.Add(this);

            // parent.Scene.Add(this);
        }
    }
}
