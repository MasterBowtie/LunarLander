using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CS5410;

class Terrain {
    private int maxHeight;
    private int minHeight;
    private int minWidth;
    private int maxWidth;
    private float zoneWidth;

    private float roughness = .8f;
    private int passes = 7;
    // private int passes = 3;

    private VertexPositionColor[] vertsTriStrip;
    private int[] indexTriStrip;


    MyRandom random = new MyRandom();

    public Terrain(int minX, int maxX, int minY, int maxY, float zoneRatio) {
      maxHeight = maxY;
      minHeight = minY;
      minWidth = minX;
      maxWidth = maxX;
      zoneWidth = (maxX - minX) * zoneRatio;
    }

    public VertexPositionColor[] getStrip() {
      return vertsTriStrip;
    }

    public int[] getStripIndex() {
      return indexTriStrip;
    }

    public int getPass() {
      return passes;
    }

    public void setPass(int value) {
      passes = value;
      System.Console.WriteLine("Pass: {0}", passes);
    }

    public float getRough() {
      return roughness;
    }

    public void setRough(float value) {
      roughness = value;
      System.Console.WriteLine("Rough: {0}", roughness);
    }

    // parameter is a ratio of the total width
    public void setZoneWidth ( float ratio) {
      zoneWidth = (maxWidth - minWidth) * ratio;
    }

    public Vector3 midPoint(Vector3 pt1, Vector3 pt2) {
        float x = (pt2.X - pt1.X) / 2 + pt1.X;

        float r = (float)(random.nextGaussian(0, 1) * roughness * Math.Abs(pt2.X - pt1.X));

        float y = (pt2.Y + pt1.Y) / 2 + r;

        return new Vector3(x, y, 0);
    }

    public void buildTerrain(int zones)
    {
        List<Vector3> floor = new List<Vector3>();
        // end points
        Vector3 start = new Vector3(minWidth, maxHeight/2, .0f);
        Vector3 end = new Vector3(maxWidth, maxHeight/2, .0f);


        float minX = maxWidth * .15f;
        float maxX = maxWidth - minX;
        float minY = maxHeight * .15f + minHeight;
        float maxY = maxHeight - minY;

        floor.Add(start);

        for (int z = 0; z < zones; z++) {
          int index = 0;
          float x = random.nextRange(minX, maxX - zoneWidth);
          float y = random.nextRange(minY, maxY);
          while (index < floor.Count) { 
            if (index != 0 && x < floor[index-1].X && x + zoneWidth > floor[index-1].X || x < floor[index].X && x + zoneWidth > floor[index].X) {
              x = random.nextRange(minX, maxX - zoneWidth);
              index = 0;
            }
            else {
              index += 2;
            }
          }
          Vector3 zoneStart = new Vector3(x, y, 0);
          Vector3 zoneEnd = new Vector3(x + zoneWidth, y, 0);
          floor.Add(zoneStart);
          floor.Add(zoneEnd);
        }
        floor.Add(end);

        // Vector3 start = new Vector3(.0f, random.Next((int)maxHeight, m_graphics.PreferredBackBufferHeight) + .0f, .0f);
        // Vector3 end = new Vector3(m_graphics.PreferredBackBufferWidth + .0f, random.Next((int)maxHeight, m_graphics.PreferredBackBufferHeight) + .0f, .0f);


        for (int level = 0; level < passes; level++)
        {
            List<Vector3> newTerrain = new List<Vector3>();
            for (int i = 1; i < floor.Count; i++)
            {
              if (floor[i - 1].Y == floor[i].Y) 
              {
                newTerrain.Add(floor[i - 1]);
                // newTerrain.Add(new Vector3((floor[i].X - floor[i-1].X)/2 + floor[i-1].X, floor[i-1].Y, 0));
              } 
              else 
              {
                newTerrain.Add(floor[i - 1]);
                Vector3 newVector = midPoint(floor[i-1], floor[i]);

                while( newVector.Y > maxHeight || newVector.Y < minHeight)
                {
                  // System.Console.WriteLine("reroll");
                  newVector = midPoint(floor[i-1], floor[i]);
                }

                newTerrain.Add(newVector);
              }
            }
            newTerrain.Add(floor[floor.Count - 1]);
            floor = newTerrain;
        }

        vertsTriStrip =  new VertexPositionColor[floor.Count * 2];
        indexTriStrip = new int[floor.Count * 2];
        for (int i = 0; i < floor.Count; i++)
        {

            vertsTriStrip[i * 2].Position = floor[floor.Count - i - 1];
            vertsTriStrip[i * 2].Color = Color.Red;

            vertsTriStrip[i * 2 + 1].Position = new Vector3(floor[floor.Count - i - 1].X, maxHeight, 0);
            vertsTriStrip[i * 2 + 1].Color = Color.Red;

            indexTriStrip[i * 2] = i * 2;
            indexTriStrip[i * 2 + 1] = i * 2 + 1;
        }
    }
}
