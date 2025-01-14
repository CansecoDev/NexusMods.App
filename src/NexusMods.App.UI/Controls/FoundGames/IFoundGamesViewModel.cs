using System.Collections.ObjectModel;
using NexusMods.Abstractions.Games;
using NexusMods.App.UI.Controls.GameWidget;

namespace NexusMods.App.UI.Controls.FoundGames;

public interface IFoundGamesViewModel : IViewModelInterface
{
    /// <summary>
    /// All the games in this list.
    /// </summary>
    public ReadOnlyObservableCollection<IGameWidgetViewModel> Games { get; }

    /// <summary>
    /// Initialize the list of games in a way where each "add" button would manage
    /// an already found game.
    /// </summary>
    /// <param name="games"></param>
    /// <returns></returns>
    void InitializeFromFound(IEnumerable<IGame> games);

    /// <summary>
    /// Initialize the list of games in a way where each "add" button would
    /// trigger a manual add of the game.
    /// </summary>
    /// <param name="games"></param>
    /// <returns></returns>
    void InitializeManual(IEnumerable<IGame> games);
}
