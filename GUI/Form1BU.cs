using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32.SafeHandles;
using System.Security.Permissions;
//create using directives for easier access of AForge library's methods
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using System.Drawing.Imaging;
//allows the Marshal.copy to be used for the image processing
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Reflection;
//enable the use of the serial port
using System.IO.Ports;
using System.Timers;

namespace ImageProcessingCapture
{
    public partial class Image_Process : Form
    {

        ImageCapture myImageCapt;

        //delegates
        internal delegate void SerialDataReceivedEventHandlerDelegate(object sender, SerialDataReceivedEventArgs e);
        delegate void SetTextCallback(string text);
        delegate void SetPictureCallback(Bitmap bmp);
        string DataBuffer = String.Empty;

        //global variables
        public byte[] ImageBytes;
        int redHueValMax, redHueValMin, blueHueValMax, blueHueValMin, greenHueValMax, greenHueValmin, yellowHueValMax, yellowHueValMin, orangeHueValMax, orangeHueValMin, magentaHueValMax, magentaHueValMin, cyanHueValMax, cyanHueValMin;
        int redSatValueA, blueSatValueA, greenSatValueA, yellowSatValueA, orangeSatValueA, magSatValueA, cyanSatValueA;

        int targetX;
        int targetY;

        //binary flags
        Boolean stopping = false;
        
        
        public struct ColourPosPixel
        {
            public int x, y, n;
            public ColourPosPixel(int px, int py, int pn)
            {
                x = px;
                y = py;
                n = pn;
            }
        }
        ColourPosPixel redPosPixel;
        ColourPosPixel bluePosPixel;
        ColourPosPixel greenPosPixel;
        ColourPosPixel yellowPosPixel;
        ColourPosPixel orangePosPixel;
        ColourPosPixel magentaPosPixel;
        ColourPosPixel cyanPosPixel;

        public struct ColourPosMM
        {
            public double x, y;
            public ColourPosMM(double px, double py)
            {
                x = px;
                y = py;
            }
        }
        ColourPosMM orangePosMM;
        ColourPosMM magentaPosMM;
        ColourPosMM cyanPosMM;

        public struct ImageBitmap           //this structure will store the Bitmap image
        {
            public Bitmap bMap, bMap2, bNew;
            public ImageBitmap(Bitmap image, Bitmap image2, Bitmap image3)
            {
                bMap = image;
                bMap2 = image2;
                bNew = image3;
            }

        }

        //Create webcam object
        int lengthValue, widthValue;
        int msgCount = 0;


        private object lockObject = new object();

        string InputData = String.Empty;

        public Image_Process()
        {
            InitializeComponent();          //inialise everything
            stopping = true;
            msgCount = 0;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            myImageCapt = new ImageCapture();       //creating image capture object

            int camCheck = 0;
            do
            {
                camCheck = myImageCapt.checkCamera();
                if (camCheck != 1)
                    MessageBox.Show("NO WEB CAM DEVICE DETECTED PLEASE CONNECT CAMERA!");
            } while (camCheck != 1);

            //setup serial port event handler
            this.serialPort1.DataReceived += new SerialDataReceivedEventHandler(serialPort1_DataReceived);

            //Initialise text boxes 
            imageDataBox.Text = "Count,Rn,Rx,Ry,Bn,BX,By,Gn,Gx,Gy,Yn,Yx,Yy,On,Ox,Oy,Mn,Mx,My,Cn,Cx,Cy\r\n";
            serialPortBox.Text = "Count, ballX, ballY, robotX, robotY, robotHead, ballHead, distance, error, leftSpeed, rightSpeed\r\n";
            robotSentDataBox.Text ="Count, ballX, ballY, robotX, robotY, robotHead, ballHead, distance, error, leftSpeed, rightSpeed\r\n";
           
            textBox1.Text = "Please update for red pixel count";

            //load the colour default values on startup
            redHueValMax = 347;
            redHueValMin = 20;

            blueHueValMax = 260;
            blueHueValMin = 215;

            greenHueValMax = 150;
            greenHueValmin = 80;

            yellowHueValMax = 57;
            yellowHueValMin = 45;

            orangeHueValMax = 40;
            orangeHueValMin = 23;

            magentaHueValMax = 340;
            magentaHueValMin = 300;

            cyanHueValMax = 200;
            cyanHueValMin = 174;

            //set colour values 
            redSatValueA = 150;
            blueSatValueA = 100;
            greenSatValueA = 100;
            yellowSatValueA = 75;
            orangeSatValueA = 125;
            magSatValueA = 100;
            cyanSatValueA = 100;

            //Setup serial port and find Wixel
            //openToolStripMenuItem1.Enabled = false;

			string[] ArrayComPortsNames = null;
            cboPorts.Items.Clear();

            ArrayComPortsNames = SerialPort.GetPortNames();

            for (int i = 0; i < ArrayComPortsNames.Length; i++)
            {
                if (ArrayComPortsNames[i] == "COM4")
                    cboPorts.Items.Add(ArrayComPortsNames[i]);
                    cboPorts.Text = ArrayComPortsNames[i];
            }			

			//cboPorts.Text = ArrayComPortsNames[0];

            //load target values to default setting
            targetX = (int)tgtX.Value;
            targetX = (int)tgtY.Value;

            //start image timer 
            Image_timer.Enabled = true;
        }


        private void SetPicture(Bitmap bmp)
        {
            if (liveCamVideo.InvokeRequired)
            {
                SetPictureCallback d = new SetPictureCallback(SetPicture);
                if (!stopping && bmp != null)
                {
                    Invoke(d, bmp); // new object[] { bmp });

                }
            }
            else
            {
                liveCamVideo.Image = bmp;    //display incomming data in the text box
            }
         }

        	
        //This will be the time that calculate the pixel coordinates of the robot
        private void Image_timer_Tick(object sender, EventArgs e)
        {
            string robotX = null;
            string robotY = null;
            string ballX = null;
            string ballY = null;
            string robotData = null;   //this will contain data to send to seria port

            Bitmap bmp = null;
            Bitmap imageProcess = null;
            Bitmap bmpCal = null;

            bmp = myImageCapt.getFrame();   //get frame from webCam

            if (bmp == null)        //return if Bitmap is null
                return;

            if (calButt.Checked == true)            //check if calibration button has been enabled
            {
                bmpCal = myImageCapt.CopyBMP(bmp);   //copy frame            
                imageMaskBox.Image = CalTimerImageFunction(bmpCal);      //display masked image
            }
            else
            {
                imageMaskBox.Image = null;
                imageProcess = bmp;
                SetPicture(bmp);//make frame equal to bmp   
                calcPixelPosition(imageProcess);    //calculate the pixel value coordinates and pass frame to function
                Image_timer.Enabled = false;
        
                //check for a sensible amount of pixels to do the calculations 
                if (redPosPixel.n > 700 && bluePosPixel.n > 700 && greenPosPixel.n > 700 && yellowPosPixel.n > 700 && orangePosPixel.n > 300 && magentaPosPixel.n > 200 && cyanPosPixel.n > 200)
                {
                    //calculate X,Y position of ball
                    calcPosMM();    //calculate the pixel values into MM which are loaded into structures

                    //Convert BallX and BallY into a string
                    ballX = Convert.ToString(Convert.ToInt64(orangePosMM.x));
                    ballY = Convert.ToString(Convert.ToInt64(orangePosMM.y));

                    //Calculate robot position 
                    double robotAvrX = (cyanPosMM.x + magentaPosMM.x) / 2;
                    double robotAvrY = (cyanPosMM.y + magentaPosMM.y) / 2;
                    robotX = Convert.ToString(Convert.ToInt64(robotAvrX));
                    robotY = Convert.ToString(Convert.ToInt64(robotAvrY));

                    //Calculate the heading of the robot
                    double robotHeadX = cyanPosMM.x - magentaPosMM.x;
                    double robotHeadY = cyanPosMM.y - magentaPosMM.y;
                    double robotHeadRad = Math.Atan2(robotHeadY, robotHeadX);  //calculate robot heading in radians
                    double robotHeadDeg = (robotHeadRad * 180)/ Math.PI;
                    string robotHeadstr = Convert.ToString(Convert.ToInt64(robotHeadDeg));
                    string robotHead = Convert.ToString(Convert.ToDouble(robotHeadRad));     //send readings in radians

                    robotData = null;

                    string tgtXVal = Convert.ToString(Convert.ToInt64(targetX));
                    string tgtYVal = Convert.ToString(Convert.ToInt64(targetY));

                    robotData = "POS" + " " + ballX + ", " + ballY + ", " + robotX + ", " + robotY + ", " + robotHead + ", " + tgtXVal + ", " + tgtYVal + ", ";

                    textBox1.Text = "Ball location is at: " + ballX + " MM," + ballY + " MM" + "\r\n"
                                + "Robot Position is at: " + robotX + " MM," + robotY + " MM" + "\r\n"
                                + "Robot Heading at " + robotHeadstr + " Degrees" + "\r\n";

                }
                else
                {
                    textBox1.Text = "CALIBRATION NEEDED!";
                    robotData = null;
                    robotData = "STOP "; //construct a stop string to send to the robot!              
                }
                if (openToolStripMenuItem2.Enabled == false && serialPort1.IsOpen && startRobotButt.Enabled == false)    //check if serial port has been open
                { 
                    msgCount++;
                    string msgStr = Convert.ToString(msgCount);
                    string robotDataStr = robotData + msgStr + "\r\n";
                    robotSentDataBox.Text += robotDataStr;

                    imageDataBox.Text += msgCount + ", " + redPosPixel.n + ", " + redPosPixel.x + ", " + redPosPixel.y
                                + ", " + bluePosPixel.n + ", " + bluePosPixel.x + ", " + bluePosPixel.y
                                + ", " + greenPosPixel.n + ", " + greenPosPixel.x + ", " + greenPosPixel.y
                                + ", " + yellowPosPixel.n + ", " + yellowPosPixel.x + ", " + yellowPosPixel.y
                                + ", " + orangePosPixel.n + ", " + orangePosPixel.x + ", " + orangePosPixel.y
                                + ", " + magentaPosPixel.n + ", " + magentaPosPixel.x + ", " + magentaPosPixel.y
                                + ", " + cyanPosPixel.n + ", " + cyanPosPixel.x + ", " + cyanPosPixel.y + "\r\n";

                    sendToSerial(robotDataStr);   //send data to robot  
                }
            }
            Image_timer.Enabled = true;
        }

        //calculates the colours of the webcam image into pixel coordinates
		private void calcPixelPosition(Bitmap latestBmp)
		{
			byte[] rgbValues;       //this value will store the current Bitmap for processing the image
			IntPtr ptr;
			int bytes;
			BitmapData bData;

			bData = latestBmp.LockBits(new Rectangle(0, 0, latestBmp.Width, latestBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb); 

            unsafe
            {
                //get address of the first line
                ptr = bData.Scan0;
                // Declare an array to hold the bytes of the bitmap. 
                bytes = Math.Abs(bData.Stride) * latestBmp.Height;
                rgbValues = new byte[bytes];
                // Copy the RGB values into the array.
                System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);
            } // unsafe
            // Unlock the bits.
            latestBmp.UnlockBits(bData);            //unlock the image    
            latestBmp = null;                

            //initialise red_pixel count variables
			int red_pixel = 0;
			int blue_pixel = 0;
			int green_pixel = 0;
			int yellow_pixel = 0;
			int orange_pixel = 0;
			int magenta_pixel = 0;
			int cyan_pixel = 0;
			//Initialise values for X,Y pixels
			int redXtotal = 0;
			int redYtotal = 0;
			int blueXtotal = 0;
			int blueYtotal = 0;
			int greenXtotal = 0;
			int greenYtotal = 0;
			int yellowXtotal = 0;
			int yellowYtotal = 0;
			int orangeXtotal = 0;
			int orangeYtotal = 0;
			int magentaXtotal = 0;
			int magentaYtotal = 0;
			int cyanXtotal = 0;
			int cyanYtotal = 0;
			// check every pixel colour
			int hue = 0;
			int sat = 0;
			int pixel_x = 0;
			int pixel_y = 0;

			for (int counter = 0; counter < rgbValues.Length; counter += 3)                 
			{
				CalcHueSat(rgbValues[counter + 2], rgbValues[counter + 1], rgbValues[counter], ref hue, ref sat);

				if ((hue > redHueValMax || hue < redHueValMin) && (sat > redSatValueA))      //red red pixel range
				{
					red_pixel++;
					redXtotal += pixel_x;
					redYtotal += pixel_y;
				}
				else if ((hue > blueHueValMin && hue < blueHueValMax) && (sat > blueSatValueA))  // dark blue pixel range
				{
					blue_pixel++;
					blueXtotal += pixel_x;
					blueYtotal += pixel_y;
				}
				else if ((hue > greenHueValmin && hue < greenHueValMax) && (sat > greenSatValueA))   //dark green pixel range
				{
					green_pixel++;
					greenXtotal += pixel_x;
					greenYtotal += pixel_y;
				}
				else if ((hue > yellowHueValMin && hue < yellowHueValMax) && (sat > yellowSatValueA))   //yellow pixel range
				{
					yellow_pixel++;
					yellowXtotal += pixel_x;
					yellowYtotal += pixel_y;
				}
				else if ((hue > orangeHueValMin && hue < orangeHueValMax) && (sat > orangeSatValueA)) //orange pixel range
				{
					orange_pixel++;
					orangeXtotal += pixel_x;
					orangeYtotal += pixel_y;
				}
				else if ((hue > magentaHueValMin && hue < magentaHueValMax) && (sat > magSatValueA)) //magenta pixel range
				{
					magenta_pixel++;
					magentaXtotal += pixel_x;
					magentaYtotal += pixel_y;
				}
				else if ((hue > cyanHueValMin && hue < cyanHueValMax) && (sat > cyanSatValueA)) //cyan pixel range
				{
					cyan_pixel++;
					cyanXtotal += pixel_x;
					cyanYtotal += pixel_y;
				}        
           		pixel_x++;
				if (pixel_x >= 640)
				{
					pixel_x = 0;
					pixel_y++;
				}          
            }
        
            //calculate pixel coordinates
			int xposRed = -1, yposRed = -1;
			if (red_pixel > 0)
			{
				xposRed = redXtotal / red_pixel;
				yposRed = redYtotal / red_pixel;
			}
			int xposBlue = -1, yposBlue = -1;
			if (blue_pixel > 0)
			{
				xposBlue = blueXtotal / blue_pixel;
				yposBlue = blueYtotal / blue_pixel;
			}
			int xposGreen = -1, yposGreen = -1;
			if (green_pixel > 0)
			{
				xposGreen = greenXtotal / green_pixel;
				yposGreen = greenYtotal / green_pixel;
			}
			int xposYellow = -1, yposYellow = -1;
			if (yellow_pixel > 0)
			{
				xposYellow = yellowXtotal / yellow_pixel;
				yposYellow = yellowYtotal / yellow_pixel;
			}
			int xposOrange = -1, yposOrange = -1;           //for the orange ball
			if (orange_pixel > 0)
			{
				xposOrange = orangeXtotal / orange_pixel;
				yposOrange = orangeYtotal / orange_pixel;
			}
			int xposMagenta = -1, yposMagenta = -1;        //for the magenta circle on robot
			if (magenta_pixel > 0)
			{
				xposMagenta = magentaXtotal / magenta_pixel;
				yposMagenta = magentaYtotal / magenta_pixel;
			}
			int xposCyan = -1, yposCyan = -1;        //for the cyan circle on robot
			if (cyan_pixel > 0)
			{
				xposCyan = cyanXtotal / cyan_pixel;
				yposCyan = cyanYtotal / cyan_pixel;
			}
            
            
            //load variables into the structures
            redPosPixel.n = red_pixel;
            redPosPixel.x = xposRed;
            redPosPixel.y = yposRed;

            bluePosPixel.n = blue_pixel;
            bluePosPixel.x = xposBlue;
            bluePosPixel.y = yposBlue;

            greenPosPixel.n = green_pixel;
            greenPosPixel.x = xposGreen;
            greenPosPixel.y = yposGreen;

            yellowPosPixel.n = yellow_pixel;
            yellowPosPixel.x = xposYellow;
            yellowPosPixel.y = yposYellow;

            orangePosPixel.n = orange_pixel;
            orangePosPixel.x = xposOrange;
            orangePosPixel.y = yposOrange;

            magentaPosPixel.n = magenta_pixel;
            magentaPosPixel.x = xposMagenta;
            magentaPosPixel.y = yposMagenta;

            cyanPosPixel.n = cyan_pixel;
            cyanPosPixel.x = xposCyan;
            cyanPosPixel.y = yposCyan;             
		}
		
		//this function calculates the X,Y coordinates of the colours on the field in terms of Millimetres(MM)
        private void calcPosMM() 
		{
            //validPos = true;
            int L = lengthValue;                                    //length and width of the field set by global variables in calibration mode
            int W = widthValue;
            int[] a = new int[4];
            int[] b = new int[4];

            a[0] = bluePosPixel.x;                                        //calculate formulas for the four colours
            a[1] = yellowPosPixel.x - bluePosPixel.x;
            a[2] = greenPosPixel.x - bluePosPixel.x;
            a[3] = redPosPixel.x + bluePosPixel.x - greenPosPixel.x - yellowPosPixel.x;
            b[0] = bluePosPixel.y;
            b[1] = yellowPosPixel.y - bluePosPixel.y;
            b[2] = greenPosPixel.y - bluePosPixel.y;
            b[3] = redPosPixel.y + bluePosPixel.y - greenPosPixel.y - yellowPosPixel.y;
            long[] q = new long[3];                 //calculate the ball's position in the field
            q[0] = a[1] * b[3] - a[3] * b[1];       //this value will be the same for the other colours

            q[1] = a[1] * b[2] - a[2] * b[1] + a[0] * b[3] - a[3] * b[0] + orangePosPixel.y * a[3] - orangePosPixel.x * b[3];
            q[2] = a[0] * b[2] - a[2] * b[0] + orangePosPixel.y * a[2] - orangePosPixel.x * b[2];
            double ballx = -1000; // in mm
            double bally = -1000;
            long D;
            if (q[0] != 0)
            {
                D = q[1] * q[1] - 4 * q[0] * q[2];
                if (D >= 0)
                {
                    if (q[1] >= 0)
                    {
                        ballx = (-q[1] + Math.Sqrt(D)) / (2 * q[0]);
                    }
                    else
                    {
                        ballx = (-q[1] - Math.Sqrt(D)) / (2 * q[0]);
                    }
                }
            }
            else if (q[1] != 0)
            {
                ballx = -q[2] / q[1];
            }
            if (ballx != -1000 && a[2] + a[3] * ballx != 0)
            {
                bally = (orangePosPixel.x - a[0] - a[1] * ballx) / (a[2] + a[3] * ballx);
                ballx = L * ballx;
                bally = W * bally;
                //load the structures with the calculate coordinates in MM
                orangePosMM.x = ballx;
                orangePosMM.y = bally;
            }

            //calculate the magenta pixel value in MM
            q[1] = a[1] * b[2] - a[2] * b[1] + a[0] * b[3] - a[3] * b[0] + magentaPosPixel.y * a[3] - magentaPosPixel.x * b[3];
            q[2] = a[0] * b[2] - a[2] * b[0] + magentaPosPixel.y * a[2] - magentaPosPixel.x * b[2];
            double magentax = -1000; // in mm
            double magentay = -1000;

            if (q[0] != 0)
            {
                D = q[1] * q[1] - 4 * q[0] * q[2];
                if (D >= 0)
                {
                    if (q[1] >= 0)
                    {
                        magentax = (-q[1] + Math.Sqrt(D)) / (2 * q[0]);
                    }
                    else
                    {
                        magentax = (-q[1] - Math.Sqrt(D)) / (2 * q[0]);
                    }
                }
            }
            else if (q[1] != 0)
            {
                magentax = -q[2] / q[1];
            }
            if (magentax != -1000 && a[2] + a[3] * magentax != 0)
            {
                magentay = (magentaPosPixel.x - a[0] - a[1] * magentax) / (a[2] + a[3] * magentax);
                magentax = L * magentax;
                magentay = W * magentay;
                //load the structures with the calculate coordinates in MM
                magentaPosMM.x = magentax;
                magentaPosMM.y = magentay;
            }

            //calculate the cyan pixel value in MM
            q[1] = a[1] * b[2] - a[2] * b[1] + a[0] * b[3] - a[3] * b[0] + cyanPosPixel.y * a[3] - cyanPosPixel.x * b[3];
            q[2] = a[0] * b[2] - a[2] * b[0] + cyanPosPixel.y * a[2] - cyanPosPixel.x * b[2];
            double cyanx = -1000; // in mm
            double cyany = -1000;

            if (q[0] != 0)
            {
                D = q[1] * q[1] - 4 * q[0] * q[2];
                if (D >= 0)
                {
                    if (q[1] >= 0)
                    {
                        cyanx = (-q[1] + Math.Sqrt(D)) / (2 * q[0]);
                    }
                    else
                    {
                        cyanx = (-q[1] - Math.Sqrt(D)) / (2 * q[0]);
                    }
                }
            }
            else if (q[1] != 0)
            {
                cyanx = -q[2] / q[1];
            }
            if (cyanx != -1000 && a[2] + a[3] * cyanx != 0)
            {
                cyany = (cyanPosPixel.x - a[0] - a[1] * cyanx) / (a[2] + a[3] * cyanx);
                cyanx = L * cyanx;
                cyany = W * cyany;
                //load the structures with the calculate coordinates in MM
                cyanPosMM.x = cyanx;
                cyanPosMM.y = cyany;
            }        
		}
     
		Bitmap CalTimerImageFunction(Bitmap calBMP)
		{
			byte[] rgbModValues;       //this value will store the current Bitmap for processing the image
			IntPtr ptr2;
			int bytes;
			BitmapData bData2;

			//initialise red_pixel count variables
			int red_pixel = 0;
			int blue_pixel = 0;
			int green_pixel = 0;
			int yellow_pixel = 0;
			int orange_pixel = 0;
			int magenta_pixel = 0;
			int cyan_pixel = 0;

			// check every pixel colour
			int hue = 0;
			int sat = 0;
            
            bData2 = calBMP.LockBits(new Rectangle(0, 0, calBMP.Width, calBMP.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
			unsafe
			{
				//get address of the first line
				ptr2 = bData2.Scan0;
				// Declare an array to hold the bytes of the bitmap. 
                bytes = Math.Abs(bData2.Stride) * calBMP.Height;
				rgbModValues = new byte[bytes];
				// Copy the RGB values into the array
                System.Runtime.InteropServices.Marshal.Copy(ptr2, rgbModValues, 0, bytes);
			} 

			for (int counter = 0; counter < rgbModValues.Length; counter += 3)
			{
				CalcHueSat(rgbModValues[counter + 2], rgbModValues[counter + 1], rgbModValues[counter], ref hue, ref sat);

				if ((hue > redHueValMax || hue < redHueValMin) && (sat > redSatValueA))      //red red pixel range
				{
					red_pixel++;
					rgbModValues[counter + 2] = 255;
					rgbModValues[counter + 1] = 0;
					rgbModValues[counter] = 0;
				}
				else if ((hue > blueHueValMin && hue < blueHueValMax) && (sat > blueSatValueA))  // dark blue pixel range
				{
					blue_pixel++;
					rgbModValues[counter + 2] = 0;
					rgbModValues[counter + 1] = 0;
					rgbModValues[counter] = 255;
				}
				else if ((hue > greenHueValmin && hue < greenHueValMax) && (sat > greenSatValueA))   //dark green pixel range
				{
					green_pixel++;
					rgbModValues[counter + 2] = 0;
					rgbModValues[counter + 1] = 255;
					rgbModValues[counter] = 0;
				}
				else if ((hue > yellowHueValMin && hue < yellowHueValMax) && (sat > yellowSatValueA))   //yellow pixel range
				{
					yellow_pixel++;
					rgbModValues[counter + 2] = 255;
					rgbModValues[counter + 1] = 255;
					rgbModValues[counter] = 0;
				}
				else if ((hue > orangeHueValMin && hue < orangeHueValMax) && (sat > orangeSatValueA))	//orange pixel range
				{
					orange_pixel++;
					rgbModValues[counter + 2] = 255;
					rgbModValues[counter + 1] = 128;
					rgbModValues[counter] = 0;
				}
				else if ((hue > magentaHueValMin && hue < magentaHueValMax) && (sat > magSatValueA)) //magenta pixel range
				{
					magenta_pixel++;
					rgbModValues[counter + 2] = 255;
					rgbModValues[counter + 1] = 0;
					rgbModValues[counter] = 255;
				}
				else if ((hue > cyanHueValMin && hue < cyanHueValMax) && (sat > cyanSatValueA)) 	//cyan pixel range
				{
					cyan_pixel++;
					rgbModValues[counter + 2] = 0;
					rgbModValues[counter + 1] = 255;
					rgbModValues[counter] = 255;
				}
				else																				//make pixels black otherwise
				{
					rgbModValues[counter + 2] = 0;
					rgbModValues[counter + 1] = 0;
					rgbModValues[counter] = 0;
				}
			}
			unsafe
			{
				//get address of the first line
				ptr2 = bData2.Scan0;
				System.Runtime.InteropServices.Marshal.Copy(rgbModValues, 0, ptr2, bytes);
			}
            calBMP.UnlockBits(bData2);            //unlock the image    (A generic error occurred in GDI+)            
            return calBMP;
        }

        private void sendToSerial(string robotData)
        {                  

            //is the serial port open
            if (serialPort1.IsOpen)
            {
                try
                {
                    serialPort1.Write(robotData);   //send data to serial port  
                }
                catch(TimeoutException)
                {
                    //Serial Port communications disabled!
                    SerialPortCheck.Checked = false;
                    SerialPortCheck.Enabled = true;
                    cboPorts.Enabled = true;
                    cboBaudRate.Enabled = true;
                    openToolStripMenuItem2.Enabled = true;

                    if (serialPort1.IsOpen)
                    {
                        serialPort1.Close();
                        this.BeginInvoke((Action)(() => MessageBox.Show("Serial Port closed!")));
                    }

                }              
            }
        
        }

        private void Image_Process_FormClosing(object sender, FormClosingEventArgs e)
        {
            myImageCapt.StopCam();      //turn off webcam
            Image_timer.Enabled = false;
        } 
		//when Serial port option has been selected!
		private void openToolStripMenuItem1_Click(object sender, EventArgs e)
		{

			//openToolStripMenuItem1.Enabled = false;

			string[] ArrayComPortsNames = null;
			int index = -1;
			string ComPortName = null;
            cboPorts.Items.Clear();

            ArrayComPortsNames = SerialPort.GetPortNames();
			do
			{
				index += 1;
				cboPorts.Items.Add(ArrayComPortsNames[index]);

			}
			while (!((ArrayComPortsNames[index] == ComPortName)
			  || (index == ArrayComPortsNames.GetUpperBound(0))));
			Array.Sort(ArrayComPortsNames);

			if (index == ArrayComPortsNames.GetUpperBound(0))
			{
				ComPortName = ArrayComPortsNames[0];
			}
			cboPorts.Text = ArrayComPortsNames[0];

			//baud rates
			//cboBaudRate.Items.Add(115200);
			//cboBaudRate.Items.ToString();
			//get first item print in text
			//cboBaudRate.Text = cboBaudRate.Items[0].ToString();

		}
		
		//Data being recieved from the serial port  
		private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
		{
            try
            {
                DataBuffer = serialPort1.ReadExisting();        //read incoming data from the serial port
                if (DataBuffer != String.Empty)
                {
                    SetText(DataBuffer); //Put time at front of string
                }
            }
            catch(TimeoutException)
            {
                //Serial Port communications disabled!
                SerialPortCheck.Checked = false;
                SerialPortCheck.Enabled = true;
                cboPorts.Enabled = true;
                cboBaudRate.Enabled = true;
                openToolStripMenuItem2.Enabled = true;

                if (serialPort1.IsOpen)
                {
                    serialPort1.Close();
                    this.BeginInvoke((Action)(() => MessageBox.Show("Serial Port closed!")));

                }
            }

		}
		//Uses delegate method for receiving the data from the serial port
		private void SetText(string text)
		{
			if (serialPortBox.InvokeRequired)
			{
				SetTextCallback d = new SetTextCallback(SetText);
				Invoke(d, new object[] { text });
			}
			else
			{
				this.serialPortBox.Text += text;    //display incomming data in the text box
			}
		}  

		private void CalcHueSat(int red, int green, int blue, ref int hueVal, ref int satVal)
		{
			hueVal = 0;
			satVal = 0;
			if (blue >= green)
			{
				if (red >= blue)
				{
					// maxVal = red; // 300-360 R>B>G
					// minVal = green;
					if (red != green)
					{
						hueVal = 360 - 60 * (blue - green) / (red - green);
						satVal = 255 * (red - green) / red;
					}
				}
				else // blue > red
				{
					// maxVal = blue; // 180-300
					if (red >= green)
					{
						// minVal = green; // 240-300 B>R>G
						if (blue != green)
						{
							hueVal = 240 + 60 * (red - green) / (blue - green);
							satVal = 255 * (blue - green) / blue;
						}
					}
					else
					{
						// minVal = red; // 180-240 B>G>R
						if (blue != red)
						{
							hueVal = 240 - 60 * (green - red) / (blue - red);
							satVal = 255 * (blue - red) / blue;
						}
					}
				}
			}
			else // green > blue
			{
				if (green >= red)
				{
					// maxVal = green; // 60-180
					if (red >= blue)
					{
						// minVal = blue; // 60-120 G>R>B
						{
							hueVal = 120 - 60 * (red - blue) / (green - blue);
							satVal = 255 * (green - blue) / green;
						}
					}
					else // blue > red
					{
						// minVal = red; // 120-180 G>B>R
						if (green != red)
						{
							hueVal = 120 + 60 * (blue - red) / (green - red);
							satVal = 255 * (green - red) / green;
						}
					}
				}
				else // red > green
				{
					// maxVal = red; // 0-60 R>G>B
					// minVal = blue;
					if (red != blue)
					{
						hueVal = 60 * (green - blue) / (red - blue);
						satVal = 255 * (red - blue) / red;
					}
				}
			}

		}

        //If the serial port open option has been selected
        private void openToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SerialPortCheck.Checked = true;
            SerialPortCheck.Enabled = false;
            cboPorts.Enabled = false;
            cboBaudRate.Enabled = false;
            openToolStripMenuItem2.Enabled = false;

            try
            {
                serialPort1.PortName = Convert.ToString(cboPorts.Text);
                serialPort1.BaudRate = Convert.ToInt32(cboBaudRate.Text);
                serialPort1.Open();         //open serial port

                if (!(serialPort1.IsOpen))
                {
                    SerialPortCheck.Checked = false;
                    SerialPortCheck.Enabled = true;
                    cboPorts.Enabled = true;
                    cboBaudRate.Enabled = true;
                }
                else
                {
                    this.BeginInvoke((Action)(() => MessageBox.Show("Serial Port Open!"))); 
                }
            }
            catch (Exception)
            {
                this.BeginInvoke((Action)(() => MessageBox.Show("Select Serial Port First"))); 
                SerialPortCheck.Checked = false;
                SerialPortCheck.Enabled = true;
                cboPorts.Enabled = true;
                cboBaudRate.Enabled = true;
                openToolStripMenuItem2.Enabled = true;

            }

        }        

		private void clearToolStripMenuItem_Click(object sender, EventArgs e)
		{
			serialPortBox.Text = null;
		}

		//If the serial port closed option has been selected
		private void closeToolStripMenuItem1_Click(object sender, EventArgs e)
		{

			SerialPortCheck.Checked = false;
			SerialPortCheck.Enabled = true;
			cboPorts.Enabled = true;
			cboBaudRate.Enabled = true;
			openToolStripMenuItem2.Enabled = true;

			if (serialPort1.IsOpen)
			{
                try
                {
                    serialPort1.Close();
                    this.BeginInvoke((Action)(() => MessageBox.Show("Serial Port closed!")));
                }
                catch(Exception)
                {
                    this.BeginInvoke((Action)(() => MessageBox.Show("Serial missing serial port now closed!")));
                }

			}

		}
	
		private void button1_Click(object sender, EventArgs e)              //clear serial port boxes
		{
			imageDataBox.Text = null;
            imageDataBox.Text = "Count,Rn,Rx,Ry,Bn,BX,By,Gn,Gx,Gy,Yn,Yx,Yy,On,Ox,Oy,Mn,Mx,My,Cn,Cx,Cy\r\n";
			serialPortBox.Text = null;
            serialPortBox.Text = "Count, ballX, ballY, robotX, robotY, robotHead, ballHead, distance, error, leftSpeed, rightSpeed\r\n";
            robotSentDataBox.Text = null;
            robotSentDataBox.Text = "Count, ballX, ballY, robotX, robotY, robotHead, ballHead, distance, error, leftSpeed, rightSpeed\r\n";
		}
		private void targetSetButt_Click(object sender, EventArgs e)
        {
            targetX= (int)tgtX.Value;
            targetY = (int)tgtY.Value;
        }

        private void setCalVal_Click(object sender, EventArgs e)
        {
            redHueValMax = (int)RedUpDwnMax.Value;
            redHueValMin = (int)RedUpDwnMin.Value;

            blueHueValMax = (int)BlueUpDwnMax.Value;
            blueHueValMin = (int)BlueUpDwnMin.Value;

            greenHueValMax = (int)GreenUpDwnMax.Value;
            greenHueValmin = (int)GreenUpDwnMin.Value;

            yellowHueValMax = (int)YellowUpDwnMax.Value;
            yellowHueValMin = (int)YellowUpDwnMin.Value;

            orangeHueValMax = (int)OrangeUpDwnMax.Value;
            orangeHueValMin = (int)OrangeUpDwnMin.Value;

            magentaHueValMax = (int)MagUpDwnMax.Value;
            magentaHueValMin = (int)MagUpDwnMin.Value;

            cyanHueValMax = (int)CyanUpDwnMax.Value;
            cyanHueValMin = (int)CyanUpDwnMin.Value;

            //set colour values 
            redSatValueA = (int)redSatVal.Value;
            blueSatValueA = (int)blueSatVal.Value;
            greenSatValueA = (int)greenSatVal.Value;
            yellowSatValueA = (int)yellowSatVal.Value;
            orangeSatValueA = (int)orangeSatVal.Value;
            magSatValueA = (int)MagSat.Value;
            cyanSatValueA = (int)CyanSat.Value;

            //set field size values
            lengthValue = (int)LengthUpDown.Value;
            widthValue = (int)WidthUpDown.Value;
        }
        private void SerialPortCheck_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void startRobotButt_Click(object sender, EventArgs e)
        {

            if (!serialPort1.IsOpen)            //check if serial port is open
            {
                this.BeginInvoke((Action)(() => MessageBox.Show("Open Serial Port First!"))); 
                startRobotButt.Enabled = true;
            }
            else if (calButt.Checked == true)   //check if calibration is on
            {
                startRobotButt.Enabled = true;
                this.BeginInvoke((Action)(() => MessageBox.Show("Please Stop Calibration First!"))); 
            }
            else
                startRobotButt.Enabled = false;
        }

        private void stopRobButt_Click(object sender, EventArgs e)            
        {
            sendToSerial("STOP 0\r\n");     //send out stop command to robot
            startRobotButt.Enabled = true;            
            msgCount = 0;
        }

       
    }

}



  





