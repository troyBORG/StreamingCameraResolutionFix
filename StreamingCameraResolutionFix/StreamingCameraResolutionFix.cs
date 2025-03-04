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

		public static ModConfiguration config = null!; // Ensure it's initialized later

		[AutoRegisterConfigKey]
		private static ModConfigurationKey<int2> STREAMING_RESOLUTION =
		new ModConfigurationKey<int2>("Streaming Camera Resolution", "Resolution for the Streaming Camera", () => new int2(1920, 1080));

		public override void OnEngineInit() {
			config = GetConfiguration();
			Harmony harmony = new Harmony($"dev.{Author}.{Name}");
			harmony.PatchAll();
		}

		[HarmonyPatch(typeof(RenderTextureProvider), "OnCommonUpdate")]
		static class StreamingCameraResolutionPatch {
			[HarmonyPostfix]
			static void ChangeResolution(RenderTextureProvider __instance) {
				// Ensure config is initialized
				if (config == null) return;

				// Get the local user
				User localUser = Userspace.UserspaceWorld?.LocalUser;
				if (localUser == null) return;

				// Get the user's slot (where their avatar and camera are stored)
				Slot userSlot = localUser.ActiveUserRoot;
				if (userSlot == null) return;

				// Ensure this RenderTextureProvider is inside the user's slot and belongs to a Streaming Camera
				if (!__instance.Slot.IsChildOf(userSlot)) return;

				// Get the resolution from mod settings
				int2 customResolution = config.GetValue(STREAMING_RESOLUTION);
				__instance.Size.Value = customResolution;

				Msg($"[StreamingCameraResolutionFix] Set Streaming Camera resolution to {customResolution.x}x{customResolution.y}");
			}
		}
	}
}
