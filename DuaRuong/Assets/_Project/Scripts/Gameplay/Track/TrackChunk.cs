using System;
using System.Collections.Generic;
using DuaRuong.Systems.Pool;
using UnityEngine;

namespace DuaRuong.Gameplay.Track
{
    /// <summary>
    /// One pre-authored segment of the level. Spawn points act as candidate slots
    /// for obstacles or items. The <see cref="TrackGenerator"/> decides per spawn
    /// whether to fill, what to fill with, and which lane.
    /// </summary>
    public sealed class TrackChunk : MonoBehaviour, IPoolable
    {
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _endPoint;
        [SerializeField] private Transform[] _spawnPoints;

        private readonly List<Action> _releaseCallbacks = new(8);

        public Transform StartPoint => _startPoint;
        public Transform EndPoint => _endPoint;
        public Transform[] SpawnPoints => _spawnPoints;

        public float Length => Vector3.Distance(_startPoint.position, _endPoint.position);

        public void RegisterRelease(Action callback) => _releaseCallbacks.Add(callback);

        public void OnSpawned() { }

        public void OnDespawned()
        {
            foreach (var cb in _releaseCallbacks) cb();
            _releaseCallbacks.Clear();
        }
    }
}
