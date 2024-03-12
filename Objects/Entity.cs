
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Apedaile {
  public interface IEntity {
    void initialize(GraphicsDeviceManager graphics, SpriteBatch spriteBatch);

    void loadContent(ContentManager contentManager);

    void update(GameTime gameTime);

    void render(GameTime gameTime);
  }
}