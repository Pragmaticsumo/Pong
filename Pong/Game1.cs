using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Pong
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        SpriteFont font;
        SpriteFont fontBig;
        List<SoundEffect> soundEffects;

        private Rectangle singleplayerButtonPosition;
        private Rectangle exitButtonPosition;
        private Rectangle multiplayerButtonPosition;
        private Rectangle creditsButtonPosition;
        private Rectangle backButtonPosition;

        MouseState mouseState;
        MouseState previousMouseState;

        Song music;
        Song menuMusic;
        Song multiMusic;

        Texture2D ballSprite;
        Vector2 ballPosition = Vector2.Zero;
        Vector2 ballSpeed = new Vector2(150, 150);

        Texture2D paddleSprite;
        Vector2 paddlePosition;

        Texture2D player1Sprite;
        Vector2 player1Position;

        Texture2D player2Sprite;
        Vector2 player2Position;

        int point;
        int displaySpeed = 1;

        int player1Points;
        int player2Points;

        private ScoreManager _scoreManager;
        private GameState gameState;

        enum GameState
        {
            StartMenu,
            SinglePlayer,
            MultiPlayer,
            Credits
        }

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();
            IsMouseVisible = true;

            if (font == null)
                font = Content.Load<SpriteFont>("MainMenuFont");
            if (fontBig == null)
                fontBig = Content.Load<SpriteFont>("MainMenuFontBig");

            soundEffects = new List<SoundEffect>();
            soundEffects.Add(Content.Load<SoundEffect>("MenuItemSelect"));
            soundEffects.Add(Content.Load<SoundEffect>("MenuItem"));
            soundEffects.Add(Content.Load<SoundEffect>("Pop"));

            menuMusic = Content.Load<Song>("skye-cuillin-by-kevin-macleod-from-filmmusic-io");
            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(menuMusic);

            gameState = GameState.StartMenu;

            mouseState = Mouse.GetState();
            previousMouseState = mouseState;
            base.Initialize();
        }

        void LoadSinglePlayer()
        {
            music = Content.Load<Song>("Cipher");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(music);
            gameState = GameState.SinglePlayer;

            point = 0;

            if (font == null)
                font = Content.Load<SpriteFont>("MainMenuFont");

            ballSprite = Content.Load<Texture2D>("Ball_Idle");
            paddleSprite = Content.Load<Texture2D>("Paddle");
            paddlePosition = new Vector2(800 / 2, 600 - paddleSprite.Height);
        }

        void LoadMultiPlayer()
        {
            multiMusic = Content.Load<Song>("pippin-the-hunchback-by-kevin-macleod-from-filmmusic-io");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(multiMusic);

            gameState = GameState.MultiPlayer;

            ballSprite = Content.Load<Texture2D>("Ball_Idle");
            player1Sprite = Content.Load<Texture2D>("Paddle");
            player1Position = new Vector2(800 / 2, 600 - player1Sprite.Height);
            player2Sprite = Content.Load<Texture2D>("Paddle");
            player2Position = new Vector2(800 / 2, 0);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (gameState == GameState.SinglePlayer)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    _scoreManager.Add(new Score()
                    {
                        Value = point,
                    }
           );
                    gameState = GameState.StartMenu;
                    MediaPlayer.Stop();
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(menuMusic);
                }

                Rectangle paddleRect = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, paddleSprite.Width, paddleSprite.Height);
                Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballSprite.Width, ballSprite.Height);

                ballPosition += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                float maxX = 800 - ballSprite.Width;
                float maxY = 600 - ballSprite.Height;

                float screenWidth = 800 - paddleSprite.Width;

                if (ballPosition.X > maxX || ballPosition.X < 0)
                {
                    ballSpeed.X *= -1;
                    soundEffects[2].CreateInstance().Play();
                }

                if (ballPosition.Y < 0)
                {
                    ballSpeed.Y *= -1;
                    soundEffects[2].CreateInstance().Play();
                }
                else if (ballPosition.Y > maxY)
                {
                    _scoreManager.Add(new Score()
                    {
                        Value = point,
                    }
           );

                    ScoreManager.Save(_scoreManager);

                    ballPosition.Y = 0;
                    ballPosition.X = 0;
                    ballSpeed.X = 150;
                    ballSpeed.Y = 150;
                    point = 0;
                    displaySpeed = 1;
                }

                if (paddlePosition.X < screenWidth || paddlePosition.X > 0)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D))
                    {
                        paddlePosition.X += 8;
                        if (Keyboard.GetState().IsKeyDown(Keys.P))
                        {
                            paddlePosition.X += 10;
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.L))
                        {
                            paddlePosition.X += 20;
                        }

                        if (paddlePosition.X > screenWidth)
                        {
                            paddlePosition.X = screenWidth;
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A))
                    {
                        paddlePosition.X -= 8;
                        if (Keyboard.GetState().IsKeyDown(Keys.P))
                        {
                            paddlePosition.X -= 10;
                        }
                        else if (Keyboard.GetState().IsKeyDown(Keys.L))
                        {
                            paddlePosition.X -= 20;
                        }

                        if (paddlePosition.X < 0)
                        {
                            paddlePosition.X = 0;
                        }
                    }
                }

                if (ballRect.Intersects(paddleRect))
                {
                    point++;
                    soundEffects[1].CreateInstance().Play();

                    if (ballSpeed.X != 500)
                    {
                        if (displaySpeed != 50)
                        {
                            displaySpeed++;
                        }
                        ballSpeed.Y += 7;

                        if (ballSpeed.X < 0)
                            ballSpeed.X -= 7;
                        else
                            ballSpeed.X += 7;
                    }

                    ballSpeed.Y *= -1;
                    ballPosition.Y = paddleRect.Y - 53;
                }
            }

            if (gameState == GameState.MultiPlayer)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    gameState = GameState.StartMenu;
                    MediaPlayer.Stop();
                    MediaPlayer.IsRepeating = true;
                    MediaPlayer.Play(menuMusic);
                }

                Rectangle player1Rect = new Rectangle((int)player1Position.X, (int)player1Position.Y, player1Sprite.Width, player1Sprite.Height);
                Rectangle player2Rect = new Rectangle((int)player2Position.X, (int)player2Position.Y, player2Sprite.Width, player2Sprite.Height);
                Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballSprite.Width, ballSprite.Height);

                ballPosition += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

                float maxX = 800 - ballSprite.Width;
                float maxY = 600 - ballSprite.Height;

                float screenWidth = 800 - player1Sprite.Width;

                if (ballPosition.X > maxX || ballPosition.X < 0)
                {
                    ballSpeed.X *= -1;
                    soundEffects[2].CreateInstance().Play();
                }

                if (ballPosition.Y < 0)
                {
                    player1Points++;
                    ballPosition = new Vector2(0, 552);
                }
                else if (ballPosition.Y > maxY)
                {
                    player2Points++;
                    ballPosition = new Vector2(0, 0);
                }

                if (player1Position.X < screenWidth || player1Position.X > 0)
                {
                    if ( Keyboard.GetState().IsKeyDown(Keys.D))
                    {
                        player1Position.X += 8;

                        if (player1Position.X > screenWidth)
                        {
                            player1Position.X = screenWidth;
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.A))
                    {
                        player1Position.X -= 8;

                        if (player1Position.X < 0)
                        {
                            player1Position.X = 0;
                        }
                    }
                }

                if (player2Position.X < screenWidth || player2Position.X > 0)
                {
                    if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    {
                        player2Position.X += 8;

                        if (player2Position.X > screenWidth)
                        {
                            player2Position.X = screenWidth;
                        }
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    {
                        player2Position.X -= 8;

                        if (player2Position.X < 0)
                        {
                            player2Position.X = 0;
                        }
                    }
                }

                if (ballRect.Intersects(player1Rect))
                {
                    soundEffects[1].CreateInstance().Play();

                    ballSpeed.Y *= -1;
                    ballPosition.Y = player1Rect.Y - 53;
                }

                if (ballRect.Intersects(player2Rect))
                {
                    soundEffects[1].CreateInstance().Play();

                    ballSpeed.Y *= -1;
                    ballPosition.Y = player2Rect.Y + 25;
                }
            }

            if (gameState == GameState.Credits)
            {
                if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    gameState = GameState.StartMenu;
                }
            }

            mouseState = Mouse.GetState();
            if (previousMouseState.LeftButton == ButtonState.Pressed && mouseState.LeftButton == ButtonState.Released)
            {
                MouseClicked(mouseState.X, mouseState.Y);
            }
            previousMouseState = mouseState;
        }

        void MouseClicked(int x, int y)
        {
            Rectangle mouseClickRect = new Rectangle(x, y, 1, 1);
            if (gameState == GameState.StartMenu)
            {
                Vector2 quitBounds = font.MeasureString("QUIT");
                Vector2 creditsBounds = font.MeasureString("CREDITS");
                Vector2 singlePlayerBounds = font.MeasureString("SINGLEPLAYER");
                Vector2 multiPlayerBounds = font.MeasureString("MULTIPLAYER");

                singleplayerButtonPosition = new Rectangle((int)(800 / 2 - singlePlayerBounds.X / 2), 175, (int)singlePlayerBounds.X, 14);
                multiplayerButtonPosition = new Rectangle((int)(800 / 2 - multiPlayerBounds.X / 2), 200, (int)multiPlayerBounds.X, 14);
                creditsButtonPosition = new Rectangle((int)(800 / 2 - creditsBounds.X / 2), 225, (int)creditsBounds.X, 14);
                exitButtonPosition = new Rectangle((int)(800 / 2 - quitBounds.X / 2), 250, (int)quitBounds.X, 14);

                if (mouseClickRect.Intersects(singleplayerButtonPosition))
                {
                    soundEffects[0].CreateInstance().Play();
                    LoadSinglePlayer();
                    ballPosition = new Vector2(0, 0);
                }
                else if (mouseClickRect.Intersects(exitButtonPosition))
                {
                    soundEffects[0].CreateInstance().Play();
                    Exit();
                }
                else if (mouseClickRect.Intersects(creditsButtonPosition))
                {
                    soundEffects[0].CreateInstance().Play();
                    gameState = GameState.Credits;
                }
                else if (mouseClickRect.Intersects(multiplayerButtonPosition))
                {
                    soundEffects[0].CreateInstance().Play();
                    ballSpeed = new Vector2(150, 150);
                    ballPosition = new Vector2(0, 0);
                    player1Points = 0;
                    player2Points = 0;
                    LoadMultiPlayer();
                }
            }

            if (gameState == GameState.Credits)
            {
                Vector2 backButtonBounds = font.MeasureString("BACK");

                backButtonPosition = new Rectangle((int)(800 / 2 - backButtonBounds.X / 2), 575, (int)backButtonBounds.X, 14);

                if (mouseClickRect.Intersects(backButtonPosition))
                {
                    soundEffects[0].CreateInstance().Play();
                    gameState = GameState.StartMenu;
                }
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            Point mousePoint = new Point(mouseState.Position.X, mouseState.Position.Y);
            Vector2 nellieBounds = fontBig.MeasureString("NELLIE PONG");
            Vector2 quitBounds = font.MeasureString("QUIT");
            Vector2 creditsBounds = font.MeasureString("CREDITS");
            Vector2 singlePlayerBounds = font.MeasureString("SINGLEPLAYER");
            Vector2 multiPlayerBounds = font.MeasureString("MULTIPLAYER");
            Vector2 backButtonBounds = font.MeasureString("BACK");
            Vector2 highscoreBounds = font.MeasureString("HIGHSCORES:");
            _scoreManager = ScoreManager.Load();
            if (gameState == GameState.StartMenu)
            {
                _spriteBatch.Begin();

                _spriteBatch.DrawString(font, "HIGHSCORES:\n" + string.Join("\n", _scoreManager.Highscores.Select(c => c.Value).ToArray()), new Vector2(0, 0), Color.Gold);

                _spriteBatch.DrawString(fontBig, "NELLIE PONG", new Vector2(800 / 2 - nellieBounds.X / 2, 0), Color.LightSteelBlue);

                _spriteBatch.DrawString(font, "version 3.0", new Vector2(0, 575), Color.White);

                if (singleplayerButtonPosition.Contains(mousePoint))
                {
                    _spriteBatch.DrawString(font, "SINGLEPLAYER", new Vector2(800 / 2 - singlePlayerBounds.X / 2, 175), Color.Red);
                }
                else
                {
                    _spriteBatch.DrawString(font, "SINGLEPLAYER", new Vector2(800 / 2 - singlePlayerBounds.X / 2, 175), Color.White);
                }

                if (multiplayerButtonPosition.Contains(mousePoint))
                {
                    _spriteBatch.DrawString(font, "MULTIPLAYER", new Vector2(800 / 2 - multiPlayerBounds.X / 2, 200), Color.Red);
                }
                else
                {
                    _spriteBatch.DrawString(font, "MULTIPLAYER", new Vector2(800 / 2 - multiPlayerBounds.X / 2, 200), Color.White);
                }

                if (creditsButtonPosition.Contains(mousePoint))
                {
                    _spriteBatch.DrawString(font, "CREDITS", new Vector2(800 / 2 - creditsBounds.X / 2, 225), Color.Red);
                }
                else
                {
                    _spriteBatch.DrawString(font, "CREDITS", new Vector2(800 / 2 - creditsBounds.X / 2, 225), Color.White);
                }

                if (exitButtonPosition.Contains(mousePoint))
                {
                    _spriteBatch.DrawString(font, "QUIT", new Vector2(800 / 2 - quitBounds.X / 2, 250), Color.Red);
                }
                else
                {
                    _spriteBatch.DrawString(font, "QUIT", new Vector2(800 / 2 - quitBounds.X / 2, 250), Color.White);
                }
                _spriteBatch.End();
            }

            if (gameState == GameState.SinglePlayer)
            {
                _spriteBatch.Begin();

                _spriteBatch.Draw(ballSprite, ballPosition, Color.White);
                _spriteBatch.Draw(paddleSprite, paddlePosition, Color.White);

                _spriteBatch.DrawString(font, "PRESS ESC TO RETURN TO THE MAIN MENU", new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(font, "\nHOLD P KEY TO BOOST", new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(font, "\n\nHOLD L KEY TO TURBO BOOST", new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(font, "\n\n\nPOINTS: " + point, new Vector2(0, 0), Color.White);
                _spriteBatch.DrawString(font, "\n\n\n\nBALL SPEED: " + displaySpeed, new Vector2(0, 0), Color.White);
                _spriteBatch.End();
            }

            if (gameState == GameState.MultiPlayer)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(player1Sprite, player1Position, Color.White);
                _spriteBatch.Draw(player2Sprite, player2Position, Color.White);
                _spriteBatch.Draw(ballSprite, ballPosition, Color.White);
                _spriteBatch.DrawString(font, "PLAYER 1 SCORE: " + player1Points, new Vector2(0, 550), Color.White);
                _spriteBatch.DrawString(font, "PLAYER 2 SCORE: " + player2Points, new Vector2(0, 25), Color.White);
                _spriteBatch.End();
            }

            if (gameState == GameState.Credits)
            {
                Vector2 bounds1 = font.MeasureString("CODING, ART, AND EVERYTHING ELSE BY NELLIE GAMES");
                Vector2 bounds2 = font.MeasureString("NELLIE GAMES IS COMPRISED OF ONE PERSON, JOSEPH EGAN");
                Vector2 bounds3 = font.MeasureString("THIS WOULDNT BE POSSIBLE WITHOUT THE SUPPORT OF YOU!");
                Vector2 bounds4 = font.MeasureString("MUSIC BY KEVIN MACLEOD");
                Vector2 bounds5 = font.MeasureString("Skye Cuillin by Kevin MacLeod Link: https://incompetech.filmmusic.io/song/4371-skye-cuillin");
                Vector2 bounds6 = font.MeasureString("Cipher Kevin MacLeod (incompetech.com) Licensed under Creative Commons:");

                _spriteBatch.Begin();
                _spriteBatch.DrawString(font, "CODING, ART, AND EVERYTHING ELSE BY NELLIE GAMES", new Vector2(800 / 2 - bounds1.X / 2, 100), Color.White);
                _spriteBatch.DrawString(font, "\n\nNELLIE GAMES IS COMPRISED OF ONE PERSON, JOSEPH EGAN", new Vector2(800 / 2 - bounds2.X / 2, 100), Color.White);
                _spriteBatch.DrawString(font, "\n\n\n\nMUSIC BY KEVIN MACLEOD", new Vector2(800 / 2 - bounds4.X / 2, 100), Color.White);
                _spriteBatch.DrawString(font, "\n\n\n\n\nSkye Cuillin by Kevin MacLeod Link: https://incompetech.filmmusic.io/song/4371-skye-cuillin \nLicense: https://filmmusic.io/standard-license", new Vector2(800 / 2 - bounds5.X / 2, 100), Color.White);
                _spriteBatch.DrawString(font, "\n\n\n\n\n\n\n\nPippin the Hunchback by Kevin MacLeod \nLink: https://incompetech.filmmusic.io/song/4219-pippin-the-hunchback \nLicense: https://filmmusic.io/standard-license", new Vector2(800 / 2 - bounds6.X / 2, 100), Color.White);
                _spriteBatch.DrawString(font, "\n\n\n\n\n\n\n\n\n\n\n\n Cipher Kevin MacLeod (incompetech.com) Licensed under Creative Commons:\n By Attribution 3.0 License http://creativecommons.org/licenses/by/3.0/", new Vector2(800 / 2 - bounds6.X / 2, 100), Color.White);
                _spriteBatch.DrawString(font, "\n\n\n\n\n\n\n\n\n\n\n\n\n\n\n\nTHIS WOULDNT BE POSSIBLE WITHOUT THE SUPPORT OF YOU!", new Vector2(800 / 2 - bounds3.X / 2, 100), Color.Goldenrod);
                if (backButtonPosition.Contains(mousePoint))
                {
                    _spriteBatch.DrawString(font, "Back", new Vector2((800 / 2 - backButtonBounds.X / 2), 575), Color.Red);
                }
                else
                {
                    _spriteBatch.DrawString(font, "Back", new Vector2((800 / 2 - backButtonBounds.X / 2), 575), Color.White);
                }
                _spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
