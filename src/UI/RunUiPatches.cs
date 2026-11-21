using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace BNKZ.STS2.EndlessPlus;

internal static class RunUiPatches
{
	private const string StatusLabelName = "BNKZEndlessModeStatusLabel";

	private const string StatusTimerName = "BNKZEndlessModeStatusTimer";

	private const string GameOverScanTimerName = "BNKZEndlessModeGameOverScanTimer";

	private const string RelicToggleButtonName = "BNKZEndlessModeRelicToggleButton";

	private const float RelicToggleButtonMinimumSize = 56f;

	private static bool _showSingleRelicRowOnly;

	private static int _relicRowScrollOffset;

	private const string NewBadgeName = "BNKZNewRelicBadge";

	private const double NewBadgeDurationSeconds = 10.0;

	private static readonly Dictionary<object, DateTime> _newRelics = new();

	public static void RecordNewRelic(object relic)
	{
		lock (_newRelics)
		{
			_newRelics[relic] = DateTime.UtcNow;
		}
	}

	public static void AfterReady(object __instance)
	{
		Control val = (Control)((__instance is Control) ? __instance : null);
		if (val != null)
		{
			EnsureStatusLabel(val);
			EnsureStatusTimer(val);
			EnsureGameOverScanTimer(val);
			UpdateStatusLabel(val);
			RefreshRelicInventoryUi(val);
		}
	}

	private static void UpdateStatusLabel(Control runNode)
	{
		Node? val = FindStatusLabel((Node)(object)runNode);
		Label val2 = (Label)(object)((val is Label) ? val : null);
		if (val2 == null)
		{
			return;
		}
		try
		{
			object runManagerInstance = EndlessReflection.GetRunManagerInstance();
			object runState = EndlessReflection.GetRunState(runManagerInstance);
			val2.Text = EndlessRuntime.GetRunStatusText(runState);
		}
		catch
		{
			val2.Text = string.Empty;
		}
	}

	private static void EnsureStatusLabel(Control runNode)
	{
		//IL_0017: Unknown result type (might be due to invalid IL or missing references)
		//IL_001c: Unknown result type (might be due to invalid IL or missing references)
		//IL_002d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_004c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0055: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Expected Obj, but got Unknown
		if (FindStatusLabel((Node)(object)runNode) == null)
		{
			Label val = new Label
			{
				Name = (StringName)StatusLabelName,
				MouseFilter = (Control.MouseFilterEnum)2,
				TopLevel = true,
				HorizontalAlignment = HorizontalAlignment.Center,
				VerticalAlignment = VerticalAlignment.Top,
				Modulate = new Color(1f, 0.94f, 0.72f, 0.90f)
			};
			val.SetAnchorsPreset(Control.LayoutPreset.TopWide);
			val.OffsetTop = 5f;
			val.OffsetBottom = 35f;
			((Control)val).AddThemeFontSizeOverride((StringName)("font_size"), 18);
			((Control)val).AddThemeColorOverride((StringName)("font_outline_color"), new Color(0f, 0f, 0f, 1f));
			((Control)val).AddThemeConstantOverride((StringName)("outline_size"), 8);
			((Node)runNode).AddChild((Node)(object)val, false, (Node.InternalMode)0);
		}
	}

	private static void EnsureStatusTimer(Control runNode)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected Obj, but got Unknown
		if (FindChild((Node)(object)runNode, StatusTimerName) != null)
		{
			return;
		}
		Timer val = new Timer
		{
			Name = (StringName)StatusTimerName,
			WaitTime = 0.5,
			Autostart = true,
			OneShot = false
		};
		val.Timeout += () =>
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)runNode))
			{
				UpdateStatusLabel(runNode);
				RefreshRelicInventoryUi(runNode);
			}
		};
		((Node)runNode).AddChild((Node)(object)val, false, (Node.InternalMode)0);
	}

	private static void EnsureGameOverScanTimer(Control runNode)
	{
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0030: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0051: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_0062: Expected Obj, but got Unknown
		if (FindChild((Node)(object)runNode, GameOverScanTimerName) != null)
		{
			return;
		}
		Timer val = new Timer
		{
			Name = (StringName)GameOverScanTimerName,
			WaitTime = 0.2,
			Autostart = true,
			OneShot = false
		};
		val.Timeout += () =>
		{
			if (GodotObject.IsInstanceValid((GodotObject)(object)runNode))
			{
				GameOverScreenPatches.TryInjectFromTree((Node)(object)runNode);
			}
		};
		((Node)runNode).AddChild((Node)(object)val, false, (Node.InternalMode)0);
	}

	private static Node? FindStatusLabel(Node runNode)
	{
		return FindChild(runNode, StatusLabelName);
	}

	private static void RefreshRelicInventoryUi(Control runNode)
	{
		Control val = FindPrimaryRelicInventory((Node)(object)runNode);
		Node? val2 = FindNamedDescendant((Node)(object)runNode, RelicToggleButtonName);
		Button val3 = (Button)(object)((val2 is Button) ? val2 : null);
		if (val == null)
		{
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = false;
			}
			return;
		}
		try
		{
			val3 = EnsureRelicToggleButton(val);
			RefreshRelicCounterDisplays(val);
			SortRelicInventory(val);
			UpdateRelicNewBadges(val);
			EnsureRelicInputHooks(val, val3);
			ApplyRelicVisibilityMode(val);
			UpdateRelicToggleButton(val3, val);
		}
		catch (Exception ex)
		{
			if (val3 != null)
			{
				((CanvasItem)val3).Visible = false;
			}
			EndlessLog.Error("Failed to refresh the relic inventory UI.", ex);
		}
	}

	private static void RefreshRelicCounterDisplays(Control inventory)
	{
		foreach (Control relicInventoryHolder in EndlessReflection.GetRelicInventoryHolders(inventory))
		{
			EndlessReflection.RefreshRelicCounterDisplay(relicInventoryHolder);
		}
	}

	private static void UpdateRelicNewBadges(Control inventory)
	{
		DateTime now = DateTime.UtcNow;
		IReadOnlyList<Control> holders = EndlessReflection.GetRelicInventoryHolders(inventory);
		foreach (Control holder in holders)
		{
			object? model = EndlessReflection.GetRelicInventoryHolderModel(holder);
			bool isNew = false;
			double remaining = 0;
			if (model != null)
			{
				lock (_newRelics)
				{
					if (_newRelics.TryGetValue(model, out DateTime acquiredAt))
					{
						double elapsed = (now - acquiredAt).TotalSeconds;
						if (elapsed < NewBadgeDurationSeconds)
						{
							isNew = true;
							remaining = NewBadgeDurationSeconds - elapsed;
						}
					}
				}
			}

			Node? existingBadge = FindChild((Node)(object)holder, NewBadgeName);
			if (isNew)
			{
				Label badge = (Label)(object)((existingBadge is Label) ? existingBadge : null);
				if (badge == null)
				{
					badge = new Label
					{
						Name = (StringName)NewBadgeName,
						Text = "NEW",
						MouseFilter = Control.MouseFilterEnum.Ignore,
						HorizontalAlignment = HorizontalAlignment.Center,
						VerticalAlignment = VerticalAlignment.Center
					};
					badge.SetAnchorsPreset(Control.LayoutPreset.TopRight);
					badge.OffsetLeft = -28f;
					badge.OffsetTop = 0f;
					badge.OffsetRight = 2f;
					badge.OffsetBottom = 16f;
					((Control)badge).AddThemeFontSizeOverride((StringName)("font_size"), 10);
					((Control)badge).AddThemeColorOverride((StringName)("font_color"), new Color(1f, 0.88f, 0.2f, 1f));
					((Control)badge).AddThemeColorOverride((StringName)("font_outline_color"), new Color(0f, 0f, 0f, 1f));
					((Control)badge).AddThemeConstantOverride((StringName)("outline_size"), 4);
					((Node)holder).AddChild((Node)(object)badge, false, (Node.InternalMode)0);
				}

				float alpha = remaining < 2.0 ? Math.Clamp((float)(remaining / 2.0), 0.1f, 1.0f) : 1.0f;
				((CanvasItem)badge).Modulate = new Color(1f, 1f, 1f, alpha);
			}
			else if (existingBadge != null)
			{
				((Node)holder).RemoveChild(existingBadge);
				existingBadge.QueueFree();
			}
		}

		lock (_newRelics)
		{
			List<object>? toRemove = null;
			foreach (KeyValuePair<object, DateTime> kv in _newRelics)
			{
				if ((now - kv.Value).TotalSeconds >= NewBadgeDurationSeconds + 5.0)
				{
					toRemove ??= new List<object>();
					toRemove.Add(kv.Key);
				}
			}
			if (toRemove != null)
			{
				foreach (object key in toRemove)
				{
					_newRelics.Remove(key);
				}
			}
		}
	}

	private static void EnsureRelicInputHooks(Control inventory, Button button)
	{
		if (!inventory.HasMeta("BNKZInputHooked"))
		{
			inventory.SetMeta("BNKZInputHooked", true);
			inventory.GuiInput += (@event) => HandleRelicGuiInput(@event, inventory);
		}
		if (!button.HasMeta("BNKZInputHooked"))
		{
			button.SetMeta("BNKZInputHooked", true);
			button.GuiInput += (@event) => HandleRelicGuiInput(@event, inventory);
		}
		IReadOnlyList<Control> holders = EndlessReflection.GetRelicInventoryHolders(inventory);
		foreach (Control holder in holders)
		{
			if (!holder.HasMeta("BNKZInputHooked"))
			{
				holder.SetMeta("BNKZInputHooked", true);
				holder.GuiInput += (@event) => HandleRelicGuiInput(@event, inventory);
			}
		}
	}

	private static void HandleRelicGuiInput(InputEvent @event, Control inventory)
	{
		if (@event is InputEventMouseButton mb && mb.Pressed)
		{
			IReadOnlyList<Control> holders = EndlessReflection.GetRelicInventoryHolders(inventory);
			int rowCapacity = GetFirstRelicRowCount(inventory, holders);
			int totalRows = (holders.Count + rowCapacity - 1) / rowCapacity;
			int maxVisibleRows = _showSingleRelicRowOnly ? 1 : 3;
			int maxOffset = Math.Max(0, totalRows - maxVisibleRows);

			if (mb.ButtonIndex == MouseButton.WheelUp)
			{
				if (_relicRowScrollOffset > 0)
				{
					_relicRowScrollOffset--;
					Control topmost = GetTopmostControl(inventory);
					if (topmost != null) RefreshRelicInventoryUi(topmost);
				}
				inventory.AcceptEvent();
			}
			else if (mb.ButtonIndex == MouseButton.WheelDown)
			{
				if (_relicRowScrollOffset < maxOffset)
				{
					_relicRowScrollOffset++;
					Control topmost = GetTopmostControl(inventory);
					if (topmost != null) RefreshRelicInventoryUi(topmost);
				}
				inventory.AcceptEvent();
			}
		}
	}

	private static Button EnsureRelicToggleButton(Control inventory)
	{
		Node? val = FindChild((Node)(object)inventory, RelicToggleButtonName);
		Button val2 = (Button)(object)((val is Button) ? val : null);
		if (val2 != null)
		{
			return val2;
		}
		Button val3 = new Button
		{
			Name = (StringName)RelicToggleButtonName,
			FocusMode = (Control.FocusModeEnum)2,
			CustomMinimumSize = new Vector2(56f, 56f),
			Size = new Vector2(56f, 56f),
			Visible = false
		};
		((Control)val3).AddThemeFontSizeOverride((StringName)("font_size"), 16);
		((BaseButton)val3).Pressed += () =>
		{
			IReadOnlyList<Control> holders = EndlessReflection.GetRelicInventoryHolders(inventory);
			int rowCapacity = GetFirstRelicRowCount(inventory, holders);
			int totalRows = (holders.Count + rowCapacity - 1) / rowCapacity;
			int maxVisibleRows = _showSingleRelicRowOnly ? 1 : 3;

			if (totalRows > 3)
			{
				_relicRowScrollOffset += maxVisibleRows;
				if (_relicRowScrollOffset >= totalRows)
				{
					_relicRowScrollOffset = 0;
				}
			}
			else
			{
				_showSingleRelicRowOnly = !_showSingleRelicRowOnly;
				_relicRowScrollOffset = 0;
			}

			Control topmostControl = GetTopmostControl(inventory);
			if (topmostControl != null)
			{
				RefreshRelicInventoryUi(topmostControl);
			}
		};
		((Node)inventory).AddChild((Node)(object)val3, false, (Node.InternalMode)0);
		((Node)inventory).MoveChild((Node)(object)val3, 0);
		return val3;
	}

	private static void UpdateRelicToggleButton(Button button, Control inventory)
	{
		IReadOnlyList<Control> relicInventoryHolders = EndlessReflection.GetRelicInventoryHolders(inventory);
		if (relicInventoryHolders.Count > 0)
		{
			Control val = relicInventoryHolders[0];
			Vector2 combinedMinimumSize = val.GetCombinedMinimumSize();
			float num = MathF.Max(56f, combinedMinimumSize.X + 8f);
			float num2 = MathF.Max(56f, combinedMinimumSize.Y + 8f);
			((Control)button).CustomMinimumSize = new Vector2(num, num2);
			((Control)button).Size = ((Control)button).CustomMinimumSize;
			((Control)button).Theme = val.Theme;
			((Control)button).ThemeTypeVariation = val.ThemeTypeVariation;
		}

		int rowCapacity = GetFirstRelicRowCount(inventory, relicInventoryHolders);
		int totalRows = (relicInventoryHolders.Count + rowCapacity - 1) / rowCapacity;
		int maxVisibleRows = _showSingleRelicRowOnly ? 1 : 3;

		if (totalRows <= 1)
		{
			((CanvasItem)button).Visible = false;
			return;
		}

		((CanvasItem)button).Visible = true;
		if (totalRows > 3)
		{
			int startRowDisp = _relicRowScrollOffset + 1;
			int endRowDisp = Math.Min(totalRows, _relicRowScrollOffset + maxVisibleRows);
			((Control)button).AddThemeFontSizeOverride((StringName)("font_size"), 14);
			button.Text = $"{startRowDisp}-{endRowDisp}\n/{totalRows}";
			((Control)button).TooltipText = $"Showing rows {startRowDisp}-{endRowDisp} of {totalRows}.\nClick to cycle rows, or use Mouse Wheel to scroll.";
		}
		else
		{
			((Control)button).AddThemeFontSizeOverride((StringName)("font_size"), 26);
			button.Text = (_showSingleRelicRowOnly ? "+" : "-");
			((Control)button).TooltipText = (_showSingleRelicRowOnly ? "Expand: show up to 3 relic rows" : "Collapse: show only 1 relic row");
		}
		((Node)inventory).MoveChild((Node)(object)button, 0);
	}

	private static void SortRelicInventory(Control inventory)
	{
		IReadOnlyList<Control> relicInventoryHolders = EndlessReflection.GetRelicInventoryHolders(inventory);
		if (relicInventoryHolders.Count < 2)
		{
			return;
		}
		List<Control> list = new List<Control>(relicInventoryHolders.Count);
		List<Control> list2 = new List<Control>(relicInventoryHolders.Count);
		foreach (Control item in relicInventoryHolders)
		{
			if (HasVisibleRelicCounter(item))
			{
				list.Add(item);
			}
			else
			{
				list2.Add(item);
			}
		}
		if (list.Count == 0 || list.Count == relicInventoryHolders.Count)
		{
			return;
		}
		List<Control> list3 = list.Concat(list2).ToList();
		for (int i = 0; i < relicInventoryHolders.Count; i++)
		{
			if (relicInventoryHolders[i] != list3[i])
			{
				EndlessReflection.SetRelicInventoryHolderOrder(inventory, list3);
				break;
			}
		}
	}

	private static bool HasVisibleRelicCounter(Control holder)
	{
		string relicInventoryHolderAmountText = EndlessReflection.GetRelicInventoryHolderAmountText(holder);
		return relicInventoryHolderAmountText.Any(char.IsDigit);
	}

	private static void ApplyRelicVisibilityMode(Control inventory)
	{
		IReadOnlyList<Control> holders = EndlessReflection.GetRelicInventoryHolders(inventory);
		if (holders.Count == 0)
		{
			return;
		}
		int rowCapacity = GetFirstRelicRowCount(inventory, holders);
		int totalRows = (holders.Count + rowCapacity - 1) / rowCapacity;
		int maxVisibleRows = _showSingleRelicRowOnly ? 1 : 3;
		int maxOffset = Math.Max(0, totalRows - maxVisibleRows);
		_relicRowScrollOffset = Math.Clamp(_relicRowScrollOffset, 0, maxOffset);

		// If a new relic was acquired that is currently outside visible range, auto-scroll to reveal it
		DateTime now = DateTime.UtcNow;
		int autoScrollRow = -1;
		for (int i = 0; i < holders.Count; i++)
		{
			object? model = EndlessReflection.GetRelicInventoryHolderModel(holders[i]);
			if (model != null)
			{
				lock (_newRelics)
				{
					if (_newRelics.TryGetValue(model, out DateTime acquiredAt) && (now - acquiredAt).TotalSeconds < 3.0)
					{
						int targetRow = i / rowCapacity;
						if (targetRow < _relicRowScrollOffset || targetRow >= _relicRowScrollOffset + maxVisibleRows)
						{
							autoScrollRow = targetRow;
						}
					}
				}
			}
		}
		if (autoScrollRow >= 0)
		{
			_relicRowScrollOffset = Math.Clamp(autoScrollRow - maxVisibleRows + 1, 0, maxOffset);
		}

		int startIndex = _relicRowScrollOffset * rowCapacity;
		int endIndex = Math.Min(holders.Count, (_relicRowScrollOffset + maxVisibleRows) * rowCapacity);

		bool changed = false;
		for (int i = 0; i < holders.Count; i++)
		{
			bool visible = i >= startIndex && i < endIndex;
			if (((CanvasItem)holders[i]).Visible != visible)
			{
				((CanvasItem)holders[i]).Visible = visible;
				changed = true;
			}
		}
		if (changed)
		{
			EndlessReflection.UpdateRelicInventoryNavigation(inventory);
		}
	}

	private static int GetFirstRelicRowCount(Control inventory, IReadOnlyList<Control> holders)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_008e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Unknown result type (might be due to invalid IL or missing references)
		float num = inventory.Size.X;
		if (num <= 0f)
		{
			return holders.Count;
		}
		float num2 = 0f;
		if (inventory.HasThemeConstant((StringName)("h_separation"), (StringName)null))
		{
			num2 = inventory.GetThemeConstant((StringName)("h_separation"), (StringName)null);
		}
		Node? val = FindChild((Node)(object)inventory, RelicToggleButtonName);
		Control val2 = (Control)(object)((val is Control) ? val : null);
		if (val2 != null && ((CanvasItem)val2).Visible)
		{
			float num3 = MathF.Max(val2.Size.X, val2.GetCombinedMinimumSize().X);
			if (num3 > 0f)
			{
				num -= num3;
				if (holders.Count > 0)
				{
					num -= num2;
				}
			}
		}
		if (num <= 0f)
		{
			return 1;
		}
		float num4 = 0f;
		int num5 = 0;
		foreach (Control holder in holders)
		{
			float num6 = MathF.Max(holder.Size.X, holder.GetCombinedMinimumSize().X);
			if (num6 <= 0f)
			{
				return holders.Count;
			}
			float num7 = ((num5 == 0) ? num6 : (num4 + num2 + num6));
			if (num7 > num + 0.5f && num5 > 0)
			{
				break;
			}
			num4 = num7;
			num5++;
		}
		return Math.Max(1, num5);
	}

	private static Control? FindPrimaryRelicInventory(Node root)
	{
		//IL_0064: Unknown result type (might be due to invalid IL or missing references)
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0071: Unknown result type (might be due to invalid IL or missing references)
		//IL_0073: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0089: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_009e: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a5: Unknown result type (might be due to invalid IL or missing references)
		Control val = null;
		foreach (Node item in EnumerateDescendants(root))
		{
			Control val2 = (Control)(object)((item is Control) ? item : null);
			if (val2 == null || !string.Equals(((object)val2).GetType().Name, "NRelicInventory", StringComparison.Ordinal) || !((CanvasItem)val2).IsVisibleInTree())
			{
				continue;
			}
			if (val == null)
			{
				val = val2;
				continue;
			}
			Vector2 globalPosition = val.GlobalPosition;
			Vector2 globalPosition2 = val2.GlobalPosition;
			if (globalPosition2.Y < globalPosition.Y - 0.5f || (Mathf.IsEqualApprox(globalPosition2.Y, globalPosition.Y) && globalPosition2.X < globalPosition.X))
			{
				val = val2;
			}
		}
		return val;
	}

	private static IEnumerable<Node> EnumerateDescendants(Node root)
	{
		Node node = default;
		foreach (Node child in root.GetChildren(false))
		{
			int num;
			if (child != null)
			{
				node = child;
				num = ((1 == 0) ? 1 : 0);
			}
			else
			{
				num = 1;
			}
			if (num != 0)
			{
				continue;
			}
			yield return node;
			foreach (Node item in EnumerateDescendants(node))
			{
				yield return item;
			}
			node = null;
		}
	}

	private static Node? FindChild(Node root, string nodeName)
	{
		foreach (Node child in root.GetChildren(false))
		{
			if (child != null)
			{
				Node val = child;
				if (string.Equals(((object)val.Name).ToString(), nodeName, StringComparison.Ordinal))
				{
					return val;
				}
			}
		}
		return null;
	}

	private static Node? FindNamedDescendant(Node root, string nodeName)
	{
		foreach (Node item in EnumerateDescendants(root))
		{
			if (string.Equals(((object)item.Name).ToString(), nodeName, StringComparison.Ordinal))
			{
				return item;
			}
		}
		return null;
	}

	private static Control? GetTopmostControl(Control node)
	{
		Control val = node;
		while (true)
		{
			Node obj = ((val != null) ? ((Node)val).GetParent() : null);
			Control val2 = (Control)(object)((obj is Control) ? obj : null);
			if (val2 == null)
			{
				break;
			}
			val = val2;
		}
		return val;
	}
}
