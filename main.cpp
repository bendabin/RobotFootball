/*******************************************************************************
* File: RobotPID.C
* By  : Ben Dabin
* Date: 15 March 2014
*
* Description :
*     The following program allows the Hedgehog robot to move to a specified 
*     target position relative to its current position, using PID control. The 
*     target position is specified by constant definitions inside the program
* Limitations :
*     Written for C language (not C++).
*     This program assumes the robot starts at the origin
*     and is pointing along the x axis.
*******************************************************************************/

/*******************************************************************************
* INCLUDES
*******************************************************************************/
#include "mbed.h"
#include "m3pi.h"
#include <string.h>
#include <ctype.h>
#include <math.h>
#include <TimerEvent.h>
#include <Interruptin.h>
#include <gpio_irq_api.h>
m3pi m3pi;
RawSerial Wixel(p28, p27); // tx, rx
//Timer timer;
Ticker timer;

//DigitalOut Wixel_nReset(p26); // don't declare unless you want nReset capability
/*******************************************************************************
* DEFINES
*******************************************************************************/
#define M_PI 3.141592653589793238462643  /* pi */
#define BUF_MAX			1000
#define USE_SERIAL_INTERRUPTS 0

/****************************************************************************************
*		GLOBAL VARIABLES
****************************************************************************************/
volatile char arrayBuff[250];
volatile char gotMsg; //got command flag
volatile unsigned char match_count; // this variable will increment every 20ms
#if USE_SERIAL_INTERRUPTS == 1
char buffer[BUF_MAX];
volatile unsigned int head, tail, count;	// buffer (queue) indexes
#endif

unsigned char inIdx;                // Read by background, written by Int Handler.
unsigned char outIdx;               // Read by Int Handler, written by Background.
/****************************************************************************************
*		FUNCTION PROTOTYPES
****************************************************************************************/
void SerialRecvInterrupt ();     //declare serial recieve interrupt     
void SerialTransInterrupt();
void delay();
void OutputString(char* str, int length); 	//put string in serial port 0 transmit buffer
void LeftMotor(float speed);
void RightMotor(float speed);
void GetMessage(struct Position *pos, struct Target *tgt); //declare function prototype for message
void RunMotors(struct Position *pos, struct Target *tgt);
void setup(void);
float ErrorCalc(float heading1, float heading2, struct Position *pos, struct Target *tgt);	//this will calculate the error
void MoveToTarget(struct Position *pos, struct Target *tgt);
/****************************************************************************************
*		ENUMS
****************************************************************************************/
enum MSG{POS_MSG, STOP_MSG};
enum ROBOT_STATE{STOP, MOVE_TO_TEMP, MOVE_TO_TGT};
/****************************************************************************************
*		STRUCTURES
****************************************************************************************/
struct Position
{	
	float ballX;
	float ballY;
	float robX;
	float robY;
	float robHdg;
	float ballHdg;
	float ballError;
	float ballDistance;
	int newMsg;
	int gotMsg;	
	int count;
};

struct Target
{
	float spd;
	float tgtHdg;
	float SL;
	float SR;
	int moveStatus;
	float tgtDistance;
	int X;
	int Y;	
};


/****************************************************************************************
*		MAIN FUNCTION
****************************************************************************************/	
int main()
{	
	
	//declare structure inside the main function
	struct Position pos;
	struct Target tgt;
	tgt.spd = 0.3;			//set the speed to a global variable of 0.3
	
	setup();			//initialise everything
	while(1)
	{ 
		if(gotMsg == 1)	//check if data is being recieved from the serial port 
		{
			gotMsg = 0;		//clear message flag
			GetMessage(&pos, &tgt);			//get message from the serial port
		}
	}
}

void RunMotors(struct Position *pos, struct Target *tgt)
{	
	static int robotState = STOP;
	static char msg[250];							//declare 100 bytes to store string		
	
	float theta;
	float ballTgtError=0;
	float ballDist= 0;
	float robBallError= 0;
	float temp1Angle= 0;
	float ballRobAngle = 0;
	float tempPtDistance= 0;
	float tempPtHdg;
	float tempPtError = 0;
	float tgtHdg;
	float tgtRelaDiffX;
	float tgtRelaDiffY;
	float tgtError = 0;
	float tgtDistance = 0;
	float ballError = 0;
	float ballHdg = 0;
	float ballTgtDist = 0;
	float tempHead= 0;
	float yPoint = 0;
	float xPoint  = 0;
  float SL = 0;
	float SR = 0;
    	
	int len;
	int repeat;
	
		switch(pos->newMsg)	//The message handler analyses the type of message received
		{
			
			case POS_MSG :	
			do
			{
			  repeat = 0;
				switch(robotState)
				{
								
					case STOP :					//This is the first state the the robot enters on reset								
							
					tgtHdg = atan2((tgt->Y - pos->robY),(tgt->X - pos->robX)); //calculate angle relative to target position and the robot	
					ballTgtError = ErrorCalc(pos->ballHdg,tgtHdg,pos,tgt);		//calculate error ballHdg - tgtHd
					ballTgtDist = sqrt(pow(tgt->Y - pos->ballY,2) + pow(tgt->X  - pos->ballX,2));			//calculate how far the ball is to the target point 
					
					
					//calculating the 1st temp1
					theta = atan2((tgt->Y - pos->ballY),(tgt->X - pos->ballX));		//calculate angle between ball point and target point					
					yPoint = pos->ballY - 150*sin(theta);
					xPoint = pos->ballX - 150*cos(theta);	
						
					if(ballTgtDist >100)			//check if the ball is within 100mm of the target position point
					{
							if(ballTgtError > 0.1 || ballTgtError <-0.1)					//check if angle between TGT and Ball, relative to robot is greater than 0.1 radians
							{
								robotState = MOVE_TO_TEMP; 							//move robot to temp point as ball is not in line with the target position and robot
								repeat = 1;
							}
							else //else move to target
							{
								robotState = MOVE_TO_TGT;							//move robot straight to the target
								repeat = 1;								
							}
							
					}
					else					//stay in stop state and turn motors off
					{
						SL= 0;
						SR= 0;
						LeftMotor(tgt->SL);
						RightMotor(tgt->SR);								
					}						
					sprintf(msg, "%1i, %5.0f, %5.0f, %5.0f, %5.0f, %5.3f, %5.3f, %1i\r\n", pos->count, pos->robX, pos->robY, pos->ballX, pos->ballY, tgtHdg, ballTgtError, robotState);
					len = strlen(msg);					//get length of message
					OutputString(msg, len);			//send message to P										

					break;						
								
					case MOVE_TO_TEMP :							
					
						tgtHdg = atan2((tgt->Y - pos->robY),(tgt->X - pos->robX)); //calculate angle relative to target position and the robot	
						ballTgtError = ErrorCalc(pos->ballHdg,tgtHdg,pos,tgt);		//calculate error ballHdg - tgtHd		
	
						//perform temporary point calculations
						theta = atan2((tgt->Y - pos->ballY),(tgt->X - pos->ballX));		//calculate angle between ball point and target point					
						yPoint = pos->ballY - 150*sin(theta);
						xPoint = pos->ballX - 150*cos(theta);
					
						tempHead = atan2((yPoint - pos->robY),(xPoint - pos->robX));	//calculate the heading angle relative to the temporary point 
						tempPtError = ErrorCalc(pos->robHdg, tempHead, pos,tgt);			//calculate angle between robot heading and temp point
						tempPtDistance = sqrt(pow(yPoint - pos->robY,2) + pow(xPoint  - pos->robX,2));			//calculate temp point distance between pt and robot 	
						
					
						if(tempPtError > 0.2)					//TURN LEFT if error greater than 0.2 radians						
						{
								
								if(tempPtDistance <= 40 && (ballTgtError < 0.1 && ballTgtError > -0.1))		//check if robot is within 40mm and in line with ball and target
								{
									robotState = MOVE_TO_TGT;				//change state to target point
									repeat = 1;
								}
								else
								{						
								  SL = -0.1;						
									SR = 0.1;								
									LeftMotor(SL);		//send speed values to the robot motors
									RightMotor(SR);

								}										
							}	
							else if(tempPtError <-0.2)			//TURN RIGHT if error less than -0.2 radians
							{											
								if(tempPtDistance <= 40 && (ballTgtError < 0.1 && ballTgtError > -0.1))		//check if robot is within 40mm and in line with ball and target
								{
									robotState = MOVE_TO_TGT;				//change state to move to target pointas
									repeat = 1;
								}						
								else
								{									
										SL = 0.1;						
										SR = -0.1;								
										LeftMotor(SL);		//send speed values to the robot motors
										RightMotor(SR);
								}									
		
							}
							else	//MOVE FORWARD	
							{	
								if(tempPtDistance <= 40 && (ballTgtError < 0.1 && ballTgtError > -0.1))		//check if robot is within 40mm and in line with ball and target
								{
									robotState = MOVE_TO_TGT;
									repeat = 1;
								}
								else
								{	
									SL = 0.1;
									SR = 0.1;
									LeftMotor(SL);
									RightMotor(SR);

								}
							}		

							sprintf(msg, "%1i, %5.0f, %5.0f, %5.0f, %5.0f, %5.0f, %5.0f, %5.1f, %5.3f, %5.3f, %5.3f, %5.3f, %5.3f, %5.3f, %5.3f, %5.3f, %1i, %1i\r\n", pos->count, pos->robX, pos->robY, pos->ballX, pos->ballY, xPoint, yPoint, 
																																	tempPtDistance, theta, tempPtError, ballTgtError, pos->ballHdg, pos->robHdg, tgtHdg, SL, SR, robotState, pos->newMsg);  	
							len = strlen(msg);	//get length of message
							OutputString(msg, len);			//send message to PC 		
											
					break;		
							
					case MOVE_TO_TGT : 					//The robot will go straight for the ball in this state
						
							tgtHdg = atan2((tgt->Y - pos->robY),(tgt->X - pos->robX)); //calculate angle relative to target position and the robot	
							tgtError = ErrorCalc(pos->robHdg,tgtHdg,pos,tgt);		//calculate error between robot heading and target heading					
							tgtDistance = sqrt(pow(tgt->Y - pos->robY,2) + pow(tgt->X  - pos->robX,2));			//calculate the target distance between robot and target point					
					
							if(tgtError > 0.2)							//TURN LEFT if error greater than 0.2 radians
							{
									if(tgtDistance <= 100)			//check if robot is within 100mm of the target position
									{
										SL = 0;
										SR = 0;
										LeftMotor(SL);						//send speed values to the robot motors
										RightMotor(SR);	
										repeat = 1;
										robotState = STOP;				//go back to stop state when this occurs
									}
									else
									{	
										if(tgtError >=0.2 && tgtError <= 0.3)
										{
											SL = -0.07;		
											SR = 0.07;				 
											LeftMotor(SL);			//send speed values to the robot motors
											RightMotor(SR);												
										}
										else
										{											
											SL = -0.1;						
											SR = 0.1;								
											LeftMotor(SL);		//send speed values to the robot motors
											RightMotor(SR);	
										}
									}
										
							}	
							else if(tgtError <-0.2)			//TURN RIGHT if error less than -0.2 radians
							{
									if(tgtDistance <= 100)			//check if robot is within 110mm of the target position
									{
										SL = 0;
										SR = 0;
										LeftMotor(SL);		//send speed values to the robot motors
										RightMotor(SR);	
										repeat = 1;
										robotState = STOP;				//go back to stop state when this occurs
									}
									else
									{
										if(tgtError <=-0.2 && tgtError >=-0.3)
										{
											SL = 0.07;		
											SR = -0.07;				 
											LeftMotor(SL);			//send speed values to the robot motors
											RightMotor(SR);														
										}
										else
										{
											SL = 0.1;		
											SR = -0.1;				 
											LeftMotor(SL);			//send speed values to the robot motors
											RightMotor(SR);	
										}											
									}
		
							}
							else	//MOVE FORWARD	
							{								
									if(tgtDistance <= 100)			//check if robot is within 110mm of the target position
									{
										SL = 0;
										SR = 0;
										LeftMotor(SL);		//send speed values to the robot motors
										RightMotor(SR);
										repeat = 1;
										robotState = STOP;				//go back to stop state when this occurs
									}
									else
									{	
										
										SL= tgt->spd*0.001*(tgtDistance - 100*sin(tgtError));		//Adjust the motor speed using the following algorithm
										SR= tgt->spd*0.001*(tgtDistance + 100*sin(tgtError));	
				
										if(SL <= 0.1 || SR <= 0.1)			//check if speed values drop below 0.1
										{
											SL = 0.1;
											SR = 0.1;
										}
										LeftMotor(SL);
										RightMotor(SR);
										
									}										
							}							
							sprintf(msg, "%1i, %5.0f, %5.0f, %5.0f, %5.0f, %5.3f, %5.3f, %5.3f, %5.3f, %5.3f, %5.3f, %1i, %1i\r\n", pos->count, pos->robX, pos->robY, pos->ballX, pos->ballY, 
																																	tgtDistance, tgtHdg, pos->robHdg, tgtError, SL, SR, robotState, pos->newMsg);  	
							len = strlen(msg);	//get length of message
							OutputString(msg, len);			//send message to PC 		

					break;					
				}	
			}while (repeat == 1);
		break;
				
		case STOP_MSG :
				SL = 0;
				SR = 0;
				LeftMotor(SL);			//send speed values to the robot motors
				RightMotor(SR);
				sprintf(msg, "%5.3f, %5.3f\r\n", SL, SR);	
				len = strlen(msg);	//get length of message		
				OutputString(msg, len);			//send message to PC
				robotState = STOP;
		break;			
	}	
			
		
}

float ErrorCalc(float heading1, float heading2, struct Position *pos, struct Target *tgt)
{
	float error = 0;
	
						//calculate error 	
						error = heading2 - heading1;		//calculate robot difference
						if(error > M_PI)			//account for the error angle changes 
						{
								error = error - 2*M_PI;
						}
						else if (error < - M_PI)
						{
								error = error + 2*M_PI; 
						}
		return error;
}


void setup(void)
{	
	__disable_irq();		//disable interrupts
	m3pi.cls(); 
	Wixel.baud(115200); // Wixel default baud rate (can configure other rates via Wixel config app...)
	Wixel.attach(&SerialRecvInterrupt, Wixel.RxIrq);
	gotMsg= 0;      //initialise the got command flag
#if USE_SERIAL_INTERRUPTS == 1
	Wixel.attach(&SerialTransInterrupt, Wixel.TxIrq);	//Attach a transmit interrupt
	// initialise serial output buffer
	head = 0;
	tail = 0;
	count = 0;
#endif	
	__enable_irq();		//enable interrupts
}


void GetMessage(struct Position *pos, struct Target *tgt)	//extracts message recieved from the serial port 
{
	int n, d1;
	int count;
	float f1, f2, f3, f4, f5, f6, f7;
	int i1;
	float ballX, ballY, robotX, robotY, robotHead;
            
		if(strncmp((char*)arrayBuff, "POS",3)== 0)
		{
			//extract the position values 
			n= sscanf((char*)arrayBuff + 3, "%f,%f,%f,%f,%f,%f,%f,%i",&f1, &f2, &f3, &f4, &f5, &f6, &f7, &i1);
			if (n == 8)    //got all the numbers
			{
				pos->ballX = f1;		//load variables into structures
				pos->ballY = f2;
				pos->robX = f3;
				pos->robY = f4;
				pos->robHdg = f5;				
				tgt->X = f6;
				tgt->Y = f7;				
				pos->count = i1;						  
				
				//*********************************************************************************
				//*				perform calculations on the numbers and place into position structure
				//*********************************************************************************				
				//set position flag and string
				pos->newMsg= POS_MSG;	//this will change the flag to position message
				pos->ballHdg = atan2((pos->ballY - pos->robY),(pos->ballX - pos->robX)); //calculate ball heading relative to the robot heading
			
			}			
		}
		else if(strncmp((char*)arrayBuff, "STOP",4)==0)
		{
			n= sscanf((char*)arrayBuff + 4, "%i", &i1);
			if(n == 1)
			{
				pos->count = i1;
				pos->newMsg= STOP_MSG;			
			}

		}
		RunMotors(pos, tgt);		//run the motors after recieving a position message	
}



//Interrupts
void SerialRecvInterrupt (void)
{
	static char buffer [250];
	static int index= 0;       
	char buff= 0;  

	buff = Wixel.getc();
	if(buff >= ' ' && buff <='~' && index < 99)
	{
		buffer[index]= buff; 
		index++;
	}
	if(buff == '\r')
	{          
		buffer[index] = 0; //null character code
		strcpy((char*)arrayBuff, buffer);      //copy string into 
		index= 0;
		gotMsg = 1;        
	}               
}
#if USE_SERIAL_INTERRUPTS == 1
void OutputString(char* str)
{
	  //can_irq_set(1,IRQ_TX,1);
	
    int length = strlen(str);
    NVIC_DisableIRQ(UART2_IRQn); // disable serial port 0 UDRE interrupt
		// check for too many chars
    if (count + length >= BUF_MAX)
    {
        NVIC_EnableIRQ(UART2_IRQn);          // enable serial port 0 UDRE interrupt
				return;
    }
    // write the characters into the buffer
    for (int n = 0; n < length; n++)
    {
        buffer[tail] = str[n];
        tail++;
        if (tail >= BUF_MAX)
        {
          tail = 0;
        }
    }
    count += length;
    NVIC_EnableIRQ(UART2_IRQn);            // enable serial port 0 UDRE interrupt
}
#else 
// transmit serial string NOT USING INTERRUPTS
void OutputString(char* str, int length)
{
    //int length = strlen(str);
		int ptr = 0;
	
		while(ptr < length)
		{
			if(Wixel.writeable())
			{
				Wixel.putc(str[ptr]);
				ptr++;
			}
		}		
}
#endif

// This is to put a wrapper on the motor speed ]. 
void LeftMotor(float speed)
{	
	m3pi.left_motor(-speed);		//this function is because we running motors backwards
}															//we want to reverse the sign of the motors to make it move in direction we want

void RightMotor(float speed)
{	
	m3pi.right_motor(-speed);		//this function is because we running motors backwards
}

#if USE_SERIAL_INTERRUPTS == 1
void SerialTransInterrupt(void)
{
	if (count > 0)
	{		
		Wixel.putc(buffer[head]);	//transmit the next character 
		
		head++;
		if (head > BUF_MAX)
		{
			head = 0;
		}
		count --;		
	}
	if(count == 0) //check to see if there are anymore characters
	{
		//serial_irq_set(
		NVIC_DisableIRQ(UART2_IRQn);
		//Wixel.set_flow_control(
	}	
}
#endif





