using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
//create using directives for easier access of AForge library's methods
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using System.Drawing.Imaging;
//allows the Marshal.copy to be used for the image processing
using System.Runtime.InteropServices;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace ImageProcessingCapture
{
    public partial class Image_Process : Form
    {
        public byte[] ImageBytes;
        Boolean processing = false;
        Boolean imaging = false;
        int red_pixel, green_pixel, blue_pixel, orange_pixel;
        int sum1, sum2, sum3;
        Bitmap b;

        public Image_Process()
        {
            InitializeComponent();          //inialise everything
        }
        //Create webcam object
        VideoCaptureDevice videoSource;

        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "Please update for red pixel count";
            //List all available video sources. (That can be webcams as well as tv cards, etc)
            FilterInfoCollection videosources = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //Check if atleast one video source is available
            if (videosources != null)
            {
                //For example use first video device. You may check if this is your webcam.
                videoSource = new VideoCaptureDevice(videosources[0].MonikerString);

                try
                {
                    //Check if the video device provides a list of supported resolutions
                    if (videoSource.VideoCapabilities.Length > 0)
                    {
                        string highestSolution = "0;0";
                        //Search for the highest resolution
                        for (int i = 0; i < videoSource.VideoCapabilities.Length; i++)
                        {
                            if (videoSource.VideoCapabilities[i].FrameSize.Width > Convert.ToInt32(highestSolution.Split(';')[0]))
                                highestSolution = videoSource.VideoCapabilities[i].FrameSize.Width.ToString() + ";" + i.ToString();
                        }
                        //Set the highest resolution as active
                        videoSource.VideoResolution = videoSource.VideoCapabilities[0];
                        //Set Frame rate to highest
                        videoSource.DesiredFrameRate= 33;
                       // videoSource.SetCameraProperty(CameraControlProperty.Focus, 0, CameraControlFlags.Auto); //set focus to auto
                        this.videoSource.SetCameraProperty(CameraControlProperty.Focus, 0, CameraControlFlags.Auto);
                        this.videoSource.SetCameraProperty(CameraControlProperty.Zoom, 0, CameraControlFlags.Auto);
                        
                    }
                }
                catch {}

                //Create NewFrame event handler
                //(This one triggers every time a new frame/image is captured
                videoSource.NewFrame += new AForge.Video.NewFrameEventHandler(videoSource_NewFrame);
             //   CameraControlProperty.Focus= 
                //Start recording
                // videoSource.Start();
            }

        }

        //call this event everytime image data is recieved from camera
        void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            if (processing)
            {
                return;
            }
            //Cast the frame as Bitmap object and don't forget to use ".Clone()" otherwise
            //you'll probably get access violation exceptions

            Bitmap frame_copy= (Bitmap)eventArgs.Frame;
            Bitmap frame_process = (Bitmap)eventArgs.Frame;  
            pictureBoxVideo.Image = frame_copy.Clone(new Rectangle(0, 0, frame_copy.Width, frame_copy.Height), frame_copy.PixelFormat); //display image into pic box (exception error when in use)             
            if(imaging)     //if update button pressed process image frame
            {
                imaging = false;
                red_pixel= 0;   //initialise red_pixel count
                blue_pixel = 0;
                green_pixel = 0;
                orange_pixel= 0;
                int average_blue = 0;
                int average_green = 0;
                int average_red = 0;
                sum1=0;
                sum2 = 0;
                sum3=0;
                    //Bitmap frame_process = (Bitmap)eventArgs.Frame;         
                pictureBoxMod.Image = (Bitmap)frame_process.Clone(new Rectangle(0, 0, frame_process.Width, frame_process.Height), frame_process.PixelFormat);
                    BitmapData bData = frame_process.LockBits(new Rectangle(0, 0, frame_process.Width, frame_process.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
 
                    //get address of the first line
                    IntPtr ptr = bData.Scan0;
  
                    // Declare an array to hold the bytes of the bitmap. 
                    int bytes = Math.Abs(bData.Stride) * frame_copy.Height;
                    byte[] rgbValues = new byte[bytes];

                    // Copy the RGB values into the array.
                        System.Runtime.InteropServices.Marshal.Copy(ptr, rgbValues, 0, bytes);

                        int xtotal = 0, ytotal = 0;
                        int orangeXtotal = 0, orangeYtotal = 0;
                        int redXtotal= 0, redYtotal= 0;
                        int number, number2 = 0;
                        // check every pixel colour

                //   int sum1, sum2, sum3, counter_sum;     


                for (int counter = 0; counter < rgbValues.Length; counter += 3)  
                {
                    float hue = Color.FromArgb(rgbValues[counter + 2], rgbValues[counter + 1], rgbValues[counter]).GetHue();
                    if (hue > 30 && hue < 80)
                    {
                        orange_pixel += 1;             //increment orange pixel count
                        number = counter / 3;
                        int y_orange = number / 640;
                        int x_orange = number % 640;
                        orangeXtotal = orangeXtotal + x_orange;
                        orangeYtotal = orangeYtotal + y_orange;
                    }
                    
                }

                for (int counter = 0; counter < rgbValues.Length; counter += 3)
                {
                    float hue = Color.FromArgb(rgbValues[counter + 2], rgbValues[counter + 1], rgbValues[counter]).GetHue();
                    //float sat = Color.FromArgb(rgbValues[counter + 2], rgbValues[counter + 1], rgbValues[counter]).GetSaturation();

                  //  if ((hue > 359 && hue < 360)&&(sat > 7/8 && sat <=1))        //increment red pixel count
                    if ((hue > 359 && hue < 360))
                    {
                        red_pixel += 1;
                        number2 = counter / 3;
                        int y_red = number2 / 640;
                        int x_red = number2 % 640;
                        redXtotal = redXtotal + x_red;
                        redYtotal = redYtotal + y_red;
                    }
                }               
 /////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////             
 //                                                     calculate the coordinates
//
/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

                        int xposRed = -1, yposRed = -1;
                        if (red_pixel > 0)
                        {
                            xposRed = redXtotal / red_pixel;
                            yposRed = redYtotal / red_pixel;
                        }
                        int xposOrange = -1, yposOrange = -1;
                        if(orange_pixel > 0)
                        {
                            xposOrange= orangeXtotal/orange_pixel;
                            yposOrange = orangeYtotal / orange_pixel;
                        }


                        string red_pixel_value= Convert.ToString(red_pixel);
                        string orange_pixel_value= Convert.ToString(orange_pixel);
                            
                        if(textBox1.InvokeRequired)
                        {
                            textBox1.Invoke((MethodInvoker)delegate { textBox1.Text = "Red Pixel Count: " + red_pixel_value + " at " + xposRed + ", " + yposRed + " Orange Pixel Count: " + orange_pixel_value + " at " + xposOrange + ", " + yposOrange + " (" + orangeXtotal + ", " + orangeYtotal + ")"; });
                        }
                        else
                        {
                            textBox1.Text = "Red Pixel Count: " + red_pixel_value + " at " + xposRed + ", " + yposRed + " Orange Pixel Count: " + orange_pixel_value + " at " + xposOrange + ", " + yposOrange + " (" + orangeXtotal + ", " + orangeYtotal + ")";
                        }
                            
                        // Copy the RGB values back to the bitmap
                        System.Runtime.InteropServices.Marshal.Copy(rgbValues, 0, ptr, bytes);

                        // Unlock the bits.
                        frame_process.UnlockBits(bData);
                       

                       // pictureBoxMod.Image = (Bitmap)frame_process.Clone(new Rectangle(0, 0, frame_process.Width, frame_process.Height), frame_process.PixelFormat);                      
                        
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            //Stop and free the webcam object if application is closing
            if (videoSource != null && videoSource.IsRunning)
            {
                videoSource.SignalToStop();
                videoSource = null;
                //pictureBoxVideo.BackgroundImage.Dispose();
                //pictureBoxVideo.InitialImage = null;
            }
        }

        private void Stop_button_Click(object sender, EventArgs e)
        {

            videoSource.SignalToStop();
            videoSource.WaitForStop();
            pictureBoxVideo.Image = null;
            pictureBoxMod.Image = null;
        }

        //start video signal
        private void Start_button_Click(object sender, EventArgs e)
        {
            videoSource.Start();         //start video processing(not this threw a NullReferenceException  problem when the code was started 
            processing = false;
            imaging = false;
        }

        private void Update_Butt_Click(object sender, EventArgs e)
        {
            imaging = true;             //change state to true to update status
        }

    }
}













