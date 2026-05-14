using DuaRuong.Gameplay.Items;
using DuaRuong.Gameplay.Obstacles;
using UnityEngine;

namespace DuaRuong.Gameplay.Track
{
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "DuaRuong/Configs/Spawn")]
    public sealed class SpawnConfig : ScriptableObject
    {
        [Header("Chunk")]
        [SerializeField, Min(1)] private int _initialChunkCount = 4;
        [SerializeField, Min(0.1f)] private float _chunkLength = 30f;
        [SerializeField] private TrackChunk[] _chunkPrefabs;

        [Header("Obstacle Prefabs")]
        [SerializeField] private ObstacleBase[] _obstaclePrefabs;

        [Header("Item Prefabs")]
        [SerializeField] private ItemBase[] _itemPrefabs;

        [Header("Difficulty Curves (input: distance in metres)")]
        [SerializeField] private AnimationCurve _obstacleDensityByDistance =
            AnimationCurve.Linear(0f, 0.3f, 1500f, 1f);
        [SerializeField] private AnimationCurve _itemRatioByDistance =
            AnimationCurve.Linear(0f, 0.5f, 1500f, 0.15f);

        public int InitialChunkCount => _initialChunkCount;
        public float ChunkLength => _chunkLength;
        public TrackChunk[] ChunkPrefabs => _chunkPrefabs;
        public ObstacleBase[] ObstaclePrefabs => _obstaclePrefabs;
        public ItemBase[] ItemPrefabs => _itemPrefabs;

        public float ObstacleDensityAt(float distance) => _obstacleDensityByDistance.Evaluate(distance);
        public float ItemRatioAt(float distance) => _itemRatioByDistance.Evaluate(distance);
    }
}
