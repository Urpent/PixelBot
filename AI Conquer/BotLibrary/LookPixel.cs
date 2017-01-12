using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

using System.Threading.Tasks;
using System.Windows.Forms;

namespace AI_Conquer.BotLibrary
{
    class LookPixel
    {
        private const PixelFormat PixelFormat = System.Drawing.Imaging.PixelFormat.Format32bppArgb;
        public static Size GameScreenSize;
        public static Point GameScreenPosition;



        public static Bitmap CapturePicture(Point captureLocation)
        {
            Bitmap bmpScreenshot = new Bitmap(Form1.ImageSize.Width, Form1.ImageSize.Height, PixelFormat); //Create a 24bit container
            Graphics g = Graphics.FromImage(bmpScreenshot);

            //TODO:_imageSize from Form1 is ugly
            g.CopyFromScreen(captureLocation.X, captureLocation.Y, 0, 0, new Size(Form1.ImageSize.Width, Form1.ImageSize.Height));
            g.Dispose();

            return bmpScreenshot;
        }
        /// <summary>
        /// Capture a Picture
        /// </summary>
        /// <param name="pictureSize">Size of Picture Capturing</param>
        /// <param name="captureLocation">Capture location on screen</param>
        /// <returns>Return bitmap</returns>
        public static Bitmap CapturePicture(Size pictureSize, Point captureLocation)
        {
            var bmpScreenshot = new Bitmap(pictureSize.Width, pictureSize.Height, PixelFormat); 
            Graphics g = Graphics.FromImage(bmpScreenshot);
            Cursor.Position = new Point(0, 0);
            g.CopyFromScreen(captureLocation.X, captureLocation.Y, 0, 0, pictureSize); //printscreen
            Cursor.Position = captureLocation;
            g.Dispose();
            return bmpScreenshot; 
        }

        public static Bitmap ResizeBitmap(Bitmap bitmap, Size pictureSize)
        {
            Bitmap result = new Bitmap(pictureSize.Width, pictureSize.Height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = InterpolationMode.NearestNeighbor;
                g.DrawImage(bitmap, 0, 0, pictureSize.Width, pictureSize.Height);
            }
            return result;
        }

        unsafe public static bool IsBitmapFoundFastParallel_Int32(Bitmap bmpNeedle, Bitmap bmpHaystack, out Point location)
        {   //Fastest Completed Code
            var equal = false;

            var hayStackWidth = bmpHaystack.Width;
            var hayStackHeight = bmpHaystack.Height;
            var needleWidth = bmpNeedle.Width;
            var needleHeight = bmpNeedle.Height;

            var rectSmall = new Rectangle(0, 0, needleWidth, needleHeight);
            var rectBig = new Rectangle(0, 0, hayStackWidth, hayStackHeight);
            BitmapData bmpDataNeedle = bmpNeedle.LockBits(rectSmall, ImageLockMode.ReadOnly, bmpNeedle.PixelFormat);
            BitmapData bmpDataHaystack = bmpHaystack.LockBits(rectBig, ImageLockMode.ReadOnly, bmpHaystack.PixelFormat);

            Point l = Point.Empty;

            int outerY = hayStackHeight - needleHeight; //The last few pixel of haystack no need to scan, because it is smaller than the needle picture
            int outerX = hayStackWidth - needleWidth;

            var nextRow = hayStackWidth - needleWidth;

            var pHaystackStart = (Int32*)bmpDataHaystack.Scan0; //Start Address
            var pNeedleStart = (Int32*)bmpDataNeedle.Scan0;

            Parallel.For(0, outerY, (y, loop) =>
            {
                var outerRow = pHaystackStart + y * hayStackWidth; //Current Row of Haystack

                for (int x = 0; x <= outerX; x++)
                {
                    var pHaystackCurrent = outerRow + x; //Current Position of Haystack = Current row + Column
                    var pNeedleCurrent = pNeedleStart;

                    for (int inY = 0; inY < needleHeight; inY++)
                    {
                        for (int inX = 0; inX < needleWidth; inX++)
                        {

                            if ((*pNeedleCurrent) >> 24 != 0)
                                if (*(pHaystackCurrent) != *(pNeedleCurrent))
                                    goto NotFound;
                            ++pHaystackCurrent; //Post Fix increment for Next address 
                            ++pNeedleCurrent;
                        }
                        pHaystackCurrent += nextRow; //If first row all true, then go to next haystack row
                    }
                    l = new Point(x, y);
                    equal = true;
                    loop.Stop();
                NotFound: ;
                }
            });

            location = l;
            bmpHaystack.UnlockBits(bmpDataHaystack);
            bmpNeedle.UnlockBits(bmpDataNeedle);
            
            return equal;
        }
        
        unsafe public static bool CompareBitmapPixelsFast(Bitmap[] bmpNeedles, Bitmap bmpHaystack, out Point location) 
        {
            bool equals = false;
            location = Point.Empty;
            Rectangle rectHayStack = new Rectangle(0, 0,bmpHaystack.Width, bmpHaystack.Height);
            Rectangle rectNeedle =  new Rectangle(0, 0,1, 1);
            BitmapData[] bmpDataNeedles = new BitmapData[bmpNeedles.Length];
            Int32*[] needlePtrs = new Int32*[bmpNeedles.Length];
            for(int i = 0 ; i < bmpNeedles.Length; i++)
            {
                 bmpDataNeedles[i] = bmpNeedles[i].LockBits(rectNeedle, ImageLockMode.ReadOnly, PixelFormat);
                 needlePtrs[i] = (Int32*)bmpDataNeedles[i].Scan0;
            }

            BitmapData bmpDataHayStack = bmpHaystack.LockBits(rectHayStack, ImageLockMode.ReadOnly,PixelFormat);
            
;
            Int32* haystackPtr = (Int32*)bmpDataHayStack.Scan0;
           
            for (var y = 0;  y < bmpHaystack.Height; y++)
            {
                for (var x = 0; x < bmpDataHayStack.Width; x++)
                {
                    for (var pixelCount = 0; pixelCount < bmpNeedles.Length; pixelCount++)
                    {
                        if (*haystackPtr == *needlePtrs[pixelCount])
                        {
                            location = new Point(x,y);
                            equals = true;
                            goto Found;
                        }
                    }
                    haystackPtr++;
                }                                                                                      
            }
            Found:

            for(var i = 0 ; i < bmpNeedles.Length; i++)
                 bmpNeedles[i].UnlockBits(bmpDataNeedles[i]);

            bmpHaystack.UnlockBits(bmpDataHayStack);
            
            return equals;
        }
        
        
        public static bool IsBitmapFound_PreciseLocation(Bitmap bmpNeedle, Bitmap bmpNeedle2)
        {
            for (int y = 0; y < bmpNeedle.Height; y++)
            {
                for (int x = 0; x < bmpNeedle.Width; x++)
                {
                    Color cNeedle = bmpNeedle.GetPixel(x, y);
                    Color cNeedle2 = bmpNeedle2.GetPixel(x, y);

                    if (cNeedle.R != cNeedle2.R || cNeedle.G != cNeedle2.G || cNeedle.B != cNeedle2.B)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public static Bitmap Screenshot_FullScreen()
        {
            var bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width,
                Screen.PrimaryScreen.Bounds.Height, PixelFormat);
            Graphics g = Graphics.FromImage(bmpScreenshot); //use using instead
            g.CopyFromScreen(0, 0, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            g.Dispose();
            return bmpScreenshot;
        }
        public static Bitmap Screenshot_GameScreen()
        {

            var bmpScreenshot = new Bitmap(GameScreenSize.Width,
                GameScreenSize.Height, PixelFormat);
            using (Graphics g = Graphics.FromImage(bmpScreenshot))
            {
                g.CopyFromScreen(GameScreenPosition.X, GameScreenPosition.Y, 0, 0, Screen.PrimaryScreen.Bounds.Size);
            }
            return bmpScreenshot;
        }
    }
}
