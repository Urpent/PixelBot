using System;
using System.Drawing;

namespace AI_Conquer.BotLibrary
{
    class LookDirection
    {
        public static Point CentrePosition;
        public static int MaxLeft;
        public static int MaxRight;
        public static int MaxTop;
        public static int MaxBottom;

        const double RotateTheta = Math.PI / 4;
        
        public static Point GetDirection(Point current, Point destination, int moveDistance)
        {
            double theta = GetAngle(current,  destination);
            //theta = (2 * Math.PI - theta) % (2 * Math.PI);
            //double angle = (2 * Math.PI - theta) % (2 * Math.PI); //For angle
            double x = (172* Math.Cos(theta));
            double y = (172* Math.Sin(theta));

            double rotatedX = x*Math.Cos(RotateTheta) - y*Math.Sin(RotateTheta);
            double rotatedY = x*Math.Cos(RotateTheta) + y*Math.Sin(RotateTheta);

            double maxX = rotatedX ;
            double maxY = rotatedY ;

            double rotatedTheta = Math.Atan2(maxY, maxX);

            //TODO: Compare Theta with 1 or whatever, instead of 172
            if(Math.Abs(rotatedY) < 172 && Math.Abs(rotatedY) > 1)
            {
                if (rotatedY < 0)
                    maxY = MaxTop - CentrePosition.Y;// -260; //Top
                else if (rotatedY > 0)
                    maxY = MaxBottom - CentrePosition.Y; //165; //Bottom
                
                maxX = (maxY / Math.Tan(rotatedTheta));
                if (maxX<MaxLeft-CentrePosition.X || maxX > MaxRight-CentrePosition.X)
                {
                    if (rotatedX < 0)
                        maxX = MaxLeft-CentrePosition.X; //Left
                    else if (rotatedX > 0) //TODO: Value sure always than 0, may change to single condition branch
                        maxX = MaxRight-CentrePosition.X; //Right
                    maxY = (maxX * Math.Tan(rotatedTheta));
                }  
            }

            return new Point((CentrePosition.X + (int)maxX), (CentrePosition.Y + (int)maxY));
        }

        private static double GetAngle(Point current, Point destination)
        {
            double dY = destination.Y - current.Y;
            double dX = destination.X - current.X;
            return Math.Atan2(dY, dX);
        }
    }
}
