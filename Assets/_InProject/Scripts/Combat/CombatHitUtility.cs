using UnityEngine;

public interface IDamageable
{
    bool IsAlive { get; }
    bool ReceiveDamage(int damage, GameObject attacker, Vector3 hitPoint);
}

public static class CombatHitUtility
{
    private static readonly Collider[] HitResults = new Collider[16];

    public static int DamageSphere(GameObject attacker, Vector3 center, float radius, int damage)
    {
        int count = Physics.OverlapSphereNonAlloc(
            center,
            radius,
            HitResults,
            Physics.AllLayers,
            QueryTriggerInteraction.Collide);

        int hitCount = 0;
        IDamageable[] damagedTargets = new IDamageable[HitResults.Length];
        int damagedTargetCount = 0;

        for (int i = 0; i < count; i++)
        {
            Collider hit = HitResults[i];
            HitResults[i] = null;
            if (hit == null || hit.transform.IsChildOf(attacker.transform))
                continue;

            IDamageable target = FindDamageable(hit.transform);
            if (target == null || !target.IsAlive || Contains(damagedTargets, damagedTargetCount, target))
                continue;

            damagedTargets[damagedTargetCount++] = target;
            Vector3 hitPoint = hit.ClosestPoint(center);
            if (target.ReceiveDamage(Mathf.Max(0, damage), attacker, hitPoint))
                hitCount++;
        }

        return hitCount;
    }

    private static IDamageable FindDamageable(Transform transform)
    {
        MonoBehaviour[] behaviours = transform.GetComponentsInParent<MonoBehaviour>(true);
        foreach (MonoBehaviour behaviour in behaviours)
        {
            if (behaviour is IDamageable damageable)
                return damageable;
        }

        return null;
    }

    private static bool Contains(IDamageable[] targets, int count, IDamageable target)
    {
        for (int i = 0; i < count; i++)
        {
            if (ReferenceEquals(targets[i], target))
                return true;
        }

        return false;
    }
}