using System.Collections.Generic;
using CoolTools.Utilities;

namespace CoolTools.Actors
{
    public class ProjectileSystem : Singleton<ProjectileSystem>
    {
        private static List<Projectile> _registeredProjectiles = new();
        
        public static void RegisterProjectile(Projectile projectile)
        {
            
        }
        
        public static void UnregisterProjectile(Projectile projectile)
        {
            
        }
    }
}