/* YourDuinoStarter Example: nRF24L01 Receive Joystick values

 - WHAT IT DOES: Receives data from another transceiver with
   2 Analog values from a Joystick or 2 Potentiometers
   Displays received values on Serial Monitor
 - SEE the comments after "//" on each line below
 - CONNECTIONS: nRF24L01 Modules See:
 http://arduino-info.wikispaces.com/Nrf24L01-2.4GHz-HowTo
   1 - GND
   2 - VCC 3.3V !!! NOT 5V
   3 - CE to Arduino pin 9
   4 - CSN to Arduino pin 10
   5 - SCK to Arduino pin 13
   6 - MOSI to Arduino pin 11
   7 - MISO to Arduino pin 12
   8 - UNUSED
   
 - V1.00 11/26/13
   Based on examples at http://www.bajdi.com/
   Questions: terry@yourduino.com */

/*-----( Import needed libraries )-----*/
#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
/*-----( Declare Constants and Pin Numbers )-----*/
#define CE_PIN   9
#define CSN_PIN 10

// NOTE: the "LL" at the end of the constant is "LongLong" type
const uint64_t pipe = 0xE8E8F0F0E1LL; // Define the transmit pipe


/*-----( Declare objects )-----*/
RF24 radio(CE_PIN, CSN_PIN); // Create a Radio
/*-----( Declare Variables )-----*/
int joystick[1];  // 2 element array holding Joystick readings
int enableA = 7; int pinA1 = 6; int pinA2 = 5;
int enableB = 2; int pinB1 = 4; int pinB2 = 3;



void setup()   /****** SETUP: RUNS ONCE ******/
{
  
      pinMode(enableA, OUTPUT);
      pinMode(pinA1, OUTPUT);
      pinMode(pinA2, OUTPUT);
      pinMode(enableB, OUTPUT);
      pinMode(pinB1, OUTPUT);
      pinMode(pinB2, OUTPUT);
      digitalWrite(enableA, HIGH);
      digitalWrite(enableB, HIGH);
      digitalWrite(pinA1, LOW);
      digitalWrite(pinA2, LOW);
      digitalWrite(pinB1, LOW);
      digitalWrite(pinB2, LOW);
      
  Serial.begin(9600);
  delay(1000);
  Serial.println("Nrf24L01 Receiver Starting");
  radio.begin();
  radio.openReadingPipe(1,pipe);
  radio.startListening();;
}//--(end setup )---


void loop()   /****** LOOP: RUNS CONSTANTLY ******/
{
  if ( radio.available() )
  {
    // Read the data payload until we've received everything
    bool done = false;
    while (!done)
    {
   
      // Fetch the data payload
      done = radio.read( joystick, sizeof(joystick) );
      
      if(joystick[0]=='1')
      {
         Serial.println("First command");
           digitalWrite(pinB1,LOW);
        digitalWrite(pinB2, HIGH); 
       
        digitalWrite(pinA1,LOW);
        digitalWrite(pinA2, HIGH); 
      }
    
      else if(joystick[0]=='2')
      {
         Serial.print("First command");
         
           digitalWrite(pinB1,LOW);
        digitalWrite(pinB2, HIGH); 
       
        digitalWrite(pinA1,LOW);
        digitalWrite(pinA2, LOW); 
      }
      else  if(joystick[0]=='3')
      {
         Serial.print("First command");
         
           digitalWrite(pinB1,LOW);
        digitalWrite(pinB2, LOW); 
       
        digitalWrite(pinA1,LOW);
        digitalWrite(pinA2, HIGH); 
      }
       else if(joystick[0]=='4')
      {
         Serial.print("First command");
         
           digitalWrite(pinB1,LOW);
        digitalWrite(pinB2, LOW); 
       
        digitalWrite(pinA1,LOW);
        digitalWrite(pinA2, LOW); 
      }
      else if (joystick[0]=='5')
      {
        digitalWrite(pinB1,HIGH);
        digitalWrite(pinB2, LOW); 
       
        digitalWrite(pinA1,HIGH);
        digitalWrite(pinA2, LOW); 
        
      }
        
      //  Serial.print("ttttttttttttttttttttttttttttt");

      
    
    /*  Serial.print("X = ");
      Serial.println(joystick[0]);*/
     /* Serial.print(" Y = ");      
      Serial.println(joystick[1]);*/
    }
  }
  else
  {    
      Serial.println("No radio available");
  }

}//--(end main loop )---

/*-----( Declare User-written Functions )-----*/

//NONE
//*********( THE END )***********

