using System.Xml.Schema;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
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

	private static readonly string[] EnemyTypes =
		{ "drifter", "shiver", "bowtorn", "locust", "wolf", "bear", "hyena", "bell", "eidolon" };

	private readonly AttributeStorage attributeStorage;
	private readonly IDictionary<AssetLocation, bool> enemyCodeCache = new Dictionary<AssetLocation, bool>();
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
	private long timeLastAttackDetected = -1L;

	private const int
		AttackCooldownSeconds = 2; // Time in seconds that isAttacked remains true after enemies stop attacking

	private readonly int worldHeight;
	private readonly int seaLevel;
	private readonly ModSystemRifts riftModSystem;

	public SituationalFactsCollector(AttributeStorage attributeStorage)
	{
		clientApi = VintageSymphony.ClientApi;
		this.attributeStorage = attributeStorage;
		PlayerEntity.WatchedAttributes.RegisterModifiedListener("onHurt", OnPlayerHurt);
		clientApi.Event.BlockChanged += OnBlockChanged;
		worldHeight = clientApi.World.BlockAccessor.MapSize.Y;
		seaLevel = clientApi.World.SeaLevel;
		riftModSystem = clientApi.ModLoader.GetModSystem<ModSystemRifts>();
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

	public SituationalFacts GatherFacts(float dt)
	{
		UpdateMovementDistances();
		UpdateMovementRadius();
		UpdateDistanceFromHome();
		UpdateTime();
		UpdateHeight();
		UpdateHoldingWeapon();

		var nearbyEnemies = FetchNearbyEnemies();
		UpdateEnemyDistance(nearbyEnemies);
		UpdateIsAttacked(nearbyEnemies);

		UpdateRiftDistance();
		UpdateSunFacts();
		UpdateAlive();
		UpdateResonators();

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

		facts.DistanceFromHome = MathF.Sqrt(nearestDistanceSq);
	}

	private void UpdateTime()
	{
		var calendar = clientApi.World.Calendar;
		facts.Time = calendar.HourOfDay / calendar.HoursPerDay;
		facts.Now = GetNow();
		facts.SecondsSinceLastDamage = timeLastDamageTaken > 0
			? (float)(facts.Now - timeLastDamageTaken) / 1000L
			: float.PositiveInfinity;
	}

	private void UpdateHeight()
	{
		var playerPosition = PlayerEntity.Pos;
		var playerHeight = (float)playerPosition.Y;
		var terrainHeight = clientApi.World.BlockAccessor.GetTerrainMapheightAt(playerPosition.AsBlockPos);
		facts.RelativeHeight = MoreMath.Normalize(playerHeight, 0, seaLevel, worldHeight);
		facts.DistanceToSurface = terrainHeight - playerHeight;
	}

	private Entity[] FetchNearbyEnemies()
	{
		const float maxHorizontalDistance = SituationalFacts.EnemyDistanceMax;
		const float maxVerticalDistance = 15;

		return clientApi.World.GetEntitiesAround(
			PlayerEntity.Pos.XYZ,
			maxHorizontalDistance,
			maxVerticalDistance,
			IsEntityEnemy
		);
	}

	private void UpdateEnemyDistance(Entity[] nearbyEnemies)
	{
		var playerPos = PlayerEntity.Pos.XYZFloat;

		float closestDistance = float.PositiveInfinity;
		float closestVisibleDistance = float.PositiveInfinity;

		foreach (var entity in nearbyEnemies)
		{
			float distance = MoreMath.DistanceWithWeightedVerticality(entity.Pos.XYZFloat, playerPos, 3f);
			if (distance < closestDistance)
			{
				closestDistance = distance;
			}

			if (!IsEntityVisible(entity))
			{
				continue;
			}

			if (distance < closestVisibleDistance)
			{
				closestVisibleDistance = distance;
			}
		}

		facts.EnemyDistance = closestDistance;
		facts.VisibleEnemyDistance = closestVisibleDistance;
	}

	private bool IsEntityVisible(Entity entity)
	{
		try
		{
			var playerEyePos = PlayerEntity.Pos.XYZ + PlayerEntity.LocalEyePos;
			clientApi.World.RayTraceForSelection(
				playerEyePos,
				entity.Pos.XYZ,
				ref raytraceIntersectionBlock,
				ref raytraceIntersectionEntity,
				efilter: _ => false);

			switch (raytraceIntersectionBlock?.Block?.BlockMaterial)
			{
				case EnumBlockMaterial.Stone:
				case EnumBlockMaterial.Ore:
				case EnumBlockMaterial.Metal:
				case EnumBlockMaterial.Mantle:
				case EnumBlockMaterial.Brick:
				case EnumBlockMaterial.Ceramic:
				case EnumBlockMaterial.Wood:
				case EnumBlockMaterial.Soil:
				case EnumBlockMaterial.Gravel:
				case EnumBlockMaterial.Sand:
				case EnumBlockMaterial.Snow:
				case EnumBlockMaterial.Ice:
					return false;
				default:
					return true;
			}
		}
		catch (IndexOutOfRangeException)
		{
			// ignore
			// workaround for https://github.com/anegostudios/VintageStory-Issues/issues/5126
			return false;
		}
	}

	private void UpdateIsAttacked(Entity[] nearbyEnemies)
	{
		bool isCurrentlyAttacked = false;
		var playerPos = PlayerEntity.Pos.XYZ;

		// Use the already fetched nearby entities
		foreach (var entity in nearbyEnemies)
		{
			// Check if entity is attacking (animation check)
			var attackAnimation = entity.AnimManager?.IsAnimationActive("attack") ?? false;
			var hurtAnimation = entity.AnimManager?.IsAnimationActive("hurt") ?? false;
			var runAnimation = entity.AnimManager?.IsAnimationActive("run") ?? false;

			// Check if entity is moving toward player
			var entityToPlayerVec = playerPos.SubCopy(entity.Pos.XYZ);
			entityToPlayerVec.Y = 0; // Ignore height difference for direction check
			entityToPlayerVec = entityToPlayerVec.Normalize();

			var entityMovementVec = entity.Pos.Motion.Clone();
			entityMovementVec.Y = 0; // Ignore vertical motion

			var movingTowardPlayer = false;
			if (entityMovementVec.Length() > 0.01) // Entity is moving
			{
				entityMovementVec = entityMovementVec.Normalize();
				var dot = entityMovementVec.Dot(entityToPlayerVec);
				movingTowardPlayer = dot > 0.3; // Entity is generally moving toward player
			}

			// Entity is considered attacking if:
			// 1. It's in attack animation, OR
			// 2. It's running toward the player (walk animations excluded per request)
			if (attackAnimation || hurtAnimation || (movingTowardPlayer && runAnimation))
			{
				isCurrentlyAttacked = true;
				break;
			}
		}

		long now = GetNow();
		if (isCurrentlyAttacked)
		{
			timeLastAttackDetected = now;
		}

		facts.SecondsSinceLastAttack = (float)(now - timeLastAttackDetected) / 1000L;
	}

	private void UpdateRiftDistance()
	{
		Vec3d playerPos = PlayerEntity.Pos.XYZ;
		float sqrDistance = riftModSystem.nearestRifts?
			.Select(r => (float?)playerPos.SquareDistanceTo(r.Position))
			.DefaultIfEmpty()
			.Min() ?? float.PositiveInfinity;
		facts.RiftDistance = MathF.Sqrt(sqrDistance);
	}

	private void UpdateSunFacts()
	{
		facts.SunLevel = VintageSymphony.ClientMain.playerProperties.sunSlight;
		facts.DayLight = VintageSymphony.ClientMain.playerProperties.DayLight;
	}

	private void UpdateAlive()
	{
		facts.Alive = VintageSymphony.ClientMain.EntityPlayer.Alive;
	}

	private void UpdateResonators()
	{
		const int radius = SituationalFacts.PlayingResonatorDistanceMax;
		var playerPos = PlayerEntity.Pos.AsBlockPos;
		var blockAccessor = clientApi.World.GetLockFreeBlockAccessor();
		facts.PlayingResonatorDistance = float.PositiveInfinity;

		blockAccessor.WalkBlocks(
			playerPos.SubCopy(radius, radius, radius),
			playerPos.AddCopy(radius, radius, radius),
			(block, x, y, z) =>
			{
				if (!block?.Code?.PathStartsWith("resonator") ?? true)
				{
					return;
				}

				var blockPos = new BlockPos(x, y, z);
				var playing = blockAccessor.GetBlockEntity<BlockEntityResonator>(blockPos)?.IsPlaying ?? false;
				if (!playing)
				{
					return;
				}

				var distance = blockPos.DistanceTo(playerPos);
				if (distance < facts.PlayingResonatorDistance)
				{
					facts.PlayingResonatorDistance = distance;
				}
			});
	}

	private static bool IsBedBlock(Block? block)
	{
		return block?.Code.PathStartsWith("bed") ?? false;
	}

	private bool IsEntityEnemy(Entity entity)
	{
		
		if (!entity.IsCreature || !entity.Alive)
		{
			return false;
		}

		if (enemyCodeCache.TryGetValue(entity.Code, out bool isEnemy))
		{
			return isEnemy;
		}

		isEnemy = false;
		for (int i = 0; i < EnemyTypes.Length; i++)
		{
			if (entity.Code.PathStartsWith(EnemyTypes[i])
			    && !entity.Code.Path.Contains("baby")
			    && !entity.Code.Path.Contains("hacked"))
			{
				isEnemy = true;
				break;
			}
		}

		enemyCodeCache[entity.Code] = isEnemy;
		return isEnemy;
	}

	static BlockSelection? raytraceIntersectionBlock = new();
	static EntitySelection? raytraceIntersectionEntity = new();
}