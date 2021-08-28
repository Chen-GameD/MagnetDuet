﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

namespace Rapid_Prototyping_T7.Game.Objects
{
    class Player : GameObject
    {    
        private Vector2 velocity;
        public Vector2 Velocity
        {
            get { return velocity; }
            set { velocity = value; }
        }

        public Level Level
        {
            get { return level; }
        }
        Level level;

        private float acceleration = 2500;
        private float speed_decay = 0.95f;
        private float max_speed =  250f;
        private float min_speed = 35f;

        public bool IsOnGround
        {
            get { return isOnGround; }
        }
        bool isOnGround;

        public Rectangle Rectangle
        {
            get
            {
                return new Rectangle((int)Position.X, (int)Position.Y, sprite.Width, sprite.Height);
            }
        }
        private Rectangle localBounds;

        public Rectangle BoundingRectangle
        {
            get
            {
                int left = (int)Math.Round(Position.X - (sprite.Width / 2)) + localBounds.X;
                int top = (int)Math.Round(Position.Y - sprite.Height) + localBounds.Y;

                return new Rectangle(left, top, localBounds.Width, localBounds.Height);
            }
        }


        public Player(Level level, Vector2 position)
        {
            this.level = level;

            LoadContent();

            Reset(position);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            var scale = 0.25f;
            var rotation = 0f;
            var origin = new Vector2(sprite.Width / 2, sprite.Height / 2);
            var depth = 0;
            spriteBatch.Draw(sprite,
                position, 
                null, 
                Color.White, 
                rotation,
                origin,
                scale, 
                SpriteEffects.None, 
                depth
                );
        }

        public override void Initialize()
        {
            //position = new Vector2(0, 0);
            //velocity = new Vector2(0, 0);
        }

        public override void LoadContent()
        {
            sprite = level.Content.Load<Texture2D>("Sprites/Player/Silhouette-Stick-Figure");
            int width = (int)(sprite.Width);
            int left = (sprite.Width - width) / 2;
            int height = (int)(sprite.Height);
            int top = sprite.Height - height;
            localBounds = new Rectangle(left, top, width, height);
        }

        public override void Update(GameTime gameTime)
        {
            var kstate = Keyboard.GetState();

            if (kstate.IsKeyDown(Keys.Left))
            {
                velocity.X -= acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                velocity.X = MathF.Max(velocity.X, -max_speed);
            }
            else if (kstate.IsKeyDown(Keys.Right))
            {
                velocity.X += acceleration * (float)gameTime.ElapsedGameTime.TotalSeconds;
                velocity.X = MathF.Min(velocity.X, max_speed);
            }
            else 
            { 
                velocity.X *= speed_decay;
                if (MathF.Abs(velocity.X) < min_speed)
                { 
                    velocity.X = 0.0f; 
                } 
            }

            position += velocity * (float)gameTime.ElapsedGameTime.TotalSeconds;

            HandleCollisions();

        }

        private void HandleCollisions()
        {
            // Get the player's bounding rectangle and find neighboring tiles.
            Rectangle bounds = BoundingRectangle;
            int leftTile = (int)Math.Floor((float)bounds.Left / Tile.Width);
            int rightTile = (int)Math.Ceiling(((float)bounds.Right / Tile.Width)) - 1;
            int topTile = (int)Math.Floor((float)bounds.Top / Tile.Height);
            int bottomTile = (int)Math.Ceiling(((float)bounds.Bottom / Tile.Height)) - 1;

            // Reset flag to search for ground collision.
            isOnGround = false;

            // For each potentially colliding tile,
            for (int y = topTile; y <= bottomTile; ++y)
            {
                for (int x = leftTile; x <= rightTile; ++x)
                {
                    // If this tile is collidable,
                    TileCollision collision = Level.GetCollision(x, y);
                    if (collision != TileCollision.Passable)
                    {
                        // Determine collision depth (with direction) and magnitude.
                        Rectangle tileBounds = Level.GetBounds(x, y);
                        Vector2 depth = RectangleExtensions.GetIntersectionDepth(bounds, tileBounds);
                        if (depth != Vector2.Zero)
                        {
                            float absDepthX = Math.Abs(depth.X);
                            float absDepthY = Math.Abs(depth.Y);

                            if (collision == TileCollision.Impassable) // Ignore platforms.
                            {
                                // Resolve the collision along the X axis.
                                Position = new Vector2(Position.X + depth.X, Position.Y);

                                // Perform further collisions with the new bounds.
                                bounds = BoundingRectangle;
                            }
                        }
                    }
                }
            }
        }

        public void Reset(Vector2 position)
        {
            Position = position;
            Velocity = Vector2.Zero;
        }

        //Position is the Center bottom of the sprite. So it should relocalize for the left-top corner.
        /*private Vector2 RelocalizePosition(Vector2 pos)
        {
            var posX = pos.X - (Rectangle.Width * 0.25f) / 2;
            var posY = pos.Y - Rectangle.Height * 0.25f;
            return new Vector2(posX, posY);
        }*/
    }
}
