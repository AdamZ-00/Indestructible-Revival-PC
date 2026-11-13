# Indestructible Revival

Play **Indestructible** on a Windows PC using an Android emulator.

---

## Download

Download the ready-to-play package (`Indestructible_Revival_PC_v1.0.zip`) from the [**GitHub Releases**](https://github.com/AdamZ-00/Indestructible-Revival-PC/releases/latest) page.

---

## Requirements

- **Windows PC**
- **Android emulator** with 32-bit ARM (`armeabi-v7a`) support:
  - **LDPlayer 9** *(recommended)*
  - **BlueStacks** *(use a 32-bit instance)*
- ~150 MB of free space

---

## How to Install

1. **Extract the entire ZIP** into a folder (right-click > *Extract All...*).
   - Do not run the installer from inside the ZIP window.
2. **Open your emulator** and wait for the Android home screen.
   - Keep only **one** emulator instance open.
3. **Enable ADB debugging** *(one-time setup)*:
   - **LDPlayer 9:** Menu (`☰`, top-right) > **Settings** > **Other settings** > **ADB debugging** > **Open local connection** > **Save**.
   - **BlueStacks:** **Settings** > **Advanced** > turn on **Android Debug Bridge (ADB)**.
4. **Double-click `INSTALL_GAME.bat`**.
   - If Windows shows *"Windows protected your PC"*, click **More info** > **Run anyway**.
   - Wait for **`[SUCCESS]`**.
5. **Launch "Indestructible"** from the emulator home screen.
   - If Android shows *"This app was built for an older version of Android"*, tap **OK**.

---

## Notes & Troubleshooting

- **Starter save & offline play:** A starter save is created automatically on first launch, and solo modes work offline.
- **Reinstalling:** Running `INSTALL_GAME.bat` again updates the game without deleting your saves.
- **`No emulator detected`:** Check that the emulator finished booting and ADB debugging is enabled and saved (Step 3), then press any key in the installer window to retry.
- **`INSTALL_FAILED_NO_MATCHING_ABIS`:** Your emulator is 64-bit only. Use **LDPlayer 9** or a 32-bit instance.
- **Multiplayer (Photon):** Prompts for a Photon App ID and a room code if you enter multiplayer.

---

## Transparency & Package Contents

- [`INSTALL_GAME.bat`](INSTALL_GAME.bat) — Plain-text Windows ADB installer (fully readable in this repository).
- [`src/`](src/) — Custom revival UI scripts (`PhotonAppIDPrompt.cs`, `CustomMatchPrompt.cs`) and the patch tool used on `Assembly-CSharp.dll`.
- `tools/` — Official Google Android SDK Platform-Tools (`adb.exe`, digitally signed by **Google LLC** — verify via *Right-click > Properties > Digital Signatures*).
- `com.glu.indestructible.standalone.apk` & `main.300.com.glu.indestructible.obb` — Game binaries included in the Release ZIP *(original assets belong to their respective copyright holders)*.
