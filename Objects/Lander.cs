using CS5410;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;


namespace Apedaile {
  public class Lander : PlayerEntity {
    protected Texture2D _image;
    protected Rectangle _rectangle;
    protected Vector2 _renderVector;
    private bool _waitforKeyRelease = false;
    private bool _continue = true;

    private float _radius;

    private float _rotation = 0;
    private float _rotationRate = .02f;
    private float _fuel = 5;

    //position has to be a vector, since rectangle works in int and we need a constantly updated vector.
    private Vector2 _position = new Vector2(50, 50);

    // accelerators
    private Vector2 _momentum = new Vector2(0,0);
    private Vector2 _accelerate = new Vector2(0, -.03f);
    private Vector2 _gravity = new Vector2(0,.005f);

    public void setupInput(KeyboardInput keyboard) {
      _keyboard = keyboard;
    }

    public void bindCommand(IInputDevice.CommandDelegate callback, Keys key) {
      _keyboard.registerCommand(key, _waitforKeyRelease, callback);
    }

    public override void loadContent(ContentManager contentManager) {
      _image = contentManager.Load<Texture2D>("Images/player");
      _renderVector = new Vector2(_image.Width/2, _image.Height/2);
      float width =  _graphics.PreferredBackBufferHeight/10;
      float height = (width/_image.Width) * _image.Height;
      // System.Console.WriteLine("{0}, {1}, {2}, {3}", width, _image.Width, height, _image.Height);
      _rectangle = new Rectangle((int)(_position.X + width/2), (int)(_position.Y + height/2), (int) width, (int) height);
      _radius = _rectangle.Height/2;
    }

    public override void update(GameTime gameTime) {
      if (_continue) {
        _momentum = Vector2.Add(_momentum, _gravity);
        _position = Vector2.Add(_position, _momentum);
        _rectangle.X = (int)_position.X + _rectangle.Width/2;
        _rectangle.Y = (int)_position.Y + _rectangle.Height/2;
      }
    }

    public override void render(GameTime gameTime) {
      _spriteBatch.Draw(
        _image, 
        _rectangle, 
        null,
        Color.White,
        _rotation,
        _renderVector,
        SpriteEffects.None,
        0
      ); 
    }

    public void freeze() {
      System.Console.WriteLine(_momentum);
      _continue = false;
    }

    public void rotateLeft(GameTime gameTime, float value) {
      if (_continue) {
        _rotation -= _rotationRate * value;
      }
    }
    
    public void rotateRight(GameTime gameTime, float value) {
      if (_continue) {  
      _rotation += _rotationRate * value;
      }
    }

    // Attempted to use Framework.Vector2 Methods, but unable to find Rotate
    // Ended up copying the code from the Monogame Git Repo
    public void moveForward(GameTime gameTime, float value) {

      float X = _accelerate.X;
      float Y = _accelerate.Y;

      float cos = MathF.Cos(_rotation);
      float sin = MathF.Sin(_rotation);


      X = X * cos - Y * sin;
      Y = _accelerate.X * sin + Y * cos;
      _fuel -=.001f;
      if (_fuel > 0) {
        _momentum = Vector2.Add(_momentum, new Vector2(X, Y));
      }
    }

    public void pause(GameTime gameTime, float value) {
      // implement Entity States
    }

    public void reset() {
      _rotation = 0;
      _position = new Vector2(50, 50);
      _momentum = new Vector2(0,0);
      _fuel = 5;
      _continue = true;
    }

    public float getRadius() {
      return _radius;
    }

    public Vector2 getCenter() {
      // System.Console.WriteLine("{0},{1}",_position, _rectangle);
      // System.Console.WriteLine(_graphics.PreferredBackBufferHeight);
      return new Vector2(_position.X + _radius, _position.Y + _radius);
    }

    public void crash() {
      
    }

    protected bool testPoints(Vector3 pt1, Vector3 pt2, Vector2 player, float radius) {
      Vector2 v1 = new Vector2( pt2.X - pt1.X, pt2.Y - pt1.Y );
      Vector2 v2 = new Vector2( pt1.X - player.X, pt1.Y - player.Y );
      double b = -2 * (v1.X * v2.X + v1.Y * v2.Y);
      double c =  2 * (v1.X * v1.X + v1.Y * v1.Y);
      double d = b * b - 2 * c * (v2.X * v2.X + v2.Y * v2.Y - radius * radius);
      if (d < 0) { // no intercept
          return false;
      }
      d = Math.Sqrt(d);
      // These represent the unit distance of point one and two on the line
      double u1 = (b - d) / c;  
      double u2 = (b + d) / c;
      if (u1 <= 1 && u1 >= 0) {  // If point on the line segment
          return true;
      }
      if (u2 <= 1 && u2 >= 0) {  // If point on the line segment
          return true;
      }
      return false;
    }

    public bool collision(VertexPositionColor[] floor) {
      int index = 2;

      Vector2 player = getCenter();
      float radius = getRadius();
      
      while (index < floor.Length) {
        Vector3 pt1 = floor[index - 2].Position;
        Vector3 pt2 = floor[index].Position;
        if (testPoints(pt1, pt2, player, radius)) {
          return true;
        }

        index += 2;
      }
      return false;
    }
  }
}