using System.Windows.Input;
using ReactiveUI.Fody.Helpers;

namespace NexusMods.App.UI.Controls.Spine.Buttons.Icon;

public class IconButtonViewModel : AViewModel<IIconButtonViewModel>, IIconButtonViewModel
{
    [Reactive]
    public bool IsActive { get; set; }

    [Reactive] public ICommand Click { get; set; } = Initializers.ICommand;

    [Reactive] public string Name { get; set; } = string.Empty;
}
