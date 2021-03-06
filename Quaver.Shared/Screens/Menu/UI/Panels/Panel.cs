/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Helpers;
using Wobble.Graphics;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI.Buttons;

namespace Quaver.Shared.Screens.Menu.UI.Panels
{
    public class Panel : Button
    {
        /// <summary>
        ///     The thumbnail image for the panel.
        /// </summary>
        public Sprite Thumbnail { get; private set; }

        /// <summary>
        ///     Contains the heading for the panel.
        /// </summary>
        public Sprite HeadingContainer { get; private set; }

        /// <summary>
        ///     The title of the panel.
        /// </summary>
        public SpriteTextBitmap Title { get; private set; }

        /// <summary>
        ///     The description of the panel.
        /// </summary>
        public SpriteTextBitmap Description { get; private set; }

        /// <summary>
        ///     The original size of the panel.
        /// </summary>
        private ScalableVector2 OriginalSize { get; } = new ScalableVector2(310, 310);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="title"></param>
        /// <param name="description"></param>
        /// <param name="activeThumbnail"></param>
        /// <param name="onClick"></param>
        public Panel(string title, string description, Texture2D activeThumbnail, EventHandler onClick = null) : base (onClick)
        {
            Size = new ScalableVector2(OriginalSize.X.Value, OriginalSize.Y.Value);

            CreateThumbnail(activeThumbnail);
            CreateHeadingContainer();
            CreateTitleText(title);
            CreateDescriptionText(description);

            AddBorder(Color.White, 2);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime)
        {
            PerformHoverAnimation(gameTime);
            base.Update(gameTime);
        }

        /// <summary>
        ///     Performs the border and size hover animation for the panel.
        /// </summary>
        /// <param name="gameTime"></param>
        private void PerformHoverAnimation(GameTime gameTime)
        {
            var dt = gameTime.ElapsedGameTime.TotalMilliseconds;

            if (IsHovered)
            {
                Width = MathHelper.Lerp(Width, OriginalSize.X.Value * 1.08f + 2, (float) Math.Min(dt / 30, 1));
                Height = MathHelper.Lerp(Height, OriginalSize.Y.Value * 1.08f + 2, (float) Math.Min(dt / 30, 1));

                Border.Thickness = MathHelper.Lerp(Border.Thickness, 4, (float) Math.Min(dt / 30, 1));
                Border.FadeToColor(Color.Yellow, dt, 30);

                // Resetting the parent allows the panel to go on top of the other ones (changes draw order)
                Parent = Parent;
            }
            else
            {
                Width = MathHelper.Lerp(Width, OriginalSize.X.Value, (float) Math.Min(dt / 30, 1));
                Height = MathHelper.Lerp(Height, OriginalSize.Y.Value, (float) Math.Min(dt / 30, 1));

                Border.Thickness = MathHelper.Lerp(Border.Thickness, 2, (float) Math.Min(dt / 30, 1));
                Border.FadeToColor(Color.White, dt, 30);
            }

            // Always make sure thumbnail is at the correct size
            Thumbnail.Width = Width;
            Thumbnail.Height = Height;

            // Always make sure heading container is at the correct size.
            HeadingContainer.Width = Width;
            HeadingContainer.Height = 100;
        }

        /// <summary>
        ///     Creates the thumbnail sprite.
        /// </summary>
        /// <param name="image"></param>
        private void CreateThumbnail(Texture2D image)
        {
            Thumbnail = new Sprite()
            {
                Parent = this,
                Size = new ScalableVector2(Width, OriginalSize.Y.Value),
                Image = image,
            };
        }

        /// <summary>
        ///     Creates the heading container sprite.
        /// </summary>
        private void CreateHeadingContainer() => HeadingContainer = new Sprite()
        {
            Parent = this,
            Alignment = Alignment.BotLeft,
            Size = new ScalableVector2(Width, 100),
            Tint = ColorHelper.HexToColor("#0f0f0f"),
            Alpha = 0.6f
        };

        /// <summary>
        ///     Creates the text that displays the title of the panel.
        /// </summary>
        /// <param name="title"></param>
        private void CreateTitleText(string title) => Title = new SpriteTextBitmap(FontsBitmap.GothamBold, title.ToUpper())
        {
            Parent = HeadingContainer,
            Alignment = Alignment.TopLeft,
            X = 12,
            Y = 10,
            Tint = Color.White,
            FontSize = 24,
            MaxWidth = (int)Width
        };

        /// <summary>
        ///     Creates the text that displays the description of the panel.
        /// </summary>
        /// <param name="description"></param>
        private void CreateDescriptionText(string description) => Description = new SpriteTextBitmap(FontsBitmap.GothamRegular,
            description)
        {
            Parent = HeadingContainer,
            Alignment = Alignment.TopLeft,
            X = 12,
            Y = Title.Y + Title.Height + 8,
            Tint = Color.White,
            FontSize = 16,
            MaxWidth = (int) Width - 30
        };
    }
}
