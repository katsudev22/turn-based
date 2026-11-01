using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using turn_based_test.TurnBased;

namespace turn_based_test;

public class Game1 : Game
{
    private GraphicsDeviceManager _graphics;
    private GameManager _gameManager;
    private Matrix _screenScaleMatrix;

    public Game1()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }

    protected override void Initialize()
    {
        _graphics.PreferredBackBufferWidth = Globals.GameWindow.X;
        _graphics.PreferredBackBufferHeight = Globals.GameWindow.Y;
        _graphics.ApplyChanges();

        UpdateMatrix();

        base.Initialize();
    }

    protected override void LoadContent()
    {
        Globals.GraphicsDevice = GraphicsDevice;
        Globals.SpriteBatch = new SpriteBatch(GraphicsDevice);
        Globals.ContentManager = Content;

        _gameManager = new();
    }

    protected override void Update(GameTime gameTime)
    {
        Globals.Update(gameTime);
        _gameManager.Update();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        Globals.SpriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _screenScaleMatrix);
        _gameManager.Draw();
        Globals.SpriteBatch.End();

        base.Draw(gameTime);
    }

    private void UpdateMatrix()
    {
        float zoom = 6f;

        Vector2 center = new(Globals.GameWindow.X / 2f, Globals.GameWindow.Y / 2f);

        _screenScaleMatrix = Matrix.CreateTranslation(-center.X, -center.Y, 0) *
                             Matrix.CreateScale(zoom, zoom, 1f) *
                             Matrix.CreateTranslation(center.X, center.Y, 0);
    }
}
