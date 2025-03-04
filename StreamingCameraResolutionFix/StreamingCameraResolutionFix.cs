using Elements.Core;
using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System;

namespace StreamingCameraResolutionFix {
	public class StreamingCameraResolutionFix : ResoniteMod {
		internal const string VERSION_CONSTANT = "0.0.1";
		public override string Name => "StreamingCameraResolutionFix";
		public override string Author => "troyBORG";
		public override string Version => VERSION_CONSTANT;
		public override string Link => "https://github.com/troyBORG/StreamingCameraResolutionFix/";

		public static ModConfiguration config = null!;

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int2> STREAMING_RESOLUTION =
		new ModConfigurationKey<int2>("Streaming Camera Resolution", "Resolution for the Streaming Camera", () => new int2(1920, 1080));

		public override void OnEngineInit() {
			config = GetConfiguration();
			Harmony harmony = new Harmony($"dev.{Author}.{Name}");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(InteractiveCamera), "OnCommonUpdate")]
		static class InteractiveCameraResolutionPatch {
			[HarmonyPostfix]
			static void ChangeResolution(InteractiveCamera __instance) {
				// Ensure config is initialized
				if (config == null) return;

				// Ensure Userspace is valid
				if (Userspace.UserspaceWorld?.LocalUser == null) return;
				User localUser = Userspace.UserspaceWorld.LocalUser;

				// Get the user's root slot
				Slot userSlot = localUser.Root.Slot;
				if (userSlot == null) return;

				// Ensure we are modifying the local user's InteractiveCamera
				if (!__instance.Slot.IsChildOf(userSlot)) return;

				// Get the resolution from mod settings
				int2 customResolution = config.GetValue(STREAMING_RESOLUTION);

				// Apply the resolution to InteractiveCamera properties
				__instance.PreviewWidth.Value = customResolution.x / 2;  // Following observed behavior
				__instance.PreviewHeight.Value = customResolution.y / 2;
				__instance.RenderWidth.Value = customResolution.x;  // Full resolution width

				Msg($"[StreamingCameraResolutionFix] Set InteractiveCamera resolution to {customResolution.x}x{customResolution.y}");
			}
		}
	}
}
