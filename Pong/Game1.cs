using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        List<SoundEffect> soundEffects;
        Song music;

        Texture2D black;

        Texture2D ballSprite;
        Vector2 ballPosition = Vector2.Zero;
        Vector2 ballSpeed = new Vector2(150, 150);

        Texture2D paddleSprite;
        Vector2 paddlePosition;

        int point;
        int displaySpeed = 1;

        private ScoreManager _scoreManager;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 600;
            _graphics.ApplyChanges();
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _scoreManager = ScoreManager.Load();
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            base.LoadContent();

            soundEffects = new List<SoundEffect>();

            point = 0;

            if (font == null)
                font = Content.Load<SpriteFont>("MainMenuFont");

            black = Content.Load<Texture2D>("Fade_Black");
            ballSprite = Content.Load<Texture2D>("Ball_Idle");
            paddleSprite = Content.Load<Texture2D>("Paddle");
            paddlePosition = new Vector2(800 / 2, 600 - paddleSprite.Height);

            soundEffects.Add(Content.Load<SoundEffect>("MenuItem"));
            soundEffects.Add(Content.Load<SoundEffect>("Pop"));
            music = Content.Load<Song>("Cipher");

            MediaPlayer.IsRepeating = true;
            MediaPlayer.Play(music);
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.B))
                Exit();

            Rectangle paddleRect = new Rectangle((int)paddlePosition.X, (int)paddlePosition.Y, paddleSprite.Width, paddleSprite.Height);
            Rectangle ballRect = new Rectangle((int)ballPosition.X, (int)ballPosition.Y, ballSprite.Width, ballSprite.Height);

            ballPosition += ballSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            float maxX = 800 - ballSprite.Width;
            float maxY = 600 - ballSprite.Height;

            float screenWidth = 800 - paddleSprite.Width;

            if (ballPosition.X > maxX || ballPosition.X < 0)
            {
                ballSpeed.X *= -1;
                soundEffects[1].CreateInstance().Play();
            }

            if (ballPosition.Y < 0)
            {
                ballSpeed.Y *= -1;
                soundEffects[1].CreateInstance().Play();
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
                if (Keyboard.GetState().IsKeyDown(Keys.Right) || Keyboard.GetState().IsKeyDown(Keys.D) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadRight) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickRight))
                {
                    paddlePosition.X += 8;
                    if(Keyboard.GetState().IsKeyDown(Keys.P) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Y))
                    {
                        paddlePosition.X += 10;
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.L) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.X))
                    {
                        paddlePosition.X += 20;
                    }

                    if (paddlePosition.X > screenWidth)
                    {
                        paddlePosition.X = screenWidth;
                    }
                }
                else if (Keyboard.GetState().IsKeyDown(Keys.Left) || Keyboard.GetState().IsKeyDown(Keys.A) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.DPadLeft) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.LeftThumbstickLeft))
                {
                    paddlePosition.X -= 8;
                    if (Keyboard.GetState().IsKeyDown(Keys.P) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.Y))
                    {
                        paddlePosition.X -= 10;
                    }
                    else if (Keyboard.GetState().IsKeyDown(Keys.L) || GamePad.GetState(PlayerIndex.One).IsButtonDown(Buttons.X))
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
                soundEffects[0].CreateInstance().Play();

                if (ballSpeed.X != 500)
                {
                    displaySpeed++;
                    ballSpeed.Y += 7;

                    if (ballSpeed.X < 0)
                        ballSpeed.X -= 7;
                    else
                        ballSpeed.X += 7;
                }

                ballSpeed.Y *= -1;
                ballPosition.Y = paddleRect.Y - 53;

                base.Update(gameTime);
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            _spriteBatch.Draw(black, new Rectangle(0, 0, 800, 600), Color.White);

            _spriteBatch.Draw(ballSprite, ballPosition, Color.White);
            _spriteBatch.Draw(paddleSprite, paddlePosition, Color.White);

            _spriteBatch.DrawString(font, "PRESS ESC KEY/B BUTTON TO QUIT", new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(font, "\nHOLD P KEY/Y BUTTON TO BOOST", new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(font, "\n\nHOLD L KEY/X BUTTON TO TURBO BOOST", new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(font, "\n\n\nPOINTS: " + point, new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(font, "\n\n\n\nHIGHSCORE: " + string.Join("\n", _scoreManager.Highscores.Select(c => c.Value).ToArray()), new Vector2(0, 0), Color.White);
            _spriteBatch.DrawString(font, "\n\n\n\n\nBALL SPEED: " + displaySpeed, new Vector2(0, 0), Color.White);
            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
