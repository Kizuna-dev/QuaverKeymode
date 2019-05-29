﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Quaver.Server.Client;
using Quaver.Server.Client.Handlers;
using Quaver.Server.Common.Objects;
using Quaver.Server.Common.Objects.Multiplayer;
using Quaver.Shared.Assets;
using Quaver.Shared.Graphics;
using Quaver.Shared.Graphics.Backgrounds;
using Quaver.Shared.Graphics.Menu;
using Quaver.Shared.Graphics.Notifications;
using Quaver.Shared.Helpers;
using Quaver.Shared.Online;
using Quaver.Shared.Online.Chat;
using Quaver.Shared.Screens.Lobby;
using Quaver.Shared.Screens.Lobby.UI.Dialogs.Create;
using Quaver.Shared.Screens.Menu.UI.Visualizer;
using Quaver.Shared.Screens.Multiplayer.UI;
using Quaver.Shared.Screens.Multiplayer.UI.Feed;
using Quaver.Shared.Screens.Multiplayer.UI.List;
using Quaver.Shared.Screens.Select;
using Quaver.Shared.Screens.Settings;
using Wobble;
using Wobble.Assets;
using Wobble.Graphics;
using Wobble.Graphics.Animations;
using Wobble.Graphics.Sprites;
using Wobble.Graphics.UI;
using Wobble.Graphics.UI.Dialogs;
using Wobble.Logging;
using Wobble.Screens;
using Wobble.Window;

namespace Quaver.Shared.Screens.Multiplayer
{
    public class MultiplayerScreenView : ScreenView
    {
        /// <summary>
        /// </summary>
        public MultiplayerScreen MultiplayerScreen { get; }

        /// <summary>
        /// </summary>
        private BackgroundImage Background { get; set; }

        /// <summary>
        /// </summary>
        private MenuHeader Header { get; set; }

        /// <summary>
        /// </summary>
        private MenuFooter Footer { get; set; }

        /// <summary>
        /// </summary>
        public MultiplayerMap Map { get; private set; }

        /// <summary>
        /// </summary>
        private PlayerListHeader PlayerListHeader { get; }

        /// <summary>
        /// </summary>
        public PlayerList PlayerList { get; }

        /// <summary>
        /// </summary>
        public MenuAudioVisualizer Visualizer { get; }

        /// <summary>
        /// </summary>
        public MultiplayerFeed Feed { get; }

        /// <summary>
        /// </summary>
        public MultiplayerGameHeader GameTitleHeader { get; set; }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="screen"></param>
        public MultiplayerScreenView(MultiplayerScreen screen) : base(screen)
        {
            MultiplayerScreen = screen;

            CreateBackground();
            CreateHeader();
            CreateFooter();

            Visualizer = new MenuAudioVisualizer((int) WindowManager.Width, 400, 150, 5)
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Y = -Footer.Height
            };

            Visualizer.Bars.ForEach(x =>
            {
                x.Alpha = 0.30f;
            });

            CreateGameTitleHeader();
            CreateMap();

            PlayerListHeader = new PlayerListHeader(MultiplayerScreen.Game)
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                X = -24,
                Y = Header.Height + 20
            };

            // Get a list of all the online users in the game from the player ids.
            var players = new List<OnlineUser>();
            MultiplayerScreen.Game.PlayerIds.ForEach(x =>
            {
                if (!players.Contains(OnlineManager.OnlineUsers[x].OnlineUser))
                    players.Add(OnlineManager.OnlineUsers[x].OnlineUser);
            });

            PlayerList = new PlayerList(players, int.MaxValue, 0,
                new ScalableVector2(PlayerListHeader.Width, 600),
                new ScalableVector2(PlayerListHeader.Width, 600))
            {
                Parent = Container,
                Alignment = Alignment.TopRight,
                Position = new ScalableVector2(-24, PlayerListHeader.Y + PlayerListHeader.Height + 10)
            };

            Feed = new MultiplayerFeed()
            {
                Parent = Container,
                Alignment = Alignment.BotLeft,
                Position = new ScalableVector2(20, -Footer.Height - 14)
            };

            OnlineManager.Client.OnUserJoinedGame += OnUserJoinedGame;
            OnlineManager.Client.OnUserLeftGame += OnUserLeftGame;
            OnlineManager.Client.OnChatMessageReceived += OnChatMessageReceived;
            BackgroundHelper.Blurred += OnBackgroundBlurred;
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Update(GameTime gameTime) => Container?.Update(gameTime);

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        /// <param name="gameTime"></param>
        public override void Draw(GameTime gameTime)
        {
            GameBase.Game.GraphicsDevice.Clear(Color.CornflowerBlue);
            BackgroundHelper.Draw(gameTime);
            Container?.Draw(gameTime);
        }

        /// <inheritdoc />
        /// <summary>
        /// </summary>
        public override void Destroy()
        {
            OnlineManager.Client.OnUserJoinedGame -= OnUserJoinedGame;
            OnlineManager.Client.OnUserLeftGame -= OnUserLeftGame;
            OnlineManager.Client.OnChatMessageReceived -= OnChatMessageReceived;
            BackgroundHelper.Blurred -= OnBackgroundBlurred;
            Container?.Destroy();
        }

        /// <summary>
        /// </summary>
        private void CreateBackground() => Background = new BackgroundImage(UserInterface.MenuBackgroundRaw, 45, true)
        {
            Parent = Container
        };

        /// <summary>
        /// </summary>
        private void CreateHeader()
        {
            Header = new MenuHeader(FontAwesome.Get(FontAwesomeIcon.fa_earth_globe), "MULTIPLAYER", "GAME",
                "play a match together in real-time with others", ColorHelper.HexToColor("#f95186")) { Parent = Container };

            Header.Y = -Header.Height;
            Header.MoveToY(0, Easing.OutQuint, 600);
        }

        /// <summary>
        /// </summary>
        private void CreateFooter()
        {
            Footer = new MenuFooterMultiplayer(new List<ButtonText>
            {
                new ButtonText(FontsBitmap.GothamRegular, "leave game", 14, (o, e) => MultiplayerScreen.LeaveGame()),
                new ButtonText(FontsBitmap.GothamRegular, "options menu", 14, (o, e) => DialogManager.Show(new SettingsDialog())),
                new MenuFooterButtonGameChat(FontsBitmap.GothamRegular, "game chat", 14, (o, e) => ChatManager.ToggleChatOverlay(true)),
            }, new List<ButtonText>
            {
            }, ColorHelper.HexToColor("#f95186"))
            {
                Parent = Container,
                Alignment = Alignment.BotLeft
            };

            Footer.Y = Footer.Height;
            Footer.MoveToY(0, Easing.OutQuint, 600);
        }

        private void CreateGameTitleHeader() => GameTitleHeader = new MultiplayerGameHeader()
        {
            Parent = Container,
            Alignment = Alignment.TopLeft,
            X = 24,
            Y = Header.Height + 20
        };

        /// <summary>
        /// </summary>
        private void CreateMap() => Map = new MultiplayerMap((MultiplayerScreen) Screen, MultiplayerScreen.Game)
        {
            Parent = Container,
            Position = new ScalableVector2(24, GameTitleHeader.Y + GameTitleHeader.Height + 11)
        };

         /// <summary>
        ///     Called when a user has joined the multiplayer game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserJoinedGame(object sender, UserJoinedGameEventArgs e)
        {
            var user = OnlineManager.OnlineUsers[e.UserId];

            // Add the player to the player list.
            PlayerList.AddOrUpdatePlayer(user.OnlineUser);

            var log = $"{( user.HasUserInfo ? user.OnlineUser.Username : $"User#{e.UserId}" )} has joined the game.";
            Logger.Important(log, LogType.Network);

            Feed.AddItem(Color.Cyan, log);
            MultiplayerScreen.SetRichPresence();
        }

        /// <summary>
        ///     Called when a user has left the multiplayer game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnUserLeftGame(object sender, UserLeftGameEventArgs e)
        {
            if (OnlineManager.CurrentGame == null)
                return;

            var user = OnlineManager.OnlineUsers[e.UserId];

            // Remove the player from the list
            MultiplayerScreen.RemovePlayer(user.OnlineUser);

            var log = $"{(user.HasUserInfo ? user.OnlineUser.Username : $"User#{e.UserId}")} has left the game.";
            Logger.Important(log, LogType.Network);

            Feed.AddItem(Color.Cyan, log);
            MultiplayerScreen.SetRichPresence();
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnChatMessageReceived(object sender, ChatMessageEventArgs e)
        {
            if (!e.Message.Channel.StartsWith("#multi"))
                return;

            var prefix = e.Message.Channel.StartsWith("#multiplayer") ? "[CHAT]" : "[TEAM]";

            var color = Color.Yellow;

            // Team chat
            if (e.Message.Channel.StartsWith("#multi_team"))
            {
                if (OnlineManager.CurrentGame.RedTeamPlayers.Contains(e.Message.Sender.OnlineUser.Id))
                    color = Color.Crimson;
                if (OnlineManager.CurrentGame.BlueTeamPlayers.Contains(e.Message.Sender.OnlineUser.Id))
                    color = ColorHelper.HexToColor($"#4cb0f7");
            }

            Feed.AddItem(color, $"{prefix} {e.Message.SenderName}: " +
                         $"{string.Concat(e.Message.Message.Take(40))}{(e.Message.Message.Length >= 40 ? "..." : "")}" );
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnBackgroundBlurred(object sender, BackgroundBlurredEventArgs e)
        {
            Background.Image = e.Texture;
            FadeBackgroundIn();
        }

        /// <summary>
        ///     Fades the background in upon load.
        /// </summary>
        public void FadeBackgroundIn()
        {
            Background.BrightnessSprite.ClearAnimations();

            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha,
                Easing.Linear, Background.BrightnessSprite.Alpha, 0.65f, 200));
        }

        /// <summary>
        /// </summary>
        public void FadeBackgroundOut()
        {
            Background.BrightnessSprite.ClearAnimations();

            Background.BrightnessSprite.Animations.Add(new Animation(AnimationProperty.Alpha,
                Easing.Linear, Background.BrightnessSprite.Alpha, 1f, 200));
        }
    }
}