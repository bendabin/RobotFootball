using System;
using AForge.Video;
using AForge.Video.DirectShow;
using AForge.Imaging;
using System.Drawing.Drawing2D;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Data;
using System.Text;

namespace ImageProcessingCapture 
{

    public class ImageCapture
    {
        VideoCaptureDevice videoSource;

        Bitmap latestBmp = null;
        bool bitmapFlag = false;

        public ImageCapture()
        {
            
        }
        
        public int checkCamera()
        {
            string sourceCheck = null;
            //List all available video sources. (That can be webcams as well as tv cards, etc)
            FilterInfoCollection videosources = new FilterInfoCollection(FilterCategory.VideoInputDevice);
            //Check if atleast one video source is available            

            if (videosources.Count > 0)
            {
                for (int i = 0; i < videosources.Count; i++)
                {
                    //For example use first video device. You may check if this is your webcam.
                    videoSource = new VideoCaptureDevice(videosources[i].Name);
                    if (videoSource.Source == "Logitech HD Pro Webcam C920")
                    {
                        sourceCheck = "Logitech";
                        videoSource = new VideoCaptureDevice(videosources[i].MonikerString);
                    }
                }

                if (sourceCheck == "Logitech") //check if video source found is the logitech webcam!
                {

                    try
                    {
                        //Check if the video device provides a list of supported resolutions
                        if (videoSource.VideoCapabilities.Length > 0)
                        {
                            //Set the highest resolution as active
                            videoSource.VideoResolution = videoSource.VideoCapabilities[0];
                            //Set Frame rate to highest
                            videoSource.DesiredFrameRate = 33;
                            videoSource.SetCameraProperty(CameraControlProperty.Focus, 0, CameraControlFlags.Manual); //set focus to auto
                        }
                    }
                    catch
                    {

                    }
                    ///// TODO : Constructor
                    videoSource.NewFrame += new AForge.Video.NewFrameEventHandler(videoSource_NewFrame);
                    videoSource.Start(); //turn video on    
                    return 1;
                    
                }
                return 0;       //return 0 if webcam not found
            }
            else
            {
                return 0;
            }
        }

        private void videoSource_NewFrame(object sender, AForge.Video.NewFrameEventArgs eventArgs)
        {
            Bitmap newBmp;
            newBmp = (Bitmap)eventArgs.Frame;   //recieve new frame from webcam and convert into Bitmap
            latestBmp=  CopyBMP(newBmp);
            bitmapFlag = true;
        }

        public Bitmap getFrame()
        {
            if (bitmapFlag)
            {//check if event handler has set latest image to true
                bitmapFlag = false;          
                return latestBmp;
            }
            else
                return null;
        }

        public void StopCam()       //turn witch cam off
        {
            videoSource.NewFrame -= videoSource_NewFrame;   //remove new Frame event handler
            videoSource.Stop();
            videoSource.SignalToStop();
            videoSource.WaitForStop();
        }

        public Bitmap CopyBMP(Bitmap bmp)
        {
            Bitmap newBMP = null;
            if (bmp != null)        //if bmp is not null
            {
                //clone the Bitmap
                Rectangle cloneImage = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
                newBMP = bmp.Clone(cloneImage, format);
            }
            return newBMP;
        }



        /*
        private Bitmap CopyBMP(Bitmap bmp)
        {
            Bitmap newBMP = null;
            if (bmp != null)        //if bmp is not null
            {
                //clone the Bitmap
                Rectangle cloneImage = new Rectangle(0, 0, bmp.Width, bmp.Height);
                System.Drawing.Imaging.PixelFormat format = bmp.PixelFormat;
                newBMP = bmp.Clone(cloneImage, format);
            }
            return newBMP;           
        }
        */

        //private VideoCaptureDevice videoSource;
    }
}

