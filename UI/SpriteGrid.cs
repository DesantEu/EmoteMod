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
        int cursor_at;
        public float scroll_offset = 0;

        // BG stuff
        float bg_rotation;
        float bg_alpha = 0f;
        Vector2 bg_origin;
        static float bg_thickness = Celeste.TargetWidth;

        // Spacing stuff
        public static float spacing = 70;
        float x_offset = Celeste.TargetWidth / 2 - 2.5f * SpriteGridCell.default_size * SpriteGridCell.upscale - spacing * 2;
        float y_offset = 90;

        bool Focused;

        // private int cells_reset;

        float fadeIn => Math.Min(0.125f + cells.Count * 0.02f, 0.375f); // TODO: move to SpriteGridElement constructor probably idk



        public override void Update()
        {
            base.Update();

            if (coro != null)
                coro.MoveNext();


            if (Focused)
            {
                #region Navigation
                if (Input.MenuRight.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at++;

                    if (cursor_at >= cells.Count)
                        cursor_at = 0;

                    cells[cursor_at].Select();

                    coro = Refocus();
                }
                else if (Input.MenuLeft.Pressed)
                {
                    cells[cursor_at].Deselect();

                    cursor_at--;

                    if (cursor_at < 0)
                        cursor_at = cells.Count - 1;

                    cells[cursor_at].Select();

                    coro = Refocus();
                }
                else if (Input.MenuUp.Pressed)
                {
                    int cursor_past = cursor_at;
                    cells[cursor_at].Deselect();

                    // if (cursor_at - 5 < 0)
                    // {
                    //     int temp_pos = ((int)Math.Ceiling(cells.Count / 5f)) * 5 + cursor_at - 5;
                    //     if (temp_pos >= cells.Count)
                    //         cursor_at = temp_pos - 5;
                    //     else
                    //         cursor_at = temp_pos;
                    // }
                    // else
                    // {
                    //     cursor_at -= 5;
                    //     if (cursor_at < cells_reset && cursor_past > cells_reset)
                    //         cursor_at -= cells_reset % 5;
                    // }

                    SpriteGridCell next_cell = cursor_at >= 5 //              if we have something above
                        ? cells.Where(c => //                                 find
                            c.Position.Y < cells[cursor_at].Position.Y //     something above
                            && c.Position.X == cells[cursor_at].Position.X // on the same row
                            ).Last() //                                       and pick the bottom one
                        : cells.Where(c => c.Position.X == cells[cursor_at].Position.X).Last(); // or rollover to lowest

                    cursor_at = cells.IndexOf(next_cell);

                    cells[cursor_at].Select();
                    coro = Refocus();
                }
                else if (Input.MenuDown.Pressed)
                {
                    cells[cursor_at].Deselect();

                    // if (cursor_at + 5 >= cells.Count)
                    //     cursor_at %= 5;
                    // else
                    //     cursor_at += 5;

                    SpriteGridCell next_cell = cursor_at + 5 < cells.Count // if we have something below
                        ? cells.Where(c => //                                 find
                            c.Position.Y > cells[cursor_at].Position.Y //     something below
                            && c.Position.X == cells[cursor_at].Position.X // on the same row
                            ).First() //                                      and pick the top one
                        : cells.Where(c => c.Position.X == cells[cursor_at].Position.X).First(); // or rollover to highest

                    cursor_at = cells.IndexOf(next_cell);

                    cells[cursor_at].Select();
                    coro = Refocus();
                }

                #endregion
                #region Other buttons

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
                        string sb = titles.First().text;

                        parent.emote = new(sb == "Default" ? animation : $"{sb}:{animation}", parent.emote.bind);
                        parent.emote.RefreshInfo();
                        parent.RefreshSprite();

                        parent.changesMade = true;
                        Focused = false;
                        Audio.Play("event:/new_content/ui/rename_entry_accept_locked");
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
                #endregion
            }
        }

        #region Render
        public override void Render()
        {
            base.Render();

            Draw.LineAngle(bg_origin,
                    bg_rotation, Celeste.TargetWidth * 2,
                    Color.Black * bg_alpha, bg_thickness);
        }

        #endregion
        #region Transitions

        private IEnumerator SpriteBankEnter()
        {
            Visible = true;
            parent.stopVisibilityChecks = true;

            fillWithSpriteBanks(gradualFadeIn: true);

            // lmaoo no rotating rectangles?
            // too bad
            // metallica - fade to black
            Audio.Play("event:/ui/main/button_select");
            Audio.Play("event:/ui/main/whoosh_large_in");

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
            List<string> anims;

            if (spritebank == "Default")
            {
                List<string> sbs = AnimationHelper.GlobalEmotes.Keys.ToList();
                anims = GFX.SpriteBank.SpriteData[sbs[0]].Sprite.Animations.Keys.ToList();
                for (int i = 1; i < sbs.Count(); i++)
                {
                    anims = anims.Intersect(GFX.SpriteBank.SpriteData[sbs[i]].Sprite.Animations.Keys.ToList()).ToList();
                }
            }
            else
            {
                anims = GFX.SpriteBank.SpriteData[spritebank].Sprite.Animations.Keys.ToList();
            }

            Focused = false;

            // fade out
            foreach (SpriteGridCell c in cells)
                c.FadeOut();
            foreach (SpriteGridTitle t in titles)
                t.FadeOut();

            Audio.Play("event:/ui/main/button_select");
            Audio.Play("event:/ui/main/whoosh_large_in");

            for (float d = 1; d > 0; d -= Engine.DeltaTime * 8)
                yield return null;

            Clear();
            cursor_at = 0;
            scroll_offset = 0;

            // display text and emotes
            titles.Add(new(spritebank, new(x_offset, y_offset), this));
            float cells_offset = y_offset + titles.First().total_height;

            foreach (string a in anims)
            {
                EmoteInfo info = AnimationHelper.GetInfo(spritebank == "Default" ? a : $"{spritebank}:{a}");

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

                // cells.Add(new(info, a, new_cell_pos(cells_offset), this, cells.Count * 0.02f));
                cells.Add(new(info, a, new_cell_pos(cells.Count), this, cells.Count * 0.02f));
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

        #endregion

        #region Helper methods
        // int even_ifier => cells_reset == 0 ? 0 : 5 - cells_reset % 5;

        // // Vector2 new_cell_pos(int index)
        // Vector2 new_cell_pos(float vert_offset = 0, int reset_at = 0)
        // {
        //     // float length = SpriteGridCell.upscale * SpriteGridCell.default_size;

        //     // float X = index % 5 * (length + spacing);
        //     // float Y = index % 5 * length * 1.3f;

        //     // return new Vector2(X, Y);

        //     int even_ifier = cells_reset == 0 ? 0 : 5 - cells_reset % 5;

        //     float x = left_start + (cells.Count - cells_reset) % 5 * (length + spacing);
        //     float y = (cells.Count + even_ifier) / 5 * length * 1.3f + vert_offset;

        //     return new(x, y);
        // }

        /// <summary>
        /// Returns an absolute cell pos
        /// </summary>
        /// <param name="count_from_title">cell's index starting from the last title</param>
        Vector2 new_cell_pos(int count_from_title)
        {
            float length = SpriteGridCell.upscale * SpriteGridCell.default_size;
            float y_start = titles.Last().Y + titles.Last().total_height; // this assumes there is always a title before any sprites

            // relative pos
            float X = count_from_title % 5 * (length + spacing);
            float Y = count_from_title / 5 * (length + spacing); // TODO: add text spacing too

            // absolute pos
            Vector2 abs_offset = new(x_offset, y_start);
            return new Vector2(X, Y) + abs_offset;
        }

        /// <summary>
        /// Fills the grid with CONTENT
        /// </summary>
        /// <param name="content">Pairs of { title : { display_text : animation } }</param>
        /// <param name="gradualFadeIn">Fade or no fade</param>
        void fillWithContent(Dictionary<string, Dictionary<string, string>> content, bool gradualFadeIn = false)
        {
            Vector2 abs_offset = new(x_offset, y_offset);
            // Vector2 vert_offset = Vector2.Zero;
            int title_after_ind;
            Vector2 title_rel_pos = Vector2.Zero;

            foreach (KeyValuePair<string, Dictionary<string, string>> kvp_title_spr in content)
            {
                string title = kvp_title_spr.Key;
                Dictionary<string, string> sprites = kvp_title_spr.Value;

                title_after_ind = cells.Count;


                titles.Add(new SpriteGridTitle(
                    title,
                    abs_offset + title_rel_pos,
                    this,
                    gradualFadeIn ? fadeIn : 0.125f
                ));

                // vert_offset.Y += titles.Last().total_height;

                foreach (KeyValuePair<string, string> kvp_text_anim in sprites)
                {
                    string text = kvp_text_anim.Key;
                    string animation = kvp_text_anim.Value;
                    cells.Add(new SpriteGridCell(
                        AnimationHelper.GetInfo(animation),
                        text,
                        new_cell_pos(cells.Count - title_after_ind),
                        this,
                        fadeIn

                    ));
                }

                title_rel_pos = new(0f, cells.Last().Y + cells.Last().total_height);
            }
        }

        /// <summary>
        /// generates and fills spritebank content
        /// </summary>
        /// <param name="gradualFadeIn"></param>
        void fillWithSpriteBanks(bool gradualFadeIn = false)
        {
            Audio.Play("event:/ui/main/button_select");
            Dictionary<string, Dictionary<string, string>> content = new();


            // add not-so-customs
            // titles.Add(new(
            //     "Visible to everyone",
            //     new(x_offset, y_offset),
            //     this,
            //     gradualFadeIn ? fadeIn : 0.125f)
            // );

            Dictionary<string, string> global = new();

            // offset = new(x_offset, y_offset + titles.Last().total_height);


            // cells.Add(new(
            //     AnimationHelper.GetInfo("player:idle"),
            //     "Default",
            //     new_cell_pos(0) + offset,
            //     this,
            //     gradualFadeIn ? fadeIn : 0.125f
            // ));
            global["Default"] = "player:idle";

            foreach (string gsm in AnimationHelper.GlobalEmotes.Keys.ToList())
                global[gsm] = $"{gsm}:idle";

            content["Visible to everyone:"] = global;

            // int count = 1;
            // foreach (string sm in AnimationHelper.GlobalEmotes.Keys.ToList())
            // {
            //     SpriteGridCell cell = new(
            //         AnimationHelper.GetInfo($"{sm}:idle"),
            //         sm,
            //         new_cell_pos(count) + offset,
            //         this,
            //         gradualFadeIn ? fadeIn : 0.125f
            //     );

            //     if (cells.Count > 1)
            //         cell.left = cells.Last();

            //     if (cells.Count > 5)
            //         cell.up = cells.Last().left.left.left.left.left; // this is aids

            //     cells.Add(cell);
            //     count++;
            // }


            // titles.Add(new(
            //     "Visible to Emotemod users",
            //     new_cell_pos(count + count % 5) + offset,
            //     this,
            //     gradualFadeIn ? fadeIn : 0.125f)
            // );

            // count = 0;
            // offset = titles.Last().Position + titles.Last().total_height * Vector2.UnitY;
            Dictionary<string, string> customs = new();

            List<string> sprites = GFX.SpriteBank.SpriteData.Keys // get all non-global sprites
                .Where(s => !AnimationHelper.GlobalEmotes.ContainsKey(s))
                .ToList();

            foreach (string sprite in sprites)
            {
                if (GFX.SpriteBank.SpriteData[sprite].Sprite.Animations.Count == 0) // skip if empty
                    continue;

                // choose idle or whatever
                List<string> anims = GFX.SpriteBank.SpriteData[sprite].Sprite.Animations.Keys.ToList();
                string anim_name = anims.Contains("idle") ? "idle" : anims.First();

                string full_name = $"{sprite}:{anim_name}";
                customs[sprite] = full_name;
                Emote.addCustomEmote(full_name);
                customsForRemoval.Add(full_name);
                // EmoteInfo info = AnimationHelper.GetInfo(full_name);



            }

            content["Visible to Emotemod users"] = customs;

            fillWithContent(content, gradualFadeIn);

            // // add DEFAULT
            // float defaults_y_start = top_free_space + titles.Last().total_height;
            // cells.Add(new(AnimationHelper.GetInfo("player:idle"), "Default", new_cell_pos(defaults_y_start), this,
            //             gradualFadeIn ? fadeIn : 0.125f));
            // // add the rest
            // foreach (string m in AnimationHelper.global_emotes.Keys.ToList())
            // {
            //     cells.Add(new(AnimationHelper.GetInfo($"{m}:idle"), m, new_cell_pos(defaults_y_start), this,
            //             gradualFadeIn ? fadeIn : 0.125f));
            // }

            // // collect customs
            // List<string> modes = GFX.SpriteBank.SpriteData.Keys
            //     .Where(i => !AnimationHelper.global_emotes.ContainsKey(i))
            //     .ToList();
            // cells_reset = cells.Count;
            // Vector2 customs_start_pos = new_cell_pos(defaults_y_start);

            // // add cells
            // titles.Add(new("Visible to EmoteMod users", new(left_start, customs_start_pos.Y), this,
            //             gradualFadeIn ? fadeIn : 0.125f));

            // float customs_y_start = defaults_y_start + titles.Last().total_height;
            // foreach (string m in modes)
            // {
            //     // skip the ones with no animations if those even exist
            //     if (GFX.SpriteBank.SpriteData[m].Sprite.Animations.Count() == 0)
            //         continue;

            //     List<string> anims = GFX.SpriteBank.SpriteData[m].Sprite.Animations.Keys.ToList();
            //     string anim_name = anims.Contains("idle") ? "idle" : anims.First();
            //     string full_name = $"{m}:{anim_name}";
            //     EmoteInfo info = AnimationHelper.GetInfo(full_name);

            //     if (info == null)
            //     {
            //         Logger.Log(LogLevel.Info, "EmoteMod", $"could not get info for '{full_name}'");
            //         continue;
            //     }
            //     else if (info.isCustom)
            //     {
            //         Emote.addCustomEmote(full_name);
            //         customsForRemoval.Add($"{full_name}");
            //     }

            //     cells.Add(new(info, m, new_cell_pos(customs_y_start), this,
            //             // fadeIn));
            //             gradualFadeIn ? fadeIn : 0.125f));
            // }
        }

        float get_new_shift()
        {
            // int row = cursor_at / 5 + even_ifier;

            float target;
            float new_shift;
            if (cursor_at < 5)
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
        #endregion

        #region Constructors


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
        #endregion
    }
}
