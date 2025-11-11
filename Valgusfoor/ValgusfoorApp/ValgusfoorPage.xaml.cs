using Microsoft.Maui.Controls;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ValgusfoorApp
{
    public partial class ValgusfoorPage : ContentPage
    {
        // Флаг, включён ли светофор
        bool isOn = false;

        // Активен ли дневной режим
        bool isDayMode = false;

        // Активен ли ночной режим
        bool isNightMode = false;

        // Токен для остановки циклов (дневной / ночной режим)
        CancellationTokenSource? cts;

        public ValgusfoorPage()
        {
            InitializeComponent();
        }

        // Сбрасываем все цвета на серый (светофор выключен)
        private void ResetLights()
        {
            redLight.BackgroundColor = Colors.Gray;
            yellowLight.BackgroundColor = Colors.Gray;
            greenLight.BackgroundColor = Colors.Gray;
            timerLabel.Text = "";
        }

        // Кнопка "Sisse" — включает светофор
        private void OnTurnOnClicked(object sender, EventArgs e)
        {
            isOn = true;
            isDayMode = false;
            isNightMode = false;
            cts?.Cancel(); // Останавливаем возможные активные циклы
            statusLabel.Text = "Foor on sisse lülitatud"; // Текст: светофор включен
            ResetLights();
        }

        // Кнопка "Välja" — выключает светофор
        private void OnTurnOffClicked(object sender, EventArgs e)
        {
            isOn = false;
            isDayMode = false;
            isNightMode = false;
            cts?.Cancel(); // Останавливаем все циклы
            statusLabel.Text = "Foor on välja lülitatud"; // Текст: светофор выключен
            ResetLights();
        }

        // Кнопка "Päevarežiim" — запускает дневной цикл (как в реальной жизни)
        private async void OnDayModeClicked(object sender, EventArgs e)
        {
            // Проверяем, включён ли светофор
            if (!isOn)
            {
                await DisplayAlert("Hoiatus", "Lülita esmalt foor sisse!", "OK"); // Сообщение: включите светофор
                return;
            }

            // Настройка состояний
            isDayMode = true;
            isNightMode = false;
            cts?.Cancel(); // Отменяем старый цикл, если был запущен
            cts = new CancellationTokenSource();
            var token = cts.Token;

            statusLabel.Text = "Päevarežiim aktiivne"; // Надпись о дневном режиме
            ResetLights();

            // Запускаем цикл в отдельном потоке (чтобы не зависал UI)
            _ = Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    // Красный — 3 секунды
                    await ShowLight("red", 3, token);
                    // Красный + Жёлтый — 1 секунда
                    await ShowLight("red_yellow", 1, token);
                    // Зелёный — 3 секунды
                    await ShowLight("green", 3, token);
                    // Жёлтый — 1 секунда
                    await ShowLight("yellow", 1, token);
                }
            });
        }

        // Кнопка "Öörežiim" — запускает ночной режим (только мигает жёлтый)
        private async void OnNightModeClicked(object sender, EventArgs e)
        {
            // Проверяем, включён ли светофор
            if (!isOn)
            {
                await DisplayAlert("Hoiatus", "Lülita esmalt foor sisse!", "OK");
                return;
            }

            // Настройка состояний
            isNightMode = true;
            isDayMode = false;
            cts?.Cancel();
            cts = new CancellationTokenSource();
            var token = cts.Token;

            statusLabel.Text = "Öörežiim "; // Надпись: ночной режим активен
            ResetLights();

            // Мигающий цикл
            _ = Task.Run(async () =>
            {
                bool on = false;
                while (!token.IsCancellationRequested)
                {
                    on = !on; // Переключаем состояние света
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        yellowLight.BackgroundColor = on ? Colors.Yellow : Colors.Gray;
                        redLight.BackgroundColor = Colors.Gray;
                        greenLight.BackgroundColor = Colors.Gray;
                        timerLabel.Text = on ? "Öörežiim" : "";
                    });
                    await Task.Delay(500, token); // Пауза между миганиями
                }
            });
        }

        // Универсальный метод для отображения выбранного света
        private async Task ShowLight(string mode, int seconds, CancellationToken token)
        {
            // Цикл, который обновляет таймер каждую секунду
            for (int i = seconds; i >= 0; i--)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // В зависимости от текущего состояния включаем нужные цвета
                    switch (mode)
                    {
                        case "red":
                            redLight.BackgroundColor = Colors.Red;
                            yellowLight.BackgroundColor = Colors.Gray;
                            greenLight.BackgroundColor = Colors.Gray;
                            timerLabel.Text = $"Seisa – {i}s"; // Таймер красного
                            break;

                        case "red_yellow":
                            redLight.BackgroundColor = Colors.Red;
                            yellowLight.BackgroundColor = Colors.Yellow;
                            greenLight.BackgroundColor = Colors.Gray;
                            timerLabel.Text = $"Ettevaatlikult – {i}s"; // Таймер для перехода
                            break;

                        case "yellow":
                            redLight.BackgroundColor = Colors.Gray;
                            yellowLight.BackgroundColor = Colors.Yellow;
                            greenLight.BackgroundColor = Colors.Gray;
                            timerLabel.Text = $"Ettevaatlikult – {i}s"; // Таймер жёлтого
                            break;

                        case "green":
                            redLight.BackgroundColor = Colors.Gray;
                            yellowLight.BackgroundColor = Colors.Gray;
                            greenLight.BackgroundColor = Colors.Green;
                            timerLabel.Text = $"Sõida/Mine – {i}s"; // Таймер зелёного
                            break;
                    }
                });

                await Task.Delay(1000, token); // Ждём 1 секунду, затем уменьшаем таймер
            }
        }
    }
}

