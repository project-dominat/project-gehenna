using System.Numerics;
using Content.Shared.CCVar;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Configuration;

namespace Content.Shared.Weapons.Ranged.Systems;

public sealed class SharedGunRecoilSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private float _recoilKickMultiplier;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_cfg, CCVars.WeaponAimRecoilKickMultiplier, v => _recoilKickMultiplier = v, true);
    }

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
            var recoilScale = recoil.RecoilKickMultiplier * recoil.RecoilScale * gun.Comp.CameraRecoilScalarModified;
            kick = recoil.Kick * recoilScale;
            lateral = recoil.LateralKick * recoilScale;
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

        var multiplier = float.IsFinite(_recoilKickMultiplier)
            ? _recoilKickMultiplier
            : 1f;

        return result * multiplier;
    }
}
