using System.Threading.Tasks;

namespace SuperSigns.Patch;

[HarmonyPatch]
public static class PingSign
{
    [HarmonyPatch(typeof(Sign), nameof(Sign.Awake)), HarmonyWrapSafe]
    private static class Patch_SignAwake
    {
        [UsedImplicitly]
        private static async void Postfix(Sign __instance)
        {
            while (true)
            {
                await Task.Delay(1000);
                if (__instance == null)
                {
                    DebugWarning("Sign was destroyed");
                    return;
                }

                Debug("Sign ping at " + __instance.transform.position.RoundCords());
                Debug($"referencePosition = {ZNet.instance.GetReferencePosition().RoundCords().ToSimpleVector3()}");
                if (Input.GetKey(KeyCode.Keypad7)) return;
            }
        }
    }
}