
using System.Text.Json;

namespace testnovnav
{
    public partial class MainPage : ContentPage
    {
        CancellationTokenSource cts;
        bool isTracking = false;

        public MainPage()
        {
            InitializeComponent();

            // Загрузка карты при загрузке страницы
            var htmlSource = new HtmlWebViewSource
            {
                Html = @"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8' />
    <title>Яндекс.Карта</title>
    <script src='https://api-maps.yandex.ru/2.1/?apikey=9f4d093c-51a6-4adf-aa7b-1a48afc68077&lang=ru_RU' type='text/javascript'></script>
    <style>
                            html, body, #map {
                                width: 100%; height: 100%; padding: 0; margin: 0;
                            }
                        </style>
                        </head>
                        <body>
                        <div id='map'></div>
                        <script>
                            var myMap;
                            var fromPlacemark;
                            var toPlacemark;
                            var route;
                            var squarePlacemark;
                            var markerMode = null;
                            var userLocationPlacemark;
                            var squareGeometry;

                            ymaps.ready(init);

                            function init() {
                                myMap = new ymaps.Map('map', {
                                    center: [56.3264816, 44.0051395],
                                    zoom: 14
                                });

                                // Создаем квадратный объект на карте
                                var squareSize = 0.00025;
                                var centerCoords = [56.326856050820446, 44.00993764400482];

                                var squareCoordinates = [
                                    [
                                        [centerCoords[0] - squareSize, centerCoords[1] - squareSize],
                                        [centerCoords[0] - squareSize, centerCoords[1] + squareSize],
                                        [centerCoords[0] + squareSize, centerCoords[1] + squareSize],
                                        [centerCoords[0] + squareSize, centerCoords[1] - squareSize],
                                        [centerCoords[0] - squareSize, centerCoords[1] - squareSize]
                                    ]
                                ];

                                squarePlacemark = new ymaps.Polygon(squareCoordinates, {
                                    hintContent: 'Целевая точка'
                                }, {
                                    fillColor: '#00990055',
                                    strokeColor: '#00990055',
                                    strokeWidth: 2,
                                    draggable: false
                                });

                                myMap.geoObjects.add(squarePlacemark);

                                // Получаем геометрию квадрата для проверки пересечения
                                squareGeometry = squarePlacemark.geometry;

                                // Обработка нажатия на квадратный объект
                                squarePlacemark.events.add('click', function (e) {
                                    if (markerMode === 'to') {
                                        if (fromPlacemark) {
                                            var toCoords = centerCoords;
                                            buildRoute(toCoords, function() {
                                                // Оповещаем приложение о необходимости открыть новую страницу после построения маршрута
                                                window.location.href = 'app://openNewPage';
                                            });
                                        } else {
                                            alert('Сначала определите своё местоположение или выберите точку отправления (\""Откуда\"")');
                                        }
                                    }
                                });

                                // Обработка нажатия на карту
                                myMap.events.add('click', function (e) {
                                    var coords = e.get('coords');

                                    if (markerMode === 'from') {
                                        addFromPlacemark(coords);
                                    } else if (markerMode === 'to') {
                                        addToPlacemark(coords);
                                        buildRoute(coords);
                                    }
                                });
                            }

                            function addFromPlacemark(coords) {
                                if (fromPlacemark) {
                                    myMap.geoObjects.remove(fromPlacemark);
                                }
                                fromPlacemark = new ymaps.Placemark(coords, {
                                    hintContent: 'Начальная точка'
                                }, {
                                    preset: 'islands#greenDotIcon'
                                });
                                myMap.geoObjects.add(fromPlacemark);
                            }

                            function addToPlacemark(coords) {
                                if (toPlacemark) {
                                    myMap.geoObjects.remove(toPlacemark);
                                }
                                toPlacemark = new ymaps.Placemark(coords, {
                                    hintContent: 'Конечная точка'
                                }, {
                                    preset: 'islands#redDotIcon'
                                });
                                myMap.geoObjects.add(toPlacemark);
                            }

                            function buildRoute(toCoords, callback) {
                                if (fromPlacemark) {
                                    var fromCoords = fromPlacemark.geometry.getCoordinates();

                                    ymaps.route([fromCoords, toCoords], {
                                        routingMode: 'pedestrian'
                                    }).then(function (r) {
                                        if (route) {
                                            myMap.geoObjects.remove(route);
                                        }
                                        route = r;
                                        myMap.geoObjects.add(route);
                                        if (callback) callback();
                                    }, function (error) {
                                        alert('Не удалось построить маршрут: ' + error.message);
                                    });
                                } else {
                                    alert('Сначала определите своё местоположение или выберите точку отправления (\""Откуда\"")');
                                }
                            }

                            function clearMap() {
                                myMap.geoObjects.removeAll();
                                myMap.geoObjects.add(squarePlacemark);
                                fromPlacemark = null;
                                toPlacemark = null;
                                route = null;
                            }

                            function updateUserLocation(lat, lon) {
                                var coords = [lat, lon];

                                if (userLocationPlacemark) {
                                    userLocationPlacemark.geometry.setCoordinates(coords);
                                } else {
                                    userLocationPlacemark = new ymaps.Placemark(coords, {
                                        hintContent: 'Моё местоположение'
                                    }, {
                                        preset: 'islands#blueCircleIcon',
                                        iconColor: '#1E90FF'
                                    });
                                    myMap.geoObjects.add(userLocationPlacemark);
                                }

                                // Обновляем начальную точку
                                fromPlacemark = userLocationPlacemark;


                                // Проверяем, находится ли местоположение внутри квадрата
                                checkIfInsideSquare(coords);
                            }

                            function checkIfInsideSquare(coords) {
                                if (squareGeometry.contains(coords)) {
                                    // Оповещаем приложение о необходимости открыть новую страницу
                                    window.location.href = 'app://openNewPage';
}
                            }
                        </script>
                    </body>
                    </html>"
            };
            MapWebView.Source = htmlSource;

            // Обработка навигации внутри WebView
            MapWebView.Navigating += MapWebView_Navigating;
        }

        private async void LocateButton_Clicked(object sender, EventArgs e)
        {
            if (!isTracking)
            {
                isTracking = true;
                cts = new CancellationTokenSource();
                await StartTrackingAsync(cts.Token);

            }
            else
            {
                isTracking = false;
                cts.Cancel();
            }
        }
        private async Task StartTrackingAsync(CancellationToken token)
        {
            try
            {
                // Проверяем и запрашиваем разрешение на доступ к местоположению
                var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                if (status != PermissionStatus.Granted)
                {
                    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
                    if (status != PermissionStatus.Granted)
                    {
                        ToButton.IsEnabled = true;
                        await DisplayAlert("Ошибка", "Не предоставлены права на доступ к местоположению.", "OK");
                        return;

                    }
                }

                while (!token.IsCancellationRequested)
                {
                    var location = await Geolocation.Default.GetLocationAsync(new GeolocationRequest(GeolocationAccuracy.Best));
                    if (location != null)
                    {
                        string jsCode = $"updateUserLocation({location.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}, {location.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)});";
                        await MainThread.InvokeOnMainThreadAsync(() =>
                        {
                            MapWebView.EvaluateJavaScriptAsync(jsCode);
                        });
                    }
                    ToButton.IsEnabled = true;
                    await Task.Delay(1000, token); // Задержка между обновлениями (1 секунда)
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Ошибка", $"Не удалось получить местоположение: {ex.Message}", "OK");
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

        private void FromButton_Clicked(object sender, EventArgs e)
        {
            MapWebView.EvaluateJavaScriptAsync("markerMode = 'from';");
            ToButton.IsEnabled = true;
        }

        private void ToButton_Clicked(object sender, EventArgs e)
        {
            MapWebView.EvaluateJavaScriptAsync("markerMode = 'to';");
            otkuda.IsEnabled = false;
            timedlinbar.IsVisible = true;
        }

        private void ClearButton_Clicked(object sender, EventArgs e)
        {
            MapWebView.EvaluateJavaScriptAsync("clearMap();");
            otkuda.IsEnabled = true;
            ToButton.IsEnabled = false;
            timedlinbar.IsVisible = false;
            butmenubar.IsVisible = false;
            butmenubar.IsEnabled = false;
            menubar.IsVisible = false;
            bildcheck.IsVisible = false;
        }

        private void MapWebView_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url == "app://openNewPage")
            {
                e.Cancel = true;
                // Открываем новую страницу
                bildcheck.IsVisible = true;
                menubar.IsVisible = true;
                butmenubar.IsVisible = true;
                butmenubar.IsEnabled = true;

            }
            else
            {
                bildcheck.IsVisible = false;
                menubar.IsVisible = false;
                butmenubar.IsVisible = false;
                butmenubar.IsEnabled = false;
            }
        }

        private void butotkuda_Clicked(object sender, EventArgs e)
        {
            navbar.IsVisible = true;
            butnavbar.IsVisible = true;
            butnavbar.IsEnabled = true;
            otkudabord.StrokeThickness = 2;
            otkudabord.BackgroundColor = Color.FromHex("#27ab84");
        }

        private void CloseMenu(object sender, EventArgs e)
        {
            navbar.IsVisible = false;
            ToButton.IsEnabled = false;
            kudabord.BackgroundColor = Colors.Transparent;
            otkudabord.StrokeThickness = 0;
            butmenubar.IsVisible = false;
            butmenubar.IsEnabled = false;
            menubar.IsVisible = false;
        }

        private void indoors(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new NewPage2());
        }

        private void success(object sender, EventArgs e)
        {
            menubar.IsVisible = false;
            butotkuda.IsEnabled = true;
            otkudabord.BackgroundColor = Color.FromHex("#2ec095");
        }

        private void plant(object sender, EventArgs e)
        {
            menubar.IsVisible = false;
            Navigation.PushModalAsync(new NewPage1());

            butotkuda.IsEnabled = true;
            otkudabord.BackgroundColor = Color.FromHex("#2ec095");
        }

        private void CloseMenuBar(object sender, EventArgs e)
        {
            menubar.IsVisible = false;
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            timedlinbar.IsVisible = false;
        }

        private void butnavbar_Clicked(object sender, EventArgs e)
        {
            navbar.IsVisible = false;
            butnavbar.IsVisible = false;
            butnavbar.IsEnabled = false;
            otkudabord.StrokeThickness = 0;
            otkudabord.BackgroundColor = Color.FromHex("#2ec095");

        }
    }

}
