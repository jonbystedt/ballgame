using UnityEngine;
using System.Collections;
using System;
using System.Text;
using Random = UnityEngine.Random;

public enum RelativeDirection { 
	north, 
	northeast,
	east, 
	southeast,
	south, 
	southwest,
	west, 
	northwest,
	none
};

public enum RelativeHeight {
	up,
	level,
	down
}

public struct Direction 
{
	public RelativeDirection direction;
	public RelativeHeight vertical;
}

public static class WorldUtils 
{
	public static Direction GetDirection(WorldPosition pos1, WorldPosition pos2)
	{
		Direction rLoc = new Direction();

		if (pos2.y > pos1.y)
			rLoc.vertical = RelativeHeight.up;

		if (pos2.y < pos1.y)
			rLoc.vertical = RelativeHeight.down;

		if (pos2.y == pos1.y)
			rLoc.vertical = RelativeHeight.level;

		if (pos2.z > pos1.z && pos2.x > pos1.x)
			rLoc.direction = RelativeDirection.northeast;
		
		else if (pos2.z < pos1.z && pos2.x > pos1.x)
			rLoc.direction = RelativeDirection.southeast;
		
		else if (pos2.z < pos1.z && pos2.x < pos1.x)
			rLoc.direction = RelativeDirection.southwest;

		else if (pos2.z > pos1.z && pos2.x < pos1.x)
			rLoc.direction = RelativeDirection.northwest;

		else if (pos2.z > pos1.z)
			rLoc.direction = RelativeDirection.north;

		else if (pos2.z < pos1.z)
			rLoc.direction = RelativeDirection.south;

		else if (pos2.x > pos1.x)
			rLoc.direction = RelativeDirection.east;
		
		else if (pos2.x < pos1.x)
			rLoc.direction = RelativeDirection.west;

		else
			rLoc.direction = RelativeDirection.none;

		return rLoc;

	}

	public static WorldPosition PositionOnPlane(WorldPosition pos, Direction rLoc, int x, int y)
	{
		WorldPosition planePos = new WorldPosition();

		switch(rLoc.direction) 
		{
			case RelativeDirection.north:

				planePos.x = pos.x + x;
				planePos.y = pos.y + y;

				if (rLoc.vertical == RelativeHeight.up)
					planePos.z = pos.z - y;

				else if (rLoc.vertical == RelativeHeight.down)
					planePos.z = pos.z + y;

				else
					planePos.z = pos.z;

				break;

			case RelativeDirection.northeast:

				planePos.y = pos.y + y;

				if (rLoc.vertical == RelativeHeight.up)
				{
					planePos.x = pos.x + x - y;
					planePos.z = pos.z - x - y;
				}
				else if (rLoc.vertical == RelativeHeight.down)
				{
					planePos.x = pos.x + x + y;
					planePos.z = pos.z - x + y;
				}
				else
				{
					planePos.x = pos.x + x;
					planePos.z = pos.z - x;
				}

				break;	

			case RelativeDirection.east:

				planePos.y = pos.y + y;
				planePos.z = pos.x + x;

				if (rLoc.vertical == RelativeHeight.up)
					planePos.x = pos.x - y;

				else if (rLoc.vertical == RelativeHeight.down)
					planePos.x = pos.x + y;

				else
					planePos.x = pos.x;

				break;

			case RelativeDirection.southeast:
				
				planePos.y = pos.y + y;

				if (rLoc.vertical == RelativeHeight.up)
				{
					planePos.x = pos.x + x + y;
					planePos.z = pos.z + x + y;
				}
				else if (rLoc.vertical == RelativeHeight.down)
				{
					planePos.x = pos.x + x - y;
					planePos.z = pos.z + x - y;
				}
				else
				{
					planePos.x = pos.x + x;
					planePos.z = pos.z + x;
				}

				break;

			case RelativeDirection.south:
				
				planePos.x = pos.x + x;
				planePos.y = pos.y + y;

				if (rLoc.vertical == RelativeHeight.up)
					planePos.z = pos.z + y;

				else if (rLoc.vertical == RelativeHeight.down)
					planePos.z = pos.z - y;

				else
					planePos.z = pos.z;

				break;

			case RelativeDirection.southwest:
				
				planePos.y = pos.y + y;

				if (rLoc.vertical == RelativeHeight.up)
				{
					planePos.x = pos.x + x + y;
					planePos.z = pos.z - x + y;
				}
				else if (rLoc.vertical == RelativeHeight.down)
				{
					planePos.x = pos.x + x - y;
					planePos.z = pos.z - x - y;
				}
				else
				{
					planePos.x = pos.x + x;
					planePos.z = pos.z - x;
				}

				break;	

			case RelativeDirection.west:
				
				planePos.y = pos.y + y;
				planePos.z = pos.x + x;

				if (rLoc.vertical == RelativeHeight.up)
					planePos.x = pos.x + y;

				if (rLoc.vertical == RelativeHeight.down)
					planePos.x = pos.x - y;

				else
					planePos.x = pos.x;

				break;

			case RelativeDirection.northwest:

				planePos.y = pos.y + y;

				if (rLoc.vertical == RelativeHeight.up)
				{
					planePos.x = pos.x + x - y;
					planePos.z = pos.z + x - y;
				}
				else if (rLoc.vertical == RelativeHeight.down)
				{
					planePos.x = pos.x + x + y;
					planePos.z = pos.z + x + y;
				}
				else
				{
					planePos.x = pos.x + x;
					planePos.z = pos.z + x;
				}

				break;	

			case RelativeDirection.none:
				
				planePos.x = pos.x + x;
				planePos.z = pos.z + y;
				planePos.y = pos.y;

				break;

			}

		return planePos;
	}
}
