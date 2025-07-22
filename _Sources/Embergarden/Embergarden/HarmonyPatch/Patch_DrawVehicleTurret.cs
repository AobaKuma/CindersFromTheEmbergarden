using Verse;
using UnityEngine;
using HarmonyLib;

namespace Embergarden
{
    [HarmonyPatch(typeof(PawnRenderUtility), nameof(PawnRenderUtility.DrawEquipmentAndApparelExtras))]
    internal static class Patch_DrawVehicleTurret
    {
        [HarmonyPriority(600)]
        public static bool Prefix(Pawn pawn, Vector3 drawPos, Rot4 facing, PawnRenderFlags flags)
        {

            CompVehicleWeapon compWeapon = CompVehicleWeapon.cachedVehicldesPawns.TryGetValue(pawn);

            if (compWeapon != null)
            {
                Pawn vehicle = (Pawn)compWeapon.parent;
                if (vehicle.equipment != null && vehicle.equipment.Primary != null)
                {
                    DrawTuret(vehicle, compWeapon, vehicle.equipment.Primary);
                }
                return false;
            }
            return true;
        }
        public static void DrawTuret(Pawn pawn, CompVehicleWeapon compWeapon, Thing equipment)
        {
            float aimAngle = compWeapon.CurrentAngle;
            Vector3 drawLoc = pawn.DrawPos + compWeapon.GetOffsetByRot();
            drawLoc.y += Altitudes.AltInc * compWeapon.Props.drawData.LayerForRot(pawn.Rotation, 1);
            float num = aimAngle - 90f;
            num += equipment.def.equippedAngleOffset;
            Mesh mesh;
            mesh = MeshPool.plane10;
            num %= 360f;

            Vector3 drawSize = compWeapon.Props.drawSize != 0 ? Vector3.one * compWeapon.Props.drawSize : (Vector3)equipment.Graphic.drawSize;
            Matrix4x4 matrix = Matrix4x4.TRS(drawLoc, Quaternion.AngleAxis(num, Vector3.up), new Vector3(drawSize.x, 1f, drawSize.y));
            var mat = (!(equipment.Graphic is Graphic_StackCount graphic_StackCount)) ?
                equipment.Graphic.MatSingle :
                graphic_StackCount.SubGraphicForStackCount(1, equipment.def).MatSingle;

            Graphics.DrawMesh(mesh, matrix, mat, 0);
        }
    }
}
