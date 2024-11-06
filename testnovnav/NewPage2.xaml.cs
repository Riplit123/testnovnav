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

    // ��������� ����� ��� ������� ����� (� ������)
    (double minHeight, double maxHeight, string floorName)[] floorHeights = new (double, double, string)[]
    {
            (130.00, 159.50, "������ ����"),
            (159.90, 161.50, "������ ����"),
            (162.50, 164.10, "������ ����"),
            (164.20, 175.70, "��������� ����"),
            (175.70, 300.70, "����� ����"),
    };
    public bool ShowHandle { get; internal set; }

    // ������� ����� ������������
    double markerX = 135;
    double markerY = 240;

    // ���������� �������� ��� ��������� ����� �����������
    double previousTotalX = 0;
    double previousTotalY = 0;

    // ���������� �������� ������������� ��� �������� �����
    double prevAcceleration = 0;
    int stepCount = 0;

    // ���������� ��� �������
    double currentHeading = 0;

    // ������� ����
    int currentFloor = 1;
    public NewPage2()
	{
		InitializeComponent();
        // ���������� ������� ��� ����������� �����
        var panGesture = new PanGestureRecognizer();
        panGesture.PanUpdated += OnPanUpdated;
        UserMarker.GestureRecognizers.Add(panGesture);

        // ��������� ��������� ��������� � Label
        CoordinatesLabel.Text = $"����������: X = {markerX:F2}, Y = {markerY:F2}";

        // ������ ��������� ������ � ��������
        StartSensors();

        // �������� �������������� ������������ ������
        isTracking = true;
        cts = new CancellationTokenSource();
        StartTrackingAsync(cts.Token);
    }

    private async void StartTrackingAsync(CancellationToken token)
    {
        try
        {
            // ��������� � ����������� ���������� �� ������ � ��������������
            var locationStatus = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
            if (locationStatus != PermissionStatus.Granted)
            {
                locationStatus = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                if (locationStatus != PermissionStatus.Granted)
                {
                    await DisplayAlert("������", "�� ������������� ����� �� ������ � ��������������.", "OK");
                    return;
                }
            }

            while (!token.IsCancellationRequested)
            {
                await UpdateAltitudeAsync();

                await Task.Delay(1000, token); // �������� ����� ������������ (1 �������)
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("������", $"�� ������� �������� ������: {ex.Message}", "OK");
        }
    }

    private async Task UpdateAltitudeAsync()
    {
        var location = await Geolocation.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
        if (location != null)
        {
            // �������� �������� ������
            double altitude = location.Altitude.HasValue ? location.Altitude.Value : 0;

            // ��������, ���� ������ ������ 0, �������� GPS ��� ������������� ��������
            if (altitude < 0)
                altitude = -altitude;

            // ��������� ���������
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CoordinatesLabel.Text = $"{altitude:F2} �";
                string floorName = DetermineFloor(altitude);

                UpdateFloorDisplay(floorName);
            });
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                CoordinatesLabel.Text = "�� ������� ���������� ������.";
                // �������� ��� �����
                UpdateFloorDisplay("����������� ����");
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
        return "����������� ����";
    }

    private void UpdateFloorDisplay(string floorName)
    {
        // �������� ��� Frame
           Floor1Layout.IsVisible = false;
           Floor2Layout.IsVisible = false;
           Floor3Layout.IsVisible = false;
           Floor4Layout.IsVisible = false;
           Floor5Layout.IsVisible = false;

        // ���������� Frame ���������������� �����
           if (floorName == "������ ����")
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
            else if (floorName == "������ ����")
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
             else if (floorName == "������ ����")
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
           else if (floorName == "��������� ����")
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
           else if (floorName == "����� ����")
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
            // ����������� ���� ��� �� ������� ���������� ������
            // ����� �������� ��� Frame ��������
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
                // ����� ���������� �������� ��� ������ �����
                previousTotalX = 0;
                previousTotalY = 0;
                break;

            case GestureStatus.Running:
                // ���������� �������� � ���������� ����������
                double deltaX = e.TotalX - previousTotalX;
                double deltaY = e.TotalY - previousTotalY;

                // ���������� ������� ����� �������� ��� ���������� ����������
                previousTotalX = e.TotalX;
                previousTotalY = e.TotalY;

                // ���������� ������� ����� � ������ ������������
                TryMoveMarker(deltaX, deltaY);
                break;
            case GestureStatus.Completed:
            case GestureStatus.Canceled:
                // ����� ���������� �������� ��� ���������� ��� ������ �����
                previousTotalX = 0;
                previousTotalY = 0;
                break;
        }
    }

    private void TryMoveMarker(double deltaX, double deltaY)
    {
        // ���������� ������ �������
        double oldX = markerX;
        double oldY = markerY;

        // ���������� �������
        markerX += deltaX;
        markerY += deltaY;

        // ���������� ������� �����
        UpdateMarkerPosition();

        // ���� ������������ �� ������, �������� �������
        if (IsCollidingWithWall())
        {
            markerX = oldX;
            markerY = oldY;
            UpdateMarkerPosition();
        }

      
    }

    private void UpdateMarkerPosition()
    {
        // �������� ������ ������� ���������
        markerX = Math.Max(0, Math.Min(NavigationArea.WidthRequest - UserMarker.WidthRequest, markerX));
        markerY = Math.Max(0, Math.Min(NavigationArea.HeightRequest - UserMarker.HeightRequest, markerY));

        // ���������� ������� �����
        AbsoluteLayout.SetLayoutBounds(UserMarker, new Rect(markerX, markerY, 20, 20));

        // ���������� ��������� � Label
        //CoordinatesLabel.Text = $"����������: X = {markerX:F2}, Y = {markerY:F2}";

        // �������� ���������� � �������
        if (IsInsideRoom())
        {
            RoomLabel.Text = "����";
        }

        // �������� ���������� � �������
        if (IsInsideRoom2())
        {
            RoomLabel.Text = "����";
        }

        // �������� ���������� � �������
        if (IsInsideRoom3())
        {
            RoomLabel.Text = "���������";
        }

        // �������� ���������� � �������
        if (IsInsideRoom4())
        {
            RoomLabel.Text = "��������";
        }

        // �������� ���������� � �������
        if (IsInsideRoom5())
        {
            RoomLabel.Text = "���";
        }

        // �������� ���������� � �������
        if (IsInsideRoom6())
        {
            RoomLabel.Text = "��������";
        }

        // �������� ���������� � �������
        if (currentFloor == 3)
        {
            if(IsInsideRoom7())
            {
              RoomLabel.Text = "IT-�������";
            }
            if (IsInsideRoom8())
            {
              RoomLabel.Text = "VR/AR-�������";
            }
            if (IsInsideRoom9())
            {
                RoomLabel.Text = "�������";
            }

        }
    }

    private bool IsCollidingWithWall()
    {
        // �������� ������ ���� � ����������� �� �������� �����
        BoxView[] walls = currentFloor switch
        {
            1 => new[] { Wall1_Floor1, Wall2_Floor1, Wall3_Floor1, Wall4_Floor1, Wall5_Floor1, Wall6_Floor1 },
            2 => new[] { Wall1_Floor2, Wall2_Floor2 },
            3 => new[] { Wall1_Floor3, Wall2_Floor3 },
            4 => new[] { Wall1_Floor4, Wall2_Floor4 },
            5 => new[] { Wall1_Floor5, Wall2_Floor5 },
            _ => Array.Empty<BoxView>()
        };

        // �������� ����������� � ������ ������
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� ������� � ����������� �� �������� �����
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
        // �������� �� ������ �������������
        if (!Accelerometer.IsMonitoring)
        {
            Accelerometer.ReadingChanged += Accelerometer_ReadingChanged;
            try
            {
                Accelerometer.Start(SensorSpeed.UI);
            }
            catch (FeatureNotSupportedException)
            {
                // ������������ �� �������������� �� ����������
            }
        }

        // �������� �� ������ �������
        if (!Compass.IsMonitoring)
        {
            Compass.ReadingChanged += Compass_ReadingChanged;
            try
            {
                Compass.Start(SensorSpeed.UI, true);
            }
            catch (FeatureNotSupportedException)
            {
                // ������ �� �������������� �� ����������
            }
        }
    }

    private void Accelerometer_ReadingChanged(object sender, AccelerometerChangedEventArgs e)
    {
        // ��������� ������ ������������� ��� �������� �����
        double acceleration = Math.Sqrt(
            Math.Pow(e.Reading.Acceleration.X, 2) +
            Math.Pow(e.Reading.Acceleration.Y, 2) +
            Math.Pow(e.Reading.Acceleration.Z, 2));

        double delta = Math.Abs(acceleration - prevAcceleration);

        if (delta > 0.2) // ����� ��� ����������� ����
        {
            stepCount++;
            MoveMarkerByStep();
        }

        prevAcceleration = acceleration;
    }

    [Obsolete]
    private void MoveMarkerByStep()
    {
        // ������������ ����� � ����������� currentHeading
        Device.BeginInvokeOnMainThread(() =>
        {
            double stepSize = 3; // ������ ���� � �������� 

            double radians = currentHeading * Math.PI / 180;

            double deltaX = stepSize * Math.Sin(radians);
            double deltaY = stepSize * Math.Cos(radians);

            TryMoveMarker(deltaX, -deltaY); // ����������� deltaY ��� ����������� �����������
        });
    }

    private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
    {
        // ���������� �������� �����������
        currentHeading = e.Reading.HeadingMagneticNorth;
    }

    private void OnFloor1ButtonClicked(object sender, EventArgs e)
    {
        // ������������� �������������� ����������
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // ������������ �� ������ ����
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

        // �������� ������� �����, ���� ����������
        UpdateMarkerPosition();
    }

    private void OnFloor2ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // ������������ �� ������ ����
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

        // �������� ������� �����, ���� ����������
        UpdateMarkerPosition();
    }

    private void OnFloor3ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // ������������ �� ������ ����
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


        // �������� ������� �����, ���� ����������
        UpdateMarkerPosition();
    }

    private void OnFloor4ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // ������������ �� ��������� ����
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

        // �������� ������� �����, ���� ����������
        UpdateMarkerPosition();
    }

    private void OnFloor5ButtonClicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        // ������������ �� ����� ����
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

        // �������� ������� �����, ���� ����������
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