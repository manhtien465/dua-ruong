using System.Collections.Generic;
using UnityEngine;

namespace DuaRuong.Systems.Pool
{
    /// <summary>
    /// Generic component pool. Avoids Instantiate / Destroy in the gameplay loop.
    /// Pooled objects must inherit <see cref="MonoBehaviour"/> and may implement <see cref="IPoolable"/>
    /// for spawn / despawn callbacks.
    /// </summary>
    public sealed class ObjectPool<T> where T : MonoBehaviour
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Stack<T> _idle;
        private readonly int _maxSize;

        public int CountIdle => _idle.Count;

        public ObjectPool(T prefab, int prewarm = 0, int maxSize = 256, Transform parent = null)
        {
            if (prefab == null) throw new System.ArgumentNullException(nameof(prefab));

            _prefab = prefab;
            _parent = parent;
            _maxSize = Mathf.Max(maxSize, prewarm);
            _idle = new Stack<T>(prewarm);

            for (int i = 0; i < prewarm; i++)
            {
                var obj = CreateInstance();
                obj.gameObject.SetActive(false);
                _idle.Push(obj);
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj = _idle.Count > 0 ? _idle.Pop() : CreateInstance();

            var t = obj.transform;
            t.SetPositionAndRotation(position, rotation);
            obj.gameObject.SetActive(true);

            if (obj is IPoolable p) p.OnSpawned();
            return obj;
        }

        public void Release(T obj)
        {
            if (obj == null) return;

            if (obj is IPoolable p) p.OnDespawned();

            obj.gameObject.SetActive(false);

            if (_idle.Count >= _maxSize)
            {
                Object.Destroy(obj.gameObject);
                return;
            }

            obj.transform.SetParent(_parent, worldPositionStays: false);
            _idle.Push(obj);
        }

        public void Clear()
        {
            while (_idle.Count > 0)
            {
                var obj = _idle.Pop();
                if (obj != null) Object.Destroy(obj.gameObject);
            }
        }

        private T CreateInstance()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.name = _prefab.name;
            return obj;
        }
    }
}
