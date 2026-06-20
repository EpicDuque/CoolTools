using UnityEngine;

namespace CoolTools.Utilities
{
    public static class RigidbodyExtensions
    {
        /// <summary>
        /// Pushes a rigidbody to <paramref name="target"/> in <paramref name="period"/> seconds. The longer
        /// the time the higher the movement arch is.
        /// </summary>
        /// <param name="target">Where the rigidbody needs top land.</param>
        /// <param name="period">Time in seconds to take for the movement.</param>
        /// <param name="launch">If true, the rigidbody will start moving immediately.</param>
        public static void ImpulseToLandOn(this Rigidbody rb, Vector3 target, float period, bool launch, out Vector3 initVelocity)
        {
            rb.isKinematic = false;
            rb.linearDamping = 0f;
            rb.useGravity = true;

            var myPos = rb.transform.position;

            var velZ = (target.z - myPos.z) / period;
            var velX = (target.x - myPos.x) / period;
            var velY = (target.y - myPos.y - 0.5f * Physics.gravity.y * Mathf.Pow(period, 2)) / period;

            initVelocity = new Vector3(velX, velY, velZ);

            if(launch)
                rb.linearVelocity = initVelocity;
        }
    }
}