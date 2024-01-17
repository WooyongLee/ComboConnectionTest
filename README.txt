------------------------------------------------------------------------------
PACT Calibration SW
by SoLunStar Co.,Ltd.                                 
------------------------------------------------------------------------------

1. Description
--------------
PACT - Design of Cal SW v1.5 문서를 기반으로 
PACT Calibration SW를 개발 구조 및 내용에 대해서 설명하는 문서임.


2. Development Environment
------------------------------
IDE 툴: Microsoft Visual Studio Community 2019.
언어: C# 


3. 주요 파일 설명
-------------------------
  1) Common.cs              : 일반 Util 함수 및 Json 관련된 함수 구현.     
  2) Constants.cs           : 전역 상수나 클래스 정의.
  3) FileUitl.cs            : 파일 처리 관련 함수 구현.
  4) frmMain.cs             : Windows Form 구조이고, GUI 디자인 및 기능 구현. 
  5) frmMainFtpClient.cs    : Windows Form Partial 구조이고, FtpClient 기능 구현(NuGet 패키지 - FluentFTP 패키지 사용).
  6) frmMainMqttClient.cs   : Windows Form Partial 구조이고, MqttClient 기능 구현.
                              (Global Offset, IQ imbalance, Power & Atten Accuracy 시나리오 구현)
  7) frmMainScpiPM.cs       : Windows Form Partial 구조이고, PMClient 기능 구현.
                              (Cable Loss 시나리오 구현)
  8) frmMainScpiSA.cs       : Windows Form Partial 구조이고, SAClient 기능 구현.
  9) frmMainScpiSG.cs       : Windows Form Partial 구조이고, SGClient 기능 구현.
  10) Mqtt.cs               : MQTT Class 정의 및 관련 함수 구현(NuGet 패키지 - MQTTnet 패키지 사용)
  11) TelnetConnect.cs      : TCP Class 정의 및 관련 함수 구현
  12) menunode.json         : 프로그램 로딩시, 기본적인 접속 정보 및 
                              Calibration 관련 Parameter를 JSON 형태로 정의


4. DabinPACT 구조 설명
--------------------
  1) 최초 화면 로딩: DabinPACT.exe 파일을 실행을 시키면, frmMain.cs에서 GUI화면을 초기화한다.
                    frmMain()에서 Tree구조의 메뉴를 구성하기 위해서 menujson.json을 Load하고, 
                    GUI Component가 생성이 완료되면 OnLoad()에서 기본적인 Cal하고자 하는 장비 접속 시도한다.
                    Loading 화면을 띄우고, 접속이 완료되면 Loading 화면을 없어지면, Calibratoin 준비가 완료된 상태이다.
                    (Cal 하고자 하는 장비의 정보는 JSON에서 가져 오지만, Tree 메뉴에는 추가하지 않는다.)

  2) Multi Task 사용: 각 시나리오(Global Offset을 제외)는 Task로 실행을 하고, Task 수행을 완료될 때까지 await를 하기 된다.
                      Task를 중단하기 위해서는 CancellationToken을 이용해서, token이 Cancel 되었는지에 대해서 주기적으로
                      Check를 해서 Task를 중단을 시킬수도 있는 구조 이다.

  3) Calibration 시작: GUI의 상단에 있는 DO를 클릭하면 메뉴에서 check된 시나리오를 수행을 한다.
                       STOP을 클릭하면 수행중인 시나리오를 중단한다.

  4) 시나리오 수행 함수: 함수 호출전 수행 node에 대한 하위 Parameter를 Hashtable에 저장하고, 
                        호출 parameter로 Hashtable과 CancellationToken을 전달한다.
                        수행 함수에는 PACT - Design of Cal SW v1.5 문서에서 설명한 Calibratoin 절차대로 구현을 하였고,
                        수행 함수는 다음과 같다.
     가) ExecCableLossScenario: PMClient에서 수행
     나) ExecGlobalOffsetScenario: MqttClient에서 수행
     다) ExecIQimbalanceScenario: MqttClient에서 수행
     라) ExecPowerAttenScenario: MqttClient에서 수행

  5) 자료구조: Attenuator의 Code를 찾기 위해서 MultiKeyDictionary 사용, 
              Frequency별 검색을 위해서 Frequency를 키로 하는 Hashtable 사용

  6) 파일 저장 위치: 실행파일의 상위 폴더에 output 폴더를 생성해서 파일을 저장.
  

5. Contact Info
---------------

Email: yj.park@solunstar.com
               
