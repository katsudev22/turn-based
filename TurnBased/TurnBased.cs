using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace turn_based_test.TurnBased;

public static class Globals
{
    public static float Time { get; set; }
    public static ContentManager ContentManager { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static Point GameWindow { get; set; } = new(960, 540);
    public static Point VirtualGameWindow { get; set; } = new(480, 270);

    public static void Update(GameTime gameTime)
    {
        Time = (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}

public class GameManager
{
    public BattleManager battleManager;

    public GameManager()
    {
        battleManager = new();
    }
    
    public void Update()
    {
        battleManager.Update();
    }

    public void Draw()
    {
        battleManager.Draw();
    }
}

public class BattleManager
{
    public UiManager uiManager;
    public Player player;
    public Enemy enemy;

    public BattleManager()
    {
        uiManager = new();
        player = new(new(Globals.GameWindow.X / 2 - 50, Globals.GameWindow.Y / 2 - 10));
        enemy = new(new(Globals.GameWindow.X / 2 + 40, Globals.GameWindow.Y / 2 - 30));
    }

    public void Update()
    {

    }

    public void Draw()
    {
        uiManager.Draw();
        player.Draw();
        enemy.Draw();
    }
}

public class UiManager
{
    public Texture2D BackgroundTexture;
    public HealthBar PlayerHealthBar;
    public HealthBar EnemyHealthBar;
    public ActionButton AttackButton;
    public ActionButton FireButton;

    public UiManager()
    {
        BackgroundTexture = Globals.ContentManager.Load<Texture2D>("background");
        PlayerHealthBar = new(Globals.ContentManager.Load<Texture2D>("health"), new(Globals.GameWindow.X / 2, Globals.GameWindow.Y / 2 + 10));
        EnemyHealthBar = new(Globals.ContentManager.Load<Texture2D>("health"), new(Globals.GameWindow.X / 2 - 75, Globals.GameWindow.Y / 2 - 40));
        AttackButton = new(Globals.ContentManager.Load<Texture2D>("attack"), new(Globals.GameWindow.X / 2 - 70, Globals.GameWindow.Y / 2 + 20));
        FireButton = new(Globals.ContentManager.Load<Texture2D>("fire"), new(Globals.GameWindow.X / 2 - 30, Globals.GameWindow.Y / 2 + 32));
    }

    public void Update()
    {
        
    }
    
    public void Draw()
    {
        Vector2 backgroundPosition = new(Globals.GameWindow.X / 2, Globals.GameWindow.Y / 2);
        Vector2 backgroundOrigin = new(BackgroundTexture.Width / 2, BackgroundTexture.Height / 2);
        Globals.SpriteBatch.Draw(BackgroundTexture, backgroundPosition, null, Color.White, 0f, backgroundOrigin, 1f, SpriteEffects.None, 0f);
        PlayerHealthBar.Draw();
        EnemyHealthBar.Draw();
        AttackButton.Draw();
        FireButton.Draw();
    }
}

public class HealthBar
{
    public Texture2D Texture;
    public Vector2 Position;

    public HealthBar(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
    }
}

public class ActionButton
{
    public Texture2D Texture;
    public Vector2 Position;

    public ActionButton(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
    }
}

public class Player
{
    public Texture2D Texture;
    public Vector2 Position;

    public Player(Vector2 position)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("player");
        Position = position;
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
    }
}

public class Enemy
{
    public Texture2D Texture;
    public Vector2 Position;

    public Enemy(Vector2 position)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("enemy");
        Position = position;
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
    }
}