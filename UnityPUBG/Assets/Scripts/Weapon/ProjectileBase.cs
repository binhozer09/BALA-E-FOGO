using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityPUBG.Scripts.Entities;
using UnityPUBG.Scripts.Items;
using UnityPUBG.Scripts.Logic;
using UnityPUBG.Scripts.Utilities;

namespace UnityPUBG.Scripts
{
    [RequireComponent(typeof(SphereCollider), typeof(Rigidbody))]
    public class ProjectileBase : PoolObject
    {
        private ProjectileInfo projectileInfo;
        private SphereCollider projectileSphereCollider;
        private Rigidbody projectileRigidbody;

        private readonly static string myPlayerTag = "MyPlayer";

        public bool isFired { get; private set; }

        #region 유니티 메시지
        private void Awake()
        {
            projectileSphereCollider = GetComponent<SphereCollider>();
            projectileSphereCollider.isTrigger = true;
            projectileRigidbody = GetComponent<Rigidbody>();
            projectileRigidbody.useGravity = false;
        }

        private void FixedUpdate()
        {
            if (isFired)
            {
                projectileRigidbody.MovePosition(transform.position + projectileInfo.fireDirection * projectileInfo.speed * Time.fixedDeltaTime);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            // A projectile must never damage its own player, including child colliders.
            if (other.transform.root.CompareTag(myPlayerTag))
            {
                return;
            }

            var damageableEntity = other.gameObject.GetComponent<IDamageable>();
            if (damageableEntity == null)
            {
                damageableEntity = other.GetComponentInParent<IDamageable>();
            }

            if (damageableEntity != null)
            {
                damageableEntity.OnTakeDamage(projectileInfo.damage, projectileInfo.damageType);

                if (other.attachedRigidbody != null)
                {
                    other.attachedRigidbody.AddForce(transform.forward * projectileInfo.knockbackPower);
                }

                SaveToPool();
            }
        }
        #endregion

        #region PoolObject
        public override void OnObjectReuse()
        {
            CancelInvoke(nameof(SaveToPool));
            isFired = false;
        }

        public override void OnObjectSaveToPool()
        {
            CancelInvoke(nameof(SaveToPool));
            isFired = false;
        }
        #endregion

        public void InitializeProjectile(ProjectileInfo projectileInfo, Vector3 startPosition)
        {
            this.projectileInfo = projectileInfo;
            transform.position = startPosition;
            transform.LookAt(transform.position + projectileInfo.fireDirection);
            projectileSphereCollider.radius = projectileInfo.colliderRadius;
        }

        public void Fire()
        {
            CancelInvoke(nameof(SaveToPool));
            isFired = true;
            Invoke(nameof(SaveToPool), Mathf.Max(0.01f, projectileInfo.lifeTime));
        }

        private void SaveToPool()
        {
            ObjectPoolManager.Instance.SaveObjectToPool(this);
        }

        public struct ProjectileInfo
        {
            public readonly Vector3 fireDirection;
            public readonly float speed;
            public readonly float lifeTime;
            public readonly float colliderRadius;

            public readonly float damage;
            public readonly DamageType damageType;
            public readonly float knockbackPower;

            public ProjectileInfo(Vector3 fireDirection, float speed, float lifeTime, float colliderRadius, float damage, DamageType damageType, float knockbackPower)
            {
                this.fireDirection = fireDirection;
                this.speed = speed;
                this.lifeTime = lifeTime;
                this.colliderRadius = colliderRadius;
                this.damage = damage;
                this.damageType = damageType;
                this.knockbackPower = knockbackPower;
            }
        }

    }
}
