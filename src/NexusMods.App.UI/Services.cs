using System.Text.Json.Serialization;
using Microsoft.Extensions.DependencyInjection;
using NexusMods.Abstractions.Serialization.ExpressionGenerator;
using NexusMods.Abstractions.Serialization.Json;
using NexusMods.App.UI.Controls;
using NexusMods.App.UI.Controls.DataGrid;
using NexusMods.App.UI.Controls.DevelopmentBuildBanner;
using NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadGameName;
using NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadName;
using NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadSize;
using NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadStatus;
using NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadVersion;
using NexusMods.App.UI.Controls.FoundGames;
using NexusMods.App.UI.Controls.GameWidget;
using NexusMods.App.UI.Controls.Spine;
using NexusMods.App.UI.Controls.Spine.Buttons.Download;
using NexusMods.App.UI.Controls.Spine.Buttons.Icon;
using NexusMods.App.UI.Controls.Spine.Buttons.Image;
using NexusMods.App.UI.Controls.TopBar;
using NexusMods.App.UI.LeftMenu;
using NexusMods.App.UI.LeftMenu.Downloads;
using NexusMods.App.UI.LeftMenu.Home;
using NexusMods.App.UI.LeftMenu.Items;
using NexusMods.App.UI.LeftMenu.Loadout;
using NexusMods.App.UI.Overlays;
using NexusMods.App.UI.Overlays.Download.Cancel;
using NexusMods.App.UI.Overlays.Generic.MessageBox.OkCancel;
using NexusMods.App.UI.Overlays.Login;
using NexusMods.App.UI.Overlays.MetricsOptIn;
using NexusMods.App.UI.Overlays.Updater;
using NexusMods.App.UI.Pages.Downloads;
using NexusMods.App.UI.Pages.LoadoutGrid;
using NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModCategory;
using NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModEnabled;
using NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModInstalled;
using NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModName;
using NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModVersion;
using NexusMods.App.UI.Pages.MyGames;
using NexusMods.App.UI.Windows;
using NexusMods.App.UI.WorkspaceAttachments;
using NexusMods.App.UI.WorkspaceSystem;
using NexusMods.Paths;
using ReactiveUI;
using DownloadGameNameView = NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadGameName.DownloadGameNameView;
using DownloadNameView = NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadName.DownloadNameView;
using DownloadSizeView = NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadSize.DownloadSizeView;
using DownloadStatusView = NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadStatus.DownloadStatusView;
using DownloadVersionView = NexusMods.App.UI.Controls.DownloadGrid.Columns.DownloadVersion.DownloadVersionView;
using FoundGamesView = NexusMods.App.UI.Controls.FoundGames.FoundGamesView;
using ImageButton = NexusMods.App.UI.Controls.Spine.Buttons.Image.ImageButton;
using ModCategoryView = NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModCategory.ModCategoryView;
using ModEnabledView = NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModEnabled.ModEnabledView;
using ModInstalledView = NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModInstalled.ModInstalledView;
using ModNameView = NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModName.ModNameView;
using ModVersionView = NexusMods.App.UI.Pages.LoadoutGrid.Columns.ModVersion.ModVersionView;
using NexusLoginOverlayView = NexusMods.App.UI.Overlays.Login.NexusLoginOverlayView;

namespace NexusMods.App.UI;

public static class Services
{
    // ReSharper disable once InconsistentNaming
    public static IServiceCollection AddUI(this IServiceCollection c, ILauncherSettings? settings)
    {
        if (settings == null)
            c.AddSingleton<ILauncherSettings, LauncherSettings>();
        else
            c.AddSingleton(settings);

        return c
            // JSON converters
            .AddSingleton<JsonConverter, RectJsonConverter>()
            .AddSingleton<JsonConverter, ColorJsonConverter>()
            .AddSingleton<JsonConverter, AbstractClassConverterFactory<IPageFactoryContext>>()

            // Type Finder
            .AddSingleton<ITypeFinder, TypeFinder>()

            .AddTransient<MainWindow>()

            // Services
            .AddSingleton<IOverlayController, OverlayController>()
            .AddTransient<IImageCache, ImageCache>()

            // View Models
            .AddTransient<MainWindowViewModel>()
            .AddTransient(typeof(DataGridViewModelColumn<,>))
            .AddTransient(typeof(DataGridColumnFactory<,,>))
            .AddSingleton<IViewLocator, InjectedViewLocator>()

            .AddViewModel<DevelopmentBuildBannerViewModel, IDevelopmentBuildBannerViewModel>()
            .AddViewModel<DownloadsLeftMenuViewModel, IDownloadsLeftMenuViewModel>()
            .AddViewModel<FoundGamesViewModel, IFoundGamesViewModel>()
            .AddViewModel<GameWidgetViewModel, IGameWidgetViewModel>()
            .AddViewModel<HomeLeftMenuViewModel, IHomeLeftMenuViewModel>()
            .AddViewModel<IconButtonViewModel, IIconButtonViewModel>()
            .AddViewModel<IconViewModel, IIconViewModel>()
            .AddViewModel<ImageButtonViewModel, IImageButtonViewModel>()
            .AddViewModel<InProgressViewModel, IInProgressViewModel>()
            .AddViewModel<LaunchButtonViewModel, ILaunchButtonViewModel>()
            .AddViewModel<LoadoutGridViewModel, ILoadoutGridViewModel>()
            .AddViewModel<ModCategoryViewModel, IModCategoryViewModel>()
            .AddViewModel<ModEnabledViewModel, IModEnabledViewModel>()
            .AddViewModel<ModInstalledViewModel, IModInstalledViewModel>()
            .AddViewModel<ModNameViewModel, IModNameViewModel>()
            .AddViewModel<ModVersionViewModel, IModVersionViewModel>()
            .AddViewModel<MyGamesViewModel, IMyGamesViewModel>()
            .AddViewModel<NexusLoginOverlayViewModel, INexusLoginOverlayViewModel>()
            .AddViewModel<SpineViewModel, ISpineViewModel>()
            .AddViewModel<TopBarViewModel, ITopBarViewModel>()
            .AddViewModel<SpineDownloadButtonViewModel, ISpineDownloadButtonViewModel>()
            .AddViewModel<DownloadGameNameViewModel, IDownloadGameNameViewModel>()
            .AddViewModel<DownloadNameViewModel, IDownloadNameViewModel>()
            .AddViewModel<DownloadVersionViewModel, IDownloadVersionViewModel>()
            .AddViewModel<DownloadSizeViewModel, IDownloadSizeViewModel>()
            .AddViewModel<DownloadStatusViewModel, IDownloadStatusViewModel>()
            .AddViewModel<CancelDownloadOverlayViewModel, ICancelDownloadOverlayViewModel>()
            .AddViewModel<MessageBoxOkCancelViewModel, IMessageBoxOkCancelViewModel>()
            .AddViewModel<MetricsOptInViewModel, IMetricsOptInViewModel>()
            .AddViewModel<UpdaterViewModel, IUpdaterViewModel>()
            .AddViewModel<LoadoutLeftMenuViewModel, ILoadoutLeftMenuViewModel>()

            // Views
            .AddView<DevelopmentBuildBannerView, IDevelopmentBuildBannerViewModel>()
            .AddView<DownloadsLeftMenuView, IDownloadsLeftMenuViewModel>()
            .AddView<FoundGamesView, IFoundGamesViewModel>()
            .AddView<GameWidget, IGameWidgetViewModel>()
            .AddView<HomeLeftMenuView, IHomeLeftMenuViewModel>()
            .AddView<IconButton, IIconButtonViewModel>()
            .AddView<IconView, IIconViewModel>()
            .AddView<ImageButton, IImageButtonViewModel>()
            .AddView<InProgressView, IInProgressViewModel>()
            .AddView<LaunchButtonView, ILaunchButtonViewModel>()
            .AddView<LeftMenuView, ILeftMenuViewModel>()
            .AddView<LoadoutGridView, ILoadoutGridViewModel>()
            .AddView<MetricsOptInView, IMetricsOptInViewModel>()
            .AddView<ModCategoryView, IModCategoryViewModel>()
            .AddView<ModEnabledView, IModEnabledViewModel>()
            .AddView<ModInstalledView, IModInstalledViewModel>()
            .AddView<ModNameView, IModNameViewModel>()
            .AddView<ModVersionView, IModVersionViewModel>()
            .AddView<MyGamesView, IMyGamesViewModel>()
            .AddView<NexusLoginOverlayView, INexusLoginOverlayViewModel>()
            .AddView<Spine, ISpineViewModel>()
            .AddView<TopBarView, ITopBarViewModel>()
            .AddView<SpineDownloadButtonView, ISpineDownloadButtonViewModel>()
            .AddView<DownloadGameNameView, IDownloadGameNameViewModel>()
            .AddView<DownloadNameView, IDownloadNameViewModel>()
            .AddView<DownloadVersionView, IDownloadVersionViewModel>()
            .AddView<DownloadSizeView, IDownloadSizeViewModel>()
            .AddView<DownloadStatusView, IDownloadStatusViewModel>()
            .AddView<CancelDownloadOverlayView, ICancelDownloadOverlayViewModel>()
            .AddView<MessageBoxOkCancelView, IMessageBoxOkCancelViewModel>()
            .AddView<UpdaterView, IUpdaterViewModel>()
            .AddView<LoadoutLeftMenuView, ILoadoutLeftMenuViewModel>()

            // workspace system
            .AddSingleton<IWindowManager, WindowManager>()
            .AddViewModel<WorkspaceViewModel, IWorkspaceViewModel>()
            .AddViewModel<PanelViewModel, IPanelViewModel>()
            .AddViewModel<AddPanelButtonViewModel, IAddPanelButtonViewModel>()
            .AddViewModel<AddPanelDropDownViewModel, IAddPanelDropDownViewModel>()
            .AddViewModel<PanelTabHeaderViewModel, IPanelTabHeaderViewModel>()
            .AddViewModel<NewTabPageViewModel, INewTabPageViewModel>()
            .AddViewModel<NewTabPageSectionViewModel, INewTabPageSectionViewModel>()
            .AddView<WorkspaceView, IWorkspaceViewModel>()
            .AddView<PanelView, IPanelViewModel>()
            .AddView<AddPanelButtonView, IAddPanelButtonViewModel>()
            .AddView<AddPanelDropDownView, IAddPanelDropDownViewModel>()
            .AddView<PanelTabHeaderView, IPanelTabHeaderViewModel>()
            .AddView<NewTabPageView, INewTabPageViewModel>()

            // page factories
            .AddSingleton<PageFactoryController>()
            .AddSingleton<IPageFactory, DummyPageFactory>()
            .AddSingleton<IPageFactory, NewTabPageFactory>()
            .AddSingleton<IPageFactory, MyGamesPageFactory>()
            .AddSingleton<IPageFactory, LoadoutGridPageFactory>()
            .AddSingleton<IPageFactory, InProgressPageFactory>()

            // LeftMenu factories
            .AddSingleton<ILeftMenuFactory, DownloadsLeftMenuFactory>()
            .AddSingleton<ILeftMenuFactory, HomeLeftMenuFactory>()
            .AddSingleton<ILeftMenuFactory, LoadoutLeftMenuFactory>()

            // Workspace Attachments
            .AddSingleton<IWorkspaceAttachmentsFactoryManager, WorkspaceAttachmentsFactoryManager>()
            .AddSingleton<IWorkspaceAttachmentsFactory, DownloadsAttachmentsFactory>()
            .AddSingleton<IWorkspaceAttachmentsFactory, HomeAttachmentsFactory>()
            .AddSingleton<IWorkspaceAttachmentsFactory, LoadoutAttachmentsFactory>()

            // Other
            .AddViewModel<DummyViewModel, IDummyViewModel>()
            .AddView<DummyView, IDummyViewModel>()
            .AddSingleton<InjectedViewLocator>()
            .AddFileSystem();
    }

}
