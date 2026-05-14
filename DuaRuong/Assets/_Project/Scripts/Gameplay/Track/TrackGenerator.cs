using System.Collections.Generic;
using DuaRuong.Core;
using DuaRuong.Gameplay.Items;
using DuaRuong.Gameplay.Obstacles;
using DuaRuong.Systems.Pool;
using UnityEngine;

namespace DuaRuong.Gameplay.Track
{
    /// <summary>
    /// Streams track chunks ahead of the player and recycles chunks that fall behind.
    /// Chunks spawn from per-prefab pools so total allocations are bounded.
    /// After spawning a chunk, PopulateChunk fills its spawn points with obstacles/items
    /// based on difficulty curves from SpawnConfig.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TrackGenerator : MonoBehaviour
    {
        [SerializeField] private SpawnConfig _config;
        [SerializeField] private Transform _player;
        [SerializeField] private Transform _chunkParent;

        [SerializeField, Min(0f)] private float _spawnAheadDistance = 60f;
        [SerializeField, Min(0f)] private float _despawnBehindDistance = 20f;

        // Spawn points in Chunk_A are laid out as rows of 3 tiers.
        // PopulateChunk uses this to ensure at most 1 obstacle per row, leaving ≥2 tiers free.
        private const int TiersPerRow = 3;

        private readonly Queue<TrackChunk> _activeChunks = new(8);
        private readonly Dictionary<TrackChunk, ObjectPool<TrackChunk>> _chunkPools = new();
        private readonly Dictionary<ObstacleBase, ObjectPool<ObstacleBase>> _obstaclePools = new();
        private readonly Dictionary<ItemBase, ObjectPool<ItemBase>> _itemPools = new();

        private float _nextChunkZ;

        private void Awake()
        {
            if (_config == null) Debug.LogError($"[{nameof(TrackGenerator)}] SpawnConfig missing.", this);
            if (_player == null) Debug.LogError($"[{nameof(TrackGenerator)}] Player transform missing.", this);
        }

        private void OnEnable()
        {
            GameEvents.GameStarted += HandleGameStarted;
        }

        private void OnDisable()
        {
            GameEvents.GameStarted -= HandleGameStarted;
        }

        private void Update()
        {
            if (_player == null || _config == null) return;

            float playerZ = _player.position.z;

            while (_nextChunkZ - playerZ < _spawnAheadDistance)
            {
                SpawnNextChunk();
            }

            while (_activeChunks.Count > 0)
            {
                var oldest = _activeChunks.Peek();
                if (oldest.EndPoint.position.z >= playerZ - _despawnBehindDistance) break;
                _activeChunks.Dequeue();
                ReleaseChunk(oldest);
            }
        }

        private void HandleGameStarted()
        {
            ClearActive();
            _nextChunkZ = 0f;
            for (int i = 0; i < _config.InitialChunkCount; i++) SpawnNextChunk();
        }

        private void SpawnNextChunk()
        {
            var prefabs = _config.ChunkPrefabs;
            if (prefabs == null || prefabs.Length == 0) return;

            var prefab = prefabs[Random.Range(0, prefabs.Length)];
            var pool = GetOrCreateChunkPool(prefab);

            float chunkBaseZ = _nextChunkZ;
            var chunk = pool.Get(new Vector3(0f, 0f, _nextChunkZ), Quaternion.identity);
            chunk.transform.SetParent(_chunkParent, worldPositionStays: true);

            float length = chunk.Length > 0f ? chunk.Length : _config.ChunkLength;
            _nextChunkZ += length;

            PopulateChunk(chunk, chunkBaseZ);
            _activeChunks.Enqueue(chunk);
        }

        // One obstacle or item per row max — guarantees ≥ 2 free lanes for the player.
        private void PopulateChunk(TrackChunk chunk, float chunkBaseZ)
        {
            var obstacPrefabs = _config.ObstaclePrefabs;
            var itemPrefabs   = _config.ItemPrefabs;
            bool hasObs   = obstacPrefabs != null && obstacPrefabs.Length > 0;
            bool hasItems = itemPrefabs   != null && itemPrefabs.Length > 0;
            if (!hasObs && !hasItems) return;

            var pts = chunk.SpawnPoints;
            if (pts == null || pts.Length == 0) return;

            float density   = _config.ObstacleDensityAt(chunkBaseZ);
            float itemRatio = _config.ItemRatioAt(chunkBaseZ);

            int rowCount = pts.Length / TiersPerRow;
            for (int row = 0; row < rowCount; row++)
            {
                if (Random.value > density) continue;

                int lane = Random.Range(0, TiersPerRow);
                var pt   = pts[row * TiersPerRow + lane];

                if (hasItems && Random.value < itemRatio)
                    SpawnItem(chunk, pt);
                else if (hasObs)
                    SpawnObstacle(chunk, pt);
            }
        }

        private void SpawnObstacle(TrackChunk chunk, Transform spawnPt)
        {
            var prefabs = _config.ObstaclePrefabs;
            var prefab  = prefabs[Random.Range(0, prefabs.Length)];
            var pool    = GetOrCreateObstaclePool(prefab);
            var obs     = pool.Get(spawnPt.position, Quaternion.identity);
            obs.transform.SetParent(chunk.transform, worldPositionStays: true);
            chunk.RegisterRelease(() => pool.Release(obs));
        }

        private void SpawnItem(TrackChunk chunk, Transform spawnPt)
        {
            var prefabs = _config.ItemPrefabs;
            var prefab  = prefabs[Random.Range(0, prefabs.Length)];
            var pool    = GetOrCreateItemPool(prefab);
            var item    = pool.Get(spawnPt.position, Quaternion.identity);
            item.transform.SetParent(chunk.transform, worldPositionStays: true);
            chunk.RegisterRelease(() => pool.Release(item));
        }

        private ObjectPool<TrackChunk> GetOrCreateChunkPool(TrackChunk prefab)
        {
            if (_chunkPools.TryGetValue(prefab, out var pool)) return pool;
            pool = new ObjectPool<TrackChunk>(prefab, prewarm: 2, maxSize: 16, parent: _chunkParent);
            _chunkPools.Add(prefab, pool);
            return pool;
        }

        private ObjectPool<ObstacleBase> GetOrCreateObstaclePool(ObstacleBase prefab)
        {
            if (_obstaclePools.TryGetValue(prefab, out var pool)) return pool;
            pool = new ObjectPool<ObstacleBase>(prefab, prewarm: 3, maxSize: 30, parent: transform);
            _obstaclePools.Add(prefab, pool);
            return pool;
        }

        private ObjectPool<ItemBase> GetOrCreateItemPool(ItemBase prefab)
        {
            if (_itemPools.TryGetValue(prefab, out var pool)) return pool;
            pool = new ObjectPool<ItemBase>(prefab, prewarm: 3, maxSize: 30, parent: transform);
            _itemPools.Add(prefab, pool);
            return pool;
        }

        private void ReleaseChunk(TrackChunk chunk)
        {
            foreach (var kv in _chunkPools)
            {
                if (kv.Key.name == chunk.name)
                {
                    kv.Value.Release(chunk);
                    return;
                }
            }
            Destroy(chunk.gameObject);
        }

        private void ClearActive()
        {
            while (_activeChunks.Count > 0) ReleaseChunk(_activeChunks.Dequeue());
        }
    }
}
