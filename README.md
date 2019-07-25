# Introduction to FiddleFiddle

FiddleFiddle (피들피들) 은 .NET 기반의 Fiddler 확장 프로젝트 입니다. Fiddler 는 웹 디버깅(Web Debugging)용도로 주로 사용되는 툴인데요. 저 코마는 이 기능을 좀더 확장하여 API Request 를 자동 보관하는 확장 프로그램을 만들었습니다.

## Change Log

- 2019 
  - 12 Dec : Release 배포 예정
  - 06 June : 현재 계정으로 Repo 이관 (From gbkim1988 to code-machina)
- 2018
  - 18 June : Beta 버전
  - 5 March : Alpha 버전

## 기능

- Posgtresql 를 데이터베이스로 사용
- Django Rest API 를 제공
- Fiddler 실행 시 로그인 팝업 제공
  - JWT 기반의 인증을 통해 Access Control
- 도메인별 선택적 Request 수집 옵션 제공
- Web Request UID 기능을 제공
  - 중복 수집을 방지

### 구동 방법

> 피들러 구동 전에 deus_rest Django 웹 서비스를 구동한다. (설치 방법을 참조)

윈도우 시작에서 Fiddler 를 실행, FiddleFiddle 확장 기능이 설치된 경우 로그온 화면이 출력

![Fiddler 실행](./img/windows_start.png)

#### 로그인 화면

로그인 화면이 아래와 같이 출력되며 인증을 해야, 웹 리퀘스트 수집이 가능

![피들피들 로그인](./img/fiddlefiddle_logon.png)

#### 관리 패널 (Control Panel)

피들피들은 수집을 위해 도메인 정보를 입력 받는다. 등록된 도메인에 대해서 수집을 하므로 이에 유의한다.

Inspector 와 같은 툴 리스트 중에서 FiddleFiddle 을 선택한다.

![피들피들 관리 패널](./img/fiddlefiddle_control_panel.png)

#### 호스트 이름을 등록 (Register your hostname)

`www.example.com` 을 등록 한 뒤에 수집 테스트를 한다.

![피들피들 호스트네임 등록](./img/fiddlefiddle_register_hostname.png)

```bash
curl http://www.example.com/where/am/i?param1=value1&param2=value2
```

#### JWT Auth Key

정상적으로 로그인한 경우 JWT 키가 발급된다. 해당 키는 일정 주기로 Refresh 되며 프로그램을 종료하면 키가 만료된다.

![JWT 인증키 값](./img/fiddlefiddle_jwt_auth.png)

## 효과

- 모의해킹/QA/테스트 데이터를 중복없이 보관
- 점검 기록을 재사용
- Static Resource 제한
- Net Uri 리스트업
- 전수 리스트 목록 도출
- Web Fuzzing 솔루션 구축의 기반 작업

## 설치

### Postgresql 구동

- test DB 생성
- postgres 확장 기능 (hstore, citext 설정)

### Django 구동

```bash
cd ./deus_rest # Rest 프로젝트 폴더로 이동
pip install -r ./requirements.txt # Dependency Module 설치
python manage.py migrate # DB 초기화
python manage.py createsuperuser # 관리자 계정 생성
python manage.py runserver 
```

Rest Open API 화면

![OpenAPI 화면](./img/deus_rest_web.png)

호스트 `wwww.example.com` 의 수집 내역 샘플

![Request Items](./img/deus_rest_request_items.png)

Json 뷰

#### FiddleFiddle 구동

- 요구사항
  - Visual Studio 2015 (Express) 이상 설치
  - Fiddler 설치

`FiddleFiddle.sln` 파일 실행 

`빌드 이벤트` 에서 아래의 구문을 `빌드 후 이벤트 명령줄`에 입력

```bash
copy "$(TargetDir)\*.dll" "C:\Users\cert\Documents\Fiddler2\Scripts"
```

![Build Event](./img/fiddlefiddle_visualstudio_build_event.png)

`Build` 버튼 클릭 후 빌드 성공 시 Fiddler 실행

## 핵심 포인트

### 중복 방지 기능

Uri, Parameter Name 을 키로 변환하여 보관하며 이를 통해 동일한 Parameter Name 리스트에 대해 동일한 Unique ID 를 부여합니다.

#### 케이스 1

파라미터 목록은 동일하나 파라미터 순서가 다른 경우, 이름 정렬을 통해 단일한 케이스로 식별

|No|param order| value order |
|:---:|:---:|:---:|
| case1 | param1, param2, param3 | value1, value2, value3 |
| case2 | param1, param3, param2 | value1, value3, value2 |

```bash
curl http://www.example.com/where/are/we?param1=value1&param2=value&param3=value3
```

```bash
curl http://www.example.com/where/are/we?param1=value1&param3=value3&param2=value
```


#### 케이스 2

파라미터 목록은 동일하나, 그 값이 다른 경우 동일한 케이스로 취급

|No|param order| value order |
|:---:|:---:|:---:|
| case1 | param1, param2, param3 | value1, value2, value3 |
| case2 | param1, param2, param3 | value1, value3, value2 |


## TO-DO

- 질의어를 이용한 검색 기능 강화
- Web Request Fuzzing 알고리즘 구현
- 분산-비동기 Task Queue 를 이용한 Web Fuzzer 아키텍처 
  - github.com/code-machina/xiired (쉬레드) 참고
- 자동화 리포트 데몬 추가
- 웹 관리 툴 구현 (MEVN)




