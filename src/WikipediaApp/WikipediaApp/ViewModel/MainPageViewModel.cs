﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.Input;

namespace WikipediaApp
{
  public class MainPageViewModel : ViewModelBase
  {
    private readonly IWikipediaService wikipediaService;
    private readonly INavigationService navigationService;
    private readonly IDialogService dialogService;

    private bool isBusy = false;

    private RelayCommand showHomePageCommand = null;
    private RelayCommand showRandomArticleCommand = null;
    private RelayCommand showMapCommand = null;
    private RelayCommand<ArticleHead> showArticleCommand = null;

    private IList<LanguageViewModel> languages = null;
    private LanguageViewModel language = null;
    private RelayCommand<LanguageViewModel> changeLanguageCommand = null;

    public SearchViewModel Search { get; }

    public bool IsBusy
    {
      get { return isBusy; }
      private set { SetProperty(ref isBusy, value); }
    }

    public ICommand ShowHomePageCommand
    {
      get { return showHomePageCommand ?? (showHomePageCommand = new RelayCommand(ShowHomePage)); }
    }

    public ICommand ShowRandomArticleCommand
    {
      get { return showRandomArticleCommand ?? (showRandomArticleCommand = new RelayCommand(ShowRandomArticle)); }
    }

    public ICommand ShowMapCommand
    {
      get { return showMapCommand ?? (showMapCommand = new RelayCommand(ShowMap)); }
    }

    public ICommand ShowArticleCommand
    {
      get { return showArticleCommand ?? (showArticleCommand = new RelayCommand<ArticleHead>(ShowArticle)); }
    }

    public IList<LanguageViewModel> Languages
    {
      get { return languages; }
      private set { SetProperty(ref languages, value); }
    }

    public LanguageViewModel Language
    {
      get { return language; }
      private set
      {
        var currentLanguage = language;

        if (SetProperty(ref language, value))
        {
          language.IsActive = true;

          if (currentLanguage != null)
            currentLanguage.IsActive = false;

          Search.Language = language;
        }
      }
    }

    public ICommand ChangeLanguageCommand
    {
      get { return changeLanguageCommand ?? (changeLanguageCommand = new RelayCommand<LanguageViewModel>(ChangeLanguage)); }
    }

    public PictureOfTheDayViewModel PictureOfTheDay { get; }

    public MainPageViewModel(IWikipediaService wikipediaService, INavigationService navigationService, IDialogService dialogService, SearchViewModel searchViewModel, PictureOfTheDayViewModel pictureOfTheDayViewModel)
    {
      this.wikipediaService = wikipediaService;
      this.navigationService = navigationService;
      this.dialogService = dialogService;

      Search = searchViewModel;
      PictureOfTheDay = pictureOfTheDayViewModel;
    }

    private async void ShowHomePage()
    {
      IsBusy = true;

      var article = await wikipediaService.GetMainPage(language.Code);

      if (article != null)
        navigationService.ShowArticle(article);
      else
        dialogService.ShowLoadingError();

      IsBusy = false;
    }

    private async void ShowRandomArticle()
    {
      IsBusy = true;

      var article = await wikipediaService.GetRandomArticle(language.Code);

      if (article != null)
        navigationService.ShowArticle(article);
      else
        dialogService.ShowLoadingError();

      IsBusy = false;
    }

    private void ShowMap()
    {
      navigationService.ShowMap(Language.Code);
    }

    public void ShowArticle(ArticleHead article)
    {
      navigationService.ShowArticle(article);
    }

    private void ChangeLanguage(LanguageViewModel language)
    {
      if (Language == language)
        return;

      Settings.Current.SearchLanguage = language.Code;

      Language = language;
    }

    public override async Task Initialize()
    {
      if (ArticleLanguages.All.Count == 0 || Languages?.Count > 0)
        return;

      var language = Settings.Current.SearchLanguage;

      Languages = ArticleLanguages.All;
      Language = Languages.FirstOrDefault(x => x.Code == language) ?? Languages.First(x => x.Code == Settings.DefaultSearchLanguage);

      if (Settings.Current.StartPictureOfTheDay)
        PictureOfTheDay.Today();
    }
  }
}