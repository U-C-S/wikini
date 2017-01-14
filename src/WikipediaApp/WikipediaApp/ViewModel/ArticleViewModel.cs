﻿using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace WikipediaApp
{
  public class ArticleViewModel : ViewModelBase
  {
    private readonly WikipediaService wikipediaService = new WikipediaService();
    private readonly NavigationService navigationService = new NavigationService();
    private readonly DialogService dialogService = new DialogService();

    private bool isBusy = false;

    private readonly ArticleHead initialArticle;

    private Article article = null;
    private IList<ArticleSection> sections = null;
    private IList<ArticleLanguage> languages = null;

    private Command refreshCommand;
    private Command<ArticleLanguage> changeLanguageCommand;
    private Command openInBrowserCommand;

    private Command<Uri> navigateCommand;
    private Command<Uri> loadedCommand;
    private Command<Article> showArticleCommand;

    public bool IsBusy
    {
      get { return isBusy; }
      set { SetProperty(ref isBusy, value); }
    }

    public IList<Article> History
    {
      get { return ArticleHistory.Session; }
    }

    public Article Article
    {
      get { return article; }
      private set
      {
        if (SetProperty(ref article, value))
        {
          if (article != null)
          {
            Languages = article.Languages;
            Sections = Settings.Current.SectionsCollapsed ? article.GetRootSections() : article.Sections;

            ArticleHistory.AddArticle(article);
          }
          else
          {
            Languages = null;
            Sections = null;
          }
        }
      }
    }

    public IList<ArticleSection> Sections
    {
      get { return sections; }
      private set { SetProperty(ref sections, value); }
    }

    public IList<ArticleLanguage> Languages
    {
      get { return languages; }
      private set { SetProperty(ref languages, value); }
    }

    public ICommand RefreshCommand
    {
      get { return refreshCommand ?? (refreshCommand = new Command(Refresh)); }
    }

    public ICommand ChangeLanguageCommand
    {
      get { return changeLanguageCommand ?? (changeLanguageCommand = new Command<ArticleLanguage>(ChangeLanguage)); }
    }

    public ICommand OpenInBrowserCommand
    {
      get { return openInBrowserCommand ?? (openInBrowserCommand = new Command(OpenInBrowser)); }
    }

    public ICommand NavigateCommand
    {
      get { return navigateCommand ?? (navigateCommand = new Command<Uri>(Navigate)); }
    }

    public ICommand LoadedCommand
    {
      get { return loadedCommand ?? (loadedCommand = new Command<Uri>(Loaded)); }
    }

    public ICommand ShowArticleCommand
    {
      get { return showArticleCommand ?? (showArticleCommand = new Command<Article>(ShowArticle)); }
    }

    public ArticleViewModel(ArticleHead initialArticle)
    {
      this.initialArticle = initialArticle;
    }

    private async void Refresh()
    {
      if (article == null && initialArticle == null)
        return;

      IsBusy = true;

      var updated = article != null
        ? await wikipediaService.RefreshArticle(article, Settings.Current.SectionsCollapsed)
        : await wikipediaService.GetArticle(initialArticle, Settings.Current.SectionsCollapsed);

      if (updated != null)
      {
        Article = null;
        Article = updated;
      }
      else
      {
        IsBusy = false;

        dialogService.ShowLoadingError();
      }
    }

    private void ChangeLanguage(ArticleLanguage language)
    {
      Navigate(language.Uri);
    }

    private void OpenInBrowser()
    {
      var uri = article?.Uri ?? initialArticle?.Uri;

      if (uri != null)
        navigationService.OpenInBrowser(uri);
    }

    private async void Navigate(Uri uri)
    {
      IsBusy = true;

      if (wikipediaService.IsWikipediaUri(uri))
      {
        var article = await wikipediaService.GetArticle(uri, Settings.Current.SectionsCollapsed);

        if (article != null)
        {
          Article = article;
        }
        else
        {
          IsBusy = false;

          dialogService.ShowLoadingError();
        }
      }
      else
      {
        IsBusy = false;

        navigationService.OpenInBrowser(uri);
      }
    }

    private void Loaded(Uri uri)
    {
      IsBusy = false;
    }

    private void ShowArticle(Article article)
    {
      if (this.article != null && (this.article == article || this.article.Uri == article.Uri))
        return;

      IsBusy = true;
      Article = article;
    }

    public override async void Initialize()
    {
      IsBusy = true;

      var article = await wikipediaService.GetArticle(initialArticle, Settings.Current.SectionsCollapsed);

      if (article != null)
        Article = article;
      else
      {
        IsBusy = false;

        dialogService.ShowLoadingError();
      }
    }
  }
}