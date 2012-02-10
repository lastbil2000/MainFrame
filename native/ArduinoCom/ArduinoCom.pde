
//package definitions:
#define PACKAGE_COMUNIT 0x1
#define PACKAGE_SERVO 0x2
#define PACKAGE_SETPIN 0x3
#define PACKAGE_2Y0A02 0x4
#define PACKAGE_SRF05 0x5
#define PACKAGE_EZ1 0x6

//ServoController package: <def><ouputPin><headerSize><dataSize><header><data><checkByte>
#define PACKAGE_SIZE 7

//serial data flags
#define GENERAL_MESSAGE_FLAG  0x0
#define ERROR_MESSAGE_FLAG 0x1
#define WARNING_MESSAGE_FLAG 0x2
#define OPERATION_COMPLETE_MESSAGE_FLAG  0x4

//constants
#define SHARP2Y0A02_VOLTS_PER_UNIT 0.0048828125f
#define SHARP2Y0A02_EQUATION_EXPONENT -1.1904f
#define SHARP2Y0A02_CM_CONVERSION 60.495f
#define SONAR_DENOMIATOR 58          //relation between output interval and distance in cm
#define SERVO_HANDSHAKE_INTERVAL 20  //time to intitiate servo communication

//digital pins
#define PIN_SERIAL_INPUT 0x0    //occupied by the 
#define PIN_SERIAL_OUTPUT 0x1   //usb-serial device. i think
#define PIN_SERVO1 0x0C   
#define PIN_SERVO2 0x0B
#define PIN_SERVOBOARD 0x0A    //controls wheither the servo board is on or of
#define PIN_LASER 0x09
#define PIN_MOTOR_POWER 0x08  //pin controlling engine power supply input port
#define PIN_SRF05_INPUT 0x3    //echo pin
#define PIN_SRF05_OUTPUT 0x4   //trigger pin
#define PIN_EZ1_PW 0x5   //pulse width pin for ez1

//analogue pins
#define APIN_2Y0A02 0x1 //sharp
#define APIN_EZ1 0x2 //EZ MaxSonar using analogue reading

//configuration
#define BAUDRATE 9600
#define CORRUPTION_CHECK_BYTE 0xAA
#define SERIAL_TRANSFER_INTERVAL 100  //delay in milliseconds after each serial transfer (read or write)
#define SERVO_TRANSFER_INTERVAL 95 //the transfer interval in microsoconds (multiply by 2) used while talking to the servo controller

static byte bytes[10];
static int byteCounter = 0;
static int irDistance = 0;

void setup() {
  
//  pinMode(PIN_SERIAL_INPUT, INPUT);
  pinMode(PIN_SERIAL_OUTPUT, OUTPUT);
  pinMode (PIN_SERVO1, OUTPUT);
  pinMode (PIN_SERVO2, OUTPUT);
  pinMode (PIN_SERVOBOARD, OUTPUT);
  pinMode (PIN_LASER, OUTPUT);
  pinMode (PIN_MOTOR_POWER, OUTPUT);
  pinMode (PIN_SRF05_INPUT, INPUT);
  pinMode (PIN_SRF05_OUTPUT, OUTPUT);
  pinMode (PIN_EZ1_PW, INPUT);  

  digitalWrite (PIN_SERVO1, LOW);
  digitalWrite (PIN_SERVO2, LOW);
  digitalWrite (PIN_SRF05_OUTPUT, LOW);
  digitalWrite (PIN_SERVOBOARD, LOW);
  
  Serial.begin(BAUDRATE);
  for (int i = 0; i < 10; i++)
     bytes[i] = 0;
      
  byteCounter = 0;  

  Serial.println(" Arduino is ready");

}


void loop() {

  delay(50);
  //i stället här: mät ett antal värden. förkasta dem som skiljer sig för mycket från medianen. beräkna genomsnittsvärdet. 
  float volts = analogRead(APIN_2Y0A02)*SHARP2Y0A02_VOLTS_PER_UNIT;  //analogRead(APIN_2Y0A02);//
  irDistance = (int) (SHARP2Y0A02_CM_CONVERSION * pow(volts, SHARP2Y0A02_EQUATION_EXPONENT));
  if (irDistance < 20 || irDistance > 150)
    irDistance = 0;
  
  //Serial.print ("Distance:  ");
  //Serial.print (irDistance);
//  irDistance = 42;
  

  //TODO: read until CORRUPTION_CHECK_BYTE is found...
  if(Serial.available() == PACKAGE_SIZE ){
      byte pack =  Serial.read();
      while (Serial.available() > 0)
        bytes[byteCounter++] = Serial.read();

      if (bytes[byteCounter - 1] != CORRUPTION_CHECK_BYTE) {
        setError("corruption check failed. Aborting transfer");
        delay(SERIAL_TRANSFER_INTERVAL);
          Serial.print((int)pack);
        delay(SERIAL_TRANSFER_INTERVAL);
        setError("------------------");
        delay(SERIAL_TRANSFER_INTERVAL);
          Serial.print(byteCounter);
        delay(SERIAL_TRANSFER_INTERVAL);
        setError("------------------");
        delay(SERIAL_TRANSFER_INTERVAL);
          Serial.print((int)bytes[byteCounter - 1]);
        Serial.flush();
      }
      
      
      else {
       switch (pack) {
          case PACKAGE_SETPIN:
             writePort(bytes[0],bytes[1], 1);
          break;
          case PACKAGE_SERVO:
            picServoTransfer();
          break;
          case PACKAGE_2Y0A02:
            irDistanceTransfer();
            break;
          case PACKAGE_SRF05:
            srf05Transfer();
            break;
          case PACKAGE_EZ1:
             ez1Transfer();
             break;
          default:
             delay(SERIAL_TRANSFER_INTERVAL);
             setWarning("WARNING: unknown package or packages lost.");
             delay(SERIAL_TRANSFER_INTERVAL);
             Serial.flush();
          break;
       
         }
      }

      
      byteCounter = 0;
      for (int i = 0; i < 10; i++)
        bytes[i] = 0;


   }
    
}

void srf05Transfer() {
  digitalWrite (PIN_SRF05_OUTPUT, HIGH);
  delayMicroseconds(50);
  digitalWrite (PIN_SRF05_OUTPUT, LOW);
  unsigned long pulseTime = pulseIn (PIN_SRF05_INPUT, HIGH);
  unsigned long distance = pulseTime / SONAR_DENOMIATOR;
  Serial.write(distance);
  Serial.write(OPERATION_COMPLETE_MESSAGE_FLAG);
//  delay(SERIAL_TRANSFER_INTERVAL);
}



void ez1Transfer() {
 unsigned long pulseTime = pulseIn(PIN_EZ1_PW, HIGH);
 unsigned long distance = pulseTime / SONAR_DENOMIATOR;

  Serial.write(distance);
  Serial.write(OPERATION_COMPLETE_MESSAGE_FLAG);

//  delay(SERIAL_TRANSFER_INTERVAL);
 
  
}

void irDistanceTransfer() {
  //Serial.write(DATA_MESSAGE_FLAG);
  Serial.write(irDistance);
  Serial.write(OPERATION_COMPLETE_MESSAGE_FLAG);
  //Serial.flush();
 
}

void picServoTransfer() {

     unsigned char arduinoPort = bytes[0];
     unsigned char headerBits = bytes[1];
     unsigned char dataBits = bytes[2];
     unsigned char header = bytes[3];
     unsigned char data = bytes[4];
     
     if (arduinoPort == PIN_SERVO1 ||
         arduinoPort == PIN_SERVO2)
     {
       //handShake: 
         digitalWrite(arduinoPort, HIGH);
    
         delay(20);
         digitalWrite(arduinoPort, LOW);

         writePort((int)arduinoPort, header, (int)headerBits);
         writePort((int)arduinoPort, data, (int)dataBits);
     
         digitalWrite(arduinoPort, LOW);
         Serial.write(OPERATION_COMPLETE_MESSAGE_FLAG);

        
//        delay(SERIAL_TRANSFER_INTERVAL);
        //Serial.println("Data sent to servo!");

     }
     else
       setError("writing to illegal output pin");        
//     while (Serial.available())
     
     //delay(SERIAL_TRANSFER_INTERVAL);

}

void writePort(int outputPin, unsigned char data, int length) {
       for (int i = 0; i < length; i++) {
         if (data & 1 == 1)
            digitalWrite(outputPin, HIGH);
         else
            digitalWrite(outputPin, LOW);
         
         data = data >> 1;

         //delay(SERVO_TRANSFER_INTERVAL);         
         delayMicroseconds(SERVO_TRANSFER_INTERVAL);         
       }
}


//like write port, but with the MSB first

void writePortReversed(int outputPin, unsigned char data, int length) {
       for (int i = 0; i < length; i++) {
         if (data & 128 == 128)
            digitalWrite(outputPin, HIGH);
         else
            digitalWrite(outputPin, LOW);
         
         data = data << 1;
         
         //Serial.print(data);
         delay(SERVO_TRANSFER_INTERVAL);
          
         //delayMicroseconds(SERVO_TRANSFER_INTERVAL);         
       }
}

void setError(const char * message) {

 // Serial.write(WARNING_MESSAGE_FLAG);
  Serial.write( ERROR_MESSAGE_FLAG);
 
  Serial.print ("ERROR: ");
  Serial.println(message);
  
  //delay(SERIAL_TRANSFER_INTERVAL);
}

void setWarning (const char * message) {

 // Serial.write(WARNING_MESSAGE_FLAG);
  Serial.write( WARNING_MESSAGE_FLAG);
 
  Serial.print ("WARNING: ");
  Serial.println(message);
  
 // delay(SERIAL_TRANSFER_INTERVAL);
}

void comUnitTransfer() {
  /*
    //digitalWrite(comUnitOutputPin, HIGH);
     unsigned char portSelect = bytes[1];
     unsigned char header = bytes[2];
     unsigned char data = bytes[3];
     
     //if (portSelect > 0)
     //  digitalWrite(comUnitOutputPin, HIGH);
     
     if (portSelect == 1 && header == 2 && data == 42)
       digitalWrite(comUnitOutputPin, HIGH);
     else
        digitalWrite(comUnitOutputPin, LOW);
     */
}
