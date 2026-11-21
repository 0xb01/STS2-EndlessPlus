using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;

namespace BNKZ.STS2.EndlessPlus;

/// <summary>
/// Harmony patches and UI hooks for the Victory / Game Over screen.
/// Injects the "Endless" and "Force Endless" action buttons and manages multiplayer ready synchronizations.
/// </summary>
internal static class GameOverScreenPatches
{
	private sealed class GameOverScreenState
	{
		public bool LobbyReadyInitialized { get; set; }

		public bool SawHostReady { get; set; }

		public bool RemoteLaunchTriggered { get; set; }
	}

	private const string EndlessButtonName = "BNKZEndlessModeButton";

	private const string ForceButtonName = "BNKZEndlessModeForceButton";

	private const string RefreshTimerName = "BNKZEndlessModeRefreshTimer";

	private static readonly ConditionalWeakTable<Node, GameOverScreenState> ScreenStates = new ConditionalWeakTable<Node, GameOverScreenState>();

	private static readonly string[] ButtonsToBlockWhileLaunching = new string[3] { "NReturnToMainMenuButton", "NGameOverContinueButton", "NViewRunButton" };

	/// <summary>
	/// Postfix patch invoked after the Game Over screen initializes.
	/// Injects custom Endless buttons into the UI hierarchy adjacent to Return to Main Menu.
	/// </summary>
	public static void HandleAfterReady(object __instance)
	{
		try
		{
			Node screen = (Node)((__instance is Node) ? __instance : null);
			if (screen == null || FindDescendant(screen, (Node node) => ((object)node.Name).ToString() == EndlessButtonName) != null)
			{
				return;
			}
			EnsureInitialReadyState(screen);
			Node? val = FindDescendant(screen, (Node node) => ((object)node).GetType().Name == "NReturnToMainMenuButton");
			Control val2 = (Control)(object)((val is Control) ? val : null);
			if (val2 == null)
			{
				EndlessLog.Info("Game over screen found, but the main menu button could not be located.");
				return;
			}
			Node parent = ((Node)val2).GetParent();
			Control val3 = (Control)(object)((parent is Control) ? parent : null);
			if (val3 == null)
			{
				EndlessLog.Info("Game over screen main menu button has no control parent to attach the endless button to.");
				return;
			}

			Node? template = FindDescendant(screen, (Node node) => ((object)node).GetType().Name == "NGameOverContinueButton") ?? val;
			Control endlessButton;
			if (template is Control templateCtrl)
			{
				endlessButton = (Control)templateCtrl.Duplicate((int)Node.DuplicateFlags.Scripts | (int)Node.DuplicateFlags.Groups);
			}
			else
			{
				endlessButton = new Button
				{
					FocusMode = Control.FocusModeEnum.All,
					CustomMinimumSize = val2.Size,
					Size = val2.Size,
					SizeFlagsHorizontal = val2.SizeFlagsHorizontal,
					SizeFlagsVertical = val2.SizeFlagsVertical,
					Theme = val2.Theme,
					ThemeTypeVariation = val2.ThemeTypeVariation
				};
			}

			endlessButton.Name = (StringName)EndlessButtonName;
			endlessButton.TooltipText = GetEndlessButtonTooltip(null, null);

			Button forceButton = new Button
			{
				Name = (StringName)ForceButtonName,
				Text = "Force Endless",
				TooltipText = "Start the next endless loop without waiting for every player",
				FocusMode = Control.FocusModeEnum.All,
				CustomMinimumSize = val2.Size,
				Size = val2.Size,
				SizeFlagsHorizontal = val2.SizeFlagsHorizontal,
				SizeFlagsVertical = val2.SizeFlagsVertical,
				Theme = val2.Theme,
				ThemeTypeVariation = val2.ThemeTypeVariation
			};

			((Node)val3).AddChild((Node)(object)endlessButton, false, (Node.InternalMode)0);
			((Node)val3).MoveChild((Node)(object)endlessButton, ((Node)val2).GetIndex(false) + 1);
			((Node)val3).AddChild((Node)(object)forceButton, false, (Node.InternalMode)0);
			((Node)val3).MoveChild((Node)(object)forceButton, ((Node)endlessButton).GetIndex(false) + 1);
			((CanvasItem)forceButton).Visible = false;

			endlessButton.Visible = true;
			endlessButton.Modulate = Colors.White;

			MegaLabel? label = endlessButton.GetNodeOrNull<MegaLabel>("Label");
			if (label != null)
			{
				label.SetTextAutoSize(GetEndlessButtonText(null, null));
			}
			else if (endlessButton is Button btn)
			{
				btn.Text = GetEndlessButtonText(null, null);
			}

			TextureRect? img = endlessButton.GetNodeOrNull<TextureRect>("Image");
			if (img != null && img.Material is ShaderMaterial shaderMat)
			{
				ShaderMaterial newMat = (ShaderMaterial)shaderMat.Duplicate();
				img.Material = newMat;
				try
				{
					endlessButton.Set("_hsv", newMat);
				}
				catch
				{
				}
			}

			try
			{
				endlessButton.Call("UnregisterHotkeys");
			}
			catch
			{
			}

			Control? hotkeyIcon = endlessButton.GetNodeOrNull<Control>("%HotkeyIcon") ?? endlessButton.GetNodeOrNull<Control>("HotkeyIcon");
			if (hotkeyIcon != null)
			{
				hotkeyIcon.Visible = false;
			}

			if (!(val3 is BoxContainer))
			{
				((Control)endlessButton).Position = val2.Position + new Vector2(val2.Size.X + 24f, 0f);
				((Control)forceButton).Position = ((Control)endlessButton).Position + new Vector2(((Control)endlessButton).Size.X + 24f, 0f);
				try
				{
					endlessButton.Set("_showPosition", endlessButton.Position);
				}
				catch
				{
				}
			}

			if (endlessButton is NClickableControl clickable)
			{
				clickable.Connect(NClickableControl.SignalName.Released, Callable.From<NClickableControl>((NClickableControl _) =>
				{
					TaskHelper.RunSafely(OnEndlessButtonPressedAsync(screen, endlessButton, forceButton));
				}));
			}
			else if (endlessButton is BaseButton baseBtn)
			{
				baseBtn.Pressed += () =>
				{
					TaskHelper.RunSafely(OnEndlessButtonPressedAsync(screen, endlessButton, forceButton));
				};
			}

			((BaseButton)forceButton).Pressed += () =>
			{
				TaskHelper.RunSafely(OnForceButtonPressedAsync(screen, endlessButton, forceButton));
			};

			EnsureRefreshTimer(screen);
			RefreshScreenState(screen);
			EndlessLog.Info("Injected Endless button into the game over screen.");
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while injecting the game over screen button.", ex);
		}
	}

	/// <summary>
	/// Click event handler for the Endless Mode continue button. Disables inputs, signals multiplayer peers, and initiates loop transition.
	/// </summary>
	private static async Task OnEndlessButtonPressedAsync(Node screen, Control endlessButton, Control? forceButton)
	{
		try
		{
			object runManager = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManager);
			bool isMultiplayer = EndlessReflection.GetPlayersCount(runState) > 1;
			if (isMultiplayer && !EndlessRuntime.CanLocalPlayerLaunchEndless(runManager, runState))
			{
				EndlessLog.Info("Ignoring endless button on client: only host can start endless mode.");
				return;
			}
			SetControlDisabled(endlessButton, true);
			if (forceButton != null)
			{
				SetControlDisabled(forceButton, true);
			}
			SetButtonsInteractive(screen, isInteractive: false);
			EndlessLog.Info("Endless button pressed on game over screen.");
			await EndlessRuntime.StartLoopFromGameOverScreenAsync(screen, forceReady: true, allowClientRelay: false, broadcastLaunchMarker: isMultiplayer);
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while starting the next endless loop from the game over screen.", ex);
			if (IsAlive((GodotObject?)(object)screen))
			{
				SetButtonsInteractive(screen, isInteractive: true);
			}
			if (IsAlive((GodotObject?)(object)endlessButton))
			{
				SetControlDisabled(endlessButton, false);
			}
			if (forceButton != null && IsAlive((GodotObject?)(object)forceButton))
			{
				SetControlDisabled(forceButton, false);
			}
		}
	}

	/// <summary>
	/// Click event handler for the Force Endless button, bypassing player ready checks to immediately launch the next chapter.
	/// </summary>
	private static async Task OnForceButtonPressedAsync(Node screen, Control endlessButton, Control? forceButton)
	{
		try
		{
			object runManager = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManager);
			if (!EndlessRuntime.CanLocalPlayerLaunchEndless(runManager, runState))
			{
				EndlessLog.Info("Blocked forced endless start on a multiplayer client because only the host may launch the next chapter.");
				RefreshScreenState(screen);
			}
			else
			{
				SetControlDisabled(endlessButton, true);
				if (forceButton != null)
				{
					SetControlDisabled(forceButton, true);
				}
				SetButtonsInteractive(screen, isInteractive: false);
				EndlessLog.Info("Force endless button pressed on game over screen.");
				await EndlessRuntime.StartLoopFromGameOverScreenAsync(screen, forceReady: true);
			}
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while force-starting the next endless loop from the game over screen.", ex);
			if (IsAlive((GodotObject?)(object)screen))
			{
				SetButtonsInteractive(screen, isInteractive: true);
			}
			if (IsAlive((GodotObject?)(object)endlessButton))
			{
				SetControlDisabled(endlessButton, false);
			}
			if (forceButton != null && IsAlive((GodotObject?)(object)forceButton))
			{
				SetControlDisabled(forceButton, false);
			}
		}
	}

	private static void SetControlDisabled(Control? control, bool disabled)
	{
		if (!IsAlive((GodotObject?)(object)control))
		{
			return;
		}
		if (control is BaseButton baseBtn)
		{
			baseBtn.Disabled = disabled;
		}
		else
		{
			control.MouseFilter = disabled ? Control.MouseFilterEnum.Ignore : Control.MouseFilterEnum.Stop;
			control.Modulate = disabled ? new Color(0.6f, 0.6f, 0.6f, 0.8f) : Colors.White;
		}
	}

	public static void TryInjectFromTree(Node contextNode)
	{
		try
		{
			if (IsAlive((GodotObject?)(object)contextNode))
			{
				Node val = FindGameOverScreen((Node)(object)contextNode.GetTree().Root);
				if (val != null)
				{
					HandleAfterReady(val);
				}
			}
		}
		catch
		{
		}
	}

	public static bool BeforeAnyButtonPress(object __instance)
	{
		Node node = (Node)((__instance is Node) ? __instance : null);
		if (node == null)
		{
			return true;
		}
		if (!EndlessRuntime.IsLaunchingFromGameOverScreen)
		{
			if (Array.Exists(ButtonsToBlockWhileLaunching, (string buttonName) => string.Equals(buttonName, ((object)node).GetType().Name, StringComparison.Ordinal)))
			{
				EndlessRuntime.SetPreserveMultiplayerConnectionDuringGameOver(preserve: false, ((object)node).GetType().Name + " pressed");
			}
			return true;
		}
		if (Array.Exists(ButtonsToBlockWhileLaunching, (string buttonName) => string.Equals(buttonName, ((object)node).GetType().Name, StringComparison.Ordinal)))
		{
			EndlessLog.Info("Blocking button press on " + ((object)node).GetType().Name + " while endless loop start is in progress.");
			return false;
		}
		return true;
	}

	private static void SetButtonsInteractive(Node screen, bool isInteractive)
	{
		if (!IsAlive((GodotObject?)(object)screen))
		{
			return;
		}
		string[] buttonsToBlockWhileLaunching = ButtonsToBlockWhileLaunching;
		foreach (string buttonName in buttonsToBlockWhileLaunching)
		{
			Node? val = FindDescendant(screen, (Node node) => ((object)node).GetType().Name == buttonName);
			Control val2 = (Control)(object)((val is Control) ? val : null);
			if (val2 != null)
			{
				((CanvasItem)val2).Visible = isInteractive;
				SetControlDisabled(val2, !isInteractive);
			}
		}
	}

	private static void RefreshScreenState(Node screen)
	{
		if (!IsAlive((GodotObject?)(object)screen))
		{
			return;
		}
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManagerInstance);
			Node? val = FindDescendant(screen, (Node node) => ((object)node.Name).ToString() == EndlessButtonName);
			Control? endlessButton = val as Control;
			if (endlessButton != null)
			{
				Node? val3 = FindDescendant(screen, (Node node) => ((object)node.Name).ToString() == ForceButtonName);
				Control? forceButton = val3 as Control;
				MegaLabel? label = endlessButton.GetNodeOrNull<MegaLabel>("Label");
				if (label != null)
				{
					label.SetTextAutoSize(GetEndlessButtonText(runManagerInstance, runState));
				}
				else if (endlessButton is Button btn)
				{
					btn.Text = GetEndlessButtonText(runManagerInstance, runState);
				}
				endlessButton.TooltipText = GetEndlessButtonTooltip(runManagerInstance, runState);
				bool isLaunching = EndlessRuntime.IsLaunchingFromGameOverScreen;
				SetControlDisabled(endlessButton, isLaunching);
				endlessButton.Visible = true;
				if (forceButton != null)
				{
					((CanvasItem)forceButton).Visible = false;
					SetControlDisabled(forceButton, true);
				}
			}
		}
		catch
		{
		}
	}

	private static void EnsureRefreshTimer(Node screen)
	{
		//IL_0045: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Expected Obj, but got Unknown
		if (FindDescendant(screen, (Node node) => ((object)node.Name).ToString() == RefreshTimerName) != null)
		{
			return;
		}
		Timer val = new Timer
		{
			Name = (StringName)RefreshTimerName,
			WaitTime = 0.1,
			Autostart = true,
			OneShot = false
		};
		val.Timeout += () =>
		{
			if (IsAlive((GodotObject?)(object)screen))
			{
				RefreshScreenState(screen);
			}
		};
		screen.AddChild((Node)(object)val, false, (Node.InternalMode)0);
	}

	private static void EnsureInitialReadyState(Node screen)
	{
		GameOverScreenState orCreateValue = ScreenStates.GetOrCreateValue(screen);
		if (orCreateValue.LobbyReadyInitialized)
		{
			return;
		}
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManagerInstance);
			if (EndlessReflection.GetPlayersCount(runState) <= 1)
			{
				orCreateValue.LobbyReadyInitialized = true;
				return;
			}
			EndlessRuntime.SetPreserveMultiplayerConnectionDuringGameOver(preserve: true, "multiplayer game over screen initialized");
			EndlessLog.Info("Skipping multiplayer endless readiness setup; each player may continue endless locally.");
			orCreateValue.LobbyReadyInitialized = true;
		}
		catch (Exception ex)
		{
			EndlessLog.Error("Failed while initializing multiplayer endless readiness.", ex);
		}
	}

	private static bool ToggleClientReady(object runManager)
	{
		if (EndlessReflection.TryIsLocalPlayerReady(runManager, out var isReady))
		{
			if (EndlessReflection.TrySetLocalPlayerReady(runManager, !isReady))
			{
				EndlessLog.Info($"Client endless readiness toggled to {!isReady}.");
				return true;
			}
			EndlessLog.Info("Client attempted to toggle endless readiness, but RunLobby did not accept the update.");
			return false;
		}
		if (EndlessReflection.TrySetLocalPlayerReady(runManager, isReady: true))
		{
			EndlessLog.Info("Client endless readiness set to true.");
			return true;
		}
		EndlessLog.Info("Client could not set endless readiness; RunLobby readiness APIs were unavailable.");
		return false;
	}

	private static void SetAuxiliaryButtonsVisible(Node screen, bool isVisible)
	{
		string[] buttonsToBlockWhileLaunching = ButtonsToBlockWhileLaunching;
		foreach (string buttonName in buttonsToBlockWhileLaunching)
		{
			Node? val = FindDescendant(screen, (Node node) => ((object)node).GetType().Name == buttonName);
			Control val2 = (Control)(object)((val is Control) ? val : null);
			if (val2 != null)
			{
				((CanvasItem)val2).Visible = isVisible;
				SetControlDisabled(val2, !isVisible);
			}
		}
	}

	private static Node? FindDescendant(Node root, Func<Node, bool> predicate)
	{
		if (!IsAlive((GodotObject?)(object)root))
		{
			return null;
		}
		if (predicate(root))
		{
			return root;
		}
		foreach (Node child in root.GetChildren(false))
		{
			if (child == null)
			{
				continue;
			}
			Node root2 = child;
			if (true)
			{
				Node val = FindDescendant(root2, predicate);
				if (val != null)
				{
					return val;
				}
			}
		}
		return null;
	}

	private static Node? FindGameOverScreen(Node root)
	{
		Node val = FindDescendant(root, (Node node) => ((object)node).GetType().Name == "NReturnToMainMenuButton");
		if (val == null)
		{
			return null;
		}
		for (Node val2 = val; val2 != null; val2 = val2.GetParent())
		{
			if (string.Equals(((object)val2).GetType().Name, "NGameOverScreen", StringComparison.Ordinal))
			{
				return val2;
			}
		}
		return val.GetParent() ?? val;
	}

	private static bool IsAlive(GodotObject? obj)
	{
		return obj != null && GodotObject.IsInstanceValid(obj);
	}

	private static string GetEndlessButtonText(object? runManager, object? runState)
	{
		return "Endless";
	}

	private static string GetEndlessButtonTooltip(object? runManager, object? runState)
	{
		return "Start the next endless loop";
	}

	private static string GetReadySummary(object runManager, object runState)
	{
		int playersCount = EndlessReflection.GetPlayersCount(runState);
		if (EndlessReflection.TryGetReadyPlayerCount(runManager, out var readyCount))
		{
			return $"{readyCount}/{playersCount} ready";
		}
		return $"{playersCount} players";
	}

	private static void TryAutoLaunchClientFromHostSignal(Node screen, GameOverScreenState state, object runManager)
	{
		if (state.RemoteLaunchTriggered || EndlessRuntime.IsLaunchingFromGameOverScreen || !EndlessReflection.TryIsHostReady(runManager, out var isReady))
		{
			return;
		}
		if (isReady)
		{
			state.SawHostReady = true;
		}
		else if (state.SawHostReady)
		{
			state.RemoteLaunchTriggered = true;
			Node? val = FindDescendant(screen, (Node node) => ((object)node.Name).ToString() == EndlessButtonName);
			Control? val2 = val as Control;
			if (val2 != null)
			{
				SetControlDisabled(val2, true);
			}
			Node? val3 = FindDescendant(screen, (Node node) => ((object)node.Name).ToString() == ForceButtonName);
			Control? val4 = val3 as Control;
			if (val4 != null)
			{
				SetControlDisabled(val4, true);
			}
			SetButtonsInteractive(screen, isInteractive: false);
			EndlessLog.Info("Detected the host's endless launch marker on a multiplayer client. Starting the next endless chapter locally.");
			_ = StartClientLaunchAsync(screen, state);
		}
	}

	private static async Task StartClientLaunchAsync(Node screen, GameOverScreenState state)
	{
		try
		{
			await EndlessRuntime.StartLoopFromGameOverScreenAsync(screen, forceReady: true, allowClientRelay: true, broadcastLaunchMarker: false);
		}
		catch (Exception ex)
		{
			Exception ex2 = ex;
			state.RemoteLaunchTriggered = false;
			EndlessLog.Error("Failed while relaying the host's endless launch to a multiplayer client.", ex2);
			if (IsAlive((GodotObject?)(object)screen))
			{
				SetButtonsInteractive(screen, isInteractive: true);
			}
		}
	}
}
