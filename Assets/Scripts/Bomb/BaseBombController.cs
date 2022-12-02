using System.Collections;
using Character;
using Player;
using Terrain;
using UnityEngine;

namespace Bomb
{
    public abstract class BaseBombController : MonoBehaviour
    {
        [SerializeField] protected Explosion explosionPrefabs;
        [SerializeField] protected MeshRenderer meshRenderer;

        protected PlayerBombController _playerBombController;
        protected int _bombPower;
        
        protected bool _hasAlreadyExploded = false;

        private void Start()
        {
            meshRenderer.material = new Material(meshRenderer.material);
            meshRenderer.sharedMaterial.SetFloat("_FuseTime", BombData.FuseTime);
            meshRenderer.sharedMaterial.SetFloat("_SpawnTime", Time.time);
            
            // (temps max - temps écoulé) / temps max
            
        }

        public virtual void SetupBomb(PlayerBombController bombController, int bombPower)
        {
            _playerBombController = bombController;
            _bombPower = bombPower;
            StartCoroutine(StartTimer());
        }

        protected virtual IEnumerator StartTimer()
        {
            yield return new WaitForSeconds(BombData.FuseTime);
            StartExplode();
        }
        
        public virtual void CancelTimer()
        {
            StopAllCoroutines();
        }

        public virtual bool HasAlreadyExploded()
        {
            return _hasAlreadyExploded;
        }
        
        public virtual void StartExplode()
        {
            _hasAlreadyExploded = true;
            Vector3 bombPosition = TerrainUtils.GetTerrainPosition(transform.position);
            
            Explosion baseExplosion = Instantiate(explosionPrefabs, bombPosition, Quaternion.identity);
            baseExplosion.DestroyAfter(BombData.ExplosionDuration);
            
            Explode(bombPosition, Vector3.back, _bombPower);
            Explode(bombPosition, Vector3.right, _bombPower);
            Explode(bombPosition, Vector3.forward, _bombPower);
            Explode(bombPosition, Vector3.left, _bombPower);
            
            if (_playerBombController != null)
            {
                _playerBombController.OnBombExplode();
            }
            
            Destroy(gameObject);
        }

        protected virtual void Explode(Vector3 position, Vector3 direction, int length)
        {
            if (length <= 0) return;

            position += direction;

            Collider[] colliders = Physics.OverlapBox(position, new Vector3(0.49f, 0.49f, 0.49f), Quaternion.identity);

            foreach (Collider c in colliders)
            {
                if (c.CompareTag("Terrain"))
                {
                    return;
                }
                
                if (c.CompareTag("Bomb"))
                {
                    BaseBombController bombController = c.GetComponent<BaseBombController>();
                    if (bombController.HasAlreadyExploded()) return;
                    bombController.CancelTimer();
                    bombController.StartExplode();
                    return;
                }

                if (c.CompareTag("Destructible"))
                {
                    EntityHealth entityHealth = c.GetComponent<EntityHealth>();
                    entityHealth.PerformDamage();
                    length = 0;
                    break;
                }
            }
            
            Explosion explosion = Instantiate(explosionPrefabs, position, Quaternion.identity);
            explosion.DestroyAfter(BombData.ExplosionDuration);
            
            Explode(position, direction, length - 1);
        }
    }
}