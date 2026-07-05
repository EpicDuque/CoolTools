using System;
using UnityEngine;
using CoolTools.Utilities;
using UnityEngine.SceneManagement;

namespace CoolTools.Actors
{
    [CreateAssetMenu(fileName = "New Launcher Module", menuName = "Modules/Launcher/Linear (Default)", order = 0)]
    public class LauncherModule : ScriptableObject
    {
        private int _launchAmount = 1;
        public bool UsePool { get; set; }

        public virtual int LaunchAmount
        {
            get => _launchAmount;
            set =>  _launchAmount = 1;
        }

        // ReSharper disable Unity.PerformanceAnalysis
        public virtual void Launch(ProjectileLauncher launcher, ObjectPool pool)
        {
            for (int i = 0; i < LaunchAmount; i++)
            {
                var spawnPosition = GetSpawnPosition(launcher, i);
                var spawnRotation = GetSpawnRotation(launcher, i);
                
                var p = CreateProjectile(launcher, pool, spawnPosition, spawnRotation);
                
                launcher.Events.OnLaunchedProjectile.Invoke(p);
            }
        }

        protected virtual Vector3 GetSpawnPosition(ProjectileLauncher launcher, int index)
        {
            return launcher.LaunchPoint.position;
        }
        
        protected virtual Quaternion GetSpawnRotation(ProjectileLauncher launcher, int index)
        {
            return launcher.LaunchPoint.rotation;
        }

        protected virtual Projectile CreateProjectile(ProjectileLauncher launcher, ObjectPool pool, Vector3 position, Quaternion rotation)
        {
            var prefab = launcher.ProjectilePrefab;

            Projectile p = null;
            if (UsePool)
            {
                p = launcher.SpawnWithOwnership(prefab, position, rotation, pool);
            }
            else
            {
                p = launcher.SpawnWithOwnership(prefab, position, rotation);
            }

            p.Launcher = launcher;
            
            switch (launcher.ETargetMode)
            {
                case ProjectileLauncher.TargetMode.None:
                    p.ReleaseTarget();
                    break;
                case ProjectileLauncher.TargetMode.Detectable:
                    p.SetTarget(launcher.Target);
                    break;
                case ProjectileLauncher.TargetMode.Position:
                    break;
            }

            p.Initialize();
            p.RB.linearVelocity = GetInitialVelocity(launcher, p);

            return p;
        }

        protected virtual Vector3 GetInitialVelocity(ProjectileLauncher launcher, Projectile p)
        {
            return p.transform.forward * launcher.LaunchForce.Value;
        }
    }
}