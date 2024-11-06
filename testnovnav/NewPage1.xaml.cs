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

    public NewPage1()
    {
        InitializeComponent();

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
                AltitudeLabel.Text = $"{altitude:F2} �";
                string floorName = DetermineFloor(altitude);

                UpdateFloorDisplay(floorName);
            });
        }
        else
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                AltitudeLabel.Text = "�� ������� ���������� ������.";
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
        FirstFloorFrame.IsVisible = false;
        SecondFloorFrame.IsVisible = false;
        ThirdFloorFrame.IsVisible = false;
        FourFloorFrame.IsVisible = false;
        FiveFloorFrame.IsVisible = false;

        // ���������� Frame ���������������� �����
        if (floorName == "������ ����")
        {
            floorspage.BackgroundColor = Colors.White;
            FirstFloorFrame.IsVisible = true;
        }
        else if (floorName == "������ ����")
        {
            floorspage.BackgroundColor = Colors.White;
            SecondFloorFrame.IsVisible = true;
        }        
        else if (floorName == "������ ����")
        {
            floorspage.BackgroundColor = Colors.White;
            ThirdFloorFrame.IsVisible = true;
        }
        else if (floorName == "��������� ����")
        {
            floorspage.BackgroundColor = Colors.White;
            FourFloorFrame.IsVisible = true;
        }
        else if (floorName == "����� ����")
        {
            floorspage.BackgroundColor = Colors.White;
            FiveFloorFrame.IsVisible = true;
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

    // ����������� ������� �� ������ ������
    private void FloorButton1_Clicked(object sender, EventArgs e)
    {
        // ������������� �������������� ����������
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("������ ����");

    }

    private void FloorButton2_Clicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("������ ����");
    }

    private void FloorButton3_Clicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("������ ����");
    }

    // ���� ��� ����� ������ ��� ������� ����������, ������ �������� ���� ����������
    private async void UpdateButton_Clicked(object sender, EventArgs e)
    {
        // ��������� ����������� ���������� ������
        await UpdateAltitudeAsync();
    }

    private void FloorButton4_Clicked(object sender, EventArgs e)
    {
        if (isTracking)
        {
            isTracking = false;
            cts?.Cancel();
        }
        UpdateFloorDisplay("��������� ����");
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
        UpdateFloorDisplay("����� ����");
    }
}








