using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace CS5410 {
  public abstract class GameStateView : IGameState {
    protected GraphicsDeviceManager _graphics;
    protected BasicEffect _effect;
    protected SpriteBatch _spriteBatch;
    protected KeyboardInput _keyboard;

    public void initialize(GraphicsDevice graphicsDevice, GraphicsDeviceManager graphics) {
      _graphics = graphics;
      _spriteBatch = new SpriteBatch(graphicsDevice);
      _keyboard = new KeyboardInput();
    }
    
    public abstract void setupInput();

    public abstract void loadContent(ContentManager contentManager);

    public abstract GameStateEnum processInput(GameTime gameTime);

    public abstract void render(GameTime gameTime);

    public abstract void update(GameTime gameTime);
  }
}