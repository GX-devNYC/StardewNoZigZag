using HarmonyLib;
using NoZigZag.API;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Pathfinding;
using StardewValley.TerrainFeatures;
using System.Diagnostics.Metrics;
using static StardewValley.Pathfinding.PathFindController;
using System;
using System.Collections.Generic;
using System.Threading;
using Microsoft.Xna.Framework;
using StardewValley.Audio;
using StardewValley.Extensions;
using StardewValley.Locations;
using xTile.Tiles;

namespace NoZigZag.src
{
	public class HarmonyPathfinding
	{
		public static IMonitor Monitor;

		// call this method from Entry class
		internal static void Initialize(IMonitor monitor)
		{
			Monitor = monitor;
		}

		protected static int _counterH = 0;
		protected static PriorityQueue _openListH = new PriorityQueue();
		protected static HashSet<int> _closedListH = new HashSet<int>();
		protected static readonly sbyte[,] DirectionsH = new sbyte[4, 2]
			{
					{ -1, 0 },
					{ 1, 0 },
					{ 0, 1 },
					{ 0, -1 }
			};

		public static Stack<Point> findPath_prefix(Point startPoint, Point endPoint, isAtEnd endPointFunction, GameLocation location, Character character, int limit)
		{
			// ????
			if (Interlocked.Increment(ref _counterH) != 1)
			{
				throw new Exception();
			}
			try
			{
				bool ignore_obstructions = character is FarmAnimal animal && animal.CanSwim() && animal.isSwimming.Value;
				_openListH.Clear();
				_closedListH.Clear();
				PriorityQueue openList = _openListH;
				HashSet<int> closedList = _closedListH;
				int iterations = 0;
				openList.Enqueue(new PathNode(startPoint.X, startPoint.Y, 0, null), Math.Abs(endPoint.X - startPoint.X) + Math.Abs(endPoint.Y - startPoint.Y));
				int layerWidth = location.map.Layers[0].LayerWidth;
				int layerHeight = location.map.Layers[0].LayerHeight;
				int facing_direction = character.FacingDirection;
				while (!openList.IsEmpty())
				{
					PathNode currentNode = openList.Dequeue();
					if (endPointFunction(currentNode, endPoint, location, character))
					{
						return reconstructPath(currentNode);
					}
					closedList.Add(currentNode.id);
					int ng = (byte)(currentNode.g + 1);
					for (int i = 0; i < 4; i++)
					{
						int nx = currentNode.x + DirectionsH[i, 0];
						int ny = currentNode.y + DirectionsH[i, 1];
						int nid = PathNode.ComputeHash(nx, ny);
						if (closedList.Contains(nid))
						{
							continue;
						}
						if ((nx != endPoint.X || ny != endPoint.Y) && (nx < 0 || ny < 0 || nx >= layerWidth || ny >= layerHeight))
						{
							closedList.Add(nid);
							continue;
						}
						PathNode neighbor = new PathNode(nx, ny, currentNode);
						neighbor.g = (byte)(currentNode.g + 1);
						if (!ignore_obstructions && location.isCollidingPosition(new Rectangle(neighbor.x * 64 + 1, neighbor.y * 64 + 1, 62, 62), Game1.viewport, character is Farmer, 0, glider: false, character, pathfinding: true))
						{
							closedList.Add(nid);
							continue;
						}
						int f = ng + (Math.Abs(endPoint.X - nx) + Math.Abs(endPoint.Y - ny));
						closedList.Add(nid);
						openList.Enqueue(neighbor, f);
					}
					iterations++;
					if (iterations >= limit)
					{
						return null;
					}
				}
				return null;
			}
			finally
			{
				if (Interlocked.Decrement(ref _counterH) != 0)
				{
					throw new Exception();
				}
			}
		}

	}
}
