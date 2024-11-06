using System.Text.Json;
using Microsoft.Maui.Controls;
using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace testnovnav;

public partial class NewPage1 : ContentPage
{
    CancellationTokenSource cts;
    bool isTracking = false;

    // Диапазоны высот для каждого этажа (в метрах)
    (double minHeight, double maxHeight, string floorName)[] floorHeights = new (double, double, string)[]
    {
            (130.00, 159.50, "Первый этаж"),
            (159.90, 161.50, "Второй этаж"),
            (162.50, 164.10, "Третий этаж"),
            (164.20, 175.70, "Четвертый этаж"),
            (175.70, 300.70, "Пятый этаж"),
    };

    public bool ShowHandle { get; internal set; }

    public NewPage1()
    {
        InitializeComponent();

        // Начинаем автоматическое отслеживание высоты
        isTracking = true;
        cts = new CancellationTokenSource();
        StartTrackingAsync(cts.Token);
    }

    private async void StartTrackingAsync(CancellationToken token)
    {
        try
        {
            // Проверяем и запрашиваем разрешение на доступ к местоположению
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    await DisplayAlert("Ошибка", "Не предоставлены права на доступ к местоположению.", "OK");
                    return;
                }
            }

            while (!token.IsCancellationRequested)
            {
                await UpdateAltitudeAsync();

                await Task.Delay(1000, token); // Задержка между обновлениями (1 секунда)
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Ошибка", $"Не удалось получить данные: {ex.Message}", "OK");
        }
    }

    private async Task UpdateAltitudeAsync()
    {
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
        if (location != null)
        {
            // Получаем значение высоты
            double altitude = location.Altitude.HasValue ? location.Altitude.Value : 0;

            // Проверка, если высота меньше 0, возможно GPS даёт отрицательные значения
            if (altitude < 0)
                altitude = -altitude;

            // Обновляем интерфейс
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AltitudeLabel.Text = $"{altitude:F2} м";
                string floorName = DetermineFloor(altitude);

                UpdateFloorDisplay(floorName);
            });
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AltitudeLabel.Text = "Не удалось определить высоту.";
                // Скрываем все этажи
                UpdateFloorDisplay("Неизвестный этаж");
            });
        }
    }

    private string DetermineFloor(double altitude)
    {
        foreach (var floor in floorHeights)
        {
            if (altitude >= floor.minHeight && altitude <= floor.maxHeight)
            {
                return floor.floorName;
            }
        }
        return "Неизвестный этаж";
    }

    private void UpdateFloorDisplay(string floorName)
    {
        // Скрываем все Frame
        FirstFloorFrame.IsVisible = false;
        SecondFloorFrame.IsVisible = false;
        ThirdFloorFrame.IsVisible = false;
        FourFloorFrame.IsVisible = false;
        FiveFloorFrame.IsVisible = false;

        // Показываем Frame соответствующего этажа
        if (floorName == "Первый этаж")
        {
            floorspage.BackgroundColor = Colors.White;
            FirstFloorFrame.IsVisible = true;
        }
        else if (floorName == "Второй этаж")
        {
            floorspage.BackgroundColor = Colors.White;
            SecondFloorFrame.IsVisible = true;
        }        
        else if (floorName == "Третий этаж")
        {
            floorspage.BackgroundColor = Colors.White;
            ThirdFloorFrame.IsVisible = true;
        }
        else if (floorName == "Четвертый этаж")
        {
            floorspage.BackgroundColor = Colors.White;
            FourFloorFrame.IsVisible = true;
        }
        else if (floorName == "Пятый этаж")
        {
            floorspage.BackgroundColor = Colors.White;
            FiveFloorFrame.IsVisible = true;
        }
        else
        {
            // Неизвестный этаж или не удалось определить высоту
            // Можно оставить все Frame скрытыми
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
    }

    // Обработчики нажатия на кнопки этажей
    private void FloorButton1_Clicked(object sender, EventArgs e)
    {
        // Останавливаем автоматическое обновление
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("Первый этаж");

    }

    private void FloorButton2_Clicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("Второй этаж");
    }

    private void FloorButton3_Clicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("Третий этаж");
    }

    // Если вам нужна кнопка для ручного обновления, можете добавить этот обработчик
    private async void UpdateButton_Clicked(object sender, EventArgs e)
    {
        // Выполняем однократное обновление высоты
        await UpdateAltitudeAsync();
    }

    private void FloorButton4_Clicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("Четвертый этаж");
    }

    private void butotkuda_Clicked(object sender, EventArgs e)
    {
        floorsmenu.IsVisible = true;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
    }

    private void Button_Clicked_1(object sender, EventArgs e)
    {
        floorsmenu.IsVisible = false;
    }

    private void FloorButton5_Clicked(object sender, EventArgs e)
    {

        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("Пятый этаж");
    }
}








