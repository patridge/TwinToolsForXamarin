using System;
using MonoDevelop.Components.Commands;
using MonoDevelop.Ide;
using TwinTechs.EditorExtensions.Helpers;

namespace TwinTechs.EditorExtensions.Commands
{

	public class ToggleViewModelPreferCodeBehind : CommandHandler
	{
		protected override void Run ()
		{
			MemberExtensionsHelper.Instance.ToggleVMXamlCs (true);
		}

		protected override void Update (CommandInfo info)
		{
			info.Enabled = true;
			info.Visible = true;
		}
	}

}
