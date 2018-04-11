#include <SPI.h>
#include <Wire.h>
#include <Adafruit_GFX.h>
#include <Adafruit_SSD1306.h>

//Versió amb tres botons

int switchPin = 4;
int upPin = 5;
int downPin = 3;
int analogPin=1;
String text;
unsigned char image[128];
String entrada;
int result;
int analog;
int analogOld;
String opcio;
bool connected;
bool switchButton, upButton, downButton, setMode;
Adafruit_SSD1306 display(7);
void setup() {
  // put your setup code here, to run once:
  Serial.begin(4800);
  pinMode(switchPin,INPUT);
  pinMode(upPin,INPUT);
  pinMode(downPin,INPUT);
  pinMode(LED_BUILTIN, OUTPUT);
  display.begin(SSD1306_SWITCHCAPVCC, 0x3C);
  display.clearDisplay();
  display.setTextSize(2);
  display.setTextColor(WHITE);
  display.println("NOT");display.println("CONNECTED");
  display.setCursor(0,0);
  display.display();
   // text display tests
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.setCursor(0,0);
  display.display();
  setMode=false;
  connected=false;
}
void redrawSlider(){
    display.drawRect(34,24,80,8,WHITE);
    if(setMode){
      display.fillRect(36,26,76,4,BLACK);
      display.fillRect(36,26,((((float)analog>1000?1000:(float)analog)/1000.0f)*76),4,WHITE);
    }
    else{
      display.fillRect(36,26,76,4,BLACK);
      display.drawRect(50,27,48,1,WHITE);
    }
    display.display();
  }
void redrawSetMode(){
  display.fillRect(121, 23, 7, 12, (setMode?WHITE:BLACK));
  if(setMode){
    display.setCursor(122,24);
    display.setTextSize(1);
    display.setTextColor(BLACK);
    display.println("S");
  }
  display.display();
}
void clearData(){
  display.fillRect(0,0,32,32,BLACK);
  display.fillRect(0,0,128,23,BLACK);
}
void displayData(){
  clearData();
  display.setCursor(40,10);
  display.setTextSize(1);
  display.setTextColor(WHITE);
  display.println(text);
  display.display();
  if(text!="----"){
    display.drawBitmap(0, 0,(unsigned char*)image, 32, 32, WHITE);
    display.display();
  }
  redrawSlider();
  redrawSetMode();
}

void tractarEntrada(){
  if(!connected){
    connected=true;
    display.clearDisplay();
  }
  opcio = Serial.readStringUntil('\n');
  switch(opcio[0]){
    case 'N':{// Program Name
      while(!Serial.available()){}
      text=Serial.readStringUntil('\n');
      Serial.println(opcio);
      Serial.println(text);
      displayData();
      break;
    }
    case 'I':{// Program Image
      while(!Serial.available()){}
      for(int i=0;i<128;i++){
        image[i]=Serial.read();
        Serial.read(); // netejem el \0
      }
      displayData();
      break;
    }
    case 'M':{// Set Mode
      while(!Serial.available()){}
      setMode=Serial.readStringUntil('\n')=="SET";
      digitalWrite(LED_BUILTIN, setMode?HIGH:LOW);
      redrawSetMode();
      redrawSlider();
      break;
    }
  }
}
void loop() {
  switchButton = digitalRead(switchPin);
  upButton=digitalRead(upPin);
  downButton=digitalRead(downPin);
  analog=analogRead(analogPin);
  Serial.print("Start\n");
  Serial.print(switchButton);
  Serial.print('\n');
  Serial.print(upButton);
  Serial.print('\n');
  Serial.print(downButton);
  Serial.print('\n');
  Serial.print(analog);
  Serial.print('\n');
  Serial.flush();
  if(Serial.available()){
    tractarEntrada();
  }
  if(setMode && (analogOld/2 != analog/2)){
    redrawSlider();
  }
  analogOld=analog;
}
