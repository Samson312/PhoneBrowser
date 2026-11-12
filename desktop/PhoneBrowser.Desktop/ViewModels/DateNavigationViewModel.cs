namespace PhoneBrowser.Desktop.ViewModels;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PhoneBrowser.Desktop.Helpers;
using PhoneBrowser.Desktop.Models;
using System.Collections.ObjectModel;
using System.Globalization;

public enum DateNavigationLevel { Years, Months }

public record YearItem(int Year, int PhotoCount)
{
    public string CountText => PhotoCount == 1 ? "1 zdjęcie" : PolishPlural.Photos(PhotoCount);
}

public record MonthItem(int Year, int Month, int PhotoCount)
{
    public string Name
    {
        get
        {
            var name = CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(Month);
            return char.ToUpper(name[0]) + name[1..];
        }
    }

    public string CountText => PhotoCount == 1 ? "1 zdjęcie" : PolishPlural.Photos(PhotoCount);
}

public partial class DateNavigationViewModel : ObservableObject
{
    private const int PageSize = 6;

    private IReadOnlyList<YearItem> allYears = [];
    private IReadOnlyList<MonthItem> allMonths = [];
    private IReadOnlyList<MonthItem> selectedYearMonths = [];

    private int page;

    private int yearsPageBeforeMonths;

    public ObservableCollection<YearItem> Years { get; } = new();
    public ObservableCollection<MonthItem> Months { get; } = new();

    public bool IsYearLevel => Level == DateNavigationLevel.Years;
    public bool IsMonthLevel => Level == DateNavigationLevel.Months;

    public bool CanGoNewer => page > 0;
    public bool CanGoOlder => (page + 1) * PageSize < CurrentCount;

    private int CurrentCount => IsYearLevel ? allYears.Count : selectedYearMonths.Count;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsYearLevel), nameof(IsMonthLevel))]
    private DateNavigationLevel level = DateNavigationLevel.Years;

    [ObservableProperty]
    private int selectedYear;

    public event Action<MonthItem>? MonthSelected;

    public void Load(IEnumerable<PhotoItem> photos)
    {
        allMonths = photos
            .GroupBy(p => (p.DateTakenUtc.Year, p.DateTakenUtc.Month))
            .Select(g => new MonthItem(g.Key.Year, g.Key.Month, g.Count()))
            .OrderByDescending(m => m.Year)
            .ThenByDescending(m => m.Month)
            .ToList();

        allYears = allMonths
            .GroupBy(m => m.Year)
            .Select(g => new YearItem(g.Key, g.Sum(m => m.PhotoCount)))
            .ToList();

        page = 0;
        Level = DateNavigationLevel.Years;
        ShowPage();
    }

    private void ShowPage()
    {
        if (IsYearLevel) Refill(Years, allYears);
        else Refill(Months, selectedYearMonths);

        OnPropertyChanged(nameof(CanGoNewer));
        OnPropertyChanged(nameof(CanGoOlder));
    }

    private void Refill<T>(ObservableCollection<T> target, IReadOnlyList<T> source)
    {
        target.Clear();

        foreach (var item in source.Skip(page * PageSize).Take(PageSize))
            target.Add(item);
    }

    [RelayCommand]
    private void SelectYear(YearItem year)
    {
        yearsPageBeforeMonths = page;

        selectedYearMonths = allMonths.Where(m => m.Year == year.Year).ToList();
        SelectedYear = year.Year;

        page = 0;
        Level = DateNavigationLevel.Months;
        ShowPage();
    }

    [RelayCommand]
    private void BackToYears()
    {
        page = yearsPageBeforeMonths;
        Level = DateNavigationLevel.Years;
        ShowPage();
    }

    [RelayCommand]
    private void Newer()
    {
        if (!CanGoNewer) return;

        page--;
        ShowPage();
    }

    [RelayCommand]
    private void Older()
    {
        if (!CanGoOlder) return;

        page++;
        ShowPage();
    }

    [RelayCommand]
    private void SelectMonth(MonthItem month) => MonthSelected?.Invoke(month);
}