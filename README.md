# 프로젝트 정보  
- C# .Net Core를 사용한 비동기 소켓 서버
- C# .Net Framework를 사용한 Non-Blocking 소켓 클라이언트
- APM(비동기 프로그래밍 모델), EAP(이벤트 기반 비동기 패턴), TAP(작업 기반의 비동기 패턴)의 비동기 프로그래밍 패턴을 사용한 TCP 네트워크 통신 서버 구현
- Unity에서 사용가능하도록 클라이언트의 경우 .Net Framework 사용
  
# 서버
- 관련 프로젝트 : APMSocket, EAPSocket, TAPSocket, TestServer
- APM(비동기 프로그래밍 모델) : IAsyncResult 인터페이스를 사용한 모델로 Begin~, End~ 메소드를 사용하여 구현
- EAP(이벤트 기반 비동기 패턴) : 비동기 소켓작업을 처리하는 SocketAsyncEventArgs 객체를 이용한 모델로 SocketAsyncEventArgs를 매개변수로 사용하는 ~Async 메소드 사용하여 구현
- TAP(작업 기반의 비동기 패턴) : Task 형식을 기반으로 작성한 모델로 Task를 반환하는 ~Async 메서드, FromAsync 메서드와 await async 키워드를 사용하여 구현
- TestServer 프로젝트를 사용하여 각 모델 테스트 구동 가능
  
# 클라이언트
- 관련 프로젝트 : ClientSocket, TestClient
- Non-Blocking 소켓을 사용하여 단일스레드에서 구동이 가능하도록 구현
- NetworkService 클래스의 Service 메소드(하트비트 및 송수신 처리) 주기적으로 호출 필요
- TestClient 프로젝트를 사용하여 테스트 구동 가능

# 내부 구조
![슬라이드1](https://user-images.githubusercontent.com/100393621/204741173-5a2d8a58-5a8d-4855-beea-dc4fed277bfd.PNG)
![슬라이드2](https://user-images.githubusercontent.com/100393621/204741179-ea43ab16-0d5a-450b-9832-349d25f19d52.PNG)
![슬라이드3](https://user-images.githubusercontent.com/100393621/204741181-f9fe67f9-b212-418d-90d9-46a216520bb0.PNG)
![슬라이드4](https://user-images.githubusercontent.com/100393621/204741185-982540c6-1f72-4627-9ccd-7d0219d7a960.PNG)
![슬라이드5](https://user-images.githubusercontent.com/100393621/204741189-3a5e4b0d-aedd-481b-81ee-d19865de3ac5.PNG)
![슬라이드6](https://user-images.githubusercontent.com/100393621/204741191-9773ae81-bcc2-4d02-a9f5-0a3bd9fc5a08.PNG)
