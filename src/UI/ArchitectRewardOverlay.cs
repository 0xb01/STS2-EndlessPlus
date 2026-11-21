using Godot;

namespace BNKZ.STS2.EndlessPlus;

internal static class ArchitectRewardOverlay
{
	private const string OverlayName = "BNKZArchitectRewardOverlay";

	public static void Show(Node parent, int goldAmount, int bonusCardCount)
	{
		//IL_0020: Unknown result type (might be due to invalid IL or missing references)
		//IL_0025: Unknown result type (might be due to invalid IL or missing references)
		//IL_0036: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_004b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_0063: Unknown result type (might be due to invalid IL or missing references)
		//IL_006f: Unknown result type (might be due to invalid IL or missing references)
		//IL_007d: Expected Obj, but got Unknown
		//IL_007d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0082: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b7: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Unknown result type (might be due to invalid IL or missing references)
		//IL_00dc: Expected Obj, but got Unknown
		//IL_00ec: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0105: Unknown result type (might be due to invalid IL or missing references)
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_011c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0128: Unknown result type (might be due to invalid IL or missing references)
		//IL_0134: Unknown result type (might be due to invalid IL or missing references)
		//IL_0140: Unknown result type (might be due to invalid IL or missing references)
		//IL_014c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0158: Unknown result type (might be due to invalid IL or missing references)
		//IL_0164: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Expected Obj, but got Unknown
		//IL_0181: Unknown result type (might be due to invalid IL or missing references)
		//IL_0187: Expected Obj, but got Unknown
		//IL_01a5: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ac: Expected Obj, but got Unknown
		//IL_0208: Unknown result type (might be due to invalid IL or missing references)
		//IL_020f: Expected Obj, but got Unknown
		//IL_0232: Unknown result type (might be due to invalid IL or missing references)
		//IL_0237: Unknown result type (might be due to invalid IL or missing references)
		//IL_0243: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Expected Obj, but got Unknown
		//IL_0257: Unknown result type (might be due to invalid IL or missing references)
		//IL_025c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0264: Unknown result type (might be due to invalid IL or missing references)
		//IL_026c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0274: Unknown result type (might be due to invalid IL or missing references)
		//IL_0288: Expected Obj, but got Unknown
		//IL_030b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0310: Unknown result type (might be due to invalid IL or missing references)
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0325: Unknown result type (might be due to invalid IL or missing references)
		//IL_0330: Expected Obj, but got Unknown
		if (FindExisting(parent) == null)
		{
			Control overlay = new Control
			{
				Name = (StringName)OverlayName,
				MouseFilter = (Control.MouseFilterEnum)0,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				OffsetRight = 0f,
				OffsetBottom = 0f,
				ProcessMode = (Node.ProcessModeEnum)3
			};
			ColorRect val = new ColorRect
			{
				Color = new Color(0f, 0f, 0f, 0.76f),
				MouseFilter = (Control.MouseFilterEnum)0,
				AnchorRight = 1f,
				AnchorBottom = 1f,
				OffsetRight = 0f,
				OffsetBottom = 0f
			};
			((Node)overlay).AddChild((Node)(object)val, false, (Node.InternalMode)0);
			PanelContainer val2 = new PanelContainer
			{
				MouseFilter = (Control.MouseFilterEnum)0,
				CustomMinimumSize = new Vector2(640f, 0f),
				AnchorLeft = 0.5f,
				AnchorTop = 0.5f,
				AnchorRight = 0.5f,
				AnchorBottom = 0.5f,
				OffsetLeft = -320f,
				OffsetTop = -220f,
				OffsetRight = 320f,
				OffsetBottom = 220f
			};
			((Node)overlay).AddChild((Node)(object)val2, false, (Node.InternalMode)0);
			VBoxContainer val3 = new VBoxContainer();
			((Control)val3).AddThemeConstantOverride((StringName)("separation"), 18);
			((Node)val2).AddChild((Node)(object)val3, false, (Node.InternalMode)0);
			MarginContainer val4 = new MarginContainer();
			((Control)val4).AddThemeConstantOverride((StringName)("margin_left"), 26);
			((Control)val4).AddThemeConstantOverride((StringName)("margin_top"), 24);
			((Control)val4).AddThemeConstantOverride((StringName)("margin_right"), 26);
			((Control)val4).AddThemeConstantOverride((StringName)("margin_bottom"), 24);
			((Node)val3).AddChild((Node)(object)val4, false, (Node.InternalMode)0);
			VBoxContainer val5 = new VBoxContainer();
			((Control)val5).AddThemeConstantOverride((StringName)("separation"), 14);
			((Node)val4).AddChild((Node)(object)val5, false, (Node.InternalMode)0);
			((Node)val5).AddChild((Node)new Label
			{
				Text = "Chapter Reward",
				HorizontalAlignment = (HorizontalAlignment)1
			}, false, (Node.InternalMode)0);
			((Node)val5).AddChild((Node)new RichTextLabel
			{
				BbcodeEnabled = true,
				FitContent = true,
				ScrollActive = false,
				Text = "[center]The Architect has sent you into the next chapter.[/center]"
			}, false, (Node.InternalMode)0);
			((Node)val5).AddChild((Node)(object)CreateRewardPanel($"{goldAmount} Gold", "Added directly to your run."), false, (Node.InternalMode)0);
			((Node)val5).AddChild((Node)(object)CreateRewardPanel((bonusCardCount == 1) ? "1 golden card" : $"{bonusCardCount} golden cards", "Added directly to your deck as a bonus card."), false, (Node.InternalMode)0);
			Button val6 = new Button
			{
				Text = "Continue",
				FocusMode = (Control.FocusModeEnum)2,
				SizeFlagsHorizontal = (Control.SizeFlags)4
			};
			((BaseButton)val6).Pressed += () =>
			{
				((Node)overlay).QueueFree();
			};
			((Node)val5).AddChild((Node)(object)val6, false, (Node.InternalMode)0);
			parent.AddChild((Node)(object)overlay, false, (Node.InternalMode)0);
			parent.MoveChild((Node)(object)overlay, parent.GetChildCount(false) - 1);
			((Control)val6).GrabFocus();
		}
	}

	private static Control CreateRewardPanel(string title, string description)
	{
		//IL_0001: Unknown result type (might be due to invalid IL or missing references)
		//IL_0006: Unknown result type (might be due to invalid IL or missing references)
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_001d: Expected Obj, but got Unknown
		//IL_001d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Expected Obj, but got Unknown
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0080: Expected Obj, but got Unknown
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0091: Unknown result type (might be due to invalid IL or missing references)
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		//IL_00aa: Expected Obj, but got Unknown
		//IL_00ac: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b1: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d3: Expected Obj, but got Unknown
		PanelContainer val = new PanelContainer
		{
			CustomMinimumSize = new Vector2(0f, 96f)
		};
		MarginContainer val2 = new MarginContainer();
		((Control)val2).AddThemeConstantOverride((StringName)("margin_left"), 18);
		((Control)val2).AddThemeConstantOverride((StringName)("margin_top"), 14);
		((Control)val2).AddThemeConstantOverride((StringName)("margin_right"), 18);
		((Control)val2).AddThemeConstantOverride((StringName)("margin_bottom"), 14);
		((Node)val).AddChild((Node)(object)val2, false, (Node.InternalMode)0);
		VBoxContainer val3 = new VBoxContainer();
		((Node)val2).AddChild((Node)(object)val3, false, (Node.InternalMode)0);
		((Node)val3).AddChild((Node)new Label
		{
			Text = title,
			HorizontalAlignment = (HorizontalAlignment)0
		}, false, (Node.InternalMode)0);
		((Node)val3).AddChild((Node)new Label
		{
			Text = description,
			AutowrapMode = (TextServer.AutowrapMode)3,
			HorizontalAlignment = (HorizontalAlignment)0
		}, false, (Node.InternalMode)0);
		return (Control)(object)val;
	}

	private static Node? FindExisting(Node root)
	{
		if (root.Name == (StringName)OverlayName)
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
				Node val = FindExisting(root2);
				if (val != null)
				{
					return val;
				}
			}
		}
		return null;
	}
}
