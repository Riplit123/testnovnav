using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;
using System.Numerics;
using System.Threading.Tasks;
namespace testnovnav;

public partial class NewPage2 : ContentPage
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

    // Позиция метки пользователя
    double markerX = 135;
    double markerY = 240;

    // Предыдущие значения для обработки жеста перемещения
    double previousTotalX = 0;
    double previousTotalY = 0;

    // Предыдущие значения акселерометра для подсчета шагов
    double prevAcceleration = 0;
    int stepCount = 0;

    // Переменные для компаса
    double currentHeading = 0;

    // Текущий этаж
    int currentFloor = 1;
    public NewPage2()
	{
		InitializeComponent();
        // Обработчик касаний для перемещения метки
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        UserMarker.GestureRecognizers.Add(panGesture);

        // Установка начальных координат в Label
        CoordinatesLabel.Text = $"Координаты: X = {markerX:F2}, Y = {markerY:F2}";

        // Начать получение данных с датчиков
        StartSensors();

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
                CoordinatesLabel.Text = $"{altitude:F2} м";
                string floorName = DetermineFloor(altitude);

                UpdateFloorDisplay(floorName);
            });
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CoordinatesLabel.Text = "Не удалось определить высоту.";
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
           Floor1Layout.IsVisible = false;
           Floor2Layout.IsVisible = false;
           Floor3Layout.IsVisible = false;
           Floor4Layout.IsVisible = false;
           Floor5Layout.IsVisible = false;

        // Показываем Frame соответствующего этажа
           if (floorName == "Первый этаж")
           {
           
               currentFloor = 1;
               Floor1Layout.IsVisible = true;
               Floor2Layout.IsVisible = false;
               Floor3Layout.IsVisible = false;
               Floor4Layout.IsVisible = false;
               Floor5Layout.IsVisible = false;


               FloorName1.IsVisible = true;
               FloorName2.IsVisible = false;
               FloorName3.IsVisible = false;
               FloorName4.IsVisible = false;
               FloorName5.IsVisible = false;
            FloorPlanImage.Source = "floorone.jpg";
           }
            else if (floorName == "Второй этаж")
                {
                  currentFloor = 2;
                Floor1Layout.IsVisible = false;
                Floor2Layout.IsVisible = true;
                Floor3Layout.IsVisible = false;
                Floor4Layout.IsVisible = false;
                Floor5Layout.IsVisible = false;

                FloorName1.IsVisible = false;
                FloorName2.IsVisible = true;
                FloorName3.IsVisible = false;
                FloorName4.IsVisible = false;
                FloorName5.IsVisible = false;
                FloorPlanImage.Source = "floortwo.jpg";
            }
             else if (floorName == "Третий этаж")
            {
                currentFloor = 3;
                Floor1Layout.IsVisible = false;
                Floor2Layout.IsVisible = false;
                Floor3Layout.IsVisible = true;
                Floor4Layout.IsVisible = false;
                Floor5Layout.IsVisible = false;


                FloorName1.IsVisible = false;
                FloorName2.IsVisible = false;
                FloorName3.IsVisible = true;
                FloorName4.IsVisible = false;
                FloorName5.IsVisible = false;
                FloorPlanImage.Source = "floorthree.jpg";
             }
           else if (floorName == "Четвертый этаж")
           {
            currentFloor = 4;
            Floor1Layout.IsVisible = false;
            Floor2Layout.IsVisible = false;
            Floor3Layout.IsVisible = false;
            Floor4Layout.IsVisible = true;
            Floor5Layout.IsVisible = false;


            FloorName1.IsVisible = false;
            FloorName2.IsVisible = false;
            FloorName3.IsVisible = false;
            FloorName4.IsVisible = true;
            FloorName5.IsVisible = false;
            FloorPlanImage.Source = "floorfour.jpg";
            }
           else if (floorName == "Пятый этаж")
            {
            currentFloor = 5;
            Floor1Layout.IsVisible = false;
            Floor2Layout.IsVisible = false;
            Floor3Layout.IsVisible = false;
            Floor4Layout.IsVisible = false;
            Floor5Layout.IsVisible = true;


            FloorName1.IsVisible = false;
            FloorName2.IsVisible = false;
            FloorName3.IsVisible = false;
            FloorName4.IsVisible = false;
            FloorName5.IsVisible = true;
            FloorPlanImage.Source = "floorfive.jpg";
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

    private void OnPanUpdated(object sender, PanUpdatedEventArgs e)
    {
        switch (e.StatusType)
        {
            case GestureStatus.Started:
                // Сброс предыдущих значений при начале жеста
                previousTotalX = 0;
                previousTotalY = 0;
                break;

            case GestureStatus.Running:
                // Вычисление смещения с последнего обновления
                double deltaX = e.TotalX - previousTotalX;
                double deltaY = e.TotalY - previousTotalY;

                // Сохранение текущих общих смещений для следующего обновления
                previousTotalX = e.TotalX;
                previousTotalY = e.TotalY;

                // Обновление позиции метки с учетом столкновений
                TryMoveMarker(deltaX, deltaY);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                // Сброс предыдущих значений при завершении или отмене жеста
                previousTotalX = 0;
                previousTotalY = 0;
                break;
        }
    }

    private void TryMoveMarker(double deltaX, double deltaY)
    {
        // Сохранение старой позиции
        double oldX = markerX;
        double oldY = markerY;

        // Обновление позиции
        markerX += deltaX;
        markerY += deltaY;

        // Обновление позиции метки
        UpdateMarkerPosition();

        // Если столкновение со стеной, откатить позицию
        if (IsCollidingWithWall())
        {
            markerX = oldX;
            markerY = oldY;
            UpdateMarkerPosition();
        }

      
    }

    private void UpdateMarkerPosition()
    {
        // Проверка границ области навигации
        markerX = Math.Max(0, Math.Min(NavigationArea.WidthRequest - UserMarker.WidthRequest, markerX));
        markerY = Math.Max(0, Math.Min(NavigationArea.HeightRequest - UserMarker.HeightRequest, markerY));

        // Обновление позиции метки
        AbsoluteLayout.SetLayoutBounds(UserMarker, new Rect(markerX, markerY, 20, 20));

        // Обновление координат в Label
        //CoordinatesLabel.Text = $"Координаты: X = {markerX:F2}, Y = {markerY:F2}";

        // Проверка нахождения в комнате
        if (IsInsideRoom())
        {
            RoomLabel.Text = "холл";
        }

        // Проверка нахождения в комнате
        if (IsInsideRoom2())
        {
            RoomLabel.Text = "холл";
        }

        // Проверка нахождения в комнате
        if (IsInsideRoom3())
        {
            RoomLabel.Text = "коворкинг";
        }

        // Проверка нахождения в комнате
        if (IsInsideRoom4())
        {
            RoomLabel.Text = "лестница";
        }

        // Проверка нахождения в комнате
        if (IsInsideRoom5())
        {
            RoomLabel.Text = "зал";
        }

        // Проверка нахождения в комнате
        if (IsInsideRoom6())
        {
            RoomLabel.Text = "лестница";
        }

        // Проверка нахождения в комнате
        if (currentFloor == 3)
        {
            if(IsInsideRoom7())
            {
              RoomLabel.Text = "IT-квантум";
            }
            if (IsInsideRoom8())
            {
              RoomLabel.Text = "VR/AR-квантум";
            }
            if (IsInsideRoom9())
            {
                RoomLabel.Text = "коридор";
            }

        }
    }

    private bool IsCollidingWithWall()
    {
        // Получаем список стен в зависимости от текущего этажа
        BoxView[] walls = currentFloor switch
        {
            1 => new[] { Wall1_Floor1, Wall2_Floor1, Wall3_Floor1, Wall4_Floor1, Wall5_Floor1, Wall6_Floor1 },
            2 => new[] { Wall1_Floor2, Wall2_Floor2 },
            3 => new[] { Wall1_Floor3, Wall2_Floor3 },
            4 => new[] { Wall1_Floor4, Wall2_Floor4 },
            5 => new[] { Wall1_Floor5, Wall2_Floor5 },
            _ => Array.Empty<BoxView>()
        };

        // Проверка пересечения с каждой стеной
        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        foreach (var wall in walls)
        {
            Rect wallRect = new Rect(
                AbsoluteLayout.GetLayoutBounds(wall).X,
                AbsoluteLayout.GetLayoutBounds(wall).Y,
                wall.WidthRequest,
                wall.HeightRequest);

            if (markerRect.IntersectsWith(wallRect))
            {
                return true;
            }
        }

        return false;
    }

   

    private bool IsInsideRoom()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 1 ? Room1_Floor1 : Room1_Floor2;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom2()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 1 ? Room2_Floor1 : Room1_Floor2;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom3()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 1 ? Room3_Floor1 : Room1_Floor2;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom4()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 1 ? Room4_Floor1 : Room1_Floor2;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom5()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 2 ? Room1_Floor2 : Room2_Floor2;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom6()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 2 ? Room2_Floor2 : Room2_Floor2;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom7()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 3 ? Room1_Floor3 : Room1_Floor3;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom8()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 3 ? Room2_Floor3 : Room2_Floor3;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private bool IsInsideRoom9()
    {
        // Получаем комнату в зависимости от текущего этажа
        BoxView room = currentFloor == 3 ? Room3_Floor3 : Room3_Floor3;

        Rect markerRect = new Rect(markerX, markerY, UserMarker.WidthRequest, UserMarker.HeightRequest);

        Rect roomRect = new Rect(
            AbsoluteLayout.GetLayoutBounds(room).X,
            AbsoluteLayout.GetLayoutBounds(room).Y,
            room.WidthRequest,
            room.HeightRequest);

        if (roomRect.Contains(markerRect))
        {
            return true;
        }

        return false;
    }

    private void StartSensors()
    {
        // Подписка на данные акселерометра
        if (!Accelerometer.IsMonitoring)
        {
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            try
            {
                Accelerometer.Start(SensorSpeed.UI);
            }
            catch (FeatureNotSupportedException)
            {
                // Акселерометр не поддерживается на устройстве
            }
        }

        // Подписка на данные компаса
        if (!Compass.IsMonitoring)
        {
            Compass.ReadingChanged += Compass_ReadingChanged;
            try
            {
                Compass.Start(SensorSpeed.UI, true);
            }
            catch (FeatureNotSupportedException)
            {
                // Компас не поддерживается на устройстве
            }
        }
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        // Обработка данных акселерометра для подсчета шагов
        double acceleration = Math.Sqrt(
            Math.Pow(e.Reading.Acceleration.X, 2) +
            Math.Pow(e.Reading.Acceleration.Y, 2) +
            Math.Pow(e.Reading.Acceleration.Z, 2));

        double delta = Math.Abs(acceleration - prevAcceleration);

        if (delta > 0.2) // Порог для обнаружения шага
        {
            stepCount++;
            MoveMarkerByStep();
        }

        prevAcceleration = acceleration;
    }

    [Obsolete]
    private void MoveMarkerByStep()
    {
        // Передвижение метки в направлении currentHeading
        Device.BeginInvokeOnMainThread(() =>
        {
            double stepSize = 3; // Размер шага в пикселях 

            double radians = currentHeading * Math.PI / 180;

            double deltaX = stepSize * Math.Sin(radians);
            double deltaY = stepSize * Math.Cos(radians);

            TryMoveMarker(deltaX, -deltaY); // Инвертируем deltaY для корректного направления
        });
    }

    private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
    {
        // Обновление текущего направления
        currentHeading = e.Reading.HeadingMagneticNorth;
    }

    private void OnFloor1ButtonClicked(object sender, EventArgs e)
    {
        // Останавливаем автоматическое обновление
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // Переключение на первый этаж
        currentFloor = 1;
        FloorPlanImage.Source = "floorone.jpg";
        FloorName1.IsVisible = true;
        FloorName2.IsVisible = false;
        FloorName3.IsVisible = false;
        FloorName4.IsVisible = false;
        FloorName5.IsVisible = false;
        Floor1Layout.IsVisible = true;
        Floor2Layout.IsVisible = false;
        Floor3Layout.IsVisible = false;
        Floor4Layout.IsVisible = false;
        Floor5Layout.IsVisible = false;

        // Обновить позицию метки, если необходимо
        UpdateMarkerPosition();
    }

    private void OnFloor2ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // Переключение на второй этаж
        currentFloor = 2;
        FloorPlanImage.Source = "floortwo.jpg";
        FloorName1.IsVisible = false;
        FloorName2.IsVisible = true;
        FloorName3.IsVisible = false;
        FloorName4.IsVisible = false;
        FloorName5.IsVisible = false;
        Floor1Layout.IsVisible = false;
        Floor2Layout.IsVisible = true;
        Floor3Layout.IsVisible = false;
        Floor4Layout.IsVisible = false;
        Floor5Layout.IsVisible = false;

        // Обновить позицию метки, если необходимо
        UpdateMarkerPosition();
    }

    private void OnFloor3ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // Переключение на третий этаж
        currentFloor = 3;
        FloorPlanImage.Source = "floorthree.jpg";
        FloorName1.IsVisible = false;
        FloorName2.IsVisible = false;
        FloorName3.IsVisible = true;
        FloorName4.IsVisible = false;
        FloorName5.IsVisible = false;
        Floor1Layout.IsVisible = false;
        Floor2Layout.IsVisible = false;
        Floor3Layout.IsVisible = true;
        Floor4Layout.IsVisible = false;
        Floor5Layout.IsVisible = false;


        // Обновить позицию метки, если необходимо
        UpdateMarkerPosition();
    }

    private void OnFloor4ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // Переключение на четвертый этаж
        currentFloor = 4;
        FloorPlanImage.Source = "floorfour.jpg";
        FloorName1.IsVisible = false;
        FloorName2.IsVisible = false;
        FloorName3.IsVisible = false;
        FloorName4.IsVisible = true;
        FloorName5.IsVisible = false;
        Floor1Layout.IsVisible = false;
        Floor2Layout.IsVisible = false;
        Floor3Layout.IsVisible = false;
        Floor4Layout.IsVisible = true;
        Floor5Layout.IsVisible = false;

        // Обновить позицию метки, если необходимо
        UpdateMarkerPosition();
    }

    private void OnFloor5ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // Переключение на пятый этаж
        currentFloor = 5;
        FloorPlanImage.Source = "floorfive.jpg";
        FloorName1.IsVisible = false;
        FloorName2.IsVisible = false;
        FloorName3.IsVisible = false;
        FloorName4.IsVisible = false;
        FloorName5.IsVisible = true;
        Floor1Layout.IsVisible = false;
        Floor2Layout.IsVisible = false;
        Floor3Layout.IsVisible = false;
        Floor4Layout.IsVisible = false;
        Floor5Layout.IsVisible = true;

        // Обновить позицию метки, если необходимо
        UpdateMarkerPosition();
    }
    private void Button_Clicked_1(object sender, EventArgs e)
    {
        otkuda.IsVisible = true;
        routeman.IsVisible = true;
    }

    private void Button_Clicked_2(object sender, EventArgs e)
    {
        kuda.IsEnabled = false;
        textfloor.IsVisible = true;
    }

    private void Button_Clicked_3(object sender, EventArgs e)
    {
        kudaimg.IsVisible = false;
        visit1.IsVisible = false;
        arrow1.IsVisible = false;
        stairs1.IsVisible = false;
        arrow2.IsVisible = false;
        arrow3.IsVisible = false;
        itlogo.IsVisible = false;
        floor3num.IsVisible = false;
        otkuda.IsVisible = false;
        routeman.IsVisible = false;
        otkudabutton.IsEnabled = true;
        kuda.IsEnabled = false;
        otkudaimg.IsVisible = false;
        routeone.IsVisible = false;
        routetwo.IsVisible = false;
        routethree.IsVisible = false;
    }

    private void Button_Clicked(object sender, EventArgs e)
    {
        otkudaimg.IsVisible = true;
        kuda.IsEnabled = true;
        kudabut.IsVisible = true;
        otkudabutton.IsEnabled = false;
    }

    private void kudabut_Clicked(object sender, EventArgs e)
    {
        kudaimg.IsVisible = true;
        visit1.IsVisible = true;
        arrow1.IsVisible = true;
        stairs1.IsVisible = true;
        arrow2.IsVisible = true;
        arrow3.IsVisible = true;
        itlogo.IsVisible = true;
        floor3num.IsVisible = true;
        textfloor.IsVisible = false;
        routeone.IsVisible = true;
        routetwo.IsVisible = true;
        routethree.IsVisible = true;
    }
}