using System.Collections.Generic;
using UnityEngine;

namespace LiftStudio.Pools
{
    public abstract class GenericObjectPool<T> : MonoBehaviour where T : Component
    {
        [SerializeField] private T prefab;

        private readonly Queue<T> _objects = new Queue<T>();

        public T Get(Transform parentTransform)
        {
            if (_objects.Count == 0)
            {
                AddObjects(parentTransform);
            }
            
            return _objects.Dequeue();
        }

        public void ReturnToPool(T objectToReturn)
        {
            objectToReturn.gameObject.SetActive(false);
            _objects.Enqueue(objectToReturn);
        }

        private void AddObjects(Transform parentTransform)
        {
            var newObject = Instantiate(prefab, parentTransform);
            newObject.gameObject.SetActive(false);
            _objects.Enqueue(newObject);
        }
    }
}