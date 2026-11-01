using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace turn_based_test.TurnBased;

public static class Globals
{
    public static float Time { get; set; }
    public static ContentManager ContentManager { get; set; }
    public static SpriteBatch SpriteBatch { get; set; }
    public static GraphicsDevice GraphicsDevice { get; set; }
    public static Point GameWindow { get; set; } = new(960, 540);
    public static Point VirtualGameWindow { get; set; } = new(480, 270);
    public static Matrix GameMatrix { get; set; }

    public static void Update(GameTime gameTime)
    {
        Time = (float)gameTime.ElapsedGameTime.TotalSeconds;
    }

    public static void GetMatrix()
    {
        float zoom = 6f;

        Vector2 center = new(Globals.GameWindow.X / 2f, Globals.GameWindow.Y / 2f);

        GameMatrix = Matrix.CreateTranslation(-center.X, -center.Y, 0) *
                     Matrix.CreateScale(zoom, zoom, 1f) *
                     Matrix.CreateTranslation(center.X, center.Y, 0);
    }
}

public class InputManager
{
    public static InputManager Instance { get; private set; }
    private MouseState _currentMouseState;
    private MouseState _previousMouseState;

    private InputManager() { }

    public static void Initialize()
    {
        if (Instance is null) Instance = new();
    }

    public static void Update()
    {
        if (Instance is null) return;

        Instance._previousMouseState = Instance._currentMouseState;
        Instance._currentMouseState = Mouse.GetState();
    }

    public static Point GetMousePosition()
    {
        if (Instance is null) return new Point(0,0);

        Vector2 mousePosition = new(Instance._currentMouseState.X, Instance._currentMouseState.Y);

        Matrix inverseMatrix = Matrix.Invert(Globals.GameMatrix);
        Vector2 mouseWorld = Vector2.Transform(mousePosition, inverseMatrix);

        return new Point((int)mouseWorld.X, (int)mouseWorld.Y);
    }

    public static bool IsLeftButtonClicked()
    {
        if (Instance is null) return false;

        return Instance._currentMouseState.LeftButton == ButtonState.Pressed &&
               Instance._previousMouseState.LeftButton == ButtonState.Released;   
    }
}

public class GameManager
{
    public BattleManager battleManager;

    public GameManager()
    {
        InputManager.Initialize();
        battleManager = new();
    }
    
    public void Update()
    {
        InputManager.Update();
        battleManager.Update();
    }

    public void Draw()
    {
        battleManager.Draw();
    }
}

public class BattleManager : IObserver
{
    public UiManager uiManager;
    public Player player;
    public Enemy enemy;
    public List<ICommand> Actions;
    public int Round;

    public BattleManager()
    {
        uiManager = new();
        player = new(new(Globals.GameWindow.X / 2 - 50, Globals.GameWindow.Y / 2 - 10));
        enemy = new(new(Globals.GameWindow.X / 2 + 40, Globals.GameWindow.Y / 2 - 30));
        Actions = new();
        Round = 0;

        AddObservers();
        InitNewRound();        
    }

    public void Update()
    {
        uiManager.Update();
    }

    public void Draw()
    {
        uiManager.Draw();
        player.Draw();
        enemy.Draw();
    }

    public void OnNotify(IEvent receivedEvent)
    {
        if (receivedEvent is OnAttackButtonClickedEvent)
            player.Attack();
        
        if (receivedEvent is OnFireButtonClickedEvent)
            player.Fire();

        if (receivedEvent is OnActorTakeDamageEvent actorTakeDamageEvent)
        {
            uiManager.UpdateHealthBars(player.Health, enemy.Health);
        }

        if (receivedEvent is OnPlayerAttackEvent playerAttackEvent)
        {
            AttackCommand playerAttack = new(player, enemy, playerAttackEvent.Damage);
            Actions.Add(playerAttack);
            Console.WriteLine("Add the player attack command on actions");

            AttackCommand enemyAttackCommand = new(enemy, player, 5);
            Actions.Add(enemyAttackCommand);
            Console.WriteLine("Add the enemy attack command on actions");

            foreach (ICommand action in Actions)
                action.Execute();

            InitNewRound();
        }
    }

    private void InitNewRound()
    { 
        Round++;
        uiManager.Log.LogMessage($"Round {Round} started!!");
    }

    private void AddObservers()
    {
        uiManager.AddObserver(this);
        player.AddObserver(this);
        enemy.AddObserver(this);
    }
}

public class UiManager : Subject
{
    public Texture2D BackgroundTexture;
    public HealthBar PlayerHealthBar;
    public HealthBar EnemyHealthBar;
    public ActionButton AttackButton;
    public ActionButton FireButton;
    public Log Log;

    public UiManager()
    {
        BackgroundTexture = Globals.ContentManager.Load<Texture2D>("background");
        PlayerHealthBar = new(Globals.ContentManager.Load<Texture2D>("health"), new(Globals.GameWindow.X / 2, Globals.GameWindow.Y / 2 + 10));
        EnemyHealthBar = new(Globals.ContentManager.Load<Texture2D>("health"), new(Globals.GameWindow.X / 2 - 75, Globals.GameWindow.Y / 2 - 40));
        AttackButton = new(Globals.ContentManager.Load<Texture2D>("attack"), new(Globals.GameWindow.X / 2 - 70, Globals.GameWindow.Y / 2 + 20));
        FireButton = new(Globals.ContentManager.Load<Texture2D>("fire"), new(Globals.GameWindow.X / 2 - 30, Globals.GameWindow.Y / 2 + 32));
        Log = new(new(Globals.GameWindow.X / 2 + 15, Globals.GameWindow.Y / 2 + 22));
    }

    public void Update()
    {
        Point mousePosition = InputManager.GetMousePosition();
        Rectangle attackButtonBounds = new((int)AttackButton.Position.X, (int)AttackButton.Position.Y, AttackButton.Texture.Width, AttackButton.Texture.Height);
        Rectangle fireButtonBounds = new((int)FireButton.Position.X, (int)FireButton.Position.Y, FireButton.Texture.Width, FireButton.Texture.Height);

        if (attackButtonBounds.Contains(mousePosition) && InputManager.IsLeftButtonClicked())
        {
            OnAttackButtonClickedEvent buttonClickedEvent = new();
            Notify(buttonClickedEvent);
        }

        if (fireButtonBounds.Contains(mousePosition) && InputManager.IsLeftButtonClicked())
        {
            OnFireButtonClickedEvent buttonClickedEvent = new();
            Notify(buttonClickedEvent);
        }
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
        Log.Draw();
    }

    public void UpdateHealthBars(int playerHealth, int enemyHealth)
    {
        PlayerHealthBar.CurrentHealth = playerHealth;
        EnemyHealthBar.CurrentHealth = enemyHealth;
    }
}

public class HealthBar
{
    public Texture2D Texture;
    public Vector2 Position;
    public int MaxHealth;
    public int CurrentHealth;

    public HealthBar(Texture2D texture, Vector2 position)
    {
        Texture = texture;
        Position = position;
        MaxHealth = texture.Width;
        CurrentHealth = texture.Width;
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        float healthPercent = (float)CurrentHealth / MaxHealth;
        healthPercent = MathHelper.Clamp(healthPercent, 0f, 1f);
        Vector2 scale = new(healthPercent, 1f);

        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
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

public class Log
{
    public Texture2D Texture;
    public Vector2 Position;
    public SpriteFont SpriteFont;
    public List<string> Messages;

    public Log(Vector2 position)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("log");
        Position = position;
        SpriteFont = Globals.ContentManager.Load<SpriteFont>("Monocraft");
        SpriteFont.Spacing = 3f;
        Messages = new();
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
        
        for (int i = 0; i < Messages.Count; i++)
        {
            int indexFromBottom = Messages.Count - 1 - i;
            int offSet = indexFromBottom * 4;
            Vector2 position = new(Globals.GameWindow.X / 2 + 20, Globals.GameWindow.Y / 2 + 38 - offSet);
            Globals.SpriteBatch.DrawString(SpriteFont, Messages[i], position, Color.White, 0f, Vector2.Zero, 0.15f, SpriteEffects.None, 0f);
        }
    }

    public void LogMessage(string message)
    {
        if (Messages.Count > 3)
            Messages.RemoveAt(0);

        Messages.Add(message);
    }
}

public interface IGameActor
{
    public int Health { get; set; }

    public void TakeDamage(int damage);
}

public class Player : Subject, IGameActor
{
    public Texture2D Texture;
    public Vector2 Position;
    public int Level;
    public int Strength;
    public int Health { get; set; }

    public Player(Vector2 position)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("player");
        Position = position;
        Level = 0;
        Strength = 5;
        Health = 100;
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
    }

    public void Attack()
    {
        OnPlayerAttackEvent attackEvent = new(damage: Strength);
        Notify(attackEvent);
    }

    public void Fire()
    {
        OnPlayerAttackEvent fireEvent = new(damage: 2 * Strength);
        Notify(fireEvent);
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        OnActorTakeDamageEvent takeDamageEvent = new(this, Health);
        Notify(takeDamageEvent);
    }
}

public class Enemy : Subject, IGameActor
{
    public Texture2D Texture;
    public Vector2 Position;
    public int Health { get; set; }
    

    public Enemy(Vector2 position)
    {
        Texture = Globals.ContentManager.Load<Texture2D>("enemy");
        Position = position;
        Health = 100;
    }

    public void Update()
    {
        
    }

    public void Draw()
    {
        Globals.SpriteBatch.Draw(Texture, Position, null, Color.White);
    }

    public void TakeDamage(int damage)
    {
        Health -= damage;
        OnActorTakeDamageEvent takeDamageEvent = new(this, Health);
        Notify(takeDamageEvent);
    }
}

// # -- Command pattern -- # //
public interface ICommand
{
    public void Execute();
}

public class AttackCommand : ICommand
{
    private IGameActor Attacker;
    private IGameActor Defender;
    private int Damage;

    public AttackCommand(IGameActor attacker, IGameActor defender, int damage)
    {
        Attacker = attacker;
        Defender = defender;
        Damage = damage;
    }

    public void Execute() =>
        Defender.TakeDamage(Damage);
}

// # -- Observer pattern -- # //
public interface IEvent { }


public class OnAttackButtonClickedEvent : IEvent
{
    public OnAttackButtonClickedEvent() { }
}

public class OnFireButtonClickedEvent : IEvent
{
    public OnFireButtonClickedEvent() { }
}

public class OnPlayerAttackEvent : IEvent
{
    public int Damage;
    public OnPlayerAttackEvent(int damage) => Damage = damage;
}

public class OnActorTakeDamageEvent : IEvent
{
    public IGameActor Actor;
    public int Health;
    public OnActorTakeDamageEvent(IGameActor actor, int health)
    {
        Actor = actor;
        Health = health;
    }
}

public interface IObserver
{
    public void OnNotify(IEvent receivedEvent);
}

public abstract class Subject
{
    public List<IObserver> Observers { get; private set; } = new();
    public void AddObserver(IObserver observer) => Observers.Add(observer);
    public void RemoveObserver(IObserver observer) => Observers.Remove(observer);

    public void Notify(IEvent sendingEvent)
    {
        foreach (IObserver observer in Observers)
            observer.OnNotify(sendingEvent);
    }
}