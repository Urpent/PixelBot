using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Speech.Synthesis;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using WindowsInput;
using AI_Conquer.BotLibrary;
using AI_Conquer.BotLibrary.GlobalControls;
using AI_Conquer.Properties;

namespace AI_Conquer
{
    public partial class Form1 : Form
    {
        
        public static readonly Size ImageSize = new Size(1,1);
        readonly SpeechSynthesizer _speaker = new SpeechSynthesizer();
        private LookMemory _lookMemory;
        readonly KeyboardHook _keyboardHook = new KeyboardHook();
        private int _nextLocationIndex;
        private Point _moveToPosition;
        private Point _inGamePreviousPoint;
        private uint _counterStuckTime;
        
        

        readonly Dictionary<RadioButton, PictureBox> _dictRadioBtnToPicBox = new Dictionary<RadioButton, PictureBox>();
        readonly Dictionary<RadioButton, Point> _dictRadioBtnToPoint = new Dictionary<RadioButton, Point>();

        private readonly List<Point> _listBotLocation = new List<Point>();


        public Form1()
        {
            InitializeComponent();
            ExternalData.LoadSavedFile();  //Must Be first to Call
            DictionaryAddControls(); //RadioButton & PictureBox name have to follow the Rule.      
            _keyboardHook.KeyUp += Global_KeyUp;
            _keyboardHook.Start();                    
        }

        private void Global_KeyUp(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.F5:

                    break;
                case Keys.F6:
                    //KeyBoardSimulator.KeyUp(KeyFlags.Control);
                    InputSimulator.SimulateKeyUp(VirtualKeyCode.LCONTROL);
                    break;
                case Keys.F7:
                    tbDisplayMousePosition.Text = Cursor.Position.ToString();
                    break;

                case Keys.F8:
                    CaptureAndStorePicture();
                    break;
                case Keys.F9:
                    //InputSimulator.SimulateKeyDown(VirtualKeyCode.CONTROL);
                    //KeyBoardSimulator.KeyDown(KeyFlags.Control);
                    //KeyBoardSimulator.KeyUp(KeyFlags.Control);

                    break;
                case Keys.F10:
                    cbStart.Checked = !cbStart.Checked;
                    break;
            }
        }

        #region Form Event
        private void Form1_Load(object sender, EventArgs e)
        {
            LoadSavedPictures();
            LoadSetting();
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            ExternalData.BinaryData.ListRbName.Clear();
            ExternalData.BinaryData.ListRbPoint.Clear();

            foreach (KeyValuePair<RadioButton, Point> pair in _dictRadioBtnToPoint)
            {
                string[] namePart = pair.Key.Name.Split('_');
                ExternalData.BinaryData.ListRbName.Add(namePart[1]);
                ExternalData.BinaryData.ListRbPoint.Add(pair.Value);
            }
            ExternalData.SaveData();
        }
        private void cbStart_CheckedChanged(object sender, EventArgs e)
        {
            if (cbStart.Checked && tbXAddress.Text != "" && tbYAddress.Text != "")
            {
                SaveSetting();
                TuneOffset();

                timerUpdateMemory.Start();
            }
            else
                timerUpdateMemory.Stop();
        }

        private void TuneOffset()
        {
            var rect = new MouseSimulator.RECT();
            MouseSimulator.GetWindowRectangle(ref rect);

            LookDirection.CentrePosition = new Point((rect.Right - rect.Left)/2, (rect.Bottom - rect.Top)/2);
            LookDirection.MaxTop = Settings.Default.MaxTop - rect.Top;
            LookDirection.MaxBottom = Settings.Default.MaxBottom - rect.Top; //Correct, need to minus rect top offset
            LookDirection.MaxLeft = Settings.Default.MaxLeft - rect.Left;
            LookDirection.MaxRight = Settings.Default.MaxRight - rect.Left; //Correct

            LookPixel.GameScreenSize = new Size(rect.Right - rect.Left, rect.Bottom -rect.Top);
            LookPixel.GameScreenPosition = new Point(rect.Left,rect.Top);
        }
        #endregion

        private void LoadSetting()
        {
            tbXAddress.Text = Settings.Default.AddressX;
            tbYAddress.Text = Settings.Default.AddressY;
            tbBotSpeed.Text = Settings.Default.BotSpeed.ToString(CultureInfo.InvariantCulture);


            tbCentrePoint.Text = Settings.Default.CentrePoint.X + "," + Settings.Default.CentrePoint.Y;
            tbMaxLeft.Text     = Settings.Default.MaxLeft.ToString();
            tbMaxRight.Text    = Settings.Default.MaxRight.ToString();
            tbMaxTop.Text      = Settings.Default.MaxTop.ToString();
            tbMaxBottom.Text   = Settings.Default.MaxBottom.ToString();
        }
        private void SaveSetting()
        {
            var temp = tbCentrePoint.Text.Split(',');
            LookDirection.CentrePosition = Settings.Default.CentrePoint = new Point(int.Parse(temp[0]),int.Parse(temp[1]));
            LookDirection.MaxLeft = Settings.Default.MaxLeft = int.Parse(tbMaxLeft.Text);
            LookDirection.MaxRight = Settings.Default.MaxRight = int.Parse(tbMaxRight.Text);
            LookDirection.MaxTop = Settings.Default.MaxTop = int.Parse(tbMaxTop.Text);
            LookDirection.MaxBottom = Settings.Default.MaxBottom = int.Parse(tbMaxBottom.Text);

            int speed;
            if (int.TryParse(tbBotSpeed.Text, out speed))
                Settings.Default.BotSpeed = timerUpdateMemory.Interval = speed < 1000 ? 
                                                                        1000 : speed;

            Settings.Default.Save();        
        }


        private void DictionaryAddControls()
        {
            var mapRbStringToPoint = new Dictionary<string, Point>();
            var mapStrToRadioBtn = new Dictionary<string, RadioButton>();
            var mapStrToPicBox = new Dictionary<string, PictureBox>();

            var listRadioBtnName = new List<string>();

            foreach (var control in from TabPage tabPageOuter in tabControlOuter.Controls 
                                    where tabPageOuter.Name.Contains("S_") 
                                    from Control control in tabPageOuter.Controls 
                                    select control)
            {
                if (control.Name.Contains("S_"))
                {
                    if (control is RadioButton)
                    {
                        //Eg. rB_Home spilt into first & second element, "rB" & "Home" 
                        string[] nameInParts = control.Name.Split(new[] {"S_"}, StringSplitOptions.None);
                        mapStrToRadioBtn[nameInParts[1]] = control as RadioButton;
                        listRadioBtnName.Add(nameInParts[1]); //Get a list of radiobutton name
                    }
                    else if (control is PictureBox)
                    {
                        string[] nameInParts = control.Name.Split(new[] {"S_"}, StringSplitOptions.None);
                        mapStrToPicBox[nameInParts[1]] = control as PictureBox;
                    }
                }
                else if (control is TabControl)
                {
                    //Loop Inner TabControl if it exist
                    foreach (var control2 in control.Controls.Cast<TabPage>().SelectMany(
                        innerTab => innerTab.Controls.Cast<Control>())
                        .Where(control2 => control2.Name.Contains("S_")))
                    {
                        if (control2 is RadioButton)
                        {
                            string[] nameInParts = control2.Name.Split(new[] {"S_"}, StringSplitOptions.None);
                            //Eg. rB_Home spilt into first & second element, "rB" & "Home" 
                            mapStrToRadioBtn[nameInParts[1]] = control2 as RadioButton;
                            listRadioBtnName.Add(nameInParts[1]);
                        }
                        else if (control2 is PictureBox)
                        {
                            string[] nameInParts = control2.Name.Split(new[] {"S_"}, StringSplitOptions.None);
                            mapStrToPicBox[nameInParts[1]] = control2 as PictureBox;
                        }
                    }
                }
            }
            if (ExternalData.BinaryData == null)
                ExternalData.BinaryData = new ExternalData.SavedData();

            if (ExternalData.BinaryData.ListRbName == null || ExternalData.BinaryData.ListRbName.Count != listRadioBtnName.Count)
            {
                ExternalData.BinaryData.ListRbName = new List<string>();
                ExternalData.BinaryData.ListRbPoint = new List<Point>();
            }
            if (ExternalData.BinaryData.ListRbName.Count == listRadioBtnName.Count)
            {
                for (int index = 0; index < ExternalData.BinaryData.ListRbName.Count; index++)
                {   //SaveData contain the Point of rb
                    mapRbStringToPoint.Add(ExternalData.BinaryData.ListRbName[index], ExternalData.BinaryData.ListRbPoint[index]);
                }
            }
            else
                foreach (string name in listRadioBtnName)
                    mapRbStringToPoint.Add(name, Point.Empty);

            foreach (string strName in listRadioBtnName)
            {
                _dictRadioBtnToPicBox.Add(mapStrToRadioBtn[strName], mapStrToPicBox[strName]);
                _dictRadioBtnToPoint.Add(mapStrToRadioBtn[strName], mapRbStringToPoint[strName]);
            }
        }
        private void LoadSavedPictures()
        {
            string path = Directory.GetCurrentDirectory() + "\\CO_PicturesFolder";
            string path2 = path + "\\CO_ErrorPictures";

               
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Directory.CreateDirectory(path2);
                MessageBox.Show(@"Missing CO_PicturesFolder was created");
                return;
            }

            if (Directory.GetFiles(path).Length == 0) //Return if folder exist but empty
                return;

            foreach (var dict in _dictRadioBtnToPicBox)
            {
                if (!File.Exists(path + "\\" + dict.Key.Text + ".png")) continue;
                var bmpFromFile = new Bitmap(path + "\\" + dict.Key.Text + ".png");
                dict.Value.Image = bmpFromFile; //Dictionary value is picturebox
            }
        }

        private async void timerUpdateMemory_Tick(object sender, EventArgs e)
        {
            timerUpdateMemory.Enabled = false;
            Point inGamePositionCurrentPoint = _lookMemory.GetCurrentCoordinate();
            if (inGamePositionCurrentPoint.Equals(_inGamePreviousPoint))
                ++_counterStuckTime;
            if (++_counterStuckTime > 4)
            {
                MouseSimulator.LeftClick(_moveToPosition);
                //TODO: Normal Left Click Needed Here instead of jump
                _counterStuckTime = 0;
            }
            

            tbDisplayX.Text = inGamePositionCurrentPoint.X.ToString(CultureInfo.InvariantCulture);
            tbDisplayY.Text = inGamePositionCurrentPoint.Y.ToString(CultureInfo.InvariantCulture);

           

            if (_listBotLocation[_nextLocationIndex].X - 10 < inGamePositionCurrentPoint.X &&
                inGamePositionCurrentPoint.X < _listBotLocation[_nextLocationIndex].X + 10 &&
                _listBotLocation[_nextLocationIndex].Y - 10 < inGamePositionCurrentPoint.Y &&
                inGamePositionCurrentPoint.Y < _listBotLocation[_nextLocationIndex].Y + 10)
            {
                ++_nextLocationIndex;
                if (_nextLocationIndex >= lbBotLocation.Items.Count)
                    _nextLocationIndex = 0;
            }          
            //TODO:Change constant move distance to others :)
            _moveToPosition = LookDirection.GetDirection(inGamePositionCurrentPoint, _listBotLocation[_nextLocationIndex], 170);
            

            var intRandom = new Random(673579);
            Point monsterPositionPoint;
            Point itemPositionPoint;
            if (IsImageFound(out monsterPositionPoint, out itemPositionPoint))
            {
                if (itemPositionPoint != Point.Empty)
                {
                    MouseSimulator.LeftClick(itemPositionPoint); //inGame position screen
                    await Task.Delay(300);
                }
                    
                if (monsterPositionPoint != Point.Empty)
                {
                    MouseSimulator.RightClick(monsterPositionPoint);
                    await Task.Delay(intRandom.Next(900, 1200)); //Thread.Sleep will block GUI
                    MouseSimulator.RightClick(_moveToPosition);
                    //Cursor.Position = monsterPositionPoint; //Computer screen position instead of inGame position
                }
            }

           // KeyBoardSimulator.KeyDown(KeyFlags.Control); //*****************
            InputSimulator.SimulateKeyDown(VirtualKeyCode.LCONTROL);
            await Task.Delay(intRandom.Next(200,500));
            
            MouseSimulator.Jump(_moveToPosition);
            //TODO: Work on control and background picture scan
            //TODO: Background control not working, find way to auto set windows focus
            InputSimulator.SimulateKeyUp(VirtualKeyCode.LCONTROL);
          
            _inGamePreviousPoint = inGamePositionCurrentPoint;
            timerUpdateMemory.Enabled = true;
        }

        private bool IsImageFound(out Point monsterPositionPoint, out Point itemPositionPoint)
        {
            bool isMonsterFound, isItemFound;


            using(Bitmap gameScreenBitmap = LookPixel.Screenshot_GameScreen())
            {
                isMonsterFound = LookPixel.IsBitmapFoundFastParallel_Int32((Bitmap)_dictRadioBtnToPicBox[rbS_MobHpRed].Image,
               gameScreenBitmap, out monsterPositionPoint);

                if (isMonsterFound)
                    monsterPositionPoint = new Point(monsterPositionPoint.X+ LookPixel.GameScreenPosition.X,
                    monsterPositionPoint.Y+ LookPixel.GameScreenPosition.Y);

                Bitmap[] temp = new[] {(Bitmap) pbS_Meteor.Image, (Bitmap) pbS_Sdg.Image, (Bitmap) pbS_Others.Image};

                isItemFound = LookPixel.CompareBitmapPixelsFast(temp,gameScreenBitmap, out itemPositionPoint);

            }
            return isMonsterFound | isItemFound;
        }


        #region Controls
        private void bAttach_Click(object sender, EventArgs e)
        {

            _lookMemory = new LookMemory("conquer", tbXAddress.Text, tbYAddress.Text);
            Settings.Default.AddressX = tbXAddress.Text;
            Settings.Default.AddressY = tbYAddress.Text;
            Settings.Default.Save();
        }
        private void bSetLocation_Click(object sender, EventArgs e)
        {
            Point location = _lookMemory.GetCurrentCoordinate();
            lbBotLocation.Items.Add(location);
            _listBotLocation.Add(location);
        }
        private void bListClear_Click(object sender, EventArgs e)
        {
            lbBotLocation.Items.Clear();
            _listBotLocation.Clear();
        }
        private void bCheckImage_Click(object sender, EventArgs e)
        {
            RadioButton radioButtonChecked;
            if (!GetCheckedRadioButton(GetSelectedTabPage(), out radioButtonChecked))
                return; //Return if no RadioButtonChecked at selected Tab

            if (_dictRadioBtnToPicBox[radioButtonChecked].Image == null)
                return; //Return if no picture

            Point location;
            if (LookPixel.IsBitmapFound_PreciseLocation(_dictRadioBtnToPicBox[radioButtonChecked].Image as Bitmap
                , LookPixel.CapturePicture(_dictRadioBtnToPoint[radioButtonChecked])))
            {
                _speaker.SpeakAsync("Correct");
                Cursor.Position = _dictRadioBtnToPoint[radioButtonChecked];
            }
            else if (LookPixel.IsBitmapFoundFastParallel_Int32(_dictRadioBtnToPicBox[radioButtonChecked].Image as Bitmap,
                LookPixel.Screenshot_FullScreen(), out location))
            {
                Cursor.Position = location;

                _dictRadioBtnToPoint[radioButtonChecked] = location;
                _speaker.SpeakAsync("Auto Corrected");
            }
            else
                _speaker.SpeakAsync("Cannot Detect Picture");
        }
        #endregion

        #region Capture and save small picture in picturebox
        private void CaptureAndStorePicture()
        {
            RadioButton radioButtonChecked;
            if (!GetCheckedRadioButton(GetSelectedTabPage(), out radioButtonChecked))
                return; //Return if no RadioButtonChecked at selected Tab

            string path = Directory.GetCurrentDirectory() + "\\CO_PicturesFolder";

            Point captureLocation = Cursor.Position;
            Bitmap bmpCatured = LookPixel.CapturePicture(ImageSize, captureLocation);

            _dictRadioBtnToPoint[radioButtonChecked] = captureLocation;//Store Point in picturebox tag


            if (File.Exists(path + "\\" + radioButtonChecked.Text + ".png"))
            {
                if (_dictRadioBtnToPicBox[radioButtonChecked].Image != null)
                    _dictRadioBtnToPicBox[radioButtonChecked].Image.Dispose();
            }

            bmpCatured.Save(path + "\\" + radioButtonChecked.Text + ".png", System.Drawing.Imaging.ImageFormat.Png);
            _dictRadioBtnToPicBox[radioButtonChecked].Image = bmpCatured;
        }
        private static bool GetCheckedRadioButton(TabPage selectedTabPage, out RadioButton radioButtonChecked)
        {
            foreach (RadioButton radiobtn in selectedTabPage.Controls.OfType<RadioButton>().Select(c => c as RadioButton).Where(radiobtn => radiobtn.Checked))
            {
                radioButtonChecked = radiobtn;
                return true;
            }
            radioButtonChecked = null;
            return false;
        }
        private TabPage GetSelectedTabPage()
        {
            foreach (Control control in tabControlOuter.SelectedTab.Controls)
            {
                if (control is TabControl)
                {
                    var test = control as TabControl;
                    return test.SelectedTab;
                }
                if (control is RadioButton)
                {
                    goto found;
                }
            }
        found:
            return tabControlOuter.SelectedTab;
        }
        #endregion












    }
}
