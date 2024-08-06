using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using VintageSymphony.Storage;
using VintageSymphony.Util;

namespace VintageSymphony.Situations.Facts;

public class SituationalFactsCollector
{
	struct PositionSample
	{
		public Vec3f Position;
		public long Time;
	}

	private readonly string[] EnemyTypes = { "drifter", "locust", "wolf", "bear", "hyena", "bell", "eidolon" };

	private readonly AttributeStorage attributeStorage;
	private EntityPlayer PlayerEntity => clientApi.World.Player.Entity;

	private SituationalFacts facts = new();

	private readonly LinkedList<PositionSample> playerPositionSamples = new(); // distances travelled per second
	private const float PlayerPositionFrameDuration = 60f;

	private readonly EnumTool[] weapons =
	{
		EnumTool.Bow,
		EnumTool.Sling,
		EnumTool.Spear,
		EnumTool.Sword,
	};

	private ICoreClientAPI clientApi;
	private long timeLastDamageTaken = -1L;
	private readonly int worldHeight;
	private readonly int seaLevel;

	public SituationalFactsCollector(AttributeStorage attributeStorage)
	{
		clientApi = MusicManager.ClientApi;
		this.attributeStorage = attributeStorage;
		PlayerEntity.WatchedAttributes.RegisterModifiedListener("onHurt", OnPlayerHurt);
		clientApi.Event.BlockChanged += OnBlockChanged;
		worldHeight = clientApi.World.BlockAccessor.MapSize.Y;
		seaLevel = clientApi.World.SeaLevel;
	}

	private long GetNow() => clientApi.InWorldEllapsedMilliseconds;

	private void OnBlockChanged(BlockPos pos, Block oldblock)
	{
		var newBlock = clientApi.World.BlockAccessor.GetBlock(pos);
		if (IsBedBlock(newBlock) && newBlock.Code.Path.Contains("-feet-"))
		{
			attributeStorage.PlayerHomeLocations.Add(pos.AsVec3i);
		}
		else if (IsBedBlock(oldblock) && newBlock.Code.PathStartsWith("air"))
		{
			attributeStorage.PlayerHomeLocations.RemoveAll(v => v.SquareDistanceTo(pos.AsVec3i) <= 1);
		}
	}

	private void OnPlayerHurt()
	{
		timeLastDamageTaken = GetNow();
		facts.SecondsSinceLastDamage = 0;
	}


	public SituationalFacts AssessSituation(float dt)
	{
		UpdateMovementDistances();
		UpdateMovementRadius();
		UpdateDistanceFromHome();
		UpdateTime();
		UpdateHeight();
		UpdateHoldingWeapon();
		UpdateEnemyDistance();
		UpdateSunLevel();

		return facts;
	}

	private void UpdateHoldingWeapon()
	{
		var item = PlayerEntity.RightHandItemSlot?.Itemstack?.Item;
		EnumTool? tool = item?.Tool;

		facts.IsHoldingWeapon = item != null
		                        && (tool.HasValue && weapons.Contains(tool.Value) ||
		                            item.Code.BeginsWith("game", "club"));
	}

	private void UpdateMovementDistances()
	{
		long now = GetNow();

		var sample = new PositionSample
		{
			Position = PlayerEntity.Pos.XYZFloat,
			Time = now
		};
		playerPositionSamples.AddLast(sample);
		long timeLimit = now - (long)(PlayerPositionFrameDuration * 1000);
		while (playerPositionSamples.Count > 0 && playerPositionSamples.First!.Value.Time < timeLimit)
		{
			playerPositionSamples.RemoveFirst();
		}

		facts.DistanceTravelledTotal = 0;
		facts.DistanceTravelledDiagonal = 0;
		if (playerPositionSamples.Count < 2)
		{
			return;
		}

		var previousNode = playerPositionSamples.First;
		for (var currentNode = previousNode!.Next;
		     currentNode!.Next != null;
		     previousNode = currentNode, currentNode = currentNode.Next)
		{
			facts.DistanceTravelledTotal += previousNode.Value.Position.DistanceTo(currentNode.Value.Position);
		}

		facts.DistanceTravelledDiagonal =
			playerPositionSamples.First!.Value.Position.DistanceTo(playerPositionSamples.Last!.Value.Position);
	}

	private void UpdateMovementRadius()
	{
		var boundingSphere = BoundingSphere.Calculate(playerPositionSamples.Select(s => s.Position));
		facts.MovementRadius = boundingSphere.Radius;
	}

	private void UpdateDistanceFromHome()
	{
		var playerPosition = PlayerEntity.Pos.XYZInt!;
		var homeLocations = attributeStorage.PlayerHomeLocations;

		if (homeLocations.Count == 0)
		{
			facts.DistanceFromHome = float.PositiveInfinity;
			return;
		}

		var nearestDistanceSq = long.MaxValue;
		for (int i = 0; i < homeLocations.Count; i++)
		{
			nearestDistanceSq = long.Min(nearestDistanceSq, playerPosition.SquareDistanceTo(homeLocations[i]));
		}

		facts.DistanceFromHome = (float)Math.Sqrt(nearestDistanceSq);
	}

	private void UpdateTime()
	{
		var calendar = clientApi.World.Calendar;
		facts.Time = calendar.HourOfDay / calendar.HoursPerDay;
		facts.Now = GetNow();
		facts.SecondsSinceLastDamage = timeLastDamageTaken > 0
			? (int)(facts.Now - timeLastDamageTaken) / 1000
			: int.MaxValue;
	}

	private void UpdateHeight()
	{
		var playerPosition = PlayerEntity.Pos;
		var playerHeight = (float)playerPosition.Y;
		var terrainHeight = clientApi.World.BlockAccessor.GetTerrainMapheightAt(playerPosition.AsBlockPos);
		facts.RelativeHeight = MoreMath.Normalize(playerHeight, 0, seaLevel, worldHeight);
		facts.DistanceToSurface = terrainHeight - playerHeight;
	}

	private void UpdateEnemyDistance()
	{
		bool IsEnemy(Entity entity)
		{
			if (!entity.IsCreature || !entity.Alive)
			{
				return false;
			}

			for (int i = 0; i < EnemyTypes.Length; i++)
			{
				if (entity.Code.PathStartsWith(EnemyTypes[i]))
				{
					return true;
				}
			}

			return false;
		}

		const float maxHorizontalDistance = SituationalFacts.EnemyDistanceMax;
		const float maxVerticalDistance = 15;
		var enemyEntity = clientApi.World.GetNearestEntity(
			PlayerEntity.Pos.XYZ,
			maxHorizontalDistance,
			maxVerticalDistance,
			IsEnemy
		);

		facts.EnemyDistance = enemyEntity == null
			? float.PositiveInfinity
			: MoreMath.DistanceWithWeightedVerticality(enemyEntity.Pos.XYZFloat, PlayerEntity.Pos.XYZFloat, 4f);
	}

	private void UpdateSunLevel()
	{
		facts.SunLevel = MusicManager.ClientMain.playerProperties.sunSlight;
	}

	private static bool IsBedBlock(Block? block)
	{
		return block?.Code.PathStartsWith("bed") ?? false;
	}
}