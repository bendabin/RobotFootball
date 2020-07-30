# RobotFootball


This is a final year University Project, which consists of a GUI(Graphical User Interface) and a Pololu Robot. The main objective of the project is to allow the robot to move a ball to a specified location which is chosen by the user on the GUI screen. Images of the ball are supplied by a Webcam mounted above the robot which are fed to the GUI program, which process the information into (x,y) cordinates and then the coordinates are transmitted back to the robot through a wireless serial link.   

The code is split into two parts. 

- Part 1 (The Robot which has an algorithm for controlling the robot's motor wheels from the (x,y) coordinates which are transmitted and sent to the robot.
- Part 2 (The GUI which process images from a Webcam into (x,y) coordinates).

**Bold**For more details on the project report please click [here](/Full_Final_Year_Report.pdf).

![ImageText](/Images/project_stuff.jpg?raw=true "Complete robot setup") 

## Part 1 

The Robot which has an algorithm for controlling the robot's motor wheels from the (x,y) coordinates which are transmitted and sent to the robot. 

Click [here](/RobotCode/main.cpp) to view the code for the robot algorithm. The name of the file is called main.cpp and is written in C++.

## Part 2

![ImageText](/Images/GUI1.png?raw=true "The GUI for the project")

**Bold**To view the code for the GUI for calculting the algorithms for the robot coordinates please click [here](/GUI/Form1BU.cs). The name of the file is called Form1BU.cs and is written C#. 


