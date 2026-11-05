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
    private IReadOnlyList<MonthItem> allMonths = [];

    public ObservableCollection<YearItem> Years { get; } = new();
    public ObservableCollection<MonthItem> Months { get; } = new();

    public bool IsYearLevel => Level == DateNavigationLevel.Years;
    public bool IsMonthLevel => Level == DateNavigationLevel.Months;

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

        Years.Clear();

        foreach (var year in allMonths.GroupBy(m => m.Year))
            Years.Add(new YearItem(year.Key, year.Sum(m => m.PhotoCount)));

        Level = DateNavigationLevel.Years;
    }

    [RelayCommand]
    private void SelectYear(YearItem year)
    {
        Months.Clear();

        foreach (var month in allMonths.Where(m => m.Year == year.Year))
            Months.Add(month);

        SelectedYear = year.Year;
        Level = DateNavigationLevel.Months;
    }

    [RelayCommand]
    private void BackToYears() => Level = DateNavigationLevel.Years;

    [RelayCommand]
    private void SelectMonth(MonthItem month) => MonthSelected?.Invoke(month);
}