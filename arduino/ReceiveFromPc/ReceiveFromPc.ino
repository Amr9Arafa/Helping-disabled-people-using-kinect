#include <SPI.h>
#include <nRF24L01.h>
#include <RF24.h>
/*-----( Declare Constants and Pin Numbers )-----*/
#define CE_PIN   9
#define CSN_PIN 10
#define JOYSTICK_X A0
#define JOYSTICK_Y A1


int incomingByte = 0;// for incoming serial data
const uint64_t pipe = 0xE8E8F0F0E1LL;

RF24 radio(CE_PIN, CSN_PIN); // Create a Radio
/*-----( Declare Variables )-----*/
int joystick[1];


void setup() {
        Serial.begin(9600);  
        
         radio.begin();
         radio.openWritingPipe(pipe);
     
        // opens serial port, sets data rate to 9600 bps
       //   pinMode(13, OUTPUT);
}

void loop() {

        // send data only when you receive data:
        
         
          
     
          
        if (Serial.available() > 0) {
                // read the incoming byte:
                incomingByte = Serial.read();
                
                joystick[0]=incomingByte;

              if( incomingByte=='1')
               {
                 
                 radio.write( joystick,sizeof(joystick));
              //    digitalWrite(13, HIGH);
               }
             
              else if(incomingByte=='2')
              {
                radio.write( joystick, sizeof(joystick) );
              }  
              
              else if(incomingByte=='3')
              {
                radio.write( joystick, sizeof(joystick) );
              }  
              else if(incomingByte=='4')
              {
                radio.write( joystick, sizeof(joystick) );
              }  
              else if(incomingByte=='5')
              {
                radio.write( joystick, sizeof(joystick) );
              } 
              
               
                // say what you got:
                //Serial.print("I received: ");
                //Serial.println(incomingByte, DEC);
        }
}
