using Microsoft.Maker.Firmata;
using Microsoft.Maker.RemoteWiring;
using Microsoft.Maker.Serial;
using Microsoft.WindowsAzure.Storage.Auth;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Tewks.Simon.Domain;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Tewks.Simon
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page, IDevice
    {

        private static readonly StorageCredentials cred = new StorageCredentials("simonstorageaccount", "<access key>");
        public static readonly CloudBlobContainer container = new CloudBlobContainer(new Uri("http://simonstorageaccount.blob.core.windows.net/stats/"), cred);

        // pin settings
        private readonly byte GREEN_LED = 2;
        private readonly byte GREEN_BTN = 3;
        private readonly byte WHITE_LED = 4;
        private readonly byte WHITE_BTN = 5;
        private readonly byte YELLOW_LED = 6;
        private readonly byte YELLOW_BTN = 7;
        private readonly byte RED_LED = 8;
        private readonly byte RED_BTN = 9;
        private readonly byte GAME_LED = 10;
        private readonly byte GAME_BTN = 12;

        // sound files
        private readonly string RED_SOUND_FILE = "red.wav";
        private readonly string YELLOW_SOUND_FILE = "yellow.wav";
        private readonly string WHITE_SOUND_FILE = "white.wav";
        private readonly string GREEN_SOUND_FILE = "green.wav";
        private readonly string GAME_OVER_SOUND_FILE = "game-over.wav";

        private DeviceManager Manager { get; set; } = new DeviceManager();
        private volatile bool _Debounced = true;
        private UsbSerial _Serial;
        private RemoteDevice _Arduino;
        private DateTime _Clicked = DateTime.MinValue;
        private DateTime _LastClicked = DateTime.Now;
        private DateTime _LastUp = DateTime.Now;

        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
        }

        public void GameOver(ButtonColor color)
        {
            _Arduino.digitalWrite(RED_LED, PinState.LOW);
            _Arduino.digitalWrite(WHITE_LED, PinState.LOW);
            _Arduino.digitalWrite(GREEN_LED, PinState.LOW);
            _Arduino.digitalWrite(YELLOW_LED, PinState.LOW);
            PlaySoundFile(GAME_OVER_SOUND_FILE).ConfigureAwait(false);
            FlashAllColorButton().ConfigureAwait(false);

        }

        public void RoundStarted(ButtonColor[] colors)
        {
            Task.Delay(1000).Wait();

            foreach (var color in colors)
            {
                if (Manager.CurrentGame.GameOver)
                {
                    return;
                }

                if (color == ButtonColor.Green)
                {
                    HighlightButton(GREEN_LED, GREEN_SOUND_FILE, 500);
                }
                else if (color == ButtonColor.Red)
                {
                    HighlightButton(RED_LED, RED_SOUND_FILE, 500);
                }
                else if (color == ButtonColor.White)
                {
                    HighlightButton(WHITE_LED, WHITE_SOUND_FILE, 500);
                }
                else if (color == ButtonColor.Yellow)
                {
                    HighlightButton(YELLOW_LED, YELLOW_SOUND_FILE, 500);
                }
            }
        }

        /// <summary>
        /// Called when the application loads.  Responsible for initializng the
        /// serial connection and configuration of the arduino device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            // testing

            //var blob = container.GetBlockBlobReference("stats2.json");

            //var text = string.Empty;
            //if (await blob.ExistsAsync())
            //{
            //    text = await blob.DownloadTextAsync();
            //}
            //else
            //{

            //}


            // create a serial connection to the arduino board
            _Serial = new UsbSerial(await FindFirstArduinoBoard());

            _Serial.ConnectionEstablished += () =>
            {
                Debug.WriteLine("Serial connection established");
            };

            _Serial.ConnectionFailed += (m) =>
            {
                Debug.WriteLine("Serial connection failed: " + m);
            };

            _Arduino = new RemoteDevice(_Serial);

            // configure the button inputs and led outputs
            _Arduino.DeviceReady += async () =>
            {
                Debug.WriteLine("Device ready");
                _Arduino.pinMode(GREEN_LED, PinMode.OUTPUT);
                _Arduino.pinMode(GREEN_BTN, PinMode.PULLUP);
                _Arduino.pinMode(WHITE_LED, PinMode.OUTPUT);
                _Arduino.pinMode(WHITE_BTN, PinMode.PULLUP);
                _Arduino.pinMode(YELLOW_LED, PinMode.OUTPUT);
                _Arduino.pinMode(YELLOW_BTN, PinMode.PULLUP);
                _Arduino.pinMode(RED_LED, PinMode.OUTPUT);
                _Arduino.pinMode(RED_BTN, PinMode.PULLUP);
                _Arduino.pinMode(GAME_LED, PinMode.OUTPUT);
                _Arduino.pinMode(GAME_BTN, PinMode.PULLUP);

                _Arduino.digitalWrite(RED_LED, PinState.LOW);
                _Arduino.digitalWrite(WHITE_LED, PinState.LOW);
                _Arduino.digitalWrite(GREEN_LED, PinState.LOW);
                _Arduino.digitalWrite(YELLOW_LED, PinState.LOW);

                Manager.Run(this);

                // show as ready
                await FlashAllColorButton();
            };

            _Serial.begin(57600);

            _Arduino.DigitalPinUpdated += _Arduino_DigitalPinUpdated;
        }

        /// <summary>
        /// Ensures that all led's are off when the application shuts down.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            if (_Arduino != null)
            {
                _Arduino.digitalWrite(GREEN_LED, PinState.LOW);
                _Arduino.digitalWrite(YELLOW_LED, PinState.LOW);
                _Arduino.digitalWrite(WHITE_LED, PinState.LOW);
                _Arduino.digitalWrite(RED_LED, PinState.LOW);
            }
        }

        /// <summary>
        /// Used to filter out switch bounce noise from a button press
        /// </summary>
        /// <returns></returns>
        private async Task Debounce()
        {
            _Debounced = false;
            await Task.Delay(50);
            _Debounced = true;
        }

        /// <summary>
        /// Handles the button press events and is responsible
        /// for integrating the physical button presses
        /// with the simon game logic
        /// </summary>
        /// <param name="pin"></param>
        /// <param name="state"></param>
        private async void _Arduino_DigitalPinUpdated(byte pin, PinState state)
        {
            if (!_Debounced)
            {
                // this means that the button click is probably
                // switch noise, therefore, we don't process the click
                return;
            }

            // put in a delay so that we can process out potential
            // switch noise on future events
            Debounce();

            // start a new game
            if (pin == GAME_BTN && state == PinState.LOW)
            {
                Debug.WriteLine("New Game Clicked");
                await Manager.StartGame();
            }

            if (state == PinState.HIGH)
            {
                _Arduino.digitalWrite(GREEN_LED, PinState.LOW);
                _Arduino.digitalWrite(WHITE_LED, PinState.LOW);
                _Arduino.digitalWrite(YELLOW_LED, PinState.LOW);
                _Arduino.digitalWrite(RED_LED, PinState.LOW);
                _LastUp = DateTime.Now;
            }

            // don't allow the player to click buttons if the
            // device isn't ready
            if (!Manager.Ready || Manager.CurrentGame?.GameOver == true)
            {
                return;
            }

            // green button pushed
            if (pin == GREEN_BTN && state == PinState.LOW)
            {
                await ColorClickButton(GREEN_SOUND_FILE, GREEN_LED, ButtonColor.Green);
            }
            // green button released
            else if (pin == GREEN_BTN && state == PinState.HIGH)
            {
                _Arduino.digitalWrite(GREEN_LED, PinState.LOW);
                _LastUp = DateTime.Now;
            }

            
            // white button pushed
            else if (pin == WHITE_BTN && state == PinState.LOW)
            {
                await ColorClickButton(WHITE_SOUND_FILE, WHITE_LED, ButtonColor.White);
            }
            // white button released
            else if (pin == WHITE_BTN && state == PinState.HIGH)
            {
                _Arduino.digitalWrite(WHITE_LED, PinState.LOW);
                _LastUp = DateTime.Now;
            }


            // yellow button pushed 
            else if (pin == YELLOW_BTN && state == PinState.LOW)
            {
                await ColorClickButton(YELLOW_SOUND_FILE, YELLOW_LED, ButtonColor.Yellow);
            }
            // yellow button released
            else if (pin == YELLOW_BTN && state == PinState.HIGH)
            {
                _Arduino.digitalWrite(YELLOW_LED, PinState.LOW);
                _LastUp = DateTime.Now;
            }


            // red button pushed
            else if (pin == RED_BTN && state == PinState.LOW)
            {
                await ColorClickButton(RED_SOUND_FILE, RED_LED, ButtonColor.Red);
            }
            // red button released
            else if (pin == RED_BTN && state == PinState.HIGH)
            {
                _Arduino.digitalWrite(RED_LED, PinState.LOW);
                _LastUp = DateTime.Now;
            }
        }

        /// <summary>
        /// Retrieves a usb serial device associated with the arduino.
        /// </summary>
        /// <returns></returns>
        private async Task<DeviceInformation> FindFirstArduinoBoard()
        {
            var usbDevices = await UsbSerial.listAvailableDevicesAsync();
            return usbDevices.FirstOrDefault(d => d.Name.ToUpper().Contains("ARDUINO"));
        }

        /// <summary>
        /// Called when a color button is clicked.
        /// </summary>
        /// <param name="soundFile">name of the sound file to play</param>
        /// <param name="ledPin">pin number associated with the button</param>
        /// <param name="color">the color associated with the button click</param>
        /// <returns></returns>
        private async Task ColorClickButton(string soundFile, byte ledPin, ButtonColor color)
        {
            Debug.WriteLine($"{color.ToString()} clicked");
            _Arduino.digitalWrite(ledPin, PinState.HIGH);
            await PlaySoundFile(soundFile);
            await Manager.SelectColor(color);
        }

        /// <summary>
        /// Provides a means of playing a sound file associated with 
        /// a particular button
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private async Task PlaySoundFile(string fileName)
        {
                var player = BackgroundMediaPlayer.Current;
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri($"ms-appx:///Assets/{fileName}"));
                player.AutoPlay = false;
                player.SetFileSource(file);
                player.Play();
        }


        private async Task FlashAllColorButton()
        {
            _Arduino.digitalWrite(GREEN_LED, PinState.HIGH);
            _Arduino.digitalWrite(YELLOW_LED, PinState.HIGH);
            _Arduino.digitalWrite(WHITE_LED, PinState.HIGH);
            _Arduino.digitalWrite(RED_LED, PinState.HIGH);
            await Task.Delay(500);
            _Arduino.digitalWrite(GREEN_LED, PinState.LOW);
            _Arduino.digitalWrite(YELLOW_LED, PinState.LOW);
            _Arduino.digitalWrite(WHITE_LED, PinState.LOW);
            _Arduino.digitalWrite(RED_LED, PinState.LOW);
        }

        private async Task DisplayAsReady()
        {
            HighlightAllButtons(PinState.HIGH);
            await Task.Delay(500);
            HighlightAllButtons(PinState.LOW);
            await Task.Delay(500);
            HighlightAllButtons(PinState.HIGH);
            await Task.Delay(500);
            HighlightAllButtons(PinState.LOW);
        }

        private void HighlightButton(byte led, string soundFile, int delay)
        {
            _Arduino.digitalWrite(led, PinState.LOW);
            Task.Delay(delay).Wait();
            _Arduino.digitalWrite(led, PinState.HIGH);
            PlaySoundFile(soundFile).ConfigureAwait(false);
            Task.Delay(delay).Wait();
            _Arduino.digitalWrite(led, PinState.LOW);
        }

        private void HighlightButtonForSong(byte led, string soundFile, int delay)
        {
            _Arduino.digitalWrite(led, PinState.HIGH);
            PlaySoundFile(soundFile).ConfigureAwait(false);
            Task.Delay(delay).Wait();
            _Arduino.digitalWrite(led, PinState.LOW);
        }

        private void PlayMaryHadALittleLamb()
        {
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(WHITE_LED, WHITE_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 1000);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 1000);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 1000);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(WHITE_LED, WHITE_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(RED_LED, RED_SOUND_FILE, 500);
            HighlightButtonForSong(YELLOW_LED, YELLOW_SOUND_FILE, 500);
            HighlightButtonForSong(WHITE_LED, WHITE_SOUND_FILE, 500);
        }

        private void HighlightAllButtons(PinState state)
        {
            _Arduino.digitalWrite(GREEN_LED, state);
            _Arduino.digitalWrite(YELLOW_LED, state);
            _Arduino.digitalWrite(WHITE_LED, state);
            _Arduino.digitalWrite(RED_LED, state);
        }
    }
}
