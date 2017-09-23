//Versió amb tres botons

int switchPin = 3;
int upPin = 5;
int downPin = 7;

int result;

bool switchButton, upButton, downButton;

void setup() {
  // put your setup code here, to run once:
  Serial.begin(9600);
  pinMode(switchPin,INPUT);
  pinMode(upPin,INPUT);
  pinMode(downPin,INPUT);
}

void loop() {
  switchButton = digitalRead(switchPin);
  upButton=digitalRead(upPin);
  downButton=digitalRead(downPin);
  result=0;
  if(switchButton)result++;
  if(upButton)result+=2;
  if(downButton)result+=4;
  Serial.println(result);
}
