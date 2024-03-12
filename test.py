import random

def buildTerrain(minX, maxX, minY, maxY, depth):
  terrain = []
  pt1 = (minX, random.randint(minY, maxY))
  pt2 = (maxX, random.randint(minY, maxY))
  terrain.append(pt1)
  terrain.append(pt2)

  for level in range(depth):
    newTerrain = []
    for i in range(1, len(terrain)):
      newTerrain.append(terrain[i-1])
      newTerrain.append(midpoint(terrain[i-1], terrain[i]))
    newTerrain.append(terrain[i])
    # print(newTerrain)
    terrain = newTerrain

  return terrain

def midpoint(pt1, pt2):
  x = (pt2[0] - pt1[0])/2 + pt1[0]

  surface = .5
  g = random.gauss(0, 1)
  roughness = g * surface * abs(pt2[0] - pt1[0])
  print(f"{g} * {surface} * [{pt2[0] - pt1[0]}] = {roughness} + {(pt1[1] + pt2[1])/2}")

  y = (pt1[1] + pt2[1])/2 + roughness

  return (x,y)

if __name__=="__main__":

  terrain = buildTerrain(0, 500, 0, 0, 3)

  for point in terrain:
    print(point)
  
  
  
