using System.Numerics;
using Content.Shared.Weapons.Ranged.Components;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class SharedGunRecoilSystem : EntitySystem
{
    public Vector2 GetCameraKick(Entity<GunComponent> gun, Vector2 recoilDirection)
    {
        if (recoilDirection == Vector2.Zero)
            return Vector2.Zero;

        var direction = recoilDirection.Normalized();
        var kick = gun.Comp.CameraRecoilScalarModified * 0.5f;
        var lateral = 0f;
        var maxKick = 1f;

        if (TryComp<GunRecoilComponent>(gun, out var recoil))
        {
            kick = recoil.Kick * gun.Comp.CameraRecoilScalarModified;
            lateral = recoil.LateralKick;
            maxKick = recoil.MaxKick;
        }

        var result = direction * kick;
        if (lateral != 0f)
        {
            var side = new Vector2(-direction.Y, direction.X);
            var sign = (gun.Comp.ShotCounter & 1) == 0 ? 1f : -1f;
            result += side * lateral * sign;
        }

        if (maxKick > 0f && result.Length() > maxKick)
            result = result.Normalized() * maxKick;

        return result;
    }
}
